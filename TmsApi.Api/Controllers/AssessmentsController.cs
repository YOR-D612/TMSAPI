using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController(IAssessmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssessmentResponseDto>>> GetAll(
        CancellationToken ct)
    {
        return Ok(await service.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssessmentResponseDto>> Get(
        int id,
        CancellationToken ct)
    {
        var assessment = await service.GetByIdAsync(id, ct);

        if (assessment is null)
            return NotFound();

        return Ok(assessment);
    }

    [HttpPost]
    public async Task<ActionResult<AssessmentResponseDto>> Create(
        CreateAssessmentRequest request,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(Get),
            new { id = created.Id },
            created);
    }
}