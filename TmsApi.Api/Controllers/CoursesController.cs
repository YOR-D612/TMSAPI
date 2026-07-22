using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{

    // GET: api/v1/courses
    [HttpGet]
    [ProducesResponseType(
        typeof(List<CourseResponseDto>),
        StatusCodes.Status200OK)]
    [EndpointSummary("List courses")]
    [EndpointDescription(
        "Returns all courses from cache.")]
    public async Task<IActionResult> GetCourses(
        CancellationToken ct)
    {
        var courses =
            await cachedCourseService.GetAllCoursesAsync(ct);

        return Ok(courses);
    }



    // GET: api/v1/courses/{code}
    [HttpGet("{code}", Name = nameof(GetCourseByCode))]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get course by code")]
    public async Task<IActionResult> GetCourseByCode(
        string code,
        CancellationToken ct)
    {
        var course =
            await cachedCourseService.GetCourseAsync(code, ct);


        if (course is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"Course with code '{code}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }


        var self =
            linkGenerator.GetPathByName(
                HttpContext,
                nameof(GetCourseByCode),
                new { code });


        if (self is null)
        {
            throw new InvalidOperationException(
                "Route generation failed.");
        }


        var links = new List<LinkDto>
        {
            new(
                self,
                "self",
                "GET"),

            new(
                self,
                "update",
                "PUT"),

            new(
                self,
                "delete",
                "DELETE")
        };


        return Ok(new
        {
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            course.EnrollmentCount,
            Links = links
        });
    }



    // POST: api/v1/courses
    [HttpPost]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        if (await courseService.CodeExistsAsync(
                request.Code,
                ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                    $"A course with code '{request.Code}' already exists.",
                Status =
                    StatusCodes.Status409Conflict
            });
        }


        var result =
            await courseService.CreateAsync(
                request,
                ct);


        await cachedCourseService
            .InvalidateCourseCacheAsync(ct);


        return CreatedAtAction(
            nameof(GetCourseByCode),
            new
            {
                code = result.Code,
                version = "1.0"
            },
            result);
    }
}