namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Pairs an <see cref="ISizedRenderable"/> with its <see cref="FlexSize"/> strategy
/// for use in a flex layout container.
/// </summary>
/// <param name="Widget">The renderable to render.</param>
/// <param name="Size">The sizing strategy for this item.</param>
internal record FlexItem(ISizedRenderable Widget, FlexSize Size);
