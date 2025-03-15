using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace IfolorProducerService.Application.Services
{
    public class ControlService : IControlService
    {
        private readonly ISensorService _sensorService;
        private readonly IMessageProducer _messageProducer;
        private readonly RabbitMQConfig _rabbitMQConfig;
        private readonly ILogger<ControlService> _logger;
        private readonly IEventRepository _eventRepository;
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private Task[] _runningTasks;

        public ControlService(ISensorService sensorService, 
            IMessageProducer messageProducer,
            IOptions<RabbitMQConfig> rabbitMQConfig,
            ILogger<ControlService> logger,
            IEventRepository eventRepository)
        {
            _sensorService = sensorService;
            _messageProducer = messageProducer;
            _rabbitMQConfig = rabbitMQConfig.Value;
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

            _runningTasks = sensors.Select(sensor => Task.Run(() => RunAsync(sensor, token))
                 .ContinueWith(task =>
                 {
                     if (task.IsFaulted)
                     {
                         _logger.LogError(task.Exception, "Error in sensor task");
                     }
                 })).ToArray();
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
                try
                {
                    var measurements = sensor.GenerateData();
                    var sensorData = new SensorData
                    {
                        SensorId = sensor.SensorId,
                        Timestamp = DateTime.UtcNow,
                        MeasurementType = sensor.MeasurementType,
                        MeasurementValue = measurements
                    };

                    // Saving to SQLite
                    await _eventRepository.SaveSensorDataAsync(sensorData);
                    _logger.LogInformation("Saved event {EventId} to SQLite", sensorData.EventId);

                    // Send to RabbitMQ
                    var message = JsonSerializer.Serialize(sensorData);
                    await _messageProducer.SendMessage(_rabbitMQConfig, message);
                    _logger.LogInformation("Published event {EventId} to RabbitMQ", sensorData.EventId);

                    await Task.Delay(sensor.Interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break; // expected stop
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
    }
}
