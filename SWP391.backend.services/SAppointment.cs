using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Appointment;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SAppointment : IAppointment
    {
        private readonly swpContext _context;
        private readonly IConfiguration _configuration;

        public SAppointment(swpContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AppointmentDTO> BookAppointment(CreateAppointmentDTO dto)
        {
            if (string.IsNullOrEmpty(dto.VaccineType) || (dto.VaccineType != "Single") && (dto.VaccineType != "Package"))
            {
                throw new ArgumentException($"Invalid vaccine type");
            }

            var child = _context.Children.FirstOrDefault(c => c.ChildrenFullname == dto.ChildFullName);

            var appointment = new Appointment
            {
                ChildrenId = child?.Id,
                Type = dto.VaccineType == "Single" ? "Single" : "Package",
                VaccineId = dto.VaccineType == "Single" ? dto.SelectedVaccineId : null,
                VaccinePackageId = dto.VaccineType == "Package" ? dto.SelectedVaccinePackageId : null,
                RoomId = null,
                DoctorId = null,
                DateInjection = dto.AppointmentDate,
                Status = "Booked",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Validate the selected vaccine or package exists
            if (appointment.Type == "Single" && appointment.VaccineId.HasValue)
            {
                var vaccine = await _context.Vaccines.FindAsync(appointment.VaccineId);
                if (vaccine == null)
                    throw new ArgumentException("Selected vaccine not found.");
            }
            else if (appointment.Type == "Package" && appointment.VaccinePackageId.HasValue)
            {
                var package = await _context.VaccinePackages.FindAsync(appointment.VaccinePackageId);
                if (package == null)
                    throw new ArgumentException("Selected vaccine package not found.");
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new AppointmentDTO
            {
                Id = appointment.Id,
                ChildrenId = appointment.ChildrenId,
                Type = appointment.Type,
                VaccineId = appointment.VaccineId,
                VaccinePackageId = appointment.VaccinePackageId,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId,
                DateInjection = appointment.DateInjection,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        public async Task<AppointmentDTO> GetAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found.");

            return new AppointmentDTO
            {
                Id = appointment.Id,
                ChildrenId = appointment.ChildrenId,
                Type = appointment.Type,
                VaccineId = appointment.VaccineId,
                VaccinePackageId = appointment.VaccinePackageId,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId,
                DateInjection = appointment.DateInjection,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        public async Task<List<AppointmentDTO>> GetAppointmentsToday()
        {
            var today = DateTime.UtcNow.Date;
            var appointmentList = await _context.Appointments
                .Where(a => a.DateInjection == today)
                .OrderBy(a => a.DateInjection)
                .ToListAsync();

            return appointmentList.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ChildrenId = a.ChildrenId,
                VaccinePackageId = a.VaccinePackageId,
                DoctorId = a.DoctorId,
                VaccineId = a.VaccineId,
                Type = a.Type,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                RoomId = a.RoomId,
                DateInjection = a.DateInjection
            }).ToList();
        }

        public async Task<List<AppointmentDTO>> GetAppointmentsFuture()
        {
            var today = DateTime.UtcNow.Date;
            var appointmentList = await _context.Appointments
                .Where(a => a.DateInjection > today)
                .OrderBy(a => a.DateInjection)
                .ToListAsync();

            return appointmentList.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ChildrenId = a.ChildrenId,
                VaccinePackageId = a.VaccinePackageId,
                DoctorId = a.DoctorId,
                VaccineId = a.VaccineId,
                Type = a.Type,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                RoomId = a.RoomId,
                DateInjection = a.DateInjection
            }).ToList();
        }
    }
}
