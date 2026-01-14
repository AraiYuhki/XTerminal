namespace Company.Terminal.Parsing
{
    public enum TokenKind
    {
        Text,
        Pipe,
        RedirectIn,
        RedirectOut,
        RedirectAppend,
        OptionTerminator
    }
}
