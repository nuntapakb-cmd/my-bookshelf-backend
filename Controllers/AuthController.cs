using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MyBookshelf.Api.Data;
using MyBookshelf.Api.DTOs;
using MyBookshelf.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace MyBookshelf.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext db, IConfiguration config, ILogger<AuthController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        // -------------------------------------------------
        // REGISTER (Email-based)
        // -------------------------------------------------
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                // 0) null check
                if (dto == null)
                {
                    return BadRequest(new { message = "Invalid request body." });
                }

                // 1) required fields ( Username not required )
                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return BadRequest(new { message = "Email, password and confirm password are required." });
                }

                // 2) password match
                if (dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Passwords do not match." });
                }

                // 3) basic email check
                if (!dto.Email.Contains("@"))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                // 4) duplicate email
                var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
                if (emailExists)
                {
                    return BadRequest(new { message = "Email already exists." });
                }

                // 5) create username 
                var username = string.IsNullOrWhiteSpace(dto.Username)
                    ? dto.Email.Split('@')[0]
                    : dto.Username.Trim();

                // 6) hash + create user
                var hashedPwd = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Email = dto.Email,
                    Username = username,  
                    PasswordHash = hashedPwd
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error for email {Email}", dto?.Email);

                return StatusCode(500, new
                {
                    message = "Server error during register",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // -------------------------------------------------
        // LOGIN (Email-based)
        // -------------------------------------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid email or password" });

                var token = GenerateJwtToken(user);

                // ✨ return token + email
                return Ok(new { token, email = user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for email {Email}", dto?.Email);
                return StatusCode(500, new
                {
                    message = "Server error during login",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // -------------------------------------------------
        // GENERATE JWT TOKEN
        // -------------------------------------------------
        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");

            var keyString = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(keyString))
                throw new Exception("JWT Key is not configured (Jwt:Key).");

            if (!double.TryParse(jwtSection["DurationMinutes"], out var durationMinutes))
                durationMinutes = 60;

            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),   
                new Claim("username", user.Username),     
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(durationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
