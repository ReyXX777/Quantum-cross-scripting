using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Validate incoming model
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username and Password are required");
            }

            // Find user by username
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Invalid login attempt for user: {Username}", model.Username);
                return Unauthorized("Invalid credentials");
            }

            // Prepare claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, await GetUserRole(user)) // Dynamically fetching role
            };

            // Generate the JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpirationHours"])),
                signingCredentials: creds
            );

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok(new { Message = "User logged out successfully." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Username, Password, and Email are required");
            }

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for user: {Username}", model.Username);
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User registered successfully: {Username}", model.Username);
            return Ok(new { Message = "User registered successfully." });
        }

        private async Task<string> GetUserRole(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? "User"; // Default to "User" if no role is found
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    // ApplicationUser class is part of ASP.NET Identity
    public class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
    }
}
