using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Controllers;


[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building query object");

        var query = context.Students
            .Where(s => s.GPA >= 3.0);

        Console.WriteLine(">>> STEP 2: Adding sort");

        var orderedQuery = query
            .OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Executing query");

        var results = orderedQuery.ToList();

        Console.WriteLine(">>> STEP 4: Query completed\n");

        return Ok(results);
    }

    private static bool IsHonorRoll(double gpa)
    {
        return gpa >= 3.5;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return BadRequest(new
            {
                Message = ex.Message
            });
      
        }
        
    }

}