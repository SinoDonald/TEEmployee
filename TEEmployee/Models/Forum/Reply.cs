using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Forum
{
    public class Reply
    {
        public int id { get; set; }
        public int postId { get; set; }
        public string empno { get; set; }
        public string name { get; set; }
        public string replyContent { get; set; }
        public string replyDate { get; set; }
    }
}