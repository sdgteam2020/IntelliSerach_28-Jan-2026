using AIDocSearch.CustomMiddleware;
using BusinessLogicsLayer.Account;
using BusinessLogicsLayer.Loger;
using BusinessLogicsLayer.Logers;
using BusinessLogicsLayer.Ranks;
using BusinessLogicsLayer.ScraperAPI; // Ensure this is included for 'UseExceptionProcessor'
using BusinessLogicsLayer.SearchContent;
using BusinessLogicsLayer.Service;
using BusinessLogicsLayer.UnitOfWorks;
using BusinessLogicsLayer.UploadPdf;
using DataAccessLayer;
using DataAccessLayer.Account;
using DataAccessLayer.Logers;
using DataAccessLayer.UploadFiles;
using DataTransferObject.IdentityModel;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Ensure this is included
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configration = builder.Configuration;
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(configration.GetConnectionString("DefaultConnection")).UseExceptionProcessor());
//builder.Services.AddHttpClient();

builder.Services.AddHttpClient("NoSSL")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 8;
    opts.Password.RequiredUniqueChars = 1;
    opts.User.RequireUniqueEmail = false;

    // Lockout
    opts.Lockout.AllowedForNewUsers = true;
    opts.Lockout.MaxFailedAccessAttempts = 3;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict; // use Lax/None if you have external IdP redirects
    // Ensure immediate revalidation
    o.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});
// Antiforgery cookie hardened (__Host- prefix requires Secure + path "/" + no Domain)
builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "__Host-AntiForgery";

    //  o.Cookie.HttpOnly = true;
    // o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.Path = "/"; // required for __Host- cookies
    // o.Cookie.SameSite = SameSiteMode.Strict; // enable if you don't post from cross-site contexts
});// Identity application cookie hardening

// Session cookie hardened
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = ".Docs.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Get allowed origins from configuration
            var allowedOrigins = new[]
            {
                "*",
                "https://admin.example.com"
            };
            // Always allow localhost in development
            if (builder.Environment.IsDevelopment() &&
                origin.StartsWith("http://localhost"))
            {
                return true;
            }

            // Check against configured list
            return allowedOrigins.Contains(origin) ||
                   allowedOrigins.Contains("*");
        });

        // Only allow specific methods
        policy.WithMethods("GET", "POST", "PUT");

        // Only allow specific headers
        policy.WithHeaders("Authorization", "Content-Type", "X-Requested-With");

        // Allow credentials (cookies, auth headers)
        policy.AllowCredentials();

        // Cache preflight for 1 hour
        policy.SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

builder.Services.AddRepository();

builder.Services.AddScoped<ISearch, Search>();
builder.Services.AddScoped<IAccount, Account>();
builder.Services.AddScoped<IAccountDL, AccountDL>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRank, Rank>();
builder.Services.AddScoped<IAPI, API>();
builder.Services.AddScoped<IService, ServiceRepository>();
builder.Services.AddScoped<IUploadFiles, UploadFiles>();
builder.Services.AddScoped<IUploadFilesDB, UploadFilesDB>();

builder.Services.AddScoped<ILoger, Loger>();
builder.Services.AddScoped<ILogerDB, LogerDB>();

// Session cookie hardened
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = ".WebAnalytics.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Use the default property (Pascal) casing
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    //ctx.Response.OnStarting(() =>
    //{
    //    ctx.Response.Headers.Remove("Expires");

    //    return Task.CompletedTask;
    //});
    //// ========== 0) BLOCK DANGEROUS HTTP METHODS FIRST ==========
    //var blockedMethods = new[] { "OPTIONS", "TRACE", "TRACK", "CONNECT" };

    //if (blockedMethods.Contains(ctx.Request.Method, StringComparer.OrdinalIgnoreCase))
    //{
    //    // Log for monitoring (optional)
    //    app.Logger.LogWarning($"Security: Blocked {ctx.Request.Method} request to {ctx.Request.Path}");

    //    ctx.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
    //    ctx.Response.Headers["Allow"] = "GET, HEAD, POST"; // Only allowed methods
    //    await ctx.Response.WriteAsync("Method Not Allowed");
    //    return; // Stop further processing
    //}

    //// 1) Content Security Policy
    //ctx.Response.Headers["Content-Security-Policy"] =
    //    //"default-src 'self'; " +
    //    "script-src 'self'; " +
    //    "style-src 'self'; " + // allow Bootstrap inline styles
    //    "img-src 'self' data:; " +
    //    "font-src 'self' data:; " +
    //    //"connect-src 'self'; " +
    //    "frame-ancestors 'self'; " +
    //    "base-uri 'self'; " +
    //    "form-action 'self';";

    //// 2) X-Frame-Options (align with frame-ancestors)
    //ctx.Response.Headers["X-Frame-Options"] = "DENY";

    //// 3) Referrer-Policy
    //ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    //// Extra good headers
    //ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    //ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    //// Use HSTS only on HTTPS + production
    //ctx.Response.Headers["Strict-Transport-Security"] =
    //    "max-age=31536000; includeSubDomains; preload";

    //// Hide tech details where possible
    //ctx.Response.Headers.Remove("X-Powered-By");
    //ctx.Response.Headers.Remove("x-aspnet-version");

    //ctx.Request.PathBase = "/IntelliSearch";

    await next();
});
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});
app.UseHttpsRedirection();
app.UsePathBase("/IntelliSearch");
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
// Session BEFORE endpoints (so MVC/Razor can use it)
app.UseSession();
app.UseMiddleware<GlobalExceptionMiddleware>();
//app.UseMiddleware<XssProtectionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();