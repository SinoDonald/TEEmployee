using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public static class Utilities
    {
        // Get the current date to specific string
        public static string DayStr()
        {
            DateTime thisDay = DateTime.Today;
            return thisDay.Year.ToString() + (thisDay.Month < 7 ? "H1" : "H2");
        }

    }
}