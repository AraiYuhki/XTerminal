namespace Company.Terminal.Parsing
{
    public readonly struct SourceSpan
    {
        public SourceSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
    }
}
