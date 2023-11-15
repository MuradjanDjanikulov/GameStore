﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Api.Models;
using DataAccess.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using API.Models;
using DataAccess.Repository;
using WebAPI.Utils;
using WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Hosting.Internal;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;
        //private readonly SignInManager<AppUser> _signInManager;


        public AuthController(
            Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, 
            //RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, 
            AppDbContext context,
            TokenValidationParameters tokenValidationParameters
            //SignInManager<AppUser> signInManager
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            //_roleManager = roleManager;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
            //_signInManager = signInManager;

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

            var user = new AppUser { 
                SecurityStamp = Guid.NewGuid().ToString(), 
                UserName = registerModel.Username, 
                Email = registerModel.Email,
                FirstName = registerModel.Firstname,
                LastName =registerModel.Lastname 
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

                //return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel("Error", "User creation failed!"));
            }

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok(new ResponseModel("Success", "User created successfully"));
        }

        [Route("update-password")]
        [HttpPost, Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordModel passwordModel)
        {
            var foundUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (foundUser != null)
            {
                var result = await _userManager.ChangePasswordAsync(foundUser, passwordModel.OldPassword, passwordModel.NewPassword);

                if (result.Succeeded)
                {
                    var stored = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.User.Id == foundUser.Id);
                    if (stored != null && stored.DateExpire >= DateTime.UtcNow)
                    {
                        _context.RefreshTokens.Remove(stored);
                        await _context.SaveChangesAsync();
                        
                        return Ok(new ResponseModel("Success", "Password changed successfully"));

                    }

                }
               
            }

            return BadRequest( new ResponseModel("Error", "Password changing failed"));
        }

      

        [Route("up-permission")]
        [HttpPost]
        [Authorize(Policy = "RequireAdminOnly")]
        public async Task<IActionResult> UpPermission(String Username)
        {
            var foundUser = await _userManager.FindByNameAsync(Username);
            if (foundUser == null)
            {
                return NotFound(new ResponseModel("Error", $"User '{Username}' not found"));
            }

            await _userManager.AddToRoleAsync(foundUser, UserRoles.Manager);

            return Ok(new ResponseModel("Success", $"Role '{UserRoles.Manager}' added to user '{Username}'"));
        }

        [Route("down-permission")]
        [HttpPost]
        [Authorize(Policy = "RequireAdminOnly")]
        public async Task<IActionResult> DownPermission(String Username)
        {
            var foundUser = await _userManager.FindByNameAsync(Username);
            if (foundUser == null)
            {
                return NotFound(new ResponseModel("Error", $"User '{Username}' not found"));
            }

            await _userManager.RemoveFromRoleAsync(foundUser, UserRoles.Manager);

            return Ok(new ResponseModel("Success", $"Role '{UserRoles.Manager}' is deleted from user '{Username}'"));
        }


        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel LoginModel)
        {
            //var result = await _signInManager.PasswordSignInAsync(LoginModel.UserName, LoginModel.Password, LoginModel.RememberMe, lockoutOnFailure: false);

            var foundUser = await _userManager.FindByNameAsync(LoginModel.UserName);
            if (foundUser != null && await _userManager.CheckPasswordAsync(foundUser, LoginModel.Password))
            {
                var tokenValue = await GenerateJWTTokenAsync(foundUser, null, LoginModel.RememberMe);
                return Ok(tokenValue);
            }
            return Unauthorized();

        }


        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel TokenModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }

            AppUser DbUser;

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == TokenModel.RefreshToken && x.DateExpire > DateTime.UtcNow);

            if (storedToken != null)
            {
                DbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                if (DbUser == null) { return BadRequest("(Invalid access or refresh token"); }

            }
            else 
            {
                return BadRequest("(Invalid refresh token");
            }

            try
            {
                jwtTokenHandler.ValidateToken(TokenModel.Token, _tokenValidationParameters, out SecurityToken securityToken);

                return Ok(await GenerateJWTTokenAsync(DbUser, storedToken,false));
            }
            catch (SecurityTokenExpiredException)
            {
                return Ok(await GenerateJWTTokenAsync(DbUser, storedToken, false));
            }
            catch (SecurityTokenException)
            {
                return BadRequest("(Invalid access or refresh token");
            }

        }


        private async Task<TokenModel> GenerateJWTTokenAsync(AppUser User, RefreshToken? RefToken, bool RememberMe ) 
        {
            var roles = await _userManager.GetRolesAsync(User);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, User.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(ClaimTypes.Role, UserRoles.User)
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
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: signingCredentials);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = RefToken;
            if (RefToken == null)
            {
                refreshToken = new RefreshToken()
                {
                    UserId = User.Id,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(10),
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

        [HttpPost, Authorize]
        [Route("revoke-refresh-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var username = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            
            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.User.Id == user.Id);
            if (stored != null && stored.DateExpire >= DateTime.UtcNow)
            {
                _context.RefreshTokens.Remove(stored);
                await _context.SaveChangesAsync();
                //await _signInManager.SignOutAsync();
                return Ok("Refresh token is deleted");
            }
            
            return BadRequest("Refresh token is not found!");
        }


        [Route("test")]
        [HttpGet]
        //[AllowAnonymous]
        [Authorize(Policy = "RequireAdminOnly")]
        public async Task<IActionResult> Test()
        {
            return Ok("You entered successfully");
        }


        [HttpGet, Authorize]
        [Route("user-info")]
        public async Task<IActionResult> UserInfo() 
        {
            var username = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            var userDb = new UserModel { FirstName = user.FirstName, LastName = user.LastName, UserName = user.UserName, Email = user.Email, ImageUrl = user.ImageUrl };
            return Ok(userDb);
        }

      
        [Route("set-image")]
        [HttpPost, Authorize]
        public async Task<IActionResult> SetImage(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var foundUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            string imageDir = Directory.GetCurrentDirectory();

            string uniqueFileName = $"{foundUser.Id}.jpeg";

            string filePath = Path.Combine(imageDir,"Images", uniqueFileName);

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

            return Ok(new ResponseModel("Success", "Image is set successfully"));
           
        }
    }

}