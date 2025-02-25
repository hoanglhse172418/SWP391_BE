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

        [HttpPut("update-appointment")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO dto)
        {
            var result = await this.a.UpdateAppointmentAsync(id, dto.Status, dto.DoctorId, dto.RoomId);
            if (!result) return BadRequest("Cannot update appointment");

            return Ok(new { message = "Appointment updated successfully" });
        }
    }
}
