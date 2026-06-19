
using TmsApi.Services;
using TmsApi.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();


builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();



//
// Validate DI on build (good for debugging)
//
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});


var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
// Development tools
if (app.Environment.IsDevelopment())
{


    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseStatusCodePages();
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});


app.Run();

