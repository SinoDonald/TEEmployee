using System;

namespace TEEmployee.Models.Facility
{
    public class Facility
    {
        public Int64 id { get; set; }
        public String type { get; set; }

        public String empno { get; set; }

        public String name { get; set; }

        public String contactTel { get; set; }

        public String startTime { get; set; }

        public String endTime { get; set; }

        public String meetingDate { get; set; }

        public String modifiedDate { get; set; }

        public String modifiedUser { get; set; }

        public int num { get; set; }

        public String deviceID { get; set; }

        public String deviceName { get; set; }

        public String title { get; set; }
        public bool available { get; set; }

        public String start { get; set; }

        public String end { get; set; }

        public bool allDay { get; set; }
    }
}