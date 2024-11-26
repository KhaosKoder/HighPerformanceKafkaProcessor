using Microsoft.AspNetCore.Mvc;
using HighPerformanceKafkaProcessor.Management;
using HighPerformanceKafkaProcessor.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;


namespace HighPerformanceKafkaProcessor.Api
{

    [ApiController]
    [Route("api/kafka/groups")]
    public class KafkaManagementController : ControllerBase
    {
        private readonly ITopicGroupManager _groupManager;
        private readonly ILogger<KafkaManagementController> _logger;

        public KafkaManagementController(
            ITopicGroupManager groupManager,
            ILogger<KafkaManagementController> logger)
        {
            _groupManager = groupManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicGroupStatus>>> GetAllGroups()
        {
            var statuses = await _groupManager.GetAllGroupStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("{groupName}")]
        public async Task<ActionResult<TopicGroupStatus>> GetGroup(string groupName)
        {
            var status = await _groupManager.GetGroupStatusAsync(groupName);
            if (status == null)
            {
                return NotFound();
            }
            return Ok(status);
        }

        [HttpPost("{groupName}/start")]
        public async Task<IActionResult> StartGroup(string groupName)
        {
            var result = await _groupManager.StartGroupAsync(groupName);
            if (!result)
            {
                return BadRequest("Failed to start group. Check logs for details.");
            }
            return Ok();
        }

        [HttpPost("{groupName}/stop")]
        public async Task<IActionResult> StopGroup(string groupName)
        {
            var result = await _groupManager.StopGroupAsync(groupName);
            if (!result)
            {
                return BadRequest("Failed to stop group. Check logs for details.");
            }
            return Ok();
        }
    }
}