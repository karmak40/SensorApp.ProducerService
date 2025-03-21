using IfolorProducerService.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ifolor.ProducerService.Core.Models
{
    [Table("SensorData")]

    public class SensorEventEntity
    {
        public int Id { get; set; }
        public Guid EventId { get; set; }
        public EventStatus EventStatus { get; set; }
        public required string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public double MeasurementValue { get; set; }
    }
}
