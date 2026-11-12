import * as fs from 'fs';
import { request } from '@playwright/test';
import { BASE_URL, ROLES_FILE, RUNTIME_DIR, ROLE_KEYS, ROLE_LABELS, RoleKey, RolesFile, TestUser } from './helpers/env';

/**
 * 測試開始前：
 *  1. 確認測試站台已啟動且為測試模式
 *  2. 還原測試資料 (除非 TE_NO_RESET=1)
 *  3. 從 User.db 挑出各角色的代表人物，寫入 .runtime/roles.json
 */
export default async function globalSetup() {
  const api = await request.newContext({ baseURL: BASE_URL });

  await waitHealthy(api, 20);

  if (process.env.TE_NO_RESET !== '1') {
    console.log('\n[測試準備] 還原測試資料 (DevTools/Reset)…');
    const r = await api.post('DevTools/Reset');
    const body = await r.json().catch(() => ({}));
    if (!body.ok) throw new Error(`還原測試資料失敗：${JSON.stringify(body)}`);
    await new Promise((res) => setTimeout(res, 1500));
    await waitHealthy(api, 60);
  }

  const res = await api.get('DevTools/Users');
  if (!res.ok()) throw new Error(`讀取員工清單失敗 HTTP ${res.status()}`);
  const users = (await res.json()) as TestUser[];

  const roles = pickRoles(users);
  const data: RolesFile = { roles, users };
  fs.mkdirSync(RUNTIME_DIR, { recursive: true });
  fs.writeFileSync(ROLES_FILE, JSON.stringify(data, null, 2), 'utf8');

  console.log('\n[測試準備] 測試角色：');
  for (const k of ROLE_KEYS) {
    const u = roles[k];
    console.log(`  ${ROLE_LABELS[k].padEnd(10, '　')} ${u ? `${u.empno} ${u.name}（${u.group_one || u.group || ''}）` : '— 找不到，相關測試會略過'}`);
  }
  console.log('');

  await api.dispose();
}

async function waitHealthy(api: Awaited<ReturnType<typeof request.newContext>>, seconds: number) {
  let lastErr = '';
  for (let i = 0; i < seconds; i++) {
    try {
      const r = await api.get('DevTools/Health', { timeout: 15_000 });
      if (r.ok()) {
        const j = await r.json();
        if (j.testMode) return;
      }
      lastErr = `HTTP ${r.status()}`;
    } catch (e: any) {
      lastErr = e.message;
    }
    await new Promise((res) => setTimeout(res, 1000));
  }
  throw new Error(
    `連不到測試站台 ${BASE_URL}/DevTools/Health (${lastErr})。\n` +
      `請先執行 TestEnv\\scripts\\Start-TestSite.ps1 (或雙擊 TestEnv\\開啟模擬環境.cmd)，` +
      `並確認專案是以 Debug 編譯。`,
  );
}

function pickRoles(users: TestUser[]): Partial<Record<RoleKey, TestUser>> {
  const byEnv = (k: RoleKey) => {
    const v = process.env[`TE_ROLE_${k.replace(/([A-Z])/g, '_$1').toUpperCase()}`];
    return v ? users.find((u) => u.empno === v) : undefined;
  };
  const isSub = (u: TestUser) => u.group_one_manager || u.group_two_manager || u.group_three_manager || u.group_four_manager;
  const anyMgr = (u: TestUser) => u.department_manager || u.group_manager || isSub(u);
  const hasGroup = (u: TestUser) => !!(u.group && u.group_one);
  // 「一般員工」不能帶任何特殊權限
  const special = (u: TestUser) => u.isAdmin || u.isForumAdmin || !!u.isTalentEditor || !!u.isWishHandler || !!u.isAdminPageUser || !!u.isFacilityManager;
  const valid = users.filter((u) => u.empno && u.name);

  const roles: Partial<Record<RoleKey, TestUser>> = {};
  roles.admin = byEnv('admin') || valid.find((u) => u.isAdmin && hasGroup(u)) || valid.find((u) => u.isAdmin);
  roles.deptManager = byEnv('deptManager') || valid.find((u) => !u.isAdmin && u.department_manager);
  roles.groupManager = byEnv('groupManager') || valid.find((u) => !u.isAdmin && u.group_manager && !u.department_manager);
  roles.subManager = byEnv('subManager') || valid.find((u) => !u.isAdmin && isSub(u) && !u.group_manager && !u.department_manager);
  roles.pm = byEnv('pm') || valid.find((u) => !u.isAdmin && u.project_manager && !anyMgr(u));
  roles.employee = byEnv('employee') || valid.find((u) => !special(u) && !anyMgr(u) && !u.project_manager && hasGroup(u));
  const e = roles.employee;
  roles.employee2 =
    byEnv('employee2') ||
    valid.find((u) => e && u.empno !== e.empno && !special(u) && !anyMgr(u) && !u.project_manager && hasGroup(u) && u.group_one !== e.group_one) ||
    valid.find((u) => e && u.empno !== e.empno && !special(u) && !anyMgr(u) && hasGroup(u));
  roles.talentEditor = byEnv('talentEditor') || valid.find((u) => !!u.isTalentEditor);
  roles.adminPageUser = byEnv('adminPageUser') || valid.find((u) => !!u.isAdminPageUser && !u.isAdmin);
  return roles;
}
