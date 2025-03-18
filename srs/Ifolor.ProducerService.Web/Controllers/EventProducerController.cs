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
        public IActionResult Start()
        {
            _controlService.AppStartAsync();
            return Ok(new { Message = "Producer Service started", _controlService.IsRunning });
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            _controlService.AppStopAsync();
            return Ok(new { Message = "Producer Service stopped", _controlService.IsRunning });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { _controlService.IsRunning });
        }
    }
}
