using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager; // Assuming you're using ASP.NET Identity

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Validate user credentials (you should use ASP.NET Identity or similar mechanism)
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.Username),
                // Additional claims can be added, e.g., roles, email
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User") // Assuming the user has a "User" role
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpirationHours"])), // Configurable expiration
                signingCredentials: creds
            );

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // ApplicationUser class is part of ASP.NET Identity
    public class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
    }
}
