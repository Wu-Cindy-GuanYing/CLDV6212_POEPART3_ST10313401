using ABCRetailers.Services;
using ABCRetailers.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// 1) MVC and Accessor
// -----------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// -----------------------------------------------------------
// 2) EF Core: Azure SQL Database
// -----------------------------------------------------------
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("AuthDataBase");
    options.UseSqlServer(connStr);
});

// -----------------------------------------------------------
// 3) Azure Functions API Clients (CORRECTED)
// -----------------------------------------------------------

// Primary FunctionsApiClient with interface registration
builder.Services.AddHttpClient<IFunctionsApi, FunctionsApiClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["FunctionApi:BaseUrl"] ?? "https://localhost:7167";

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/");
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("Accept", "application/json");

    // Log the configuration for debugging
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("FunctionsApiClient configured with BaseAddress: {BaseAddress}", client.BaseAddress);
});

// Register services in the correct order
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();
builder.Services.AddScoped<IManualStorageInitializationService, ManualStorageInitializationService>();

// Register StorageInitializationService as singleton and hosted service (ONCE)
builder.Services.AddSingleton<StorageInitializationService>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<StorageInitializationService>());

// Scoped registration for direct usage
builder.Services.AddScoped<FunctionsApiClient>();

// -----------------------------------------------------------
// 4) Cookie Authentication (Unified Scheme)
// -----------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccessDenied";

        options.Cookie.Name = "ABCAuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// -----------------------------------------------------------
// 5) Session Setup
// -----------------------------------------------------------
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ABCSession";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// -----------------------------------------------------------
// 6) File Upload Limit
// -----------------------------------------------------------
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// -----------------------------------------------------------
// 7) Logging
// -----------------------------------------------------------
builder.Services.AddLogging();

// -----------------------------------------------------------
// 8) Build App
// -----------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------
// 9) Culture Settings (for decimal handling - fixes price issues)
// -----------------------------------------------------------
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// -----------------------------------------------------------
// 10) Middleware Pipeline
// -----------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session BEFORE authentication
app.UseSession();

// Authentication / Authorization
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------
// 11) Apply Database Migrations Automatically
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.MigrateAsync();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations");
    }
}

// -----------------------------------------------------------
// 12) Routes
// -----------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();