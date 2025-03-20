using AutoMapper;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Core.Generator;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace IfolorProducerService.Application.Services
{
    public class ControlService : IControlService
    {
        private BlockingCollection<Sensor> _sensorQueue = new BlockingCollection<Sensor>();

        private readonly ISendService _sendService;
        private readonly IResendService _resendService;
        private readonly ISensorService _sensorService;
        private readonly IEventRepository _eventRepository;
        private readonly ISensorDataGenerator _sensorDataGenerator;

        private readonly ProducerPolicy _producerPolicy;
        private readonly ILogger<ControlService> _logger;

        private CancellationTokenSource _cts;
        private bool _isRunning;

        public ControlService(
            ISendService sendService,
            IResendService resendService,
            ISensorService sensorService, 
            IMessageProducer messageProducer,
            ISensorDataGenerator sensorDataGenerator,
            IOptions<ProducerPolicy> producerPolicy,
            ILogger<ControlService> logger,
            IEventRepository eventRepository,
            IMapper mapper)
        {
            _sendService = sendService;
            _resendService = resendService;
            _sensorService = sensorService;
            _producerPolicy = producerPolicy.Value;
            _sensorDataGenerator = sensorDataGenerator;
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

            // Start the sensor processing task
            _ = Task.Run(() => ProcessSensorsAsync(sensors, 5, token), token);

            // Start the unsend messages handling task
            _ = Task.Run(() => _resendService.HandleUnsendMessages(30, token), token);

            // Await both tasks to ensure they run concurrently
            // await Task.WhenAll(sensorTask, resendTask);
            await Task.Delay(500); // todo replace which tasks
        }

        public async Task AppStopAsync()
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Producer is not running");
                return;
            }

            // Signal cancellation
            _cts.Cancel();

            try
            {
                await Task.Delay(500); // Small delay to allow tasks to react to cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while stopping the producer.");
            }
            finally
            {
                // Ensure the state is updated even if an exception occurs
                _isRunning = false;
                _logger.LogInformation("Producer stopped");
            }
        }



        public async Task ProcessSensorsAsync(IEnumerable<Sensor> sensors, int numberOfWorkers, CancellationToken token)
        {
            _sensorQueue = new BlockingCollection<Sensor>();
            foreach (var sensor in sensors)
            {
                _sensorQueue.Add(sensor, token);
            }
            _sensorQueue.CompleteAdding();

            // Start worker tasks
            var workers = Enumerable.Range(0, numberOfWorkers)
                                    .Select(_ => Task.Run(() => ProcessSensorAsync(token), token))
                                    .ToArray();

            await Task.WhenAll(workers);
        }

        private async Task ProcessSensorAsync(CancellationToken token)
        {
            foreach (var sensor in _sensorQueue.GetConsumingEnumerable(token))
            {
                await RunAsync(sensor, token);
            }
        }

        private async Task RunAsync(Sensor sensor, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SensorData sensorData = _sensorDataGenerator.GenerateData(sensor);

                try
                {
                    await _sendService.SendToMessageBroker(sensorData);
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

        public BlockingCollection<Sensor> GetSensorQueue()
        {
            return _sensorQueue;
        }
    }
}
