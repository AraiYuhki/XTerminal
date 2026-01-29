using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.UniTerminal.BuiltInCommands
{
    /// <summary>
    /// 変数を削除します
    /// </summary>
    [Command("unset", "Unset one or more variables")]
    [CommandDocumentation(
        Category = "utilities",
        Synopsis = "unset NAME [NAME ...]",
        LongDescription = "Removes one or more shell variables. Multiple variable names can be specified separated by spaces."
    )]
    public class UnsetCommand : ICommand
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string CommandName => "unset";

        /// <summary>
        /// コマンドの説明
        /// </summary>
        public string Description => "Unset one or more variables";

        /// <summary>
        /// コマンドを実行します
        /// </summary>
        /// <param name="context">コマンドコンテキスト</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>終了コード</returns>
        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            if (context.Variables == null)
            {
                await context.Stderr.WriteLineAsync("Variable store is not available", ct);
                return ExitCode.RuntimeError;
            }

            if (context.PositionalArguments.Count == 0)
            {
                await context.Stderr.WriteLineAsync("Usage: unset NAME [NAME ...]", ct);
                return ExitCode.UsageError;
            }

            foreach (var name in context.PositionalArguments)
            {
                context.Variables.Unset(name);
            }

            return ExitCode.Success;
        }

        /// <summary>
        /// 補完候補を取得します
        /// </summary>
        /// <param name="context">補完コンテキスト</param>
        /// <returns>補完候補（定義済みの変数名）</returns>
        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            if (context.Variables == null)
                return Enumerable.Empty<string>();

            return context.Variables.Enumerate().Select(kv => kv.Key);
        }
    }
}
