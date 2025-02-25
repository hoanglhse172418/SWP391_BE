using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Payment
{
    public class PaymentDetailDTO
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public DateTime DateInjection { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string InjectionProcessStatus { get; set; }

        public List<VaccineDetailDTO> Vaccines { get; set; }
    }

    public class VaccineDetailDTO
    {
        public string VaccineName { get; set; }
        public int DoseNumber { get; set; }
        public int DoseRemaining { get; set; }
        public decimal PricePerDose { get; set; }
        public bool IsInjected { get; set; } // Xác định mũi này đã tiêm chưa
    }

}
