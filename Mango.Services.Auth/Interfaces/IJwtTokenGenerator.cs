using Mango.Services.Auth.Models;

namespace Mango.Services.Auth.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser);
    }
}
