using Mango.Services.Auth.Interfaces;
using Mango.Services.Auth.Models.Dto;

namespace Mango.Services.Auth.Services
{
    public class AuthService : IAuthService
    {
        public async Task<LoginRequestDto> Login(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto> Register(RegisterationRequestDto registerationRequestDto)
        {
            throw new NotImplementedException();
        }
    }
}
