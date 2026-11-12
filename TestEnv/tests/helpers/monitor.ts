import { Page, Response, expect, test } from '@playwright/test';
import { BASE_URL } from './env';
import { looksLikeServerError } from './api';

/**
 * 監看頁面上的錯誤：
 *  - 未攔截的 JavaScript 例外 (pageerror)
 *  - console.error (只算本站的；外部 CDN、Google 字型等連線失敗不算)
 *  - 本站 API / 資源回應 4xx、5xx
 *  - 本站請求失敗 (連線中斷等)
 */
export class PageMonitor {
  problems: string[] = [];
  /** 不算失敗、只在報告中提醒的項目 (例如員工照片尚未上傳) */
  warnings: string[] = [];
  private host = new URL(BASE_URL).host;

  constructor(private page: Page) {
    page.on('pageerror', (err) => {
      this.problems.push(`JS 例外：${err.message}`);
    });

    page.on('console', (msg) => {
      if (msg.type() !== 'error') return;
      const loc = msg.location();
      const url = loc && loc.url ? loc.url : '';
      const text = msg.text();
      // 錯誤來源不是本站 (例如公司強制安裝的瀏覽器擴充套件、外部 CDN)，不算網站的問題
      if (url && !this.isSameOrigin(url)) return;
      // 外部資源載入失敗不算 (內網可能連不到 CDN / 外部頭像)
      if (/Failed to load resource/i.test(text) && url && !this.isSameOrigin(url)) return;
      if (/Failed to load resource/i.test(text)) return; // 本站的會由 response 監聽記錄 (含網址)
      if (/favicon/i.test(text)) return;
      this.problems.push(`console.error：${text.slice(0, 500)}${url ? `  (${url})` : ''}`);
    });

    page.on('response', (res: Response) => {
      const url = res.url();
      if (!this.isSameOrigin(url)) return;
      if (/favicon\.ico/i.test(url)) return;
      const status = res.status();
      if (status >= 400) {
        const type = res.request().resourceType();
        const line = `HTTP ${status}：${res.request().method()} ${this.shortUrl(url)} (${type})`;
        // 員工照片 (Content/CV/員編.jpg) 未上傳：頁面會改顯示預設圖，只列為提醒
        if (status === 404 && type === 'image' && /\/Content\/CV\//i.test(url)) this.warnings.push(line);
        else this.problems.push(line);
      }
    });

    page.on('requestfailed', (req) => {
      const url = req.url();
      if (!this.isSameOrigin(url)) return;
      const failure = req.failure()?.errorText || '';
      if (/ERR_ABORTED|NS_BINDING_ABORTED/i.test(failure)) return; // 換頁時中斷屬正常
      this.problems.push(`請求失敗：${req.method()} ${this.shortUrl(url)} ${failure}`);
    });
  }

  reset() {
    this.problems = [];
    this.warnings = [];
  }

  private isSameOrigin(url: string) {
    try {
      return new URL(url).host === this.host;
    } catch {
      return false;
    }
  }

  private shortUrl(url: string) {
    try {
      const u = new URL(url);
      return u.pathname + u.search;
    } catch {
      return url;
    }
  }
}

/** 等頁面與 AngularJS 的 API 呼叫都完成 */
export async function settle(page: Page, ms = 10_000) {
  await page.waitForLoadState('load', { timeout: ms }).catch(() => {});
  await page.waitForLoadState('networkidle', { timeout: ms }).catch(() => {});
  // AngularJS：等 $http 沒有待處理的請求
  await page
    .waitForFunction(
      () => {
        const ng = (window as any).angular;
        if (!ng) return true;
        try {
          const el = document.querySelector('[ng-app]') || document.body;
          const inj = ng.element(el).injector();
          if (!inj) return true;
          return inj.get('$http').pendingRequests.length === 0;
        } catch {
          return true;
        }
      },
      null,
      { timeout: ms },
    )
    .catch(() => {});
}

/**
 * 開啟頁面並檢查：HTTP 狀態、錯誤頁、未授權導向、JS 錯誤、API 錯誤
 * 錯誤以 soft assertion 記錄，一次列出該頁所有問題。
 */
export async function openAndCheck(page: Page, monitor: PageMonitor, path: string, label = path, opts: { warnOnly?: boolean } = {}) {
  monitor.reset();
  let mainStatus = 0;
  let navError = '';
  try {
    const resp = await page.goto(path, { waitUntil: 'domcontentloaded' });
    mainStatus = resp ? resp.status() : 0;
  } catch (e: any) {
    navError = e.message;
  }
  await settle(page);

  const url = page.url();
  const html = await page.content().catch(() => '');
  const title = await page.title().catch(() => '');

  if (monitor.warnings.length) {
    test.info().annotations.push({ type: '提醒', description: `${label}：\n${monitor.warnings.join('\n')}` });
  }

  // 舊版頁面：只在報告中列出問題，不算失敗
  if (opts.warnOnly) {
    const issues: string[] = [];
    if (navError) issues.push(`無法開啟頁面：${navError}`);
    if (mainStatus >= 400) issues.push(`頁面回應 HTTP ${mainStatus}`);
    if (looksLikeServerError(html)) issues.push(`伺服器錯誤頁「${title}」`);
    issues.push(...monitor.problems);
    if (issues.length) test.info().annotations.push({ type: '舊版頁面問題 (不計失敗)', description: issues.join('\n') });
    return { status: mainStatus, url, title, html };
  }

  expect.soft(navError, `${label}：無法開啟頁面`).toBe('');
  expect.soft(mainStatus, `${label}：頁面回應 HTTP ${mainStatus}`).toBeLessThan(400);
  expect.soft(looksLikeServerError(html), `${label}：出現伺服器錯誤頁「${title}」`).toBe(false);
  expect.soft(/\/Error\/Unauthorized/i.test(url), `${label}：被導向未授權頁 ${url}`).toBe(false);
  expect.soft(/\/DevTools/i.test(url), `${label}：測試身分失效，被導回 DevTools`).toBe(false);
  expect.soft(monitor.problems, `${label}：頁面上有錯誤\n  - ${monitor.problems.join('\n  - ')}`).toEqual([]);

  if (test.info().errors.length) {
    await test.info().attach(`${label}.png`, { body: await page.screenshot({ fullPage: true }).catch(() => Buffer.from('')), contentType: 'image/png' });
  }
  return { status: mainStatus, url, title, html };
}
