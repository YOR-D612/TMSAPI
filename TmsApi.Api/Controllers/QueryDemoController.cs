using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/demo")]
public class QueryDemoController : ControllerBase
{
    private readonly TmsDbContext db;

    public QueryDemoController(TmsDbContext db)
    {
        this.db = db;
    }

    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlusOne(CancellationToken cancellationToken)
    {
        var students = await db.Students.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);

            Console.WriteLine($"{s.Name}: {count} enrollments");
        }

        return Ok("Check console logs for N+1 queries");
    }

    [HttpGet("fixed")]
    public async Task<IActionResult> FixedQuery(CancellationToken cancellationToken)
    {
        var report = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var r in report)
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount}");

        return Ok(report);
    }
}