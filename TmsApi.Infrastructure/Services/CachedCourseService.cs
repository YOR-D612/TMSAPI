using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{

    public async Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct)
    {
        var key = CacheKeys.Course(code);

        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            code,
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);


                var course =
                    await courseService.GetByCodeAsync(
                        state,
                        token);


                if (course is null)
                {
                    throw new KeyNotFoundException(
                        $"Course {state} not found");
                }


               return new CourseResponseDto(
    course.Id,
    course.Code,
    course.Title,
    course.MaxCapacity,
    course.Enrollments.Count);

            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return result;
    }



    public async Task<List<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;

        var dbHit = false;


        var result = await cache.GetOrCreateAsync(
            key,
            true,
            async (_, token) =>
            {
                dbHit = true;


                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);



                // Your service requires pagination
                var response =
                    await courseService.GetCoursesAsync(
                        new PagedRequest
                        {
                            Page = 1,
                            PageSize = 50
                        },
                        token);



                return response.Items.ToList();

            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return result;
    }



    public async Task InvalidateCourseCacheAsync(
        CancellationToken ct)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);


        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            ct);
    }
}