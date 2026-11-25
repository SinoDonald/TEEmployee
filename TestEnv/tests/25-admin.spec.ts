/**
 * 25 管理頁 Admin（2026-11 新版 UI）
 *   - 資料檢視（開發者）：UserDB / ProfessionDB 改成表格，限 Admin 與 DbViewers 名單
 *   - 下載 UserDB / ProfessionDB 已移除
 *
 * 只跑這支：.\scripts\Run-Tests.ps1 -Only 25-admin -Restart
 *
 * 安全：本檔「不會」真的重置任何資料庫。長按確認的測試會攔截所有 Delete / Reset 類 API，
 *       回傳假的成功結果，請求不會送到伺服器。
 */
import { test, expect, Page, BrowserContext } from '@playwright/test';
import { getRole, requireRole, loadRoles, TestUser, describeUser } from './helpers/env';
import { loginContext, apiAs, AdminOverride } from './helpers/auth';
import { PageMonitor, openAndCheck, settle } from './helpers/monitor';

/** 開啟管理頁 (以指定身分) */
async function openAdmin(context: BrowserContext, user: TestUser, admin: AdminOverride = '') {
  await loginContext(context, user.empno, { admin });
  const page = await context.newPage();
  const monitor = new PageMonitor(page);
  return { page, monitor };
}

/** DbViewers (開發者)：優先挑「不是 Admin」的人；開發機的 7291、7596 同時是 Admin，就強制關閉管理員身分 (te_test_admin=0) 來模擬 */
function pickDbViewer(): { user: TestUser; admin: AdminOverride } | null {
  const pure = getRole('dbViewer');
  if (pure) return { user: pure, admin: '' };
  const u = loadRoles().users.find((x) => !!x.isDbViewer);
  return u ? { user: u, admin: '0' } : null;
}

/** 可進管理頁、但不能檢視資料的人 (在 AdminPageUsers、不在 DbViewers、不是 Admin) */
function pickAdminPageOnly(): TestUser | null {
  return getRole('adminPageUser') || loadRoles().users.find((x) => !!x.isAdminPageUser && !x.isAdmin && !x.isDbViewer) || null;
}

/** 開啟「資料檢視」表格，回傳表格 locator */
async function openViewer(page: Page, which: 'UserDB' | 'ProfessionDB') {
  const [res] = await Promise.all([
    page.waitForResponse((r) => new RegExp(`Home/Review${which}`, 'i').test(r.url())),
    page.locator('.adm-tile', { hasText: `檢視 ${which}` }).click(),
  ]);
  expect(res.status(), `Home/Review${which} 回應錯誤`).toBe(200);
  const table = page.locator('.adm-dt');
  await expect(table).toBeVisible();
  return table;
}

