using System;

namespace MyBookshelf.Api.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public DateTime? PublishedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
