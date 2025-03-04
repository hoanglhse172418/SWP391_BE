using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Payment
    {
        public Payment()
        {
            Appointments = new HashSet<Appointment>();
            PaymentDetails = new HashSet<PaymentDetail>();
        }

        public int Id { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PackageProcessStatus { get; set; }
        public string? TransactionId { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<PaymentDetail> PaymentDetails { get; set; }
    }
}