// =============================================================================
test.describe('管理頁 Admin｜系統管理員', () => {
  let context: BrowserContext;
  let page: Page;
  let monitor: PageMonitor;
  let admin: TestUser;

  test.beforeAll(async ({ browser }) => {
    const a = getRole('admin');
    if (!a) return;
    admin = a;
    context = await browser.newContext();
    ({ page, monitor } = await openAdmin(context, admin));
  });
  test.afterAll(async () => context?.close());

  test('頁面可正常開啟：無錯誤、無亂碼、時鐘有在跑', async () => {
    requireRole('admin');
    test.info().annotations.push({ type: '身分', description: describeUser(admin) });
    const { html } = await openAndCheck(page, monitor, '/Home/Admin', '[管理員] 管理頁');

    // 中文正常 (Admin.cshtml 若沒有 BOM，會變成亂碼或「剖析器錯誤」)
    expect(html, '頁面中文出現亂碼 (.cshtml 可能沒存成 UTF-8 with BOM)').not.toMatch(/\uFFFD|剖析器錯誤|Parser Error|Unterminated string/);
    await expect(page.locator('.adm-hero h2')).toContainText(admin.name);
    for (const t of ['帳號管理', '自評表與考核', '年度行事曆目標', '維護工具', '資料檢視', '資料庫重置', '本頁操作紀錄']) {
      // 只找管理頁內容區 (上方選單也有「自評表與考核」等同名項目，但收合時是隱藏的)
      await expect(page.locator('.adm').getByText(t, { exact: false }).first(), `找不到「${t}」區塊`).toBeVisible();
    }
    // 頁面上的 JavaScript 有執行
    await expect(page.locator('#admClock')).toHaveText(/^\d{2}:\d{2}:\d{2}$/);
    await expect(page.locator('#admDate')).toHaveText(/^\d{4}\/\d{2}\/\d{2}（.）$/);
    await expect(page.locator('#admGreeting')).not.toHaveText('你好');
  });

  test('下載 UserDB / ProfessionDB 按鈕已移除', async () => {
    requireRole('admin');
    await page.goto('/Home/Admin');
    await settle(page);
    await expect(page.locator('a[href*="DownloadUserDB"], a[href*="DownloadProfessionDB"]')).toHaveCount(0);
    await expect(page.getByText('下載 UserDB')).toHaveCount(0);
  });

  test('搜尋與分類篩選', async () => {
    requireRole('admin');
    await page.goto('/Home/Admin');
    await settle(page);
    const visible = () => page.locator('#admSections > section:not(.hidden-by-filter)');
    const search = page.locator('#admSearch');

    // 按 / 會聚焦搜尋框
    await page.locator('.adm-hero').click();
    await page.keyboard.press('/');
    await expect(search).toBeFocused();

    await search.fill('userdb');
    await expect(visible()).toHaveCount(1);
    await expect(visible().first()).toContainText('資料檢視');

    await search.fill('這個關鍵字不存在zzz');
    await expect(visible()).toHaveCount(0);
    await expect(page.locator('#admNoResult')).toBeVisible();

    await search.press('Escape');
    await expect(search).toHaveValue('');
    await expect(page.locator('#admNoResult')).toBeHidden();

    await page.locator('.adm-pill[data-cat="danger"]').click();
    await expect(visible()).toHaveCount(1);
    await expect(visible().first()).toHaveAttribute('data-cat', 'danger');

    // 「資料工具」= 維護工具 + 資料檢視
    await page.locator('.adm-pill[data-cat="data"]').click();
    await expect(visible()).toHaveCount(2);

    await page.locator('.adm-pill[data-cat="all"]').click();
    await expect(visible()).toHaveCount(await page.locator('#admSections > section').count());
  });

  test('KPI 年份「今年 / 明年」快速填入', async () => {
    requireRole('admin');
    await page.goto('/Home/Admin');
    await settle(page);
    const y = new Date().getFullYear();
    await page.locator('.adm-mini[data-year="0"]').click();
    await expect(page.locator('#kpiyear')).toHaveValue(String(y));
    await page.locator('.adm-mini[data-year="1"]').click();
    await expect(page.locator('#kpiyear')).toHaveValue(String(y + 1));
  });

  test('資料檢視 UserDB：表格、搜尋、排序、關閉', async () => {
    requireRole('admin');
    await page.goto('/Home/Admin');
    await settle(page);

    const table = await openViewer(page, 'UserDB');
    await expect(table.locator('.adm-dt-title')).toContainText('user.db');
    const rows = table.locator('tr.dt-row');
    await expect(rows.first(), '管理員檢視 UserDB 應顯示資料').toBeVisible();
    const total = await rows.count();
    expect(total).toBeGreaterThan(1);
    await expect(page.locator('#admCount')).toHaveText('1');
    await expect(page.locator('#admFeed li').first()).toContainText('檢視 UserDB');

    // 搜尋自己的員編 → 筆數變少、包含自己
    const input = table.locator('.adm-dt-search input');
    await input.fill(admin.empno);
    await expect.poll(() => rows.count()).toBeLessThan(total);
    await expect(rows.first()).toContainText(admin.empno);
    await expect(table.locator('.adm-dt-count')).toContainText(`/ ${total}`);

    // 找不到
    await input.fill('不存在的資料zzz');
    await expect(table.locator('.adm-dt-empty')).toContainText('找不到符合');
    await input.fill('');
    await expect.poll(() => rows.count()).toBe(Math.min(total, 500));

    // 點欄位標題排序：出現 ▲，再點變 ▼
    const th = table.locator('thead th').first();
    await th.click();
    await expect(th).toContainText('▲');
    await th.click();
    await expect(th).toContainText('▼');

    // 關閉
    await table.locator('.adm-dt-close').click();
    await expect(page.locator('.adm-dt')).toHaveCount(0);
  });

  test('資料檢視 ProfessionDB：表格有資料、巢狀清單可展開', async () => {
    requireRole('admin');
    await page.goto('/Home/Admin');
    await settle(page);

    const table = await openViewer(page, 'ProfessionDB');
    await expect(table.locator('.adm-dt-title')).toContainText('profession.db');
    const rows = table.locator('tr.dt-row');
    if ((await rows.count()) === 0) {
      test.info().annotations.push({ type: '提醒', description: '測試資料的 ProfessionDB 沒有資料，只檢查表格能開啟' });
      return;
    }
    const nest = table.locator('.nest-btn').first();
    if (await nest.count()) {
      await nest.click();
      await expect(table.locator('tr.nest-row').first()).toBeVisible();
      await nest.click();
      await expect(table.locator('tr.nest-row')).toHaveCount(0);
    }
  });

  test('資料庫重置：短按不會執行，長按 1.5 秒才送出 (已攔截，不會真的刪除)', async () => {
    requireRole('admin');
    const sent: string[] = [];
    // 攔截所有刪除 / 重置類請求，回傳假的成功，絕不送到伺服器
    await page.route(/\/(DeleteAll|DeleteGroupPlan|DeletePersonalPlan|DeleteTalent|DeleteKpiModels|DeleteSolitaryKpiModels|Reset)/i, async (route) => {
      sent.push(new URL(route.request().url()).pathname);
      await route.fulfill({ status: 200, contentType: 'application/json', body: 'true' });
    });

    await page.goto('/Home/Admin');
    await settle(page);
    await page.locator('.adm-db', { hasText: '我要抱抱' }).click();
    const modal = page.locator('#exampleModal');
    await expect(modal).toBeVisible();
    await expect(modal.locator('h3')).toContainText('我要抱抱');

    // 等彈出視窗的滑入動畫結束，再取按鈕位置 (動畫中取位置會按到按鈕外面)
    await expect(modal).toHaveClass(/show/);
    await page.waitForTimeout(600);
    const hold = modal.locator('.adm-hold');
    const label = hold.locator('.label');
    const pressAt = async () => {
      await hold.hover();
      const box = (await hold.boundingBox())!;
      await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
    };

    // 短按 0.4 秒就放開 → 有開始倒數，但放開後不應送出
    await pressAt();
    await page.mouse.down();
    await expect(label, '按下按鈕後沒有出現倒數 (長按功能沒有反應)').toContainText('繼續按住', { timeout: 1000 });
    await page.waitForTimeout(200);
    await page.mouse.up();
    await page.waitForTimeout(1600);
    expect(sent, '短按就送出了重置請求').toEqual([]);
    await expect(modal, '短按後視窗不應關閉').toBeVisible();
    await expect(label, '放開後按鈕文字應恢復').toContainText('長按');

    // 長按 2 秒 → 送出、視窗關閉、紀錄出現
    await pressAt();
    await page.mouse.down();
    await page.waitForTimeout(2000);
    await page.mouse.up();
    await expect.poll(() => sent.length, { message: '長按 1.5 秒後沒有送出重置請求' }).toBeGreaterThan(0);
    expect(sent.join(','), '「我要抱抱」應呼叫 Forum 的刪除').toMatch(/Forum/i);
    await expect(modal).toBeHidden();
    await expect(page.locator('#admFeed li').first()).toContainText('我要抱抱');

    // 取消按鈕可關閉視窗、不送出
    sent.length = 0;
    await page.locator('.adm-db', { hasText: 'KPI' }).click();
    await expect(modal).toBeVisible();
    await modal.locator('.adm-cancel').click();
    await expect(modal).toBeHidden();
    expect(sent, '按取消卻送出了重置請求').toEqual([]);

    await page.unrouteAll({ behavior: 'ignoreErrors' });
  });
});

