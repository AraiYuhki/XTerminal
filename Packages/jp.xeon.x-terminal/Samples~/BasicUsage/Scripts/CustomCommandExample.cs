using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.Samples
{
    /// <summary>
    /// XTerminal用のカスタムコマンドを作成する例
    /// </summary>
    [Command("greet", "サンプルの挨拶コマンド")]
    public class GreetCommand : ICommand
    {
        [Option("name", "n", Description = "挨拶する名前")]
        public string Name;

        [Option("times", "t", Description = "挨拶する回数")]
        public int Times = 1;

        [Option("uppercase", "u", Description = "大文字で出力する")]
        public bool Uppercase;

        public string CommandName => "greet";
        public string Description => "カスタムコマンドの作成方法を示すサンプルの挨拶コマンド";

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
            // --name オプションに対して補完候補を提供する
            if (context.CurrentToken == "name" || context.CurrentToken == "n")
            {
                yield return "Alice";
                yield return "Bob";
                yield return "Charlie";
            }
        }
    }

    /// <summary>
    /// 標準入力（Stdin）から読み取るコマンドの例（パイプライン用）
    /// </summary>
    [Command("count", "入力からの行数をカウントする")]
    public class CountCommand : ICommand
    {
        [Option("words", "w", Description = "行数の代わりに単語数をカウントする")]
        public bool CountWords;

        [Option("chars", "c", Description = "行数の代わりに文字数をカウントする")]
        public bool CountChars;

        public string CommandName => "count";
        public string Description => "入力から行数、単語数、または文字数をカウントする";

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
