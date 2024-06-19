using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace TEEmployee.Models
{
    public class ManageAssessmentTxtRepository : IAssessmentRepository
    {
        private string _appData = "";

        public ManageAssessmentTxtRepository()
        {
            _appData = HttpContext.Current.Server.MapPath("~/App_Data");
        }
        public Assessment Get(int Id)
        {
            string fn = Path.Combine(_appData, Id.ToString() + "ManageAssessments.txt");
            string[] fileText = File.ReadAllLines(fn);
            Assessment selfAssessment = new Assessment();

            selfAssessment.Id = Convert.ToInt32(fileText[0]);
            selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
            selfAssessment.Content = fileText[2];
            return selfAssessment;
        }
        public List<Assessment> GetAll()
        {           
            string fn = Path.Combine(_appData, "ManageResponse/ManageAssessments.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);
            List<Assessment> manageAssessments = new List<Assessment>();

            foreach (var item in lines)
            {
                string[] subs = item.Split('/');
                Assessment manageAssessment = new Assessment();

                manageAssessment.Id = Convert.ToInt32(subs[0]);
                manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                manageAssessment.Content = subs[2];
                manageAssessments.Add(manageAssessment);
            }                      

            manageAssessments = manageAssessments.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();

            return manageAssessments;
        }
        public static IEnumerable<string> EnumerateSubDirectories(string path, int depth)
        {
            if (!Directory.Exists(path)) return new List<string>();

            var result = new List<string>();
            depth--;

            foreach (var subPath in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                result.AddRange(depth == 0 ? new List<string> { subPath } : EnumerateSubDirectories(subPath, depth));
            }

            return result;
        }
        public List<User> GetScorePeople()
        {
            IUserRepository _userRepository = new UserRepository();
            List<User> users = new List<User>();
            string fn = Path.Combine(_appData, "ManageResponse", "ScorePeople.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);

            // 移除重複
            List<string> saveEmpno = new List<string>();
            foreach (string line in lines)
            {
                saveEmpno.Add(line);
            }
            saveEmpno = saveEmpno.Distinct().ToList();
            foreach (string line in saveEmpno)
            {
                User user = _userRepository.Get(line);
                if (user != null)
                {
                    //if (user.dutyName.Equals("NULL"))
                    //{
                    //    user.dutyName = "";
                    //}
                    //if (user.empno.Equals("4125"))
                    //{
                    //    user.dutyName = "協理";
                    //}

                    if (user.department_manager)
                    {
                        user.dutyName = "協理";
                    }                        
                    else if (user.group_manager && user.duty == "技術經理") {
                        user.dutyName = "技術經理";
                    }
                    else
                    {
                        user.dutyName = "";
                    }
                        
                    users.Add(user);
                }
            }

            // 依員編排序
            users = users.OrderByDescending(x => x.dutyName).ThenBy(x => x.empno).ToList();

            return users;
        }
        public List<string> GetManageYearList(string userId)
        {
            List<string> years = new List<string>();

            string dn = Path.Combine(_appData, "ManageResponse");
            var dirs = System.IO.Directory.GetDirectories(dn);

            // 讀取有資料的年份
            foreach (var dir in dirs)
            {
                if (Directory.Exists(Path.Combine(dn, $"{dir}")))
                {
                    years.Add((new DirectoryInfo(dir)).Name);
                }
            }
            // 沒有舊有資料則新增當年度的資料夾
            if (!years.Contains(Utilities.DayStr()))
            {
                years.Add(Utilities.DayStr());
            }

            return years.OrderByDescending(a => a).ToList();
        }
        /// <summary>
        /// 取得員工評核主管各年季度的回覆
        /// </summary>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GetStateOfResponse(string year, string manager, string user)
        {
            if (String.IsNullOrEmpty(year))
            {
                year = Utilities.DayStr();
            }
            string state = "";
            try
            {
                string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
                string line = File.ReadLines(fn).FirstOrDefault();
                if (line != "")
                    state = line.Split(';')[0];
            }
            catch
            {

            }

            return state;
        }
        public List<Assessment> GetResponse(string manager)
        {
            List<Assessment> manageResponse = new List<Assessment>();

            return manageResponse;
        }
        // 0912 unescaped break line
        public List<Assessment> GetResponse(string year, string manager, string user)
        {
            if (String.IsNullOrEmpty(year))
            {
                year = Utilities.DayStr();
            }
            List<Assessment> manageResponse = new List<Assessment>();
            try
            {
                string dirPath = Path.Combine(_appData, $"ManageResponse/{year}/{manager}");
                //if (Directory.Exists(dirPath) == false)
                //{
                //    Directory.CreateDirectory(dirPath);
                //}
                string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
                if (!File.Exists(fn))
                {
                    fn = Path.Combine(_appData, $"ManageResponse/ManageAssessments.txt");
                }
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    try
                    {
                        string[] subs = item.Split('/');
                        Assessment manageAssessment = new Assessment();

                        manageAssessment.Id = Convert.ToInt32(subs[0]);
                        manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                        manageAssessment.Content = subs[2];
                        
                        string unescapedValue = subs[3].Replace("\\n", "\n");

                        // replace slash
                        if (manageAssessment.Content == "建議")
                            unescapedValue = SaveSlash(unescapedValue);

                        manageAssessment.Choice = unescapedValue;
                        //manageAssessment.Choice = subs[3];

                        manageResponse.Add(manageAssessment);
                    }
                    catch
                    {

                    }
                }
                manageResponse = manageResponse.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch
            {

            }

            return manageResponse;
        }
        //deprecated
        public List<Assessment> GetAllResponses()
        {
            string fn = Path.Combine(_appData, "ManageResponse");
            var responseList = System.IO.Directory.GetFiles(fn, "*.txt").OrderBy(p => System.IO.Path.GetFileName(p)).ToList();

            List<Assessment> allResponses = new List<Assessment>();

            foreach (var response in responseList)
            {
                string[] lines = System.IO.File.ReadAllLines(response);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment manageAssessment = new Assessment();

                    manageAssessment.Id = Convert.ToInt32(subs[0]);
                    manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    manageAssessment.Content = subs[2];
                    manageAssessment.Choice = subs[3];
                    allResponses.Add(manageAssessment);
                }
            }
            return allResponses;
        }
        /// <summary>
        /// 建立主管回覆
        /// 0912: escaped break line \n
        /// </summary>
        /// <param name="assessments"></param>
        /// <param name="state"></param>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Update(List<Assessment> assessments, string state, string year, string manager, string user)
        {
            string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
            bool ret = false;
            try
            {
                List<string> responses = new List<string>();

                responses.Add($"{state};{DateTime.Now}");

                foreach (var item in assessments)
                {
                    // replace slash
                    if (item.Content == "建議")
                        item.Choice = SaveSlash(item.Choice);

                    // 0912: escaped break line \n
                    string original = $"{item.Id}/{item.CategoryId}/{item.Content}/{item.Choice}";
                    string escapedValue = original.Replace("\n", "\\n");
                    responses.Add(escapedValue);
                }

                (new FileInfo(fn)).Directory.Create();
                System.IO.File.WriteAllLines(fn, responses);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        public bool Update(List<Assessment> assessments, string user)
        {
            throw new NotImplementedException();
        }

        //==================
        // chart
        //==================
        public List<string> GetChartYearList()
        {
            List<string> years = new List<string>();
            string dn = Path.Combine(_appData, "ManageResponse");
            var dirs = System.IO.Directory.GetDirectories(dn);

            foreach (var dir in dirs)
                years.Add((new DirectoryInfo(dir)).Name);

            return years.OrderByDescending(x => x).ToList();
        }
        // 0826
        public List<Assessment> GetAllManagerAssessmentResponses(string manno, string year)
        {
            
            List<Assessment> allResponses = new List<Assessment>();

            string dir = Path.Combine(_appData, $"ManageResponse/{year}/{manno}");

            try
            {
                var fnList = System.IO.Directory.GetFiles(dir, "*.txt").OrderBy(x => System.IO.Path.GetFileName(x)).ToList();

                foreach (var fn in fnList)
                {

                    var lines = File.ReadAllLines(fn);

                    if (lines.FirstOrDefault().Split(';').FirstOrDefault() != "sent")
                        continue;

                    List<Assessment> responses = new List<Assessment>();

                    for (int i = 1; i != lines.Length; i++)
                    {
                        string[] subs = lines[i].Split('/');

                        Assessment manageAssessment = new Assessment();

                        manageAssessment.Id = Convert.ToInt32(subs[0]);
                        manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                        manageAssessment.Content = subs[2];

                        string unescapedValue = subs[3].Replace("\\n", "\n");

                        // replace slash
                        if (manageAssessment.Content == "建議")
                            unescapedValue = SaveSlash(unescapedValue);

                        manageAssessment.Choice = unescapedValue;
                        //manageAssessment.Choice = subs[3];                    

                        allResponses.Add(manageAssessment);
                    }

                }
            }
            catch
            {

            }
            

            return allResponses;
        }
        // 0826
        // Get target managers id through directory names 
        public List<string> GetChartManagers(string year)
        {
            List<string> managers = new List<string>();
            string dn = Path.Combine(_appData, $"ManageResponse/{year}");

            // Update: 240112 - Create Diretory if not exist
            if (!Directory.Exists(dn))
            {
                Directory.CreateDirectory(dn);
            }
            // End of update

            var dirs = System.IO.Directory.GetDirectories(dn);

            

            foreach (var dir in dirs)
                managers.Add((new DirectoryInfo(dir)).Name);
            
            // empno
            return managers.OrderByDescending(x => x).ToList();            
        }
        // 0914  Update Score Manager List
        public bool UpdateScoreManagers(List<User> selectedManagers)
        {
            bool ret = false;

            string fn = Path.Combine(_appData, "ManageResponse", "ScorePeople.txt");
            try
            {
                List<string> empnos = selectedManagers.Select(x => x.empno).ToList();
                System.IO.File.WriteAllLines(fn, empnos);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }

        public List<Assessment> GetAllByYear(string year)
        {
            string fn = Path.Combine(_appData, $"ManageResponse/ManageAssessments{year}.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);
            List<Assessment> manageAssessments = new List<Assessment>();

            foreach (var item in lines)
            {
                string[] subs = item.Split('/');
                Assessment manageAssessment = new Assessment();

                manageAssessment.Id = Convert.ToInt32(subs[0]);
                manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                manageAssessment.Content = subs[2];
                manageAssessments.Add(manageAssessment);
            }

            manageAssessments = manageAssessments.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();

            return manageAssessments;
        }

        private string SaveSlash(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (s.Contains("/"))
            {
                s = s.Replace("/", "slash");
            }
            else if (s.Contains("slash"))
            {
                s = s.Replace("slash", "/");
            }

            return s;
        }

        public bool DeleteAll()
        {
            string dn = Path.Combine(_appData, $"ManageResponse");

            try
            {
                if (Directory.Exists(dn))
                {
                    var directories = Directory.GetDirectories(dn);
                    foreach (var directory in directories)
                    {
                        Directory.Delete(directory, true); // true to delete subdirectories and files
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public void Dispose()
        {
            return;
        }
    }
}