
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.ToTable("Movies", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Movies_DurationMinutes",
                        "[DurationMinutes] > 0");
                });

                entity.HasKey(m => m.MovieId);

                entity.Property(m => m.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(m => m.Description)
                    .HasMaxLength(500);

                entity.Property(m => m.DurationMinutes)
                    .IsRequired();

                entity.Property(m => m.ReleaseDate)
                    .HasColumnType("date");

                entity.Property(m => m.Language)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(m => m.Genre)
                    .HasMaxLength(50)
                    .IsRequired();
            });
        }
    }
}
