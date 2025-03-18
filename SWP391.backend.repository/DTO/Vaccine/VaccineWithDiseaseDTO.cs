using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Vaccine
{
    public class VaccineWithDiseaseDTO
    {
        public int VaccineId { get; set; }
        public string VaccineName { get; set; }
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; }
    }
}
