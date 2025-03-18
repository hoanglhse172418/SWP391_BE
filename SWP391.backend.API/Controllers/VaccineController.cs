using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Vaccine;
using SWP391.backend.services;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineController : ControllerBase
    {
        private readonly IVaccine v;
        private readonly SCloudinary _cloudinary;

        public VaccineController(IVaccine v, SCloudinary cloudinary)
        {
            this.v = v;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var vaccineList = await v.GetAllVaccine();
            if (vaccineList == null || !vaccineList.Any())
            {
                return NotFound();
            }
            return Ok(vaccineList);
        }

        [HttpGet]
        [Route("get-by-id/{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var vaccine = await v.GetById(Id);
            if (vaccine == null)
            {
                return NotFound();
            }
            return Ok(vaccine);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromForm]CreateVaccineDTO vaccine)
        {
            try
            {
                string imageUrl = null;
                if (vaccine.ImageFile != null) 
                { 
                    imageUrl = await _cloudinary.UploadImageAsync(vaccine.ImageFile);
                }
                var v = await this.v.Create(vaccine, imageUrl);
                return Ok(v);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{Id}")]
        public async Task<IActionResult> Update(int Id, [FromForm]UpdateVaccineDTO vaccine, IFormFile imageFile)
        {
            try
            {
                string? imageUrl = null;
                if(imageFile != null)
                {
                    imageUrl = await _cloudinary.UploadImageAsync(imageFile);
                }
                var v = await this.v.Update(Id, vaccine, imageUrl);
                return Ok(v);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-vaccines-by-diasease-name/{diseaseName}")]
        public async Task<IActionResult> GetVaccinesByDisease(string diseaseName)
        {
            try
            {
                var vaccines = await v.GetAllVaccinesByDiasease(diseaseName);
                if (vaccines == null || vaccines.Count == 0)
                {
                    return NotFound($"No vaccines found for disease: {diseaseName}");
                }
                return Ok(vaccines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
