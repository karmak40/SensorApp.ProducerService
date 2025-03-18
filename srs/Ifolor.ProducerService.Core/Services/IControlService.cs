using IfolorProducerService.Core.Models;
using System.Collections.Concurrent;

namespace IfolorProducerService.Core.Services
{
    public interface IControlService
    {
        bool IsRunning { get; }
        public void AppStartAsync();
        public void AppStopAsync();
        public BlockingCollection<Sensor> GetSensorQueue();
    }
}
