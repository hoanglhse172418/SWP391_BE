using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiseaseController : ControllerBase
    {
        private readonly IDisease d;

        public DiseaseController(IDisease d)
        {
            this.d = d;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            try
            {
                var diseases = await d.GetAll(request);
                if (diseases == null || !diseases.Any())
                {
                    return NotFound();
                }
                return Ok(diseases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(string name)
        {
            try
            {
                var a = await this.d.Create(name);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{Id}")]
        public async Task<IActionResult> Update(int Id, string name)
        {
            try
            {
                var a = await this.d.Update(Id, name);
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
            try
            {
                var disease = await d.GetById(Id);
                if (disease == null)
                {
                    return NotFound();
                }
                return Ok(disease);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var result = await d.Delete(Id);
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
