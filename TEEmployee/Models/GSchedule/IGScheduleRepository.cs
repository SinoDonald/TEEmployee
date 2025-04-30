using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TEEmployee.Models.GSchedule
{
    interface IGScheduleRepository
    {
        List<Schedule> GetAll();
        List<Schedule> GetAllGroupSchedules(string role);
        Schedule Update(Schedule schedule);
        bool Update(List<Schedule> schedules);
        Schedule Insert(Schedule schedule);
        bool Delete(List<Schedule> schedules);
        List<ProjectSchedule> GetAllProjectSchedules();
        bool Insert(ProjectSchedule projectSchedule);
        bool Update(ProjectSchedule projectSchedule);
        bool Delete(ProjectSchedule projectSchedule);
        bool DeleteAll();
        /// <summary>
        /// 取得年份
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        List<string> GetYears(string view);
        /// <summary>
        /// 上傳群組與個人規劃PDF
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        string UploadPDFFile(HttpPostedFileBase file, string view, string year, string empno);
        string UpdatePersonalPlan();
        ///// <summary>
        ///// 上傳個人規劃PDF
        ///// </summary>
        ///// <param name="view"></param>
        ///// <returns></returns>
        //string ImportPDFFile(HttpPostedFileBase file, string empno);
        /// <summary>
        /// 取得PDF
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        string GetPDF(string view, string year, string group, string userName);
        /// <summary>
        /// 取得主管回饋
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        List<Planning> GetResponse(string view, string year, string group, string empno, string name);
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        bool SaveResponse(string view, string year, string group, string manager_id, string name, List<Planning> response);
        /// <summary>
        /// 刪除群組規劃
        /// </summary>
        /// <returns></returns>
        bool DeleteGroupPlan();
        /// <summary>
        /// 刪除個人規劃
        /// </summary>
        /// <returns></returns>
        bool DeletePersonalPlan();
        void Dispose();
    }
}
