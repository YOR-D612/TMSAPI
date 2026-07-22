using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using System.Reflection;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);


// =============================
// Controllers + MediatR + Validation
// =============================

builder.Services.AddControllers();


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly);
});


builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// Pipeline order matters
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));



// =============================
// Exception Handling
// =============================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



// =============================
// Application Services
// =============================

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();



// =============================
// Database
// =============================

builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"));

    options.LogTo(
        Console.WriteLine,
        LogLevel.Information);

    options.EnableSensitiveDataLogging();
});



// =============================
// OpenAPI + Scalar
// =============================

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "TMS API V1";
        return Task.CompletedTask;
    });
});


builder.Services.AddOpenApi("v2", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "TMS API V2";
        return Task.CompletedTask;
    });
});


// =============================
// API Versioning
// =============================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.ReportApiVersions = true;

        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });



// =============================
// Authorization
// =============================

builder.Services.AddAuthorization();



// =============================
// Dependency Injection Validation
// =============================

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});



var app = builder.Build();



// =============================
// Middleware Pipeline
// =============================


app.UseExceptionHandler();


app.UseMiddleware<RequestLoggingMiddleware>();


app.UseHttpsRedirection();


app.UseAuthorization();



// =============================
// Controllers
// =============================

app.MapControllers();



// =============================
// Test Exception Endpoint
// =============================

app.MapGet("/api/error", () =>
{
    throw new Exception(
        "Simulated database failure");
});



// =============================
// Scalar Documentation
// =============================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");


    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace);


        options.AddDocument(
            "v1",
            "API Version 1.0");


        options.AddDocument(
            "v2",
            "API Version 2.0");
    });
}

app.Run();