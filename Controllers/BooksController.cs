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
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BooksController(AppDbContext db)
        {
            _db = db;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: api/Books
        [HttpGet]
        public async Task<IActionResult> GetMyBooks()
        {
            var userId = GetUserId();
            var books = await _db.Books
                                 .AsNoTracking()
                                 .Where(b => b.UserId == userId)
                                 .ToListAsync();
            return Ok(books);
        }

        // GET: api/Books/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var userId = GetUserId();
            var book = await _db.Books.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book == null) return NotFound();
            return Ok(book);
        }

        // POST: api/Books
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookDto dto)
        {
            var userId = GetUserId();
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                PublishedAt = dto.PublishedAt,
                UserId = userId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        // PUT: api/Books/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto dto)
        {
            var userId = GetUserId();
            var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book == null) return NotFound();

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.PublishedAt = dto.PublishedAt;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Books/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var userId = GetUserId();
            var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book == null) return NotFound();

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
