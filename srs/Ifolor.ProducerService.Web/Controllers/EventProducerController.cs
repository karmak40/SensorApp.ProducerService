using IfolorProducerService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ifolor.ProducerService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventProducerController : ControllerBase
    {
        private readonly IControlService _controlService;
        private readonly ILogger<EventProducerController> _logger;

        public EventProducerController(
            IControlService controlService,
            ILogger<EventProducerController> logger)
        {
            _controlService = controlService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            await _controlService.AppStartAsync();
            return Ok(new { Message = "Producer Service started", IsRunning = _controlService.IsRunning });
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            await _controlService.AppStopAsync();
            return Ok(new { Message = "Producer Service stopped", IsRunning = _controlService.IsRunning });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { IsRunning = _controlService.IsRunning });
        }
    }
}
