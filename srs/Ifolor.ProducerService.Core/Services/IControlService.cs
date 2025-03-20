using IfolorProducerService.Core.Models;
using System.Collections.Concurrent;

namespace IfolorProducerService.Core.Services
{
    public interface IControlService
    {
        bool IsRunning { get; }
        Task AppStartAsync();
        Task AppStopAsync();
        public BlockingCollection<Sensor> GetSensorQueue();
    }
}
