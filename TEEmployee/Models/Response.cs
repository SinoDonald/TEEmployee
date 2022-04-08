using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class Response
    {
        public int Id { get; set; }
        public List<string> choices { get; set; }
    }
}