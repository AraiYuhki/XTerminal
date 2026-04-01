using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 指定したIPアドレスのノードに接続するコマンド
    /// クラックされていないノードには接続できない
    /// </summary>
    [Command("connect", "Connect to a network node")]
    public class ConnectCommand : ICommand
    {
        public string CommandName => "connect";
        public string Description => "Connect to a network node";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var state = GameState.Instance;
            if (state == null)
            {
                await context.Stderr.WriteLineAsync("ERROR: Game not initialized.", ct);
                return ExitCode.RuntimeError;
            }

            if (context.PositionalArguments.Count == 0)
            {
                await context.Stderr.WriteLineAsync("Usage: connect <ip>", ct);
                return ExitCode.UsageError;
            }

            var ip   = context.PositionalArguments[0];
            var node = state.FindNode(ip);

            if (node == null)
            {
                await context.Stderr.WriteLineAsync($"connect: no route to host: {ip}", ct);
                return ExitCode.RuntimeError;
            }

            if (!node.IsCracked)
            {
                await context.Stdout.WriteLineAsync($"[!] Connection refused: authentication required.", ct);
                await context.Stdout.WriteLineAsync($"    Use 'crack' after connecting to bypass auth.", ct);
                await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.ConnectFailed, ct);
                return ExitCode.RuntimeError;
            }

            state.Connect(node);
            await context.Stdout.WriteLineAsync($"[+] Connected to {node.IpAddress} ({node.NodeType})", ct);
            await context.Stdout.WriteLineAsync($"    {node.Description}", ct);
            await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.Connect, ct);
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var state = GameState.Instance;
            if (state == null)
                return Enumerable.Empty<string>();

            return state.NetworkNodes.Select(n => n.IpAddress);
        }
    }
}
