using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBank.Data;
using OnlineBank.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private AppDbContext _context;
        private IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration) 
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel register)
        {
            if (await _context.Users.AnyAsync(u => u.GovId == register.GovId))
                return BadRequest("User with this Goverment Id already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = register.Name,
                Surname = register.Surname,
                GovId = register.GovId,
                Role = "User",
                Balance = 0,
                BankAccountNumber = GenerateBankAccountNumber(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("user registered.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GovId == login.GovId);
            if(user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new {token});
        }

        private string GenerateBankAccountNumber()
        {
            return $"AZ{Guid.NewGuid().ToString().Substring(0, 10).ToUpper()}";
        }

        private string GenerateJwtToken(User user) 
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("BankAccountNumber", user.BankAccountNumber)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
