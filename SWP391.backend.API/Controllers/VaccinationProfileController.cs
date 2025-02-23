using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationProfileController : ControllerBase
    {
        private readonly IVaccinationProfile vp;

        public VaccinationProfileController(IVaccinationProfile vp)
        {
            this.vp = vp;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            try
            {
                var profiles = await vp.GetAll(request);
                if (profiles == null || !profiles.Any())
                {
                    return NotFound();
                }
                return Ok(profiles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(int childId)
        {
            try
            {
                var a = await this.vp.Create(childId);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{Id}")]
        public async Task<IActionResult> Update(int Id)
        {
            try
            {
                var a = await this.vp.Update(Id);
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
            var profile = await vp.GetById(Id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var result = await vp.Delete(Id);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
