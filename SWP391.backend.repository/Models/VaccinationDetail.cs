using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class VaccinationDetail
    {
        public int Id { get; set; }
        public int? VaccinationProfileId { get; set; }
        public int? DiseaseId { get; set; }
        public int? VaccineId { get; set; }
        public DateTime? ExpectedInjectionDate { get; set; }
        public DateTime? ActualInjectionDate { get; set; }

        public virtual Disease? Disease { get; set; }
        public virtual VaccinationProfile? VaccinationProfile { get; set; }
        public virtual Vaccine? Vaccine { get; set; }
    }
}
