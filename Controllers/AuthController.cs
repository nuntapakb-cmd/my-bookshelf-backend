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

        // ----------------------------
        // REGISTER
        // ----------------------------
        [HttpPost("register")]
         [AllowAnonymous]  
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already exists." });

            var hashedPwd = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User { Username = dto.Username, PasswordHash = hashedPwd };

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Register error for username {Username}", dto.Username);
                return BadRequest(new { message = "Could not create user. Username may already exist." });
            }

            return Ok(new { message = "Registered successfully" });
        }

        // ----------------------------
        // LOGIN
        // ----------------------------
        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });

            try
            {
                // FirstOrDefaultAsync = ปลอดภัยกว่า SingleOrDefaultAsync
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

                if (user == null)
                    return Unauthorized(new { message = "Invalid username or password" });

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid username or password" });

                var token = GenerateJwtToken(user);

                return Ok(new { token, username = user.Username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for username {Username}", dto?.Username);
                return StatusCode(500, new
                {
                    message = "Server error during login",
                    error = ex.Message
                });
            }
        }

        // ----------------------------
        // GENERATE JWT TOKEN
        // ----------------------------
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
                new Claim(ClaimTypes.Name, user.Username)
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
