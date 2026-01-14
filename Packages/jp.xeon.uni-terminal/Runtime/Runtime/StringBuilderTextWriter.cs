using System.Text;
using System.Threading.Tasks;

namespace Company.Terminal.Runtime
{
    public sealed class StringBuilderTextWriter : TextWriterBase
    {
        private readonly StringBuilder _builder;

        public StringBuilderTextWriter(StringBuilder builder)
        {
            _builder = builder ?? new StringBuilder();
        }

        public StringBuilderTextWriter() : this(new StringBuilder())
        {
        }

        public override Task WriteAsync(string text)
        {
            _builder.Append(ConvertNewlines(text));
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string line)
        {
            _builder.Append(ConvertNewlines(line + "\n"));
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
