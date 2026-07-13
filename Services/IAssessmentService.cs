using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IAssessmentService
{
    Task<IEnumerable<AssessmentResponseDto>> GetAllAsync(CancellationToken ct);

    Task<AssessmentResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<AssessmentResponseDto> CreateAsync(CreateAssessmentRequest request, CancellationToken ct);
}