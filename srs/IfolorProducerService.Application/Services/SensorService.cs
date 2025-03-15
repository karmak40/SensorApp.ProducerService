using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;

namespace IfolorProducerService.Application.Services
{
    public class SensorService: ISensorService
    {
        public List<Sensor> GetSensors()
        {
            return new List<Sensor>
            {
                new Sensor { SensorId = "TemperatureSensor-001", Interval = 1000, 
                    MeasurementType  = "Temperature", 
                    GenerateData = GenerateTemperatureData },
                new Sensor { SensorId = "PressureSensor-001", 
                    Interval = 2000, 
                    MeasurementType = "Pressure",  
                    GenerateData = GeneratePressureData },
            };
        }

        public static double GenerateTemperatureData()
        {
            var random = new Random();
            return random.Next(100, 130); // temp from 0 to 130
        }

        public static double GeneratePressureData()
        {
            var random = new Random();
            return random.Next(1000, 1300); // Pressure from 1000 to 1300
        }
    }
}
