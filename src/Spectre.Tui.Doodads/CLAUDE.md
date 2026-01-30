# Spectre.Tui.Doodads

A component library for building interactive TUI applications using The Elm Architecture (TEA). Models are immutable records that implement `IDoodad<TSelf>`, processing messages and returning updated state plus optional side-effect commands.

## Core Interface

```csharp
public interface IDoodad<TSelf> : IWidget
    where TSelf : IDoodad<TSelf>
{
    Command? Init();
    (TSelf Model, Command? Command) Update(Message message);
}
```

Uses CRTP (Curiously Recurring Template Pattern) for type-safe self-references. `Init()` returns an optional startup command. `Update(Message)` processes messages and returns the updated model plus an optional command. `Render(RenderContext)` is inherited from `IWidget`.

## Command System

```csharp
public delegate Task<Message?> Command(CancellationToken cancellationToken);
```

The `Commands` static class provides factory methods:

- **`Commands.Message(Message)`** — Immediately produces a message.
- **`Commands.Batch(params Command?[])`** — Runs commands concurrently, collects results into `BatchMessage`.
- **`Commands.Sequence(params Command?[])`** — Runs commands sequentially, each result fed through `Update` before the next.
- **`Commands.Tick(TimeSpan, Func<DateTimeOffset, Message>)`** — Waits for an interval, then produces a message. Used for animations and timers.
- **`Commands.Quit()`** — Produces `QuitMessage` to exit the program.

## Program Runner

```csharp
public static Task<TModel> RunAsync<TModel>(
    TModel initialModel,
    Action<ProgramOptions>? configure = null,
    CancellationToken cancellationToken = default)
    where TModel : IDoodad<TModel>
```

Event loop: read input → drain message queue → call `Update` → schedule returned commands → render. Returns the final model state on exit.

### ProgramOptions

| Property | Type | Default | Description |
|---|---|---|---|
| `TerminalMode` | `ITerminalMode?` | `FullscreenMode` | Screen management strategy |
| `TargetFps` | `int` | `60` | Render target frame rate |
| `Terminal` | `ITerminal?` | `null` | Optional external terminal |
| `CancellationToken` | `CancellationToken` | `default` | Cancellation support |

## Message Hierarchy

All messages are immutable records inheriting from `abstract record Message`.

| Message | Properties | Description |
|---|---|---|
| `KeyMessage` | `Key`, `Runes[]`, `Alt`, `Shift`, `Ctrl` | Keyboard input |
| `MouseMessage` | `X`, `Y`, `MouseAction`, `MouseButton` | Mouse input |
| `WindowSizeMessage` | `Width`, `Height` | Terminal resize |
| `FocusMessage` | *(none)* | Terminal gained focus |
| `BlurMessage` | *(none)* | Terminal lost focus |
| `TickMessage` | `Time`, `Id`, `Tag` | Animation/timer frame |
| `QuitMessage` | *(none)* | Exit program |
| `BatchMessage` | `Messages` | Internal: batch command results |
| `SequenceMessage` | `StepMessage`, `Remaining` | Internal: sequence step result |
| `CommandErrorMessage` | `Exception` | Produced when a command throws an unhandled exception |

### Key Enum

Navigation (`Up`, `Down`, `Left`, `Right`, `Home`, `End`, `PageUp`, `PageDown`), editing (`Backspace`, `Delete`, `Insert`, `Tab`, `ShiftTab`, `Enter`, `Escape`), character (`Char`, `Space`), function keys (`F1`–`F24`), ctrl combinations (`CtrlA`–`CtrlZ`), and `None`.

### Mouse Enums

- **MouseAction:** `Press`, `Release`, `Motion`, `WheelUp`, `WheelDown`
- **MouseButton:** `None`, `Left`, `Right`, `Middle`

## Input Abstraction

```csharp
public interface IInputReader : IDisposable
{
    ValueTask<Message?> ReadAsync(CancellationToken cancellationToken);
}
```

`ConsoleInputReader` is the default implementation. It detects window resize, maps console keys to `KeyMessage`, and handles modifier keys.

### KeyBinding System

