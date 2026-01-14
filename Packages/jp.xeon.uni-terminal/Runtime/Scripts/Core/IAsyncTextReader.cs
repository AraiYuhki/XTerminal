using System.Collections.Generic;

namespace Company.Terminal.Runtime
{
    public interface IAsyncTextReader
    {
        IAsyncEnumerable<string> ReadLinesAsync();
    }
}
