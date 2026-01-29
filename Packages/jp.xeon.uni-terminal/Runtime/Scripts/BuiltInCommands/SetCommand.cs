using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.UniTerminal.BuiltInCommands
{
    /// <summary>
    /// 変数を設定します
    /// </summary>
    [Command("set", "Set a variable (NAME=VALUE)")]
    public class SetCommand : ICommand
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string CommandName => "set";

        /// <summary>
        /// コマンドの説明
        /// </summary>
        public string Description => "Set a variable (NAME=VALUE)";

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
                await context.Stderr.WriteLineAsync("Usage: set NAME=VALUE", ct);
                return ExitCode.UsageError;
            }

            var assignment = string.Join(" ", context.PositionalArguments);
            int eqIndex = assignment.IndexOf('=');

            if (eqIndex < 0)
            {
                await context.Stderr.WriteLineAsync("Usage: set NAME=VALUE (missing '=')", ct);
                return ExitCode.UsageError;
            }

            string name = assignment.Substring(0, eqIndex);
            string value = assignment.Substring(eqIndex + 1);

            if (string.IsNullOrEmpty(name))
            {
                await context.Stderr.WriteLineAsync("Variable name cannot be empty", ct);
                return ExitCode.UsageError;
            }

            if (!VariableStore.IsValidName(name))
            {
                await context.Stderr.WriteLineAsync(
                    $"Invalid variable name: '{name}'. Must match [A-Za-z_][A-Za-z0-9_]*", ct);
                return ExitCode.UsageError;
            }

            try
            {
                context.Variables.Set(name, value);
                return ExitCode.Success;
            }
            catch (ArgumentException ex)
            {
                await context.Stderr.WriteLineAsync(ex.Message, ct);
                return ExitCode.UsageError;
            }
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
