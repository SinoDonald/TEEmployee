/**
 * 後端 API 清單
 *
 * READ_ENDPOINTS        只讀取的 API：各角色呼叫都應該成功 (HTTP 200、沒有伺服器錯誤)
 * ADMIN_ONLY_READS      只有管理員應該看得到的資料：一般員工呼叫應被拒絕
 * DESTRUCTIVE_ENDPOINTS 會清空 / 大量修改資料的 API：一般員工呼叫應被拒絕
 *
 * 不要放進來的 API (測試不會呼叫)：
 *   Training/SendExternalTrainingMail   會寄真的 Email
 *   Facility/GetSensorResourceData      會連外部感測器
 *   Facility/Change                     會改 Teams 會議室密碼 (連外部)
 */

const now = new Date();
const year = now.getFullYear();
const rocYear = year - 1911;
const mm = String(now.getMonth() + 1).padStart(2, '0');
export const ROC_YYMM = `${rocYear}${mm}`;
export const YEAR = year;

export interface EndpointDef {
  url: string;
  data?: any;
  note?: string;
}

export const READ_ENDPOINTS: EndpointDef[] = [
  // 首頁
  { url: 'Home/Notify' },

  // 我要抱抱
  { url: 'Forum/GetAllPosts' },

  // 許願池
  { url: 'Wish/GetAll' },
  { url: 'Wish/GetAuthorization' },

  // 自評表與考核
  { url: 'Assessment/GetAllSelfAssessments' },
  { url: 'Assessment/UserManagers' },
  { url: 'Assessment/GetResponse', data: { employee: {} } },
  { url: 'Assessment/GetAllCategorySelfAssessmentCharts' },
  { url: 'Assessment/GetManagers' },
  { url: 'Assessment/GetScorePeople' },
  { url: 'Assessment/GetAllEmployeesWithStateByRole' },
  { url: 'Assessment/GetYearList' },
  { url: 'Assessment/GetManageYearList' },
  { url: 'Assessment/GetFeedbackNotification' },
  { url: 'Assessment/GetChartYearList', data: { isManagerResponse: false } },
  { url: 'Assessment/GetChartYearList', data: { isManagerResponse: true } },
  { url: 'Assessment/GetChartGroupList' },
  { url: 'Assessment/ManageResponseStateCheck' },

  // 年度行事曆目標
  { url: 'GSchedule/GetAuthorization', data: { page_id: '' } },
  { url: 'GSchedule/GetAuthorization', data: { page_id: '0' } },
  { url: 'GSchedule/GetAuthorization', data: { page_id: '1' } },
  { url: 'GSchedule/GetAuthorization', data: { page_id: '2' } },
  { url: 'GSchedule/GetAllSchedules' },
  { url: 'GSchedule/GetAllFutures' },
  { url: 'GSchedule/GetAllProjectSchedules' },
  { url: 'GSchedule/GetGroupList', data: { view: 'GroupPlan' } },
  { url: 'GSchedule/GetGroupList', data: { view: 'PersonalPlan' } },
  { url: 'GSchedule/GetYears', data: { view: 'GroupPlan' } },
  { url: 'GSchedule/GetYears', data: { view: 'PersonalPlan' } },
  { url: 'GSchedule/NotUploadUsers' },
  { url: 'GSchedule/GetAllAgents' },

  // 工作紀錄管控表
  { url: 'Tasklog/GetAllMonthlyRecord', data: { yymm: ROC_YYMM } },
  { url: 'Tasklog/GetTasklogData', data: { empno: '', yymm: ROC_YYMM } },

  // 人才培育資料庫
  { url: 'Talent/GetYears' },
  { url: 'Talent/GetGroupList' },
  { url: 'Talent/GetAll' },
  { url: 'Talent/GetSenioritys' },
  { url: 'Talent/HighPerformer' },

  // 核心專業盤點
  { url: 'Profession/GetAuthorization' },
  { url: 'Profession/GetAll' },

  // 升等
  { url: 'Promotion/GetAll' },
  { url: 'Promotion/GetByUser', data: { empno: '' } },

  // 拓展專業培養
  { url: 'EducationV2/GetAllContents' },
  { url: 'EducationV2/GetAllValidContents' },
  { url: 'EducationV2/GetAuthorization' },

  // 培訓
  { url: 'Training/GetRecentRecords' },
  { url: 'Training/GetExternalTrainings' },
  { url: 'Training/GetAuthorization' },
  { url: 'Training/GetGroupAuthorization' },
  { url: 'Training/GetAllRecords' },

  // KPI
  { url: 'GKpi/GetAuth' },
  { url: 'GKpi/GetEmployeeKpiModels', data: { year: YEAR } },
  { url: 'GKpi/GetManagerKpiModels', data: { year: YEAR } },

  // 辦公資源
  { url: 'Facility/CurrentUser' },
  { url: 'Facility/GetDevices' },

  // 議題管控
  { url: 'IssueV2/GetAllProjects' },
  { url: 'IssueV2/GetAuthorization' },

  // 票選
  { url: 'Ballot/GetAllEmployeeCandidates' },
  { url: 'Ballot/GetBallotByUserAndEvent', data: { event_name: '2025emp' } },
];

