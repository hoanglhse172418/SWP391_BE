using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccineTemplate
{
    public class VaccineTemplateDTO
    {
        public int Id { get; set; }
        public int DiseaseId { get; set; }
        public string Description { get; set; }
        public int? Month { get; set; }
        public string AgeRange { get; set; }
        public int DoseNumber { get; set; }
        public string Notes { get; set; }
        public DateTime? ExpectedInjectionDate { get; set; } // Thêm ExpectedInjectionDate
    }
}
