using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TEEmployee.Models.Forum;
using TEEmployee.Models.Talent;
using TEEmployee.Models.Training;

namespace TEEmployee.Models.Wish
{
    public class WishService : IDisposable
    {
        private IWishRepository _wishRepository;
        private IUserRepository _userRepository;

        public WishService()
        {
            _wishRepository = new WishRepository();
            _userRepository = new UserRepository();
        }

        public List<WishDto> GetAll(string empno)
        {
            var users = _userRepository.GetAll();

            var ret = _wishRepository.GetAll().OrderByDescending(x => x.id).Select(x => new WishDto
            {
                wish = x,
                name = users.FirstOrDefault(t => t.empno == x.empno)?.name,
                applicationDate = x.applicationDate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return ret;
        }

        //public bool InsertWish(Wish wish, string empno)
        //{
        //    wish.empno = empno;
        //    wish.applicationDate = DateTime.Now;            
        //    var ret = _wishRepository.InsertWish(wish);

        //    return ret;
        //}

        public bool InsertWish(HttpPostedFileBase file, Wish wish, string empno)
        {
            try
            {
                wish.empno = empno;
                wish.applicationDate = DateTime.Now;

                if (file != null)
                {
                    string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Wish");

                    if (!Directory.Exists(_uploadFolder))
                        Directory.CreateDirectory(_uploadFolder);

                    wish.filepath = file.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(_uploadFolder, fileName + extension);

                    while (File.Exists(fullPath))
                    {
                        string randomSuffix = "_" + Guid.NewGuid().ToString("N").Substring(0, 6); // 6-char random string
                        wish.filepath = fileName + randomSuffix + extension;
                        fullPath = Path.Combine(_uploadFolder, fileName + randomSuffix + extension);
                    }

                    file.SaveAs(fullPath);
                }

                var ret = _wishRepository.InsertWish(wish);
                return ret;
            }
            catch
            {
                return false;
            }

        }



        

        public User GetAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);

            return user;
        }

        public bool UpdateWishStatus(Wish wish, string empno)
        {
            if (empno != "8477") return false;

            var ret = _wishRepository.UpdateWishStatus(wish);
            return ret;
        }

        public byte[] DownloadFile(Wish wish)
        {
            string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Wish");
            string fn = Path.Combine(_uploadFolder, wish.filepath);

            try
            {
                var fileBytes = File.ReadAllBytes(fn);
                return fileBytes;
            }
            catch
            {
                return null;
            }

        }

        public class WishDto
        {
            public Wish wish { get; set; }
            public string applicationDate { get; set; }
            public string name { get; set; }
        }


        public void Dispose()
        {
            _wishRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}