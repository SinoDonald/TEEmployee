using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class GEducationService
    {
        private IGEducationRepository _educationRepository;
        private IUserRepository _userRepository;

        public GEducationService()
        {
            _educationRepository = new GEducationRepository();
            _userRepository = new UserRepository();
        }

        public void Dispose()
        {
            _educationRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}