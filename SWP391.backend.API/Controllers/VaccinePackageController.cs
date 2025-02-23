using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.VaccinePackage;
using SWP391.backend.repository.Models;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinePackageController : ControllerBase
    {
        private readonly IVaccinePackage _vp;

        public VaccinePackageController(IVaccinePackage vp)
        {
            _vp = vp;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVaccinePackage([FromBody] CreateVaccinePackageDTO request)
        {
            try
            {
                var result = await _vp.CreateVaccinePackageAsync(request);
                return CreatedAtAction(nameof(GetVaccinePackageById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateVaccinePackage([FromBody] UpdateVaccinePackageDTO request)
        {
            try
            {
                var updatedPackage = await _vp.UpdateVaccinePackageAsync(request);
                return Ok(updatedPackage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the vaccine package.", error = ex.Message });
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetVaccinePackageById(int id)
        {
            var package = await _vp.GetVaccinePackageByIdAsync(id);

            if (package == null)
                return NotFound(new { message = "Vaccine package not found." });

            return Ok(package);
        }
    }
}
