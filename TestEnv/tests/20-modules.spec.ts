/**
 * 20 各模組功能測試 (新增 → 查詢 → 修改 → 刪除)
 * 只在測試站台的資料副本上操作。
 *
 * 新增模組測試：複製一個 test.describe 區塊，照著呼叫該模組的 API 即可。
 */
import { test, expect } from '@playwright/test';
import { requireRole } from './helpers/env';
import { apiAs } from './helpers/auth';
import { post, postMultipart, expectOk, firstLines } from './helpers/api';
import { ROC_YYMM } from './catalog/endpoints';

const stamp = () => new Date().toISOString().replace(/\D/g, '').slice(4, 14);

test.describe('許願池 Wish', () => {
  test('提出需求 (含附件) → 清單中看得到', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const purpose = `自動化測試需求 ${stamp()}`;
    const wish = { category: '系統功能', purpose, detail: '測試用，可刪除' };

    const r = await postMultipart(api, 'Wish/InsertWish', {
      wish: JSON.stringify(wish),
      file: { name: 'note.txt', mimeType: 'text/plain', buffer: Buffer.from('附件內容') },
    });
    expectOk(r, 'Wish/InsertWish');
    expect(r.json, `新增失敗：${r.text.slice(0, 200)}`).toBe(true);

    const list = await post(api, 'Wish/GetAll');
    expectOk(list, 'Wish/GetAll');
    const found = (list.json as any[]).find((x) => (x.wish || x).purpose === purpose);
    expect(found, '新增的需求沒有出現在清單').toBeTruthy();
    await api.dispose();
  });
});

test.describe('議題管控 IssueV2', () => {
  test('計畫與議題：新增 → 查詢 → 修改 → 刪除', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const group_one = emp.group_one!;
    const name = `測試計畫 ${stamp()}`;

    expect((await post(api, 'IssueV2/CreateProject', { project: { name, group_one, project_type: 0, finished: false } })).json, '新增計畫失敗').toBe(true);

    let projects = (await post(api, 'IssueV2/GetProjectsByGroupOne', { group_one })).json as any[];
    const project = projects.find((p) => p.name === name);
    expect(project, '新增的計畫查不到').toBeTruthy();

    const content = `測試議題 ${stamp()}`;
    const issue = { project_id: project.id, importance: 1, category: '測試', content, members: emp.name, progress: '進行中', status: '0', registered_date: '2026/11/12', finished_date: '' };
    expect((await post(api, 'IssueV2/CreateIssue', { issue })).json, '新增議題失敗').toBe(true);

    projects = (await post(api, 'IssueV2/GetProjectsByGroupOne', { group_one })).json as any[];
    const created = projects.find((p) => p.id === project.id)?.issues?.find((i: any) => i.content === content);
    expect(created, '新增的議題查不到').toBeTruthy();

    expect((await post(api, 'IssueV2/UpdateIssue', { issue: { ...created, progress: '已完成' } })).json, '修改議題失敗').toBe(true);
    projects = (await post(api, 'IssueV2/GetProjectsByGroupOne', { group_one })).json as any[];
    expect(projects.find((p) => p.id === project.id)?.issues?.find((i: any) => i.id === created.id)?.progress).toBe('已完成');

    expect((await post(api, 'IssueV2/UpdateProject', { project: { ...project, name: name + '(改)', issues: [] } })).json, '修改計畫失敗').toBe(true);
    expect((await post(api, 'IssueV2/DeleteIssue', { issue: created })).json, '刪除議題失敗').toBe(true);
    expect((await post(api, 'IssueV2/DeleteProject', { project: { id: project.id } })).json, '刪除計畫失敗').toBe(true);

    projects = (await post(api, 'IssueV2/GetProjectsByGroupOne', { group_one })).json as any[];
    expect(projects.find((p) => p.id === project.id), '刪除後計畫仍存在').toBeFalsy();
    await api.dispose();
  });
});

test.describe('辦公資源 Facility', () => {
  test('每個公用裝置的行事曆都能讀取', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const devices = (await post(api, 'Facility/GetDevices')).json as any[];
    const ids = Array.from(new Set((devices || []).map((d) => d.deviceID).filter(Boolean)));
    test.info().annotations.push({ type: '裝置數', description: String(ids.length) });
    for (const deviceID of ids) {
      const r = await post(api, 'Facility/GetEvents', { deviceID });
      expect.soft(r.isServerErrorPage, `裝置 ${deviceID} 的行事曆讀取失敗（可能有日期格式異常的預約）：${firstLines(r.text)}`).toBe(false);
    }
    await api.dispose();
  });
});

test.describe('工作紀錄管控表 Tasklog', () => {
  test('最近 6 個月的工作紀錄都能讀取', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const d = new Date();
    for (let i = 0; i < 6; i++) {
      const m = new Date(d.getFullYear(), d.getMonth() - i, 1);
      const yymm = `${m.getFullYear() - 1911}${String(m.getMonth() + 1).padStart(2, '0')}`;
      const r = await post(api, 'Tasklog/GetTasklogData', { empno: emp.empno, yymm });
      expect.soft(r.isServerErrorPage, `${yymm} 讀取失敗：${firstLines(r.text)}`).toBe(false);
    }
    expectOk(await post(api, 'Tasklog/GetAllMonthlyRecord', { yymm: ROC_YYMM }), 'GetAllMonthlyRecord');
    await api.dispose();
  });
});

test.describe('人才培育資料庫 Talent', () => {
  test('主管可以讀取部門員工履歷；員工可以讀取自己的履歷', async () => {
    const mgr = requireRole('deptManager');
    const emp = requireRole('employee');

    const mgrApi = await apiAs(mgr.empno);
    const r1 = await post(mgrApi, 'Talent/Get', { empno: emp.empno });
    expectOk(r1, '主管讀取員工履歷');

    const empApi = await apiAs(emp.empno);
    const r2 = await post(empApi, 'Talent/Get', { empno: emp.empno });
    expectOk(r2, '員工讀取自己的履歷');
    await mgrApi.dispose();
    await empApi.dispose();
  });
});

test.describe('升等規劃 Promotion', () => {
  test('員工可以讀取自己的升等條件', async () => {
    const emp = requireRole('employee');
    const api = await apiAs(emp.empno);
    const r = await post(api, 'Promotion/GetByUser', { empno: emp.empno });
    expectOk(r, 'Promotion/GetByUser');
    expect(Array.isArray(r.json) && r.json.length > 0, '沒有任何升等條件').toBe(true);
    await api.dispose();
  });
});
