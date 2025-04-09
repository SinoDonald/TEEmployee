using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Dapper;

namespace TEEmployee.Models
{
    public class NotifyRepository : INotifyRepository, IDisposable
    {        
        private IDbConnection _conn;

        public NotifyRepository()
        {
            string userConnection = ConfigurationManager.ConnectionStrings["UserConnection"].ConnectionString;
            _conn = new SQLiteConnection(userConnection);
        }
        public List<User> GetAll()
        {
            List<User> ret;

            //string sql = @"select * from user order by empno";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno";
            ret = _conn.Query<User>(sql).ToList();

            return ret;
        }
        public User Get(string id)
        {
            User ret;

            //string sql = @"select * from user where empno=@id";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
            ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();

            return ret;
        }
        // 首頁通知
        public List<bool> GetNotify(string season, List<User> users, string date, string empno)
        {
            _conn.Open();
            // 先檢查是否有建立過Table的檔案存在
            string _appData = HttpContext.Current.Server.MapPath("~/App_Data");
            string filePath = Path.Combine(_appData, season + ".log");
            if (!File.Exists(filePath))
            { 
                // 確認是否有userNotify資料表, 沒有則CREATE
                SQLiteConnection conn = (SQLiteConnection)_conn;
                DataTable dataTable = conn.GetSchema("Tables");
                bool tableExist = false;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (dataRow[2].ToString().Equals("userNotify"))
                    {
                        // 檢查userNotify的Columns中是否有personPlan跟planFreeback
                        string sql = @"SELECT * FROM userNotify";
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            using (var tran = _conn.BeginTransaction())
                            {
                                SQLiteCommand sqliteCmd = (SQLiteCommand)_conn.CreateCommand();
                                if (!table.Columns.Contains("personPlan"))
                                {
                                    sqliteCmd.CommandText = "ALTER TABLE userNotify ADD COLUMN personPlan INTEGER";
                                    sqliteCmd.ExecuteNonQuery();
                                }
                                if (!table.Columns.Contains("planFreeback"))
                                {
                                    sqliteCmd.CommandText = "ALTER TABLE userNotify ADD COLUMN planFreeback INTEGER";
                                    sqliteCmd.ExecuteNonQuery();
                                }
                                tran.Commit();
                            }
                        }
                        tableExist = true; break; 
                    }
                }

                users = users.OrderBy(x => x.empno).ToList();
                List<UserNotify> userNotifyList = new List<UserNotify>();
                if (tableExist == false)
                {
                    using (var tran = _conn.BeginTransaction())
                    {
                        SQLiteCommand sqliteCmd = (SQLiteCommand)_conn.CreateCommand();
                        sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS userNotify (empno TEXT, date TEXT, self INTEGER, manager_suggest INTEGER, freeback INTEGER, future INTEGER, personPlan INTEGER, planFreeback INTEGER)";
                        sqliteCmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }

                // 先確認資料庫中是否已更新當季的資料
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"SELECT * FROM userNotify WHERE date=@date";
                    userNotifyList = _conn.Query<UserNotify>(sql, new { date }).ToList();
                    tran.Commit();
                }

