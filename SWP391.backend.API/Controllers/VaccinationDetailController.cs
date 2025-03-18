using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccinationDetail;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationDetailController : ControllerBase
    {
        private readonly IVaccinationDetail vd;

        public VaccinationDetailController(IVaccinationDetail vd)
        {
            this.vd = vd;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            var vaccinationDetails = await vd.GetAll(request);
            if (vaccinationDetails == null || !vaccinationDetails.Any())
            {
                return NotFound();
            }
            return Ok(vaccinationDetails);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(CreateVaccinationDetailDTO vaccinationDetail)
        {
            try
            {
                var a = await this.vd.Create(vaccinationDetail);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("create-doctor")]
        public async Task<IActionResult> Createbydoctor(CreateVaccinationDetailDTO vaccinationDetail)
        {
            try
            {
                var a = await this.vd.Createbydoctor(vaccinationDetail);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{Id}")]
        public async Task<IActionResult> Update(int Id, UpdateVaccinationDetailDTO vaccinationDetail)
        {
            try
            {
                var a = await this.vd.Update(Id, vaccinationDetail);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update-expected-date-by-doctor/{Id}")]

        public async Task<IActionResult> UpdateExpectedDatebyDoctor(int Id, DateTime expectedDay)
        {
            try
            {
                var a = await this.vd.UpdateExpectedDatebyDoctor(Id, DateOnly.FromDateTime(expectedDay));
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-id/{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var vaccinationDetail = await vd.GetById(Id);
            if (vaccinationDetail == null)
            {
                return NotFound();
            }
            return Ok(vaccinationDetail);
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            var result = await vd.Delete(Id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

    }
}
