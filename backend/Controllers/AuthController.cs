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

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, await GetUserRole(user))
            };

            var token = GenerateJwtToken(claims);
            return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token) });
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

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for user: {Username}", model.Username);
                return BadRequest(new ProblemDetails { Title = "Registration failed", Detail = string.Join(", ", result.Errors.Select(e => e.Description)), Status = 400 });
            }

            // Assign default role to the user
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User registered successfully: {Username}", model.Username);
            return Ok(new RegisterResponse { Message = "User registered successfully." });
        }

        [HttpPost("create-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExists)
            {
                return BadRequest(new ProblemDetails { Title = "Role already exists", Status = 400 });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails { Title = "Role creation failed", Detail = string.Join(", ", result.Errors.Select(e => e.Description)), Status = 400 });
            }

            _logger.LogInformation("Role created successfully: {RoleName}", model.RoleName);
            return Ok(new { Message = "Role created successfully." });
        }

        private JwtSecurityToken GenerateJwtToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpirationHours"])),
                signingCredentials: creds
            );
        }

        private async Task<string> GetUserRole(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? "User";
        }
    }

    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
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
    }

    public class RoleModel
    {
        [Required]
        public string RoleName { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }

    public class RegisterResponse
    {
        public string Message { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
    }
}
