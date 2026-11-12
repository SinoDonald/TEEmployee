import { APIRequestContext, APIResponse, expect } from '@playwright/test';

export interface ApiResult {
  status: number;
  contentType: string;
  text: string;
  json: any;
  isJson: boolean;
  /** ASP.NET 黃色錯誤頁 (Server Error) */
  isServerErrorPage: boolean;
  /** 被導向登入／未授權頁 */
  redirectedTo: string | null;
  ms: number;
}

/** 偵測 ASP.NET 錯誤頁 (英文或中文語系) */
export function looksLikeServerError(text: string): boolean {
  return /Server Error in '.*' Application|伺服器錯誤|應用程式中發生伺服器錯誤|Runtime Error|<title>\s*[^<]*Exception/i.test(text);
}

async function toResult(res: APIResponse, started: number): Promise<ApiResult> {
  const text = await res.text().catch(() => '');
  const contentType = res.headers()['content-type'] || '';
  let json: any = undefined;
  let isJson = false;
  try {
    json = JSON.parse(text);
    isJson = true;
  } catch { /* 非 JSON */ }
  const loc = res.headers()['location'];
  return {
    status: res.status(),
    contentType,
    text,
    json,
    isJson,
    isServerErrorPage: res.status() >= 500 || looksLikeServerError(text),
    redirectedTo: res.status() >= 300 && res.status() < 400 ? (loc || '') : null,
    ms: Date.now() - started,
  };
}

/**
 * 以 JSON 呼叫後端 (等同前端 AngularJS $http.post)
 * 例：post(api, 'Forum/GetPost', { id: 3 })
 */
export async function post(api: APIRequestContext, url: string, data: any = {}): Promise<ApiResult> {
  const started = Date.now();
  const res = await api.post(url.replace(/^\//, ''), { data, headers: { 'Content-Type': 'application/json; charset=utf-8' } });
  return toResult(res, started);
}

/** 以 multipart/form-data 呼叫 (上傳檔案) */
export async function postMultipart(api: APIRequestContext, url: string, multipart: Record<string, any>): Promise<ApiResult> {
  const started = Date.now();
  const res = await api.post(url.replace(/^\//, ''), { multipart });
  return toResult(res, started);
}

/** multipart 但同一個欄位有多個檔案 (Playwright 的 multipart 物件不支援重複 key，改用 FormData) */
export async function postFormData(api: APIRequestContext, url: string, form: FormData): Promise<ApiResult> {
  const started = Date.now();
  const res = await api.post(url.replace(/^\//, ''), { multipart: form });
  return toResult(res, started);
}

export async function get(api: APIRequestContext, url: string): Promise<ApiResult> {
  const started = Date.now();
  const res = await api.get(url.replace(/^\//, ''));
  return toResult(res, started);
}

/** 呼叫成功：HTTP 200、不是錯誤頁、沒有被導向未授權頁 */
export function expectOk(r: ApiResult, label: string) {
  expect.soft(r.isServerErrorPage, `${label}：伺服器錯誤 (HTTP ${r.status})\n${firstLines(r.text)}`).toBe(false);
  expect.soft(r.redirectedTo, `${label}：被導向 ${r.redirectedTo}`).toBeNull();
  expect.soft(r.status, `${label}：HTTP ${r.status}`).toBe(200);
}

/**
 * 是否「被拒絕」：回傳 false / 空值 / 403 / 401 / 導向未授權頁，都算有擋。
 * 回傳 true 或實際資料 = 沒擋 (權限漏洞)
 */
export function wasDenied(r: ApiResult): boolean {
  if (r.status === 401 || r.status === 403 || r.status === 404) return true;
  if (r.redirectedTo !== null) return true;
  if (r.status >= 500) return false;           // 伺服器有執行，只是出錯 → 沒擋
  if (!r.isJson) return r.text.trim() === '' || /^false$/i.test(r.text.trim());
  const j = r.json;
  // 後端以文字訊息回覆「沒有權限」也算有擋 (例如公用裝置的 API 回傳字串給前端 alert)
  if (typeof j === 'string' && /權限|不允許|無法/.test(j)) return true;
  if (j === false || j === null || j === '' || j === 0) return true;
  if (Array.isArray(j)) return j.length === 0;
  if (typeof j === 'object' && (j.ok === false || j.success === false)) return true;
  return false;
}

export function firstLines(text: string, n = 6): string {
  // 從錯誤頁抓出例外訊息，方便閱讀
  const m = text.match(/<title>([\s\S]*?)<\/title>/i);
  const title = m ? m[1].replace(/\s+/g, ' ').trim() : '';
  const body = text.replace(/<[^>]+>/g, ' ').replace(/\s+/g, ' ').trim();
  return (title ? `[${title}] ` : '') + body.slice(0, 400) + (body.length > 400 ? '…' : '') + (n ? '' : '');
}
