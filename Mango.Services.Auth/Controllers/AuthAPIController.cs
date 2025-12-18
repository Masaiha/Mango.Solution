using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthAPIController : ControllerBase
    {
        [HttpGet(Name = "register")]
        public async Task<IActionResult> Register()
        {

            return Ok();
        }

        [HttpPost(Name = "login")]
        public async Task<IActionResult> Login()
        {
            return Ok();

        }
    }
}
