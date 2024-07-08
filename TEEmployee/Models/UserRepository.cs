using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Dapper;
using OfficeOpenXml;

namespace TEEmployee.Models
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private IDbConnection _conn;
        public UserRepository()
        {
            string userConnection = ConfigurationManager.ConnectionStrings["UserConnection"].ConnectionString;
            _conn = new SQLiteConnection(userConnection);
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        public List<User> GetAll()
        {
            List<User> ret;

            //string sql = @"select * from user order by empno";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno";
            ret = _conn.Query<User>(sql).ToList();

            // for informal employee
            List<User> customRet;
            string customSql = @"SELECT * FROM customUser AS u LEFT JOIN userExtra AS e ON u.empno = e.empno";
            customRet = _conn.Query<User>(customSql).ToList();

            ret.AddRange(customRet);

            return ret;
        }
        public List<User> GetManagers()
        {
            List<User> ret;
            string sql = @"select * from user where duty != 'NULL' order by dutyName";
            ret = _conn.Query<User>(sql).ToList();

            return ret;
        }
        public User Get(string id)
        {
            User ret;

            //string sql = @"select * from user where empno=@id";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
            ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();

            // for informal employee
            if (ret is null)
            {
                sql = @"SELECT * FROM customUser AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
                ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();
            }

            return ret;
        }

        // insert userExtra
        public bool Insert(List<User> users)
        {

            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"DELETE FROM userExtra";
                ret = _conn.Execute(sql);


                sql = @"INSERT INTO userExtra (empno, department_manager, 'group', group_manager,
                                group_one, group_one_manager, group_two, group_two_manager, group_three, group_three_manager, project_manager, projects, assistant_project_manager, custom_duty) 
                        VALUES(@empno, @department_manager, @group, @group_manager, @group_one, @group_one_manager,
                                @group_two, @group_two_manager, @group_three, @group_three_manager, @project_manager, @projects, @assistant_project_manager, @custom_duty)";

                ret = _conn.Execute(sql, users);

                tran.Commit();

                return ret > 0;

            }

            //    int ret;

            //string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
            //            VALUES(@empno, @depno, @yymm, @projno, @itemno, @workhour, @overtime)";

            //ret = _conn.Execute(sql, projectItem);

            //return ret > 0 ? true : false;


        }

        // 1206: insert "User" (Delete all first)
        public bool InsertUser(List<User> users)
        {

            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"DELETE FROM user";
                ret = _conn.Execute(sql);


                sql = @"INSERT INTO user (empno, name, gid, profTitle, duty, dutyName, tel, email) 
                        VALUES(@empno, @name, @gid, @profTitle, @duty, @dutyName, @tel, @email)";

                ret = _conn.Execute(sql, users);

                tran.Commit();

                return ret > 0;

            }

        }

        // 2023 version

        //// GET the group one name list owned by the manager
        //public List<string> GetSubGroups(string manno)
        //{
        //    List<string> groups = new List<string>();
        //    User user = this.Get(manno);

        //    // department manager or group manager

        //    if ((user.group_manager && user.group == "設計") || user.department_manager)
        //    {
        //        groups.AddRange(new List<string> { "地工組", "界面整合管理組", "BIM暨程式開發組", "智慧軌道創新小組" });
        //    }

        //    if ((user.group_manager && user.group == "規劃") || user.department_manager)
        //    {
        //        groups.AddRange(new List<string> { "土木組", "規劃組" });
        //    }

        //    if ((user.group_manager && user.group == "專管") || user.department_manager)
        //    {
        //        groups.AddRange(new List<string> { "工程管理組", "成本/契約組", "工務組" });
        //    }

        //    if (user.department_manager)
        //    {
        //        groups.AddRange(new List<string> { /*"計畫管理組",*/ "行政組" });
        //    }

        //    // sub group manager
        //    if (user.group_one_manager) groups.Add(user.group_one);
        //    if (user.group_two_manager) groups.Add(user.group_two);
        //    if (user.group_three_manager) groups.Add(user.group_three);

        //    // remove duplicates from special case
        //    groups = groups.Distinct().ToList();

        //    return groups;
        //}

        //public bool DeleteUserExtra()
        //{
        //    int ret;

        //    string sql = @"DELETE FROM userExtra";
        //    ret = _conn.Execute(sql);

        //    return ret > 0;
        //}


        // 2024 version
        // GET the group one name list owned by the manager
        public List<string> GetSubGroups(string manno)
        {
            List<string> groups = new List<string>();
            User user = this.Get(manno);
            List<User> users = this.GetAll();

            // department manager or group manager
            if (user.department_manager)
            {
                groups.AddRange(users.Select(x => x.group_one));
                groups.AddRange(users.Select(x => x.group_two));
                groups.AddRange(users.Select(x => x.group_three));
            }
            else if (user.group_manager)
            {
                groups.AddRange(users.Where(x => x.group == user.group).Select(x => x.group_one).ToList());
            }

            // sub group manager
            if (user.group_one_manager) groups.Add(user.group_one);
            if (user.group_two_manager) groups.Add(user.group_two);
            if (user.group_three_manager) groups.Add(user.group_three);

            // remove duplicates null empty
            groups = groups.Distinct().ToList();
            groups.RemoveAll(string.IsNullOrEmpty);

            return groups;
        }

        // 取得群組 <-- 培文
        public List<string> GetGroupList(string view, string empno)
        {
            List<string> ret = new List<string>();
            List<User> users = new UserRepository().GetAll();
            User user = users.Where(x => x.empno.Equals(empno)).FirstOrDefault();

            if (view.Equals("GroupPlan"))
            {
                if (user.department_manager)
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group)).Select(x => x.group).Distinct().ToList();
                    List<string> group_ones = users.Where(x => String.IsNullOrEmpty(x.group)).Where(x => !String.IsNullOrEmpty(x.group_one)).Select(x => x.group_one).Distinct().OrderByDescending(x => x).ToList();
                    foreach (string group_one in group_ones) { ret.Insert(0, group_one); }
                }
                // 一般使用者
                else
                {
                    string group = users.Where(x => x.empno.Equals(empno)).Select(x => x.group).FirstOrDefault();
                    string group_one = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_one).FirstOrDefault();
                    string group_two = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_two).FirstOrDefault();
                    string group_three = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_three).FirstOrDefault();
                    if (!String.IsNullOrEmpty(group)) { ret.Add(group); }
                    else if (!String.IsNullOrEmpty(group_one)) { ret.Add(group_one); }
                    else if (!String.IsNullOrEmpty(group_two)) { ret.Add(group_two); }
                    else if (!String.IsNullOrEmpty(group_three)) { ret.Add(group_three); }
                }
            }
            else if (view.Equals("PersonalPlan"))
            {
                if (user.department_manager) // 協理
                {
                    List<string> groups = users.Where(x => x.group != null).Select(x => x.group).Distinct().ToList();
                    foreach (string group in groups)
                    {
                        if (!String.IsNullOrEmpty(group)) { ret.Add(group); }
                    }
                    foreach (string group in groups) { AddSubGroup(users, group, ret); } // 新增子群組 
                    ret = ret.Distinct().ToList();
                }
                else if (user.group_manager) // 技術經理
                {
                    string group = users.Where(x => x.empno.Equals(empno)).Select(x => x.group).FirstOrDefault();
                    ret.Add(group);
                    AddSubGroup(users, group, ret); // 新增子群組
                    ret = ret.Where(x => x != "").Distinct().ToList();
                }
                // 組長
                else if (user.group_one_manager || user.group_two_manager || user.group_three_manager)
                {
                    string group_one = users.Where(x => x.empno.Equals(empno)).Where(x => x.group_one_manager.Equals(true)).Select(x => x.group_one).FirstOrDefault();
                    string group_two = users.Where(x => x.empno.Equals(empno)).Where(x => x.group_two_manager.Equals(true)).Select(x => x.group_two).FirstOrDefault();
                    string group_three = users.Where(x => x.empno.Equals(empno)).Where(x => x.group_three_manager.Equals(true)).Select(x => x.group_three).FirstOrDefault();
                    if (group_one != null) { ret.Add(group_one); }
                    if (group_two != null) { ret.Add(group_two); }
                    if (group_three != null) { ret.Add(group_three); }
                }
                // 一般使用者
                else
                {
                    string group = users.Where(x => x.empno.Equals(empno)).Select(x => x.group).FirstOrDefault();
                    string group_one = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_one).FirstOrDefault();
                    string group_two = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_two).FirstOrDefault();
                    string group_three = users.Where(x => x.empno.Equals(empno)).Select(x => x.group_three).FirstOrDefault();
                    if (!String.IsNullOrEmpty(group)) { ret.Add(group); }
                    else if (!String.IsNullOrEmpty(group_one)) { ret.Add(group_one); }
                    else if (!String.IsNullOrEmpty(group_two)) { ret.Add(group_two); }
                    else if (!String.IsNullOrEmpty(group_three)) { ret.Add(group_three); }
                }
            }

            return ret;
        }
        // 新增子群組
        private void AddSubGroup(List<User> users, string group, List<string> ret)
        {
            users = users.Where(x => x.group != null).Where(x => x.group.Equals(group)).ToList();
            List<string> group_one = users.Where(x => x.group != null).Where(x => x.group.Equals(group)).Where(x => !String.IsNullOrEmpty(x.group_one)).Select(x => x.group_one).Distinct().OrderBy(x => x).ToList();
            List<string> group_two = users.Where(x => x.group != null).Where(x => x.group.Equals(group)).Where(x => !String.IsNullOrEmpty(x.group_two)).Select(x => x.group_two).Distinct().OrderBy(x => x).ToList();
            List<string> group_three = users.Where(x => x.group != null).Where(x => x.group.Equals(group)).Where(x => !String.IsNullOrEmpty(x.group_three)).Select(x => x.group_three).Distinct().OrderBy(x => x).ToList();
            foreach (string item in group_three) { ret.Insert(ret.FindIndex(x => x.Equals(group)) + 1, item); }
            foreach (string item in group_two) { ret.Insert(ret.FindIndex(x => x.Equals(group)) + 1, item); }
            foreach (string item in group_one) { ret.Insert(ret.FindIndex(x => x.Equals(group)) + 1, item); }
        }

        // 取得群組同仁 <-- 培文
        public List<string> GetGroupUsers(string selectedGroup, string empno)
        {
            List<string> ret = new List<string>();
            List<User> users = new UserRepository().GetAll();
            User user = users.Where(x => x.empno.Equals(empno)).FirstOrDefault();

            if (user.department_manager) // 協理
            {
                ret = users.Where(x => !String.IsNullOrEmpty(x.group)).Where(x => x.group.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組
                if (ret.Count.Equals(0))
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group_one)).Where(x => x.group_one.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組一
                    if (ret.Count.Equals(0))
                    {
                        ret = users.Where(x => !String.IsNullOrEmpty(x.group_two)).Where(x => x.group_two.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組二
                        if (ret.Count.Equals(0))
                        {
                            ret = users.Where(x => !String.IsNullOrEmpty(x.group_three)).Where(x => x.group_three.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組三
                        }
                    }
                }
            }
            else if (user.group_manager) // 技術經理
            {
                ret = users.Where(x => !String.IsNullOrEmpty(x.group)).Where(x => x.group.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組
                if (ret.Count.Equals(0))
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group_one)).Where(x => x.group.Equals(user.group) && x.group_one.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組一
                    if (ret.Count.Equals(0))
                    {
                        ret = users.Where(x => !String.IsNullOrEmpty(x.group_two)).Where(x => x.group.Equals(user.group) && x.group_two.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組二
                        if (ret.Count.Equals(0))
                        {
                            ret = users.Where(x => !String.IsNullOrEmpty(x.group_three)).Where(x => x.group.Equals(user.group) && x.group_three.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組三
                        }
                    }
                }
            }
            else if (user.group_one_manager || user.group_two_manager || user.group_three_manager) // 組長
            {
                List<string> groupManagerNames = users.Where(x => !String.IsNullOrEmpty(x.group)).Where(x => x.group.Equals(user.group)).Where(x => x.group_manager).Select(x => x.name).ToList(); // 找到技術經理
                List<string> groupUserNames = new List<string>();
                if (user.group_one_manager && user.group_one.Equals(selectedGroup))
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group_one)).Where(x => x.group_one.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList();
                }
                else if (user.group_two_manager && user.group_two.Equals(selectedGroup))
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group_two)).Where(x => x.group_two.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList();
                }
                else if (user.group_three_manager && user.group_three.Equals(selectedGroup))
                {
                    ret = users.Where(x => !String.IsNullOrEmpty(x.group_three)).Where(x => x.group_three.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList();
                }
                foreach (string groupManagerName in groupManagerNames)
                {
                    ret.Remove(groupManagerName);
                }
            }
            else { ret.Add(user.name); } // 一般使用者

            return ret;
        }

        // 下載User.db <-- 培文
        public string DownloadUserDB()
        {
            string ret = string.Empty;
            List<User> users = new UserRepository().GetAll().OrderBy(x => x.empno).ToList();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Files"));
                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, "userDB.xlsx");
                var file = new FileInfo(filePath); // 檔案路徑
                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("userDB"); // 建立分頁                
                    int row = 1;
                    int col = 1;

                    // 標題
                    ws.Cells[row, col++].Value = "員編";
                    ws.Cells[row, col++].Value = "姓名";
                    ws.Cells[row, col++].Value = "gid";
                    ws.Cells[row, col++].Value = "職等";
                    ws.Cells[row, col++].Value = "職責";
                    ws.Cells[row, col++].Value = "職責名稱";
                    ws.Cells[row, col++].Value = "電話";
                    ws.Cells[row, col++].Value = "Email";
                    ws.Cells[row, col++].Value = "部門主管";
                    ws.Cells[row, col++].Value = "群組";
                    ws.Cells[row, col++].Value = "群組主管";
                    ws.Cells[row, col++].Value = "群組一";
                    ws.Cells[row, col++].Value = "群組一主管";
                    ws.Cells[row, col++].Value = "群組二";
                    ws.Cells[row, col++].Value = "群組二主管";
                    ws.Cells[row, col++].Value = "群組三";
                    ws.Cells[row, col++].Value = "群組三主管";
                    ws.Cells[row, col++].Value = "專案經理";
                    ws.Cells[row, col++].Value = "專案";
                    ws.Cells[row, col++].Value = "助理專案經理";

                    foreach (User user in users)
                    {
                        row++;
                        col = 1;
                        ws.Cells[row, col++].Value = user.empno;
                        ws.Cells[row, col++].Value = user.name;
                        ws.Cells[row, col++].Value = user.gid;
                        ws.Cells[row, col++].Value = user.profTitle;
                        ws.Cells[row, col++].Value = user.duty;
                        ws.Cells[row, col++].Value = user.dutyName;
                        ws.Cells[row, col++].Value = user.tel;
                        ws.Cells[row, col++].Value = user.email;
                        ws.Cells[row, col++].Value = user.department_manager;
                        ws.Cells[row, col++].Value = user.group;
                        ws.Cells[row, col++].Value = user.group_manager;
                        ws.Cells[row, col++].Value = user.group_one;
                        ws.Cells[row, col++].Value = user.group_one_manager;
                        ws.Cells[row, col++].Value = user.group_two;
                        ws.Cells[row, col++].Value = user.group_two_manager;
                        ws.Cells[row, col++].Value = user.group_three;
                        ws.Cells[row, col++].Value = user.group_three_manager;
                        ws.Cells[row, col++].Value = user.project_manager;
                        ws.Cells[row, col++].Value = user.projects;
                        ws.Cells[row, col++].Value = user.assistant_project_manager;
                    }
                    excel.SaveAs(file); // 儲存Excel
                    ret = filePath;
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message + "\n" + ex.ToString();
            }

            return ret;
        }
    }
}