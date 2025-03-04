﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Payment
{
    public class PaymentDetailDTO
    {
        public int PaymentId { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PackageProcessStatus { get; set; }
        public List<PaymentItemDTO> Items { get; set; } = new();
    }

    public class PaymentItemDTO
    {
        public int? VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public int? DoseNumber { get; set; }
        public int? DoseRemaining { get; set; }
        public decimal? PricePerDose { get; set; }
    }


}
