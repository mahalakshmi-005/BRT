using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using BRT.Data;

var builder = WebApplication.CreateBuilder(args);

// Npgsql 6+ is strict about DateTime.Kind for timestamp columns — this project only ever writes
// DateTime.UtcNow, but this switch avoids a repeat of past "Cannot write DateTime with Kind=Unspecified" errors.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// --- Render: listen on the port Render assigns via $PORT ---
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// --- Services ---
builder.Services.AddControllersWithViews();

var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<BRT.Services.IFileUploadService, BRT.Services.FileUploadService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.AccessDeniedPath = "/Admin/Account/Login";
        options.Cookie.Name = "BRT.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- Trust Render's reverse proxy headers (X-Forwarded-Proto/For) so HTTPS detection & secure cookies work ---
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Render's proxy IP isn't known ahead of time and changes — clear the default loopback-only
// allow-list so forwarded headers from Render's edge are actually trusted.
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// --- Seed database on startup ---
// --- Apply migrations & seed database on startup ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Create/update database schema
    context.Database.Migrate();

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // Seed initial data
    DbInitializer.Seed(context, config);
}
// --- Middleware pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var isRunningOnRender = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER"));
if (!isRunningOnRender)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Render provides DATABASE_URL as postgres://user:pass@host:port/dbname — Npgsql needs key=value format.
// Falls back to appsettings.json ConnectionStrings:DefaultConnection for local development.
static string BuildConnectionString(IConfiguration config)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrEmpty(databaseUrl))
        return config.GetConnectionString("DefaultConnection") ?? string.Empty;

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    return $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.AbsolutePath.TrimStart('/')};" +
           $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}