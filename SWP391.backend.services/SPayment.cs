using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Payment;
using SWP391.backend.repository.Models;
using SWP391.backend.repository.NewFolder;
using SWP391.backend.repository.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SWP391.backend.services
{
    public class SPayment : IPayment
    {
        private readonly swpContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SPayment(swpContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        //Tạo payment
        public async Task<int> CreatePaymentForAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .ThenInclude(a => a.VaccinePackageItems)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return 0;

            // Xử lý cho vắc xin lẻ
            if (appointment.Type == "Single")
            {
                var payment = new Payment
                {
                    PaymentStatus = PaymentStatusEnum.NotPaid,
                    PackageProcessStatus = "NotComplete"
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                var paymentDetail = new PaymentDetail
                {
                    PaymentId = payment.Id,
                    VaccineId = appointment.VaccineId,
                    DoseNumber = 1,
                    DoseRemaining = 1,
                    PricePerDose = decimal.Parse(appointment.Vaccine?.Price ?? "0"),
                };
                _context.PaymentDetails.Add(paymentDetail);
                await _context.SaveChangesAsync();

                payment.TotalPrice = paymentDetail.PricePerDose * paymentDetail.DoseNumber;
                await _context.SaveChangesAsync();

                appointment.PaymentId = payment.Id;
                await _context.SaveChangesAsync();

                return 1; // Lẻ
            }

            // Xử lý cho gói vaccine
            var existingPaymentId = await _context.Appointments
                .Where(a => a.Id == appointment.Id && a.PaymentId != null)
                .Select(a => a.PaymentId)
                .FirstOrDefaultAsync();


            if (existingPaymentId.HasValue && existingPaymentId > 0)
            {
                appointment.ProcessStep = ProcessStepEnum.WaitingInject;
                return 2; // Đã có payment, chỉ cần cập nhật status
            }

            // Tạo Payment mới nếu chưa có
            var newPayment = new Payment
            {
                PaymentStatus = PaymentStatusEnum.NotPaid,
                PackageProcessStatus = "NotComplete"
            };
            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            int paymentId = newPayment.Id;

            var packageItemDetails = new List<PaymentDetail>();
            decimal totalPrice = 0;

            foreach (var vpi in appointment.VaccinePackage.VaccinePackageItems)
            {
                if (!vpi.VaccineId.HasValue) continue; // Bỏ qua nếu VaccineId là null

                var pricePerDose = vpi.PricePerDose ?? 0;
                var doseNumber = vpi.DoseNumber ?? 1;

                var paymentDetail = new PaymentDetail
                {
                    PaymentId = paymentId,
                    VaccineId = vpi.VaccineId.Value,
                    DoseNumber = doseNumber,
                    DoseRemaining = doseNumber,
                    PricePerDose = pricePerDose
                };

                totalPrice += pricePerDose * doseNumber;
                packageItemDetails.Add(paymentDetail);
            }

            _context.PaymentDetails.AddRange(packageItemDetails);
            await _context.SaveChangesAsync();

            newPayment.TotalPrice = totalPrice;
            await _context.SaveChangesAsync();

            // Gán PaymentId cho tất cả lịch hẹn trong cùng gói
            var relatedAppointments = await _context.Appointments
                .Where(a => a.VaccinePackageId == appointment.VaccinePackageId
                            && a.ChildrenId == appointment.ChildrenId)
                .ToListAsync();

            foreach (var app in relatedAppointments)
            {
                app.PaymentId = paymentId;              
            }

            await _context.SaveChangesAsync();
            return 3; // Thành công
        }


        //Bước 3 -> 4
        public async Task<int> UpdatePaymentStatus(int appointmentId, string? paymentMethod)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Payment == null) return 0;

            if(appointment.PaymentId != null && appointment.Payment.PaymentStatus == PaymentStatusEnum.Paid)
            {
                return 1;
            }

            appointment.Payment.PaymentMethod = paymentMethod == null ? "Invalid method" : paymentMethod;
            appointment.Payment.PaymentStatus = PaymentStatusEnum.Paid;
            appointment.ProcessStep = ProcessStepEnum.WaitingInject;
            await _context.SaveChangesAsync();
            return 2;
        }

        private decimal CalculateTotalPrice(Appointment appointment)
        {
            if (appointment.VaccinePackageId.HasValue)
            {
                return appointment.VaccinePackage?.TotalPrice ?? 0;
            }
            return decimal.Parse(appointment.Vaccine.Price);
        }

        public async Task<List<PaymentDetailDTO?>> GetAllPayments()
        {
            //.Where(p => p.PaymentStatus == PaymentStatusEnum.Paid)
            var payments = await _context.Payments
                .Include(p => p.Appointments)
                .Include(p => p.PaymentDetails)
                .Select(p => new PaymentDetailDTO
                {
                    PaymentId = p.Id,
                    Type = p.Appointments.Select(a => a.Type).FirstOrDefault(),
                    TotalPrice = p.TotalPrice,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    PackageProcessStatus = p.PackageProcessStatus,
                    Items = p.PaymentDetails.Select(pd => new PaymentItemDTO
                    {
                        VaccineId = pd.VaccineId,
                        VaccineName = pd.Vaccine.Name,
                        DoseNumber = pd.DoseNumber,
                        DoseRemaining = pd.DoseRemaining,
                        PricePerDose = pd.PricePerDose
                    }).ToList()
                })
                .ToListAsync();
            return payments;
        }

        public async Task<PaymentDTOs?> GetPaymentDetailByAppointmentIdAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Payment)
                .Include(a => a.Vaccine)           
                .ThenInclude(a => a.VaccinePackageItems)   
                .ThenInclude(a => a.VaccinePackage)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Payment == null) return null;
            
            return new PaymentDTOs
            {
                PaymentId = appointment.Payment.Id,
                Type = appointment.Type,
                VaccineId = appointment.VaccineId,
                VaccineName = appointment.Vaccine?.Name,
                VaccinePackageId = appointment.VaccinePackageId,
                PackageName = appointment.VaccinePackage?.Name,
                TotalPrice = appointment.Payment.TotalPrice,
                PaymentMethod = appointment.Payment.PaymentMethod,
                PaymentStatus = appointment.Payment.PaymentStatus,
                PackageProcessStatus = appointment.Payment.PackageProcessStatus,
            };
        }

        public async Task<PaymentDetailDTO?> GetPaymentDetailByPaymentId(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Appointments)
                .Include(p => p.PaymentDetails)
                .ThenInclude(pd => pd.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) return null;

            return new PaymentDetailDTO
            {
                PaymentId = payment.Id,
                Type = payment.Appointments.Select(a => a.Type).FirstOrDefault(),
                TotalPrice = payment.TotalPrice,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                PackageProcessStatus = payment.PackageProcessStatus,
                Items = payment.PaymentDetails.Select(pd => new PaymentItemDTO
                {
                    VaccineId = pd.VaccineId,
                    VaccineName = pd.Vaccine.Name,
                    DoseNumber = pd.DoseNumber,
                    DoseRemaining = pd.DoseRemaining,
                    PricePerDose = pd.PricePerDose
                }).ToList()
            };
        }

        //Lấy hóa đơn liên quan đến user đăng nhập hiện tại
        public async Task<List<PaymentDetailDTO>> GetPaymentsByCurrentUserAsync()
        {
            var customerId = GetCurrentUserId();
            if (customerId == null) return new List<PaymentDetailDTO>();


            var payments = await _context.Payments
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Children)
                .Include(p => p.PaymentDetails)
                .ThenInclude(pd => pd.Vaccine) 
                .Where(p => p.Appointments.Any(a => a.ChildrenId != null && a.Children.UserId == customerId))
                .ToListAsync();


            var paymentDtos = payments.Select(p => new PaymentDetailDTO
            {
                PaymentId = p.Id,
                Type = p.Appointments.Select(a => a.Type).FirstOrDefault(), //Single hoặc Package
                TotalPrice = p.TotalPrice ?? 0,
                PaymentMethod = p.PaymentMethod ?? "Unknown",
                PaymentStatus = p.PaymentStatus,
                PackageProcessStatus = p.PackageProcessStatus,
                Items = p.PaymentDetails.Select(d => new PaymentItemDTO
                {
                    VaccineId = d.VaccineId,
                    VaccineName = d.Vaccine != null ? d.Vaccine.Name : "Unknown",
                    DoseNumber = d.DoseNumber ?? 0,
                    DoseRemaining = d.DoseRemaining ?? 0,
                    PricePerDose = d.PricePerDose ?? 0
                }).ToList()
            }).ToList();

            return paymentDtos;
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
