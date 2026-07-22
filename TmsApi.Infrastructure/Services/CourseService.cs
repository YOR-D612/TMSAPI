using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger)
    : ICourseService
{
    public async Task<Course?> GetByCodeAsync(
        string code,
        CancellationToken ct)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Code == code,
                ct);
    }

    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        var query = context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                c.Title.Contains(request.Search) ||
                c.Code.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(ct);

        logger.LogInformation(
            "Courses in database: {Count}",
            totalCount);

        query = request.SortBy?.ToLower() switch
        {
            "title" => request.Desc
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title),

            _ => request.Desc
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)request.PageSize)
        };
    }

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }
}