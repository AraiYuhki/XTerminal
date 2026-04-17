using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// 暗号化されたファイルをマスターキーで復号して抽出するコマンド
    /// これがゲームのクリア条件
    /// </summary>
    [Command("extract", "マスターキーを使ってファイルを復号・抽出する")]
    public class ExtractCommand : ICommand
    {
        [Option("key", "k", Description = "復号用マスターキー")]
        public string Key;

        public string CommandName => "extract";
        public string Description => "マスターキーを使ってファイルを復号・抽出する";

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
                await context.Stderr.WriteLineAsync("extract: not connected to any node.", ct);
                return ExitCode.UsageError;
            }

            if (Key == null || context.PositionalArguments.Count == 0)
            {
                await context.Stderr.WriteLineAsync("Usage: extract --key <key> <filename>", ct);
                return ExitCode.UsageError;
            }

            var fileName = context.PositionalArguments[0];
            var ip       = state.ConnectedNode.IpAddress;

            if (!state.FileSystem.FileExists(ip, fileName))
            {
                await context.Stderr.WriteLineAsync($"extract: {fileName}: No such file", ct);
                return ExitCode.RuntimeError;
            }

            if (fileName != "vault.enc")
            {
                await context.Stderr.WriteLineAsync($"extract: {fileName}: Not an encrypted vault", ct);
                return ExitCode.UsageError;
            }

            await context.Stdout.WriteLineAsync($"[*] Verifying key...", ct);
            await Task.Delay(700, ct);

            if (Key != StageLoader.MasterKey)
            {
                await context.Stdout.WriteLineAsync("[!] Decryption failed: invalid key.", ct);
                await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.CrackFailed, ct);
                return ExitCode.RuntimeError;
            }

            await context.Stdout.WriteLineAsync("[*] Decrypting vault.enc ...", ct);
            await Task.Delay(1000, ct);
            await context.Stdout.WriteLineAsync("[+] Decryption successful.", ct);
            await context.Stdout.WriteLineAsync("", ct);
            await context.Stdout.WriteLineAsync("====================================", ct);
            await context.Stdout.WriteLineAsync("  PROJECT ARIA - INTERNAL MANIFEST  ", ct);
            await context.Stdout.WriteLineAsync("====================================", ct);
            await context.Stdout.WriteLineAsync("ARIA is not an AI assistant.", ct);
            await context.Stdout.WriteLineAsync("ARIA is a containment protocol.", ct);
            await context.Stdout.WriteLineAsync("Do not let it know you found this.", ct);
            await context.Stdout.WriteLineAsync("====================================", ct);
            await context.Stdout.WriteLineAsync("", ct);

            state.CompleteExtraction();
            await AriaDialogue.RespondAsync(context.Stdout, PlayerAction.Extract, ct);
            await context.Stdout.WriteLineAsync("", ct);
            await context.Stdout.WriteLineAsync($"[GAME CLEAR]  Score: {state.Score}", ct);
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var state = GameState.Instance;
            if (state == null || !state.IsConnected)
                return Enumerable.Empty<string>();

            return state.FileSystem.ListFiles(state.ConnectedNode.IpAddress)
                .Where(f => f.EndsWith(".enc"));
        }
    }
}
