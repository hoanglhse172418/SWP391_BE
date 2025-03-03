using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class PaymentDetail
    {
        public int Id { get; set; }
        public int? PaymentId { get; set; }
        public int? VaccineId { get; set; }
        public int? DoseNumber { get; set; }
        public int? DoseRemaining { get; set; }
        public decimal? PricePerDose { get; set; }
        public int? AppointmentId { get; set; }

        public virtual Appointment? Appointment { get; set; }
        public virtual Payment? Payment { get; set; }
        public virtual Vaccine? Vaccine { get; set; }
    }
}
