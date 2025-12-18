using Microsoft.AspNetCore.Identity;

namespace Mango.Services.Auth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
