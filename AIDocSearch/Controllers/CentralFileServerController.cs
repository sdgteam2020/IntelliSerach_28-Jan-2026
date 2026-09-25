using AIDocSearch.Hubs;
using AIDocSearch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AIDocSearch.Controllers
{
    [ApiController]
    [Route("api/CentralFileServer")]
    public class CentralFileServerController : ControllerBase
    {
        private readonly ConnectionManager _cm;
        private readonly IHubContext<CrawlerHub> _hub;
        private readonly ILogger<CentralFileServerController> _log;

        public CentralFileServerController(ConnectionManager cm, IHubContext<CrawlerHub> hub, ILogger<CentralFileServerController> log)
        {
            _cm = cm;
            _hub = hub;
            _log = log;
        }
        [HttpGet("GetAllConnections")]
        public async Task<IActionResult> GetAllConnections()
        {
            var connections = _cm.GetAllConnections();
            return Ok(connections);
        }

        /// <summary>
        /// Create a hello request for a registered crawler. Returns requestId.
        /// </summary>
        [HttpPost("register/{crawlerId}/request-hello")]
        public async Task<IActionResult> RequestHello(string crawlerId, string FileName, CancellationToken ct)
        {
            if (!_cm.TryGetConnectionId(crawlerId, out var connectionId))
                return NotFound(new { error = "Crawler not connected" });

            var (requestId, alreadyExists) = _cm.CreateRequest(crawlerId, FileName);

            if (alreadyExists)
            {
                _log.LogInformation("Request for file {FileName} already exists as {RequestId}; not re-sending", FileName, requestId);
                return Ok(new { requestId, status = "AlreadyRequested" });
            }

            _log.LogInformation("Sending SendHelloRequest {RequestId} to crawler {CrawlerId} (conn {Conn})", requestId, crawlerId, connectionId);

            // Send request to specific connection
            await _hub.Clients.Client(connectionId).SendAsync("SendHelloRequest", requestId, FileName, ct);

            return Ok(new { requestId, status = "Requested" });
        }

        /// <summary>
        /// Query request status
        /// </summary>
        [HttpGet("request/{requestId}/file")]
        public IActionResult GetRequestFile(string requestId)
        {
            if (!_cm.TryGetRequest(requestId, out var info) || info is null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(info.base64))
                return BadRequest("No file available.");

            try
            {
                byte[] fileBytes = Convert.FromBase64String(info.base64);

                string fileName = string.IsNullOrWhiteSpace(info.FileName)
                    ? "download.bin"
                    : info.FileName;

                string contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 data.");
            }
        }


        private static string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tif" or ".tiff" => "image/tiff",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}
