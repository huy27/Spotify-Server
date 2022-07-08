using Application.IService;
using Application.Ultilities;
using Data.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthenticateController(IUserService userService)
        {
            _userService = userService;
        }

        #region Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate(string username, string password)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultToken = await _userService.Login(username, password);
            if (string.IsNullOrEmpty(resultToken))
                return BadRequest("UserName or Password is invalid");

            return Ok(resultToken);
        }
        #endregion

        #region Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.Register(registerModel.UserName, registerModel.Password, registerModel.Email);
            if (!result)
                return BadRequest("Register Fail");

            return Ok($"Register for {registerModel.UserName} success");
        }
        #endregion

        #region RegisterAdmin
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAdmin(registerModel.UserName, registerModel.Password, registerModel.Email);
            if (!result)
                return BadRequest("Register Fail");

            return Ok($"Register for {registerModel.UserName} success");
        }
        #endregion

        #region ResetPassword
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("ResetPassword")]
        public async Task<ActionResult> ResetPassword(string username, string newPassword)
        {
            var result = await _userService.ResetPassword(username, newPassword);
            if (!result)
                return BadRequest("Something wrong");

            return Ok();
        }
        #endregion
    }
}
