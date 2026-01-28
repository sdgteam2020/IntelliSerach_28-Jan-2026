namespace AIDocSearch.CustomMiddleware
{
    public class XssProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public XssProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            // You can skip some paths if needed (e.g. static files, health checks)
            // if (request.Path.StartsWithSegments("/health")) { await _next(context); return; }

            // 1) Check query string values
            foreach (var kv in request.Query)
            {
                if (XssHelper.ContainsXss(kv.Value))
                {
                    await BlockRequest(context, "Query parameter");
                    return;
                }
            }

            // 2) Check form fields for POST/PUT with form content
            if (HttpMethods.IsPost(request.Method) ||
                HttpMethods.IsPut(request.Method) ||
                HttpMethods.IsPatch(request.Method))
            {
                if (request.HasFormContentType)
                {
                    var form = await request.ReadFormAsync();
                    foreach (var kv in form)
                    {
                        if (XssHelper.ContainsXss(kv.Value))
                        {
                            await BlockRequest(context, "Form field");
                            return;
                        }
                    }
                }

                // (Optional) If you also use JSON bodies, you can read & scan Request.Body
                // with EnableBuffering – but since your screenshot shows
                // application/x-www-form-urlencoded, this part is not mandatory now.
            }

            // If everything is clean, continue the pipeline
            await _next(context);
        }

        private static async Task BlockRequest(HttpContext context, string source)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid input detected (" + source + ").");
        }
    }
}
