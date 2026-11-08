using BitzArt.Blazor.Cookies;
using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using YouSummarize.ServicesExternal;
using YouSummarize;
using YouSummarize.Api;
using YouSummarize.Api.TravelPlanner;
using YouSummarize.Components;
using YouSummarize.Data;

using YouSummarize.Data.Repository;
using YouSummarize.Managers;
using YouSummarize.Managers.AiGenerators;

using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);


var httpPort = builder.Configuration.GetValue<int?>("HTTP_PORT");
if (httpPort.HasValue)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort.Value}");
}
else if (string.IsNullOrWhiteSpace(builder.Configuration[WebHostDefaults.ServerUrlsKey])
         && string.IsNullOrWhiteSpace(builder.Configuration["HTTP_PORTS"])
         && string.IsNullOrWhiteSpace(builder.Configuration["HTTPS_PORTS"]))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();
builder.AddBlazorCookies();



builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromSeconds(5));

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents(
    options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddHttpContextAccessor();

// Ajouter HttpClient pour les appels API internes
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// Configurer HttpClient pour l'API Supadata (extraction de transcripts YouTube)
builder.Services.AddHttpClient("Supadata", client =>
{
    client.BaseAddress = new Uri("https://api.supadata.ai");
    client.Timeout = TimeSpan.FromSeconds(60);

    var apiKey = builder.Configuration["SUPADATA_API_KEY"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }
});
builder.Services.AddScoped<ISupadataService, SupadataService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<EmailManager>();
builder.Services.AddScoped<YouSummarize.Services.ApprovalTokenService>();
builder.Services.AddScoped<YouSummarize.Services.UserApprovalEmailService>();

// TravelPlanner (YouSummarize) services
builder.Services.AddSingleton<YtDlpAudioExtractor>();
builder.Services.AddScoped<YouSummarize.Services.TravelPlanner.Sources.YoutubeSourceService>();
builder.Services.AddScoped<YouSummarize.Services.TravelPlanner.Sources.GenericSourceService>();
builder.Services.AddScoped<IEnumerable<YouSummarize.Services.TravelPlanner.Sources.IContentSourceService>>(sp =>
[
    sp.GetRequiredService<YouSummarize.Services.TravelPlanner.Sources.YoutubeSourceService>(),
]);
builder.Services.AddScoped<YouSummarize.Services.TravelPlanner.Sources.ContentSourceResolver>();
builder.Services.AddScoped<YouSummarize.Services.TravelPlanner.ISummaryService, YouSummarize.Services.TravelPlanner.SummaryService>();

builder.Services.AddRepository();

// Configure PostgreSQL connection string (centralisé)
var serverUrl = builder.Configuration["DB_HOST_URL"];
var serverPort = builder.Configuration["DB_HOST_PORT"];
var dbName = builder.Configuration["DB_DATABASE"] ?? "recettesfamilledb";
var dbUser = builder.Configuration["DB_USERNAME"] ?? "pguser";
var dbPassword = builder.Configuration["DB_PASSWORD"] ?? "PGUserPwd";

var postgresCs = $"Host={serverUrl};Port={serverPort};Database={dbName};Username={dbUser};Password={dbPassword};Pooling=true";

builder.Services.AddManagers(builder.Configuration);
builder.Services.AddScoped<AiManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(postgresCs);
}, ServiceLifetime.Scoped);

// Configure Data Protection to persist keys in PostgreSQL
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("YouSummarize"); // Important pour que toutes les instances partagent les mêmes clés

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;          
    options.Password.RequireLowercase = false;      
    options.Password.RequireUppercase = false;      
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddApiEndpoints(); // Active les endpoints API Identity

// Configuration des cookies Identity pour partage entre API et circuits Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".YouSummarize.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

    // Durée de vie du cookie (pour RememberMe = true ET false)
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // IMPORTANT pour Blazor Server : même sans "Remember Me", 
    // on crée un cookie temporaire de quelques heures pour que le circuit SignalR fonctionne
    // Le cookie expire quand même à la fermeture du navigateur si isPersistent = false
    // mais reste valide pendant la session de navigation active
    options.Cookie.MaxAge = null; // Laisse isPersistent contrôler l'expiration

    // CRITIAL: Force l'écriture du cookie avant la réponse pour éviter les race conditions avec SignalR
    options.Cookie.IsEssential = true; // Le cookie est essentiel pour le fonctionnement

    // Configurer les chemins de redirection personnalisés
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
});
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 32 * 1024 * 100;
    });

builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// IMPORTANT: Ajouter Authentication et Authorization AVANT MapRazorComponents
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Enregistrer les endpoints d'authentification personnalisés
app.MapAuthEndpoints();

// Enregistrer les endpoints d'approbation utilisateur
app.MapUserApprovalEndpoints();

// Enregistrer les endpoints utilitaires
app.MapUtilityEndpoints();

// Enregistrer les endpoints TravelPlanner (YouSummarize)
app.MapTravelPlannerEndpoints();

await app.RunAsync();

public record LoginRequest(string Email, string Password, bool RememberMe, string? ReturnUrl);
