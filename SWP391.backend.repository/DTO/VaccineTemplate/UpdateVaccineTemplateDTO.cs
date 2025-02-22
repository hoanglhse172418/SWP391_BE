using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccineTemplate
{
    public class UpdateVaccineTemplateDTO
    {
        public int DiseaseId { get; set; }
        public string? Description { get; set; }
        public int Month { get; set; }
        public string AgeRange { get; set; }
        public int DoseNumber { get; set; }
        public string Notes { get; set; }
    }
}
