using System;

namespace Company.Terminal.Runtime
{
    public abstract class TextWriterBase : IAsyncTextWriter
    {
        public abstract System.Threading.Tasks.Task WriteAsync(string text);
        public abstract System.Threading.Tasks.Task WriteLineAsync(string line);

        protected static string ConvertNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            var normalized = text.Replace("\r\n", "\n");
            return normalized.Replace("\n", "<br/>");
        }
    }
}
