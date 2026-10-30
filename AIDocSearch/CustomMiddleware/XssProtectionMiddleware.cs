namespace AIDocSearch.CustomMiddleware
{
    public sealed class XssProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public XssProtectionMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            // Check query-string values.
            foreach (var queryParameter in request.Query)
            {
                if (XssHelper.ContainsXss(queryParameter.Value.ToString()))
                {
                    await BlockRequest(context);
                    return;
                }
            }

            // Check form fields for POST, PUT, and PATCH requests.
            if (HttpMethods.IsPost(request.Method) ||
                HttpMethods.IsPut(request.Method) ||
                HttpMethods.IsPatch(request.Method))
            {
                if (request.HasFormContentType)
                {
                    try
                    {
                        var form = await request.ReadFormAsync();

                        foreach (var formField in form)
                        {
                            if (XssHelper.ContainsXss(formField.Value.ToString()))
                            {
                                await BlockRequest(context);
                                return;
                            }
                        }
                    }
                    catch (InvalidDataException)
                    {
                        await BlockRequest(context);
                        return;
                    }
                    catch (BadHttpRequestException)
                    {
                        await BlockRequest(context);
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static async Task BlockRequest(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/plain; charset=utf-8";

            await context.Response.WriteAsync("Invalid request.");
        }
    }
}