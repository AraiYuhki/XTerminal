using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 現在のゲーム状態（接続状況・スコア・ノード状態）を表示するコマンド
    /// </summary>
    [Command("status", "現在のゲーム状態を表示する")]
    public class StatusCommand : ICommand
    {
        public string CommandName => "status";
        public string Description => "現在のゲーム状態を表示する";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var state = GameState.Instance;
            if (state == null)
            {
                await context.Stderr.WriteLineAsync("ERROR: Game not initialized.", ct);
                return ExitCode.RuntimeError;
            }

            await context.Stdout.WriteLineAsync("=== SYSTEM STATUS ===", ct);
            await context.Stdout.WriteLineAsync($"Phase  : {state.Phase}", ct);
            await context.Stdout.WriteLineAsync($"Score  : {state.Score}", ct);

            var connection = state.IsConnected
                ? $"{state.ConnectedNode.IpAddress} ({state.ConnectedNode.NodeType})"
                : "none";
            await context.Stdout.WriteLineAsync($"Node   : {connection}", ct);

            await context.Stdout.WriteLineAsync("", ct);
            await context.Stdout.WriteLineAsync("Cracked nodes:", ct);

            foreach (var node in state.NetworkNodes)
            {
                var mark = node.IsCracked ? "[+]" : "[ ]";
                await context.Stdout.WriteLineAsync($"  {mark} {node.IpAddress}  {node.NodeType}", ct);
            }

            await context.Stdout.WriteLineAsync("=====================", ct);
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
            => Enumerable.Empty<string>();
    }
}
