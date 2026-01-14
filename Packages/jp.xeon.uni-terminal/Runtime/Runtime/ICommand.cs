using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Company.Terminal.Runtime
{
    public interface ICommand
    {
        string CommandName { get; }
        string Description { get; }

        Task<int> ExecuteAsync(CommandContext context, CancellationToken ct);

        IEnumerable<string> GetCompletions(CompletionContext context);
    }
}
