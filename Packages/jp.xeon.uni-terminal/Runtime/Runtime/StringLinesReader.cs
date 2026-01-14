using System.Collections.Generic;

namespace Company.Terminal.Runtime
{
    public sealed class StringLinesReader : IAsyncTextReader
    {
        private readonly IEnumerable<string> _lines;

        public StringLinesReader(IEnumerable<string> lines)
        {
            _lines = lines ?? new string[0];
        }

        public async IAsyncEnumerable<string> ReadLinesAsync()
        {
            foreach (var line in _lines)
            {
                yield return line;
                await System.Threading.Tasks.Task.Yield();
            }
        }
    }
}
