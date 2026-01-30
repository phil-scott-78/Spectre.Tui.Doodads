# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Spectre.Tui.Doodads is a .NET component library for building interactive Terminal User Interface (TUI) applications using The Elm Architecture (TEA). It provides immutable model records, message passing, and side-effect commands built on top of the **Spectre.Tui** NuGet package (an external dependency, not part of this repository). The project targets .NET 10.0 and uses C# 14 with nullable reference types enabled.

**Status:** Alpha/pre-release — breaking changes may occur.

## Project-Specific Documentation

- **[src/Spectre.Tui.Doodads/CLAUDE.md](src/Spectre.Tui.Doodads/CLAUDE.md)** — Doodads architecture: IDoodad interface, command system, program runner, message hierarchy, component catalog, and key patterns.
- **[src/Spectre.Tui.Doodads.Tests/CLAUDE.md](src/Spectre.Tui.Doodads.Tests/CLAUDE.md)** — Test suite: DoodadFixture harness, test conventions, directory structure, and snapshot testing.
- **[src/Sandbox.Doodads/CLAUDE.md](src/Sandbox.Doodads/CLAUDE.md)** — Demo app: menu-driven example launcher with Counter, Todo List, Text Editor, Dashboard, Flex Layout, Form, and Speed Test examples.

## Build Commands

The build system uses [Cake](https://cakebuild.net/) via `build.cs`. The solution file is `Spectre.Tui.Doodads.slnx`.

```shell
# Full pipeline (clean → lint → build → test → package) — default target
dotnet build.cs

# Run specific targets
dotnet build.cs --target=Build
dotnet build.cs --target=Test
dotnet build.cs --target=Lint

# Quick dev build (bypasses Cake)
dotnet build Spectre.Tui.Doodads.slnx

# Run tests directly
dotnet test Spectre.Tui.Doodads.slnx
dotnet test Spectre.Tui.Doodads.Tests/Spectre.Tui.Doodads.Tests.csproj

# Run a single test
dotnet test Spectre.Tui.Doodads.Tests/Spectre.Tui.Doodads.Tests.csproj --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Check formatting
dotnet format style Spectre.Tui.Doodads.slnx --verify-no-changes
```

Build treats all warnings as errors.

## Solution Structure

```
src/
├── Spectre.Tui.Doodads/      # TEA component library (NuGet package)
├── Spectre.Tui.Doodads.Tests/ # xUnit tests with snapshot verification
└── Sandbox.Doodads/           # Demo/example application
```

## Testing

Tests use **xUnit** with **Shouldly** assertions and **Verify** for snapshot testing.

The primary test harness is `DoodadFixture<TModel>`, which wraps a doodad model, processes commands synchronously, and provides a fluent API:

```csharp
var fixture = new DoodadFixture<CounterModel>(new CounterModel());
fixture.Init()
    .SendKey(Key.Up)
    .SendKey(Key.Up);
fixture.Model.Count.ShouldBe(2);
```

Rendering tests use `TuiFixture` (vendored from Spectre.Tui) for snapshot output where `'•'` represents empty cells.

Snapshot tests produce `.verified.txt` files — when adding new snapshot tests, run once to generate the expected output, then verify it manually.

## Hooks & Constraints

Hooks in `.claude/` enforce the following rules. Respect these to avoid blocked operations:

- **No `.csx` files.** This project does not use C# Script. Use `.cs` single-file applications instead.
- **No piping to `nul`.** Do not redirect output to the Windows null device. Let command output remain visible.
- **Use built-in tools over Bash equivalents.** Do not use `find`, `grep`, `rg`, `ls`, `dir`, `cat`, `head`, `sed`, `awk`, or `echo` via Bash. Use the Glob, Grep, Read, Edit, and Write tools instead.
- **Auto-formatting on save.** A PostToolUse hook runs `dotnet format` on any `.cs` file after Write or Edit operations.

## Key Dependencies

- **Spectre.Tui** — NuGet package providing `IWidget`, `RenderContext`, `ITerminal`, and primitives
- **Wcwidth.Sources** — Unicode character width (source-included package)
- **MinVer** — Semantic versioning from git tags
- **Roslynator.Analyzers** — Static analysis (enabled project-wide)
