using Sandbox.Download;
using Spectre.Tui;

if (args.Length == 0)
{
    Console.WriteLine("Usage: Sandbox.Download <url>");
    Console.WriteLine("  Downloads a file and displays progress.");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run --project src/Sandbox.Download -- https://example.com/file.zip");
    return;
}

var url = args[0];
var uri = new Uri(url);
var filename = Path.GetFileName(uri.AbsolutePath);
if (string.IsNullOrWhiteSpace(filename))
{
    filename = "download";
}

var outputPath = Path.Combine(Directory.GetCurrentDirectory(), filename);

var model = new DownloadModel
{
    Url = url,
    OutputPath = outputPath,
};

await Spectre.Tui.Doodads.Program.RunAsync(model, opts =>
{
    opts.TerminalMode = new InlineMode(model.MinHeight);
});