public interface IStudentService
{
    Task<StudentResponseDto?> GetStudentByIdAsync(int id, CancellationToken ct);

    Task<StudentResponseDto> CreateStudentAsync(CreateStudentRequest request,
        CancellationToken ct);

    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct);
}