using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.TextArea;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A multi-input form demo with validation, email suggestions, and focus management.
/// </summary>
public record FormModel : IDoodad<FormModel>, ISizedRenderable
{
    public int MinWidth => 60;
    public int MinHeight => 20;

    private static readonly string[] EmailDomains =
    [
        "@gmail.com",
        "@outlook.com",
        "@yahoo.com",
        "@hotmail.com",
        "@icloud.com",
        "@protonmail.com",
    ];

    /// <summary>
    /// Gets the index of the currently focused field (0=Name, 1=Email, 2=Password, 3=Notes).
    /// </summary>
    public int FocusedField { get; init; }

    /// <summary>
    /// Gets the name input.
    /// </summary>
    public TextInputModel NameInput { get; init; } = new()
    {
        Prompt = "Name: ",
        Placeholder = "Enter your name",
        CharLimit = 100,
        PromptStyle = new Appearance { Foreground = Color.Cyan1 },
        Validate = value => !value.Any(c => char.IsControl(c)),
    };

    /// <summary>
    /// Gets the email input.
    /// </summary>
    public TextInputModel EmailInput { get; init; } = new()
    {
        Prompt = "Email: ",
        Placeholder = "Enter your email",
        CharLimit = 254,
        ShowSuggestions = true,
        Validate = value => !value.Contains(' ') && !value.Contains('\t'),
    };

    /// <summary>
    /// Gets the password input.
    /// </summary>
    public TextInputModel PasswordInput { get; init; } = new()
    {
        Prompt = "Password: ",
        EchoMode = EchoMode.Password,
        CharLimit = 128,
    };

    /// <summary>
    /// Gets the notes text area.
    /// </summary>
    public TextAreaModel NotesInput { get; init; } = new()
    {
        ShowLineNumbers = true,
        CharLimit = 500,
        MinHeight = 6,
    };

    /// <summary>
    /// Gets the validation error for the name field.
    /// </summary>
    public string? NameError { get; init; }

    /// <summary>
    /// Gets the validation error for the email field.
    /// </summary>
    public string? EmailError { get; init; }

    /// <summary>
    /// Gets the validation error for the password field.
    /// </summary>
    public string? PasswordError { get; init; }

    /// <summary>
    /// Gets the validation error for the notes field.
    /// </summary>
    public string? NotesError { get; init; }

