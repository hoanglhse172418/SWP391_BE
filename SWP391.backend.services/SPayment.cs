using Microsoft.EntityFrameworkCore;
using SWP391.backend.repository;
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
                PaymentMethod = "Pending",
                PaymentStatus = "Unpaid",
                InjectionProcessStatus = "Not Started"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Phương thức tính giá tiền cho lịch hẹn
        private decimal CalculateTotalPrice(Appointment appointment)
        {
            if (appointment.VaccinePackageId.HasValue)
            {
                return appointment.VaccinePackage?.TotalPrice ?? 0;
            }
            return decimal.Parse(appointment.Vaccine.Price);
        }
    }
}
