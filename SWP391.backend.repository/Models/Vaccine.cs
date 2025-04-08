using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Vaccine
    {
        public Vaccine()
        {
            Appointments = new HashSet<Appointment>();
            PaymentDetails = new HashSet<PaymentDetail>();
            VaccinationDetails = new HashSet<VaccinationDetail>();
            VaccinePackageItems = new HashSet<VaccinePackageItem>();
            Diseases = new HashSet<Disease>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Manufacture { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? RecAgeStart { get; set; }
        public int? RecAgeEnd { get; set; }
        public int? InStockNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Price { get; set; }
        public bool IsDelete { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<PaymentDetail> PaymentDetails { get; set; }
        public virtual ICollection<VaccinationDetail> VaccinationDetails { get; set; }
        public virtual ICollection<VaccinePackageItem> VaccinePackageItems { get; set; }

        public virtual ICollection<Disease> Diseases { get; set; }
    }
}
