namespace IfolorProducerService.Application.Services
{
    public class SensorData
    {
        public required string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double MeasurementValue { get; set; }
        public required string MeasurementType { get; set; }
        public Guid EventId { get; set; } = Guid.NewGuid();
    }
}
