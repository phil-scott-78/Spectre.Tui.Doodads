namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// An interpolated string handler that renders a layout template directly to an
/// <see cref="IRenderSurface"/>. Literal text defines the visual structure, and
/// interpolation holes place content at the corresponding position.
/// </summary>
/// <remarks>
/// <para>
/// The handler renders sequentially: literal text advances the cursor, newlines move
/// to the next row, and interpolated holes render content at the current position.
/// Whitespace between holes defines relative spacing.
/// </para>
/// <para>
/// Format specifiers use Tailwind-like tokens for styling:
/// <c>{Title:bold text-red}</c>, <c>{Count:dim}</c>, <c>{Status:bold text-white bg-blue}</c>.
/// </para>
/// <para>
/// <see cref="ISizedRenderable"/> holes are rendered via <c>surface.Render(renderable, bounds)</c>
/// using the renderable's declared size. Multi-line renderables advance Y past their full height.
/// All other types are rendered as styled text via <c>ToString()</c>.
/// </para>
/// </remarks>
[InterpolatedStringHandler]
public ref struct LayoutHandler
{
    private readonly IRenderSurface _surface;
    private int _x;
    private int _y;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutHandler"/> struct.
    /// </summary>
    /// <param name="literalLength">The total length of literal text segments.</param>
    /// <param name="formattedCount">The number of interpolation holes.</param>
    /// <param name="surface">The render surface to draw into.</param>
    // ReSharper disable UnusedParameter.Local
    public LayoutHandler(int literalLength, int formattedCount, IRenderSurface surface)
    {
        _surface = surface;
    }
    // ReSharper restore UnusedParameter.Local


    /// <summary>
    /// Appends a literal string segment, rendering characters and advancing the cursor.
    /// Newlines move to the next row.
    /// </summary>
    /// <param name="s">The literal text segment.</param>
    public void AppendLiteral(string s)
    {
        var span = s.AsSpan();
        while (span.Length > 0)
        {
            var newlineIndex = span.IndexOf('\n');
            if (newlineIndex == -1)
            {
                if (span.Length > 0)
                {
                    var pos = _surface.SetString(_x, _y, span.ToString(), Appearance.Plain);
                    _x = pos.X;
                }

                break;
            }

            // Handle \r\n line endings
            var lineEnd = newlineIndex > 0 && span[newlineIndex - 1] == '\r'
                ? newlineIndex - 1
                : newlineIndex;

            if (lineEnd > 0)
            {
                var pos = _surface.SetString(_x, _y, span[..lineEnd].ToString(), Appearance.Plain);
                _x = pos.X;
            }

            _y++;
            _x = 0;
            span = span[(newlineIndex + 1)..];
        }
    }

    /// <summary>
    /// Appends an <see cref="ISizedRenderable"/> at the current position, rendering it
    /// within its measured bounds. Multi-line renderables advance Y past their full height.
    /// </summary>
    /// <param name="renderable">The sized renderable to render.</param>
    /// <param name="format">
    /// An optional format specifier. Use <c>"fill"</c> to give the renderable
    /// remaining row width, or <c>"expand"</c> for remaining width and height.
    /// </param>
    public void AppendFormatted(ISizedRenderable renderable, string? format = null)
    {
        var availableWidth = Math.Max(0, _surface.Viewport.Width - _x);
        var availableHeight = Math.Max(0, _surface.Viewport.Height - _y);
        var available = new Size(availableWidth, availableHeight);

        int w, h;
        if (string.Equals(format, "fill", StringComparison.OrdinalIgnoreCase))
        {
            var desired = renderable.Measure(available);
            w = availableWidth;
            h = Math.Clamp(desired.Height, 0, availableHeight);
        }
        else if (string.Equals(format, "expand", StringComparison.OrdinalIgnoreCase))
        {
            w = availableWidth;
            h = availableHeight;
        }
        else
        {
            var desired = renderable.Measure(available);
            w = Math.Clamp(desired.Width, 0, availableWidth);
            h = Math.Clamp(desired.Height, 0, availableHeight);
        }

        _surface.Render(renderable, new Rectangle(_x, _y, w, h));
        if (h > 1)
        {
            _y += h - 1;
        }

        _x += w;
    }

    /// <summary>
    /// Appends an <see cref="IRenderable"/>. If the renderable implements <see cref="ISizedRenderable"/>,
    /// it is rendered using its declared bounds. Otherwise its <c>ToString()</c> representation
    /// is rendered as styled text.
    /// </summary>
    /// <param name="renderable">The renderable to render.</param>
    /// <param name="format">An optional Tailwind-like format specifier for styling.</param>
    public void AppendFormatted(IRenderable renderable, string? format = null)
    {
        if (renderable is ISizedRenderable sized)
        {
            AppendFormatted(sized, format);
        }
        else
        {
            AppendFormatted(renderable.ToString(), format);
        }
    }

    /// <summary>
    /// Appends a string value at the current position with optional styling.
    /// </summary>
    /// <param name="value">The string to render.</param>
    /// <param name="format">An optional Tailwind-like format specifier (e.g. "bold text-red").</param>
    public void AppendFormatted(string? value, string? format = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var appearance = format is not null ? StyleParser.Parse(format) : Appearance.Plain;
        var pos = _surface.SetString(_x, _y, value, appearance);
        _x = pos.X;
    }

    /// <summary>
    /// Appends a <see cref="ReadOnlySpan{T}"/> of characters at the current position.
    /// </summary>
    /// <param name="value">The character span to render.</param>
    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return;
        }

        var pos = _surface.SetString(_x, _y, value.ToString(), Appearance.Plain);
        _x = pos.X;
    }

    /// <summary>
    /// Appends any value at the current position by calling <c>ToString()</c>,
    /// with optional styling via format specifier.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to render.</param>
    /// <param name="format">An optional Tailwind-like format specifier.</param>
    public void AppendFormatted<T>(T value, string? format = null)
    {
        if (value is IRenderable renderable)
        {
            AppendFormatted(renderable, format);
            return;
        }

        AppendFormatted(value?.ToString(), format);
    }
}
