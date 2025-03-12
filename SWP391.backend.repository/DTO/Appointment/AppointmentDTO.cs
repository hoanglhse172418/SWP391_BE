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
        public string? DiaseaseName { get; set; }
        public int? VaccineId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? ProcessStep { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? RoomId { get; set; }
        public int? PaymentId { get; set; }
        public DateTime? DateInjection { get; set; }
        public string? InjectionNote { get; set; }
    }

    public class AppointmentDetailDTO
    {
        public int Id { get; set; }
        public string? ChildFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? VaccineType { get; set; } // "Single" hoặc "Package"
        public int? VaccineId { get; set; }
        public string? VaccineName { get; set; } // Tên vắc xin
        public int? VaccinePackageId { get; set; }
        public string? VaccinePackageName { get; set; } //Tên gói
        public DateTime? DateInjection { get; set; }
        public string? Status { get; set; }
        public string? ProcessStep { get; set; }
        public int? DoctorId { get; set; }
        public int? RoomId { get; set; }
        public int? PaymentId { get; set; }
        public string? InjectionNote { get; set; } //phản ứng sau tiêm
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

    public class EditAppointmentDetailDTO
    {
        public int VaccineId { get; set; }
        public int DoctorId { get; set; }
        public int RoomId { get;set; }
    }

    public class EditInjectionNoteDTO
    {
        public string? InjectionNote { get; set; }
    }
}
