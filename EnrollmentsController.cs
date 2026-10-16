using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEnrollments()
        {
            var enrollments = await _context.Enrollments.Include(e => e.Event).ToListAsync();
            return Ok(enrollments);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Event)
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpPost("{eventId}")]
        public async Task<IActionResult> Enroll(int eventId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null) return NotFound("Event not found");

            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);
            if (existing != null) return BadRequest("Already enrolled");

            var enrollment = new Enrollment
            {
                EventId = eventId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollments), new { id = enrollment.Id }, enrollment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Unenroll(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();
            if (enrollment.UserId != userId) return Forbid();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}