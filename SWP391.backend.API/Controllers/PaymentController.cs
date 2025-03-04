using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayment p;
        public PaymentController(IPayment p)
        {
            this.p = p;
        }


        [HttpGet("detail/{appointmentId}")]
        public async Task<IActionResult> GetPaymentDetailByAppointmentId(int appointmentId)
        {
            var result = await this.p.GetPaymentDetailByAppointmentIdAsync(appointmentId);
            if (result == null)
            {
                return NotFound(new { message = "Payment not found" });
            }
            return Ok(result);
        }

        [HttpGet("detail/{paymentId}")]
        public async Task<IActionResult> GetPaymentDetailByPaymentId(int paymentId)
        {
            var paymentDetail = await this.p.GetPaymentDetailByPaymentId(paymentId);
            if (paymentDetail == null)
            {
                return NotFound(new { message = "Payment not found" });
            }
            return Ok(paymentDetail);
        }

        //Gọi khi từ bước 3 sang 4
        [HttpPut("update-status-payment-status/step-3-to-4")]
        public async Task<IActionResult> UpdatePaymentStatus(int appointmentId, string? paymentMethod)
        {
            int result = await this.p.UpdatePaymentStatus(appointmentId, paymentMethod);

            if (result == 0) 
            {
                return NotFound("Appointment or payment not found");
            }
            if(result == 1)
            {
                return Ok("Payment was created ! Update appointment process");
            }
            return Ok("Payment status updated to Paid successfully.");
        }
    }
}
