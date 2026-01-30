using System.Text;
using Sandbox.AspNet;
using Spectre.Tui;
using Spectre.Tui.Doodads.Rendering;

// Create the shared log store
var store = new LogStore();

// Build the ASP.NET app
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new MemoryLoggerProvider(store));
builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Snapshot service registrations before Build() (IServiceProvider doesn't expose them after)
var serviceSnapshot = builder.Services.ToList();

var app = builder.Build();
app.MapSampleEndpoints();

// Start ASP.NET in the background (non-blocking)
await app.StartAsync();

var baseUrl = app.Urls.FirstOrDefault() ?? "http://localhost:5000";

var configRoot = (IConfigurationRoot)app.Configuration;

// Run the TUI log viewer in fullscreen
using var terminal = Terminal.Create(new FullscreenMode());
var model = new AppMonitorModel
{
    Store = store,
    BaseUrl = baseUrl,
    RoutesProvider = RoutesProvider,
    ConfigProvider = () => configRoot.GetDebugView(),
    ServicesProvider = ServicesProvider,
};

await Spectre.Tui.Doodads.Program.RunAsync(model, opts =>
{
    opts.Terminal = terminal;
    opts.TerminalMode = new FullscreenMode();

    if (Environment.GetEnvironmentVariable("NO_COLOR") is not null)
    {
        opts.RenderPipeline.Add(surface => new NoColorStage(surface));
    }

    if (!Equals(Console.OutputEncoding, Encoding.UTF8))
    {
        opts.RenderPipeline.Add(surface => new FallbackCharacterStage(surface));
    }
});

await app.StopAsync();
return;


// Create data providers for routes and config views
IReadOnlyList<string> RoutesProvider()
{
    var sources = app.Services.GetRequiredService<IEnumerable<EndpointDataSource>>();
    var lines = new List<string>();
    foreach (var source in sources)
    {
        foreach (var endpoint in source.Endpoints)
        {
            if (endpoint is RouteEndpoint route)
            {
                var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;
                var methodStr = methods != null ? string.Join(",", methods) : "***";
                lines.Add($"{methodStr,-8} {route.RoutePattern.RawText}");
            }
        }
    }

    lines.Sort(StringComparer.OrdinalIgnoreCase);
    return lines;
}

static string FormatTypeName(Type type)
{
    if (!type.IsGenericType)
    {
        return type.FullName ?? type.Name;
    }

    var name = type.FullName ?? type.Name;
    var backtick = name.IndexOf('`');
    if (backtick >= 0)
    {
        name = name[..backtick];
    }

    var args = type.GetGenericArguments();
    var formattedArgs = string.Join(", ", args.Select(FormatTypeName));
    return $"{name}<{formattedArgs}>";
}


IReadOnlyList<string> ServicesProvider()
{
    var lines = new List<string>();
    foreach (var descriptor in serviceSnapshot)
    {
        var lifetime = descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => "Singleton",
            ServiceLifetime.Scoped => "Scoped   ",
            ServiceLifetime.Transient => "Transient",
            _ => descriptor.Lifetime.ToString(),
        };

        var serviceType = FormatTypeName(descriptor.ServiceType);
        string implementation;
        if (descriptor.ImplementationType is { } implType)
        {
            implementation = FormatTypeName(implType);
        }
        else if (descriptor.ImplementationFactory is not null)
        {
            implementation = "Factory";
        }
        else if (descriptor.ImplementationInstance is { } instance)
        {
            implementation = $"Instance ({instance.GetType().Name})";
        }
        else
        {
            implementation = "---";
        }

        lines.Add($"{lifetime}  {serviceType} -> {implementation}");
    }

    lines.Sort(StringComparer.OrdinalIgnoreCase);
    return lines;
}