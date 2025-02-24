using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.Appointment;

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


        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointmentDto = await this.a.GetAppointment(id);
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
    }
}
