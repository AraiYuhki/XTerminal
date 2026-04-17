using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ネットワークをスキャンして接続可能なノード一覧を表示するコマンド
    /// </summary>
    [Command("scan", "ネットワークをスキャンして利用可能なノードを検索する")]
    public class ScanCommand : ICommand
    {
        [Option("network", "n", Description = "対象ネットワーク（例: 10.0.0.0/24）")]
        public string Network;

        public string CommandName => "scan";
        public string Description => "ネットワークをスキャンして利用可能なノードを検索する";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var state = GameState.Instance;
            if (state == null)
            {
                await context.Stderr.WriteLineAsync("ERROR: Game not initialized.", ct);
                return ExitCode.RuntimeError;
            }

            var target = Network ?? "10.0.0.0/24";
            await context.Stdout.WriteLineAsync($"[*] Scanning {target} ...", ct);
            await Task.Delay(800, ct);
            await context.Stdout.WriteLineAsync("", ct);
            await context.Stdout.WriteLineAsync("IP            TYPE       STATUS     CRACKED", ct);
            await context.Stdout.WriteLineAsync("--------------------------------------------", ct);

            foreach (var node in state.NetworkNodes)
            {
                var cracked = node.IsCracked ? "YES" : "NO ";
                var status  = "ONLINE ";
                await context.Stdout.WriteLineAsync(
                    $"{node.IpAddress,-14}{node.NodeType,-11}{status,-11}{cracked}", ct);
            }

            await context.Stdout.WriteLineAsync("", ct);
            await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.Scan, ct);
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
            => Enumerable.Empty<string>();
    }
}
