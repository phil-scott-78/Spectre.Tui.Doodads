using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.TextArea;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A text editor example composing TextAreaModel with a status bar.
/// </summary>
public record TextEditorModel : IDoodad<TextEditorModel>, ISizedRenderable
{
    public int MinWidth => 80;
    public int MinHeight => 20;

    private const string SampleText =
        "Welcome to the Spectre.Tui Text Editor!\n" +
        "\n" +
        "This is a simple text editor built using the\n" +
        "TextAreaModel doodad. You can type, navigate,\n" +
        "and edit text freely.\n" +
        "\n" +
        "Features:\n" +
        "  - Arrow keys to navigate\n" +
        "  - Home/End for line start/end\n" +
        "  - Backspace/Delete to erase\n" +
        "  - Ctrl+Q to quit\n" +
        "\n" +
        "Try editing this text!";

    /// <summary>
    /// Gets the text area doodad.
    /// </summary>
    public TextAreaModel TextArea { get; init; } = new TextAreaModel
    {
        MinWidth = 60,
        MinHeight = 18,
        ShowLineNumbers = true,
    }.SetValue(SampleText).Focus().Model;

    /// <summary>
    /// Gets the terminal width for layout.
    /// </summary>
    public int TerminalWidth { get; init; } = 80;

    /// <summary>
    /// Gets the terminal height for layout.
    /// </summary>
    public int TerminalHeight { get; init; } = 24;

    /// <inheritdoc />
    public Command? Init()
    {
        return TextArea.Init();
    }

    /// <inheritdoc />
    public (TextEditorModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.CtrlQ }:
                return (this, Commands.Quit());

            case WindowSizeMessage ws:
                var newWidth = Math.Max(20, ws.Width);
                var newHeight = Math.Max(5, ws.Height - 3);
                return (this with
                {
                    TerminalWidth = ws.Width,
                    TerminalHeight = ws.Height,
                    TextArea = TextArea with
                    {
                        MinWidth = newWidth,
                        MinHeight = newHeight,
                    },
                }, null);

            default:
                var (updatedArea, areaCmd) = TextArea.Update(message);
                return (this with { TextArea = updatedArea }, areaCmd);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var textAreaHeight = Math.Max(0, height - 2);

        // Title bar
        var titleStyle = new Appearance { Decoration = Decoration.Bold | Decoration.Invert };
        var title = " Text Editor ";
        var padding = new string(' ', Math.Max(0, width - title.Length));
        surface.SetString(0, 0, title + padding, titleStyle);

        // Text area
        if (textAreaHeight > 0 && width > 0)
        {
            surface.Render(TextArea, new Rectangle(0, 1, width, textAreaHeight));
        }

        // Status bar
        var statusY = 1 + textAreaHeight;
        var statusStyle = new Appearance { Decoration = Decoration.Dim };
        var lineCount = TextArea.GetValue().Split('\n').Length;
        var statusText = $"Lines: {lineCount} | Ctrl+Q: Quit";
        if (height > 1)
        {
            surface.SetString(0, statusY, statusText, statusStyle);
        }
    }
}