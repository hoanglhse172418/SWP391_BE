using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class VaccinePackage
    {
        public VaccinePackage()
        {
            Appointments = new HashSet<Appointment>();
            VaccinePackageItems = new HashSet<VaccinePackageItem>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal Fee { get; set; } = 10;

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<VaccinePackageItem> VaccinePackageItems { get; set; }
    }
}
