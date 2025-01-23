using ExchangeProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeProgram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Programs> Programs { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Beziehung: Document - Student
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Student)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Kein Cascade Delete

            // Beziehung: Document - Application
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Application)
                .WithMany(a => a.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade Delete bleibt hier

            // Beziehung: Application - Program
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Program)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.ProgramId)
                .OnDelete(DeleteBehavior.Restrict); // Kein Cascade Delete

            // Beziehung: Application - Student
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade Delete bleibt hier
        }

    }
}
