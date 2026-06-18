using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Company.Terminal.Runtime
{
    public sealed class FileTextWriter : TextWriterBase, System.IDisposable
    {
        private readonly StreamWriter _writer;

        public FileTextWriter(string path, bool append)
        {
            _writer = new StreamWriter(path, append, new UTF8Encoding(false));
        }

        public override async Task WriteAsync(string text)
        {
            await _writer.WriteAsync(ConvertNewlines(text)).ConfigureAwait(false);
        }

        public override async Task WriteLineAsync(string line)
        {
            await _writer.WriteAsync(ConvertNewlines(line + "\n")).ConfigureAwait(false);
        }

        public Task FlushAsync()
        {
            return _writer.FlushAsync();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
