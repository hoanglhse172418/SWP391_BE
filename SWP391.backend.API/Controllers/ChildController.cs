using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Child;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildController : ControllerBase
    {
        private readonly IChild c;

        public ChildController(IChild c)
        {
            this.c = c;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            var children = await c.GetAll(request);
            if (children == null || !children.Any())
            {
                return NotFound();
            }
            return Ok(children);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(CreateChildDTO child)
        {
            try
            {
                var a = await this.c.Create(child);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{Id}")]
        public async Task<IActionResult> Update(int Id, UpdateChildDTO child)
        {
            try
            {
                var a = await this.c.Update(Id, child);
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
            var child = await c.GetById(Id);
            if (child == null)
            {
                return NotFound();
            }
            return Ok(child);
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            var result = await c.Delete(Id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
