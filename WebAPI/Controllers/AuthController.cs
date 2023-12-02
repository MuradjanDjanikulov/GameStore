using Api.Models;
using API.Models;
using DataAccess.Entity;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;
using WebAPI.Utils;

namespace Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;



        public AuthController(
            Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager,
            IConfiguration configuration,
            AppDbContext context,
            TokenValidationParameters tokenValidationParameters
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [Route("register")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var foundUser = await _userManager.FindByNameAsync(registerModel.Username);
            if (foundUser != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel("Error", "User already exists!"));
            }
            var foundUserByEmail = await _userManager.FindByEmailAsync(registerModel.Email);

            if (foundUserByEmail != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel("Error", "Email already exists!"));
            }

            var user = new AppUser
            {
                TokenSign = Guid.NewGuid().ToString(),
                UserName = registerModel.Username,
                Email = registerModel.Email,
                FirstName = registerModel.Firstname,
                LastName = registerModel.Lastname
            };

            var result = await _userManager.CreateAsync(user, registerModel.Password);
            if (!result.Succeeded)
            {
                var errorMessages = new List<string>();

                foreach (var error in result.Errors)
                {
                    errorMessages.Add(error.Description);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, errorMessages);

            }

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok(new ResponseModel("Success", "User created successfully"));
        }

        [Route("update-password")]
        [HttpPut, Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordModel passwordModel)
        {
            var foundUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (foundUser != null)
            {

                foundUser.TokenSign = Guid.NewGuid().ToString();
                await _userManager.UpdateAsync(foundUser);
                var result = await _userManager.ChangePasswordAsync(foundUser, passwordModel.OldPassword, passwordModel.NewPassword);
                if (result.Succeeded)
                {
                    var stored = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.User.Id == foundUser.Id);
                    if (stored != null && stored.DateExpire >= DateTime.UtcNow)
                    {
                        _context.RefreshTokens.Remove(stored);
                        await _context.SaveChangesAsync();
                    }
                    return Ok(new ResponseModel("Success", "Password changed successfully"));

                }

            }

            return BadRequest(new ResponseModel("Error", "Password changing failed"));
        }



        [Route("up-permission/{username}")]
        [HttpPut]
        [Authorize(Policy = "RequireManager")]
        public async Task<IActionResult> UpPermission([FromRoute] String username)
        {
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return NotFound(new ResponseModel("Error", $"User '{username}' not found"));
            }

            await _userManager.AddToRoleAsync(foundUser, UserRoles.Manager);

            return Ok(new ResponseModel("Success", $"Role '{UserRoles.Manager}' added to user '{username}'"));
        }

        [Route("down-permission/{username}")]
        [HttpPut]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DownPermission([FromRoute] String username)
        {
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return NotFound(new ResponseModel("Error", $"User '{username}' not found"));
            }

            await _userManager.RemoveFromRoleAsync(foundUser, UserRoles.Manager);

            return Ok(new ResponseModel("Success", $"Role '{UserRoles.Manager}' is removed from user '{username}'"));
        }


        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel LoginModel)
        {
            var foundUser = await _userManager.FindByNameAsync(LoginModel.UserName);
            if (foundUser != null && await _userManager.CheckPasswordAsync(foundUser, LoginModel.Password))
            {
                var tokenValue = await GenerateJWTTokenAsync(foundUser, null, LoginModel.RememberMe);
                return Ok(tokenValue);
            }
            return Unauthorized(new ResponseModel("Error", $"'{LoginModel.UserName}' not found"));

        }

        [Route("refresh-token")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel TokenModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel("Error", "Please, provide all required fields"));
            }

            AppUser DbUser;

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == TokenModel.RefreshToken && x.DateExpire > DateTime.UtcNow);

            if (storedToken != null)
            {
                DbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                if (DbUser == null) { return BadRequest(new ResponseModel("Error", "Invalid access or refresh token")); }

            }
            else
            {
                return BadRequest(new ResponseModel("Error", "Invalid refresh token"));
            }

            try
            {
                jwtTokenHandler.ValidateToken(TokenModel.Token, _tokenValidationParameters, out SecurityToken securityToken);

                return Ok(await GenerateJWTTokenAsync(DbUser, storedToken, false));
            }
            catch (SecurityTokenExpiredException)
            {
                return Ok(await GenerateJWTTokenAsync(DbUser, storedToken, false));
            }
            catch (SecurityTokenException)
            {
                return BadRequest(new ResponseModel("Error", "Invalid access or refresh token"));
            }

        }


        private async Task<TokenModel> GenerateJWTTokenAsync(AppUser User, RefreshToken? RefToken, bool RememberMe)
        {
            var roles = await _userManager.GetRolesAsync(User);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, User.UserName),
                new Claim(ClaimTypes.NameIdentifier, User.Id),
                //new Claim(JwtRegisteredClaimNames.Name, User.TokenSign),
                new Claim("TokenSign", User.TokenSign),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: signingCredentials);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = RefToken;
            if (RefToken == null)
            {
                refreshToken = new RefreshToken()
                {
                    UserId = User.Id,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(60),
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
                };

                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.User.Equals(User));
                if (storedToken != null) { _context.RefreshTokens.Remove(storedToken); }
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
            }

            return new TokenModel
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
            };
        }

        [HttpDelete, Authorize]
        [Route("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var username = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            user.TokenSign = Guid.NewGuid().ToString();
            await _userManager.UpdateAsync(user);

            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.User.Id == user.Id);
            if (stored != null && stored.DateExpire >= DateTime.UtcNow)
            {
                _context.RefreshTokens.Remove(stored);
                await _context.SaveChangesAsync();
                return Ok(new ResponseModel("Success", "Refresh token revoked successfully"));
            }

            return BadRequest(new ResponseModel("Error", "Refresh token is not found"));
        }


        [Route("set-image")]
        [HttpPost, Authorize]
        public async Task<IActionResult> SetImage(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return BadRequest(new ResponseModel("Error", "Invalid file"));
            }

            var foundUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            string imageDir = Directory.GetCurrentDirectory();

            string uniqueFileName = $"{foundUser.Id}.jpeg";

            string filePath = Path.Combine(imageDir, "Images", uniqueFileName);

            Console.WriteLine(filePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            foundUser.ImageUrl = filePath;
            await _userManager.UpdateAsync(foundUser);

            return Ok(new ResponseModel("Success", "Image set successfully"));
        }


        [HttpGet, Authorize]
        [Route("get-current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            var userDb = new UserModel { FirstName = user.FirstName, LastName = user.LastName, UserName = user.UserName, Email = user.Email, ImageUrl = user.ImageUrl };
            return Ok(userDb);
        }

        [HttpGet]
        [Route("get-user")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetUser(String Username)
        {
            var user = await _userManager.FindByNameAsync(Username);
            var userDb = new UserModel { FirstName = user.FirstName, LastName = user.LastName, UserName = user.UserName, Email = user.Email, ImageUrl = user.ImageUrl };
            return Ok(userDb);
        }

        [HttpGet]
        [Route("all-users")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userList = await _userManager.Users.ToListAsync();
            var userDbList = new List<UserModel>();
            foreach (var user in userList)
            {
                var userDb = new UserModel { FirstName = user.FirstName, LastName = user.LastName, UserName = user.UserName, Email = user.Email, ImageUrl = user.ImageUrl };

                userDbList.Add(userDb);
            }
            return Ok(userDbList);
        }

        [HttpDelete]
        [Route("remove-user/{username}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RemoveUser([FromRoute] String Username)
        {
            var usernameDb = HttpContext.User.Identity.Name;
            if (usernameDb.Equals(Username))
            {
                return BadRequest(new ResponseModel("Error", "You could not delete yourself"));
            }

            var user = await _userManager.FindByNameAsync(Username);

            if (user == null)
            {
                return BadRequest(new ResponseModel("Error", $"'{Username}' not found"));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Ok(new ResponseModel("Success", "User deleted successfully"));
            }
            return BadRequest(new ResponseModel("Error", "User deletion failed"));

        }


        [HttpGet]
        [Route("test")]
        [Authorize]
        public async Task<IActionResult> GetTest()
        {
            return Ok("Welcome to test method");
        }

    }

}
