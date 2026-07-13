using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public class CreateAssessmentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 1000)]
    public decimal MaxScore { get; set; }

    [Range(0.01, 100)]
    public decimal Weight { get; set; }

    [Required]
    public int CourseId { get; set; }
}