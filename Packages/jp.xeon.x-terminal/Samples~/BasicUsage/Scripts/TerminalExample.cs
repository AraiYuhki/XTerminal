using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.XTerminal.Samples
{
    /// <summary>
    /// XTerminalの基本的な使用例
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
            // デフォルト設定でTerminalを初期化する
            terminal = new Terminal(
                workingDirectory: Application.dataPath,
                homeDirectory: Application.dataPath,
                registerBuiltInCommands: true
            );

            stdout = new LogWriter(true);
            stderr = new LogWriter(false);

            // 初期コマンドを実行する
            ExecuteCommand(initialCommand);
        }

        /// <summary>
        /// コマンドを実行し、結果をログに出力する
        /// </summary>
        public async void ExecuteCommand(string command)
        {
            if (terminal == null || string.IsNullOrEmpty(command))
                return;

            // 前回の出力をクリアする
            stdout.Clear();
            stderr.Clear();

            // コマンドを実行する
            var exitCode = await terminal.ExecuteAsync(command, stdout, stderr);

            // 結果をログに出力する
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
        /// 使用例：現在のディレクトリのファイル一覧を表示する
        /// </summary>
        [ContextMenu("Run: ls -la")]
        public void RunListCommand()
        {
            ExecuteCommand("ls -la");
        }

        /// <summary>
        /// 使用例：シーン階層を表示する
        /// </summary>
        [ContextMenu("Run: hierarchy -r")]
        public void RunHierarchyCommand()
        {
            ExecuteCommand("hierarchy -r");
        }

        /// <summary>
        /// 使用例：特定のタグを持つGameObjectを検索する
        /// </summary>
        [ContextMenu("Run: go find -t MainCamera")]
        public void RunFindCommand()
        {
            ExecuteCommand("go find -t MainCamera");
        }

        /// <summary>
        /// 使用例：パイプラインコマンド
        /// </summary>
        [ContextMenu("Run: hierarchy -r | grep Camera")]
        public void RunPipelineCommand()
        {
            ExecuteCommand("hierarchy -r | grep --pattern=Camera");
        }
    }
}
