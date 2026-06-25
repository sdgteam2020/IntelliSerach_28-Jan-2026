using AIDocSearch.Helpers;
using Domain.CommonModel;
using Microsoft.AspNetCore.Authorization;

namespace AIDocSearch.CustomMiddleware
{
   
        public class SessionCheckMiddleware
        {
            private readonly RequestDelegate _next;
            private static readonly PathString[] _allowList =
            {
            "/",
            "/Account/Login",
            "/Account/Register",
            "/Account/FinalLogin",
             "/Account/ContactUs",
            "/Account/Error",
            "/Account/AccessDenied"
,


        };

            public SessionCheckMiddleware(RequestDelegate next) => _next = next;

            public async Task InvokeAsync(HttpContext context)
            {
                var path = context.Request.Path;

                // 1) Skip static files and common asset paths
                if (path.StartsWithSegments("/css") ||
                    path.StartsWithSegments("/js") ||
                    path.StartsWithSegments("/img") ||
                    path.StartsWithSegments("/images") ||
                    path.StartsWithSegments("/lib") ||
                    path.StartsWithSegments("/favicon.ico")
                    )
                {
                    await _next(context);
                    return;
                }

                // 2) Skip allow-listed auth pages (GET & POST)
                if (_allowList.Any(p => path.StartsWithSegments(p)))
                {
                    await _next(context);
                    return;
                }

                // 3) Respect [AllowAnonymous] on actions/pages
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
                {
                    await _next(context);
                    return;
                }

                // 4) Check session
                var dtoSession = SessionHeplers.GetObject<DTOSession>(context.Session, "Users");
                if (dtoSession == null)
                {
                    // optional: carry returnUrl
                    var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                    context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                    return;
                }

                await _next(context);
            }
        }
    
}
