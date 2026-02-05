using DidWeFeedTheCatToday.Entities;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; } = null!;
        public DbSet<Feeding> Feedings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pet>()
                .HasMany(p => p.FeedingTimes)
                .WithOne()
                .HasForeignKey(f => f.PetId)
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

            modelBuilder.Entity<Pet>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

        }
    }
}

