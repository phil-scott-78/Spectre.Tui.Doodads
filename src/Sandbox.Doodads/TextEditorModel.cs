using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.TextArea;
using Spectre.Tui.Doodads.Layout;
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
                var newHeight = Math.Max(5, ws.Height - 2);
                return (this with
                {
                    TextArea = TextArea with
                    {
                        MinWidth = newWidth,
                        MinHeight = newHeight,
                    },
                }, null);

            default:
                return this.Forward(message, m => m.TextArea, (m, v) => m with { TextArea = v });
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var lineCount = TextArea.GetValue().Split('\n').Length;

        var layout = Flex.Column()
            .Add(new TitleBar(" Text Editor "), FlexSize.Fixed(1))
            .Add(TextArea, FlexSize.Fill())
            .Add(new StatusBar($"Lines: {lineCount} | Ctrl+Q: Quit"), FlexSize.Fixed(1));

        surface.Render(layout, surface.Viewport);
    }

    /// <summary>
    /// A title bar widget that renders inverted text filling the row.
    /// </summary>
    private record TitleBar(string Title) : ISizedRenderable
    {
        public int MinWidth => 1;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var width = Math.Max(0, surface.Viewport.Width);
            var style = new Appearance { Decoration = Decoration.Invert };
            var padding = new string(' ', Math.Max(0, width - Title.Length));
            surface.SetString(0, 0, Title + padding, style);
        }
    }

    /// <summary>
    /// A status bar widget that renders dim text.
    /// </summary>
    private record StatusBar(string Text) : ISizedRenderable
    {
        public int MinWidth => 1;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var style = new Appearance { Decoration = Decoration.Dim };
            surface.SetString(0, 0, Text, style);
        }
    }
}
