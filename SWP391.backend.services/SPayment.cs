using Microsoft.EntityFrameworkCore;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Payment;
using SWP391.backend.repository.Models;
using SWP391.backend.repository.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SPayment : IPayment
    {
        private readonly swpContext _context;
        public SPayment(swpContext context)
        {
            _context = context;
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

            //Xử lý cho vắc xin lẻ
            if (appointment.Type == "Single")
            {
                // Tạo payment cho lịch hẹn vắc xin lẻ
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
                    PricePerDose = decimal.Parse(appointment.Vaccine.Price),
                };
                _context.PaymentDetails.Add(paymentDetail);
                await _context.SaveChangesAsync();

                payment.TotalPrice = paymentDetail.PricePerDose * paymentDetail.DoseNumber;
                await _context.SaveChangesAsync();

                // Cập nhật PaymentId cho lịch hẹn
                appointment.PaymentId = payment.Id;
                await _context.SaveChangesAsync();

                return 1; //Lẻ
            }

            else
            {
                // Kiểm tra xem có Payment nào cho cùng gói & đứa trẻ này không
                var existingPaymentId = await _context.Appointments
                    .Where(a => a.VaccinePackageId == appointment.VaccinePackageId
                                && a.ChildrenId == appointment.ChildrenId
                                && a.PaymentId != null)
                    .Select(a => a.PaymentId)
                    .FirstOrDefaultAsync();

                int paymentId;


                //Nếu đã tồn tại Payment, trả về false
                if (existingPaymentId.HasValue) return 2; //Đã tồn tại payment, chỉ cần cập nhật status của appointment


                // Nếu chưa có Payment, tạo mới
                var payment = new Payment
                {
                    PaymentStatus = PaymentStatusEnum.NotPaid,
                    PackageProcessStatus = "NotComplete"
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                paymentId = payment.Id;

                var packageItemDetail = new List<PaymentDetail>();
                decimal? totalPrice = 0;
                foreach (var vpi in appointment.VaccinePackage.VaccinePackageItems)
                {
                    var paymentDetail = new PaymentDetail
                    {
                        PaymentId = paymentId,
                        VaccineId = vpi.VaccineId,
                        DoseNumber = vpi.DoseNumber,
                        DoseRemaining = vpi.DoseNumber,
                        PricePerDose = vpi.PricePerDose
                    };
                    totalPrice += vpi.PricePerDose * vpi.DoseNumber;
                    packageItemDetail.Add(paymentDetail);
                }
                _context.PaymentDetails.AddRange(packageItemDetail);
                await _context.SaveChangesAsync();

                payment.TotalPrice = totalPrice;
                await _context.SaveChangesAsync();

                // Gán PaymentId cho tất cả lịch hẹn cùng gói & đứa trẻ này
                var relatedAppointments = await _context.Appointments
                    .Where(a => a.VaccinePackageId == appointment.VaccinePackageId
                                && a.ChildrenId == appointment.ChildrenId)
                    .ToListAsync();

                foreach (var app in relatedAppointments)
                {
                    app.PaymentId = paymentId;
                }
                await _context.SaveChangesAsync();

                return 3;
            }         
        }

        //Bước 3 -> 4
        public async Task<int> UpdatePaymentStatus(int appointmentId, string? paymentMethod)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Payment == null) return 0;

            if(appointment.Payment.PaymentStatus == PaymentStatusEnum.Paid)
            {
                appointment.ProcessStep = ProcessStepEnum.WaitingInject;
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

        public async Task<PaymentDetailDTO?> GetPaymentDetailByAppointmentIdAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Payment)
                .Include(a => a.Payment.PaymentDetails)
                .ThenInclude(pd => pd.Vaccine)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Payment == null) return null;

            return new PaymentDetailDTO
            {
                PaymentId = appointment.Payment.Id,
                TotalPrice = appointment.Payment.TotalPrice,
                PaymentMethod = appointment.Payment.PaymentMethod,
                PaymentStatus = appointment.Payment.PaymentStatus,
                PackageProcessStatus = appointment.Payment.PackageProcessStatus,
                Items = appointment.Payment.PaymentDetails.Select(pd => new PaymentItemDTO
                {
                    VaccineId = pd.VaccineId,
                    VaccineName = pd.Vaccine.Name,
                    DoseNumber = pd.DoseNumber,
                    DoseRemaining = pd.DoseRemaining,
                    PricePerDose = pd.PricePerDose
                }).ToList()
            };
        }

        public async Task<PaymentDetailDTO?> GetPaymentDetailByPaymentId(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentDetails)
                .ThenInclude(pd => pd.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) return null;

            return new PaymentDetailDTO
            {
                PaymentId = payment.Id,
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


    }
}
