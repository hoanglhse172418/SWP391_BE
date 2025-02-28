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
        public async Task<IActionResult> GetPaymentDetail(int appointmentId)
        {
            var result = await this.p.GetPaymentDetailAsync(appointmentId);
            if (result == null)
            {
                return NotFound(new { message = "Payment not found" });
            }
            return Ok(result);
        }

        //Gọi khi từ bước 3 sang 4
        [HttpPut("update-status-payment-status/step-3-to-4/{appointmentId}")]
        public async Task<IActionResult> UpdatePaymentStatusToPaid(int appointmentId)
        {
            var result = await this.p.UpdatePaymentStatusToPaid(appointmentId);

            if (!result)
            {
                return BadRequest("Payment update failed. Either the appointment does not exist, or the payment is already marked as Paid.");
            }
            return Ok("Payment status updated to Paid successfully.");
        }
    }
}
