namespace Company.Terminal.Parsing
{
    public readonly struct Token
    {
        public Token(TokenType kind, string text, SourceSpan span)
        {
            Kind = kind;
            Text = text;
            Span = span;
        }

        public TokenType Kind { get; }
        public string Text { get; }
        public SourceSpan Span { get; }
    }
}
