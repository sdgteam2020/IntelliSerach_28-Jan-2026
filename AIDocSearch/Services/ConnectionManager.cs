using System.Collections.Concurrent;

namespace AIDocSearch.Services
{
    public class ConnectionManager : IDisposable
    {
        // crawlerId -> connectionId
        private readonly ConcurrentDictionary<string, string> _crawlers = new();

        // requestId -> RequestInfo
        private readonly ConcurrentDictionary<string, RequestInfo> _requests = new();
        // temporary storage for chunked base64 pieces: requestId -> string[]
        private readonly ConcurrentDictionary<string, string?[]> _chunks = new();

        // Expiry and cleanup configuration
        private readonly TimeSpan _requestTtl;
        private readonly TimeSpan _cleanupInterval;
        private readonly CancellationTokenSource _cts;
        private readonly Task _cleanupTask;

        public ConnectionManager(TimeSpan? requestTtl = null, TimeSpan? cleanupInterval = null)
        {
            _requestTtl = requestTtl ?? TimeSpan.FromSeconds(50);
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromSeconds(30);
            _cts = new CancellationTokenSource();
            _cleanupTask = Task.Run(() => CleanupLoopAsync(_cts.Token));
        }

        public bool TryGetConnectionId(string crawlerId, out string connectionId) =>
            _crawlers.TryGetValue(crawlerId, out connectionId);

        public void RegisterCrawler(string crawlerId, string connectionId)
        {
            _crawlers[crawlerId] = connectionId;
        }
        public ConcurrentDictionary<string, string> GetAllConnections()
        {
            return _crawlers;
        }
        public void RemoveByConnectionId(string connectionId)
        {
            var kv = _crawlers.FirstOrDefault(p => p.Value == connectionId);
            if (!string.IsNullOrEmpty(kv.Key))
            {
                _crawlers.TryRemove(kv.Key, out _);
            }
        }

        /// <summary>
        /// Create a request. If a request already exists for the same file name (and is not expired/timeout),
        /// return the existing requestId instead of creating a new one. The boolean indicates whether the
        /// returned id already existed (true) or was newly created (false).
        /// </summary>
        public (string RequestId, bool AlreadyExists) CreateRequest(string crawlerId, string? fileName = null)
        {
            // If caller provided a fileName, try to find an existing request for that file.
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var now = DateTime.UtcNow;
                var match = _requests
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.FileName)
                                 && string.Equals(kv.Value.FileName, fileName, StringComparison.OrdinalIgnoreCase)
                                 && kv.Value.Status != RequestStatus.Timeout
                                 && kv.Value.ExpiresUtc > now)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(match.Key))
                {
                    return (match.Key, true);
                }
            }

            var requestId = Guid.NewGuid().ToString("D");
            var created = DateTime.UtcNow;
            var ri = new RequestInfo
            {
                RequestId = requestId,
                CrawlerId = crawlerId,
                Status = RequestStatus.Requested,
                CreatedUtc = created,
                ExpiresUtc = created + _requestTtl,
                FileName = fileName ?? string.Empty
            };
            _requests[requestId] = ri;
            return (requestId, false);
        }

        public bool TryGetRequest(string requestId, out RequestInfo? info)
        {
            if (_requests.TryGetValue(requestId, out info))
            {
                // Check if expired
                if (info.ExpiresUtc <= DateTime.UtcNow)
                {
                    // Mark as timeout and remove
                    info.Status = RequestStatus.Timeout;
                    _requests.TryRemove(requestId, out _);
                    _chunks.TryRemove(requestId, out _);
                    info = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        public void EnsureChunkContainer(string requestId, int total)
        {
            _chunks.TryAdd(requestId, new string?[total]);
        }

        public void AddChunk(string requestId, int index, string chunk)
        {
            if (_chunks.TryGetValue(requestId, out var arr))
            {
                if (index >= 0 && index < arr.Length)
                {
                    arr[index] = chunk;
                }
            }
        }

        public bool TryAssembleRequest(string requestId, out string? assembledBase64, string fileName)
        {
            assembledBase64 = null;
            if (!_requests.TryGetValue(requestId, out var info)) return false;
            if (!_chunks.TryGetValue(requestId, out var arr)) return false;

            // ensure all chunks present
            if (arr.Any(s => s is null)) return false;

            assembledBase64 = string.Concat(arr!);
            info.base64 = assembledBase64;
            info.FileName = fileName;
            info.Status = RequestStatus.Received;
            info.ReceivedUtc = DateTime.UtcNow;
            _requests[requestId] = info;

            _chunks.TryRemove(requestId, out _);
            return true;
        }

        /// <summary>
        /// Called when a crawler sends CrawlerHello. We map to the oldest pending request for that crawler.
        /// </summary>
        public void MarkHelloReceivedForCrawler(string crawlerId, string message, string base64)
        {
            var match = _requests
                .Where(kv => kv.Value.CrawlerId == crawlerId && kv.Value.Status == RequestStatus.Requested)
                .OrderBy(kv => kv.Value.CreatedUtc)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(match.Key))
            {
                var info = match.Value;
                info.Status = RequestStatus.Received;
                info.Message = message;
                info.base64 = base64;
                info.ReceivedUtc = DateTime.UtcNow;
                _requests[match.Key] = info;
            }
        }

        private async Task CleanupLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, ct);

                    var now = DateTime.UtcNow;
                    var expiredRequests = _requests
                        .Where(kv => kv.Value.ExpiresUtc <= now)
                        .ToList();

                    foreach (var kv in expiredRequests)
                    {
                        var requestId = kv.Key;
                        var info = kv.Value;

                        // Mark as timeout and remove from dictionaries
                        info.Status = RequestStatus.Timeout;
                        _requests.TryRemove(requestId, out _);
                        _chunks.TryRemove(requestId, out _);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch
                {
                    // Suppress other exceptions to keep the loop running
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _cleanupTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore exceptions during cleanup task wait
            }
            _cts.Dispose();
        }
    }

    public record RequestInfo
    {
        public string RequestId { get; init; } = default!;
        public string CrawlerId { get; init; } = default!;
        public RequestStatus Status { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public DateTime? ReceivedUtc { get; set; }
        public string base64 { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public TaskCompletionSource<bool> Completion { get; set; } =
       new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public enum RequestStatus
    {
        Requested,
        Received,
        Timeout
    }

}
