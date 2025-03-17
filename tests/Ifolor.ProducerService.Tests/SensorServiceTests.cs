using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;

namespace Ifolor.ProducerService.Tests
{
    public class SensorServiceTests
    {
        private readonly SensorService _sensorService;

        public SensorServiceTests()
        {
            _sensorService = new SensorService();
        }

        [Fact]
        public void GetSensors_ReturnsCorrectSensors()
        {
            // Act
            var sensors = _sensorService.GetSensors();

            // Assert
            Assert.NotNull(sensors);
            Assert.Equal(2, sensors.Count);

            var temperatureSensor = sensors[0];
            Assert.Equal("TemperatureSensor-001", temperatureSensor.SensorId);
            Assert.Equal(1000, temperatureSensor.Interval);
            Assert.Equal(MeasurementType.Temperature, temperatureSensor.MeasurementType);

            var pressureSensor = sensors[1];
            Assert.Equal("PressureSensor-001", pressureSensor.SensorId);
            Assert.Equal(2000, pressureSensor.Interval);
            Assert.Equal(MeasurementType.Pressure, pressureSensor.MeasurementType);
        }
    }
}