/** 一般員工不應取得的資料 */
export const ADMIN_ONLY_READS: EndpointDef[] = [
  { url: 'Home/ReviewUserDB', note: '全體員工資料 (含電話、Email)' },
  { url: 'Home/ReviewProfessionDB', note: '專業盤點資料庫' },
  { url: 'Promotion/GetAll', note: '全體員工升等資料 (含績效)' },
  { url: 'Assessment/GetAllResponses', note: '全體員工自評內容' },
];

/**
 * 會清空或大量修改資料的 API：一般員工呼叫必須被拒絕。
 * (測試站台的資料是副本，測試前後都會自動還原)
 */
export const DESTRUCTIVE_ENDPOINTS: EndpointDef[] = [
  { url: 'Forum/DeleteAll', note: '清空我要抱抱' },
  { url: 'Assessment/DeleteAll', note: '清空自評表' },
  { url: 'Ballot/DeleteAll', note: '清空票選' },
  { url: 'GEducation/DeleteAll', note: '清空全域教育訓練' },
  { url: 'GKpi/DeleteAll', note: '清空 KPI' },
  { url: 'GKpi/DeleteKpiModels', data: { year: String(YEAR) }, note: '刪除某年度 KPI' },
  { url: 'GSchedule/DeleteAll', note: '清空行事曆' },
  { url: 'GSchedule/DeleteGroupPlan', note: '刪除組規劃' },
  { url: 'GSchedule/DeletePersonalPlan', note: '刪除個人規劃' },
  { url: 'Profession/DeleteAll', note: '清空核心專業盤點' },
  { url: 'Promotion/DeleteAll', note: '清空升等資料與附件' },
  { url: 'Tasklog/DeleteAll', note: '清空工作紀錄' },
  { url: 'Talent/DeleteTalent', note: '刪除人才資料庫' },
  { url: 'Home/DeleteNotifyLog', note: '刪除通知記錄' },
  { url: 'Home/CreateMonthlyRecord', data: { yymm: ROC_YYMM }, note: '建立每月工作紀錄' },
  { url: 'Home/UpdateUser', note: '重新匯入員工資料' },
  { url: 'Home/InsertProjectItem', note: '匯入計畫項目' },
  { url: 'Home/UpdateNotify', note: '重建全體通知' },
  { url: 'Home/InsertUserExtra', data: { usersJson: '[]' }, note: '匯入組織表補充資料' },
  { url: 'GKpi/DeleteSolitaryKpiModels', note: '刪除孤立 KPI' },
  { url: 'GKpi/InsertKpiModels', note: '建立 KPI 模型' },
  { url: 'Tasklog/InsertCustomUser', note: '新增自訂使用者' },
  { url: 'Facility/CreateDevice', data: { facility: { deviceID: 'TEST-DEVICE', deviceName: '測試裝置', type: 'Teams' } }, note: '新增公用裝置' },
  { url: 'Facility/RemoveDevice', data: { deviceID: 'TEST-DEVICE' }, note: '移除公用裝置' },
  { url: 'Assessment/UpdateScoreManagers', data: { selectedManagers: [] }, note: '設定評分主管' },
];
