using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class Assessment
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public string Choice { get; set; }
    }
}