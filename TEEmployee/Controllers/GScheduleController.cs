using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.AgentModel;
using TEEmployee.Models.GSchedule;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class GScheduleController : Controller
    {
        private GScheduleService _service;

        public GScheduleController()
        {
            _service = new GScheduleService();
        }

        // GET: Schedule
        public ActionResult Index()
        {            
            return View();
        }
        /// <summary>
        /// 群組規劃
        /// </summary>
        /// <returns></returns>
        public ActionResult GroupPlan()
        {
            return PartialView();
        }
        /// <summary>
        /// 個人規劃
        /// </summary>
        /// <returns></returns>
        public ActionResult PersonalPlan()
        {
            return PartialView();
        }
        public ActionResult Group()
        {
            return PartialView();
        }
        public ActionResult Personal()
        {
            return PartialView();
        }
        public ActionResult Future()
        {
            return PartialView();
        }
        public ActionResult Project()
        {
            return PartialView();
        }
        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAllSchedules()
        {
            // get the record base on the role
            var ret = _service.GetAllGroupSchedules(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateSchedule(Schedule schedule)
        {
            var ret = _service.UpdateSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertSchedule(Schedule schedule)
        {          
            var ret = _service.InsertSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAuthorization(string page_id = "")
        {
            var ret = _service.GetAuthorization(page_id, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteSchedule(Schedule schedule)
        {
            var ret = _service.DeleteSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateAllPercentComplete()
        {
            var ret = _service.UpdateAllPercentComplete();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllFutures()
        {
            // get the record base on the role
            var ret = _service.GetAllFutures(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllProjectSchedules()
        {
            var ret = _service.GetAllProjectSchedules(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertProjectSchedule(ProjectSchedule project)
        {
            var ret = _service.InsertProjectSchedule(project, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteProjectSchedule(ProjectSchedule project)
        {
            var ret = _service.DeleteProjectSchedule(project, Session["empno"].ToString());
            return Json(ret);
        }

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadProjectSchedule(HttpPostedFileBase file, FormCollection form)
        {
            string jsonString = form.GetValue("projectSchedule").AttemptedValue;
            ProjectSchedule projectSchedule = JsonConvert.DeserializeObject<ProjectSchedule>(jsonString);
            var ret = _service.UploadProjectSchedule(file, projectSchedule);

            return Json(ret);
        }
        /// <summary>
        /// 取得群組
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGroupList(string view)
        {
            var ret = _service.GetGroupList(view, Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 取得群組同仁
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGroupUsers(string selectedGroup)
        {
            var ret = _service.GetGroupUsers(selectedGroup, Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 讀取使用者的資訊
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get(string page_id = "")
        {
            var ret = _service.Get(page_id, Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 取得年份
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetYears(string view)
        {
            var ret = _service.GetYears(view);
            return Json(ret);
        }
        /// <summary>
        /// 上傳群組與個人規劃PDF
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadPDFFile(HttpPostedFileBase file, string view, string year, string folder)
        {
            if (file == null) return Json(new { Status = 0, Message = "No File Selected" });
            string ret = _service.UploadPDFFile(file, view, year, Session["empno"].ToString());
            NotifyService notifyService = new NotifyService();
            notifyService.UpdateDatabase(Session["empno"].ToString(), 5, "0"); // 使用者寄送後取消個人規劃通知
            // 找到使用者各群組的主管們, 通知他們回饋個人規劃
            List<User> userManagers = notifyService.UserManagers(Session["empno"].ToString(), "freeback");
            foreach (User userManager in userManagers) { notifyService.UpdateDatabase(userManager.empno, 6, "1"); }
            return Json(ret);
        }
        /// <summary>
        /// 更新個人規劃簡報, 搬移簡報到最近的資料夾內, 並移除錯誤的資料夾(undefined)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdatePersonalPlan()
        {
            var ret = _service.UpdatePersonalPlan();
            return Json(ret);
        }

        ///// <summary>
        ///// 上傳個人規劃PDF
        ///// </summary>
        ///// <param name="selectedGroup"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult ImportPDFFile(HttpPostedFileBase file)
        //{
        //    if (file == null) return Json(new { Status = 0, Message = "No File Selected" });
        //    var ret = _service.ImportPDFFile(file, Session["empno"].ToString());
        //    return Json(ret);
        //}
        /// <summary>
        /// 取得PDF
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPDF(string view, string year, string group, string userName)
        {
            string pdfPath = _service.GetPDF(view, year, group, userName);
            var fileBytes = _service.DownloadFile(pdfPath);
            string contentType = "application/octet-stream"; // byte
            FileContentResult result = null;
            try
            {
                result = File(fileBytes, contentType, group + ".pdf");
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        /// <summary>
        /// 尚未上傳簡報名單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult NotUploadUsers()
        {
            var ret = new NotifyService().NotUploadUsers(Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        ///  更新年度個人規劃回饋
        /// </summary>
        /// <param name="empno"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdatePersonPlanFeedback(string empno)
        {
            bool ret = false;
            new NotifyService().UpdatePersonPlanFeedback(Session["empno"].ToString(), ""); // 更新年度個人規劃回饋
            return Json(ret);
        }
        /// <summary>
        /// 取得主管回饋
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetResponse(string view, string year, string group, string name)
        {
            List<Planning> ret = _service.GetResponse(view, year, group, Session["empno"].ToString(), name);
            return Json(ret);
        }
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveResponse(string view, string year, string group, string name, List<Planning> response)
        {
            bool ret = _service.SaveResponse(view, year, group, Session["empno"].ToString(), name, response);
            new NotifyService().UpdatePersonPlanFeedback(Session["empno"].ToString(), name); // 更新年度個人規劃回饋
            return Json(ret);
        }
        /// <summary>
        /// 刪除群組規劃
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteGroupPlan()
        {
            var ret = _service.DeleteGroupPlan();
            return Json(ret);
        }
        /// <summary>
        /// 刪除個人規劃
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeletePersonalPlan()
        {
            var ret = _service.DeletePersonalPlan();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllAgents()
        {
            var ret = _service.GetAllAgents(Session["empno"].ToString());
            return Json(ret);
        }        

        [HttpPost]
        public JsonResult CreateAgent(Agent agent)
        {
            var ret = _service.InsertAgent(agent, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateAgent(Agent agent)
        {
            var ret = _service.UpdateAgent(agent, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteAgent(Agent agent)
        {
            var ret = _service.DeleteAgent(agent, Session["empno"].ToString());
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            var ret = _service.DeleteAll();
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}