namespace Spectre.Tui.Doodads.Input;

/// <summary>
/// Reads input from the console and converts to TEA messages.
/// </summary>
internal sealed class ConsoleInputReader : IInputReader
{
    private int _lastWidth;
    private int _lastHeight;
    private bool _initialSizeSent;

    public ConsoleInputReader()
    {
        _lastWidth = SysConsole.WindowWidth;
        _lastHeight = SysConsole.WindowHeight;
    }

    public ValueTask<Message?> ReadAsync(CancellationToken cancellationToken)
    {
        // Check for window resize (also sends size on first call)
        var width = SysConsole.WindowWidth;
        var height = SysConsole.WindowHeight;
        if (!_initialSizeSent || width != _lastWidth || height != _lastHeight)
        {
            _initialSizeSent = true;
            _lastWidth = width;
            _lastHeight = height;
            return new ValueTask<Message?>(
                new WindowSizeMessage { Width = width, Height = height });
        }

        // Check for key input
        if (!SysConsole.KeyAvailable)
        {
            return new ValueTask<Message?>((Message?)null);
        }

        var keyInfo = SysConsole.ReadKey(intercept: true);
        var message = ConvertKey(keyInfo);
        return new ValueTask<Message?>(message);
    }

    public void Dispose()
    {
        // No resources to dispose
    }

    private static KeyMessage ConvertKey(ConsoleKeyInfo keyInfo)
    {
        var key = MapKey(keyInfo);
        var runes = keyInfo.KeyChar != '\0'
            ? [new Rune(keyInfo.KeyChar)]
            : Array.Empty<Rune>();

        return new KeyMessage
        {
            Key = key,
            Runes = runes,
            Alt = (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0,
            Shift = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0,
            Ctrl = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0,
        };
    }

    private static Key MapKey(ConsoleKeyInfo keyInfo)
    {
        // Handle Ctrl combinations first
        if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
        {
            return keyInfo.Key switch
            {
                ConsoleKey.A => Key.CtrlA,
                ConsoleKey.B => Key.CtrlB,
                ConsoleKey.C => Key.CtrlC,
                ConsoleKey.D => Key.CtrlD,
                ConsoleKey.E => Key.CtrlE,
                ConsoleKey.F => Key.CtrlF,
                ConsoleKey.G => Key.CtrlG,
                ConsoleKey.H => Key.CtrlH,
                ConsoleKey.I => Key.CtrlI,
                ConsoleKey.J => Key.CtrlJ,
                ConsoleKey.K => Key.CtrlK,
                ConsoleKey.L => Key.CtrlL,
                ConsoleKey.M => Key.CtrlM,
                ConsoleKey.N => Key.CtrlN,
                ConsoleKey.O => Key.CtrlO,
                ConsoleKey.P => Key.CtrlP,
                ConsoleKey.Q => Key.CtrlQ,
                ConsoleKey.R => Key.CtrlR,
                ConsoleKey.S => Key.CtrlS,
                ConsoleKey.T => Key.CtrlT,
                ConsoleKey.U => Key.CtrlU,
                ConsoleKey.V => Key.CtrlV,
                ConsoleKey.W => Key.CtrlW,
                ConsoleKey.X => Key.CtrlX,
                ConsoleKey.Y => Key.CtrlY,
                ConsoleKey.Z => Key.CtrlZ,
                _ => MapNonCtrlKey(keyInfo),
            };
        }

        return MapNonCtrlKey(keyInfo);
    }

    private static Key MapNonCtrlKey(ConsoleKeyInfo keyInfo)
    {
        return keyInfo.Key switch
        {
            ConsoleKey.UpArrow => Key.Up,
            ConsoleKey.DownArrow => Key.Down,
            ConsoleKey.LeftArrow => Key.Left,
            ConsoleKey.RightArrow => Key.Right,
            ConsoleKey.Home => Key.Home,
            ConsoleKey.End => Key.End,
            ConsoleKey.PageUp => Key.PageUp,
            ConsoleKey.PageDown => Key.PageDown,
            ConsoleKey.Backspace => Key.Backspace,
            ConsoleKey.Delete => Key.Delete,
            ConsoleKey.Insert => Key.Insert,
            ConsoleKey.Tab when (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0 => Key.ShiftTab,
            ConsoleKey.Tab => Key.Tab,
            ConsoleKey.Enter => Key.Enter,
            ConsoleKey.Escape => Key.Escape,
            ConsoleKey.Spacebar => Key.Space,
            ConsoleKey.F1 => Key.F1,
            ConsoleKey.F2 => Key.F2,
            ConsoleKey.F3 => Key.F3,
            ConsoleKey.F4 => Key.F4,
            ConsoleKey.F5 => Key.F5,
            ConsoleKey.F6 => Key.F6,
            ConsoleKey.F7 => Key.F7,
            ConsoleKey.F8 => Key.F8,
            ConsoleKey.F9 => Key.F9,
            ConsoleKey.F10 => Key.F10,
            ConsoleKey.F11 => Key.F11,
            ConsoleKey.F12 => Key.F12,
            ConsoleKey.F13 => Key.F13,
            ConsoleKey.F14 => Key.F14,
            ConsoleKey.F15 => Key.F15,
            ConsoleKey.F16 => Key.F16,
            ConsoleKey.F17 => Key.F17,
            ConsoleKey.F18 => Key.F18,
            ConsoleKey.F19 => Key.F19,
            ConsoleKey.F20 => Key.F20,
            ConsoleKey.F21 => Key.F21,
            ConsoleKey.F22 => Key.F22,
            ConsoleKey.F23 => Key.F23,
            ConsoleKey.F24 => Key.F24,
            _ => keyInfo.KeyChar != '\0' ? Key.Char : Key.None,
        };
    }
}