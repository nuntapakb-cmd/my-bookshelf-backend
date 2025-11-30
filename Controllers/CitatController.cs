using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookshelf.Api.Data;
using MyBookshelf.Api.DTOs;
using MyBookshelf.Api.Models;

namespace MyBookshelf.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CitatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CitatController(AppDbContext db)
        {
            _db = db;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: api/Citat  (user's citats)
        [HttpGet]
        public async Task<IActionResult> GetMyCitats()
        {
            var userId = GetUserId();
            var citats = await _db.Citats
                                  .AsNoTracking()
                                  .Where(c => c.UserId == userId)
                                  .OrderByDescending(c => c.CreatedAt)
                                  .ToListAsync();
            return Ok(citats);
        }

        // GET: api/Citat/top5
        [HttpGet("top5")]
        public async Task<IActionResult> GetTop5()
        {
            var userId = GetUserId();
            var citats = await _db.Citats
                                  .AsNoTracking()
                                  .Where(c => c.UserId == userId)
                                  .OrderByDescending(c => c.CreatedAt)
                                  .Take(5)
                                  .ToListAsync();
            return Ok(citats);
        }

        // POST: api/Citat
        [HttpPost]
        public async Task<IActionResult> CreateCitat([FromBody] CitatDto dto)
        {
            var userId = GetUserId();
            var citat = new Citat
            {
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            _db.Citats.Add(citat);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMyCitats), new { id = citat.Id }, citat);
        }

        // PUT: api/Citat/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCitat(int id, [FromBody] CitatDto dto)
        {
            var userId = GetUserId();
            var citat = await _db.Citats.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (citat == null) return NotFound();

            citat.Text = dto.Text;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Citat/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCitat(int id)
        {
            var userId = GetUserId();
            var citat = await _db.Citats.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (citat == null) return NotFound();

            _db.Citats.Remove(citat);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
