// MyBookshelf.Api/Controllers/CitatController.cs
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
                                  .Include(c => c.Book)
                                  .OrderByDescending(c => c.CreatedAt)
                                  .ToListAsync();

            var dtos = citats.Select(c => new CitatDto {
                Id = c.Id,
                Text = c.Text,
                Author = !string.IsNullOrWhiteSpace(c.Author) ? c.Author : c.Book?.Author,
                BookId = c.BookId,
                BookTitle = c.Book?.Title,
                CreatedAt = c.CreatedAt
            });

            return Ok(dtos);
        }

        // GET: api/Citat/top5
        [HttpGet("top5")]
        public async Task<IActionResult> GetTop5()
        {
            var userId = GetUserId();
            var citats = await _db.Citats
                                  .AsNoTracking()
                                  .Where(c => c.UserId == userId)
                                  .Include(c => c.Book)
                                  .OrderByDescending(c => c.CreatedAt)
                                  .Take(5)
                                  .ToListAsync();

            var dtos = citats.Select(c => new CitatDto {
                Id = c.Id,
                Text = c.Text,
                Author = !string.IsNullOrWhiteSpace(c.Author) ? c.Author : c.Book?.Author,
                BookId = c.BookId,
                BookTitle = c.Book?.Title,
                CreatedAt = c.CreatedAt
            });

            return Ok(dtos);
        }

        // GET: api/Citat/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCitatById(int id)
        {
            var userId = GetUserId();

            var citat = await _db.Citats
                         .AsNoTracking()
                         .Include(c => c.Book)
                         .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (citat == null) return NotFound();

            var dto = new CitatDto {
                Id = citat.Id,
                Text = citat.Text,
                Author = !string.IsNullOrWhiteSpace(citat.Author) ? citat.Author : citat.Book?.Author,
                BookId = citat.BookId,
                BookTitle = citat.Book?.Title,
                CreatedAt = citat.CreatedAt
            };

            return Ok(dto);
        }

        // POST: api/Citat
        [HttpPost]
        public async Task<IActionResult> CreateCitat([FromBody] CitatDto dto)
        {
            var userId = GetUserId();

            var citat = new Citat
            {
                Text = dto.Text,
                Author = string.IsNullOrWhiteSpace(dto.Author) ? null : dto.Author,
                BookId = dto.BookId,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _db.Citats.Add(citat);
            await _db.SaveChangesAsync();

            // load book for returning friendly DTO
            var book = citat.BookId.HasValue ? await _db.Books.AsNoTracking().SingleOrDefaultAsync(b => b.Id == citat.BookId.Value) : null;

            var result = new CitatDto {
                Id = citat.Id,
                Text = citat.Text,
                Author = citat.Author ?? book?.Author,
                BookId = citat.BookId,
                BookTitle = book?.Title,
                CreatedAt = citat.CreatedAt
            };

            return CreatedAtAction(nameof(GetMyCitats), new { id = citat.Id }, result);
        }


        // PUT: api/Citat/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCitat(int id, [FromBody] CitatDto dto)
        {
            var userId = GetUserId();
            var citat = await _db.Citats.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (citat == null) return NotFound();

            citat.Text = dto.Text;
            citat.Author = string.IsNullOrWhiteSpace(dto.Author) ? null : dto.Author;
            citat.BookId = dto.BookId;

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
