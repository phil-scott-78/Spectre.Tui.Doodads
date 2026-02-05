namespace Spectre.Tui.Doodads;

/// <summary>
/// Extension methods for composing child doodads within a parent.
/// </summary>
public static class DoodadExtensions
{
    /// <summary>
    /// Forwards a message to a child doodad and applies its updated state back to the parent.
    /// </summary>
    /// <typeparam name="TParent">The parent doodad type.</typeparam>
    /// <typeparam name="TChild">The child doodad type.</typeparam>
    /// <param name="parent">The parent model.</param>
    /// <param name="message">The message to forward.</param>
    /// <param name="get">Extracts the child from the parent.</param>
    /// <param name="set">Produces a new parent with the updated child.</param>
    /// <returns>The updated parent and any command from the child.</returns>
    public static (TParent Model, Command? Command) Forward<TParent, TChild>(
        this TParent parent,
        Message message,
        Func<TParent, TChild> get,
        Func<TParent, TChild, TParent> set)
        where TParent : IDoodad<TParent>
        where TChild : IDoodad<TChild>
    {
        var (updatedChild, cmd) = get(parent).Update(message);
        return (set(parent, updatedChild), cmd);
    }

    /// <summary>
    /// Chains a child forward onto a previous forward result, batching commands.
    /// </summary>
    /// <typeparam name="TParent">The parent doodad type.</typeparam>
    /// <typeparam name="TChild">The child doodad type.</typeparam>
    /// <param name="result">The result from a previous forward call.</param>
    /// <param name="message">The message to forward.</param>
    /// <param name="get">Extracts the child from the parent.</param>
    /// <param name="set">Produces a new parent with the updated child.</param>
    /// <returns>The updated parent and batched commands.</returns>
    public static (TParent Model, Command? Command) Forward<TParent, TChild>(
        this (TParent Model, Command? Command) result,
        Message message,
        Func<TParent, TChild> get,
        Func<TParent, TChild, TParent> set)
        where TParent : IDoodad<TParent>
        where TChild : IDoodad<TChild>
    {
        var (updatedChild, cmd) = get(result.Model).Update(message);
        return (set(result.Model, updatedChild), Commands.Batch(result.Command, cmd));
    }
}
