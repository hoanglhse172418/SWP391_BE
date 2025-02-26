using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoom r;

        public RoomController(IRoom r)
        {
            this.r = r;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await r.GetAll();
            if (rooms == null || !rooms.Any())
            {
                return NotFound();
            }
            return Ok(rooms);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateRoom(string roomNumber)
        {
            try
            {
                var room = await r.Create(roomNumber);
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateRoom(int id, string roomNumber)
        {
            try
            {
                var room = await r.Update(id, roomNumber);
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            try
            {
                var room = await r.GetById(id);
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var result = await r.Delete(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
