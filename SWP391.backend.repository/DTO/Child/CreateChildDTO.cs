using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Child
{
    public class CreateChildDTO
    {
        public int? UserId { get; set; }
        public string ChildrenFullname { get; set; }
        public string ParentFullname { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public List<VaccinationDetailDTO> VaccinationDetails { get; set; } = new List<VaccinationDetailDTO>();
    }
}
