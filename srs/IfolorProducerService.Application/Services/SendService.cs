using Ifolor.ProducerService.Infrastructure.Messaging;
using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;

namespace IfolorProducerService.Application.Services
{
    /// <summary>
    /// Service responsible for starting sending message to message broker and handle related errors
    /// </summary>
    public class SendService: ISendService
    {
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<SendService> _logger;

        public SendService(
            IMessageProducer messageProducer,
            ILogger<SendService> logger
        )
        {
            _messageProducer = messageProducer;
            _logger = logger;
        }

        public async Task SendToMessageBroker(SensorData sensorData)
        {
            int retryCount = 0;
            const int maxRetries = 3;
            const int initialDelayMs = 1000; // 1 second

            while (retryCount < maxRetries)
            {
                try
                {
                    await _messageProducer.SendMessage(sensorData);
                    _logger.LogInformation("Published event {EventId} to RabbitMQ", sensorData.EventId);
                    return; // Exit on success
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Event processing was canceled for sensor {SensorId}", sensorData.SensorId);
                    throw; // Re-throw to propagate cancellation
                }
                catch (BrokerUnreachableException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        sensorData.EventStatus = EventStatus.NotSend;
                        _logger.LogError(ex, "Failed to send event after {RetryCount} retries for sensor {SensorId}", retryCount, sensorData.SensorId);
                        break;
                    }

                    int delayMs = initialDelayMs * (int)Math.Pow(2, retryCount - 1);
                    _logger.LogWarning(ex, "Broker unreachable. Retrying in {DelayMs} ms for sensor {SensorId}", delayMs, sensorData.SensorId);
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    sensorData.EventStatus = EventStatus.NotSend;
                    _logger.LogError(ex, "Error processing event for sensor {SensorId}", sensorData.SensorId);
                    break;
                }
            }
        }
    }
}
