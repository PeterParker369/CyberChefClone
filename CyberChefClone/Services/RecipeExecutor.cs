using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace CyberChefClone.Services;

public class RecipeExecutor
{
    private readonly OperationRegistry _registry;
    private readonly ILogger<RecipeExecutor> _logger;

    public RecipeExecutor(OperationRegistry registry, ILogger<RecipeExecutor> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(RecipeRequest request)
    {
        var steps = request.Steps;

        // If input is empty, use empty byte array
        byte[] inputData;
        if (string.IsNullOrEmpty(request.InputBase64))
        {
            inputData = Array.Empty<byte>();
            Console.WriteLine("⚠️ Input is empty, using empty array");
        }
        else
        {
            try
            {
                inputData = Convert.FromBase64String(request.InputBase64);
            }
            catch
            {
                // If Base64 decode fails, try as UTF-8 string
                inputData = Encoding.UTF8.GetBytes(request.InputBase64);
                Console.WriteLine("⚠️ Base64 decode failed, using as UTF-8 string");
            }
        }

        var results = new ConcurrentBag<StepResult>();
        var errors = new ConcurrentBag<string>();
        var currentData = inputData;

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            try
            {
                var operation = _registry.CreateOperation(step.OperationName);

                // Copy parameters
                var newParameters = new Dictionary<string, object>();
                foreach (var param in step.Parameters)
                {
                    newParameters[param.Key] = param.Value;
                }
                operation.Parameters = newParameters;

                Console.WriteLine($"=== Executing {step.OperationName} ===");
                Console.WriteLine($"Input data length: {currentData.Length} bytes");

                currentData = await operation.ExecuteAsync(currentData);

                results.Add(new StepResult
                {
                    StepIndex = i,
                    OperationName = step.OperationName,
                    Success = true,
                    OutputBase64 = Convert.ToBase64String(currentData)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing step {Step} ({Operation})", i, step.OperationName);
                errors.Add($"Step {i} ({step.OperationName}): {ex.Message}");

                results.Add(new StepResult
                {
                    StepIndex = i,
                    OperationName = step.OperationName,
                    Success = false,
                    Error = ex.Message
                });
                break;
            }
        }

        return new ExecutionResult
        {
            Success = !errors.Any(),
            FinalOutputBase64 = errors.Any() ? null : Convert.ToBase64String(currentData),
            Steps = results.ToList(),
            Errors = errors.ToList()
        };
    }

    public async Task<ParallelExecutionResult> ExecuteParallelAsync(ParallelRecipeRequest request)
    {
        var tasks = new List<Task<ParallelStepResult>>();

        byte[] inputData;
        if (string.IsNullOrEmpty(request.InputBase64))
        {
            inputData = Array.Empty<byte>();
        }
        else
        {
            try
            {
                inputData = Convert.FromBase64String(request.InputBase64);
            }
            catch
            {
                inputData = Encoding.UTF8.GetBytes(request.InputBase64);
            }
        }

        foreach (var step in request.Steps)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var operation = _registry.CreateOperation(step.OperationName);

                    var newParameters = new Dictionary<string, object>();
                    foreach (var param in step.Parameters)
                    {
                        newParameters[param.Key] = param.Value;
                    }
                    operation.Parameters = newParameters;

                    var result = await operation.ExecuteAsync(inputData);
                    return new ParallelStepResult
                    {
                        OperationName = step.OperationName,
                        Success = true,
                        OutputBase64 = Convert.ToBase64String(result)
                    };
                }
                catch (Exception ex)
                {
                    return new ParallelStepResult
                    {
                        OperationName = step.OperationName,
                        Success = false,
                        Error = ex.Message
                    };
                }
            }));
        }

        var results = await Task.WhenAll(tasks);

        return new ParallelExecutionResult
        {
            Results = results.ToList(),
            TotalSuccess = results.Count(r => r.Success)
        };
    }
}

// DTOs
public class RecipeRequest
{
    public string InputBase64 { get; set; }
    public List<RecipeStep> Steps { get; set; }
}

public class RecipeStep
{
    public string OperationName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public string FinalOutputBase64 { get; set; }
    public List<StepResult> Steps { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class StepResult
{
    public int StepIndex { get; set; }
    public string OperationName { get; set; }
    public bool Success { get; set; }
    public string OutputBase64 { get; set; }
    public string Error { get; set; }
}

public class ParallelRecipeRequest
{
    public string InputBase64 { get; set; }
    public List<RecipeStep> Steps { get; set; }
}

public class ParallelExecutionResult
{
    public List<ParallelStepResult> Results { get; set; }
    public int TotalSuccess { get; set; }
}

public class ParallelStepResult
{
    public string OperationName { get; set; }
    public bool Success { get; set; }
    public string OutputBase64 { get; set; }
    public string Error { get; set; }
}