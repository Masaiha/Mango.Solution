using Mango.Services.Auth.Models.Dto;

namespace Mango.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<string> Register(RegisterationRequestDto registerationRequestDto);

        Task<LoginResponseDto> Login (LoginRequestDto loginRequestDto);
    }
}
