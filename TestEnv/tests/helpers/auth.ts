import { APIRequestContext, BrowserContext, Page, request as pwRequest } from '@playwright/test';
import { BASE_URL } from './env';

export type AdminOverride = '' | '1' | '0';

function testCookies(empno: string, admin: AdminOverride) {
  const host = new URL(BASE_URL).hostname;
  return [
    { name: 'te_test_empno', value: empno, domain: host, path: '/', httpOnly: false, secure: false, sameSite: 'Lax' as const, expires: -1 },
    { name: 'te_test_admin', value: admin, domain: host, path: '/', httpOnly: false, secure: false, sameSite: 'Lax' as const, expires: -1 },
  ];
}

/**
 * 讓瀏覽器以指定員工身分登入測試站台。
 * prime = true 時先開首頁 (和真實使用者一樣)，讓 Session 載入員工資料。
 */
export async function loginContext(context: BrowserContext, empno: string, opts: { admin?: AdminOverride; prime?: boolean } = {}) {
  await context.clearCookies();
  await context.addCookies(testCookies(empno, opts.admin ?? ''));
  if (opts.prime !== false) {
    const page = await context.newPage();
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.close();
  }
}

/** 已登入的 page (在同一個 context 中切換身分) */
export async function loginPage(page: Page, empno: string, opts: { admin?: AdminOverride; prime?: boolean } = {}) {
  await page.context().clearCookies();
  await page.context().addCookies(testCookies(empno, opts.admin ?? ''));
  if (opts.prime !== false) await page.goto('/', { waitUntil: 'domcontentloaded' });
}

/**
 * 建立以指定員工登入的 API 呼叫器 (不開瀏覽器，直接呼叫後端)。
 * 預設先 GET 首頁讓 Session 載入員工資料。
 */
export async function apiAs(empno: string | null, opts: { admin?: AdminOverride; prime?: boolean } = {}): Promise<APIRequestContext> {
  const ctx = await pwRequest.newContext({
    baseURL: BASE_URL,
    storageState: { cookies: empno ? testCookies(empno, opts.admin ?? '') : [], origins: [] },
    extraHTTPHeaders: { 'X-Requested-With': 'XMLHttpRequest' },
    maxRedirects: 0,
  });
  if (empno && opts.prime !== false) {
    const r = await ctx.get('/', { maxRedirects: 5 });
    if (r.status() >= 400) throw new Error(`以 ${empno} 登入失敗：首頁回應 ${r.status()}`);
  }
  return ctx;
}
