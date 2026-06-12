using TmsApi.Services;
using TmsApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
    

var app = builder.Build();
app.MapGet("/test-logs", async (IEnrollmentService service) =>
{
    await service.EnrollAsync("S-001", "CS-101");

    await service.EnrollAsync("S-001", "CS-101");

    await service.GetByIdAsync("does-not-exist");

    await service.DeleteAsync("does-not-exist");

    return Results.Ok("Logs generated");
});
app.MapGet("/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.Run();