// MyBookshelf.Api/Models/Citat.cs
using System;

namespace MyBookshelf.Api.Models
{
    public class Citat
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // new fields
        public string? Author { get; set; }

        // optional relation to Book
        public int? BookId { get; set; }
        public Book? Book { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
