using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 現在接続中のノードの仮想ファイルをパターン検索するコマンド
    /// vault.keyでmaster_keyを検索することがゲームの重要なステップになる
    /// </summary>
    [Command("grep", "Search for a pattern in a file on the connected node")]
    public class VirtualGrepCommand : ICommand
    {
        [Option("ignore-case", "i", Description = "Case insensitive matching")]
        public bool IgnoreCase;

        public string CommandName => "grep";
        public string Description => "Search for a pattern in a file on the connected node";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var state = GameState.Instance;
            if (state == null)
            {
                await context.Stderr.WriteLineAsync("ERROR: Game not initialized.", ct);
                return ExitCode.RuntimeError;
            }

            if (!state.IsConnected)
            {
                await context.Stderr.WriteLineAsync("grep: not connected to any node.", ct);
                return ExitCode.UsageError;
            }

            if (context.PositionalArguments.Count < 2)
            {
                await context.Stderr.WriteLineAsync("Usage: grep <pattern> <filename>", ct);
                return ExitCode.UsageError;
            }

            var pattern  = context.PositionalArguments[0];
            var fileName = context.PositionalArguments[1];
            var ip       = state.ConnectedNode.IpAddress;

            if (!state.FileSystem.TryReadFile(ip, fileName, out var content))
            {
                await context.Stderr.WriteLineAsync($"grep: {fileName}: No such file", ct);
                return ExitCode.RuntimeError;
            }

            var comparison = IgnoreCase
                ? System.StringComparison.OrdinalIgnoreCase
                : System.StringComparison.Ordinal;

            var matched = content
                .Split('\n')
                .Where(line => line.Contains(pattern, comparison))
                .ToList();

            if (matched.Count == 0)
                return ExitCode.RuntimeError;

            foreach (var line in matched)
                await context.Stdout.WriteLineAsync(line, ct);

            if (AriaDialogue.IsSensitiveFile(fileName))
                await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.ReadSensitiveFile, ct);

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var state = GameState.Instance;
            if (state == null || !state.IsConnected)
                return Enumerable.Empty<string>();

            // 引数が1つ以上あればファイル名補完、なければ空
            if (context.CurrentTokenIndex >= 1)
                return state.FileSystem.ListFiles(state.ConnectedNode.IpAddress);

            return Enumerable.Empty<string>();
        }
    }
}
