using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ImageUrl { get; set; }
        public string TokenSign { get; set; }
    }
}
