# XTerminal

<p align="center">
  <img src="Documentation~/Images/icon.png" alt="XTerminal Logo" width="256" height="256">
</p>

XTerminal is a Linux-like CLI execution framework for Unity.  
It parses and executes string-based commands and supports shell features such as pipelines and redirections.

## Features

- **Linux-like command syntax**: Supports pipes (`|`) and redirections (`>`, `>>`, `<`)
- **Rich built-in commands**: Provides file operations, text processing, and Unity-specific commands
- **Extensible**: Easily add custom commands
- **Asynchronous execution**: Async command execution using `async/await`
- **UniTask support**: High-performance asynchronous execution using UniTask (optional)
- **Tab completion**: Command and path completion
- **Shell variables**: Supports variable expansion such as `$NAME` and `${NAME}`
- **FlyweightScrollView**: Virtualized scroll view designed for large log output
- **Ctrl+C cancellation**: Interrupt long-running commands

## Requirements

- Unity 6000.0 or later
- (Optional) UniTask 2.0 or later

## Documentation

For detailed command references and API documentation, see:

- **Reference**  
  https://araiyuhki.github.io/XTerminal_Reference/index.html

### Enabling UniTask Support

If UniTask is installed in the project, XTerminal automatically enables UniTask support.

1. Install UniTask (OpenUPM recommended):

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

2. When XTerminal detects UniTask, the symbol `UNI_TERMINAL_UNI_TASK_SUPPORT` will be automatically defined.

## Basic Usage

### Initializing the Terminal

```csharp
using Xeon.XTerminal;

// Create a Terminal instance
var terminal = new Terminal(
    workingDirectory: Application.dataPath,
    homeDirectory: Application.dataPath,
    registerBuiltInCommands: true
);
```

### Executing a Command

```csharp
using Xeon.XTerminal;

// Output writers
var stdout = new StringBuilderTextWriter();
var stderr = new StringBuilderTextWriter();

// Execute command
var exitCode = await terminal.ExecuteAsync("echo Hello, World!", stdout, stderr);

// Get result
Debug.Log(stdout.ToString());  // "Hello, World!"
```

### Executing Commands with UniTask

```csharp
using Cysharp.Threading.Tasks;
using Xeon.XTerminal;

// UniTask-based execution
var exitCode = await terminal.ExecuteUniTaskAsync("echo Hello!", stdout, stderr);
```

### Using Pipelines

```csharp
await terminal.ExecuteAsync(
    "cat myfile.txt | grep --pattern=error | less",
    stdout,
    stderr
);
```

### Redirection

```csharp
// Output to file
await terminal.ExecuteAsync("echo Hello > output.txt", stdout, stderr);

// Append to file
await terminal.ExecuteAsync("echo World >> output.txt", stdout, stderr);

// Input from file
await terminal.ExecuteAsync("grep --pattern=pattern < input.txt", stdout, stderr);
```

### Using Variables

```csharp
// Set variable
await terminal.ExecuteAsync("set NAME=Player1", stdout, stderr);

// Use variable
await terminal.ExecuteAsync("echo $NAME", stdout, stderr);

// List variables
await terminal.ExecuteAsync("env", stdout, stderr);

// Remove variable
await terminal.ExecuteAsync("unset NAME", stdout, stderr);
```

## Built-in Commands

XTerminal includes many built-in commands.
For detailed options and usage examples, see the reference documentation.

### Command List

| Category         | Commands                                                                |
| ---------------- | ----------------------------------------------------------------------- |
| File operations  | `pwd`, `cd`, `ls`, `cat`, `find`, `less`, `diff`, `head`, `tail`        |
| Text processing  | `echo`, `grep`                                                          |
| Utilities        | `help`, `history`, `clear`, `set`, `unset`, `env`, `pbcopy`             |
| Unity operations | `hierarchy`, `go`, `transform`, `component`, `property`, `scene`, `log` |
| Asset management | `asset`, `assetdb`, `adr`, `res`                                        |

To check help for a command:

```bash
help ls
help hierarchy
```

## Example Unity Commands

### Transform Command

```bash
# Show transform information
transform /Player

# Set position
transform /Player -p 1,2,3

# Add to position
transform add /Player -p 1,0,0

# Subtract rotation
transform sub /Player -r 0,90,0

# Multiply scale
transform mul /Player -s 2,2,2
```

### Property Command

```bash
# List properties
property list /Player Rigidbody

# Get value
property get /Player Rigidbody mass

# Set value
property set /Player Rigidbody mass 10

# Arithmetic operations
property add /Player Rigidbody mass 5
property mul /Player Transform localScale 2,2,2
```

## Creating Custom Commands

### Basic Command

```csharp
using Xeon.XTerminal;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

[Command("greet", "Greet the user")]
public class GreetCommand : ICommand
{
    [Option("name", "n", Description = "Name to greet")]
    public string Name;

    public string CommandName => "greet";
    public string Description => "Greet the user";

    public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
    {
        var name = Name ?? "World";
        await context.Stdout.WriteLineAsync($"Hello, {name}!", ct);
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

### Registering a Command

```csharp
// Register manually
terminal.Registry.Register<GreetCommand>();

// Register automatically from assembly
terminal.Registry.RegisterFromAssembly(typeof(GreetCommand).Assembly);
```

### Example Usage

```bash
greet
greet -n Alice
greet --name=Bob
```

### UniTask-Compatible Command

If you want to use UniTask, implement `IUniTaskCommand`.

```csharp
using Cysharp.Threading.Tasks;
using Xeon.XTerminal;

[Command("delay", "Wait for specified time")]
public class DelayCommand : IUniTaskCommand
{
    [Option("ms", "m", Description = "Delay in milliseconds")]
    public int Milliseconds = 1000;

    public string CommandName => "delay";
    public string Description => "Wait for specified time";

    public async UniTask<ExitCode> ExecuteAsync(UniTaskCommandContext context, CancellationToken ct)
    {
        await context.Stdout.WriteLineAsync($"Waiting {Milliseconds}ms...", ct);
        await UniTask.Delay(Milliseconds, cancellationToken: ct);
        await context.Stdout.WriteLineAsync("Done!", ct);

        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

## Exit Codes

| Code                        | Description   |
| --------------------------- | ------------- |
| `ExitCode.Success` (0)      | Success       |
| `ExitCode.UsageError` (1)   | Invalid usage |
| `ExitCode.RuntimeError` (2) | Runtime error |

## Author

Xeon
