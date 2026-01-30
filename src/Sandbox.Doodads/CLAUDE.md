# Sandbox.Doodads

Interactive demo/example application for Spectre.Tui.Doodads components. Not packable — this is a development and reference tool only.

## How It Works

The app is a menu-driven launcher. `Program.cs` creates a single shared terminal instance and runs a loop: the `MenuModel` presents available examples, and on selection, the chosen example runs via `Program.RunAsync`. All examples share the same terminal, returning to the menu on exit.

## Available Examples

| Example | Model | Description |
|---------|-------|-------------|
| Counter | `CounterModel` | Simple counter with up/down keys |
| Todo List | `TodoModel` | Todo list with add, toggle, and delete |
| Text Editor | `TextEditorModel` | Multi-line text editor with status bar |
| Dashboard | `DashboardModel` | Multi-panel display dashboard |
| Flex Layout | `FlexLayoutModel` | Responsive flex layout boxes that adapt to terminal size |
| Form | `FormModel` | Form with input fields; displays submitted results on exit |
| Speed Test | `SpeedTestModel` | Rendering speed/performance test |

## Structure

Each example is a single model file implementing `IDoodad<TSelf>` directly — these serve as good reference implementations for how to build doodads.

- `Program.cs` — Entry point and example launcher loop
- `MenuModel.cs` — Menu model using `ListModel<MenuEntry>` for example selection
- `CounterModel.cs`, `TodoModel.cs`, `TextEditorModel.cs`, `DashboardModel.cs`, `FlexLayoutModel.cs`, `FormModel.cs`, `SpeedTestModel.cs` — Example models

## Running

```shell
dotnet run --project src/Sandbox.Doodads
```
