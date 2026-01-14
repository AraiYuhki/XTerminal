namespace Company.Terminal.Parsing
{
    public readonly struct Token
    {
        public Token(TokenKind kind, string text, SourceSpan span)
        {
            Kind = kind;
            Text = text;
            Span = span;
        }

        public TokenKind Kind { get; }
        public string Text { get; }
        public SourceSpan Span { get; }
    }
}
