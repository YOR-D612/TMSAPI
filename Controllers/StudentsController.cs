using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;
namespace TmsApi.Controllers;
[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService service;

    public StudentsController(IStudentService service)
    {
        this.service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<StudentResponseDto>>> GetStudents(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        return Ok(await service.GetStudentsAsync(request, ct));
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    public async Task<ActionResult<StudentResponseDto>> GetStudentById(
        int id,
        CancellationToken ct)
    {
        var student = await service.GetStudentByIdAsync(id, ct);

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> CreateStudent(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = await service.CreateStudentAsync(request, ct);

        return CreatedAtRoute(
            nameof(GetStudentById),
            new { id = student.Id },
            student);
    }
}