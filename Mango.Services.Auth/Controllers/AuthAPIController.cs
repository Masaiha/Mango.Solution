using Mango.Services.Auth.Interfaces;
using Mango.Services.Auth.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private ResponseDto _response;

        public AuthAPIController(IAuthService authService, ResponseDto response)
        {
            _authService = authService;
            _response = response;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDto user)
        {
            var errorMessage = await _authService.Register(user);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;

                return BadRequest(_response);
            }

            return Ok(_response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var loginResponse = await _authService.Login(loginRequestDto);

            if(loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect.";
                return BadRequest(_response);
            }

            _response.Response = loginResponse;
            _response.IsSuccess = true;

            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegisterationRequestDto registerationRequestDto)
        {
            var assignRoleSuccessful = await _authService.AssignRole(registerationRequestDto.Email, registerationRequestDto.Role.ToUpper());

            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "userName or Password is incorrect";
                return BadRequest(_response);
            }

            _response.Response = assignRoleSuccessful;
            return Ok(_response);
        }
    }
}
