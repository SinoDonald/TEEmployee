using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
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
        public List<bool> GetNotify(List<User> users, string date, string empno)
        {
            _conn.Open();

            SQLiteConnection conn = (SQLiteConnection)_conn;
            DataTable dataTable = conn.GetSchema("Tables");
            bool tableExist = false;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (dataRow[2].ToString().Equals("userNotify")) { tableExist = true; break; }
            }

            // 先確認是否有userNotify資料表, 沒有則CREATE
            if (tableExist == false)
            {
                using (var tran = _conn.BeginTransaction())
                {
                    SQLiteCommand sqliteCmd = (SQLiteCommand)_conn.CreateCommand();
                    sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS userNotify (empno TEXT, date TEXT, self INTEGER, freeback INTEGER, manager_suggest INTEGER, future INTEGER)";
                    sqliteCmd.ExecuteNonQuery();
                    tran.Commit();
                }
            }

            List<bool> ret = new List<bool>();
            users = users.OrderBy(x => x.empno).ToList();
            List<UserNotify> userNotifyList = new List<UserNotify>();

            // 先確認資料庫中是否已更新當季的資料
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM userNotify WHERE date=@date";
                userNotifyList = _conn.Query<UserNotify>(sql, new { date }).ToList();
                tran.Commit();
            }

            // 如果還沒當年度與當季的通知資料則建立
            if(userNotifyList.Count.Equals(0))
            {
                userNotifyList = new List<UserNotify>();

                foreach (User user in users)
                {
                    List<bool> bools = NotifyUpdate(user.empno);
                    UserNotify userNotify = new UserNotify();
                    userNotify.empno = user.empno;
                    userNotify.date = date;
                    if (bools[0] == true) userNotify.self = 1; else userNotify.self = 0;
                    if (bools[1] == true) userNotify.freeback = 1; else userNotify.freeback = 0;
                    if (bools[2] == true) userNotify.manager_suggest = 1; else userNotify.manager_suggest = 0;
                    if (bools[3] == true) userNotify.future = 1; else userNotify.future = 0;

                    userNotifyList.Add(userNotify);
                }
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"DELETE FROM userNotify";
                    _conn.Execute(sql);

                    sql = @"INSERT INTO userNotify (empno, date, self, freeback, manager_suggest, future)
                            VALUES(@empno, @date, @self, @freeback, @manager_suggest, @future)";
                    _conn.Execute(sql, userNotifyList);

                    tran.Commit();
                }
            }

            // 從資料庫抓取使用者需通知的項目
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM userNotify WHERE empno=@empno";
                UserNotify userNotify = _conn.Query<UserNotify>(sql, new { empno }).SingleOrDefault();
                if (userNotify != null)
                {
                    if (userNotify.self == 1) ret.Add(true); else ret.Add(false);
                    if (userNotify.freeback == 1) ret.Add(true); else ret.Add(false);
                    if (userNotify.manager_suggest == 1) ret.Add(true); else ret.Add(false);
                    if (userNotify.future == 1) ret.Add(true); else ret.Add(false);
                }

                tran.Commit();
            }

            return ret;
        }
        public List<bool> NotifyUpdate(string empno)
        {
            string _appData = HttpContext.Current.Server.MapPath("~/App_Data");
            List<bool> bools = new List<bool>();
            bool ret = false;

            // 先確認當月為5、11月
            DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            if (month == 5 || month == 11)
            {
                string season = string.Empty;
                if (month == 5)
                    season = year + "H1";
                else
                    season = year + "H2";

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

                // 是否已填寫給予主管建議評估表
                ret = true;
                path = Path.Combine(_appData, "ManageResponse", season);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    filePath = Path.Combine(directory, empno + ".txt");
                    if (File.Exists(filePath))
                    {
                        string state = File.ReadAllLines(filePath)[0].Split(';')[0];
                        // user已寄出不需提醒
                        if (state.Equals("sent"))
                        {
                            ret = false;
                            break;
                        }
                    }
                }
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
                    if (groupManagers.Count > 0 || empno.Equals("4125"))
                    {
                        // 檢查有sent自評表的user
                        List<string> sentEmpnos = new List<string>();

                        // 找到同group的user
                        List<User> users = _notifyRepository.GetAll();
                        List<User> sameGroupUsers = new List<User>();
                        if (empno.Equals("4125"))
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
                                        if (line.Split('\t')[1].Equals(empno))
                                        {
                                            if (line.Split('\t')[2].Equals("submit"))
                                            {
                                                ret = false;
                                                break;
                                            }
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
            }

            return bools;
        }

        // 更新資料庫
        public bool UpdateDatabase(string empno, int count, string notification)
        {
            _conn.Open();
            bool ret = false;

            // 先確認資料庫中是否已更新當季的資料
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM userNotify WHERE empno=@empno";
                UserNotify userNotify = _conn.Query<UserNotify>(sql, new { empno }).SingleOrDefault();

                if(notification == "0")
                {
                    if (count.Equals(1)) sql = @"UPDATE userNotify SET self=0 WHERE empno=@empno";
                    else if (count.Equals(2)) sql = @"UPDATE userNotify SET freeback=0 WHERE empno=@empno";
                    else if (count.Equals(3)) sql = @"UPDATE userNotify SET manager_suggest=0 WHERE empno=@empno";
                    else if (count.Equals(4)) sql = @"UPDATE userNotify SET future=0 WHERE empno=@empno";
                }
                else
                {
                    if (count.Equals(1)) sql = @"UPDATE userNotify SET self=1 WHERE empno=@empno";
                    else if (count.Equals(2)) sql = @"UPDATE userNotify SET freeback=1 WHERE empno=@empno";
                    else if (count.Equals(3)) sql = @"UPDATE userNotify SET manager_suggest=1 WHERE empno=@empno";
                    else if (count.Equals(4)) sql = @"UPDATE userNotify SET future=1 WHERE empno=@empno";
                }

                _conn.Execute(sql, userNotify, tran);
                tran.Commit();
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