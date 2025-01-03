using ExchangeProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeProgram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
    }
}
