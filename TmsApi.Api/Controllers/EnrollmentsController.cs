using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // GET: api/courses/{courseId}/enrollments
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
    typeof(IEnumerable<EnrollmentResponseDto>),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("List enrollments for a course")]
[EndpointDescription("Returns all enrollments for the specified course.")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);

        return Ok(enrollments);
    }

    // GET: api/courses/{courseId}/enrollments/{id}
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Get an enrollment")]
[EndpointDescription("Returns a single enrollment by its ID.")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);

        if (enrollment is null)
            return NotFound();

        return Ok(enrollment);
    }

    // POST: api/courses/{courseId}/enrollments
    [HttpPost]
    [ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
[EndpointSummary("Enroll a student")]
[EndpointDescription("Enrolls a student into a course if capacity allows.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
            return NotFound();

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }
}