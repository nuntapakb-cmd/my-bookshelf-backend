using System;

namespace MyBookshelf.Api.DTOs
{
    public class BookDto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public DateTime? PublishedAt { get; set; }
    }
}
