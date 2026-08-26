using Microsoft.EntityFrameworkCore;

namespace EtNotif.Libs.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Taxpayer> Taxpayers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Taxpayer>()
                .HasIndex(t => t.Vkn)
                .IsUnique();
        }
    }
}
