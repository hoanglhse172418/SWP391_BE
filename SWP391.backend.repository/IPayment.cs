using SWP391.backend.repository.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IPayment
    {
        Task<bool> CreatePaymentForAppointment(int appointmentId);
        Task<bool> UpdatePaymentStatusToPaid(int appointmentId);
        Task<PaymentDetailDTO?> GetPaymentDetailAsync(int appointmentId);
    }
}
