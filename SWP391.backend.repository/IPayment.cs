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
    }
}
