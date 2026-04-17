using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.Samples
{
    /// <summary>
    /// XTerminal用カスタムコマンドの作成例
    /// </summary>
    [Command("greet", "サンプル挨拶コマンド")]
    public class GreetCommand : ICommand
    {
        [Option("name", "n", Description = "挨拶する名前")]
        public string Name;

        [Option("times", "t", Description = "挨拶を繰り返す回数")]
        public int Times = 1;

        [Option("uppercase", "u", Description = "大文字で出力する")]
        public bool Uppercase;

        public string CommandName => "greet";
        public string Description => "カスタムコマンド作成を示すサンプル挨拶コマンド";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var name = string.IsNullOrEmpty(Name) ? "World" : Name;

            for (int i = 0; i < Times; i++)
            {
                var message = $"Hello, {name}!";
                if (Uppercase)
                {
                    message = message.ToUpperInvariant();
                }
                await context.Stdout.WriteLineAsync(message, ct);
            }

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            // --nameオプションの補完候補を提供する
            if (context.CurrentToken == "name" || context.CurrentToken == "n")
            {
                yield return "Alice";
                yield return "Bob";
                yield return "Charlie";
            }
        }
    }

    /// <summary>
    /// 標準入力から読み込むコマンドの例（パイプライン使用例）
    /// </summary>
    [Command("count", "入力から行数を数える")]
    public class CountCommand : ICommand
    {
        [Option("words", "w", Description = "行数の代わりに単語数を数える")]
        public bool CountWords;

        [Option("chars", "c", Description = "行数の代わりに文字数を数える")]
        public bool CountChars;

        public string CommandName => "count";
        public string Description => "入力から行数・単語数・文字数を数える";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var count = 0;
            await foreach (var line in context.Stdin.ReadLinesAsync(ct))
            {
                if (CountChars)
                {
                    count += line.Count();
                }
                else if (CountWords)
                {
                    var words = line.Split(new[] { ' ', '\t', '\n', '\r' },
                    System.StringSplitOptions.RemoveEmptyEntries);
                    count += words.Length;
                }
                else
                {
                    var lines = line.Split('\n');
                    count += lines.Length;
                }
            }
            await context.Stdout.WriteLineAsync(count.ToString(), ct);

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            yield break;
        }
    }
}
