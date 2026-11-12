/**
 * 10 我要抱抱 (Forum) 深度測試
 *
 * A. 畫面操作：發文、附圖 (選檔 / 貼上 / 拖曳)、預覽與移除、放大檢視、回文、匿名提醒
 * B. 權限：誰看得到刪除按鈕、誰能真的刪除 (直接呼叫 API)
 * C. 圖片處理：格式 / 張數 / 大小限制、EXIF 移除、轉正、縮圖、GIF 保留、刪除後圖片失效
 * D. 匿名保護：匿名貼文 / 回文不能從 API 回傳內容反查出作者
 * E. 健壯性：XSS、錯誤資料不應讓整個論壇壞掉
 */
import { test, expect, Page, APIRequestContext } from '@playwright/test';
import { requireRole, getRole } from './helpers/env';
import { loginPage, apiAs } from './helpers/auth';
import { post, postFormData, ApiResult, wasDenied, firstLines, expectOk } from './helpers/api';
import { fixture, oversizedPng, imageSize, hasExif, containsText, FilePayload } from './helpers/images';
import { PageMonitor, settle } from './helpers/monitor';

// ------------------------------------------------------------------ 共用

const stamp = () => new Date().toISOString().replace(/\D/g, '').slice(4, 14);

/** 與前端相同格式的發文時間 (M/D/YYYY, HH:mm:ss) */
function clientDate(): string {
  return new Date().toLocaleString('en-US', { timeZone: 'Asia/Taipei', hour12: false });
}

function toForm(prefix: 'post' | 'reply', fields: Record<string, any>, images: FilePayload[] = []): FormData {
  const fd = new FormData();
  for (const [k, v] of Object.entries(fields)) if (v !== undefined && v !== null) fd.append(`${prefix}.${k}`, String(v));
  for (const img of images) fd.append('images', new Blob([new Uint8Array(img.buffer)], { type: img.mimeType }), img.name);
  return fd;
}

async function apiCreatePost(api: APIRequestContext, opts: { title?: string; content?: string; anonymous?: boolean; images?: FilePayload[]; postDate?: string } = {}) {
  const r = await postFormData(
    api,
    'Forum/InsertPost',
    toForm('post', { title: opts.title ?? `API 測試 ${stamp()}`, content: opts.content ?? '自動化測試建立的貼文', postDate: opts.postDate ?? clientDate(), anonymous: !!opts.anonymous }, opts.images),
  );
  return r;
}

async function apiCreateReply(api: APIRequestContext, postId: number, opts: { content?: string; anonymous?: boolean; images?: FilePayload[] } = {}) {
  return postFormData(api, 'Forum/InsertReply', toForm('reply', { postId, replyContent: opts.content ?? `回覆 ${stamp()}`, replyDate: clientDate(), anonymous: !!opts.anonymous }, opts.images));
}

async function apiGetPost(api: APIRequestContext, id: number) {
  const r = await post(api, 'Forum/GetPost', { id });
  return { r, post: r.json?.Item1, replies: (r.json?.Item2 || []) as any[] };
}

async function adminDeletePost(id: number) {
  const admin = getRole('admin');
  if (!admin) return;
  const api = await apiAs(admin.empno);
  await post(api, 'Forum/DeletePost', { post: { id } });
  await api.dispose();
}

function expectCreated(r: ApiResult, label: string): number {
  expect(r.isServerErrorPage, `${label}：伺服器錯誤 ${firstLines(r.text)}`).toBe(false);
  expect(r.json?.ok, `${label}：${r.json?.message || r.text.slice(0, 200)}`).toBe(true);
  return r.json.id as number;
}

async function openNewPostModal(page: Page) {
  await page.getByRole('button', { name: /新增貼文/ }).click();
  await expect(page.locator('#exampleModal')).toBeVisible();
}

async function openReplyModal(page: Page) {
  await page.getByRole('button', { name: /回覆貼文/ }).click();
  await expect(page.locator('#exampleModal')).toBeVisible();
}

const modalSubmit = (page: Page) => page.locator('#exampleModal .modal-footer .btn-primary');
const previewItems = (page: Page) => page.locator('#exampleModal .forum-preview-item');
const pickerError = (page: Page) => page.locator('#exampleModal small.text-danger');

