using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Appointment;
using System.Security.Claims;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointment a;
        public AppointmentController(IAppointment a)
        {
            this.a = a;
        }

        [HttpPost("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDTO dto)
        {
            try
            {
                var appointmentDto = await this.a.BookAppointment(dto);
                return Ok(appointmentDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var appointmentDto = await this.a.GetAllAppointment();
                return Ok(appointmentDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var appointmentDto = await this.a.GetAppointmentByIdAsync(id);
                return Ok(appointmentDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-by-childId/{childId}")]
        public async Task<IActionResult> GetAppointmentByChildId(int childId)
        {
            try
            {
                var appointmentDto = await this.a.GetAppointmentByChildId(childId);
                return Ok(appointmentDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-appointment-today")]
        public async Task<IActionResult> GetAppointmentsToday()
        {
            try
            {
                var appointments = await this.a.GetAppointmentsToday();
                if (appointments == null || appointments.Count() == 0)
                    return NotFound();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-appointment-future")]
        public async Task<IActionResult> GetAppointmentsFuture()
        {
            try
            {
                var appointments = await this.a.GetAppointmentsFuture();
                if (appointments == null || appointments.Count() == 0)
                    return NotFound();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("customer-appointments")]
        public async Task<IActionResult> GetCustomerAppointments()
        {
            try
            {
                // Gọi Service để lấy danh sách lịch hẹn
                var appointments = await this.a.GetCustomerAppointmentsAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching customer appointments.", error = ex.Message });
            }
        }

        [HttpPut("update-status-by-staff/step-2-to-3")]
        public async Task<IActionResult> UpdateAppointmentForStaff(int id, [FromBody] EditAppointmentDetailDTO dto)
        {
            var result = await this.a.ConfirmAppointmentAsync(id, dto);
            if (!result) return BadRequest("Cannot update appointment");

            return Ok(new { message = "Appointment updated successfully" });
        }

        [HttpPut("confirm-injection-by-doctor/{appointmentId}")]
        public async Task<IActionResult> ConfirmInjection(int appointmentId)
        {
            var result = await this.a.ConfirmInjectionAsync(appointmentId);
            if (!result) return BadRequest("Cannot update appointment, not found appointment or something");

            return Ok(new { message = "Inject complete" });
        }

        [HttpPut("update-multiple-injection-dates")]
        public async Task<IActionResult> UpdateMultipleInjectionDates([FromBody] List<InjectionUpdateDTO> updates)
        {
            if (updates == null || !updates.Any())
                return BadRequest("Danh sách cập nhật không hợp lệ.");

            var result = await this.a.UpdateMultipleInjectionDatesAsync(
                updates.Select(u => (u.AppointmentId, u.NewDate)).ToList()
            );

            if (!result)
                return BadRequest("Cập nhật ngày tiêm thất bại.");

            return Ok("Cập nhật ngày tiêm thành công.");
        }

        [Authorize]
        [HttpGet("get-appointments-from-buying-package/{childId}")]
        public async Task<IActionResult> GetAppointmentsFromBuyingPackage(int childId)
        {
            var result = await this.a.GetAppointmentsFromBuyingPackageAsync(childId);
            return Ok(result);
        }

        [HttpPut("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var result = await this.a.CancelAppointmentAsync(appointmentId);
            if (!result) return BadRequest("Cannot cancel appointment, not found appointment or something");

            return Ok(new { message = "Cancelled appintment successfully !" });
        }
    }
}
