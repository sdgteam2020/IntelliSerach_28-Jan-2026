using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIDocSearch.CustomMiddleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;
        private readonly int _thresholdMs = 300; // log requests slower than this

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
                return Task.CompletedTask;
            });

            await _next(context);

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            if (elapsed > _thresholdMs)
            {
                _logger.LogWarning("Slow request {method} {path} took {ms} ms", context.Request.Method, context.Request.Path, elapsed);
            }
        }
    }
}
