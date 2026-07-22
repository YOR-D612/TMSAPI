using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);

    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);

    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct);
        Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken ct);

Task AddAsync(
    Enrollment enrollment,
    CancellationToken ct);

Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct);
}