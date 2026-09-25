using AIDocSearch.Services;
using Microsoft.AspNetCore.SignalR;

namespace AIDocSearch.Hubs
{
    public class CrawlerHub : Hub
    {
        private readonly ConnectionManager _cm;
        private readonly ILogger<CrawlerHub> _log;

        public CrawlerHub(ConnectionManager cm, ILogger<CrawlerHub> log)
        {
            _cm = cm;
            _log = log;
        }

        /// <summary>
        /// Client -> Server: register crawler id to connection id mapping.
        /// </summary>
        public Task RegisterCrawler(string crawlerId)
        {
            _log.LogInformation("RegisterCrawler: {CrawlerId} connected as {ConnectionId}", crawlerId, Context.ConnectionId);
            _cm.RegisterCrawler(crawlerId, Context.ConnectionId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Client -> Server: crawler replies with a hello message.
        /// </summary>
        public Task CrawlerHello(string crawlerId, string message, string base64)
        {
            _log.LogInformation("CrawlerHello from {CrawlerId}: {Message}", crawlerId, message);
            _cm.MarkHelloReceivedForCrawler(crawlerId, message, base64);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Receive a chunk of a Base64-encoded payload from a crawler.
        /// The worker sends chunks using the requestId supplied by the server when requesting a hello.
        /// </summary>
        public Task CrawlerHelloChunk(string crawlerId, string requestId, int index, int total, string chunk)
        {
            _log.LogInformation("CrawlerHelloChunk from {CrawlerId} request {RequestId} index {Index}/{Total}", crawlerId, requestId, index + 1, total);

            // prepare container for chunks
            _cm.EnsureChunkContainer(requestId, total);
            _cm.AddChunk(requestId, index, chunk);

            // Acknowledge receipt of this chunk back to the caller so the worker can proceed
            try
            {
                Clients.Caller.SendAsync("ChunkAck", requestId);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to send ChunkAck for request {RequestId}", requestId);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Signifies that the crawler finished sending all chunks for the request.
        /// Attempts to assemble the full Base64 payload into the request record.
        /// </summary>
        public Task CrawlerHelloComplete(string crawlerId, string requestId, int total, string fileName)
        {
            _log.LogInformation("CrawlerHelloComplete from {CrawlerId} request {RequestId} totalChunks {Total}", crawlerId, requestId, total);

            if (_cm.TryAssembleRequest(requestId, out var assembled, fileName))
            {
                _log.LogInformation("Assembled payload for request {RequestId}, length {Len}", requestId, assembled?.Length ?? 0);
            }
            else
            {
                _log.LogWarning("Failed to assemble payload for request {RequestId} - missing chunks?", requestId);
            }

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _log.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            _cm.RemoveByConnectionId(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
