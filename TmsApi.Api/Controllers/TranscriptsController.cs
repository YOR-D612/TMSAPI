using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;


[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{


    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript(
        [FromBody] object? request)
    {
        return Ok(new
        {
            Message =
            "Transcript request accepted"
        });
    }

}