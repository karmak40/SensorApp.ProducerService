using IfolorProducerService.Core.Models;

namespace IfolorProducerService.Core.Services
{
    public interface ISensorService
    {
        IReadOnlyList<Sensor> GetSensors();
    }
}
