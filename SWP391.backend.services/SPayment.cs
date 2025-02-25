using Microsoft.EntityFrameworkCore;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Payment;
using SWP391.backend.repository.Models;
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

        public async Task<bool> CreatePaymentForAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinePackage)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            // Kiểm tra nếu Payment đã tồn tại
            if (await _context.Payments.AnyAsync(p => p.AppointmentId == appointmentId))
            {
                return false; // Payment đã tồn tại, không cần tạo mới
            }

            var payment = new Payment
            {
                AppointmentId = appointment.Id,
                TotalPrice = CalculateTotalPrice(appointment),
                PaymentMethod = "Cash",
                PaymentStatus = "Paid",
                InjectionProcessStatus = "Not Started"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        private decimal CalculateTotalPrice(Appointment appointment)
        {
            if (appointment.VaccinePackageId.HasValue)
            {
                return appointment.VaccinePackage?.TotalPrice ?? 0;
            }
            return decimal.Parse(appointment.Vaccine.Price);
        }

        public async Task<PaymentDetailDTO?> GetPaymentDetailAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Appointment)
                .Include(p => p.PaymentDetails)
                    .ThenInclude(pd => pd.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) return null;

            return new PaymentDetailDTO
            {
                PaymentId = payment.Id,
                AppointmentId = payment.AppointmentId ?? 0,
                DateInjection = payment.Appointment?.DateInjection ?? DateTime.MinValue,
                Status = payment.PaymentStatus ?? "Unknown",
                TotalPrice = payment.TotalPrice ?? 0,
                PaymentMethod = payment.PaymentMethod ?? "N/A",
                PaymentStatus = payment.PaymentStatus ?? "Unpaid",
                InjectionProcessStatus = payment.InjectionProcessStatus ?? "Not Started",
                Vaccines = payment.PaymentDetails.Select(pd => new VaccineDetailDTO
                {
                    VaccineName = pd.Vaccine?.Name ?? "Unknown",
                    DoseNumber = pd.DoseNumber ?? 0,
                    DoseRemaining = pd.DoseRemaining ?? 0,
                    PricePerDose = pd.PricePerDose ?? 0,
                    IsInjected = (pd.DoseRemaining ?? 0) == 0 // Nếu DoseRemaining = 0 thì đã tiêm đủ mũi
                }).ToList()
            };
        }
    }
}
