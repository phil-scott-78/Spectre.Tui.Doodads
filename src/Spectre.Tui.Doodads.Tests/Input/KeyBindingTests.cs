using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Input;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Input;

public sealed class KeyBindingTests
{
    [Fact]
    public void Matches_Should_Return_True_For_Matching_Key()
    {
        // Given
        var binding = KeyBinding.For(Key.Up, Key.Char);
        var message = new KeyMessage { Key = Key.Up };

        // When / Then
        binding.Matches(message).ShouldBeTrue();
    }

    [Fact]
    public void Matches_Should_Return_False_For_Non_Matching_Key()
    {
        // Given
        var binding = KeyBinding.For(Key.Up);
        var message = new KeyMessage { Key = Key.Down };

        // When / Then
        binding.Matches(message).ShouldBeFalse();
    }

    [Fact]
    public void Matches_Should_Return_False_When_Disabled()
    {
        // Given
        var binding = KeyBinding.For(Key.Up).Disabled();
        var message = new KeyMessage { Key = Key.Up };

        // When / Then
        binding.Matches(message).ShouldBeFalse();
    }

    [Fact]
    public void WithHelp_Should_Set_Help_Properties()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Up)
            .WithHelp("↑", "move up");

        // Then
        binding.HelpKey.ShouldBe("↑");
        binding.HelpDescription.ShouldBe("move up");
    }

    [Fact]
    public void Disabled_Should_Set_Enabled_False()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Up).Disabled();

        // Then
        binding.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void For_Should_Create_Binding_With_Keys()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Up, Key.Char);

        // Then
        binding.Keys.Count.ShouldBe(2);
        binding.Keys[0].ShouldBe(Key.Up);
        binding.Keys[1].ShouldBe(Key.Char);
        binding.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void Re_Enabling_A_Disabled_Binding()
    {
        // Given
        var binding = KeyBinding.For(Key.Up).Disabled();
        var message = new KeyMessage { Key = Key.Up };

        // When
        var enabled = binding with { Enabled = true };

        // Then
        enabled.Matches(message).ShouldBeTrue();
    }

    [Fact]
    public void Matches_Should_Check_Alt_Modifier()
    {
        // Given
        var binding = KeyBinding.For(Key.Right).WithAlt();
        var withAlt = new KeyMessage { Key = Key.Right, Alt = true };
        var withoutAlt = new KeyMessage { Key = Key.Right, Alt = false };

        // When / Then
        binding.Matches(withAlt).ShouldBeTrue();
        binding.Matches(withoutAlt).ShouldBeFalse();
    }

    [Fact]
    public void Matches_Should_Check_Ctrl_Modifier()
    {
        // Given
        var binding = KeyBinding.For(Key.Home).WithCtrl();
        var withCtrl = new KeyMessage { Key = Key.Home, Ctrl = true };
        var withoutCtrl = new KeyMessage { Key = Key.Home, Ctrl = false };

        // When / Then
        binding.Matches(withCtrl).ShouldBeTrue();
        binding.Matches(withoutCtrl).ShouldBeFalse();
    }

    [Fact]
    public void Matches_Should_Check_Shift_Modifier()
    {
        // Given
        var binding = KeyBinding.For(Key.Tab).WithShift();
        var withShift = new KeyMessage { Key = Key.Tab, Shift = true };
        var withoutShift = new KeyMessage { Key = Key.Tab, Shift = false };

        // When / Then
        binding.Matches(withShift).ShouldBeTrue();
        binding.Matches(withoutShift).ShouldBeFalse();
    }

    [Fact]
    public void Matches_Should_Ignore_Null_Modifiers()
    {
        // Given — no modifier constraints (null by default)
        var binding = KeyBinding.For(Key.Right);
        var withAlt = new KeyMessage { Key = Key.Right, Alt = true };
        var withoutAlt = new KeyMessage { Key = Key.Right, Alt = false };

        // When / Then — both should match since Alt is null (don't care)
        binding.Matches(withAlt).ShouldBeTrue();
        binding.Matches(withoutAlt).ShouldBeTrue();
    }

    [Fact]
    public void Matches_WithoutModifiers_Should_Reject_Any_Modifier()
    {
        // Given
        var binding = KeyBinding.For(Key.Right).WithoutModifiers();

        // When / Then
        binding.Matches(new KeyMessage { Key = Key.Right }).ShouldBeTrue();
        binding.Matches(new KeyMessage { Key = Key.Right, Alt = true }).ShouldBeFalse();
        binding.Matches(new KeyMessage { Key = Key.Right, Ctrl = true }).ShouldBeFalse();
        binding.Matches(new KeyMessage { Key = Key.Right, Shift = true }).ShouldBeFalse();
    }

    [Fact]
    public void ForChar_Should_Create_Char_Binding()
    {
        // Given / When
        var binding = KeyBinding.ForChar('a', 'b');

        // Then
        binding.Keys.ShouldBe([Key.Char]);
        binding.Runes.Length.ShouldBe(2);
        binding.Runes[0].ShouldBe(new Rune('a'));
        binding.Runes[1].ShouldBe(new Rune('b'));
    }

    [Fact]
    public void WithAlt_Should_Set_Alt_Property()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Right).WithAlt();

        // Then
        binding.Alt.ShouldBe(true);
    }

    [Fact]
    public void WithAlt_False_Should_Set_Alt_False()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Right).WithAlt(false);

        // Then
        binding.Alt.ShouldBe(false);
    }

    [Fact]
    public void WithCtrl_Should_Set_Ctrl_Property()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Home).WithCtrl();

        // Then
        binding.Ctrl.ShouldBe(true);
    }

    [Fact]
    public void WithShift_Should_Set_Shift_Property()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Tab).WithShift();

        // Then
        binding.Shift.ShouldBe(true);
    }

    [Fact]
    public void WithoutModifiers_Should_Set_All_Modifiers_False()
    {
        // Given / When
        var binding = KeyBinding.For(Key.Right).WithoutModifiers();

        // Then
        binding.Alt.ShouldBe(false);
        binding.Ctrl.ShouldBe(false);
        binding.Shift.ShouldBe(false);
    }
}