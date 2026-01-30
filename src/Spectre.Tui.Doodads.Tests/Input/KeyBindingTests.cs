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
}