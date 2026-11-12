/**
 * 03 權限測試 (只讀取，不改資料)
 *
 * 前端「看不到按鈕」不代表安全：這裡直接呼叫後端 API，
 * 確認一般員工無法取得管理員資料、無法看別人的個人資料、無法讀取伺服器檔案。
 */
import { test, expect, APIRequestContext } from '@playwright/test';
import { ADMIN_ONLY_READS } from './catalog/endpoints';
import { requireRole } from './helpers/env';
import { apiAs } from './helpers/auth';
import { post, get, expectOk, wasDenied, firstLines } from './helpers/api';

test.describe('權限｜管理員專用資料', () => {
  for (const ep of ADMIN_ONLY_READS) {
    test(`一般員工不能讀取 ${ep.url}（${ep.note}）`, async () => {
      const emp = requireRole('employee');
      const api = await apiAs(emp.empno);
      const r = await post(api, ep.url, ep.data || {});
      expect(
        wasDenied(r),
        `一般員工 ${emp.empno} 可以讀取 ${ep.url}（${ep.note}），回傳 ${Array.isArray(r.json) ? r.json.length + ' 筆' : r.text.slice(0, 100)}。後端缺少管理員權限檢查。`,
      ).toBe(true);
      await api.dispose();
    });

    test(`管理員可以讀取 ${ep.url}`, async () => {
      const admin = requireRole('admin');
      const api = await apiAs(admin.empno);
      expectOk(await post(api, ep.url, ep.data || {}), ep.url);
      await api.dispose();
    });
  }

  test('一般員工不能下載全體員工資料 Home/DownloadUserDB', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const res = await api.get('Home/DownloadUserDB');
    const body = await res.body();
    const isXlsx = res.status() === 200 && body.slice(0, 2).toString() === 'PK';
    expect(isXlsx, `一般員工可以下載 userDB.xlsx (${body.length} bytes)`).toBe(false);
    await api.dispose();
  });

  test('一般員工直接輸入網址不能進入管理頁 Home/Admin', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const r = await get(api, 'Home/Admin');
    const opened = r.status === 200 && /AdminCtrl/.test(r.text);
    expect(opened, '一般員工可以直接開啟 /Home/Admin（首頁只是把連結藏起來，頁面本身沒有檢查權限）').toBe(false);
    await api.dispose();
  });
});

test.describe('權限｜不能看別人的個人資料', () => {
  let api: APIRequestContext;
  let me: ReturnType<typeof requireRole>;
  let other: ReturnType<typeof requireRole>;

  test.beforeEach(async () => {
    me = requireRole('employee');
    other = requireRole('employee2');
    api = await apiAs(me.empno);
  });
  test.afterEach(async () => api?.dispose());

  test('人才資料庫履歷 Talent/Get', async () => {
    const r = await post(api, 'Talent/Get', { empno: other.empno });
    expectOk(r, 'Talent/Get');
    const leaked = Array.isArray(r.json) && r.json.some((x: any) => x && x.empno === other.empno);
    expect(
      leaked,
      `一般員工 ${me.empno} 可以讀取 ${other.empno} 的履歷。TalentController.Get 用 Session["leader"] is false 判斷，但非主管的 Session["leader"] 是 null 不是 false，檢查永遠不成立。`,
    ).toBe(false);
  });

  test('升等規劃 Promotion/GetByUser', async () => {
    const r = await post(api, 'Promotion/GetByUser', { empno: other.empno });
    expectOk(r, 'Promotion/GetByUser');
    const leaked = Array.isArray(r.json) && r.json.some((x: any) => x && x.empno === other.empno);
    expect(leaked, `一般員工 ${me.empno} 可以讀取 ${other.empno} 的升等資料（含績效）`).toBe(false);
  });

  test('自我評估表 Assessment/GetResponse', async () => {
    const r = await post(api, 'Assessment/GetResponse', { employee: { empno: other.empno } });
    expectOk(r, 'Assessment/GetResponse');
    const leaked = r.isJson && r.json && (Array.isArray(r.json) ? r.json.length > 0 : Object.keys(r.json).length > 0);
    test.info().annotations.push({ type: '回傳', description: r.text.slice(0, 200) });
    expect(leaked, `一般員工 ${me.empno} 可以讀取 ${other.empno} 的自我評估表`).toBeFalsy();
  });

  test('核心專業個人資料 Profession/GetPersonal', async () => {
    const r = await post(api, 'Profession/GetPersonal', { empno: other.empno });
    expectOk(r, 'Profession/GetPersonal');
    const leaked = Array.isArray(r.json) ? r.json.length > 0 : !!(r.json && Object.keys(r.json).length);
    test.info().annotations.push({ type: '回傳', description: r.text.slice(0, 200) });
    expect(leaked, `一般員工 ${me.empno} 可以讀取 ${other.empno} 的核心專業個人資料`).toBeFalsy();
  });
});

