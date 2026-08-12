using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[Tags("Courses")]
[Produces("application/json")]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    LinkGenerator linkGenerator) : ControllerBase
{

    // GET api/v2/courses?fields=id,title
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("List courses with data shaping and HATEOAS")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? fields,
        CancellationToken ct)
    {
        var courses =
            await cachedCourseService.GetAllCoursesAsync(ct);


        var shaped =
            courses.ShapeData(
                fields,
                CourseResponseDtoFields.Allowed);


        var links = new List<LinkDto>
        {
            new(
                linkGenerator.GetPathByAction(
                    HttpContext,
                    nameof(GetCourses),
                    values: new
                    {
                        version = "2.0",
                        fields
                    })!,
                "self",
                "GET")
        };


        return Ok(new
        {
            Data = shaped,

            Meta = new
            {
                Count = courses.Count
            },

            Links = links
        });
    }
}