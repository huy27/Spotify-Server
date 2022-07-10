using Application.IService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBusQueueController : ControllerBase
    {
        private readonly IQueueService _queueService;

        public ServiceBusQueueController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        // GET: api/<ServiceBusQueueController>
        [HttpPost]
        public async Task<ActionResult> SendMessage(string message)
        {
            await _queueService.SenderAsync(message);
            return Ok();
        }

    }
}
