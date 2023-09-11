using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WazeCredit.Data;
using WazeCredit.Middleware;
using WazeCredit.Service;
using WazeCredit.Service.LifeTimeExample;
using WazeCredit.Utility.AppSettingsClasses;
using WazeCredit.Utility.DI_Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddTransient<IMarketForecaster, MarketForecasterV2>();
//services.AddSingleton<IMarketForecaster>(new MarketForecasterV2());
//services.AddTransient<MarketForecasterV2>();
//services.AddSingleton(new MarketForecasterV2());
//services.AddTransient(typeof(MarketForecasterV2));
//services.AddTransient(typeof(IMarketForecaster), typeof(MarketForecasterV2));

builder.Services.TryAddTransient<IMarketForecaster, MarketForecaster>();

builder.Services.AddAppSettingsConfig(builder.Configuration);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddTransient<TransientService>();
builder.Services.AddScoped<ScopedService>();
builder.Services.AddSingleton<SingletonService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseMiddleware<CustomMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
