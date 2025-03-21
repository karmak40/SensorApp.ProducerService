using AutoMapper;
using Ifolor.ProducerService.Core.Models;
using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;

namespace IfolorProducerService.Application.Services
{
    public class ResendService: IResendService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly ISendService _sendService;
        private readonly ILogger<ResendService> _logger;

        public ResendService(
            IEventRepository eventRepository, 
            IMapper mapper, 
            ISendService sendService,
            ILogger<ResendService> logger
            )
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _sendService = sendService;
            _logger = logger;
        }

        public void HandleUnsendMessages(int resendDelayInSeconds, CancellationToken token)
        {
            // with 0 we disable sending failed messages
            if (resendDelayInSeconds == 0) {
                _logger.LogInformation("Resend is disabled because resendDelayInSeconds is {ResendDelayInSeconds}", resendDelayInSeconds);
                return;
            }

            // checking are the not send messages, if yes try to send again
            var task = RunPeriodically(async () =>
            {
                // here read unsend events and try to send them again to rabbitMQ
                var unsendEvents = await _eventRepository.GetUnsendMessages();
                var eventDataList = _mapper.Map<List<SensorEventEntity>, List<SensorData>>(unsendEvents);

                await ResendEvents(eventDataList);

            }, TimeSpan.FromSeconds(resendDelayInSeconds), token);
        }

        private async Task RunPeriodically(Func<Task> method, TimeSpan interval, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var hasUnsend = await _eventRepository.HasNotSentMessages();

                    if (hasUnsend)
                    {
                        await method();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in periodic task");
                }

                await Task.Delay(interval, token); 
            }
        }

        private async Task ResendEvents(List<SensorData> eventDataList)
        {
            var tasks = eventDataList.Select(async ev =>
            {
                try
                {
                    await _sendService.SendToMessageBroker(ev);

                    ev.EventStatus = EventStatus.Send;
                    await _eventRepository.UpdateEventStatusAsync(ev.EventId, EventStatus.Send);

                    _logger.LogInformation("Resent event {EventId} for sensor {SensorId}", ev.EventId, ev.SensorId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resending event {EventId} for sensor {SensorId}", ev.EventId, ev.SensorId);
                }
            });

            await Task.WhenAll(tasks);
        }

    }
}
