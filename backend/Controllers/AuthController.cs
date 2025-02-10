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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Invalid login attempt for user: {Username}", model.Username);
                return Unauthorized(new ProblemDetails { Title = "Invalid credentials", Status = 401 });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateJwtToken(claims.ToArray());
            return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), UserRoles = userRoles, UserData = user.UserData });
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
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email, UserData = model.UserData };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for user: {Username}", model.Username);
                return BadRequest(new ProblemDetails { Title = "Registration failed", Detail = string.Join(", ", result.Errors.Select(e => e.Description)), Status = 400 });
            }

            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User registered successfully: {Username}", model.Username);
            return Ok(new RegisterResponse { Message = "User registered successfully." });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new ProblemDetails { Title = "User not found", Status = 404 });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails { Title = "Password change failed", Detail = string.Join(", ", result.Errors.Select(e => e.Description)), Status = 400 });
            }

            return Ok(new { Message = "Password changed successfully." });
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return NotFound(new ProblemDetails { Title = "User not found", Status = 404 });
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails { Title = "Role assignment failed", Detail = string.Join(", ", result.Errors.Select(e => e.Description)), Status = 400 });
            }

            return Ok(new { Message = "Role assigned successfully." });
        }

        [HttpGet("user-info")]
        [Authorize]
        public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new ProblemDetails { Title = "User not found", Status = 404 });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserInfoResponse { Username = user.UserName, Email = user.Email, UserData = user.UserData, Roles = roles.ToList() });
        }

        private JwtSecurityToken GenerateJwtToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: creds);
        }

        public class RegisterModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [MinLength(6)]
            public string Password { get; set; }

            public string UserData { get; set; }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public List<string> UserRoles { get; set; }
            public string UserData { get; set; }
        }

        public class ApplicationUser : IdentityUser
        {
            public string UserData { get; set; }
        }

        public class ChangePasswordModel
        {
            [Required]
            public string OldPassword { get; set; }

            [Required]
            [MinLength(6)]
            public string NewPassword { get; set; }
        }

        public class AssignRoleModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Role { get; set; }
        }

        public class UserInfoResponse
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string UserData { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}

