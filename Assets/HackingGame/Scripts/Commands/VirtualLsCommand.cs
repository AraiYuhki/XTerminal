using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 現在接続中のノード上の仮想ファイル一覧を表示するコマンド
    /// 実ファイルシステムではなくVirtualFileSystemを参照する
    /// </summary>
    [Command("ls", "List files on the connected node")]
    public class VirtualLsCommand : ICommand
    {
        public string CommandName => "ls";
        public string Description => "List files on the connected node";

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
                await context.Stderr.WriteLineAsync("ls: not connected to any node. Use 'connect <ip>' first.", ct);
                return ExitCode.UsageError;
            }

            var files = state.FileSystem.ListFiles(state.ConnectedNode.IpAddress).ToList();
            if (files.Count == 0)
            {
                await context.Stdout.WriteLineAsync("(empty)", ct);
                return ExitCode.Success;
            }

            foreach (var file in files)
                await context.Stdout.WriteLineAsync(file, ct);

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
            => Enumerable.Empty<string>();
    }
}
