using Microsoft.AspNetCore.Mvc;
using CyberChefClone.Services;

namespace CyberChefClone.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly RecipeExecutor _executor;
    private readonly OperationRegistry _registry;
    private readonly ILogger<RecipeController> _logger;

    public RecipeController(RecipeExecutor executor, OperationRegistry registry, ILogger<RecipeController> logger)
    {
        _executor = executor;
        _registry = registry;
        _logger = logger;
    }

    [HttpGet("operations")]
    public IActionResult GetOperations()
    {
        var ops = _registry.GetAllOperations();
        return Ok(ops);
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] RecipeRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.InputBase64) || request.Steps == null || !request.Steps.Any())
            return BadRequest("Invalid request");

        try
        {
            var result = await _executor.ExecuteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing recipe");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("execute-parallel")]
    public async Task<IActionResult> ExecuteParallel([FromBody] ParallelRecipeRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.InputBase64) || request.Steps == null || !request.Steps.Any())
            return BadRequest("Invalid request");

        try
        {
            var result = await _executor.ExecuteParallelAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing parallel recipe");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}