using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace TEEmployee.Models.Facility
{
    public class FacilityService
    {
        private IFacilityRepository _facilityRepository;

        public FacilityService()
        {
            _facilityRepository = new FacilityRepository();
        }

        /// <summary>
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        public List<Facility> GetDevices()
        {
            List<Facility> ret = _facilityRepository.GetDevices();
            return ret;
        }
    }
}