using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;

namespace IfolorProducerService.Application.Services
{
    public class SensorService : ISensorService
    {
        private static readonly Random _random = new();

        public IReadOnlyList<Sensor> GetSensors()
        {
            var sensors = new List<Sensor>
            {
                CreateSensor("TemperatureSensor-001", 1000, MeasurementType.Temperature, 100, 130),
                CreateSensor("PressureSensor-001", 2000, MeasurementType.Pressure, 1000, 1300)
            };

            if (!sensors.Any())
            {
                throw new InvalidOperationException("No sensors configured.");
            }

            return sensors.AsReadOnly();
        }

        private Sensor CreateSensor(string sensorId, int interval, MeasurementType measurementType, int minValue, int maxValue)
        {
            if (string.IsNullOrEmpty(sensorId))
            {
                throw new ArgumentException("Sensor ID cannot be null or empty.", nameof(sensorId));
            }

            if (interval <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0.", nameof(interval));
            }

            if (minValue >= maxValue)
            {
                throw new ArgumentException("minValue must be less than maxValue.", nameof(minValue));
            }

            return new Sensor
            {
                SensorId = sensorId,
                Interval = interval,
                MeasurementType = measurementType,
                GenerateData = () => GenerateSensorData(minValue, maxValue)
            };
        }

        private double GenerateSensorData(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