test.describe('權限｜敏感資訊外洩', () => {
  for (const [url, field] of [
    ['Wish/DownloadFile', 'wish'],
    ['Training/DownloadFile', 'training'],
  ] as const) {
    test(`不能用 ..\\ 讀取伺服器檔案 ${url}`, async () => {
      const emp = requireRole('employee');
      const api = await apiAs(emp.empno);
      for (const p of ['..\\..\\Web.config', '../../Web.config']) {
        const res = await api.post(url, { data: { [field]: { filepath: p }, filepath: p } });
        const text = (await res.body()).toString('utf8');
        expect(/<configuration/i.test(text), `${url} 傳入 filepath="${p}" 可下載網站的 Web.config（路徑穿越漏洞）`).toBe(false);
      }
      await api.dispose();
    });
  }

  test('許願池狀態只有處理人能改 Wish/UpdateWishStatus', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const r = await post(api, 'Wish/UpdateWishStatus', { wish: { id: -1, status: 1 } });
    expect(wasDenied(r), `一般員工可以修改許願池狀態：${firstLines(r.text)}`).toBe(true);
    const auth = await post(api, 'Wish/GetAuthorization');
    expect(auth.json?.isHandler, '一般員工的畫面被開放編輯狀態').toBe(false);
    await api.dispose();
  });

  test('未登入不能讀取我要抱抱的圖片 Forum/Image', async () => {
    const api = await apiAs(null);
    const res = await api.get('Forum/Image/1');
    const type = res.headers()['content-type'] || '';
    expect(type.startsWith('image/'), '未登入可以直接讀取圖片').toBe(false);
    await api.dispose();
  });
});

test.describe('權限｜管理員切換身分', () => {
  test('切換成一般員工後沒有管理員權限，但可以切回自己並恢復', async () => {
    const admin = requireRole('admin');
    const emp = requireRole('employee');
    const api = await apiAs(admin.empno);

    let who = await (await api.get('DevTools/WhoAmI')).json();
    expect(who.Admin, '管理員登入後應有管理員權限').toBeTruthy();

    // 用網站右上角的「切換身分」(POST Home/Index)
    await api.post('Home/Index', { form: { empno: emp.empno } });
    who = await (await api.get('DevTools/WhoAmI')).json();
    expect(who.empno).toBe(emp.empno);
    expect(who.Admin, '切換成一般員工後仍保有管理員權限').toBeFalsy();
    expect(who.ImpersonatorAdmin, '應保留「可切回」的標記').toBeTruthy();
    expect(wasDenied(await post(api, 'Home/ReviewUserDB')), '切換成一般員工後仍可讀取全體員工資料').toBe(true);

    // 切回自己
    await api.post('Home/Index', { form: { empno: admin.empno } });
    who = await (await api.get('DevTools/WhoAmI')).json();
    expect(who.empno).toBe(admin.empno);
    expect(who.Admin, '切回自己後應恢復管理員權限').toBeTruthy();
    expect(who.ImpersonatorAdmin ?? null).toBeNull();
    await api.dispose();
  });

  test('一般員工不能使用切換身分', async () => {
    const emp = requireRole('employee');
    const other = requireRole('employee2');
    const api = await apiAs(emp.empno);
    await api.post('Home/Index', { form: { empno: other.empno } });
    const who = await (await api.get('DevTools/WhoAmI')).json();
    expect(who.empno, '一般員工可以切換成別人').toBe(emp.empno);
    await api.dispose();
  });
});

test.describe('權限｜Web.config 名單', () => {
  test('AdminPageUsers：可進入管理頁，但不能讀取全體員工資料', async () => {
    const u = requireRole('adminPageUser');
    const api = await apiAs(u.empno);
    const page = await get(api, 'Home/Admin');
    expect(page.status === 200 && /AdminCtrl/.test(page.text), 'AdminPageUsers 名單的人進不了管理頁').toBe(true);
    expect(wasDenied(await post(api, 'Home/ReviewUserDB')), 'AdminPageUsers 不應能讀取全體員工資料').toBe(true);
    await api.dispose();
  });

  test('員編前面補 0 (5 碼) 也能正確登入', async () => {
    const emp = requireRole('employee');
    test.skip(emp.empno.length !== 4, '此員工員編不是 4 碼');
    const api = await apiAs('0' + emp.empno);
    const who = await (await api.get('DevTools/WhoAmI')).json();
    expect(who.empno, `以 0${emp.empno} 登入失敗`).toBe(emp.empno);
    await api.dispose();
  });
});
