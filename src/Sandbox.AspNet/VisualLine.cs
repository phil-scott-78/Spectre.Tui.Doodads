namespace Sandbox.AspNet;

internal enum VisualLineKind
{
    EntryFirstLine,
    MessageContinuation,
    ExceptionLine,
}

internal readonly record struct VisualLine(
    int EntryIndex,
    VisualLineKind Kind,
    int LineWithinEntry);