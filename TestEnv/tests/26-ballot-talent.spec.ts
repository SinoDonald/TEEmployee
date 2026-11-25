/**
 * 26 績優員工票選（多活動）＋ 人才培育資料庫儲存權限 (2026-11 更新)
 *
 * 只跑這支：.\scripts\Run-Tests.ps1 -Only 26-ballot -Restart
 *
 * 會寫入測試站台的資料 (票選一票、自我發展規劃)，只影響測試副本，下次測試開始前會自動還原。
 */
import { test, expect } from '@playwright/test';
import { getRole, requireRole, loadRoles, TestUser } from './helpers/env';
import { apiAs } from './helpers/auth';
import { post, expectOk, firstLines } from './helpers/api';

interface EventInfo { code: string; name: string; choiceCount: number; status: 'open' | 'closed' | 'notStarted' }

async function getEvents(empno: string): Promise<{ events: EventInfo[]; defaultCode: string | null }> {
  const api = await apiAs(empno);
  const r = await post(api, 'Ballot/GetEvents');
  await api.dispose();
  expectOk(r, 'Ballot/GetEvents');
  return r.json;
}

/** 下載票選統計，回傳是否拿到 Excel */
async function tryDownload(empno: string, code: string, admin: '' | '0' | '1' = '') {
  const api = await apiAs(empno, { admin });
  const res = await api.post('Ballot/DownloadEmployeeVoteExcel', { data: { event_name: code } });
  const body = await res.body();
  await api.dispose();
  return { status: res.status(), isXlsx: res.status() === 200 && body.slice(0, 2).toString() === 'PK', body };
}

