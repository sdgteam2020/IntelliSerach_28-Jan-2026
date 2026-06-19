using AIDocSearch.CustomMiddleware;
using AIDocSearch.Services;
using Domain.IdentityEntities;
using EntityFramework.Exceptions.SqlServer;
using Infrastructure;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Ensure this is included
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using Infrastructure.Repository;

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
// Force immediate security-stamp validation on every request (small installs/dev only).
// NOTE: Setting to TimeSpan.Zero will validate the security stamp on each request and
// can increase DB load. Use a small interval in production if needed (e.g., 1 minute).
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});
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
builder.Services.AddSelfInfrastructure();
builder.Services.AddInfrastructure();
builder.Services.AddSharedInfrastructure();

// Register local application services

builder.Services.AddHttpContextAccessor();
// register current user service (implementation in AIDocSearch.Services)


    

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

// Run database seeder at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Ensure database schema is up-to-date (creates Identity tables like
        // AspNetUserClaims, AspNetUserLogins, AspNetUserTokens if migrations include them)
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database migration failed during startup.");
            throw;
        }

        // Seed data (synchronously wait during startup)
        var loggerFactory = services.GetService<ILoggerFactory>();
        var seederLogger = loggerFactory?.CreateLogger("DbSeeder");
        DbSeeder.SeedAsync(userManager, roleManager, db, seederLogger).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        ctx.Response.Headers.Remove("Expires");

        return Task.CompletedTask;
    });
    // ========== 0) BLOCK DANGEROUS HTTP METHODS FIRST ==========
    var blockedMethods = new[] { "OPTIONS", "TRACE", "TRACK", "CONNECT" };

    if (blockedMethods.Contains(ctx.Request.Method, StringComparer.OrdinalIgnoreCase))
    {
        // Log for monitoring (optional)
        app.Logger.LogWarning($"Security: Blocked {ctx.Request.Method} request to {ctx.Request.Path}");

        ctx.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        ctx.Response.Headers["Allow"] = "GET, HEAD, POST"; // Only allowed methods
        await ctx.Response.WriteAsync("Method Not Allowed");
        return; // Stop further processing
    }

    // 1) Content Security Policy
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " + // allow Bootstrap inline styles
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self';";

    // 2) X-Frame-Options (align with frame-ancestors)
    ctx.Response.Headers["X-Frame-Options"] = "DENY";

    // 3) Referrer-Policy
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Extra good headers
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Use HSTS only on HTTPS + production
    ctx.Response.Headers["Strict-Transport-Security"] =
        "max-age=31536000; includeSubDomains; preload";

    // Hide tech details where possible
    ctx.Response.Headers.Remove("X-Powered-By");
    ctx.Response.Headers.Remove("x-aspnet-version");

    ctx.Request.PathBase = "/";

    await next();
});
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});
app.UseHttpsRedirection();
app.UsePathBase("/IntelliSearch");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year

        ctx.Context.Response.Headers.Append(
            "Cache-Control",
            $"public,max-age={durationInSeconds}");
    }
}); 
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
// Session BEFORE endpoints (so MVC/Razor can use it)
app.UseSession();
app.UseMiddleware<GlobalExceptionMiddleware>();
//app.UseMiddleware<XssProtectionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();