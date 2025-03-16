using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace IfolorProducerService.Application.Services
{
    public class ControlService : IControlService
    {
        private readonly ISensorService _sensorService;
        private readonly IMessageProducer _messageProducer;
        private readonly RabbitMQConfig _rabbitMQConfig;
        private readonly ProducerPolicy _producerPolicy;
        private readonly ILogger<ControlService> _logger;
        private readonly IEventRepository _eventRepository;
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private Task[] _runningTasks;

        public ControlService(ISensorService sensorService, 
            IMessageProducer messageProducer,
            IOptions<RabbitMQConfig> rabbitMQConfig,
            IOptions<ProducerPolicy> producerPolicy,
            ILogger<ControlService> logger,
            IEventRepository eventRepository)
        {
            _sensorService = sensorService;
            _messageProducer = messageProducer;
            _rabbitMQConfig = rabbitMQConfig.Value;
            _producerPolicy = producerPolicy.Value;

            _logger = logger;
            _eventRepository = eventRepository;
        }
        public bool IsRunning => _isRunning;

        public async Task AppStartAsync()
        {
            var sensors = _sensorService.GetSensors();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _isRunning = true;

            _logger.LogInformation("Producer started");

            _runningTasks = sensors.Select(sensor => RunAsync(sensor, token)).ToArray();

            // checking are the not send messages, if yes try to send again
            var task = RunPeriodically(async () => 
            {

                Console.WriteLine("Метод выполняется в: " + DateTime.UtcNow);

                // here read unsend events and try to send them again to rabbitMQ

            }, TimeSpan.FromSeconds(_producerPolicy.ResendDelayInSeconds), _cts.Token);
        }

        public async Task AppStopAsync()
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Producer is not running");
                return;
            }

            _cts.Cancel();
            await Task.WhenAll(_runningTasks);

            _isRunning = false;
            _logger.LogInformation("Producer stopped");
        }

        private async Task RunAsync(Sensor sensor, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var measurements = sensor.GenerateData();
                var sensorData = new SensorData
                {
                    SensorId = sensor.SensorId,
                    Timestamp = DateTime.UtcNow,
                    MeasurementType = sensor.MeasurementType,
                    MeasurementValue = measurements,
                    EventStatus = EventStatus.Send
                };
                try
                {
                    // Send to RabbitMQ
                    var message = JsonSerializer.Serialize(sensorData);
                    await _messageProducer.SendMessage(_rabbitMQConfig, message);
                    _logger.LogInformation("Published event {EventId} to RabbitMQ", sensorData.EventId);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Event processing was canceled for sensor {SensorId}", sensor.SensorId);
                    break;
                }
                catch (BrokerUnreachableException ex)
                {
                    sensorData.EventStatus = EventStatus.NotSend;
                    _logger.LogError(ex, "Error processing event: BrokerUnreachableException for sensor {SensorId}", sensor.SensorId);
                }
                catch (Exception ex)
                {
                    // handle other exceptions
                    sensorData.EventStatus = EventStatus.NotSend;
                    _logger.LogError(ex, "Error processing event for sensor {SensorId}", sensor.SensorId);
                }
                finally
                {
                    try
                    {
                        // saving to SQLite
                        await _eventRepository.SaveSensorDataAsync(sensorData);
                        _logger.LogInformation("Saved event {EventId} to SQLite for sensor {SensorId}", sensorData.EventId, sensor.SensorId);
                    }
                    catch (Exception ex)
                    {
                        // handle errors by saving to SQLite
                        _logger.LogError(ex, "Error saving event to SQLite for sensor {SensorId}", sensor.SensorId);
                    }
                }

                try
                {
                    // sensor delay before receiving new data
                    await Task.Delay(sensor.Interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // expected cancel
                    _logger.LogInformation("Delay was canceled for sensor {SensorId}", sensor.SensorId);
                    break;
                }
                _logger.LogInformation("RunAsync completed for sensor {SensorId}", sensor.SensorId);

            }
        }


        public async Task RunPeriodically(Func<Task> method, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                var hasUnsend = await _eventRepository.HasNotSentMessages();

                if (hasUnsend)
                {
                    await method(); 
                }

                await Task.Delay(interval, cancellationToken); // Ждем указанный интервал
            }
        }

    }
}
