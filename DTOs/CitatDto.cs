// MyBookshelf.Api/DTOs/CitatDto.cs
using System;

namespace MyBookshelf.Api.DTOs
{
    public class CitatDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = "";
        public string? Author { get; set; }
        public int? BookId { get; set; }
        public string? BookTitle { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
