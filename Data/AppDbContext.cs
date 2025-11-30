using Microsoft.EntityFrameworkCore;
using MyBookshelf.Api.Models;

namespace MyBookshelf.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Citat> Citats { get; set; } = null!;
    }
}
