using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using System.Reflection;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Api.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Workers;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TmsApi.Api.Notifications;
using TmsApi.Application.Notifications;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
var builder = WebApplication.CreateBuilder(args);

//
// Controllers
//
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton<ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();

builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));
        builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();
builder.Services.AddHostedService<TranscriptWorker>();
builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {

                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);


                return tier switch
                {

                    ApiKeyTier.Paid =>

                    RateLimitPartition.GetTokenBucketLimiter(
                        $"paid:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 200,
                            TokensPerPeriod = 100,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),


                    ApiKeyTier.Free =>

                    RateLimitPartition.GetTokenBucketLimiter(
                        $"free:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 30,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),


                    _ =>

                    RateLimitPartition.GetTokenBucketLimiter(
                        $"anon:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 5,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })
                };
            });



    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;



    options.OnRejected = async (context, ct) =>
    {

        var retryAfter = "10";


        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds)
                .ToString();
        }


        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;


        context.HttpContext.Response.ContentType =
            "application/problem+json";


        await context.HttpContext.Response
            .WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Rate limit exceeded",

                    Detail =
                        $"Too many requests. Retry after {retryAfter} seconds.",

                    Status =
                        StatusCodes.Status429TooManyRequests,

                    Type =
                        "https://tms.local/errors/rate_limit_exceeded"
                },
                ct);
    };



    // Transcript concurrency limiter

    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.QueueLimit = 20;
            opt.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });



    // Search limiter

    options.AddTokenBucketLimiter(
        "search",
        opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);
            opt.QueueLimit = 2;
        });
});
//
// MediatR
//
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly);
});

//
// FluentValidation
//
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
builder.Services.AddHostedService<TranscriptWorker>();
//
// MediatR Pipeline Behaviors
// Logging FIRST
//
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>)
);

//
// Validation SECOND
//
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>)
);

//
// Exception Handling
//
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//
// Database
//
builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"));

    options.LogTo(Console.WriteLine, LogLevel.Information);

    options.EnableSensitiveDataLogging();
});
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.MaximumKeyLength = 1024;
});
//
// Application Services
//

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddSingleton<ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();
//
// API Versioning
//
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

//
// OpenAPI
//
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

//
// Authorization
//
builder.Services.AddAuthorization();

//
// Validate DI
//
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});
builder.Services.AddResiliencePipeline(
    "certificate-api",
    pipeline =>
    {
        pipeline

        // Timeout first
        .AddTimeout(TimeSpan.FromSeconds(5))


        // Circuit breaker
        .AddCircuitBreaker(
            new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),

                ShouldHandle =
                    new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>(),


                OnOpened = args =>
                {
                    Console.WriteLine(
                        "Circuit OPENED");

                    return ValueTask.CompletedTask;
                },


                OnClosed = args =>
                {
                    Console.WriteLine(
                        "Circuit CLOSED");

                    return ValueTask.CompletedTask;
                }
            })


        // Retry
        .AddRetry(
            new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,

                Delay =
                    TimeSpan.FromMilliseconds(500),

                BackoffType =
                    DelayBackoffType.Exponential,

                UseJitter = true,


                ShouldHandle =
                    new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>(),


                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Retry #{args.AttemptNumber} " +
                        $"after {args.RetryDelay.TotalMilliseconds}ms");

                    return ValueTask.CompletedTask;
                }
            });
    });
    builder.Services
    .AddHealthChecks()

    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy("alive"),
        tags: new[] { "live" })

    .AddNpgSql(
        builder.Configuration.GetConnectionString("TmsDatabase")!,
        name: "postgres",
        tags: new[] { "ready" });
        builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;

    options.JsonWriterOptions =
        new()
        {
            Indented = false
        };
});
const string ServiceName = "tms-api";

builder.Services
    .AddOpenTelemetry()

    .ConfigureResource(resource =>
        resource.AddService(
            serviceName: ServiceName,
            serviceVersion: "1.0.0"))

    .WithTracing(tracing =>
        tracing

            .AddSource(ServiceName)

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

            .AddOtlpExporter())

    .WithMetrics(metrics =>
        metrics

            .AddMeter(ServiceName)

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

           

            .AddOtlpExporter());
var app = builder.Build();
app.MapHub<TmsHub>("/hubs/tms");
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("live")
    })
    .DisableRateLimiting();

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("ready")
    })
    .DisableRateLimiting();
builder.Services.AddSignalR();
//
// Exception Handler
//
app.UseExceptionHandler();

//
// Request Logging
//
app.UseMiddleware<RequestLoggingMiddleware>();

//
// HTTPS
//
app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();
//
// Authorization
//
app.UseAuthorization();
app.MapHub<TmsHub>("/hubs/tms");
//
// OpenAPI + Scalar
//
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API")
               .WithTheme(ScalarTheme.DeepSpace);

        options.AddDocument("v1", "API V1");
        options.AddDocument("v2", "API V2");
    });
}

//
// Controllers
//
app.MapControllers();

//
// Test endpoint
//
app.MapGet("/api/error", () =>
{
    throw new Exception("Simulated database failure");
});
var attempts = 0;


app.MapPost(
    "/fake/certificates",
    async () =>
    {
        var n =
            Interlocked.Increment(ref attempts);


        if (n % 7 == 0)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(20));

            return Results.Ok(
                new
                {
                    Status = "issued",
                    Attempt = n
                });
        }


        if (n % 3 != 0)
        {
            return Results.StatusCode(
                StatusCodes.Status503ServiceUnavailable);
        }


        if (n % 11 == 0)
        {
            return Results.BadRequest(
                new
                {
                    error = "validation_failed"
                });
        }


        return Results.Ok(
            new
            {
                Status = "issued",
                Attempt = n
            });
    })
    .WithTags("lab-fixtures");
app.Run();