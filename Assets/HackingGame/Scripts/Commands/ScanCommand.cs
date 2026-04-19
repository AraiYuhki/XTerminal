using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    using UniTask = Cysharp.Threading.Tasks.UniTask;
    /// <summary>
    /// ネットワークをスキャンして接続可能なノード一覧を表示するコマンド
    /// </summary>
    [Command("scan", "Scan the network for available nodes")]
    public class ScanCommand : ICommand
    {
        [Option("network", "n", Description = "Target network (e.g. 10.0.0.0/24)")]
        public string Network;

        public string CommandName => "scan";
        public string Description => "Scan the network for available nodes";

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
            await UniTask.Delay(800, cancellationToken: ct);
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
