using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Payment
    {
        public Payment()
        {
            PaymentDetails = new HashSet<PaymentDetail>();
        }

        public int Id { get; set; }
        public int? AppointmentId { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? InjectionProcessStatus { get; set; }
        public string? TransactionId { get; set; }

        public virtual Appointment? Appointment { get; set; }
        public virtual ICollection<PaymentDetail> PaymentDetails { get; set; }
    }
}
