using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace TEEmployee.Models.Education2024
{
    public class Content
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string CreateTime { get; set; }
        public string DeptName { get; set; }

        // Properties from ContentExtra
        public string CourseTitle { get; set; }
        public string ContentType { get; set; }
        public string ContentCode { get; set; }
        public string ContentScope { get; set; }
        public string CourseGroup { get; set; }
        public string CourseGroupOne { get; set; }
        public bool Digitalized { get; set; }
    }
}