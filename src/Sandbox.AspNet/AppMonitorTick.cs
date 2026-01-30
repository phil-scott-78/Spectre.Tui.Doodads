using Spectre.Tui.Doodads.Messages;

namespace Sandbox.AspNet;

/// <summary>
/// Internal tick message for polling the log store.
/// </summary>
internal record AppMonitorTick : Message;