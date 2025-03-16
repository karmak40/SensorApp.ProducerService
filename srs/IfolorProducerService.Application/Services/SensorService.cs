using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;
using System;

namespace IfolorProducerService.Application.Services
{
    public class SensorService: ISensorService
    {
        private static readonly Random _random = new();

        public IReadOnlyList<Sensor> GetSensors()
        {
            return new List<Sensor>
            {
                new Sensor { SensorId = "TemperatureSensor-001", 
                    Interval = 1000, 
                    MeasurementType  = MeasurementType.Temperature,
                    GenerateData = () => GenerateSensorData(100, 130) 
                },
                //new Sensor { SensorId = "PressureSensor-001", 
                //    Interval = 2000, 
                //    MeasurementType = MeasurementType.Pressure,
                //    GenerateData = () => GenerateSensorData(1000, 1300) },
            }.AsReadOnly();
        }

        private static double GenerateTemperatureData()
        {
            var random = new Random();
            return random.Next(100, 130); // temp from 0 to 130
        }

        private static double GeneratePressureData()
        {
            var random = new Random();
            return random.Next(1000, 1300); // Pressure from 1000 to 1300
        }

        private double GenerateSensorData(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
