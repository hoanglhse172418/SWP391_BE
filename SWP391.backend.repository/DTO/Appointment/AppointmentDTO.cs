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

    public class AppointmentDetailDTO
    {
        public int Id { get; set; }
        public string? ChildFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? VaccineType { get; set; } // "Single" hoặc "Package"
        public string? VaccineName { get; set; } // Tên vắc xin hoặc tên gói
        public DateTime DateInjection { get; set; }
        public string? Status { get; set; }
        public int? DoctorId { get; set; }
        public int? RoomId { get; set; }
    }

    public class TodayAppointmentDTO
    {
        public int Id { get; set; }
        public string ChildFullName { get; set; }
        public string VaccineType { get; set; } // "Single" hoặc "Package"
        public DateTime DateInjection { get; set; }
        public string Status { get; set; }
    }

    public class FutureAppointmentDTO
    {
        public int Id { get; set; }
        public string ChildFullName { get; set; }
        public string VaccineType { get; set; }
        public DateTime DateInjection { get; set; }
        public string Status { get; set; }
    }

    public class UpdateAppointmentDTO
    {
        public string? Status { get; set; }
        public int? DoctorId { get; set; }
        public int? RoomId { get; set; }
    }
}
