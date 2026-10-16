using Microsoft.AspNetCore.Mvc;           
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;      
using Microsoft.IdentityModel.Tokens;     
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;              
using System.Text;                          
using RaceDayAPI.Data;                     
using RaceDayAPI.Models;                
using BCrypt.Net;                         

namespace RaceDayAPI.Controllers            
{
    [ApiController]
    [Route("api/[controller]")]             
                       
    public class AuthController : ControllerBase  
    {
        private readonly AppDbContext _context;  

        public AuthController(AppDbContext context)  
        {
            _context = context;              
        }

        [HttpPost("register")]           
        public IActionResult Register([FromBody] UserRegisterDto dto)  
        {
            if (_context.Users.Any(u => u.Email == dto.Email))         
                return Conflict("Email already registered");           

            var user = new User           
            {
                Username = dto.Username,     
                Email = dto.Email,           
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"                
            };

            _context.Users.Add(user);        
            _context.SaveChanges();          

            return Ok("Registration successful");  
        }

        [HttpPost("login")]                  
        public IActionResult Login([FromBody] UserLoginDto dto)  
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email); 

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))  
                return Unauthorized("Invalid email or password");  

            var token = GenerateToken(user);  
            return Ok(new { Token = token }); 
        }

        private string GenerateToken(User user)  
        {
            var key = Encoding.UTF8.GetBytes("ThisIsAVeryStrongSecretKey12345678901234567890");  
            var claims = new[]                
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  
                new Claim(ClaimTypes.Email, user.Email),                   
                new Claim(ClaimTypes.Role, user.Role)                       
            };

            var token = new JwtSecurityToken(  
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(  
                    new SymmetricSecurityKey(key),            
                    SecurityAlgorithms.HmacSha256Signature) 
            );

            return new JwtSecurityTokenHandler().WriteToken(token);  
        }
    }

    public class UserRegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDto                 
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}