/** 按「發布」→ 等送出成功 → 等頁面重新整理後重新讀取資料 */
async function submitAndWaitReload(page: Page, reloadApi: string) {
  const insert = page.waitForResponse((r) => /Forum\/Insert(Post|Reply)/.test(r.url()), { timeout: 30_000 });
  // 成功時頁面立刻重新整理 (無法再讀取回應內容)，所以改等「重新讀取資料」或「錯誤訊息出現」
  const reloaded = page.waitForResponse((r) => r.url().includes(reloadApi), { timeout: 30_000 }).then(() => 'reloaded');
  const failed = page.locator('#exampleModal .alert-danger').waitFor({ state: 'visible', timeout: 30_000 }).then(() => 'failed');
  await modalSubmit(page).click();
  const res = await insert;
  expect(res.status(), '送出時伺服器錯誤').toBe(200);
  const outcome = await Promise.race([reloaded, failed]).catch(() => 'timeout');
  if (outcome !== 'reloaded') {
    const msg = await page.locator('#exampleModal .alert-danger').textContent().catch(() => '');
    throw new Error(`送出失敗：${msg || outcome}`);
  }
  await settle(page);
}

async function attach(page: Page, files: FilePayload[]) {
  await page.locator('#forumImageInput').setInputFiles(files.map((f) => ({ name: f.name, mimeType: f.mimeType, buffer: f.buffer })));
}

/** 在發文視窗觸發「貼上」事件 (模擬 Ctrl+V 截圖或從 Word 複製) */
async function pasteInto(page: Page, opts: { image?: FilePayload; text?: string }) {
  await page.evaluate(
    ({ b64, type, name, text }) => {
      const dt = new DataTransfer();
      if (text) dt.setData('text/plain', text);
      if (b64) {
        const bin = atob(b64);
        const arr = new Uint8Array(bin.length);
        for (let i = 0; i < bin.length; i++) arr[i] = bin.charCodeAt(i);
        dt.items.add(new File([arr], name as string, { type }));
      }
      const ev = new ClipboardEvent('paste', { clipboardData: dt, bubbles: true, cancelable: true });
      (document.querySelector('#exampleModal textarea') || document.getElementById('exampleModal'))!.dispatchEvent(ev);
    },
    { b64: opts.image?.buffer.toString('base64'), type: opts.image?.mimeType, name: opts.image?.name, text: opts.text },
  );
}

/** 拖曳檔案到發文視窗 */
async function dropInto(page: Page, file: FilePayload) {
  await page.evaluate(
    ({ b64, type, name }) => {
      const bin = atob(b64);
      const arr = new Uint8Array(bin.length);
      for (let i = 0; i < bin.length; i++) arr[i] = bin.charCodeAt(i);
      const dt = new DataTransfer();
      dt.items.add(new File([arr], name, { type }));
      const zone = document.getElementById('forumDropZone')!;
      for (const t of ['dragenter', 'dragover', 'drop']) zone.dispatchEvent(new DragEvent(t, { dataTransfer: dt, bubbles: true, cancelable: true }));
    },
    { b64: file.buffer.toString('base64'), type: file.mimeType, name: file.name },
  );
}

// ------------------------------------------------------------------ A. 畫面操作

