namespace Company.Terminal.Parsing
{
    public readonly struct Token
    {
        public Token(TokenType type, string text, SourceSpan span)
        {
            Type = type;
            Text = text;
            Span = span;
        }

        public TokenType Type { get; }
        public string Text { get; }
        public SourceSpan Span { get; }
    }
}
