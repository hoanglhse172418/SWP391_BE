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
                .ThenInclude(a => a.VaccinePackageItems)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            var payment = new Payment
            {
                AppointmentId = appointment.Id,
                TotalPrice = CalculateTotalPrice(appointment),
                PaymentMethod = "Cash",
                PaymentStatus = "Not paid",
                InjectionProcessStatus = "Not Started"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();


            if(appointment.Type == "Single")
            {
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
            }
            else
            {
                var packageItemDetail = new List<PaymentDetail>();

                Console.WriteLine(appointment.VaccinePackage.VaccinePackageItems);
                foreach(VaccinePackageItem vpi in appointment.VaccinePackage.VaccinePackageItems)
                {

                    var paymentDetail = new PaymentDetail
                    {
                        PaymentId = payment.Id,
                        VaccineId = vpi.VaccineId,
                        DoseNumber= vpi.DoseNumber,
                        DoseRemaining = vpi.DoseNumber,
                        PricePerDose = vpi.PricePerDose
                    };
                    packageItemDetail.Add(paymentDetail);
                }
                _context.PaymentDetails.AddRange(packageItemDetail);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> UpdatePaymentStatusToPaid(int appointmentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

            if (payment == null)
                return false; // Không tìm thấy Payment

            if (payment.PaymentStatus == "Paid")
                return false; // Trạng thái đã là Paid, không cần cập nhật

            payment.PaymentStatus = "Paid";

            var a = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if(a == null) return false;
            a.ProcessStep = "Waiting Inject";

            _context.Appointments.Update(a);
            _context.Payments.Update(payment);
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

        public async Task<PaymentDetailDTO?> GetPaymentDetailAsync(int appointmentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Vaccine) // Vaccine đơn lẻ
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.VaccinePackage) // Gói vắc xin
                        .ThenInclude(vp => vp.VaccinePackageItems) // Các mục trong gói
                            .ThenInclude(vpi => vpi.Vaccine) // Vắc xin trong gói
                .Include(p => p.PaymentDetails)
                    .ThenInclude(pd => pd.Vaccine) // Chi tiết thanh toán liên kết với vắc xin
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

            if (payment == null)
                return null;

            var appointment = payment.Appointment;
            if (appointment == null)
                throw new ArgumentException("Không tìm thấy lịch hẹn tương ứng với Payment.");

            var vaccines = new List<VaccineDetailDTO>();

            // Nếu lịch hẹn có một loại vắc xin đơn lẻ
            if (appointment.VaccineId.HasValue && appointment.Vaccine != null)
            {
                int doseInjected = _context.PaymentDetails
                    .Count(pd => pd.VaccineId == appointment.VaccineId && pd.PaymentId == payment.Id);
              
                vaccines.Add(new VaccineDetailDTO
                {
                    VaccineName = appointment.Vaccine.Name,
                    DoseNumber = 1, // Vắc xin lẻ chỉ có một liều
                    DoseRemaining = 1 - doseInjected,
                    PricePerDose = appointment.Vaccine.Price != null ? decimal.Parse(appointment.Vaccine.Price) : 0,
                    IsInjected = doseInjected > 0
                });
            }

            // Nếu lịch hẹn có gói vắc xin
            if (appointment.VaccinePackageId.HasValue && appointment.VaccinePackage != null)
            {
                foreach (var item in appointment.VaccinePackage.VaccinePackageItems)
                {
                    var vaccine = item.Vaccine;
                    if (vaccine == null) continue; // Tránh lỗi nếu Vaccine bị null

                    int doseInjected = _context.PaymentDetails
                        .Count(pd => pd.VaccineId == vaccine.Id && pd.PaymentId == payment.Id);

                    vaccines.Add(new VaccineDetailDTO
                    {
                        VaccineName = vaccine.Name,
                        DoseNumber = item.DoseNumber, // Số liều của vắc xin trong gói
                        DoseRemaining = item.DoseNumber - doseInjected,
                        PricePerDose = item.PricePerDose,
                        IsInjected = doseInjected > 0
                    });
                }
            }

            return new PaymentDetailDTO
            {
                PaymentId = payment.Id,
                AppointmentId = appointment.Id,
                DateInjection = appointment.DateInjection,
                TotalPrice = payment.TotalPrice ?? 0,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                InjectionProcessStatus = payment.InjectionProcessStatus,
                Vaccines = vaccines
            };
        }
    }
}
