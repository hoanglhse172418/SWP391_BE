using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Account;
using System.Security.Principal;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUser u;

        public UserController(IUser u)
        {
            this.u = u;
        }

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDTO request)
        {
            var users = await u.GetAll(request);

            if (users == null || !users.Any())
            {
                return NotFound();
            }

            return Ok(users);
        }

        [AllowAnonymous]
        [Route("registration")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO user)
        {
            try
            {
                var a = await this.u.CreateUser(user);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("create-staff")]
        [HttpPost]
        public async Task<IActionResult> CreateStaff(CreateUserDTO user)
        {
            try
            {
                var a = await this.u.CreateStaff(user);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [AllowAnonymous]
        [Route("create-doctor")]
        [HttpPost]
        public async Task<IActionResult> CreateDoctor(CreateUserDTO user)
        {
            try
            {
                var a = await this.u.CreateDoctor(user);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("update/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDTO user)
        {
            try
            {
                var a = await this.u.Update(id, user);
                return Ok(a);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("get/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await this.u.GetByID(id);

                // Check if the user was found
                if (user == null)
                {
                    return NotFound($"User with ID '{id}' not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("delete")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var a = await u.Delete(id);
                return Ok(a);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("forgot-password")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await u.ForgotPassword(email);
                return Ok("Password reset link sent to your email.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("reset-password")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            try
            {
                await u.ResetPassword(token, newPassword, confirmPassword);
                return Ok("Password reset successful.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO user)
        {
            try
            {
                var a = await u.Login(user);
                return Ok(a);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }
            bool isUserLoggedOut = await u.Logout(email);
            if (isUserLoggedOut)
            {
                return Ok(new { Message = "User logged out successfully." });
            }
            return BadRequest("User does not exist or is already logged out.");
        }
    }
}

