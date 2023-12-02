using DataAccess.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Utils
{
    public class TokenService
    {
        private readonly UserManager<AppUser> _userManager;

        public TokenService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AppUser> GetUserByTokenSign(string TokenSign)
        {
            var user = new AppUser();
            user = await _userManager.Users.Where(u => u.TokenSign.Equals(TokenSign)).FirstOrDefaultAsync();
            return user;
        }

    }

}
