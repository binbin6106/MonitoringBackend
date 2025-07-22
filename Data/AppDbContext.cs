using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Models;
namespace MonitoringBackend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Sensor> Sensors => Set<Sensor>();
        public DbSet<Gateway> Gateways => Set<Gateway>();
        public DbSet<AlarmThreshold> AlarmThresholds { get; set; }
        public DbSet<AlarmRecord> AlarmRecords { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gateway>()
                .HasMany(d => d.sensors)
                .WithOne()
                .HasForeignKey(s => s.gateway_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlarmThreshold>()
               .HasIndex(t => new { t.sensor_id, t.alarmType })
               .IsUnique();

            //    modelBuilder.Entity<AlarmThreshold>()
            //        .HasOne(t => t.sensor)
            //        .HasForeignKey(t => t.sensor_id)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    modelBuilder.Entity<AlarmRecord>()
            //        .HasOne(r => r.sensor)
            //        .HasForeignKey(r => r.SensorId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //}
        }
    }

}
