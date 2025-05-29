using Gatorz.Components;
using Gatorz.Components.Account;
using Gatorz.Data;
using Gatorz.Services;
using Gatorz.Models;
using Gatorz.Hubs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configure Amadeus settings
builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection("Amadeus"));

// Register HTTP client factories
builder.Services.AddHttpClient("AmadeusAuth", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AmadeusSettings>>().Value;
    client.BaseAddress = new Uri(settings.AuthUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("AmadeusAPI", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AmadeusSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register services
builder.Services.AddSingleton<ITokenService, AmadeusTokenService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<ITravelPackageService, TravelPackageService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

// Add SignalR
builder.Services.AddSignalR();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Gatorz.Client._Imports).Assembly);

// Add additional endpoints required by the Identity Razor components.
app.MapAdditionalIdentityEndpoints();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Apply migrations
    dbContext.Database.Migrate();

    // Create roles if they don't exist
    string[] roles = { UserRoles.Admin, UserRoles.Customer, UserRoles.SalesAgent };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create admin user if it doesn't exist
    var adminEmail = "admin@gatorz.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
    }

    // Add FirstName and LastName claims for existing users
    var allUsers = userManager.Users.ToList();
    foreach (var user in allUsers)
    {
        var claims = await userManager.GetClaimsAsync(user);
        if (!claims.Any(c => c.Type == "FirstName"))
        {
            await userManager.AddClaimAsync(user, new Claim("FirstName", user.FirstName));
        }
        if (!claims.Any(c => c.Type == "LastName"))
        {
            await userManager.AddClaimAsync(user, new Claim("LastName", user.LastName));
        }
    }
}

app.Run();