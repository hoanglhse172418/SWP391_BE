using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccinePackageItem
{
    public class VaccinePackageItemDTO
    {
        public int? VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public int? DoseNumber { get; set; }

        public decimal? PricePerDose { get; set; }

    }
}
