using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class ResponseTxtRepository : IResponseRepository
    {
        private string _appData = "";

        public ResponseTxtRepository()
        {
            _appData = HttpContext.Current.Server.MapPath("~/App_Data/Response");
        }

        public Response Get(int Id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Response response)
        {
            string fn = Path.Combine(_appData, response.Id.ToString() + ".txt");
            
            bool ret = false;
            try
            {
                System.IO.File.WriteAllLines(fn, response.choices);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        public void Dispose()
        {
            return;
        }
    }
}