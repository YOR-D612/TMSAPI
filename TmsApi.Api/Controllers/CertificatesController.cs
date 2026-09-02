using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;


namespace TmsApi.Api.Controllers.V2;


[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/certificates")]
public sealed class CertificatesController(
    ICertificateService certificates)
    : ControllerBase
{

    public sealed record IssueRequest(
        int StudentId,
        string CourseCode);



    [HttpPost]
    public async Task<IActionResult> Issue(
        IssueRequest request,
        CancellationToken ct)
    {
        try
        {
            var result =
                await certificates.IssueCertificateAsync(
                    request.StudentId,
                    request.CourseCode,
                    ct);


            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,

                title:
                    "Certificate request rejected",

                detail:
                    ex.Message);
        }
    }
}