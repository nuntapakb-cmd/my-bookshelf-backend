using System.Collections.Generic;

namespace MyBookshelf.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // Navigation properties
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Citat> Citats { get; set; } = new List<Citat>();
    }
}
