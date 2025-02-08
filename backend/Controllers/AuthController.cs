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
using System.Collections.Generic; // Added for UserRoles list
using Newtonsoft.Json; // Added for serializing/deserializing user data

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
            return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), UserRoles = userRoles });
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

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email, UserData = model.UserData }; // Store UserData
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

        // ... (rest of the code remains the same)

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

            public string UserData { get; set; } // Added for storing additional user data
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public List<string> UserRoles { get; set; } // Added to return User Roles
        }

        public class ApplicationUser : IdentityUser
        {
            public string UserData { get; set; } // Added for storing additional user data
        }
    }
}

// Commit message: Added User Roles and User Data to Authentication
// Comment: Included UserRoles in login response and added UserData property to RegisterModel and ApplicationUser for storing additional user information.
