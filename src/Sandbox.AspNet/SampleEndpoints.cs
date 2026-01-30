namespace Sandbox.AspNet;

/// <summary>
/// Maps sample minimal API endpoints that produce varied log levels.
/// </summary>
public static class SampleEndpoints
{
    public static WebApplication MapSampleEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Ok(new { status = "healthy" }));

        app.MapGet("/info", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Info endpoint called at {Time}", DateTimeOffset.Now);
            return Results.Ok(new { level = "info" });
        });

        app.MapGet("/warn", (ILogger<Program> logger) =>
        {
            var randomId = Random.Shared.Next(1000, 9999);
            logger.LogWarning("Warning condition detected for request #{Id}", randomId);
            return Results.Ok(new { level = "warning", id = randomId });
        });

        app.MapGet("/error", (ILogger<Program> logger) =>
        {
            try
            {
                throw new InvalidOperationException("Simulated failure in error endpoint");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error endpoint caught an exception");
                return Results.StatusCode(500);
            }
        });

        app.MapGet("/debug", (ILogger<Program> logger) =>
        {
            var memory = GC.GetTotalMemory(false);
            var threadCount = ThreadPool.ThreadCount;
            logger.LogDebug("Runtime stats: Memory={Memory}B, Threads={Threads}", memory, threadCount);
            return Results.Ok(new { level = "debug", memory, threadCount });
        });

        app.MapPost("/data", async (HttpRequest request, ILogger<Program> logger) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            logger.LogInformation("Received POST /data with {Length} bytes", body.Length);
            logger.LogDebug("POST /data body: {Body}", body.Length > 200 ? body[..200] + "..." : body);
            return Results.Ok(new { level = "info", received = body.Length });
        });

        return app;
    }
}