// =============================================================================
test.describe('績優員工票選 Ballot', () => {
  test('GetEvents：回傳活動清單與預設活動', async () => {
    const emp = requireRole('employee');
    const data = await getEvents(emp.empno);
    expect(Array.isArray(data.events) && data.events.length > 0, '沒有任何票選活動').toBe(true);
    for (const e of data.events) {
      expect(e.code, '活動缺少 code').toBeTruthy();
      expect(['open', 'closed', 'notStarted']).toContain(e.status);
    }
    expect(data.events.map((e) => e.code), 'defaultCode 不在活動清單中').toContain(data.defaultCode);
    test.info().annotations.push({ type: '活動', description: data.events.map((e) => `${e.code} ${e.status}`).join('、') });
  });

  test('投票驗證：不存在的活動、已截止、人數不對、非候選人都會被擋', async () => {
    const emp = requireRole('employee');
    const { events } = await getEvents(emp.empno);
    const api = await apiAs(emp.empno);
    const vote = async (event_name: string, choices: string[]) =>
      (await post(api, 'Ballot/CreateBallot', { ballot: { event_name, choices: choices.join(';') } })).json;

    const cands = ((await post(api, 'Ballot/GetAllEmployeeCandidates')).json as any[] || []).map((c) => c.empno);

    let r = await vote('不存在的活動', cands.slice(0, 6));
    expect(r?.success, '不存在的活動卻投票成功').toBe(false);
    expect(r?.message).toContain('找不到');

    const closed = events.find((e) => e.status === 'closed');
    if (closed) {
      r = await vote(closed.code, cands.slice(0, closed.choiceCount));
      expect(r?.success, `已截止的 ${closed.code} 卻投票成功`).toBe(false);
      expect(r?.message).toContain('截止');
    }

    const open = events.find((e) => e.status === 'open');
    if (open) {
      r = await vote(open.code, cands.slice(0, open.choiceCount - 1));
      expect(r?.success, '人數不足卻投票成功').toBe(false);
      expect(r?.message).toContain(String(open.choiceCount));

      r = await vote(open.code, [...cands.slice(0, open.choiceCount - 1), '0000']);
      expect(r?.success, '選了非候選人卻投票成功').toBe(false);
    } else {
      test.info().annotations.push({ type: '提醒', description: '目前沒有投票中的活動，略過人數 / 候選人檢查' });
    }
    await api.dispose();
  });

  test('投票成功後不能重複投票；送出的員編以登入者為準', async () => {
    const emp = requireRole('employee');
    const other = getRole('employee2');
    const { events } = await getEvents(emp.empno);
    const open = events.find((e) => e.status === 'open');
    test.skip(!open, '目前沒有投票中的活動');

    const api = await apiAs(emp.empno);
    const cands = ((await post(api, 'Ballot/GetAllEmployeeCandidates')).json as any[] || []).map((c) => c.empno);
    test.skip(cands.length < open!.choiceCount, `候選人不足 ${open!.choiceCount} 位`);
    const choices = cands.slice(0, open!.choiceCount).join(';');

    // 嘗試冒用別人的員編投票：伺服器應改用登入者員編
    const r1 = (await post(api, 'Ballot/CreateBallot', { ballot: { event_name: open!.code, choices, empno: other?.empno || '9999' } })).json;
    if (r1?.success === false && /投過/.test(r1?.message || '')) {
      test.info().annotations.push({ type: '提醒', description: `${emp.empno} 在測試資料中已經投過票` });
    } else {
      expect(r1?.success, `投票失敗：${r1?.message}`).toBe(true);
    }
    const mine = (await post(api, 'Ballot/GetBallotByUserAndEvent', { event_name: open!.code })).json;
    expect(mine, '投票後查不到自己的選票 (可能被記到別人名下)').toBeTruthy();

    const r2 = (await post(api, 'Ballot/CreateBallot', { ballot: { event_name: open!.code, choices } })).json;
    expect(r2?.success, '同一人可以重複投票').toBe(false);
    expect(r2?.message).toContain('投過');
    await api.dispose();
  });

  test('下載統計：只限部門主管（一般員工、非部門主管的 Admin 都不行）', async () => {
    const emp = requireRole('employee');
    const mgr = requireRole('deptManager');
    const { events, defaultCode } = await getEvents(emp.empno);
    const code = (events.find((e) => e.status === 'closed') || events[0]).code || defaultCode!;

    expect((await tryDownload(emp.empno, code)).isXlsx, '一般員工可以下載票選統計').toBe(false);

    const adminNotMgr = loadRoles().users.find((u: TestUser) => u.isAdmin && !u.department_manager);
    if (adminNotMgr) {
      expect((await tryDownload(adminNotMgr.empno, code)).isXlsx, `Admin ${adminNotMgr.empno} (非部門主管) 可以下載票選統計`).toBe(false);
    }

    const d = await tryDownload(mgr.empno, code);
    expect(d.isXlsx, `部門主管 ${mgr.empno} 無法下載 ${code} 統計 (HTTP ${d.status})：${firstLines(d.body.toString('utf8'))}`).toBe(true);
  });
});

// =============================================================================
test.describe('人才培育資料庫 Talent｜儲存權限', () => {
  test('本人可以儲存自己的「自我發展規劃」', async () => {
    const emp = requireRole('employee');
    const mgr = requireRole('deptManager');

    // 先確認這位員工在測試資料中有履歷 (沒有履歷時 SaveFuture 本來就會回傳 false)
    const mgrApi = await apiAs(mgr.empno);
    const cv = (await post(mgrApi, 'Talent/Get', { empno: emp.empno })).json;
    await mgrApi.dispose();
    const hasCv = Array.isArray(cv) ? cv.length > 0 : !!cv;
    test.skip(!hasCv, `員工 ${emp.empno} 在測試資料中沒有履歷`);

    const api = await apiAs(emp.empno);
    const r = await post(api, 'Talent/SaveResponse', { userCV: { empno: emp.empno, future: '自動化測試：自我發展規劃' }, planning: '' });
    expect(r.json, `本人 ${emp.empno} 無法儲存自我發展規劃：${firstLines(r.text)}`).toBe(true);
    await api.dispose();
  });

  test('一般員工不能看別人的履歷 / PDF', async () => {
    const emp = requireRole('employee');
    const other = requireRole('employee2');
    const api = await apiAs(emp.empno);
    const r = await post(api, 'Talent/Get', { empno: other.empno });
    const j = r.json;
    const leaked = JSON.stringify(j ?? '').includes(other.empno);
    expect(leaked, `一般員工 ${emp.empno} 可以讀到 ${other.empno} 的履歷`).toBe(false);
    await api.dispose();
  });
});
