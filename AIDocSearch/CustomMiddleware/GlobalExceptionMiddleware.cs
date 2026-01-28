using AIDocSearch.Helpers;
using BusinessLogicsLayer.Loger;
using DataTransferObject.CommonModel;
using DataTransferObject.DTO.Requests;

namespace AIDocSearch.CustomMiddleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        private static readonly PathString[] _noRedirect =
        {
        "/Account/Login",
        "/Account/FinalLogin",
        "/Account/Logout",

        };

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;

        }

        //public async Task InvokeAsync(HttpContext context, IExceptionLogService logService)
        public async Task InvokeAsync(HttpContext context, ILoger _iloger)
        {

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                var endpoint = context.GetEndpoint();
                var routeVals = context.GetRouteData()?.Values;
                var controller = routeVals != null && routeVals.TryGetValue("controller", out var c) ? c?.ToString() : null;
                var action = routeVals != null && routeVals.TryGetValue("action", out var a) ? a?.ToString() : null;

                var dto = SessionHeplers.GetObject<DTOSession>(context.Session, "Users");

                var log = new DTOExceptionLogRequest
                {
                    OccurredAtUtc = DateTime.UtcNow,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Endpoint = endpoint?.DisplayName,
                    Controller = controller,
                    Action = action,
                    HttpMethod = context.Request.Method,
                    Path = context.Request.Path.Value ?? string.Empty,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    SessionUser = dto?.UserName,
                    Roles = dto?.RoleName,
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString()
                };

                var data = await _iloger.AddAsync(log);


                // Different behavior for API/AJAX vs MVC
                if (IsApiRequest(context))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "An unexpected error occurred.",

                    });
                    return;
                }

                // Avoid redirect loops on login endpoints
                var path = context.Request.Path;
                if (_noRedirect.Any(p => path.StartsWithSegments(p)))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
                    return;
                }

                // Redirect to login (or an error page if you prefer)
                context.Response.Clear();
                context.Response.Redirect("/Account/Error/" + data.Data);
                return;
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api")) return true;

            var xhr = context.Request.Headers["X-Requested-With"].ToString();
            if (string.Equals(xhr, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return true;

            var accept = context.Request.Headers["Accept"].ToString();
            if (!string.IsNullOrEmpty(accept) && accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
