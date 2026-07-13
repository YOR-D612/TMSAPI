using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class StudentService : IStudentService
{
    private readonly TmsDbContext db;

    public StudentService(TmsDbContext db)
    {
        this.db = db;
    }

    public async Task<StudentResponseDto?> GetStudentByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await db.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StudentResponseDto> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = new Student
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            GPA = request.GPA,
            IsActive = request.IsActive
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        return new StudentResponseDto(
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.GPA,
            student.IsActive);
    }

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        var query = db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        query = request.Desc
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive))
            .ToListAsync(ct);
return new PagedResponse<StudentResponseDto>
{
    Items = items,
    Page = request.Page,
    PageSize = request.PageSize,
    TotalCount = totalCount,
    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
};
        
}}