using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Education2024
{
    public class EducationService
    {
        private IEducationRepository _educationRepository;
        private IUserRepository _userRepository;

        public EducationService()
        {
            _educationRepository = new EducationRepository();
            _userRepository = new UserRepository();
        }
        
        public List<Content> GetAllContents()
        {
            var ret = _educationRepository.GetAllContents();
            return ret;
        }
        
        public void Dispose()
        {
            _educationRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}