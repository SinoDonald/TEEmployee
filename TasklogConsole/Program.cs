using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEEmployee.Models.TaskLog;

namespace TasklogConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            List<ProjectItem> projectItems = new List<ProjectItem>();

            using (TasklogService service = new TasklogService(false))
            {
                projectItems = service.GetAllProjectItem();
            }

            using (TasklogService service = new TasklogService())
            {
                foreach (var item in projectItems)
                {
                    MonthlyRecord monthlyRecord = new MonthlyRecord()
                    {
                        empno = item.empno,
                        yymm = item.yymm,
                        guid = Guid.NewGuid()
                    };

                    service.UpsertProjectItem(item);
                    service.UpsertMonthlyRecord(monthlyRecord);
                }

            }


            //using (TasklogService service = new TasklogService())
            //{

            //    //ProjectItem projectItem = new ProjectItem() { 
            //    //    empno = "7596",
            //    //    itemno = "961",
            //    //    projno = "5049Z",
            //    //    yymm = "11109",
            //    //    workHour = 252,
            //    //    depno = "24"
            //    //};



            //    //var ret = service.UpsertProjectItem(projectItem);

            //    //if (ret)
            //    //    Console.WriteLine($"Succeed!");

            //    //MonthlyRecord monthlyRecord = new MonthlyRecord()
            //    //{
            //    //    empno = "7597",
            //    //    yymm = "11109",
            //    //    guid = Guid.NewGuid()
            //    //};

            //    //Console.WriteLine(monthlyRecord.guid);

            //    var record = service.GetAllMonthlyRecord();

            //    foreach (var item in record)
            //    {
            //        Console.WriteLine(item.guid);
            //    }

            //    Console.WriteLine("wow");

            //    //var ret = service.UpsertMonthlyRecord(monthlyRecord);

            //    //if (ret)
            //    //    Console.WriteLine($"Succeed!");




            //    //try
            //    //{
            //    //    var ret = service.InsertProjectItem(projectItem);

            //    //    if (ret)
            //    //        Console.WriteLine($"Succeed!");
            //    //}

            //    //catch
            //    //{

            //    //        var ret = service.UpdateProjectItem(projectItem);
            //    //        Console.WriteLine($"Update!");

            //    //}


            //}

        }
    }
}
