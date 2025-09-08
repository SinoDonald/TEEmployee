using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TEEmployee.Models.Facility;

namespace TEEmployee.Controllers
{
    public class FacilityController : Controller
    {
        // GET: Facility
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 部門公用電腦借用
        /// </summary>
        /// <returns></returns>
        public ActionResult Borrow()
        {
            return View();
        }

        // -----------------------------------------
        // Web API
        // -----------------------------------------

        /// <summary>
        /// 取得裝置行事曆
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEvents(DateTime start, DateTime end)
        {
            var viewModel = new Facility();
            var events = new List<Facility>();
            start = DateTime.Today.AddDays(-14);
            end = DateTime.Today.AddDays(-11);

            for (var i = 1; i <= 5; i++)
            {
                events.Add(new Facility()
                {
                    id = i,
                    title = "Event " + i,
                    start = start.ToString(),
                    end = end.ToString(),
                    allDay = false
                });

                start = start.AddDays(7);
                end = end.AddDays(7);
            }

            return Json(events.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}