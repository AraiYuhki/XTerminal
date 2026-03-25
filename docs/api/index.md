# API Reference

This section contains the API documentation generated from source code comments.

## Core Classes

| Class | Description |
|-------|-------------|
| [Terminal](Xeon.XTerminal.Terminal.html) | Main entry point for CLI execution |
| [CommandRegistry](Xeon.XTerminal.CommandRegistry.html) | Command registration and lookup |
| [CommandContext](Xeon.XTerminal.CommandContext.html) | Execution context for commands |

## Interfaces

| Interface | Description |
|-----------|-------------|
| [ICommand](Xeon.XTerminal.ICommand.html) | Command implementation interface |
| [IAsyncTextReader](Xeon.XTerminal.IAsyncTextReader.html) | Async text input stream |
| [IAsyncTextWriter](Xeon.XTerminal.IAsyncTextWriter.html) | Async text output stream |

## Attributes

| Attribute | Description |
|-----------|-------------|
| [CommandAttribute](Xeon.XTerminal.CommandAttribute.html) | Marks a class as a command |
| [OptionAttribute](Xeon.XTerminal.OptionAttribute.html) | Marks a field as a command option |

## Enums

| Enum | Description |
|------|-------------|
| [ExitCode](Xeon.XTerminal.ExitCode.html) | Command exit codes |

## UniTask Support

When UniTask is installed, these additional types are available:

| Type | Description |
|------|-------------|
| [IUniTaskCommand](Xeon.XTerminal.IUniTaskCommand.html) | UniTask-based command interface |
| [UniTaskCommandContext](Xeon.XTerminal.UniTaskCommandContext.html) | UniTask execution context |
| [IUniTaskTextReader](Xeon.XTerminal.IUniTaskTextReader.html) | UniTask text input stream |
| [IUniTaskTextWriter](Xeon.XTerminal.IUniTaskTextWriter.html) | UniTask text output stream |

## FlyweightScrollView

| Type | Description |
|------|-------------|
| [CircularBuffer&lt;T&gt;](Xeon.Common.FlyweightScrollView.Model.CircularBuffer-1.html) | Fixed-size circular buffer |
| [FlyweightVerticalScrollView](Xeon.Common.FlyweightScrollView.FlyweightVerticalScrollView.html) | Vertical virtual scroll view |
| [FlyweightHorizontalScrollView](Xeon.Common.FlyweightScrollView.FlyweightHorizontalScrollView.html) | Horizontal virtual scroll view |
