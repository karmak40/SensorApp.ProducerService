namespace IfolorProducerService.Core.Services
{
    public interface IControlService
    {
        bool IsRunning { get; }
        public Task AppStartAsync();
        public Task AppStopAsync();
    }
}
