using Microsoft.EntityFrameworkCore;
using NoelFPS.Server.Models;

namespace NoelFPS.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AccessKey> AccessKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessKey>()
                .HasIndex(k => k.Key)
                .IsUnique();
        }
    }
}
