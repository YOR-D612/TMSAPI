using Microsoft.AspNetCore.Authorization;
using TmsApi.Services;
using TmsApi.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddProblemDetails();
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Build app
var app = builder.Build();


// 👇 ADD IT HERE
app.Map("/error", () =>
{
    throw new Exception("Test production error handling");
});

// Environment-specific setup
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
app.UseExceptionHandler();
app.UseStatusCodePages();
// Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseAuthorization();

// Controllers
app.MapControllers();
if (app.Environment.IsDevelopment())


    // TODO 3: PRODUCTION SAFETY
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/error");
    }

// Optional but recommended
app.UseHttpsRedirection();

app.UseAuthorization();


app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});
app.Run();
