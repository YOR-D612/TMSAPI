using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCertificates()
    {
        return Ok("Certificates endpoint is working.");
    }

    [HttpGet("{id:int}")]
    public IActionResult GetCertificateById(int id)
    {
        return Ok($"Certificate {id}");
    }

    [HttpPost]
    public IActionResult CreateCertificate()
    {
        return Ok();
    }
}