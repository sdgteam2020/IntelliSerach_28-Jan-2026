using Domain.DTOs.Requests;
using Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIDocSearch.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LogBuffer _logBuffer;

        public LogsController(LogBuffer logBuffer)
        {
            _logBuffer = logBuffer;
        }
    
        [HttpPost("CrawlFilesLogs")]
        public IActionResult CrawlFilesLogs([FromBody] DTOLogEntryRequest data)
        {
            if (data == null)
                return BadRequest();

            _logBuffer.Enqueue(data);

            return Accepted();
        }
    }
}
