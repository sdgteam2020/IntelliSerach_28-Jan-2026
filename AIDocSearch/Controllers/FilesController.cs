using AIDocSearch.Helpers;
using AIDocSearch.Hubs;
using AIDocSearch.Services;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIDocSearch.Controllers
{
    public class FilesController : Controller
    {
        private readonly ConnectionManager _cm;
        private readonly IHubContext<CrawlerHub> _hub;
        private readonly ILogger<CentralFileServerController> _log;

        public FilesController(ConnectionManager cm, IHubContext<CrawlerHub> hub, ILogger<CentralFileServerController> log)
        {
            _cm = cm;
            _hub = hub;
            _log = log;
        }
        [HttpPost]
        public async Task<IActionResult> GetAllConnections()
        {
            var connections = _cm.GetAllConnections();
            if (connections.Count == 0)
                return BadRequest();
            return Ok(connections);
        }
        [HttpGet]
        public IActionResult GetCrawlerStatus(string crawlerId)
        {
            bool connected = _cm.TryGetConnectionId(crawlerId, out _);

            return Json(new
            {
                crawlerId = crawlerId,
                connected = connected
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetCrawlerStatus([FromBody] DTOFilesFromCrawlerRequest data)
        {
            if (!_cm.TryGetConnectionId(data.CrawlerId, out var connectionId))
            {
                return Ok(new
                {
                    Code = 400,
                    Message = "Indexer not connected"
                });
            }
            else
            {
                return Ok(new
                {
                    Code = 200,
                    Message = "Indexer connected"
                });
            }

        }
        [HttpPost]
        public async Task<IActionResult> GetFilesFromCrawler([FromBody]  DTOFilesFromCrawlerRequest data)
        {
            if (!_cm.TryGetConnectionId(data.CrawlerId, out var connectionId))
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "Crawler not connected"
                });
            }


            var (requestId, alreadyExists) =
                _cm.CreateRequest(data.CrawlerId, data.FileName);

            if(!alreadyExists)
            await _hub.Clients.Client(connectionId)
                .SendAsync("SendHelloRequest", requestId, data.FileName);


            RequestInfo? info = null;

            int maxAttempts = 12; // 12 x 10 sec = 2 minutes

            for (int i = 0; i < maxAttempts; i++)
            {
                _cm.TryGetRequest(requestId, out info);
               if (info!=null && !string.IsNullOrWhiteSpace(info.base64))
                {
                    break;

                }

                    await Task.Delay(TimeSpan.FromSeconds(2));
            }

            if (info == null)
            {
                return NotFound(new
                {
                    Code = 404,
                    Message = "File request not found after waiting"
                });
            }

            if (string.IsNullOrWhiteSpace(info.base64))
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "No file available"
                });
            }


            try
            {
                byte[] fileBytes = Convert.FromBase64String(info.base64);


                string fileName = string.IsNullOrWhiteSpace(info.FileName)
                    ? "download.bin"
                    : info.FileName;


                string contentType = GetContentType(fileName);


                // Add watermark only for PDF
                if (Path.GetExtension(fileName)
                        .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {

                    string clientIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                    var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(forwardedHeader))
                    {
                        clientIP = forwardedHeader
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ip => ip.Trim())
                            .FirstOrDefault()
                            ?? clientIP;
                    }

                    fileBytes = WaterMarks.AddPdfWatermark(fileBytes, clientIP);
                }

                // Return Blob response
                return File(fileBytes, contentType);

            }
            catch (FormatException)
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "Invalid Base64 data"
                });
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
