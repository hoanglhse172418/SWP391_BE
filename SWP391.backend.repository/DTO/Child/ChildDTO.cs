using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Child
{
    public class ChildDTO
    {
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
        public List<VaccinationProfileDTO> VaccinationProfiles { get; set; } = new();
    }

    public class VaccinationProfileDTO
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VaccinationDetailDTO> VaccinationDetails { get; set; } = new();
    }

    public class VaccinationDetailDTO
    {
        public int Id { get; set; }
        public int? DiseaseId { get; set; }
        public int? VaccineId { get; set; }
        public DateTime? ExpectedInjectionDate { get; set; }
        public DateTime? ActualInjectionDate { get; set; }
    }

}
