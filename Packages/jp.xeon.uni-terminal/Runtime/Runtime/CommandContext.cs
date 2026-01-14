using System;

namespace Company.Terminal.Runtime
{
    public sealed class CommandContext
    {
        public CommandContext(
            IAsyncTextReader stdin,
            IAsyncTextWriter stdout,
            IAsyncTextWriter stderr,
            string workingDirectory,
            string homeDirectory)
        {
            Stdin = stdin ?? throw new ArgumentNullException(nameof(stdin));
            Stdout = stdout ?? throw new ArgumentNullException(nameof(stdout));
            Stderr = stderr ?? throw new ArgumentNullException(nameof(stderr));
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            HomeDirectory = homeDirectory ?? throw new ArgumentNullException(nameof(homeDirectory));
        }

        public IAsyncTextReader Stdin { get; }
        public IAsyncTextWriter Stdout { get; }
        public IAsyncTextWriter Stderr { get; }
        public string WorkingDirectory { get; set; }
        public string HomeDirectory { get; }
    }
}
