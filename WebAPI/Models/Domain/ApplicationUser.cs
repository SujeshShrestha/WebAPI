using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models.Domain
{
    public class ApplicationUser:IdentityUser
    {
        public string? Name { get; set; }
    }
}
