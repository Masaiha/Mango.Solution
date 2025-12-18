using Mango.Services.Auth.Models.Dto;

namespace Mango.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> Register(RegisterationRequestDto registerationRequestDto);

        Task<LoginRequestDto> Login (LoginRequestDto loginRequestDto);
    }
}