    /// <summary>
    /// Gets a value indicating whether the form was submitted.
    /// </summary>
    public bool Submitted { get; init; }

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
        var (focusedName, focusCmd) = NameInput.Focus();
        return Commands.Batch(
            focusedName.Init(),
            focusCmd,
            EmailInput.Init(),
            PasswordInput.Init(),
            NotesInput.Init(),
            Commands.Message(new FormInitFocused { NameInput = focusedName }));
    }

    /// <inheritdoc />
    public (FormModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case FormInitFocused init:
                return (this with { NameInput = init.NameInput }, null);

            case KeyMessage { Key: Key.Escape }:
            case KeyMessage { Key: Key.CtrlC }:
                return (this, Commands.Quit());

            case KeyMessage { Key: Key.CtrlS }:
                return TrySubmit();

            case KeyMessage { Key: Key.Tab }:
                return MoveFocus(1);

            case KeyMessage { Key: Key.ShiftTab }:
                return MoveFocus(-1);

            case KeyMessage { Key: Key.Enter } when FocusedField < 3:
                return MoveFocus(1);

            case WindowSizeMessage ws:
                return HandleResize(ws);

            default:
                return ForwardToFocusedChild(message);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var y = 0;

        // Title
        var titleStyle = new Appearance { Decoration = Decoration.Bold };
        surface.SetString(0, y, "Registration Form", titleStyle);
        y += 2;

        // Name input
        if (y < height)
        {
            surface.Render(NameInput, new Rectangle(0, y, width, 1));
            y++;
        }

        if (y < height)
        {
            RenderError(surface, 2, y, NameError, width);
            y++;
        }

        // Email input
        if (y < height)
        {
            surface.Render(EmailInput, new Rectangle(0, y, width, 1));
            y++;
        }

        if (y < height)
        {
            RenderError(surface, 2, y, EmailError, width);
            y++;
        }

        // Password input
        if (y < height)
        {
            surface.Render(PasswordInput, new Rectangle(0, y, width, 1));
            y++;
        }

        if (y < height)
        {
            RenderError(surface, 2, y, PasswordError, width);
            y++;
        }

        // Notes label with character counter
        if (y < height)
        {
            var notesLabel = "Notes:";
            var notesLength = NotesInput.Length();
            var counter = $"{notesLength}/500";
            var counterStyle = notesLength >= 500
                ? new Appearance { Foreground = Color.Red }
                : new Appearance { Decoration = Decoration.Dim };
            surface.SetString(0, y, notesLabel, FocusedField == 3
                ? new Appearance { Foreground = Color.Cyan1 }
                : Appearance.Plain);
            var counterX = Math.Max(0, width - counter.Length);
            surface.SetString(counterX, y, counter, counterStyle);
            y++;
        }

        // Notes text area (fill remaining space minus help line)
        var notesHeight = Math.Max(1, height - y - 1);
        if (y < height - 1)
        {
            surface.Render(NotesInput, new Rectangle(0, y, width, notesHeight));
            y += notesHeight;
        }

        // Help bar
        if (y < height)
        {
            var helpStyle = new Appearance { Decoration = Decoration.Dim };
            surface.SetString(0, y, "Tab/Shift+Tab:navigate  Enter:next  Ctrl+S:submit  Esc:quit", helpStyle);
        }
    }

    private static void RenderError(IRenderSurface surface, int x, int y, string? error, int width)
    {
        if (error is not null)
        {
            var errorStyle = new Appearance { Foreground = Color.Red };
            var maxLen = Math.Max(0, width - x);
            var text = error.Length > maxLen ? error[..maxLen] : error;
            surface.SetString(x, y, text, errorStyle);
        }
    }

    private (FormModel Model, Command? Command) MoveFocus(int direction)
    {
        var model = ValidateField(FocusedField);
        model = model.BlurField(model.FocusedField, out var blurCmd);

        var next = ((model.FocusedField + direction) % 4 + 4) % 4;
        model = model.FocusField(next, out var focusCmd);

        return (model, Commands.Batch(blurCmd, focusCmd));
    }

    private FormModel BlurField(int field, out Command? cmd)
    {
        cmd = null;
        return field switch
        {
            0 => BlurInput(NameInput, out cmd, m => this with { NameInput = m }),
            1 => BlurInput(EmailInput, out cmd, m => this with { EmailInput = m }),
            2 => BlurInput(PasswordInput, out cmd, m => this with { PasswordInput = m }),
            3 => BlurTextArea(out cmd),
            _ => this,
        };
    }

    private FormModel BlurInput(TextInputModel input, out Command? cmd, Func<TextInputModel, FormModel> apply)
    {
        var (blurred, blurCmd) = input.Blur();
        blurred = blurred with { PromptStyle = Appearance.Plain };
        cmd = blurCmd;
        return apply(blurred);
    }

    private FormModel BlurTextArea(out Command? cmd)
    {
        var (blurred, blurCmd) = NotesInput.Blur();
        cmd = blurCmd;
        return this with { NotesInput = blurred };
    }

    private FormModel FocusField(int field, out Command? cmd)
    {
        cmd = null;
        var model = this with { FocusedField = field };
        return field switch
        {
            0 => model.FocusInput(model.NameInput, out cmd, m => model with { NameInput = m }),
            1 => model.FocusInput(model.EmailInput, out cmd, m => model with { EmailInput = m }),
            2 => model.FocusInput(model.PasswordInput, out cmd, m => model with { PasswordInput = m }),
            3 => model.FocusTextArea(out cmd),
            _ => model,
        };
    }

    private FormModel FocusInput(TextInputModel input, out Command? cmd, Func<TextInputModel, FormModel> apply)
    {
        var (focused, focusCmd) = input.Focus();
        focused = focused with { PromptStyle = new Appearance { Foreground = Color.Cyan1 } };
        cmd = focusCmd;
        return apply(focused);
    }

    private FormModel FocusTextArea(out Command? cmd)
    {
        var (focused, focusCmd) = NotesInput.Focus();
        cmd = focusCmd;
        return this with { NotesInput = focused };
    }

    private FormModel ValidateField(int field)
    {
        return field switch
        {
            0 => ValidateName(),
            1 => ValidateEmail(),
            2 => ValidatePassword(),
            _ => this,
        };
    }

    private FormModel ValidateName()
    {
        var value = NameInput.GetValue();
        if (string.IsNullOrWhiteSpace(value))
        {
            return this with { NameError = "Name is required" };
        }

        if (value.Length < 2)
        {
            return this with { NameError = "Name must be at least 2 characters" };
        }

        return this with { NameError = null };
    }

    private FormModel ValidateEmail()
    {
        var value = EmailInput.GetValue();
        if (string.IsNullOrWhiteSpace(value))
        {
            return this with { EmailError = "Email is required" };
        }

        var atIndex = value.IndexOf('@');
        if (atIndex < 1 || atIndex == value.Length - 1)
        {
            return this with { EmailError = "Enter a valid email address" };
        }

        var domain = value[(atIndex + 1)..];
        if (!domain.Contains('.') || domain.EndsWith('.') || domain.StartsWith('.'))
        {
            return this with { EmailError = "Enter a valid email address" };
        }

        return this with { EmailError = null };
    }

    private FormModel ValidatePassword()
    {
        var value = PasswordInput.GetValue();
        if (string.IsNullOrWhiteSpace(value))
        {
            return this with { PasswordError = "Password is required" };
        }

        if (value.Length < 8)
        {
            return this with { PasswordError = "Password must be at least 8 characters" };
        }

        return this with { PasswordError = null };
    }

    private (FormModel Model, Command? Command) TrySubmit()
    {
        var model = this
            .ValidateName()
            .ValidateEmail()
            .ValidatePassword();

        // Find first field with error and focus it
        if (model.NameError is not null)
        {
            return FocusOnError(model, 0);
        }

        if (model.EmailError is not null)
        {
            return FocusOnError(model, 1);
        }

        if (model.PasswordError is not null)
        {
            return FocusOnError(model, 2);
        }

        return (model with { Submitted = true }, Commands.Quit());
    }

    private static (FormModel Model, Command? Command) FocusOnError(FormModel model, int field)
    {
        if (model.FocusedField != field)
        {
            model = model.BlurField(model.FocusedField, out var blurCmd);
            model = model.FocusField(field, out var focusCmd);
            return (model, Commands.Batch(blurCmd, focusCmd));
        }

        return (model, null);
    }

    private (FormModel Model, Command? Command) HandleResize(WindowSizeMessage ws)
    {
        var model = this with
        {
            TerminalWidth = ws.Width,
            TerminalHeight = ws.Height,
        };

        return model
            .Forward(ws, m => m.NameInput, (m, v) => m with { NameInput = v })
            .Forward(ws, m => m.EmailInput, (m, v) => m with { EmailInput = v })
            .Forward(ws, m => m.PasswordInput, (m, v) => m with { PasswordInput = v })
            .Forward(ws, m => m.NotesInput, (m, v) => m with { NotesInput = v });
    }

    private (FormModel Model, Command? Command) ForwardToFocusedChild(Message message)
    {
        return FocusedField switch
        {
            0 => this.Forward(message, m => m.NameInput, (m, v) => m with { NameInput = v }),
            1 => ForwardToEmail(message),
            2 => this.Forward(message, m => m.PasswordInput, (m, v) => m with { PasswordInput = v }),
            3 => this.Forward(message, m => m.NotesInput, (m, v) => m with { NotesInput = v }),
            _ => (this, null),
        };
    }

    private (FormModel Model, Command? Command) ForwardToEmail(Message message)
    {
        var result = this.Forward(message, m => m.EmailInput, (m, v) => m with { EmailInput = v });
        return (result.Model.UpdateEmailSuggestions(), result.Command);
    }

    private FormModel UpdateEmailSuggestions()
    {
        var value = EmailInput.GetValue();
        if (!value.Contains('@'))
        {
            if (EmailInput.Suggestions.Count > 0)
            {
                return this with { EmailInput = EmailInput.SetSuggestions([]) };
            }

            return this;
        }

        var atIndex = value.IndexOf('@');
        var localPart = value[..atIndex];
        var suggestions = new List<string>(EmailDomains.Length);
        foreach (var domain in EmailDomains)
        {
            suggestions.Add(localPart + domain);
        }

        return this with { EmailInput = EmailInput.SetSuggestions(suggestions) };
    }

}

/// <summary>
/// Internal message to carry the focused name input back after Init.
/// </summary>
internal record FormInitFocused : Message
{
    public required TextInputModel NameInput { get; init; }
}
