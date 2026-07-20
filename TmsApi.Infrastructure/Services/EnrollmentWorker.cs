using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Interfaces;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();

        var svc = scope.ServiceProvider
            .GetRequiredService<IEnrollmentService>();

        Console.WriteLine("Processing enrollments...");
    }
}