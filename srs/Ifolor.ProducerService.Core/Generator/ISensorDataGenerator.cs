using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Models;

namespace IfolorProducerService.Core.Generator
{
    public interface ISensorDataGenerator
    {
        SensorData GenerateData(Sensor sensor);
    }
}
