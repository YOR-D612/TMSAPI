using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;
public class CreateCertificateRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    public DateOnly IssuedOn { get; set; }
}