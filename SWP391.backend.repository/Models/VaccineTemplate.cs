using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class VaccineTemplate
    {
        public int Id { get; set; }
        public int? DiseaseId { get; set; }
        public string? Description { get; set; }
        public int? Month { get; set; }
        public string? AgeRange { get; set; }
        public int? DoseNumber { get; set; }
        public string? Notes { get; set; }

        public virtual Disease? Disease { get; set; }
    }
}
