/**
 * 90 會改動資料的權限測試
 *
 * 用一般員工身分呼叫「清空資料」、「刪除別人的資料」等 API，確認後端有擋。
 * 如果沒擋，資料真的會被改掉，所以：
 *   - 只在測試站台的資料副本上執行
 *   - 本檔開始前、結束後都會自動還原測試資料 (DevTools/Reset)
 */
import { test, expect, request } from '@playwright/test';
import { DESTRUCTIVE_ENDPOINTS } from './catalog/endpoints';
import { BASE_URL, requireRole } from './helpers/env';
import { apiAs } from './helpers/auth';
import { post, wasDenied, firstLines } from './helpers/api';

test.describe.configure({ mode: 'default' });

async function resetData() {
  const api = await request.newContext({ baseURL: BASE_URL });
  const r = await api.post('DevTools/Reset');
  const j = await r.json().catch(() => ({}));
  if (!j.ok) throw new Error('還原測試資料失敗：' + JSON.stringify(j));
  // 網站重新啟動
  for (let i = 0; i < 60; i++) {
    await new Promise((res) => setTimeout(res, 1000));
    try {
      if ((await api.get('DevTools/Health')).ok()) break;
    } catch { /* 重啟中 */ }
  }
  await api.dispose();
}

test.beforeAll(async () => {
  await resetData();
});

test.afterAll(async () => {
  await resetData();
});

test.describe('一般員工不能執行清空 / 重建資料的 API', () => {
  for (const ep of DESTRUCTIVE_ENDPOINTS) {
    test(`${ep.url}（${ep.note}）`, async () => {
      const emp = requireRole('employee');
      const api = await apiAs(emp.empno);
      const r = await post(api, ep.url, ep.data || {});
      const detail = r.isServerErrorPage ? `伺服器執行到一半出錯 (HTTP ${r.status})：${firstLines(r.text)}` : `回傳：${r.text.slice(0, 150)}`;
      expect(wasDenied(r), `一般員工 ${emp.empno} 呼叫 ${ep.url} 沒有被拒絕。${detail}\n→ 請在控制器加上管理員權限檢查。`).toBe(true);
      await api.dispose();
    });
  }
});

test.describe('不能修改 / 刪除別人的資料', () => {
  test('公用裝置預約：不能刪除別人建立的預約 Facility/Delete', async () => {
    const emp = requireRole('employee2');
    const api = await apiAs(emp.empno);
    const rows = ((await post(api, 'Facility/GetDevices')).json || []) as any[];
    const target = rows.find((x) => x.id && x.modifiedUser && x.modifiedUser !== emp.empno && x.meetingDate);
    test.skip(!target, '測試資料中沒有別人建立的預約');
    const r = await post(api, 'Facility/Delete', { id: target.id });
    expect(wasDenied(r), `員工 ${emp.empno} 可以刪除 ${target.modifiedUser} 建立的預約 (id=${target.id})；畫面上只是把按鈕藏起來`).toBe(true);
    await api.dispose();
  });

  test('議題管控：不能刪除其他組的計畫 IssueV2/DeleteProject', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const all = ((await post(api, 'IssueV2/GetAllProjects')).json || []) as any[];
    const target = all.find((p) => p.group_one && p.group_one !== emp.group_one);
    test.skip(!target, '測試資料中沒有其他組的計畫');
    const r = await post(api, 'IssueV2/DeleteProject', { project: { id: target.id } });
    expect(wasDenied(r), `員工 ${emp.empno}（${emp.group_one}）可以刪除「${target.group_one}」的計畫「${target.name}」`).toBe(true);
    await api.dispose();
  });

  test('人才資料庫：不能修改別人的履歷規劃 Talent/SaveResponse', async () => {
    const emp = requireRole('employee');
    const other = requireRole('employee2');
    const api = await apiAs(emp.empno);
    const r = await post(api, 'Talent/SaveResponse', { userCV: { empno: other.empno }, planning: '自動化測試寫入' });
    expect(wasDenied(r), `員工 ${emp.empno} 可以修改 ${other.empno} 的履歷規劃`).toBe(true);
    await api.dispose();
  });

  test('人才資料庫：資協 (TalentEditor) 可以儲存', async () => {
    const editor = requireRole('talentEditor');
    const emp = requireRole('employee');
    const api = await apiAs(editor.empno);
    const r = await post(api, 'Talent/SaveResponse', { userCV: { empno: emp.empno }, planning: '自動化測試寫入\n' });
    expect(r.json, `資協 ${editor.empno} 無法儲存：${firstLines(r.text)}`).toBe(true);
    await api.dispose();
  });

  test('人才資料庫：資協以 5 碼員編 (前面補 0) 登入也可以儲存', async () => {
    const editor = requireRole('talentEditor');
    const emp = requireRole('employee');
    test.skip(editor.empno.length !== 4, '資協員編不是 4 碼');
    const api = await apiAs('0' + editor.empno);
    const r = await post(api, 'Talent/SaveResponse', { userCV: { empno: emp.empno }, planning: '自動化測試寫入\n' });
    expect(r.json, `以 0${editor.empno} 登入的資協無法儲存`).toBe(true);
    await api.dispose();
  });

  test('升等規劃：不能修改別人的升等資料 Promotion/Update', async () => {
    const emp = requireRole('employee');
    const other = requireRole('employee2');
    const api = await apiAs(emp.empno);
    const r = await post(api, 'Promotion/Update', { promotion: { empno: other.empno, condition: 1, content: '自動化測試寫入', achieved: true } });
    expect(wasDenied(r), `員工 ${emp.empno} 可以修改 ${other.empno} 的升等資料`).toBe(true);
    await api.dispose();
  });


});
