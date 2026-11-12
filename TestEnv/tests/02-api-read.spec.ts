/**
 * 02 讀取 API 測試：各角色呼叫所有「讀取」API，不應出現伺服器錯誤
 * (前端畫面若吃到 500，通常就是空白或轉圈圈)
 */
import { test, APIRequestContext } from '@playwright/test';
import { READ_ENDPOINTS } from './catalog/endpoints';
import { getRole, ROLE_LABELS, RoleKey } from './helpers/env';
import { apiAs } from './helpers/auth';
import { post, expectOk } from './helpers/api';

const ROLES: RoleKey[] = ['admin', 'groupManager', 'employee'];

for (const roleKey of ROLES) {
  test.describe(`讀取 API｜${ROLE_LABELS[roleKey]}`, () => {
    let api: APIRequestContext;

    test.beforeAll(async () => {
      const u = getRole(roleKey);
      if (u) api = await apiAs(u.empno);
    });
    test.afterAll(async () => api?.dispose());

    for (const ep of READ_ENDPOINTS) {
      const label = ep.data ? `${ep.url} ${JSON.stringify(ep.data)}` : ep.url;
      test(label, async () => {
        test.skip(!getRole(roleKey), `找不到「${ROLE_LABELS[roleKey]}」`);
        const r = await post(api, ep.url, ep.data || {});
        expectOk(r, `[${ROLE_LABELS[roleKey]}] ${label}`);
        if (r.ms > 5000) {
          test.info().annotations.push({ type: '效能', description: `回應時間 ${r.ms} ms` });
        }
      });
    }
  });
}
