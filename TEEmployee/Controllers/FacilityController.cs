using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}