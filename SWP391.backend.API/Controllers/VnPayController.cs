using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SWP391.backend.repository.Models;
using SWP391.backend.repository.Utilities;
using SWP391.backend.services;
using System.Threading.Tasks;

namespace SWP391.backend.api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        protected readonly swpContext context;
        private readonly IConfiguration _configuration;

        public VNPayController(IConfiguration configuration, swpContext context)
        {
            _configuration = configuration;
            this.context = context;
        }

        /// <summary>
        /// Tạo URL thanh toán cho một `Payment`
        /// </summary>
        /// <param name="PaymentId">ID của Payment</param>
        /// <returns>URL thanh toán VNPay</returns>
        [HttpGet("CreatePaymentUrl")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePaymentUrl(string PaymentId)
        {
            try
            {
                if (!int.TryParse(PaymentId, out int paymentIdInt))
                {
                    return BadRequest("ID thanh toán không hợp lệ.");
                }

                var payment = await context.Payments.FirstOrDefaultAsync(x => x.Id == paymentIdInt);
                if (payment == null)
                {
                    return NotFound("Không tồn tại ID thanh toán.");
                }

                var paymentRecord = await context.Appointments.FirstOrDefaultAsync(a => a.PaymentId == payment.Id);
                if (paymentRecord == null)
                {
                    return BadRequest("paymentId không hợp lệ.");
                }

                if (payment.TotalPrice == null)
                {
                    return BadRequest("Giá trị thanh toán không hợp lệ.");
                }

                // Get Client IP Address
                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                // Load VNPay Configurations
                string? baseUrl = _configuration["Vnpay:BaseUrl"];
                string? tmnCode = _configuration["Vnpay:TmnCode"];
                string? hashSecret = _configuration["Vnpay:HashSecret"];
                string? currCode = _configuration["Vnpay:CurrCode"];
                string? locale = _configuration["Vnpay:Locale"];
                string? returnUrl = _configuration["Vnpay:UrlReturnLocal"]; // Change to returnAzure if needed
                string? returnUrlAzure = _configuration["Vnpay:UrlReturnAzure"]; // Change to returnAzure if needed

                //if (new[] { baseUrl, tmnCode, hashSecret, currCode, locale, returnUrl }.Any(string.IsNullOrEmpty))
                //{
                //    return BadRequest("Cấu hình VNPay không hợp lệ.");
                //}
                if (new[] { baseUrl, tmnCode, hashSecret, currCode, locale, returnUrlAzure }.Any(string.IsNullOrEmpty))
                {
                    return BadRequest("Cấu hình VNPay không hợp lệ.");
                }

                // Initialize VNPay
                SVnpay pay = new SVnpay();
                pay.AddRequestData("vnp_TxnRef", payment.Id.ToString());
                pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "2.1.0");
                pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "pay");
                pay.AddRequestData("vnp_TmnCode", tmnCode);
                pay.AddRequestData("vnp_Amount", ((int)payment.TotalPrice.Value * 100).ToString());
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", currCode);
                pay.AddRequestData("vnp_IpAddr", ip);
                pay.AddRequestData("vnp_Locale", locale);
                pay.AddRequestData("vnp_OrderInfo", "Thanh toán sản phẩm thông qua hệ thống BCS");
                pay.AddRequestData("vnp_OrderType", "other");
                //local
                //pay.AddRequestData("vnp_ReturnUrl", returnUrl);

                //azureUrl
                pay.AddRequestData("vnp_ReturnUrl", returnUrlAzure);
                

                // Create Payment URL
                string paymentUrl = pay.CreateRequestUrl(baseUrl, hashSecret);
                Console.WriteLine("Payment URL: " + paymentUrl);

                // Log VNPay Request Parameters
                Console.WriteLine("Request Parameters:");
                foreach (var requestData in pay.RequestData)
                {
                    Console.WriteLine($"{requestData.Key}: {requestData.Value}");
                }

                context.Payments.Update(payment);

                if (await context.SaveChangesAsync() > 0)
                {
                    return Ok(paymentUrl);
                }
                else
                {
                    return StatusCode(500, "Lỗi trong quá trình lưu vào cơ sở dữ liệu");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("ReturnUrl")]
        public async Task<IActionResult> ReturnUrl()
        {
            // Xử lý thông tin từ query string và chuyển đổi thành string
            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var transactionId = Request.Query["vnp_TxnRef"].ToString();
            var returnUrlS = Request.Query["returnUrlS"].ToString();
            var returnUrlF = Request.Query["returnUrlF"].ToString();

            var appointment = await context.Appointments.FirstOrDefaultAsync(a => a.PaymentId.ToString() == transactionId);

            // Kiểm tra mã phản hồi và thực hiện logic cần thiết
            var payment = await context.Payments.FirstOrDefaultAsync(p => p.Id.ToString() == transactionId);
            if (payment == null)
            {
                return Content("Không tìm thấy giao dịch với mã: " + transactionId);
            }

            if (responseCode == "00")
            {
                // Thanh toán thành công
                payment.PaymentStatus = PaymentStatusEnum.Paid;
                payment.TransactionId = transactionId;
                payment.PaymentMethod = "VNPay";
                context.Payments.Update(payment);

                appointment.ProcessStep=ProcessStepEnum.WaitingInject;
                context.Appointments.Update(appointment);

                await context.SaveChangesAsync();
                return Redirect("http://localhost:5173/paymentss"); 

            }
            else
            {
                // Thanh toán thất bại
                payment.PaymentStatus = PaymentStatusEnum.NotPaid;
                context.Payments.Update(payment);

                await context.SaveChangesAsync();
                return Redirect("http://localhost:5173/paymentFaild");
            }
        }
    }
}