using Gotorz.Components;
using Gotorz.Components.Account;
using Gotorz.Data;
using Gotorz.Services;
using Gotorz.Models;
using Gotorz.Hubs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

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
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<SelectedPackageService>();


// Add SignalR with detailed errors
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// --- Add this configuration after builder.Services.AddSignalR() in your Program.cs ---

// Configure authentication for SignalR
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add CORS if needed for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder
            .WithOrigins("https://localhost:7000", "https://localhost:5000") 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


// Configure SignalR authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Customer");
    });
});

// -------------------------------------------------------------------------------

builder.Services.AddHttpContextAccessor();

// --- **IMPORTANT**: Add Razor Pages and Controllers services for antiforgery support ---
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Configure cookie policy to allow cross-site cookies for SignalR
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

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

// --- In the app configuration section, add: ---


// The order is important:
app.UseRouting();
app.UseCors("SignalRPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chathub")
    .RequireAuthorization()
    .RequireCors("SignalRPolicy");

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Gotorz.Client._Imports).Assembly);

// Add additional Identity endpoints
app.MapAdditionalIdentityEndpoints();


// -------------------------------------------------------------------------------

// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    dbContext.Database.Migrate();

    string[] roles = { UserRoles.Admin, UserRoles.Customer, UserRoles.SalesAgent };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@Gotorz.com";
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

// Custom logout endpoint with activity logging
app.MapPost("/Account/CustomLogout", async (HttpContext context, SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager, IActivityLogService activityLogService) =>
{
    try
    {
        var user = await userManager.GetUserAsync(context.User);
        await signInManager.SignOutAsync();

        if (user != null)
        {
            await activityLogService.LogActivityAsync(
                user.Id,
                "User Logout",
                "User logged out successfully",
                context
            );
        }

        var returnUrl = context.Request.Form["returnUrl"].FirstOrDefault();
        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError($"Error during logout logging: {ex.Message}");

        await signInManager.SignOutAsync();
        return Results.Redirect("/");
    }
});

app.Run();
