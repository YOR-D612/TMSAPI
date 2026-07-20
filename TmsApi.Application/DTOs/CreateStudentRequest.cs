using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.DTOs;
public class CreateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Range(0, 4)]
    public double GPA { get; set; }

    public bool IsActive { get; set; } = true;
}