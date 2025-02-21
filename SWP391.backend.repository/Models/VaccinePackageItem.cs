using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class VaccinePackageItem
    {
        public int Id { get; set; }
        public int? VaccineId { get; set; }
        public int? VaccinePackageId { get; set; }
        public int? DoseNumber { get; set; }
        public decimal? PricePerDose { get; set; }

        public virtual Vaccine? Vaccine { get; set; }
        public virtual VaccinePackage? VaccinePackage { get; set; }
    }
}
