using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccinationDetail
{
    public class CreateVaccinationDetailDTO
    {
        public int ChildrenId { get; set; }
        public int DiseaseId { get; set; }
        public int VaccineId { get; set; }
        public string month { get; set; }
    }
}