test.describe('我要抱抱｜畫面操作', () => {
  test.describe.configure({ mode: 'serial' });

  let createdId = 0;
  const title = `UI 測試貼文 ${stamp()}`;

  test.afterAll(async () => {
    if (createdId) await adminDeletePost(createdId);
  });

  test('發文：附 2 張圖片 → 列表出現並顯示圖片圖示', async ({ page }) => {
    const emp = requireRole('employee');
    const monitor = new PageMonitor(page);
    await loginPage(page, emp.empno);
    await page.goto('/Forum');
    await settle(page);

    await openNewPostModal(page);
    await page.locator('#exampleInputTitle').fill(title);
    await page.locator('#exampleModal textarea').fill('這是自動化測試的貼文內容。\n第二行。');
    await attach(page, [fixture('small.png'), fixture('small.jpg')]);
    await expect(previewItems(page)).toHaveCount(2);

    // 送出成功後頁面會重新整理並重新讀取列表 (不等 load 事件：外部 CDN 圖片在內網可能載入很久)
    await submitAndWaitReload(page, 'Forum/GetAllPosts');

    const item = page.locator('li.post-stub', { hasText: title });
    await expect(item).toHaveCount(1);
    await expect(item.locator('.forum-has-image')).toBeVisible();

    const href = await item.locator('a').getAttribute('href');
    createdId = Number((href || '').split('/').pop());
    expect(createdId, '無法從連結取得貼文編號').toBeGreaterThan(0);
    expect(monitor.problems, monitor.problems.join('\n')).toEqual([]);
  });

  test('貼文頁：顯示縮圖、點擊放大、← → 切換、Esc 關閉', async ({ page }) => {
    test.skip(!createdId, '前一個測試沒有建立貼文');
    const emp = requireRole('employee');
    await loginPage(page, emp.empno);
    await page.goto(`/Forum/Post/${createdId}`);
    await settle(page);

    await expect(page.locator('h2', { hasText: title })).toBeVisible();
    const thumbs = page.locator('article').first().locator('.forum-thumbs img');
    await expect(thumbs).toHaveCount(2);

    // 縮圖都要真的載入成功
    for (let i = 0; i < 2; i++) {
      const ok = await thumbs.nth(i).evaluate((img: HTMLImageElement) => img.complete && img.naturalWidth > 0);
      expect(ok, `第 ${i + 1} 張縮圖沒有載入`).toBe(true);
    }

    await thumbs.first().click();
    const viewer = page.locator('.forum-viewer');
    await expect(viewer).toBeVisible();
    await expect(viewer.locator('.viewer-footer')).toContainText('1 / 2');
    await page.keyboard.press('ArrowRight');
    await expect(viewer.locator('.viewer-footer')).toContainText('2 / 2');
    await page.keyboard.press('ArrowRight');
    await expect(viewer.locator('.viewer-footer')).toContainText('1 / 2');
    await page.keyboard.press('ArrowLeft');
    await expect(viewer.locator('.viewer-footer')).toContainText('2 / 2');
    await page.keyboard.press('Escape');
    await expect(viewer).toBeHidden();
  });

  test('回文：附圖、匿名提醒，送出後顯示「匿名」與縮圖', async ({ page }) => {
    test.skip(!createdId, '前一個測試沒有建立貼文');
    const emp = requireRole('employee2');
    await loginPage(page, emp.empno);
    await page.goto(`/Forum/Post/${createdId}`);
    await settle(page);

    await openReplyModal(page);
    const text = `匿名回覆 ${stamp()}`;
    await page.locator('#exampleModal textarea').fill(text);
    await attach(page, [fixture('anim.gif')]);
    await page.locator('#exampleModal #defaultCheck1').check();
    await expect(page.locator('#exampleModal .alert-warning'), '勾選匿名且有附圖時應顯示提醒').toBeVisible();

    await submitAndWaitReload(page, 'Forum/GetPost');

    const reply = page.locator('article', { hasText: text });
    await expect(reply).toHaveCount(1);
    await expect(reply.locator('h4')).toHaveText('匿名');
    await expect(reply.locator('.forum-thumbs img')).toHaveCount(1);
  });

  test('一般員工看不到「刪除貼文」與回文刪除鈕', async ({ page }) => {
    test.skip(!createdId, '前一個測試沒有建立貼文');
    const emp = requireRole('employee');
    await loginPage(page, emp.empno);
    await page.goto(`/Forum/Post/${createdId}`);
    await settle(page);
    await expect(page.getByRole('button', { name: /刪除貼文/ })).toHaveCount(0);
    await expect(page.locator('.delete-btn')).toHaveCount(0);
  });

  test('管理員看得到刪除鈕，可刪除回文與貼文', async ({ page }) => {
    test.skip(!createdId, '前一個測試沒有建立貼文');
    const admin = requireRole('admin');
    await loginPage(page, admin.empno);
    await page.goto(`/Forum/Post/${createdId}`);
    await settle(page);

    await expect(page.getByRole('button', { name: /刪除貼文/ })).toBeVisible();
    await expect(page.locator('.delete-btn').first()).toBeVisible();

    // 刪除回文 (SweetAlert 確認)
    await page.locator('.delete-btn').first().click();
    await Promise.all([page.waitForURL(/\/Forum\/?$/i), page.locator('.swal2-confirm').click()]);

    // 刪除貼文 (瀏覽器 confirm)
    await page.goto(`/Forum/Post/${createdId}`);
    await settle(page);
    page.once('dialog', (d) => d.accept());
    await Promise.all([page.waitForURL(/\/Forum\/?$/i), page.getByRole('button', { name: /刪除貼文/ }).click()]);
    await settle(page);
    await expect(page.locator('li.post-stub', { hasText: title })).toHaveCount(0);
    createdId = 0;
  });
});

