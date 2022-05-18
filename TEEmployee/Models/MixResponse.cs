using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class MixResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Content { get; set; }       
        public string Choice { get; set; }
        public string ManagerChoice { get; set; }
    }
}