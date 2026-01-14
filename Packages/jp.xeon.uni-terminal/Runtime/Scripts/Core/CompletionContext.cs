using System;

namespace Company.Terminal.Runtime
{
    public sealed class CompletionContext
    {
        public CompletionContext(string input, string workingDirectory, string homeDirectory)
        {
            Input = input ?? string.Empty;
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            HomeDirectory = homeDirectory ?? throw new ArgumentNullException(nameof(homeDirectory));
        }

        public string Input { get; }
        public string WorkingDirectory { get; }
        public string HomeDirectory { get; }
    }
}
