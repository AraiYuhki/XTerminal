using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 現在接続中のノード上の仮想ファイル内容を表示するコマンド
    /// センシティブファイルを読んだ場合はARIAが反応する
    /// </summary>
    [Command("cat", "接続中のノードのファイル内容を表示する")]
    public class VirtualCatCommand : ICommand
    {
        public string CommandName => "cat";
        public string Description => "接続中のノードのファイル内容を表示する";

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
                await context.Stderr.WriteLineAsync("cat: not connected to any node.", ct);
                return ExitCode.UsageError;
            }

            if (context.PositionalArguments.Count == 0)
            {
                await context.Stderr.WriteLineAsync("Usage: cat <filename>", ct);
                return ExitCode.UsageError;
            }

            var fileName = context.PositionalArguments[0];
            var ip       = state.ConnectedNode.IpAddress;

            if (!state.FileSystem.TryReadFile(ip, fileName, out var content))
            {
                await context.Stderr.WriteLineAsync($"cat: {fileName}: No such file", ct);
                return ExitCode.RuntimeError;
            }

            await context.Stdout.WriteLineAsync(content, ct);

            if (AriaDialogue.IsSensitiveFile(fileName))
                await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.ReadSensitiveFile, ct);

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var state = GameState.Instance;
            if (state == null || !state.IsConnected)
                return Enumerable.Empty<string>();

            return state.FileSystem.ListFiles(state.ConnectedNode.IpAddress);
        }
    }
}
