using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Models;
namespace MonitoringBackend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Sensor> Sensors => Set<Sensor>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .HasMany(d => d.sensors)
                .WithOne()
                .HasForeignKey(s => s.device_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