test.describe('我要抱抱｜附圖輸入方式與前端檢查', () => {
  test.beforeEach(async ({ page }) => {
    const emp = requireRole('employee');
    await loginPage(page, emp.empno);
    await page.goto('/Forum');
    await settle(page);
    await openNewPostModal(page);
  });

  test('預覽可以移除單張', async ({ page }) => {
    await attach(page, [fixture('small.png'), fixture('small.jpg'), fixture('anim.gif')]);
    await expect(previewItems(page)).toHaveCount(3);
    await previewItems(page).nth(1).locator('button').click();
    await expect(previewItems(page)).toHaveCount(2);
  });

  test('超過 5 張顯示錯誤，只保留 5 張', async ({ page }) => {
    const six = Array.from({ length: 6 }, (_, i) => ({ ...fixture('small.png'), name: `p${i}.png` }));
    await attach(page, six);
    await expect(previewItems(page)).toHaveCount(5);
    await expect(pickerError(page)).toContainText('最多');
  });

  test('非圖片格式 (SVG) 顯示錯誤', async ({ page }) => {
    await attach(page, [fixture('evil.svg')]);
    await expect(previewItems(page)).toHaveCount(0);
    await expect(pickerError(page)).toContainText('不是 JPG、PNG 或 GIF');
  });

  test('超過 8 MB 顯示錯誤', async ({ page }) => {
    await attach(page, [oversizedPng(8 * 1024 * 1024 + 10)]);
    await expect(previewItems(page)).toHaveCount(0);
    await expect(pickerError(page)).toContainText('8 MB');
  });

  test('Ctrl+V 貼上截圖會加入圖片', async ({ page }) => {
    await pasteInto(page, { image: fixture('small.png') });
    await expect(previewItems(page)).toHaveCount(1);
  });

  test('從 Word 複製 (文字 + 圖片) 貼上時，視為貼文字不加入圖片', async ({ page }) => {
    await pasteInto(page, { image: fixture('small.png'), text: '從 Word 複製的文字' });
    await expect(previewItems(page)).toHaveCount(0);
  });

  test('拖曳圖片到視窗會加入圖片', async ({ page }) => {
    await dropInto(page, fixture('small.jpg'));
    await expect(previewItems(page)).toHaveCount(1);
  });

  test('偽裝成 PNG 的文字檔：前端放行，但送出後伺服器拒絕並顯示訊息', async ({ page }) => {
    await page.locator('#exampleInputTitle').fill(`偽裝檔測試 ${stamp()}`);
    await page.locator('#exampleModal textarea').fill('不應該發文成功');
    await attach(page, [fixture('fake.png', 'image/png')]);
    await expect(previewItems(page)).toHaveCount(1);
    await modalSubmit(page).click();
    await expect(page.locator('#exampleModal .alert-danger')).toContainText(/損毀|格式/);
  });
});

// ------------------------------------------------------------------ B. 權限 (直接呼叫 API)

