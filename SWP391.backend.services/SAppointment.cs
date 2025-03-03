using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Appointment;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SAppointment : IAppointment
    {
        private readonly swpContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SAppointment> _logger;
        private readonly IPayment _payment;

        enum Status
        {
            Pending,
            Processing,
            Completed,
            Canceled,
            Not_Injected,
            Injected
        }
        public SAppointment(swpContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<SAppointment> logger, IPayment payment)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _payment = payment;
        }

        //Create appointment
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
                Name = dto.ContactFullName,
                Phone = dto.ContactPhoneNumber,
                Type = dto.VaccineType == "Single" ? "Single" : "Package",
                DiseaseName = dto.VaccineType == "Single" ? dto.DiaseaseName : "N/A",
                VaccineId = dto.VaccineType == "Single" ? dto.SelectedVaccineId : null,
                VaccinePackageId = dto.VaccineType == "Package" ? dto.SelectedVaccinePackageId : null,
                RoomId = null,
                DoctorId = null,
                DateInjection = dto.AppointmentDate,
                Status = "Pending",
                ProcessStep = "Booked",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
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
                DiaseaseName = appointment.DiseaseName,
                VaccineId = appointment.VaccineId,
                VaccinePackageId = appointment.VaccinePackageId,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId,
                DateInjection = appointment.DateInjection,
                Status = appointment.Status,
                ProcessStep = appointment.ProcessStep,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        public async Task<AppointmentDetailDTO?> GetAppointmentByIdAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Children)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return null;

            return new AppointmentDetailDTO
            {
                Id = appointment.Id,
                ChildFullName = appointment.Children?.ChildrenFullname ?? "N/A",
                ParentPhoneNumber = appointment.Children?.MotherPhoneNumber ?? "N/A",
                VaccineType = appointment.VaccinePackageId.HasValue ? "Package" : "Single",
                VaccineName = appointment.VaccinePackageId.HasValue ? appointment.VaccinePackage.Name : appointment.Vaccine?.Name ?? "N/A",
                DateInjection = appointment.DateInjection ?? DateTime.MinValue,
                Status = appointment.Status,
                ProcessStep = appointment.ProcessStep,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId
            };
        }

        //Gọi khi từ bước 2 sang 3
        public async Task<bool> ConfirmAppointmentAsync(int appointmentId, EditAppointmentDetailDTO dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Processing";
            appointment.ProcessStep = "Confirm Info";
            appointment.Type = dto.VaccineType;
            appointment.VaccineId = dto.VaccineId;
            appointment.DoctorId = dto.DoctorId;
            appointment.RoomId = dto.RoomId;    
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var paymentCreated = await _payment.CreatePaymentForAppointment(appointmentId);
            if (!paymentCreated) return false;

            return true;
        }
        
        //Bác sĩ gọi dùng update status sang Đã tiêm
        public async Task<bool> UpdateAppointmentForDoctorAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return false;

            appointment.ProcessStep = "Injected";
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.Status = "Completed";
            return false; // Nếu trạng thái không hợp lệ, trả về false
        }
        public async Task<List<AppointmentDTO>> GetAppointmentByChildId(int Id)
        {
            var appointmentList = await _context.Appointments
                .Where(a => a.ChildrenId == Id)
                .OrderBy(a => a.DateInjection)
                .ToListAsync();

            return appointmentList.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ChildrenId = a.ChildrenId,
                VaccinePackageId = a.VaccinePackageId,
                DoctorId = a.DoctorId,
                DiaseaseName = a.DiseaseName,
                VaccineId = a.VaccineId,
                Type = a.Type,
                Status = a.Status,
                ProcessStep = a.ProcessStep,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                RoomId = a.RoomId,
                DateInjection = a.DateInjection
            }).ToList();
        }

        public async Task<List<AppointmentDTO>> GetAllAppointment()
        {
            var appointmentList = await _context.Appointments.ToListAsync();
            return appointmentList.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ChildrenId = a.ChildrenId,
                DiaseaseName = a.DiseaseName,
                VaccinePackageId = a.VaccinePackageId,
                DoctorId = a.DoctorId,
                VaccineId = a.VaccineId,
                Type = a.Type,
                Status = a.Status,
                ProcessStep = a.ProcessStep,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                RoomId = a.RoomId,
                DateInjection = a.DateInjection
            }).ToList();
        }

        public async Task<List<TodayAppointmentDTO>> GetAppointmentsToday()
        {
            var todayLocal = DateTime.UtcNow.AddHours(7).Date; // Chuyển đổi sang múi giờ địa phương
            var appointments = await _context.Appointments
                .Where(a => a.DateInjection.HasValue && a.DateInjection.Value.Date == todayLocal)
                .Include(a => a.Children)
                .ToListAsync();

            return appointments.Select(a => new TodayAppointmentDTO
            {
                Id = a.Id,
                ChildFullName = a.Children != null ? a.Children.ChildrenFullname : "N/A",
                VaccineType = a.VaccinePackageId.HasValue ? "Package" : "Single",
                DateInjection = a.DateInjection.Value,
                Status = a.Status
            }).ToList();
        }

        public async Task<List<FutureAppointmentDTO>> GetAppointmentsFuture()
        {
            var todayLocal = DateTime.UtcNow.AddHours(7).Date; // Chuyển đổi sang múi giờ địa phương

            var appointments = await _context.Appointments
                .Where(a => a.DateInjection.HasValue && a.DateInjection.Value.Date > todayLocal)
                .Include(a => a.Children)
                .ToListAsync();

            return appointments.Select(a => new FutureAppointmentDTO
            {
                Id = a.Id,
                ChildFullName = a.Children != null ? a.Children.ChildrenFullname : "N/A",
                VaccineType = a.VaccinePackageId.HasValue ? "Package" : "Single",
                DateInjection = a.DateInjection.Value,
                Status = a.Status
            }).ToList();
        }

        public async Task<CustomerAppointmentsDTO> GetCustomerAppointmentsAsync()
        {
            var customerId = GetCurrentUserId();

            var appointments = await _context.Appointments
                .Where(a => a.Children.UserId == customerId) 
                .Include(a => a.Children)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .ThenInclude(vp => vp.VaccinePackageItems)
                .ThenInclude(vpi => vpi.Vaccine)
                .ToListAsync();


            var responseDto = new CustomerAppointmentsDTO();

            //Vắc xin gói
            foreach (var appointment in appointments)
            {
                if (appointment.VaccinePackageId.HasValue) 
                {
                    var packageDto = new PackageVaccineAppointmentDTO
                    {
                        ChildFullName = appointment.Children?.ChildrenFullname ?? "N/A",
                        ContactPhoneNumber = appointment.Children?.FatherPhoneNumber ?? "N/A",
                        VaccinePackageName = appointment.VaccinePackage?.Name ?? "Unknown Package",
                        DateInjection = appointment.DateInjection ?? DateTime.MinValue,
                        AppointmentCreatedDate = appointment.CreatedAt ?? DateTime.MinValue,
                        Status = appointment.Status ?? "Unknown"
                    };

                    DateTime nextInjectionDate = appointment.DateInjection ?? DateTime.MinValue;

                    foreach (var packageItem in appointment.VaccinePackage.VaccinePackageItems)
                    {
                        for (int i = 0; i < packageItem.DoseNumber; i++)
                        {
                            packageDto.FollowUpAppointments.Add(new FollowUpAppointmentDTO
                            {
                                VaccineName = packageItem.Vaccine.Name,
                                DoseNumber = i + 1,
                                DateInjection = nextInjectionDate,
                                Status = (i == 0) ? appointment.Status : "Pending"
                            });

                            nextInjectionDate = nextInjectionDate.AddDays(30);
                        }
                    }

                    responseDto.PackageVaccineAppointments.Add(packageDto);
                }

                else // Vắc xin lẻ
                {
                    responseDto.SingleVaccineAppointments.Add(new SingleVaccineAppointmentDTO
                    {
                        ChildFullName = appointment.Children?.ChildrenFullname ?? "N/A",
                        ContactPhoneNumber = appointment.Phone ?? "N/A",
                        DiaseaseName = appointment.DiseaseName ?? "N/A",
                        VaccineName = appointment.Vaccine?.Name ?? "Unknown Vaccine",
                        DateInjection = appointment.DateInjection ?? DateTime.MinValue,
                        AppointmentCreatedDate = appointment.CreatedAt ?? DateTime.MinValue,
                        Status = appointment.Status ?? "Unknown",
                        ProcessStep = appointment.ProcessStep ?? "Unknown"
                    });
                }
            }
            return responseDto;
        }

        private int? GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("Id");
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }        
    }
}
