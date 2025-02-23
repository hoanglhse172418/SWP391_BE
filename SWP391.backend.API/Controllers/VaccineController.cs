using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Vaccine;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineController : ControllerBase
    {
        private readonly IVaccine v;

        public VaccineController(IVaccine v)
        {
            this.v = v;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            var vaccineList = await v.GetAllVaccine(request);
            if (vaccineList == null || !vaccineList.Any())
            {
                return NotFound();
            }
            return Ok(vaccineList);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(CreateVaccineDTO vaccine)
        {
            try
            {
                var v = await this.v.Create(vaccine);
                return Ok(v);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
