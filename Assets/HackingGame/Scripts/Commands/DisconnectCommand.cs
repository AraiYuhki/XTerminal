using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 現在接続中のノードから切断するコマンド
    /// </summary>
    [Command("disconnect", "現在のノードから切断する")]
    public class DisconnectCommand : ICommand
    {
        public string CommandName => "disconnect";
        public string Description => "現在のノードから切断する";

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
                await context.Stderr.WriteLineAsync("Not connected to any node.", ct);
                return ExitCode.UsageError;
            }

            var ip = state.ConnectedNode.IpAddress;
            state.Disconnect();
            await context.Stdout.WriteLineAsync($"[-] Disconnected from {ip}.", ct);
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
            => Enumerable.Empty<string>();
    }
}
