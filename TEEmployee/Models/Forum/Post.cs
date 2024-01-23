using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Forum
{
    public class Post
    {
        public int id { get; set; }        
        public string empno { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string postDate { get; set; }
        public int count { get; set; }
    }
}