using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

var builder = WebApplication.CreateBuilder(args);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddSingleton<OperationRegistry>();
builder.Services.AddScoped<RecipeExecutor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();

app.MapGet("/api/operations", (OperationRegistry registry) =>
{
    try
    {
        var ops = registry.GetAllOperations();
        Console.WriteLine($"📤 Sent {ops.Count} operations");
        return Results.Json(ops, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.MapPost("/api/execute", async (HttpContext context, RecipeExecutor executor) =>
{
    try
    {
        var request = await context.Request.ReadFromJsonAsync<RecipeRequest>();
        if (request == null || string.IsNullOrEmpty(request.InputBase64) || request.Steps == null || !request.Steps.Any())
            return Results.BadRequest(new { error = "Invalid request" });

        var result = await executor.ExecuteAsync(request);
        return Results.Json(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Execution error: {ex.Message}");
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.MapFallbackToFile("index.html");

Console.WriteLine("🚀 Server started");
app.Run();