                // 如果還沒當年度與當季的通知資料則建立
                if (userNotifyList.Count.Equals(0))
                {
                    userNotifyList = new List<UserNotify>();

                    foreach (User user in users)
                    {
                        List<bool> bools = NotifyUpdate(season, user.empno, users); // 目前各項通知回覆狀況
                        UserNotify userNotify = new UserNotify();
                        userNotify.empno = user.empno;
                        userNotify.date = date;
                        if (bools[0] == true) userNotify.self = 1; else userNotify.self = 0; // 自我評估表
                        if (bools[1] == true) userNotify.manager_suggest = 1; else userNotify.manager_suggest = 0; // 給予主管建議表
                        if (bools[2] == true) userNotify.freeback = 1; else userNotify.freeback = 0; // 主管給予員工建議
                        if (bools[3] == true) userNotify.future = 1; else userNotify.future = 0; // 未來3年數位轉型規劃
                        if (bools[4] == true) userNotify.personPlan = 1; else userNotify.personPlan = 0; // 個人規劃
                        if (bools[5] == true) userNotify.planFreeback = 1; else userNotify.planFreeback = 0; // 個人規劃回饋

                        userNotifyList.Add(userNotify);
                    }
                    using (var tran = _conn.BeginTransaction())
                    {
                        string sql = @"DELETE FROM userNotify";
                        _conn.Execute(sql);

                        sql = @"INSERT INTO userNotify (empno, date, self, manager_suggest, freeback, future, personPlan, planFreeback)
                            VALUES(@empno, @date, @self, @manager_suggest, @freeback, @future, @personPlan, @planFreeback)";
                        _conn.Execute(sql, userNotifyList);

                        tran.Commit();
                    }
                }
                // 建立log檔
                using (FileStream fs = File.Create(filePath))
                {

                }
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    DateTime dateTime = DateTime.Now;
                    sw.WriteLine(dateTime + " 建檔成功。");
                }
            }
            _conn.Close();

            // 從資料庫抓取使用者需通知的項目
            List<bool> ret = UserNotifyState(empno);

            return ret;
        }
        // 目前各項通知回覆狀況
        public List<bool> NotifyUpdate(string season, string empno, List<User> users)
        {
            string _appData = HttpContext.Current.Server.MapPath("~/App_Data");
            List<bool> bools = new List<bool>();
            bool ret = false;

            // 是否已填寫自我評估表
            string path = Path.Combine(_appData, "Response", season);
            string filePath = Path.Combine(path, empno + ".txt");
            if (!File.Exists(filePath))
            {
                ret = true;
            }
            else if (File.Exists(filePath))
            {
                string state = File.ReadAllLines(filePath)[0].Split(';')[0];
                string isRead = File.ReadAllLines(filePath)[0].Split(';')[2];
                // 未寄出需提醒
                if (!state.Equals("submit"))
                {
                    ret = true;
                }
                // 已寄出看是否有新回饋
                else
                {
                    if (isRead.Equals("unread"))
                    {
                        ret = true;
                    }
                }
            }
            bools.Add(ret);

            // 是否已填寫給予主管建議評估表, 必填協理+group_manager2位
            List<User> userManagers = UserManagers(empno, "");
            List<string> managers = userManagers.Select(x => x.empno).ToList();
            path = Path.Combine(_appData, "ManageResponse", season);
            ret = ManagerSuggest(path, managers, empno);
            bools.Add(ret);

            // 檢查是否為主管
            ret = false;
            INotifyRepository _notifyRepository = new NotifyRepository();
            User user = _notifyRepository.Get(empno);
            if (user != null)
            {
                // 檢驗有哪些group為manager
                List<string> groupManagers = new List<string>();
                if (user.group_manager.Equals(true)) { groupManagers.Add(user.group); }
                if (user.group_one_manager.Equals(true)) { groupManagers.Add(user.group_one); }
                if (user.group_two_manager.Equals(true)) { groupManagers.Add(user.group_two); }
                if (user.group_three_manager.Equals(true)) { groupManagers.Add(user.group_three); }
                // 主管才會收到通知
                if (user.department_manager.Equals(true) || user.group_manager.Equals(true) || user.group_one_manager.Equals(true) ||
                    user.group_two_manager.Equals(true) || user.group_three_manager.Equals(true))
                {
                    // 檢查有sent自評表的user
                    List<string> sentEmpnos = new List<string>();

                    // 找到同group的user
                    List<User> sameGroupUsers = new List<User>();
                    if (user.department_manager.Equals(true))
                    {
                        sameGroupUsers = users;
                    }
                    else
                    {
                        foreach (string groupManager in groupManagers)
                        {
                            foreach (User sameGroupUser in users.Where(x => x.group.Equals(groupManager) || x.group_one.Equals(groupManager) || x.group_two.Equals(groupManager) || x.group_three.Equals(groupManager)).ToList())
                            {
                                sameGroupUsers.Add(sameGroupUser);
                            }
                        }
                        sameGroupUsers = sameGroupUsers.Distinct().ToList();
                    }

                    foreach (User sameGroupUser in sameGroupUsers)
                    {
                        path = Path.Combine(_appData, "Response", season);
                        filePath = Path.Combine(path, sameGroupUser.empno + ".txt");
                        if (File.Exists(filePath))
                        {
                            string state = File.ReadAllLines(filePath)[0].Split(';')[0];
                            // user已寄出需提醒
                            if (state.Equals("submit"))
                            {
                                if (!sameGroupUser.empno.Equals(empno))
                                {
                                    sentEmpnos.Add(sameGroupUser.empno);
                                }
                            }
                        }
                    }

                    if (sentEmpnos.Count > 0)
                    {
                        // 檢查主管是否已回饋
                        path = Path.Combine(_appData, "Feedback", season);
                        foreach (string sentEmpno in sentEmpnos)
                        {
                            ret = true;
                            filePath = Path.Combine(path, sentEmpno + ".txt");
                            if (File.Exists(filePath))
                            {
                                string[] lines = File.ReadAllLines(filePath);
                                foreach (string line in lines)
                                {
                                    try
                                    {
                                        if (line.Split('\t')[1].Equals(empno))
                                        {
                                            if (line.Split('\t')[2].Equals("submit"))
                                            {
                                                ret = false;
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                            if (ret.Equals(true))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            bools.Add(ret);

            // 未來3年數位轉型規劃
            ret = false;
            if (user != null)
            {
                // 檢驗有哪些group為manager
                List<string> groupManagers = new List<string>();
                if (user.group_manager.Equals(true)) { groupManagers.Add(user.group); }
                if (user.group_one_manager.Equals(true)) { groupManagers.Add(user.group_one); }
                if (user.group_two_manager.Equals(true)) { groupManagers.Add(user.group_two); }
                if (user.group_three_manager.Equals(true)) { groupManagers.Add(user.group_three); }
                // 主管才會收到通知
                if (groupManagers.Count() > 0 || user.department_manager.Equals(true) || user.project_manager.Equals(true))
                {
                    ret = true;
                }
            }
            bools.Add(ret);

            // 年度個人規劃
            ret = false;
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            string thisYear = DateTime.Now.ToString("yyy", culture);
            path = Path.Combine(_appData, "GSchedule", "PersonalPlan", thisYear);
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            filePath = Path.Combine(path, empno + ".pdf");
            if (File.Exists(filePath)) { ret = false; }
            else { ret = true; }
            bools.Add(ret);

            // 個人規劃回饋
            ret = false;
            bools.Add(ret);

            return bools;
        }
        // 從資料庫抓取使用者需通知的項目
        public List<bool> UserNotifyState(string empno)
        {
            List<bool> ret = new List<bool>();
            
            _conn.Open();
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM userNotify WHERE empno=@empno";
                UserNotify userNotify = _conn.Query<UserNotify>(sql, new { empno }).SingleOrDefault();
                if (userNotify != null)
                {
                    if (userNotify.self == 1) ret.Add(true); else ret.Add(false); // 自我評估表
                    if (userNotify.manager_suggest == 1) ret.Add(true); else ret.Add(false); // 給予主管建議表
                    if (userNotify.freeback == 1) ret.Add(true); else ret.Add(false); // 主管給予員工建議
                    if (userNotify.future == 1) ret.Add(true); else ret.Add(false); // 未來3年數位轉型規劃
                    if (userNotify.personPlan == 1) ret.Add(true); else ret.Add(false); // 個人規劃
                    if (userNotify.planFreeback == 1) ret.Add(true); else ret.Add(false); // 個人規劃回饋
                }

                tran.Commit();
            }
            _conn.Close();

            return  ret;
        }
        // 找到使用者各群組的主管們
        public List<User> UserManagers(string empno, string state)
        {
            List<List<User>> managerList = new List<List<User>>();
            List<User> users = GetAll();
            User user = Get(empno);
            managerList.Add(users.Where(x => x.department_manager.Equals(true)).ToList()); // 協理
            users = users.Where(x => x.group != null && x.group_one != null && x.group_two != null && x.group_three != null).ToList();
            managerList.Add(users.Where(x => x.group.Equals(user.group) && x.group_manager.Equals(true)).ToList());
            if (state.Equals("freeback"))
            {
                managerList.Add(users.Where(x => x.group_one.Equals(user.group_one) && x.group_one_manager.Equals(true)).ToList());
                managerList.Add(users.Where(x => x.group_two.Equals(user.group_two) && x.group_two_manager.Equals(true)).ToList());
                managerList.Add(users.Where(x => x.group_three.Equals(user.group_three) && x.group_three_manager.Equals(true)).ToList());
            }
            List<User> userManagers = new List<User>(); // 使用者的主管們
            foreach(List<User> managers in managerList)
            {
                foreach(User manager in managers)
                {
                    bool isExist = userManagers.Where(x => x.empno.Equals(manager.empno)).Any();
                    if(isExist == false)
                    {
                        userManagers.Add(manager);
                    }
                }
            }
            userManagers = userManagers.OrderBy(x => x.empno).ToList();

            return userManagers;
        }
        // 給予主管建議表
        public bool ManagerSuggest(string path, List<string> managers, string empno)
        {
            bool ret = false;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (string manager in managers)
            {
                string filePath = Path.Combine(path, manager, empno + ".txt");
                if (File.Exists(filePath))
                {
                    string state = File.ReadAllLines(filePath)[0].Split(';')[0];
                    // user尚未寄出需提醒
                    if (!state.Equals("sent"))
                    {
                        ret = true;
                        break;
                    }
                }
                else
                {
                    ret = true;
                    break;
                }
            }
            ////只要填寫一位主管問卷即可, ret預設值修改為true
            //string[] directories = Directory.GetDirectories(path);
            //foreach (string directory in directories)
            //{
            //    filePath = Path.Combine(directory, empno + ".txt");
            //    if (File.Exists(filePath))
            //    {
            //        string state = File.ReadAllLines(filePath)[0].Split(';')[0];
            //        // user已寄出不需提醒
            //        if (state.Equals("sent"))
            //        {
            //            ret = false;
            //            break;
            //        }
            //    }
            //}

            return ret;
        }
        // 更新資料庫
        public bool UpdateDatabase(string empno, int count, string notification)
        {
            bool ret = false;

            _conn.Open();
            // 先確認資料庫中是否已更新當季的資料
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM userNotify WHERE empno=@empno";
                UserNotify userNotify = _conn.Query<UserNotify>(sql, new { empno }).SingleOrDefault();

                if (notification == "0")
                {
                    if (count.Equals(1)) sql = @"UPDATE userNotify SET self=0 WHERE empno=@empno"; // 自我評估表
                    else if (count.Equals(2)) sql = @"UPDATE userNotify SET manager_suggest=0 WHERE empno=@empno"; // 給予主管建議表
                    else if (count.Equals(3)) sql = @"UPDATE userNotify SET freeback=0 WHERE empno=@empno"; // 主管給予員工建議
                    else if (count.Equals(4)) sql = @"UPDATE userNotify SET future=0 WHERE empno=@empno"; // 未來3年數位轉型規劃
                    else if (count.Equals(5)) sql = @"UPDATE userNotify SET personPlan=0 WHERE empno=@empno"; // 個人規劃
                    else if (count.Equals(6)) sql = @"UPDATE userNotify SET planFreeback=0 WHERE empno=@empno"; // 個人規劃回饋
                }
                else
                {
                    if (count.Equals(1)) sql = @"UPDATE userNotify SET self=1 WHERE empno=@empno"; // 自我評估表
                    else if (count.Equals(2)) sql = @"UPDATE userNotify SET manager_suggest=1 WHERE empno=@empno"; // 給予主管建議表
                    else if (count.Equals(3)) sql = @"UPDATE userNotify SET freeback=1 WHERE empno=@empno"; // 主管給予員工建議
                    else if (count.Equals(4)) sql = @"UPDATE userNotify SET future=1 WHERE empno=@empno"; // 未來3年數位轉型規劃
                    else if (count.Equals(5)) sql = @"UPDATE userNotify SET personPlan=1 WHERE empno=@empno"; // 個人規劃
                    else if (count.Equals(6)) sql = @"UPDATE userNotify SET planFreeback=1 WHERE empno=@empno"; // 個人規劃回饋
                }

                _conn.Execute(sql, userNotify, tran);
                tran.Commit();
                ret = true; // 更新成功
            }

            _conn.Close();

            return ret;
        }
        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }
    }
}