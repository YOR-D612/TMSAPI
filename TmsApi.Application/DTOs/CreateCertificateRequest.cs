using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;
public class CreateCertificateRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    public DateOnly IssuedOn { get; set; }
}