// MyBookshelf.Api/DTOs/BookDto.cs
using System;

namespace MyBookshelf.Api.DTOs
{
    public class BookDto
    {
        public int? Id { get; set; }
        public string Title { get; set; } = "";
        public string? Author { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
