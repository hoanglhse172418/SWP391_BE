﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Appointment;
using SWP391.backend.repository.Models;
using SWP391.backend.repository.NewFolder;
using SWP391.backend.repository.Utilities;
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

        public SAppointment(swpContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<SAppointment> logger, IPayment payment)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _payment = payment;
        }

        //Đặt lịch hẹn
        public async Task<List<AppointmentDTO>> BookAppointment(CreateAppointmentDTO dto)
        {
            if (string.IsNullOrEmpty(dto.VaccineType) || (dto.VaccineType != "Single" && dto.VaccineType != "Package"))
            {
                throw new ArgumentException("Invalid vaccine type");
            }

            var child = await _context.Children.FirstOrDefaultAsync(c => c.ChildrenFullname == dto.ChildFullName);
            if (child == null) throw new ArgumentException("Child not found.");

            var appointments = new List<Appointment>();

            if (dto.VaccineType == "Single")
            {
                // Tạo cuộc hẹn cho vắc xin đơn lẻ
                var appointment = new Appointment
                {
                    ChildrenId = child.Id,
                    Name = dto.ContactFullName,
                    Phone = dto.ContactPhoneNumber,
                    Type = "Single",
                    DiseaseName = dto.DiaseaseName,
                    VaccineId = dto.SelectedVaccineId,
                    VaccinePackageId = null,
                    RoomId = null,
                    DoctorId = null,
                    DateInjection = dto.AppointmentDate,
                    Status = AppointmentStatus.Pending,
                    ProcessStep = ProcessStepEnum.Booked,
                    PaymentId = null,
                    InjectionNote = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                appointments.Add(appointment);
            }
            else if (dto.VaccineType == "Package")
            {
                //// Lấy danh sách vắc xin trong gói
                //var vaccineList = await _context.VaccinePackageItems
                //    .Where(vp => vp.VaccinePackageId == dto.SelectedVaccinePackageId)
                //    .Select(vp => vp.VaccineId)
                //    .ToListAsync();

                //if (!vaccineList.Any()) throw new ArgumentException("Vaccine package is empty or not found.");

                //// Khởi tạo ngày tiêm bắt đầu
                //DateTime injectionDate = dto.AppointmentDate;

                //// Tạo lịch hẹn cho từng vắc xin trong gói
                //foreach (var vaccineId in vaccineList)
                //{
                //    var appointment = new Appointment
                //    {
                //        ChildrenId = child.Id,
                //        Name = dto.ContactFullName,
                //        Phone = dto.ContactPhoneNumber,
                //        Type = "Package",
                //        DiseaseName = "N/A",
                //        VaccineId = vaccineId,
                //        VaccinePackageId = dto.SelectedVaccinePackageId,
                //        RoomId = null,
                //        DoctorId = null,
                //        DateInjection = injectionDate, // Ngày tiêm của mũi hiện tại
                //        Status = AppointmentStatus.Pending,
                //        ProcessStep = ProcessStepEnum.Booked,
                //        PaymentId = null,
                //        InjectionNote = null,
                //        CreatedAt = DateTime.UtcNow,
                //        UpdatedAt = DateTime.UtcNow
                //    };

                //    appointments.Add(appointment);

                //    // Cập nhật ngày tiêm cho mũi tiếp theo (cách 30 ngày)
                //    injectionDate = injectionDate.AddDays(30);
                //}

                // Kiểm tra xem trẻ đã có bất kỳ gói vắc xin nào chưa hoàn tất không
                var hasOngoingPackage = await _context.Appointments.AnyAsync(a =>
                    a.ChildrenId == child.Id &&
                    a.Type == "Package" &&
                    a.Status != AppointmentStatus.Completed &&
                    a.Status != AppointmentStatus.Canceled
                );

                if (hasOngoingPackage)
                {
                    throw new InvalidOperationException("Trẻ hiện đang có lịch hẹn lẻ hay một gói tiêm chủng chưa hoàn tất. Vui lòng hoàn tất hoặc huỷ gói hiện tại trước khi đặt thêm.");
                }

                //Lấy tất cả vắc xin trong gói
                var vaccineIds = await _context.VaccinePackageItems
                    .Where(vp => vp.VaccinePackageId == dto.SelectedVaccinePackageId)
                    .Select(vp => vp.VaccineId)
                    .ToListAsync();

                if (!vaccineIds.Any()) throw new ArgumentException("Vaccine package is empty or not found.");

                var vaccines = await _context.Vaccines
                    .Include(v => v.Diseases)
                    .Where(v => vaccineIds.Contains(v.Id))
                    .ToListAsync();

                // Lấy profile của trẻ
                var vaccinationProfile = await _context.VaccinationProfiles
                    .Include(p => p.VaccinationDetails)
                    .FirstOrDefaultAsync(p => p.ChildrenId == child.Id);

                if (vaccinationProfile == null) throw new Exception("Vaccination profile not found for child.");

                foreach (var vaccine in vaccines)
                {
                    var disease = vaccine.Diseases.FirstOrDefault();
                    if (disease == null) continue;

                    var diseaseId = disease.Id;

                    // Lấy tất cả các mũi cần tiêm cho bệnh đó
                    var detailsForDisease = vaccinationProfile.VaccinationDetails
                        .Where(d => d.DiseaseId == diseaseId)
                        .OrderBy(d => d.ExpectedInjectionDate)
                        .ToList();

                    if (!detailsForDisease.Any()) continue;

                    // Lấy mũi chưa tiêm
                    var nextDose = detailsForDisease.FirstOrDefault(d => d.ActualInjectionDate == null);
                    if (nextDose == null) continue; // Đã tiêm hết các mũi rồi

                    var appointment = new Appointment
                    {
                        ChildrenId = child.Id,
                        Name = dto.ContactFullName,
                        Phone = dto.ContactPhoneNumber,
                        Type = "Package",
                        DiseaseName = disease.Name,
                        VaccineId = vaccine.Id,
                        VaccinePackageId = dto.SelectedVaccinePackageId,
                        RoomId = null,
                        DoctorId = null,
                        DateInjection = nextDose.ExpectedInjectionDate ?? dto.AppointmentDate,
                        Status = AppointmentStatus.Pending,
                        ProcessStep = ProcessStepEnum.Booked,
                        PaymentId = null,
                        InjectionNote = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    appointments.Add(appointment);
                }
            }

            _context.Appointments.AddRange(appointments);
            await _context.SaveChangesAsync();

            return appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ChildrenId = a.ChildrenId,
                Type = a.Type,
                DiaseaseName = a.DiseaseName,
                VaccineId = a.VaccineId,
                VaccinePackageId = a.VaccinePackageId,
                DoctorId = a.DoctorId,
                RoomId = a.RoomId,
                DateInjection = a.DateInjection,
                Status = a.Status,
                ProcessStep = a.ProcessStep,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }

        //Lấy lịch hẹn theo ID
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
                VaccineType = appointment.Type,
                VaccineId = appointment.VaccineId,
                VaccineName = appointment.VaccineId.HasValue ? appointment.Vaccine?.Name ?? "N/A" : null,
                VaccinePackageId = appointment.VaccineId.HasValue ? appointment.VaccinePackageId : null,
                VaccinePackageName = appointment.VaccinePackageId.HasValue ? appointment.VaccinePackage?.Name ?? "N/A" : null,
                DateInjection = appointment.DateInjection,
                Status = appointment.Status,
                ProcessStep = appointment.ProcessStep,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId,
                PaymentId = appointment.PaymentId,
                InjectionNote = appointment.InjectionNote
            };
        }

        //Lấy lịch hẹn theo Child ID
        public async Task<List<AppointmentDTO>> GetAppointmentByChildId(int childId)
        {
            var appointmentList = await _context.Appointments
                .Where(a => a.ChildrenId == childId)
                .OrderByDescending(a => a.DateInjection)
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
                PaymentId = a.PaymentId,
                DateInjection = a.DateInjection
            }).ToList();
        }

        //Lấy tất cá lịch hẹn
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
                PaymentId = a.PaymentId,
                DateInjection = a.DateInjection,
                InjectionNote = a.InjectionNote,
            }).ToList();
        }

        //Lấy tất cả lịch hẹn hôm nay
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

        //Lấy tất cả lịch hẹn sắp tới
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

        //Lấy tất cả lịch hẹn theo khách hàng đăng nhập
        public async Task<CustomerAppointmentsDTO> GetCustomerAppointmentsAsync()
        {
            var customerId = GetCurrentUserId();

            var appointments = await _context.Appointments
                .Where(a => a.Children.UserId == customerId)
                .Include(a => a.Children)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .ToListAsync();

            var responseDto = new CustomerAppointmentsDTO();

            // Nhóm lịch hẹn theo ChildrenId và VaccinePackageId
            var packageAppointments = appointments
                .Where(a => a.VaccinePackageId.HasValue)
                .GroupBy(a => new { a.ChildrenId, a.VaccinePackageId, a.CreatedAt })
                .ToList();

            // Xử lý lịch hẹn gói vắc xin
            foreach (var group in packageAppointments)
            {
                var firstAppointment = group.First();

                var packageDto = new PackageVaccineAppointmentDTO
                {
                    VaccinePackageId = firstAppointment.VaccinePackageId.Value,
                    VaccinePackageName = firstAppointment.VaccinePackage?.Name ?? "Unknown Package",
                    ChildrenId = firstAppointment.ChildrenId,
                    ChildFullName = firstAppointment.Children?.ChildrenFullname ?? "N/A",
                    ContactPhoneNumber = firstAppointment.Children?.FatherPhoneNumber ?? "N/A",               
                    VaccineItems = group.Select((a, index) => new VaccineItemDTO
                    {
                        Id = a.Id,
                        VaccineId = a.VaccineId,
                        VaccineName = a.Vaccine?.Name ?? "Unknown Vaccine",
                        DoseSequence = index + 1, // Đánh số mũi theo thứ tự
                        DateInjection = a.DateInjection,
                        Status = a.Status,
                        ProcessStep = a.ProcessStep,
                        PaymentId = a.PaymentId,
                        InjectionNote = a.InjectionNote
                    }).ToList()
                };

                responseDto.PackageVaccineAppointments.Add(packageDto);
            }

            // Xử lý lịch hẹn vắc xin lẻ
            responseDto.SingleVaccineAppointments = appointments
                .Where(a => !a.VaccinePackageId.HasValue)
                .Select(a => new SingleVaccineAppointmentDTO
                {
                    Id = a.Id,
                    ChildrenId = a.ChildrenId,
                    ChildFullName = a.Children?.ChildrenFullname ?? "N/A",
                    ContactPhoneNumber = a.Children?.FatherPhoneNumber ?? "N/A",
                    DiseaseName = a.DiseaseName,
                    VaccineId = a.VaccineId,
                    VaccineName = a.Vaccine?.Name ?? "Unknown Vaccine",
                    DateInjection = a.DateInjection,
                    Status = a.Status ?? "Unknown",
                    ProcessStep = a.ProcessStep,
                    PaymentId = a.PaymentId,
                    InjectionNote = a.InjectionNote
                })
                .ToList();

            return responseDto;
        }

        //Lấy lịch hẹn theo gói đang mua
        public async Task<CustomerAppointmentsDTO> GetAppointmentsFromBuyingPackageAsync(int childId)
        {
            var customerId = GetCurrentUserId();

            var appointments = await _context.Appointments
                .Where(a => a.Children.UserId == customerId && // Chỉ lấy lịch hẹn của khách hàng hiện tại
                            a.ChildrenId == childId &&        // Chỉ lấy lịch hẹn của đứa trẻ cụ thể
                            a.VaccinePackageId.HasValue &&    // Chỉ lấy lịch hẹn thuộc gói vắc xin
                            a.Status == "Pending")           // Chỉ lấy lịch hẹn có trạng thái Pending
                .Include(a => a.Children)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .ToListAsync();

            var responseDto = new CustomerAppointmentsDTO();

            // Nhóm lịch hẹn theo VaccinePackageId
            var packageAppointments = appointments
                .GroupBy(a => a.VaccinePackageId)
                .ToList();

            // Xử lý dữ liệu trả về
            foreach (var group in packageAppointments)
            {
                var firstAppointment = group.First();

                var packageDto = new PackageVaccineAppointmentDTO
                {
                    VaccinePackageId = firstAppointment.VaccinePackageId.Value,
                    VaccinePackageName = firstAppointment.VaccinePackage?.Name ?? "Unknown Package",
                    ChildrenId = firstAppointment.ChildrenId,
                    ChildFullName = firstAppointment.Children?.ChildrenFullname ?? "N/A",
                    ContactPhoneNumber = firstAppointment.Children?.FatherPhoneNumber ?? "N/A",
                    VaccineItems = group.Select((a, index) => new VaccineItemDTO
                    {
                        Id = a.Id,
                        VaccineId = a.VaccineId,
                        VaccineName = a.Vaccine?.Name ?? "Unknown Vaccine",
                        DoseSequence = index + 1, // Đánh số mũi theo thứ tự
                        DateInjection = a.DateInjection,
                        Status = a.Status,
                        ProcessStep = a.ProcessStep,
                        PaymentId = a.PaymentId
                    }).ToList()
                };

                responseDto.PackageVaccineAppointments.Add(packageDto);
            }

            return responseDto;
        }

        //Gọi khi từ bước 2 sang 3
        public async Task<int> ConfirmAppointmentAsync(int appointmentId, EditAppointmentDetailDTO dto)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return -1;

            if(appointment.PaymentId == null)
            {
                appointment.Status = AppointmentStatus.Processing;
                appointment.ProcessStep = ProcessStepEnum.ConfirmInfo;

                if (dto.VaccineId != null && dto.VaccineId > 0)
                {
                    appointment.VaccineId = dto.VaccineId;
                }

                appointment.DoctorId = dto.DoctorId;
                appointment.RoomId = dto.RoomId;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                try
                {
                    int paymentCreated = await _payment.CreatePaymentForAppointment(appointmentId);

                    return paymentCreated switch
                    {
                        0 => 0, //Không tìm thấy appointment
                        1 => 1, //Tạo payment cho Lẻ
                        2 => 2, //Đã có payment trong gói
                        3 => 3,  //Tạo payment mới cho gói
                        _ => -3 // Lỗi khi xử lý
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating payment: {ex.Message}");
                    return -2; // Lỗi khi tạo payment
                }
            }

            appointment.Status = AppointmentStatus.Processing;
            appointment.ProcessStep = ProcessStepEnum.WaitingInject;
            if (dto.VaccineId != null && dto.VaccineId > 0)
            {
                appointment.VaccineId = dto.VaccineId;
            }
            appointment.DoctorId = dto.DoctorId;
            appointment.RoomId = dto.RoomId;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return 4;
        }



        // Dùng để cập nhật ngày tiêm cho các mũi bác sĩ chọn (có thể 1 hoặc nhiều mũi)
        public async Task<bool> UpdateMultipleInjectionDatesAsync(List<(int appointmentId, DateTime newDate)> updates)
        {
            var appointmentIds = updates.Select(u => u.appointmentId).ToList();

            // Lấy danh sách các appointment cần cập nhật
            var appointments = await _context.Appointments
                .Where(a => appointmentIds.Contains(a.Id) && a.Status == "Pending")
                .ToListAsync();

            if (!appointments.Any())
                return false;

            // Tạo dictionary chứa ngày mới
            Dictionary<int, DateTime> updatedDates = updates.ToDictionary(u => u.appointmentId, u => u.newDate);

            // Cập nhật ngày tiêm cho các appointment cần chỉnh sửa
            foreach (var appointment in appointments)
            {
                if (updatedDates.TryGetValue(appointment.Id, out DateTime newDate))
                {
                    // Chỉ cho phép cập nhật nếu newDate >= DateInjection hiện tại
                    if (appointment.DateInjection.HasValue && newDate >= appointment.DateInjection.Value)
                    {
                        appointment.DateInjection = newDate;
                        appointment.UpdatedAt = DateTime.UtcNow;
                    }
                    // Nếu DateInjection chưa có (null), vẫn cho update
                    else if (!appointment.DateInjection.HasValue)
                    {
                        appointment.DateInjection = newDate;
                        appointment.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // Lấy danh sách PaymentId cần xử lý
            var paymentIds = appointments
                .Where(a => a.PaymentId.HasValue)
                .Select(a => a.PaymentId.Value)
                .Distinct()
                .ToList();

            // Lấy tất cả các lịch hẹn liên quan
            var relatedAppointments = await _context.Appointments
                .Where(a => paymentIds.Contains(a.PaymentId.Value) && a.Status == "Pending")
                .ToListAsync();

            // Cập nhật lịch tiêm cho các mũi còn lại
            foreach (var paymentId in paymentIds)
            {
                var orderedAppointments = relatedAppointments
                    .Where(a => a.PaymentId == paymentId)
                    .OrderBy(a => updatedDates.ContainsKey(a.Id) ? updatedDates[a.Id] : a.DateInjection)
                    .ToList();

                DateTime? lastUpdatedDate = null;

                foreach (var appt in orderedAppointments)
                {
                    if (updatedDates.ContainsKey(appt.Id))
                    {
                        lastUpdatedDate = updatedDates[appt.Id];
                    }
                    else if (lastUpdatedDate.HasValue)
                    {
                        lastUpdatedDate = lastUpdatedDate.Value.AddDays(30);
                        appt.DateInjection = lastUpdatedDate.Value;
                        appt.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }



        //Bác sĩ gọi dùng update status sang Đã tiêm đồng thời trừ đi DoseRemaining trong PaymentDetail để theo dõi quá trình của Payment (nếu có gói)
        public async Task<bool> ConfirmInjectionAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(p => p.Payment)
                .ThenInclude(pd => pd.PaymentDetails)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Payment == null || appointment.Payment.PaymentDetails == null)
                return false;

            // Cập nhật trạng thái tiêm
            appointment.ProcessStep = ProcessStepEnum.Injected;
            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Xác định loại vắc xin của appointment
            var vaccineDetail = appointment.Payment.PaymentDetails
                .FirstOrDefault(pd => pd.VaccineId == appointment.VaccineId && pd.DoseRemaining > 0);

            if (vaccineDetail != null)
            {
                vaccineDetail.DoseRemaining--;
            }

            // Kiểm tra xem tất cả DoseRemaining có về 0 hay chưa
            bool allDosesCompleted = appointment.Payment.PaymentDetails.All(d => d.DoseRemaining == 0);

            if (allDosesCompleted)
            {
                appointment.Payment.PackageProcessStatus = "Completed";
            }

            //Trừ số lượng của Vaccine đó ở trong kho khi đã xác nhận tiêm xong
            var injectedVaccine = _context.Vaccines.FirstOrDefault(v => v.Id == appointment.VaccineId);
            if (injectedVaccine != null) 
            {
                injectedVaccine.InStockNumber--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //Hủy lịch hẹn
        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Canceled;
            appointment.ProcessStep = ProcessStepEnum.Canceled;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        
        //Cập nhật phản ứng sau tiêm
        public async Task<bool> UpdateInjectionNoteAsync(int appointmentId, EditInjectionNoteDTO dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return false;
            appointment.InjectionNote = dto.InjectionNote;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AppointmentDTO>> GetAppointmentsByPackageAndPaymentAsync(int appointmentId)
        {
            // Tìm lịch hẹn hiện tại để lấy VaccinePackageId và PaymentId
            var appointment = await _context.Appointments
                .Where(a => a.Id == appointmentId)
                .Select(a => new { a.VaccinePackageId, a.PaymentId })
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy lịch hẹn hoặc không thuộc gói nào, trả về danh sách rỗng
            if (appointment == null || !appointment.VaccinePackageId.HasValue || !appointment.PaymentId.HasValue)
                return new List<AppointmentDTO>();

            // Lấy tất cả lịch hẹn thuộc cùng VaccinePackageId và PaymentId
            var appointments = await _context.Appointments
                .Where(a => a.VaccinePackageId == appointment.VaccinePackageId &&
                            a.PaymentId == appointment.PaymentId)
                .Select(a => new AppointmentDTO
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
                    PaymentId = a.PaymentId,
                    DateInjection = a.DateInjection,
                    InjectionNote = a.ProcessStep // Có thể thay đổi nếu cần
                })
                .ToListAsync();

            return appointments;
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
