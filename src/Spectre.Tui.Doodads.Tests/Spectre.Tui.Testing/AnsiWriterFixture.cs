using System.Text;
using Spectre.Console;

namespace Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

public sealed class AnsiWriterFixture
{
    private readonly StringBuilder _buffer;

    public AnsiWriter Writer { get; }
    public AnsiWriter NullWriter { get; }

    public string Output => _buffer.ToString();

    public AnsiWriterFixture()
    {
        _buffer = new StringBuilder();


        Writer = new AnsiWriter(
            new StringWriter(_buffer),
            new AnsiCapabilities
            {
                ColorSystem = ColorSystem.TrueColor,
                Ansi = true,
                Links = true,
                AlternateBuffer = true,
            });

        NullWriter = new AnsiWriter(
            new StringWriter(),
            new AnsiCapabilities
            {
                ColorSystem = ColorSystem.TrueColor,
                Ansi = true,
                Links = true,
                AlternateBuffer = true,
            });
    }
}