test.describe('我要抱抱｜刪除權限 (後端)', () => {
  test('一般員工呼叫刪除 API 會被拒絕，管理員可以刪除', async () => {
    const emp = requireRole('employee');
    const admin = requireRole('admin');
    const empApi = await apiAs(emp.empno);
    const adminApi = await apiAs(admin.empno);

    const id = expectCreated(await apiCreatePost(empApi, { images: [fixture('small.png')] }), '建立貼文');
    const replyId = expectCreated(await apiCreateReply(empApi, id, { images: [fixture('small.jpg')] }), '建立回文');

    // 一般員工：自己的貼文也不能刪 (目前規則：只有管理員 / 7291)
    expect(wasDenied(await post(empApi, 'Forum/DeleteReply', { postId: id, id: replyId })), '一般員工可以刪除回文').toBe(true);
    expect(wasDenied(await post(empApi, 'Forum/DeletePost', { post: { id } })), '一般員工可以刪除貼文').toBe(true);
    expect(wasDenied(await post(empApi, 'Forum/DeleteAll')), '一般員工可以清空論壇').toBe(true);
    expect((await apiGetPost(empApi, id)).post?.id, '貼文被一般員工刪掉了').toBe(id);

    // 管理員刪回文 → 回文圖片失效
    const before = await apiGetPost(adminApi, id);
    const replyImg = before.replies.find((x) => x.id === replyId)?.images?.[0];
    expect(replyImg, '回文應有一張圖').toBeTruthy();
    expect((await post(adminApi, 'Forum/DeleteReply', { postId: id, id: replyId })).json).toBe(true);
    expect((await adminApi.get(`Forum/Image/${replyImg.id}`)).status(), '刪除回文後圖片應無法讀取').toBe(404);

    // 管理員刪貼文 → 貼文圖片失效
    const postImg = before.post.images[0];
    expect((await post(adminApi, 'Forum/DeletePost', { post: { id } })).json).toBe(true);
    expect((await adminApi.get(`Forum/Image/${postImg.id}`)).status(), '刪除貼文後圖片應無法讀取').toBe(404);
    expect((await adminApi.get(`Forum/Image/${postImg.id}?thumb=true`)).status(), '刪除貼文後縮圖應無法讀取').toBe(404);

    await empApi.dispose();
    await adminApi.dispose();
  });

  test('「強制關閉管理員」的管理員不能刪除', async () => {
    const admin = requireRole('admin');
    test.skip(admin.empno === '7291', '7291 有獨立的刪除權限');
    const normal = await apiAs(admin.empno);
    const id = expectCreated(await apiCreatePost(normal), '建立貼文');
    const demoted = await apiAs(admin.empno, { admin: '0' });
    expect(wasDenied(await post(demoted, 'Forum/DeletePost', { post: { id } })), '已關閉管理員權限仍可刪除').toBe(true);
    await post(normal, 'Forum/DeletePost', { post: { id } });
    await normal.dispose();
    await demoted.dispose();
  });
});

// ------------------------------------------------------------------ C. 圖片處理 (後端)

