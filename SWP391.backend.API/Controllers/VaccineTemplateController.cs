using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccineTemplate;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineTemplateController : ControllerBase
    {
        private readonly IVaccineTemplate vt;

        public VaccineTemplateController(IVaccineTemplate vt)
        {
            this.vt = vt;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            var vct = await vt.GetAll(request);

            if (vct == null || !vct.Any())
            {
                return NotFound();
            }

            return Ok(vct);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromForm]CreateVaccineTemplateDTO request)
        {
            try
            {
                var vct = await vt.Create(request);
                return Ok(vct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> Update(int id,  UpdateVaccineTemplateDTO request)
        {
            try
            {
                var vct = await vt.Update(id, request);
                return Ok(vct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vct = await vt.GetById(id);
            if (vct == null)
            {
                return NotFound();
            }
            return Ok(vct);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await vt.Delete(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
