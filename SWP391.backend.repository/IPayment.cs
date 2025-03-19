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
        Task<int> CreatePaymentForAppointment(int appointmentId);
        Task<int> UpdatePaymentStatus(int appointmentId, string? paymentMethod);
        Task<PaymentDTOs?> GetPaymentDetailByAppointmentIdAsync(int appointmentId);
        Task<PaymentDetailDTO?> GetPaymentDetailByPaymentId(int paymentId);
        Task<List<PaymentDetailDTO>> GetPaymentsByCurrentUserAsync();
        Task<List<PaymentDetailDTO>> GetAllPayments();
    }
}
