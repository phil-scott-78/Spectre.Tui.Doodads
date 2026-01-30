using Sandbox.Doodads;
using Spectre.Tui;

var useInline = args.Contains("--inline", StringComparer.OrdinalIgnoreCase);

ITerminalMode mode = useInline
    ? new InlineMode(24)
    : new FullscreenMode();

using var terminal = Terminal.Create(mode);

while (true)
{
    var menuResult = await Spectre.Tui.Doodads.Program.RunAsync(
        new MenuModel(), opts =>
        {
            opts.Terminal = terminal;
            opts.TerminalMode = mode;
        });

    if (menuResult.SelectedExample is null)
    {
        break;
    }

    switch (menuResult.SelectedExample.Value)
    {
        case 0:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new CounterModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
        case 1:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new TodoModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
        case 2:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new TextEditorModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
        case 3:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new DashboardModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
        case 4:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new FlexLayoutModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
        case 5:
            var formResult = await Spectre.Tui.Doodads.Program.RunAsync(
                new FormModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            if (formResult.Submitted)
            {
                Console.WriteLine();
                Console.WriteLine("=== Form Submitted ===");
                Console.WriteLine($"  Name:     {formResult.NameInput.GetValue()}");
                Console.WriteLine($"  Email:    {formResult.EmailInput.GetValue()}");
                Console.WriteLine($"  Password: {new string('*', formResult.PasswordInput.GetValue().Length)}");
                Console.WriteLine($"  Notes:    {formResult.NotesInput.GetValue()}");
                Console.WriteLine();
                Console.WriteLine("Press any key to return to menu...");
                Console.ReadKey(true);
            }

            break;
        case 6:
            await Spectre.Tui.Doodads.Program.RunAsync(
                new SpeedTestModel(), opts =>
                {
                    opts.Terminal = terminal;
                    opts.TerminalMode = mode;
                });
            break;
    }
}