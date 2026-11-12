/**
 * 01 冒煙測試：每個角色逐頁開啟全站頁面
 *
 * 每一頁檢查：
 *   - 頁面 HTTP 狀態 < 400、沒有 ASP.NET 錯誤頁
 *   - 沒有被導到未授權頁
 *   - 沒有 JavaScript 例外 / console.error
 *   - 頁面載入時呼叫的本站 API 沒有 4xx / 5xx
 */
import { test, expect, BrowserContext, Page } from '@playwright/test';
import { PAGES, NO_AUTHORIZE_PAGES } from './catalog/pages';
import { getRole, ROLE_LABELS, RoleKey, describeUser } from './helpers/env';
import { loginContext } from './helpers/auth';
import { PageMonitor, openAndCheck } from './helpers/monitor';

/** 要跑冒煙測試的角色 (可用 TE_SMOKE_ROLES=admin,employee 縮小範圍) */
const SMOKE_ROLES: RoleKey[] = (process.env.TE_SMOKE_ROLES || 'admin,deptManager,groupManager,subManager,pm,employee')
  .split(',')
  .map((s) => s.trim() as RoleKey)
  .filter(Boolean);

for (const roleKey of SMOKE_ROLES) {
  test.describe(`冒煙測試｜${ROLE_LABELS[roleKey]}`, () => {
    let context: BrowserContext;
    let page: Page;
    let monitor: PageMonitor;

    test.beforeAll(async ({ browser }) => {
      const user = getRole(roleKey);
      if (!user) return;
      context = await browser.newContext();
      await loginContext(context, user.empno);
      page = await context.newPage();
      monitor = new PageMonitor(page);
    });

    test.afterAll(async () => {
      await context?.close();
    });

    for (const p of PAGES.filter((x) => !x.legacy)) {
      test(`${p.module}｜${p.name}`, async () => {
        const user = getRole(roleKey);
        test.skip(!user, `User.db 中找不到「${ROLE_LABELS[roleKey]}」`);
        test.info().annotations.push({ type: '身分', description: describeUser(user) }, { type: '網址', description: p.path });
        await openAndCheck(page, monitor, p.path, `[${ROLE_LABELS[roleKey]}] ${p.module}｜${p.name}`);
      });
    }
  });
}

test.describe('冒煙測試｜舊版或不在選單的頁面 (一般員工)', () => {
  let context: BrowserContext;
  let page: Page;
  let monitor: PageMonitor;

  test.beforeAll(async ({ browser }) => {
    const user = getRole('employee');
    if (!user) return;
    context = await browser.newContext();
    await loginContext(context, user.empno);
    page = await context.newPage();
    monitor = new PageMonitor(page);
  });
  test.afterAll(async () => context?.close());

  for (const p of PAGES.filter((x) => x.legacy)) {
    test(`${p.name}`, async () => {
      test.skip(!getRole('employee'), '找不到一般員工');
      test.info().annotations.push({ type: '說明', description: '舊版頁面 (保留給其他開發人員)：只列出問題，不計為失敗' });
      await openAndCheck(page, monitor, p.path, `[舊版] ${p.name}`, { warnOnly: true });
    });
  }
});

test.describe('冒煙測試｜直接從書籤 / 通知連結進入 (Session 尚未載入)', () => {
  // 模擬：使用者 Session 逾時 (40 分鐘) 或直接開啟書籤，第一個請求就進到沒有 [MyAuthorize] 的頁面
  for (const p of NO_AUTHORIZE_PAGES) {
    test(`${p.module}｜${p.name}`, async ({ browser }) => {
      const user = getRole('employee');
      test.skip(!user, '找不到一般員工');
      test.info().annotations.push({
        type: '說明',
        description: '此控制器沒有加 [MyAuthorize]，若直接進入時 Session 是空的，Session["empno"].ToString() 會出錯',
      });
      const context = await browser.newContext();
      await loginContext(context, user!.empno, { prime: false }); // 不先開首頁
      const page = await context.newPage();
      const monitor = new PageMonitor(page);
      await openAndCheck(page, monitor, p.path, `[直接進入] ${p.module}｜${p.name}`);
      await context.close();
    });
  }
});

test.describe('冒煙測試｜身分切換', () => {
  test('DevTools 可切換身分，Session 內容正確', async ({ page }) => {
    const emp = getRole('employee');
    const admin = getRole('admin');
    test.skip(!emp || !admin, '需要一般員工與管理員');

    await page.goto(`/DevTools/Login?empno=${emp!.empno}&returnUrl=/`);
    let who = await (await page.request.get('/DevTools/WhoAmI')).json();
    expect(who.empno).toBe(emp!.empno);
    expect(who.Admin).toBeFalsy();

    await page.goto(`/DevTools/Login?empno=${admin!.empno}&returnUrl=/`);
    who = await (await page.request.get('/DevTools/WhoAmI')).json();
    expect(who.empno).toBe(admin!.empno);
    expect(who.Admin).toBeTruthy();

    // 強制關閉管理員
    await page.goto(`/DevTools/Login?empno=${admin!.empno}&admin=0&returnUrl=/`);
    who = await (await page.request.get('/DevTools/WhoAmI')).json();
    expect(who.Admin).toBeFalsy();

    // 強制開啟管理員
    await page.goto(`/DevTools/Login?empno=${emp!.empno}&admin=1&returnUrl=/`);
    who = await (await page.request.get('/DevTools/WhoAmI')).json();
    expect(who.empno).toBe(emp!.empno);
    expect(who.Admin).toBeTruthy();
  });

  test('不存在的員編會被導向未授權頁', async ({ page }) => {
    await page.goto('/DevTools/Login?empno=0001&returnUrl=/');
    await expect(page).toHaveURL(/Error\/Unauthorized/i);
  });
});
