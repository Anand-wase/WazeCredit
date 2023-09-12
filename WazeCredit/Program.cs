using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WazeCredit.Data;
using WazeCredit.Middleware;
using WazeCredit.Models;
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

builder.Services.AddScoped<CreditApprovedHigh>();
builder.Services.AddScoped<CreditApprovedLow>();

builder.Services.AddScoped<Func<CreditApprovedEnum, ICreditApproved>>(ServiceProvider => range =>
{
    switch (range)
    {
        case CreditApprovedEnum.High: return ServiceProvider.GetService<CreditApprovedHigh>();
        case CreditApprovedEnum.Low: return ServiceProvider.GetService<CreditApprovedLow>();
        default: return ServiceProvider.GetService<CreditApprovedLow>();
    }
});

//services.AddScoped<IValidationChecker, AddressValidationChecker>();
//services.AddScoped<IValidationChecker, CreditValidationChecker>();
//services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationChecker, AddressValidationChecker>());
//services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationChecker, CreditValidationChecker>());

builder.Services.TryAddEnumerable(new[] {
            ServiceDescriptor.Scoped<IValidationChecker, AddressValidationChecker>(),
            ServiceDescriptor.Scoped<IValidationChecker, CreditValidationChecker>()
            });

builder.Services.AddScoped<ICreditValidator, CreditValidator>();

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
