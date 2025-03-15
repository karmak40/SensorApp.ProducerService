using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Core.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace IfolorProducerService.Application.Services
{
    public class EventProducerService : IEventProducerService
    {
        private readonly IEventGenerator _eventGenerator;
        private readonly IMessageProducer _messageProducer;
        private readonly IEventRepository _eventRepository; // Добавляем репозиторий
        private readonly ILogger<EventProducerService> _logger;
        private readonly RabbitMQConfig _rabbitMQConfig;
        private readonly int _delayMs;
        private CancellationTokenSource _cts;
        private Task _runningTask;
        private bool _isRunning;

        public EventProducerService(
            IEventGenerator eventGenerator,
            IMessageProducer messageProducer,
            IEventRepository eventRepository, // Добавляем в конструктор
            IOptions<RabbitMQConfig> rabbitMQConfig,
            ILogger<EventProducerService> logger,
            IConfiguration configuration)
        {
            _eventGenerator = eventGenerator;
            _messageProducer = messageProducer;
            _eventRepository = eventRepository;
            _logger = logger;
            _rabbitMQConfig = rabbitMQConfig.Value;
            _delayMs = configuration.GetValue<int>("Producer:DelayMs", 1000);
        }

        public bool IsRunning => _isRunning;

        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("Producer is already running");
                return;
            }

            _cts = new CancellationTokenSource();
            _isRunning = true;
            _runningTask = RunAsync(_cts.Token);
            _logger.LogInformation("Producer started");
        }

        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Producer is not running");
                return;
            }

            _cts.Cancel();
            await _runningTask;
            _isRunning = false;
            _logger.LogInformation("Producer stopped");
        }


        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var @event = await _eventGenerator.GenerateAsync();

                    // Сохраняем в SQLite
                    await _eventRepository.SaveEventAsync(@event);
                    _logger.LogInformation("Saved event {EventId} to SQLite", @event.EventId);

                    // Отправляем в RabbitMQ
                    var message = JsonSerializer.Serialize(@event);
                    await _messageProducer.SendMessage(_rabbitMQConfig, message);
                    _logger.LogInformation("Published event {EventId} to RabbitMQ", @event.EventId);

                    await Task.Delay(_delayMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Ожидаемая остановка
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
