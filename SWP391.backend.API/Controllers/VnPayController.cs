using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SWP391.backend.repository.Models;
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
                // Chuyển đổi PaymentId sang int
                if (!int.TryParse(PaymentId, out int paymentIdInt))
                {
                    return BadRequest("ID thanh toán không hợp lệ.");
                }

                // Lấy thông tin thanh toán từ cơ sở dữ liệu
                var payment = await context.Payments.FirstOrDefaultAsync(x => x.Id == paymentIdInt);
                if (payment != null)
                {
                    // Lấy địa chỉ IP của khách hàng
                    string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                    // Lấy cấu hình VNPay từ appsettings
                    string? baseUrl = _configuration["Vnpay:BaseUrl"];
                    string? tmnCode = _configuration["Vnpay:TmnCode"];
                    string? hashSecret = _configuration["Vnpay:HashSecret"];
                    string? currCode = _configuration["Vnpay:CurrCode"];
                    string? locale = _configuration["Vnpay:Locale"];
                    string? returnUrl = _configuration["Vnpay:UrlReturnLocal"];
                    string? returnAzure = _configuration["Vnpay:UrlReturnAzure"];

                    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) ||
                        string.IsNullOrEmpty(currCode) || string.IsNullOrEmpty(locale) ||
                       //local
                       //string.IsNullOrEmpty(returnUrl))

                       //azure
                       string.IsNullOrEmpty(returnAzure))
                    {
                        return BadRequest("Cấu hình VNPay không hợp lệ.");
                    }

                    SVnpay pay = new SVnpay();

                    // Thêm các thông tin yêu cầu thanh toán
                    pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? throw new ArgumentNullException("Vnpay:Version"));
                    pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? throw new ArgumentNullException("Vnpay:Command"));
                    pay.AddRequestData("vnp_TmnCode", tmnCode);
                    pay.AddRequestData("vnp_Amount", ((decimal)payment.TotalPrice!.Value * 100).ToString()); // Số tiền cần thanh toán
                    pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    pay.AddRequestData("vnp_CurrCode", currCode);
                    pay.AddRequestData("vnp_IpAddr", ip);
                    pay.AddRequestData("vnp_Locale", locale);
                    pay.AddRequestData("vnp_OrderInfo", "Thanh toán sản phẩm thông qua hệ thống BCS");
                    pay.AddRequestData("vnp_OrderType", "other");
                    //localUrl
                    pay.AddRequestData("vnp_ReturnUrl", returnUrl);

                    //azureUrl
                    //pay.AddRequestData("vnp_ReturnUrl", returnAzure);

                    // Tạo mã giao dịch cho VNPay
                    string transactionCode = (DateTime.Now.Ticks % int.MaxValue).ToString();
                    pay.AddRequestData("vnp_TxnRef", transactionCode); // Mã hóa đơn

                    // Ensure the AppointmentId is valid
                    //var appointment = await context.Appointments.FirstOrDefaultAsync(a => a.Id == payment.AppointmentId);
                    //if (appointment == null)
                    //{
                    //    return BadRequest("AppointmentId không hợp lệ.");
                    //}

                    // Tạo URL thanh toán VNPay
                    string paymentUrl = pay.CreateRequestUrl(baseUrl, hashSecret);
                    Console.WriteLine("Payment URL: " + paymentUrl); // Log link thanh toán để kiểm tra

                    // Ghi lại các tham số đã gửi đến VNPay
                    Console.WriteLine("Request Parameters: ");
                    foreach (var requestData in pay.RequestData)
                    {
                        Console.WriteLine($"{requestData.Key}: {requestData.Value}");
                    }

                    context.Payments.Update(payment);

                    // Lưu thông tin vào cơ sở dữ liệu
                    if (await context.SaveChangesAsync() > 0)
                    {
                        return Ok(paymentUrl);
                    }
                    else
                    {
                        throw new Exception("Lỗi trong quá trình lưu vào cơ sở dữ liệu");
                    }
                }
                else
                {
                    throw new Exception("Không tồn tại ID thanh toán.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ReturnUrl")]
        public async Task<IActionResult> ReturnUrl()
        {
            // Xử lý thông tin từ query string và chuyển đổi thành string
            //var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            //var transactionId = Request.Query["vnp_TxnRef"].ToString();

            //// Kiểm tra mã phản hồi và thực hiện logic cần thiết
            //var payment = await context.Payments.FirstOrDefaultAsync(p => p.AppointmentId.ToString() == transactionId);
            //if (payment == null)
            //{
            //    return Content("Không tìm thấy giao dịch với mã: " + transactionId);
            //}

            //if (responseCode == "00")
            //{
            //    // Thanh toán thành công
            //    payment.PaymentStatus = "Success";
            //    context.Payments.Update(payment);

            //    // Lấy Enrollment dựa trên EnrollmentId của Payment
            //    var enrollment = await context.Appointments.FirstOrDefaultAsync(e => e.Id == payment.Id);
            //    if (enrollment != null)
            //    {
            //        enrollment.Status = "Success"; // Cập nhật status của Enrollment
            //        context.Appointments.Update(enrollment);
            //    }

            //    await context.SaveChangesAsync();
            //    return Content("Thanh toán thành công. Mã giao dịch: " + transactionId);
            //}
            //else
            //{
            //    // Thanh toán thất bại
            //    payment.PaymentStatus = "Failed";
            //    context.Payments.Update(payment);

            //    // Lấy Enrollment dựa trên EnrollmentId của Payment
            //    var enrollment = await context.Appointments.FirstOrDefaultAsync(e => e.Id == payment.Id);
            //    if (enrollment != null)
            //    {
            //        enrollment.Status = "Failed"; // Cập nhật status của Enrollment
            //        context.Appointments.Update(enrollment);
            //    }

            //    await context.SaveChangesAsync();
            //    return Content("Thanh toán không thành công. Mã lỗi: " + responseCode);
            //}
            return Ok();
        }
    }
}