// =============================================================================
test.describe('管理頁 Admin｜DbViewers (開發者，不是 Admin)', () => {
  test('看得到「資料檢視」並可檢視 UserDB，但沒有管理員身分', async ({ browser }) => {
    const who = pickDbViewer();
    test.skip(!who, '找不到 DbViewers 名單上的人');
    test.info().annotations.push({ type: '身分', description: `${describeUser(who!.user)}${who!.admin === '0' ? '（強制關閉管理員身分）' : ''}` });

    const context = await browser.newContext();
    const { page, monitor } = await openAdmin(context, who!.user, who!.admin);
    await openAndCheck(page, monitor, '/Home/Admin', '[DbViewers] 管理頁');
    expect(page.url(), '被導離管理頁').toMatch(/\/Home\/Admin/i);

    const table = await openViewer(page, 'UserDB');
    await expect(table.locator('tr.dt-row').first(), 'DbViewers 檢視 UserDB 應有資料').toBeVisible();

    const who2 = await (await context.request.get('/DevTools/WhoAmI')).json();
    expect(who2.Admin, 'DbViewers 不應因此取得管理員身分').toBeFalsy();
    await context.close();
  });
});

test.describe('管理頁 Admin｜只在 AdminPageUsers (不是 Admin、不是 DbViewers)', () => {
  test('可進管理頁，但看不到「資料檢視」，API 也拿不到資料', async ({ browser }) => {
    const u = pickAdminPageOnly();
    test.skip(!u, '目前沒有「只在 AdminPageUsers」的人 (開發機 7291、7596 同時也是 DbViewers)，略過');

    const context = await browser.newContext();
    const { page, monitor } = await openAdmin(context, u!);
    await openAndCheck(page, monitor, '/Home/Admin', '[AdminPageUsers] 管理頁');
    await expect(page.locator('.adm-tile', { hasText: '檢視 UserDB' })).toHaveCount(0);
    await context.close();

    const api = await apiAs(u!.empno);
    const r = await api.post('Home/ReviewUserDB', { data: {} });
    const j = await r.json().catch(() => null);
    expect(Array.isArray(j) ? j.length : 0, 'AdminPageUsers 不應拿到 UserDB 資料').toBe(0);
    await api.dispose();
  });
});

test.describe('管理頁 Admin｜一般員工', () => {
  test('直接開網址進不了管理頁；也拿不到 UserDB / ProfessionDB', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);

    const r = await api.get('Home/Admin');
    const text = await r.text();
    expect(r.status() === 200 && /AdminCtrl/.test(text), '一般員工可以開啟管理頁').toBe(false);

    for (const url of ['Home/ReviewUserDB', 'Home/ReviewProfessionDB']) {
      const res = await api.post(url, { data: {} });
      const j = await res.json().catch(() => null);
      expect(Array.isArray(j) ? j.length : 0, `一般員工可以從 ${url} 拿到資料`).toBe(0);
    }
    await api.dispose();
  });
});
