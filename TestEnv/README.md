# 測試環境 (TestEnv)

軌道二部員工平台的「模擬環境」與「自動化測試」。

- **模擬環境**：在本機開一個測試用的網站副本，可以切換成任何員工身分（含強制開/關管理員），資料是 App_Data 的副本，隨便新增、上傳、刪除都不會影響開發資料，一鍵還原。
- **自動化測試**：用 Playwright 自動開瀏覽器、呼叫 API，逐頁、逐角色檢查 bug 與權限問題，產生 HTML 報告。

> 專案原始碼、專案的 Web.config、專案的 App_Data 都不會被修改。測試只在 `TestEnv\.runtime\` 底下的副本進行。

---

## 一、第一次使用

需要：Visual Studio（含 IIS Express）、[Node.js 18 以上](https://nodejs.org)。

1. 用 Visual Studio 開啟方案，確認可以用 **Debug** 建置。
2. 雙擊 **`TestEnv\開啟模擬環境.cmd`**
   - 會自動以 Debug 編譯、複製網站與資料、啟動 `http://localhost:5080`，並開啟身分切換頁。
3. 雙擊 **`TestEnv\執行全部測試.cmd`**
   - 第一次會安裝測試套件（幾分鐘），之後直接執行，完成後自動開啟測試報告。

公司網路若需要 Proxy 才能裝 npm 套件：`npm config set proxy http://proxy:port`、`npm config set https-proxy http://proxy:port`。

---

## 二、模擬環境（手動操作）

開啟 `http://localhost:5080/DevTools`：

| 功能 | 說明 |
|---|---|
| 選擇身分 | 列出 User.db 所有員工與角色（系統管理員、部門主管、組長、小組主管、計畫經理、一般員工），按「登入」即以該員工進入網站 |
| 管理員覆寫 | 「強制開啟」：讓一般員工暫時擁有管理員權限；「強制關閉」：讓管理員以一般權限瀏覽，用來確認「誰看不到」 |
| 自訂員編 | 輸入不存在的員編，可測試未授權畫面 |
| 重置測試資料 | 把測試站台的 App_Data 還原成種子資料（網站自動重啟幾秒） |

常用情境：新功能上線前，用「一般員工」、「組長」、「管理員」各登入一次，確認按鈕顯示與操作結果是否符合預期。

> 與既有的「Admin 假冒員工」功能不同：DevTools 切換身分會完全重設 Session，不會殘留管理員權限。

### 啟動參數（PowerShell）

```powershell
.\scripts\Start-TestSite.ps1               # 編譯 + 還原資料 + 啟動
.\scripts\Start-TestSite.ps1 -NoBuild      # 已在 VS 建置過，略過編譯
.\scripts\Start-TestSite.ps1 -KeepData     # 保留上次的測試資料
.\scripts\Start-TestSite.ps1 -RefreshSeed  # 從專案 App_Data 重新取一份種子資料
.\scripts\Stop-TestSite.ps1                # 停止
```

---

## 三、自動化測試

```powershell
.\scripts\Run-Tests.ps1                    # 全部
.\scripts\Run-Tests.ps1 -Project smoke     # 只跑冒煙測試
.\scripts\Run-Tests.ps1 -Only forum        # 只跑我要抱抱
.\scripts\Run-Tests.ps1 -Headed            # 看著瀏覽器跑
.\scripts\Run-Tests.ps1 -Restart           # 改了 C# 後：重新編譯、重啟再測
npx playwright test --ui                   # 互動模式，可逐一點選測試
```

