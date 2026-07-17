using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Persistence;


namespace TmsApi.Infrastructure.Services;

public class AssessmentService(TmsDbContext db) : IAssessmentService
{
    public async Task<IEnumerable<AssessmentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await db.Assessments
            .AsNoTracking()
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToListAsync(ct);
    }

    public async Task<AssessmentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Assessments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AssessmentResponseDto> CreateAsync(CreateAssessmentRequest request, CancellationToken ct)
    {
        var assessment = new Assessment
        {
            Title = request.Title,
            MaxScore = request.MaxScore,
            Weight = request.Weight,
            CourseId = request.CourseId
        };

        db.Assessments.Add(assessment);

        await db.SaveChangesAsync(ct);

        return new AssessmentResponseDto(
            assessment.Id,
            assessment.Title,
            assessment.MaxScore,
            assessment.Weight,
            assessment.CourseId);
    }
}