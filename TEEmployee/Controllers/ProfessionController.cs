﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
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

        [HttpPost]
        public JsonResult UpsertScores(List<Score> scores)
        {
            var ret = _service.UpsertScores(scores, Session["empno"].ToString());
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