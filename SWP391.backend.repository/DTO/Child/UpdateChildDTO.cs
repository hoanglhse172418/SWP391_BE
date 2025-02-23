using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Child
{
    public class UpdateChildDTO
    {
        public string? ChildrenFullname { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? FatherFullName { get; set; }
        public string? MotherFullName { get; set; }
        public string? FatherPhoneNumber { get; set; }
        public string? MotherPhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
