using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class VaccinationProfile
    {
        public VaccinationProfile()
        {
            VaccinationDetails = new HashSet<VaccinationDetail>();
        }

        public int Id { get; set; }
        public int? ChildrenId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Child? Children { get; set; }
        public virtual ICollection<VaccinationDetail> VaccinationDetails { get; set; }
    }
}
