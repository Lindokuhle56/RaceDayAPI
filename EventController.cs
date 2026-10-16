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
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] Event evt)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            evt.OrganiserId = int.Parse(userIdStr);

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, evt);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event evt)
        {
            if (id != evt.Id) return BadRequest();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();
            if (existing.OrganiserId != userId) return Forbid();

            _context.Entry(existing).CurrentValues.SetValues(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();
            if (evt.OrganiserId != userId) return Forbid();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}