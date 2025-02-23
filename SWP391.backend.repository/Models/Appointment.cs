using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Appointment
    {
        public Appointment()
        {
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public int? ChildrenId { get; set; }
        public int? VaccinePackageId { get; set; }
        public int? DoctorId { get; set; }
        public int? VaccineId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? RoomId { get; set; }
        public DateTime? DateInjection { get; set; }

        public virtual Child? Children { get; set; }
        public virtual Room? Room { get; set; }
        public virtual Vaccine? Vaccine { get; set; }
        public virtual VaccinePackage? VaccinePackage { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
