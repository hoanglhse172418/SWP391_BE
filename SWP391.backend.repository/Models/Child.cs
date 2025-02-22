using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Child
    {
        public Child()
        {
            Appointments = new HashSet<Appointment>();
            VaccinationProfiles = new HashSet<VaccinationProfile>();
        }

        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? ChildrenFullname { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? FatherFullName { get; set; }
        public string? MotherFullName { get; set; }
        public string? FatherPhoneNumber { get; set; }
        public string? MotherPhoneNumber { get; set; }
        public string? Address { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<VaccinationProfile> VaccinationProfiles { get; set; }
    }
}
