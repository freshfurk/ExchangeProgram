using ExchangeProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeProgram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Student)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Automatisch löschen, wenn Student gelöscht wird
        }
    }
}