test.describe('我要抱抱｜圖片處理 (後端)', () => {
  let api: APIRequestContext;
  const created: number[] = [];

  test.beforeAll(async () => {
    const emp = getRole('employee');
    if (emp) api = await apiAs(emp.empno);
  });
  test.afterAll(async () => {
    for (const id of created) await adminDeletePost(id);
    await api?.dispose();
  });
  test.beforeEach(() => {
    requireRole('employee');
  });

  async function countPosts() {
    const r = await post(api, 'Forum/GetAllPosts');
    return Array.isArray(r.json) ? r.json.length : -1;
  }

  const rejectCases: { name: string; images: () => FilePayload[]; expectMsg: RegExp }[] = [
    { name: '偽裝成 PNG 的文字檔', images: () => [fixture('fake.png', 'image/png')], expectMsg: /損毀|格式/ },
    { name: 'SVG (可能夾帶程式碼)', images: () => [fixture('evil.svg')], expectMsg: /JPG|PNG|GIF|格式/ },
    { name: '6 張圖片', images: () => Array.from({ length: 6 }, (_, i) => ({ ...fixture('small.png'), name: `p${i}.png` })), expectMsg: /最多/ },
    { name: '超過 8 MB', images: () => [oversizedPng(8 * 1024 * 1024 + 10)], expectMsg: /MB/ },
  ];

  for (const c of rejectCases) {
    test(`拒絕：${c.name}，且不會留下貼文`, async () => {
      const before = await countPosts();
      const r = await apiCreatePost(api, { title: `應被拒絕 ${c.name} ${stamp()}`, images: c.images() });
      expect(r.isServerErrorPage, `伺服器錯誤：${firstLines(r.text)}`).toBe(false);
      expect(r.json?.ok, `應拒絕但成功了：${r.text.slice(0, 200)}`).toBe(false);
      expect(String(r.json?.message || '')).toMatch(c.expectMsg);
      expect(await countPosts(), '被拒絕的貼文仍然被寫入資料庫').toBe(before);
    });
  }

  test('JPG：移除 EXIF (GPS / 手機型號)、依方向轉正、長邊縮到 1920、產生縮圖', async () => {
    const src = fixture('photo-exif.jpg');
    expect(hasExif(src.buffer), '測試檔本身應含 EXIF').toBe(true);

    const id = expectCreated(await apiCreatePost(api, { images: [src] }), '上傳 EXIF 照片');
    created.push(id);
    const { post: p } = await apiGetPost(api, id);
    const img = p.images[0];

    const full = await api.get(`Forum/Image/${img.id}`);
    expect(full.status()).toBe(200);
    expect(full.headers()['content-type']).toContain('image/jpeg');
    expect(full.headers()['x-content-type-options']).toBe('nosniff');
    const buf = await full.body();

    expect(hasExif(buf), '原圖仍含 EXIF').toBe(false);
    expect(containsText(buf, 'SecretPhone'), '原圖仍含手機型號').toBe(false);
    const size = imageSize(buf)!;
    // 原始 2400x1600 + Orientation=6 → 轉正為 1600x2400 → 長邊縮到 1920 → 1280x1920
    expect(size, '無法讀取圖片尺寸').toBeTruthy();
    expect(Math.max(size.width, size.height), '長邊應縮到 1920').toBe(1920);
    expect(size.height, `應依 EXIF 轉為直式，實際 ${size.width}x${size.height}`).toBeGreaterThan(size.width);

    const thumb = await api.get(`Forum/Image/${img.id}?thumb=true`);
    expect(thumb.status()).toBe(200);
    const tsize = imageSize(await thumb.body())!;
    expect(Math.max(tsize.width, tsize.height), '縮圖長邊應 ≤ 320').toBeLessThanOrEqual(320);
    expect(hasExif(await thumb.body()), '縮圖含 EXIF').toBe(false);
  });

  test('GIF：保留原檔 (動畫不被破壞)', async () => {
    const gif = fixture('anim.gif');
    const id = expectCreated(await apiCreatePost(api, { images: [gif] }), '上傳 GIF');
    created.push(id);
    const { post: p } = await apiGetPost(api, id);
    const body = await (await api.get(`Forum/Image/${p.images[0].id}`)).body();
    expect(Buffer.compare(body, gif.buffer), 'GIF 內容被改變').toBe(0);
  });

  test('前端取得的圖片資料不含實體檔名與上傳者', async () => {
    const id = expectCreated(await apiCreatePost(api, { images: [fixture('small.png')] }), '上傳');
    created.push(id);
    const { post: p } = await apiGetPost(api, id);
    const img = p.images[0];
    expect(img.fileName ?? null, '回傳了實體檔名 fileName').toBeNull();
    expect(img.thumbName ?? null, '回傳了縮圖檔名 thumbName').toBeNull();
    expect(img.empno ?? null, '回傳了上傳者員編').toBeNull();
  });

  test('讀取不存在的圖片回傳 404', async () => {
    expect((await api.get('Forum/Image/99999999')).status()).toBe(404);
  });
});

// ------------------------------------------------------------------ D. 匿名保護

