using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.XTerminal.Samples
{
    /// <summary>
    /// XTerminalの使用方法を示す基本的な例
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
            // デフォルト設定でTerminalを初期化
            terminal = new Terminal(
                workingDirectory: Application.dataPath,
                homeDirectory: Application.dataPath,
                registerBuiltInCommands: true
            );

            stdout = new LogWriter(true);
            stderr = new LogWriter(false);

            // 初期コマンドを実行
            ExecuteCommand(initialCommand);
        }

        /// <summary>
        /// コマンドを実行して結果をログ出力する
        /// </summary>
        public async void ExecuteCommand(string command)
        {
            if (terminal == null || string.IsNullOrEmpty(command))
                return;

            // 前回の出力をクリア
            stdout.Clear();
            stderr.Clear();

            // コマンドを実行
            var exitCode = await terminal.ExecuteAsync(command, stdout, stderr);

            // 結果をログ出力
            var output = stdout.ToString();
            var error = stderr.ToString();

            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log($"[XTerminal] 出力:\n{output}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[XTerminal] エラー:\n{error}");
            }

            Debug.Log($"[XTerminal] 終了コード: {exitCode}");
        }

        /// <summary>
        /// 使用例：現在のディレクトリのファイルを一覧表示する
        /// </summary>
        [ContextMenu("実行: ls -la")]
        public void RunListCommand()
        {
            ExecuteCommand("ls -la");
        }

        /// <summary>
        /// 使用例：シーンヒエラルキーを表示する
        /// </summary>
        [ContextMenu("実行: hierarchy -r")]
        public void RunHierarchyCommand()
        {
            ExecuteCommand("hierarchy -r");
        }

        /// <summary>
        /// 使用例：特定のタグを持つGameObjectを検索する
        /// </summary>
        [ContextMenu("実行: go find -t MainCamera")]
        public void RunFindCommand()
        {
            ExecuteCommand("go find -t MainCamera");
        }

        /// <summary>
        /// 使用例：パイプラインコマンド
        /// </summary>
        [ContextMenu("実行: hierarchy -r | grep Camera")]
        public void RunPipelineCommand()
        {
            ExecuteCommand("hierarchy -r | grep --pattern=Camera");
        }
    }
}
