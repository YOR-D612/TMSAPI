using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly TmsDbContext _db;

    public EnrollmentsController(TmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync(cancellationToken);

        return Ok(enrollments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var enrollment = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Enrollment enrollment, CancellationToken cancellationToken)
    {
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(enrollment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Enrollment updated, CancellationToken cancellationToken)
    {
        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (existing == null)
            return NotFound();

        existing.StudentId = updated.StudentId;
        existing.CourseId = updated.CourseId;

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment == null)
            return NotFound();

        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}