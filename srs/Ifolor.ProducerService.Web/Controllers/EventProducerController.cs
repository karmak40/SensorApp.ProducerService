using IfolorProducerService.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ifolor.ProducerService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventProducerController : ControllerBase
    {
        private readonly IEventProducerService _producerService;
        private readonly ILogger<EventProducerController> _logger;

        public EventProducerController(
            IEventProducerService producerService,
            ILogger<EventProducerController> logger)
        {
            _producerService = producerService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            await _producerService.StartAsync();
            return Ok(new { Message = "Event producer started", IsRunning = _producerService.IsRunning });
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            await _producerService.StopAsync();
            return Ok(new { Message = "Event producer stopped", IsRunning = _producerService.IsRunning });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { IsRunning = _producerService.IsRunning });
        }
    }
}
