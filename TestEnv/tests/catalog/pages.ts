/**
 * 全站頁面清單 (冒煙測試逐頁開啟)
 *
 * 新增頁面時在這裡加一行即可。
 *   path   相對網址；AngularJS ui-router 子頁用 "#!/狀態"
 *   legacy true = 舊版或不在選單中的頁面 (另外分組回報)
 */
export interface PageDef {
  module: string;
  name: string;
  path: string;
  legacy?: boolean;
}

export const PAGES: PageDef[] = [
  // 首頁 / 說明
  { module: '首頁', name: '首頁', path: '/' },
  { module: '首頁', name: '操作說明', path: '/Home/Document' },
  { module: '首頁', name: 'Q&A', path: '/Home/Question' },
  { module: '首頁', name: '平台更新歷程', path: '/Home/History' },
  { module: '首頁', name: '管理頁 Admin', path: '/Home/Admin' },

  // 我要抱抱
  { module: '我要抱抱', name: '貼文列表', path: '/Forum' },

  // 許願池
  { module: '許願池', name: '許願池', path: '/Wish' },

  // 自評表與考核
  { module: '自評表與考核', name: '選單', path: '/Assessment' },
  { module: '自評表與考核', name: '自我評估表', path: '/Assessment/Self' },
  { module: '自評表與考核', name: '給予主管建議', path: '/Assessment/Manage' },
  { module: '自評表與考核', name: '給予主管建議-選項', path: '/Assessment/Manage#!/ManagerOption' },
  { module: '自評表與考核', name: '給予主管建議-填寫', path: '/Assessment/Manage#!/ManagerSuggest' },
  { module: '自評表與考核', name: '給予主管建議-設定主管', path: '/Assessment/Manage#!/SetManager' },
  { module: '自評表與考核', name: '員工評估', path: '/Assessment/Employee' },
  { module: '自評表與考核', name: '員工評估-清單', path: '/Assessment/Employee#!/EmpList' },
  { module: '自評表與考核', name: '員工評估-檢視', path: '/Assessment/Employee#!/Review' },
  { module: '自評表與考核', name: '員工評估-評估', path: '/Assessment/Employee#!/AssessEmp' },
  { module: '自評表與考核', name: '員工評估-比較', path: '/Assessment/Employee#!/Compare' },
  { module: '自評表與考核', name: '主管回饋', path: '/Assessment/Feedback' },
  { module: '自評表與考核', name: '主管回饋-員工清單', path: '/Assessment/Feedback#!/EmployeeList' },
  { module: '自評表與考核', name: '主管回饋-評估', path: '/Assessment/Feedback#!/AssessEmployee' },
  { module: '自評表與考核', name: '主管回饋-檢視', path: '/Assessment/Feedback#!/ReviewEmployee' },
  { module: '自評表與考核', name: '自評圖表', path: '/Assessment/SelfChart' },
  { module: '自評表與考核', name: '圖表選單', path: '/Assessment/ChartMenu' },
  { module: '自評表與考核', name: '員工圖表', path: '/Assessment/ChartEmployee' },
  { module: '自評表與考核', name: '主管圖表', path: '/Assessment/ChartManager' },
  { module: '自評表與考核', name: '績效圖表', path: '/Assessment/ChartPerformance' },

  // 年度行事曆目標
  { module: '年度行事曆目標', name: '首頁', path: '/GSchedule' },
  { module: '年度行事曆目標', name: '組規劃', path: '/GSchedule#!/GroupPlan' },
  { module: '年度行事曆目標', name: '個人規劃', path: '/GSchedule#!/PersonalPlan' },
  { module: '年度行事曆目標', name: '組行事曆', path: '/GSchedule#!/Group' },
  { module: '年度行事曆目標', name: '個人行事曆', path: '/GSchedule#!/Personal' },
  { module: '年度行事曆目標', name: '未來3年規劃', path: '/GSchedule#!/Future' },
  { module: '年度行事曆目標', name: '計畫', path: '/GSchedule#!/Project' },

  // 工作紀錄管控表
  { module: '工作紀錄管控表', name: '首頁', path: '/Tasklog' },
  { module: '工作紀錄管控表', name: '填寫', path: '/Tasklog/Edit' },
  { module: '工作紀錄管控表', name: '清單', path: '/Tasklog/List' },
  { module: '工作紀錄管控表', name: '清單-人員', path: '/Tasklog/List#!/UserList' },
  { module: '工作紀錄管控表', name: '清單-個人明細', path: '/Tasklog/List#!/UserDetails' },
  { module: '工作紀錄管控表', name: '清單-多人明細', path: '/Tasklog/List#!/UsersDetails' },

  // 人才培育資料庫
  { module: '人才培育資料庫', name: '首頁', path: '/Talent' },
  { module: '人才培育資料庫', name: '篩選', path: '/Talent#!/TalentOption' },
  { module: '人才培育資料庫', name: '高績效人員', path: '/Talent#!/TalentHighPerformers' },
  { module: '人才培育資料庫', name: '成功案例', path: '/Talent#!/TalentSuccessExample' },
  { module: '人才培育資料庫', name: '履歷', path: '/Talent#!/TalentRecord' },

  // 核心專業盤點
  { module: '核心專業盤點', name: '首頁', path: '/Profession' },
  { module: '核心專業盤點', name: '技能', path: '/Profession#!/Skill' },
  { module: '核心專業盤點', name: '評分', path: '/Profession#!/Score' },
  { module: '核心專業盤點', name: '圖表', path: '/Profession#!/Chart' },
  { module: '核心專業盤點', name: '散佈圖', path: '/Profession#!/Scatter' },
  { module: '核心專業盤點', name: '個人', path: '/Profession#!/Personal' },

  // 升等規劃與條件
  { module: '升等規劃與條件', name: '升等規劃', path: '/Promotion' },

  // 拓展專業培養
  { module: '拓展專業培養', name: '首頁', path: '/EducationV2' },
  { module: '拓展專業培養', name: '指派選單', path: '/EducationV2/AssignMenu' },
  { module: '拓展專業培養', name: '指派', path: '/EducationV2/Assign' },
  { module: '拓展專業培養', name: '取消指派', path: '/EducationV2/Unassign' },
  { module: '拓展專業培養', name: '紀錄', path: '/EducationV2/Record' },

  // 培訓紀錄與規劃
  { module: '培訓紀錄與規劃', name: '首頁', path: '/Training' },
  { module: '培訓紀錄與規劃', name: '組培訓', path: '/Training/Group' },
  { module: '培訓紀錄與規劃', name: '外訓', path: '/Training/External' },

  // KPI
  { module: 'KPI', name: '填寫 KPI', path: '/GKpi/Fillin' },
  { module: 'KPI', name: 'KPI 回饋', path: '/GKpi/Feedback' },

  // 辦公資源借用
  { module: '辦公資源借用', name: '首頁', path: '/Facility' },
  { module: '辦公資源借用', name: '公用裝置', path: '/Facility/Borrow' },

  // 議題管控
  { module: '議題管控', name: '首頁', path: '/IssueV2' },
  { module: '議題管控', name: '清單', path: '/IssueV2/List' },

  // 票選活動
  { module: '票選活動', name: '票選', path: '/Ballot' },

  // ---------- 舊版 / 不在選單中的頁面 ----------
  { module: '舊版', name: 'KPI 首頁 (GKpi/Index)', path: '/GKpi', legacy: true },
  { module: '舊版', name: 'KPI v1 首頁', path: '/Kpi', legacy: true },
  { module: '舊版', name: 'KPI v1 填寫', path: '/Kpi/Fillin', legacy: true },
  { module: '舊版', name: 'KPI v1 回饋', path: '/Kpi/Feedback', legacy: true },
  { module: '舊版', name: '教育訓練 v1', path: '/Education', legacy: true },
  { module: '舊版', name: '全域教育訓練', path: '/GEducation', legacy: true },
  { module: '舊版', name: '全域教育訓練-指派', path: '/GEducation/Assign', legacy: true },
  { module: '舊版', name: '全域教育訓練-課程', path: '/GEducation/Curriculum', legacy: true },
  { module: '舊版', name: '全域教育訓練-數位', path: '/GEducation/Digital', legacy: true },
  { module: '舊版', name: '行事曆 v1', path: '/Schedule', legacy: true },
  { module: '舊版', name: '行事曆 v1-組', path: '/Schedule#!/Group', legacy: true },
  { module: '舊版', name: '行事曆 v1-個人', path: '/Schedule#!/Individual', legacy: true },
  { module: '舊版', name: '議題管控 v1', path: '/Issue/List', legacy: true },
  { module: '舊版', name: '人才 Talent/Talent', path: '/Talent/Talent', legacy: true },
  { module: '舊版', name: '測試頁 Test', path: '/Test', legacy: true },
];

/**
 * 沒有加 [MyAuthorize] 的控制器：依賴「之前已在別頁載入的 Session」。
 * 使用者直接從書籤 / 通知連結進入、或 Session 逾時後，這些頁面可能出錯。
 */
export const NO_AUTHORIZE_PAGES: PageDef[] = [
  { module: 'KPI', name: '填寫 KPI', path: '/GKpi/Fillin' },
  { module: '人才培育資料庫', name: '首頁', path: '/Talent' },
  { module: '拓展專業培養', name: '首頁', path: '/EducationV2' },
  { module: '培訓紀錄與規劃', name: '首頁', path: '/Training' },
  { module: '議題管控', name: '清單', path: '/IssueV2/List' },
  { module: '票選活動', name: '票選', path: '/Ballot' },
];
