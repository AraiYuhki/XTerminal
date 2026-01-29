using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.UniTerminal.BuiltInCommands
{
    /// <summary>
    /// 変数一覧を表示します
    /// </summary>
    [Command("env", "List all variables")]
    [CommandDocumentation(
        Category = "utilities",
        Synopsis = "env",
        LongDescription = "Displays all defined shell variables in NAME=VALUE format."
    )]
    public class EnvCommand : ICommand
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string CommandName => "env";

        /// <summary>
        /// コマンドの説明
        /// </summary>
        public string Description => "List all variables";

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

            foreach (var kv in context.Variables.Enumerate())
            {
                await context.Stdout.WriteLineAsync($"{kv.Key}={kv.Value}", ct);
            }

            return ExitCode.Success;
        }

        /// <summary>
        /// 補完候補を取得します
        /// </summary>
        /// <param name="context">補完コンテキスト</param>
        /// <returns>補完候補</returns>
        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            return Enumerable.Empty<string>();
        }
    }
}
