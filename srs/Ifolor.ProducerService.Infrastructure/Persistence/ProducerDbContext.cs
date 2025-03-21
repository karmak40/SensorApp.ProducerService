using Ifolor.ProducerService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class ProducerDbContext: DbContext
    {
        public DbSet<SensorEventEntity> SensorData { get; set; }

        public ProducerDbContext(DbContextOptions<ProducerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorEventEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventStatus)
                      .HasConversion<string>();

                entity.Property(e => e.MeasurementType)
                        .HasConversion<string>();
            });

        }
    }
}
