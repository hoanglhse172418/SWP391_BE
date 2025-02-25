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

        [HttpGet("detail/{paymentId}")]
        public async Task<IActionResult> GetPaymentDetail(int paymentId)
        {
            var result = await this.p.GetPaymentDetailAsync(paymentId);
            if (result == null)
            {
                return NotFound(new { message = "Payment not found" });
            }
            return Ok(result);
        }
    }
}
