using AutoMapper;
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
        private readonly IMapper _mapper;

        public ControlService(ISensorService sensorService, 
            IMessageProducer messageProducer,
            IOptions<RabbitMQConfig> rabbitMQConfig,
            IOptions<ProducerPolicy> producerPolicy,
            ILogger<ControlService> logger,
            IEventRepository eventRepository,
            IMapper mapper)
        {
            _sensorService = sensorService;
            _messageProducer = messageProducer;
            _rabbitMQConfig = rabbitMQConfig.Value;
            _producerPolicy = producerPolicy.Value;

            _logger = logger;
            _eventRepository = eventRepository;
            _mapper = mapper;
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
                await ResendEvents();
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
                    await SendToMessageBroker(sensorData);
                }
                catch (OperationCanceledException)
                {
                    // expected cancel
                    _logger.LogInformation("Delay was canceled for sensor {SensorId}", sensor.SensorId);
                    break;
                }
                finally
                {
                    try
                    {
                        // saving to SQLite
                        await _eventRepository.SaveSensorDataAsync(sensorData);
                        _logger.LogInformation("Saved event {EventId} to SQLite for sensor {SensorId}", sensorData.EventId, sensorData.SensorId);
                    }
                    catch (Exception ex)
                    {
                        // handle errors by saving to SQLite
                        _logger.LogError(ex, "Error saving event to SQLite for sensor {SensorId}", sensorData.SensorId);
                    }
                }
                _logger.LogInformation("RunAsync completed for sensor {SensorId}", sensor.SensorId);
                // sensor delay before receiving new data
                await Task.Delay(sensor.Interval, cancellationToken);
            }
        }

        private async Task SendToMessageBroker(SensorData sensorData)
        {
            try
            {
                // Send to RabbitMQ
                await _messageProducer.SendMessage(_rabbitMQConfig, sensorData);
                _logger.LogInformation("Published event {EventId} to RabbitMQ", sensorData.EventId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Event processing was canceled for sensor {SensorId}", sensorData.SensorId);
            }
            catch (BrokerUnreachableException ex)
            {
                sensorData.EventStatus = EventStatus.NotSend;
                _logger.LogError(ex, "Error processing event: BrokerUnreachableException for sensor {SensorId}", sensorData.SensorId);
            }
            catch (Exception ex)
            {
                sensorData.EventStatus = EventStatus.NotSend;
                _logger.LogError(ex, "Error processing event for sensor {SensorId}", sensorData.SensorId);
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

        public async Task ResendEvents()
        {
            var unsendEvents = await _eventRepository.GetUnsendMessages();
            var eventDataList = _mapper.Map<List<SensorEventEntity>, List<SensorData>>(unsendEvents);

            eventDataList.ForEach(async ev => await SendToMessageBroker(ev));
        }

    }
}
