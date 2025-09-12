using DocumentFormat.OpenXml.Office2010.Excel;
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
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool ret = _facilityRepository.Delete(id);
            return ret;
        }
        /// <summary>
        /// 修改與新增
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reserve"></param>
        /// <returns></returns>
        public bool Send(string state, Facility reserve)
        {
            bool ret = _facilityRepository.Send(state, reserve);
            return ret;
        }
    }
}