```csharp
public record KeyBinding
{
    public required IReadOnlyList<Key> Keys { get; init; }
    public string HelpKey { get; init; }
    public string HelpDescription { get; init; }
    public bool Enabled { get; init; } = true;

    public bool Matches(KeyMessage message);
    public static KeyBinding For(params Key[] keys);
}

public interface IKeyMap
{
    IEnumerable<KeyBinding> ShortHelp();
    IEnumerable<IEnumerable<KeyBinding>> FullHelp();
}
```

Fluent API via `WithHelp(key, description)` and `Disabled()` extension methods.

## Component Catalog

### Building Blocks

- **CursorModel** — Blinking/static/hidden cursor with configurable blink speed and appearance.
- **SpinnerModel** — Animated spinner with pre-defined types (Line, Dot, MiniDot, Jump, Pulse, Points, Globe, Moon, Hamburger, Ellipsis).
- **PaginatorModel** — Page indicator in dot or numeric format. Methods: `NextPage()`, `PrevPage()`, `SetTotalPages()`, `GetSliceBounds()`.
- **HelpModel** — Renders keybinding help from an `IKeyMap`. Toggles between short and full display with `?`.
- **Divider** — Horizontal divider that fills available width with a repeated character (defaults to box-drawing horizontal line).

### Time

- **TimerModel** — Countdown timer with `Start()`, `Stop()`, `Toggle()`, `Reset()`. Fires public `TimerTimeoutMessage` on completion.
- **StopwatchModel** — Elapsed time tracker with `Start()`, `Stop()`, `Toggle()`, `Reset()`.

### Input

- **TextInputModel** — Single-line text input with prompt, placeholder, echo modes (Normal, Password, None), character limit, validation, and embedded `CursorModel`. Methods: `SetValue()`, `GetValue()`, `Focus()`, `Blur()`.
- **TextAreaModel** — Multi-line text editor with line numbers, scrolling, character limit, and embedded `CursorModel`. Methods: `SetValue()`, `GetValue()`, `Focus()`, `Blur()`, `InsertString()`.

### Data Display

- **ListModel\<TItem\>** — Generic browsable list with filtering, pagination, and status messages. Requires `IListItem` (with `FilterValue`). Embeds `TextInputModel`, `PaginatorModel`, `HelpModel`, `SpinnerModel`. Vim-style keybindings (j/k, g/G).
- **TableModel** — Tabular data display with column definitions, row selection, and scrolling. Embeds `HelpModel`.

### Navigation/Layout

- **ViewportModel** — Read-only scrollable content view with mouse wheel support. Methods: `LineDown()`, `LineUp()`, `PageDown()`, `PageUp()`, `GotoTop()`, `GotoBottom()`.
- **ProgressModel** — Animated progress bar with smooth interpolation. Methods: `SetPercent()`, `IncrPercent()`.

## ISizedWidget

```csharp
public interface ISizedWidget : IWidget
{
    int MinWidth { get; }
    int MinHeight { get; }
}
```

Widgets that declare their minimum intrinsic size implement `ISizedWidget`. `MinWidth`/`MinHeight` represent the smallest space the widget needs — when rendered inside a flex container or layout template, the widget may receive more space via `context.Viewport`.

## Flex Layout

`FlexRow` and `FlexColumn` are flex layout containers that distribute space among child widgets. They implement `ISizedWidget` so they can be nested.

```csharp
var layout = Flex.Column(gap: 1)
    .Add(header, FlexSize.Fixed(3))
    .Add(Flex.Row(gap: 2)
        .Add(sidebar, FlexSize.Fixed(20))
        .Add(content, FlexSize.Fill()),
      FlexSize.Fill())
    .Add(footer, FlexSize.Fixed(1));
```

**Sizing strategies** (`FlexSize`):
- `FlexSize.Fixed(n)` — Exactly `n` cells.
- `FlexSize.Ratio(weight)` — Proportional share of remaining space.
- `FlexSize.Fill()` — Equivalent to `Ratio(1)`.

**Adaptive rendering:** Flex containers read their allocated space from `context.Viewport.Width`/`Height` at render time, not from their own `MinWidth`/`MinHeight`. This means they fill whatever space they're given. When embedded in a `context.Layout(...)` template, the `LayoutHandler` uses `MinWidth`/`MinHeight` to allocate space, so set meaningful minimums if the container will be used in templates.

### Key Files

