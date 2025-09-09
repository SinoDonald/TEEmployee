using System;

namespace TEEmployee.Models.Facility
{
    public class FacilityService
    {
        private IFacilityRepository _facilityRepository;

        public FacilityService()
        {
            _facilityRepository = new FacilityRepository();
        }
    }
}