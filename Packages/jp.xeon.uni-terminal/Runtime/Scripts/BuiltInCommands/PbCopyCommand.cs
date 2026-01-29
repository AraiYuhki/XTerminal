using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.UniTerminal.BuiltInCommands
{
    /// <summary>
    /// 引数をクリップボードにコピーします
    /// </summary>
    [Command("pbcopy", "Copy arguments to clip board")]
    public class PbCopyCommand : ICommand
    {
        public string CommandName => "pbcopy";
        public string Description => "Copy arguments to clip board";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            var text = string.Empty;
            if (context.PositionalArguments.Count > 0)
            {
                text = string.Join('\n', context.PositionalArguments);
            }
            if (context.PositionalArguments.Count == 0)
            {
                var lines = new List<string>();
                await foreach (var line in context.Stdin.ReadLinesAsync(ct))
                    lines.Add(line);
                text = string.Join('\n', lines);
            }

            GUIUtility.systemCopyBuffer = text;
            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            yield break;
        }
    }
}
