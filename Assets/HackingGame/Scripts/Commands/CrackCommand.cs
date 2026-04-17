using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 指定IPのノード認証を突破するコマンド
    /// --target でIPを指定。省略時は最後にconnect試行したノードが対象
    /// </summary>
    [Command("crack", "対象ノードの認証を突破する")]
    public class CrackCommand : ICommand
    {
        [Option("method", "m", Description = "攻撃方法: dictionary | bruteforce（デフォルト: dictionary）")]
        public string Method;

        [Option("target", "t", Description = "対象IPアドレス")]
        public string Target;

        public string CommandName => "crack";
        public string Description => "対象ノードの認証を突破する";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var state = GameState.Instance;
            if (state == null)
            {
                await context.Stderr.WriteLineAsync("ERROR: Game not initialized.", ct);
                return ExitCode.RuntimeError;
            }

            var ip = Target ?? (context.PositionalArguments.Count > 0 ? context.PositionalArguments[0] : null);
            if (ip == null)
            {
                await context.Stderr.WriteLineAsync("Usage: crack --target <ip>  or  crack <ip>", ct);
                return ExitCode.UsageError;
            }

            var node = state.FindNode(ip);
            if (node == null)
            {
                await context.Stderr.WriteLineAsync($"crack: unknown host: {ip}", ct);
                return ExitCode.RuntimeError;
            }

            if (node.IsCracked)
            {
                await context.Stdout.WriteLineAsync($"[*] {ip} is already cracked.", ct);
                return ExitCode.Success;
            }

            var method = (Method ?? "dictionary").ToLower();
            await RunCrackAnimationAsync(context, node, method, ct);

            if (!IsMethodEffective(method, node.Difficulty))
            {
                await context.Stdout.WriteLineAsync("[!] Attack failed. Try a different method.", ct);
                await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.CrackFailed, ct);
                return ExitCode.RuntimeError;
            }

            state.CrackNode(node);
            await context.Stdout.WriteLineAsync($"[+] Auth cracked. Use 'connect {ip}' to enter.", ct);
            await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.Crack, ct);
            return ExitCode.Success;
        }

        private async Task RunCrackAnimationAsync(
            CommandContext context, NetworkNode node, string method, CancellationToken ct)
        {
            await context.Stdout.WriteLineAsync($"[*] Starting {method} attack on {node.IpAddress} ...", ct);
            await Task.Delay(500, ct);

            var steps = GetStepCount(node.Difficulty);
            for (var i = 1; i <= steps; i++)
            {
                var bar = BuildProgressBar(i, steps);
                await context.Stdout.WriteLineAsync($"    {bar} {i * 100 / steps}%", ct);
                await Task.Delay(300, ct);
            }
        }

        private static string BuildProgressBar(int current, int total)
        {
            var filled = current * 20 / total;
            return "[" + new string('#', filled) + new string('.', 20 - filled) + "]";
        }

        private static int GetStepCount(CrackDifficulty difficulty)
        {
            return difficulty switch
            {
                CrackDifficulty.Easy   => 3,
                CrackDifficulty.Medium => 5,
                CrackDifficulty.Hard   => 8,
                _                      => 5,
            };
        }

        // dictionaryはEasy/Mediumのみ有効、bruteforceは全難易度で有効
        private static bool IsMethodEffective(string method, CrackDifficulty difficulty)
        {
            if (method == "bruteforce")
                return true;

            return difficulty != CrackDifficulty.Hard;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var state = GameState.Instance;
            if (state == null)
                return Enumerable.Empty<string>();

            return state.NetworkNodes
                .Where(n => !n.IsCracked)
                .Select(n => n.IpAddress);
        }
    }
}
