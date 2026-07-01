using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/dashboard")]

public class DashboardController : ControllerBase
{
    private readonly TmsDbContext context;

    public DashboardController(TmsDbContext context)
    {
        this.context = context;
    }

    //=====================================================
    // 1. Pagination
    //=====================================================

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        int pageSize = 20;

        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    //=====================================================
    // 2. Top 5 Courses
    //=====================================================

    [HttpGet("top-courses")]
    public async Task<IActionResult> TopCourses(
        CancellationToken cancellationToken = default)
    {
        var result = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(result);
    }
}