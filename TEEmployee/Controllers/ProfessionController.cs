using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.Profession;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class ProfessionController : Controller
    {
        private ProfessionService _service;

        public ProfessionController()
        {
            _service = new ProfessionService();
        }

        // GET: Profession
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Skill()
        {
            return PartialView();
        }

        public ActionResult Score()
        {
            return PartialView();
        }

        public ActionResult Chart()
        {
            return PartialView();
        }
        public ActionResult Scatter()
        {
            return PartialView();
        }
        public ActionResult Personal()
        {
            return PartialView();
        }

        /*........................  Web api  ...........................*/


        [HttpPost]
        public JsonResult GetAllSkillsByRole(string role)
        {
            var ret = _service.GetAllSkillsByRole(role, Session["empno"].ToString());

            return Json(ret);
        }

        // dynamic object serialized by newtonJsom which is different form built-in Json()
        // so pass it with content and assign the content type
        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        [HttpPost]
        public JsonResult UpsertSkills(List<Skill> skills)
        {
            var ret = _service.UpsertSkills(skills, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteSkills(List<Skill> skills)
        {
            var ret = _service.DeleteSkills(skills, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllScoresByRole(string role)
        {
            var ret = _service.GetAllScoresByRole(role, Session["empno"].ToString());
            return Json(ret);
        }

        // 20240903 update: Due to MaxJsonDeserializerMembers, pass string and deserialize in backend
        [HttpPost]
        public JsonResult UpsertScores(string scoresJson)
        {
            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(scoresJson);
            var ret = _service.UpsertScores(scores, Session["empno"].ToString());
            return Json(ret);
        }
       
        //[HttpPost]
        //public JsonResult UpsertScores(List<Score> scores)
        //{
        //    var ret = _service.UpsertScores(scores, Session["empno"].ToString());
        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult GetAll()
        {
            var ret = _service.GetAll(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetPersonal(string empno)
        {
            //var ret = _service.GetPersonal(Session["empno"].ToString());
            var ret = _service.GetPersonal(empno);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpsertPersonal(List<Personal> personals)
        {
            var ret = _service.UpsertPersonal(personals, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeletePersonal(List<Personal> personals)
        {
            var ret = _service.DeletePersonal(personals, Session["empno"].ToString());
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

        //[HttpPost]
        //public ContentResult GetAllSkills()
        //{
        //    var ret = _service.GetAllSkills();

        //    return this.Content(ret, "application/json");
        //}

        //[HttpPost]
        //public JsonResult GetAllSkills()
        //{
        //    var ret = _service.GetAllSkills();

        //    return Json(ret);
        //}

    }
}