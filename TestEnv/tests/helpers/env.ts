import * as fs from 'fs';
import * as path from 'path';
import { test } from '@playwright/test';

/** 測試站台網址 (Start-TestSite.ps1 預設 5080) */
export const BASE_URL = (process.env.TE_BASE_URL || 'http://localhost:5080').replace(/\/$/, '');

/** TestEnv\.runtime：測試站台副本、種子資料、角色清單 */
export const RUNTIME_DIR = path.resolve(__dirname, '..', '..', '.runtime');
export const ROLES_FILE = path.join(RUNTIME_DIR, 'roles.json');
export const FIXTURES = path.resolve(__dirname, '..', 'fixtures');

export interface TestUser {
  empno: string;
  name: string;
  group: string | null;
  group_one: string | null;
  group_two: string | null;
  group_three: string | null;
  group_four: string | null;
  department_manager: boolean;
  group_manager: boolean;
  group_one_manager: boolean;
  group_two_manager: boolean;
  group_three_manager: boolean;
  group_four_manager: boolean;
  project_manager: boolean;
  isAdmin: boolean;
  isForumAdmin: boolean;
  isTalentEditor?: boolean;
  isWishHandler?: boolean;
  isAdminPageUser?: boolean;
  isFacilityManager?: boolean;
  roleLabel: string;
}

/**
 * 測試用的角色。global-setup 會從 User.db 自動挑出每個角色的代表人物，
 * 也可以用環境變數指定，例如 TE_ROLE_EMPLOYEE=1234
 */
export const ROLE_KEYS = ['admin', 'deptManager', 'groupManager', 'subManager', 'pm', 'employee', 'employee2', 'talentEditor', 'adminPageUser'] as const;
export type RoleKey = typeof ROLE_KEYS[number];

export const ROLE_LABELS: Record<RoleKey, string> = {
  admin: '系統管理員',
  deptManager: '部門主管',
  groupManager: '組長',
  subManager: '小組主管',
  pm: '計畫經理',
  employee: '一般員工',
  employee2: '一般員工(另一組)',
  talentEditor: '資協',
  adminPageUser: '管理頁使用者',
};

export interface RolesFile {
  roles: Partial<Record<RoleKey, TestUser>>;
  users: TestUser[];
}

let cache: RolesFile | null = null;

export function loadRoles(): RolesFile {
  if (!cache) {
    if (!fs.existsSync(ROLES_FILE)) {
      throw new Error(`找不到 ${ROLES_FILE}，請確認 global-setup 有執行 (npx playwright test)`);
    }
    cache = JSON.parse(fs.readFileSync(ROLES_FILE, 'utf8')) as RolesFile;
  }
  return cache;
}

export function getRole(key: RoleKey): TestUser | undefined {
  return loadRoles().roles[key];
}

/** 取得角色；資料庫找不到這種角色時，跳過該測試 */
export function requireRole(key: RoleKey): TestUser {
  const u = getRole(key);
  test.skip(!u, `User.db 中找不到「${ROLE_LABELS[key]}」角色，略過`);
  return u as TestUser;
}

export function describeUser(u: TestUser | undefined): string {
  return u ? `${u.empno} ${u.name}` : '(無)';
}
