using Xunit;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace RaceDayAPI.Tests
{
    public class AuthControllerTests
    {
        private readonly RaceDayContext _context;
        private readonly IConfiguration _config;

        public AuthControllerTests()
        {
            var opts = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase("RaceDayTestDB")
                .Options;
            _context = new RaceDayContext(opts);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Jwt:Key", "TestSecretKeyMustBeAtLeast32CharsLong!!"},
                    {"Jwt:Issuer", "Test"},
                    {"Jwt:Audience", "Test"},
                    {"Jwt:ExpiresInMinutes", "60"}
                })
                .Build();
        }

        [Fact]
        public void Register_ValidParticipant_Returns201()
        {
            var ctrl = new AuthController(_context, _config);
            var result = ctrl.Register(new RegisterDto
            {
                FullName = "Jane Runner",
                Email = "jane@test.com",
                Password = "password123",
                Role = "Participant"
            });
            var status = (result as ObjectResult)?.StatusCode;
            Assert.Equal(201, status);
        }

        [Fact]
        public void Register_DuplicateEmail_Returns400()
        {
            var ctrl = new AuthController(_context, _config);
            ctrl.Register(new RegisterDto { FullName = "A", Email = "dup@test.com", Password = "pass123", Role = "Organiser" });
            var result = ctrl.Register(new RegisterDto { FullName = "B", Email = "dup@test.com", Password = "pass456", Role = "Participant" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsUnauthorized()
        {
            var ctrl = new AuthController(_context, _config);
            ctrl.Register(new RegisterDto { FullName = "Test", Email = "user@test.com", Password = "correctpass", Role = "Participant" });
            var result = ctrl.Login(new LoginDto { Email = "user@test.com", Password = "wrongpass" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}