| 檔案 | 內容 |
|---|---|
| `01-smoke.spec.ts` | 6 種角色 × 全站約 67 個頁面（含 AngularJS 子頁）逐頁開啟：500 錯誤、JS 錯誤、API 失敗、被導向未授權頁；舊版頁面；直接從書籤進入（Session 空白）|
| `02-api-read.spec.ts` | 約 60 支讀取 API × 3 種角色，不應出現伺服器錯誤 |
| `03-access-control.spec.ts` | 一般員工不能讀管理員資料、不能看別人的履歷 / 升等 / 自評、不能用 `..\` 讀伺服器檔案、裝置密碼不外洩 |
| `10-forum.spec.ts` | 我要抱抱：發文附圖（選檔 / 貼上 / 拖曳）、預覽移除、放大檢視、回文、匿名提醒、刪除權限（畫面與 API）、格式 / 張數 / 大小限制、EXIF 移除、轉正、縮圖、GIF、刪除後圖片失效、匿名保護、XSS |
| `20-modules.spec.ts` | 許願池、議題管控、公用裝置、工作紀錄、人才資料庫、升等的基本流程 |
| `90-destructive-permissions.spec.ts` | 一般員工呼叫「清空資料」類 API、刪改別人資料是否被擋（前後自動還原資料）|

報告：`TestEnv\playwright-report\index.html`。每個失敗都附截圖與錯誤說明；點進去可看 trace（逐步重播）。

### 測試角色

測試開始時會從 User.db 自動挑選每種角色的代表人物（列在測試輸出最上方）。要指定特定人時設定環境變數：

```powershell
$env:TE_ROLE_EMPLOYEE = '1234'; $env:TE_ROLE_ADMIN = '7291'
# 其他：TE_ROLE_DEPT_MANAGER、TE_ROLE_GROUP_MANAGER、TE_ROLE_SUB_MANAGER、TE_ROLE_PM、TE_ROLE_EMPLOYEE2
```

### 其他設定

| 環境變數 | 用途 |
|---|---|
| `TE_SMOKE_ROLES=admin,employee` | 冒煙測試只跑部分角色（加快速度）|
| `TE_BROWSER=chromium` | 改用 Playwright 內建 Chromium（需先 `npx playwright install chromium`；預設用 Windows 內建 Edge）|
| `TE_NO_RESET=1` | 測試前不還原資料 |
| `TE_WORKERS=3` | 同時執行數（預設 1；>1 時會略過 90 號測試）|

---

## 四、新增功能時如何加測試

**新增頁面** → `tests/catalog/pages.ts` 加一行，冒煙測試會自動以每種角色開啟。

**新增 API** → 讀取類加到 `tests/catalog/endpoints.ts` 的 `READ_ENDPOINTS`；只有管理員能用的加到 `ADMIN_ONLY_READS` 或 `DESTRUCTIVE_ENDPOINTS`。

**新增功能流程**（例如我要抱抱的新按鈕）→ 參考 `10-forum.spec.ts`：

```ts
test('一般員工看不到 XX 按鈕，管理員看得到', async ({ page }) => {
  const emp = requireRole('employee');
  await loginPage(page, emp.empno);                 // 以一般員工登入
  await page.goto('/Forum/Post/1');
  await expect(page.getByRole('button', { name: 'XX' })).toHaveCount(0);

  const admin = requireRole('admin');
  await loginPage(page, admin.empno);               // 切換成管理員
  await page.goto('/Forum/Post/1');
  await expect(page.getByRole('button', { name: 'XX' })).toBeVisible();
});

test('一般員工直接呼叫 API 也會被拒絕', async () => {
  const api = await apiAs(requireRole('employee').empno);
  expect(wasDenied(await post(api, 'Forum/NewAction', { id: 1 }))).toBe(true);
});
```

也可以用 `npx playwright codegen http://localhost:5080` 邊點畫面邊產生測試程式碼。

> **不要**把會寄 Email、連外部系統的 API 加進測試（例如 `Training/SendExternalTrainingMail`、`Facility/Change`、`Facility/GetSensorResourceData`）。

---

## 五、安全性說明

測試登入（`Filters\TestMode.cs`、`Controllers\DevToolsController.cs`）同時需要三個條件才會生效：

1. **Debug 編譯**：程式碼包在 `#if DEBUG`，Release 版 DLL 完全不含這些程式。
2. **Web.config 有 `TestMode=true`**：只有 `Start-TestSite.ps1` 產生的測試站台副本會加，專案的 Web.config 沒有。
3. **本機連線**（`Request.IsLocal`）。

條件不成立時 `/DevTools` 回應 404，登入流程與原本完全相同。發行到正式機請照舊使用 **Release** 組態。

---

## 六、疑難排解

| 狀況 | 處理 |
|---|---|
| 「連不到測試站台」 | 先執行 `開啟模擬環境.cmd`；確認是 Debug 建置 |
| 啟動逾時 | 看工作列上最小化的 IIS Express 視窗訊息；或 5080 被占用 → `Start-TestSite.ps1 -Port 5081`，測試時設 `$env:TE_BASE_URL='http://localhost:5081'` |
| 「找不到 MSBuild」 | 在 VS 建置後用 `-NoBuild` |
| 重置失敗（檔案被鎖） | 停止測試站台再啟動（會自動還原資料）|
| 某角色的測試都被略過 | User.db 沒有該角色的人，或用 `TE_ROLE_*` 指定 |