test.describe('我要抱抱｜匿名保護', () => {
  test('匿名貼文與匿名回文：API 回傳內容不能看出作者', async () => {
    const author = requireRole('employee');
    const replier = requireRole('employee2');
    const authorApi = await apiAs(author.empno);
    const replierApi = await apiAs(replier.empno);
    const viewer = getRole('groupManager') || getRole('admin')!;
    const viewerApi = await apiAs(viewer.empno);

    const id = expectCreated(await apiCreatePost(authorApi, { anonymous: true, images: [fixture('small.png')], title: `匿名測試 ${stamp()}` }), '匿名發文');
    expectCreated(await apiCreateReply(replierApi, id, { anonymous: true, images: [fixture('small.jpg')] }), '匿名回文');

    try {
      const list = await post(viewerApi, 'Forum/GetAllPosts');
      const item = (list.json as any[]).find((x) => x.id === id);
      expect(item, '列表中找不到貼文').toBeTruthy();
      const listText = JSON.stringify(item);

      const detail = await post(viewerApi, 'Forum/GetPost', { id });
      const detailText = detail.text;

      for (const [who, u] of [['發文者', author], ['回文者', replier]] as const) {
        expect.soft(listText.includes(`"${u.empno}"`), `GetAllPosts 回傳內容含匿名${who}的員編 ${u.empno}：${listText.slice(0, 300)}`).toBe(false);
        expect.soft(listText.includes(u.name), `GetAllPosts 回傳內容含匿名${who}的姓名`).toBe(false);
        expect.soft(detailText.includes(`"${u.empno}"`), `GetPost 回傳內容含匿名${who}的員編 ${u.empno}`).toBe(false);
        expect.soft(detailText.includes(u.name), `GetPost 回傳內容含匿名${who}的姓名`).toBe(false);
      }
    } finally {
      await adminDeletePost(id);
      await authorApi.dispose();
      await replierApi.dispose();
      await viewerApi.dispose();
    }
  });
});

// ------------------------------------------------------------------ E. 健壯性

test.describe('我要抱抱｜健壯性', () => {
  test('標題與內容中的 HTML / Script 不會被執行 (XSS)', async ({ page }) => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const payload = `<img src=x onerror="window.__xss=1">XSS ${stamp()}`;
    const id = expectCreated(await apiCreatePost(api, { title: payload, content: payload }), '建立 XSS 貼文');
    expectCreated(await apiCreateReply(api, id, { content: payload }), '建立 XSS 回文');

    try {
      await loginPage(page, emp.empno);
      await page.goto('/Forum');
      await settle(page);
      expect(await page.evaluate(() => (window as any).__xss), '列表頁執行了貼文中的程式碼').toBeUndefined();
      await page.goto(`/Forum/Post/${id}`);
      await settle(page);
      expect(await page.evaluate(() => (window as any).__xss), '貼文頁執行了貼文中的程式碼').toBeUndefined();
    } finally {
      await adminDeletePost(id);
      await api.dispose();
    }
  });

  test('回覆不存在的貼文會被拒絕', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const r = await apiCreateReply(api, 99999999);
    expect(r.isServerErrorPage, firstLines(r.text)).toBe(false);
    expect(r.json?.ok, '可以回覆不存在的貼文 (會產生孤兒資料)').toBe(false);
    await api.dispose();
  });

  test('一篇日期格式異常的貼文，不應讓整個論壇列表壞掉', async () => {
    // 發文時間由前端傳入，伺服器以 ParseExact("M/d/yyyy, HH:mm:ss") 解析；
    // 只要有一筆格式不同 (例如瀏覽器語系或舊版程式)，GetAllPosts 就會整個失敗。
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const r = await apiCreatePost(api, { title: `日期異常 ${stamp()}`, postDate: '2026-11-12 10:00' });
    const id = r.json?.ok ? (r.json.id as number) : 0;
    try {
      if (!id) {
        test.info().annotations.push({ type: '結果', description: '伺服器拒絕了異常日期 (良好)' });
        return;
      }
      const list = await post(api, 'Forum/GetAllPosts');
      expect(list.isServerErrorPage, `一篇日期異常的貼文讓 GetAllPosts 整個失敗：${firstLines(list.text)}`).toBe(false);
    } finally {
      if (id) await adminDeletePost(id);
      await api.dispose();
    }
  });

  test('列表與貼文 API 正常', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    expectOk(await post(api, 'Forum/GetAllPosts'), 'Forum/GetAllPosts');
    await api.dispose();
  });
});
