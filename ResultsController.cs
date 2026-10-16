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
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Event)
                .Include(r => r.User)
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var results = await _context.Results
                .Where(r => r.EventId == eventId)
                .Include(r => r.User)
                .OrderBy(r => r.Position)
                .ToListAsync();
            return Ok(results);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddResult([FromBody] Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResults), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();
            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}