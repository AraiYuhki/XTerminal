using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.XTerminal.Samples
{
    /// <summary>
    /// Basic example demonstrating how to use XTerminal
    /// </summary>
    public class TerminalExample : MonoBehaviour
    {
        private Terminal terminal;

        private class LogWriter : IAsyncTextWriter
        {
            private bool isError;
            public LogWriter(bool isError)
            {
                this.isError = isError;
            }

            public void Clear()
            {
                Debug.ClearDeveloperConsole();
            }

            public async Task WriteAsync(string text, CancellationToken ct = default)
            {
                WriteInternal($"[XTerminal] {text}");
            }

            public async Task WriteLineAsync(string line, CancellationToken ct = default)
            {
                if (string.IsNullOrEmpty(line))
                {
                    WriteInternal("[XTerminal]");
                    return;
                }
                foreach (var text in line.Split('\n'))
                    WriteInternal(text);
            }

            private void WriteInternal(string text)
            {
                if (isError)
                    Debug.LogError($"[XTerminal] {text}");
                else
                    Debug.Log($"[XTerminal] {text}");
            }
        }

        private LogWriter stdout;
        private LogWriter stderr;

        [SerializeField]
        private string initialCommand = "echo Hello, XTerminal!";

        private async void Start()
        {
            // Initialize Terminal with default settings
            terminal = new Terminal(
                workingDirectory: Application.dataPath,
                homeDirectory: Application.dataPath,
                registerBuiltInCommands: true
            );

            stdout = new LogWriter(true);
            stderr = new LogWriter(false);

            // Execute initial command
            ExecuteCommand(initialCommand);
        }

        /// <summary>
        /// Execute a command and log the results
        /// </summary>
        public async void ExecuteCommand(string command)
        {
            if (terminal == null || string.IsNullOrEmpty(command))
                return;

            // Clear previous output
            stdout.Clear();
            stderr.Clear();

            // Execute command
            var exitCode = await terminal.ExecuteAsync(command, stdout, stderr);

            // Log results
            var output = stdout.ToString();
            var error = stderr.ToString();

            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log($"[XTerminal] Output:\n{output}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[XTerminal] Error:\n{error}");
            }

            Debug.Log($"[XTerminal] Exit Code: {exitCode}");
        }

        /// <summary>
        /// Example: List files in current directory
        /// </summary>
        [ContextMenu("Run: ls -la")]
        public void RunListCommand()
        {
            ExecuteCommand("ls -la");
        }

        /// <summary>
        /// Example: Show scene hierarchy
        /// </summary>
        [ContextMenu("Run: hierarchy -r")]
        public void RunHierarchyCommand()
        {
            ExecuteCommand("hierarchy -r");
        }

        /// <summary>
        /// Example: Find GameObjects with specific tag
        /// </summary>
        [ContextMenu("Run: go find -t MainCamera")]
        public void RunFindCommand()
        {
            ExecuteCommand("go find -t MainCamera");
        }

        /// <summary>
        /// Example: Pipeline command
        /// </summary>
        [ContextMenu("Run: hierarchy -r | grep Camera")]
        public void RunPipelineCommand()
        {
            ExecuteCommand("hierarchy -r | grep --pattern=Camera");
        }
    }
}
