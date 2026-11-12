import { defineConfig } from '@playwright/test';
import { BASE_URL } from './tests/helpers/env';

/**
 * 軌道二部員工平台 - 自動化測試設定
 *
 * 測試對象：TestEnv\scripts\Start-TestSite.ps1 啟動的「測試站台」(預設 http://localhost:5080)
 * 測試資料：測試站台使用 App_Data 的副本，開始測試前會自動還原 (可用環境變數 TE_NO_RESET=1 關閉)
 *
 * 專案：
 *   smoke       每個角色逐頁開啟，檢查 500 錯誤、JS 錯誤、API 失敗
 *   functional  API 讀取、權限、我要抱抱、各模組功能
 *   destructive 一般員工呼叫「清空資料」類 API 是否被擋 (執行後自動還原資料)
 */
export default defineConfig({
  testDir: './tests',
  outputDir: './test-results',
  globalSetup: require.resolve('./tests/global-setup'),

  // SQLite 不適合多個測試同時寫入，預設逐一執行以免誤判
  workers: Number(process.env.TE_WORKERS || 1),
  fullyParallel: false,
  retries: 0,
  timeout: 60_000,
  expect: { timeout: 10_000 },

  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
  ],

  use: {
    baseURL: BASE_URL,
    // 預設用 Windows 內建的 Edge，不必另外下載瀏覽器；設定 TE_BROWSER=chromium 改用 Playwright 的 Chromium
    channel: process.env.TE_BROWSER === 'chromium' ? undefined : (process.env.TE_BROWSER || 'msedge'),
    locale: 'zh-TW',
    timezoneId: 'Asia/Taipei',
    viewport: { width: 1400, height: 900 },
    actionTimeout: 15_000,
    navigationTimeout: 60_000,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
    ignoreHTTPSErrors: true,
  },

  // 依序執行 (workers=1 時按照下列順序)；不設 dependencies，避免前面發現 bug 就跳過後面的測試
  projects: [
    { name: 'smoke', testMatch: /(^|[\\/])01-[^\\/]+\.spec\.ts$/ },
    { name: 'functional', testMatch: /(^|[\\/])(0[2-9]|[1-8]\d)-[^\\/]+\.spec\.ts$/ },
    // 會清空資料的權限測試：前後都會還原資料；多執行緒時跳過以免干擾其他測試
    { name: 'destructive', testMatch: /(^|[\\/])9\d-[^\\/]+\.spec\.ts$/, testIgnore: Number(process.env.TE_WORKERS || 1) > 1 ? /.*/ : undefined },
  ],
});
