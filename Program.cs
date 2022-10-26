using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blazor.Server.App.Test.Areas.Identity;
using Blazor.Server.App.Test.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddHealthChecks()
    .AddCheck<HealthCheckController>("HealthCheck",failureStatus:HealthStatus.Unhealthy)
    .AddSeqPublisher(options =>
{
    options.ApiKey = "TzeLmeDhsRNEghwJjKYf";
    options.Endpoint = "http://localhost:5341";
    options.DefaultInputLevel = HealthChecks.Publisher.Seq.SeqInputLevel.Information;
},"Seq");


builder.Services
    .AddHealthChecksUI(opt =>
    {
        opt.AddHealthCheckEndpoint("default api",  "healthcheck"); //map health check api    
        opt.SetEvaluationTimeInSeconds(10); //time in seconds between check    
        opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks    
        opt.SetApiMaxActiveRequests(1); //api requests concurrency  
        
    })
    .AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Healthcheck endpoint
app.UseHealthChecks("/healthcheck", new HealthCheckOptions() {
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app
    .UseRouting()
    .UseEndpoints(config => config.MapHealthChecksUI(setup =>
    {
        setup.UIPath = "/health"; // this is ui path in your browser
        setup.ApiPath = "/health-ui-api"; // the UI ( spa app )  use this path to get information from the store ( this is NOT the healthz path, is internal ui api )
    }));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();