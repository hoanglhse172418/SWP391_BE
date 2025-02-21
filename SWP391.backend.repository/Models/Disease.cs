using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Disease
    {
        public Disease()
        {
            VaccinationDetails = new HashSet<VaccinationDetail>();
            VaccineTemplates = new HashSet<VaccineTemplate>();
            Vaccines = new HashSet<Vaccine>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<VaccinationDetail> VaccinationDetails { get; set; }
        public virtual ICollection<VaccineTemplate> VaccineTemplates { get; set; }

        public virtual ICollection<Vaccine> Vaccines { get; set; }
    }
}
