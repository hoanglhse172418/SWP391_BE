using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Appointment
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int? ChildrenId { get; set; }
        public int? VaccinePackageId { get; set; }
        public int? DoctorId { get; set; }
        public int? VaccineId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? RoomId { get; set; }
        public DateTime? DateInjection { get; set; }
    }
}
