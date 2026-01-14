using System.Threading.Tasks;

namespace Company.Terminal.Runtime
{
    public interface IAsyncTextWriter
    {
        Task WriteAsync(string text);
        Task WriteLineAsync(string line);
    }
}
