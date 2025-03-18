namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public class ProducerPolicy
    {
        public int RetryLookbackSeconds { get; set; }

        public int ResendDelayInSeconds { get; set; }
    }
}
