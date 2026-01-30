# Spectre.Tui.Doodads.Tests

xUnit test suite for the Spectre.Tui.Doodads library.

## Test Infrastructure

### DoodadFixture\<TModel\>

Primary test harness (`DoodadFixture.cs`). Wraps a doodad model, processes commands synchronously, and provides a fluent API:

- `Init()` — Runs `IDoodad.Init()` and processes the returned command.
- `Send(Message)` — Sends a message through `Update` and applies the state change.
- `SendKey(Key, alt, shift, ctrl)` — Sends a `KeyMessage`.
- `SendChar(char)` — Sends a character `KeyMessage`.
- `SendKeys(params Key[])` — Sends multiple key messages in sequence.
- `Render()` — Renders the current model via `TuiFixture` and returns a string grid.

Commands are processed synchronously (one level deep) to avoid infinite tick loops in tests.

### Spectre.Tui.Testing

The `Spectre.Tui.Testing/` directory contains rendering test infrastructure vendored from the Spectre.Tui package:

- `TuiFixture` — Renders widgets to a string grid. Empty cells are shown as `'•'`.
- `SimpleTestTerminal` / `AnsiTestTerminal` — Test terminal implementations.
- `ITestTerminal` — Test terminal interface.

## Test Conventions

- **Given/When/Then** comment structure in each test method.
- **Shouldly** assertions (`ShouldBe`, `ShouldNotBeNull`, `ShouldBeTrue`, etc.).
- **Verify** for snapshot tests — produces `.verified.txt` files.
- One test class per component, named `{Component}Tests`.

## Directory Structure

```
├── Messages/       # Message type tests
├── Input/          # KeyBinding and input tests
├── Doodads/        # One test file per component (CursorModel, SpinnerModel, etc.)
├── Layout/         # Flex layout, style parser, border, padding, and label tests
├── Integration/    # DoodadFixture integration tests, stale tick tests
├── Spectre.Tui.Testing/  # Vendored rendering test infrastructure
├── DoodadFixture.cs      # Primary test harness
└── CommandTests.cs       # Command system tests
```

## Running Tests

```shell
# Run all tests
dotnet test src/Spectre.Tui.Doodads.Tests/Spectre.Tui.Doodads.Tests.csproj

# Run a single test
dotnet test src/Spectre.Tui.Doodads.Tests/Spectre.Tui.Doodads.Tests.csproj --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

## Snapshot Tests

Snapshot tests produce `.verified.txt` files. When adding new snapshot tests, run once to generate the expected output file, then verify its contents manually before committing.