- `Layout/Flex.cs` — Factory methods `Flex.Row()`, `Flex.Column()`
- `Layout/FlexRow.cs` — Horizontal flex container
- `Layout/FlexColumn.cs` — Vertical flex container
- `Layout/FlexAlgorithm.cs` — Space distribution algorithm
- `Layout/FlexSize.cs` — Sizing strategy types
- `Layout/FlexItem.cs` — Widget + sizing pair
- `Layout/Border.cs` — Box-drawing border frame around child content
- `Layout/Padding.cs` — Configurable top/right/bottom/left padding around child content

## Interpolated View Handler

The `context.Layout(...)` extension method enables declarative view rendering using C# interpolated strings. The compiler wires the `RenderContext` into a `LayoutHandler` ref struct that renders content during string construction.

### Basic Usage

In a doodad's `Render` method, call `context.Layout` with a raw interpolated string:

```csharp
public void Render(RenderContext context)
{
    context.Layout($"""
        {"Dashboard":bold text-blue}

        {Progress}

        Items: {Count}  Status: {Status:dim}
        """);
}
```

- **Literal text** renders at the current cursor position and advances X.
- **Newlines** move to the next row and reset X to 0.
- **Interpolation holes** render strings, numbers, or widgets at the current position.
- **Whitespace between holes** defines relative spacing on the same row.

### Tailwind-Like Styling

Format specifiers use space-separated tokens parsed by `StyleParser`:

| Token | Effect |
|---|---|
| `bold`, `dim`, `italic`, `underline`, `invert`, `strikethrough` | Decoration |
| `text-red`, `text-blue`, `text-cyan`, ... | Named foreground color |
| `text-#rrggbb` | Hex foreground color |
| `bg-red`, `bg-blue`, ... | Named background color |
| `bg-#rrggbb` | Hex background color |

Named colors: `black`, `red`, `green`, `yellow`, `blue`, `magenta`, `cyan`, `white`, `grey`/`gray`.

Examples: `{Title:bold text-cyan}`, `{Count:dim}`, `{label:text-#ff0000 bg-blue}`.

### Widget Holes

- **`ISizedWidget`** (e.g. `ProgressModel`, `SpinnerModel`, `Label`) renders via `context.Render(widget, bounds)` using the widget's declared `MinWidth`/`MinHeight`. Multi-line widgets advance Y past their full height.
- **`IWidget`** that is not `ISizedWidget` falls back to `ToString()` as styled text.
- **Any other type** calls `ToString()` and renders the result as styled text.

### Label

`Label` is an `ISizedWidget` that bundles text and styling for use in layout templates:

```csharp
var label = new Label("Progress", "bold text-green");
context.Layout($"""
    {label}  {Progress}
    """);
```

### Key Files

- `Layout/LayoutHandler.cs` — Interpolated string handler ref struct
- `Layout/RenderContextExtensions.cs` — `context.Layout(...)` entry point
- `Layout/StyleParser.cs` — Tailwind-like token parser
- `Doodads/Label/Label.cs` — Styled text `ISizedWidget`

## Key Patterns

### Immutable Records

All models are `record` types. State updates use `with` expressions:

```csharp
var updated = model with { Property = newValue };
```

### Stale Tick Detection

Animated components use an `Id` + `Tag` pattern. `Id` is unique per instance (static counter via `Interlocked.Increment`). `Tag` increments on state changes. Tick messages carry both values and are ignored on `Tag` mismatch, preventing stale animation frames.

### Sub-component Composition

Parent models embed child doodads and forward messages:

```csharp
var (cursor, cursorCmd) = Cursor.Update(message);
return (this with { Cursor = cursor }, cursorCmd);
```

Notable compositions: `TextInputModel` and `TextAreaModel` embed `CursorModel`; `ListModel` embeds `TextInputModel`, `PaginatorModel`, `HelpModel`, and `SpinnerModel`; `TableModel` embeds `HelpModel`.

### Delegate Pattern

`ListModel<TItem>` uses `IListItemDelegate<TItem>` for customizable item rendering (Height, Spacing, Render methods).

### KeyMap Pattern

Components expose a typed `KeyMap` property (e.g., `TextInputKeyMap`, `ListKeyMap`) implementing `IKeyMap` for configurable keybindings. Each key map defines both short and full help display.

## Key Dependencies

- **Spectre.Tui** — NuGet package reference providing `IWidget`, `RenderContext`, `ITerminal`, primitives
- **Wcwidth.Sources** — Unicode character width calculation
