using System.Collections.Generic;
using System.Text;

namespace Xeon.UniTerminal.Parsing
{
    /// <summary>
    /// CLI入力をトークンに分割します
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// 入力文字列をトークンのリストに分割します
        /// </summary>
        /// <param name="input">トークン化する入力文字列</param>
        /// <returns>トークンのリスト</returns>
        /// <exception cref="ParseException">トークン化エラー時にスローされます</exception>
        public List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(input))
                return tokens;

            var context = new TokenizeContext(input);

            while (context.HasMore)
            {
                SkipSpaces(context);

                if (!context.HasMore)
                    break;

                tokens.Add(ReadNextToken(context));
            }

            return tokens;
        }


        private static void SkipSpaces(TokenizeContext context)
        {
            while (context.HasMore && context.Current == ' ')
            {
                context.Advance();
            }
        }

        private Token ReadNextToken(TokenizeContext context)
        {
            char c = context.Current;

            if (c == '\t')
                throw new ParseException($"Tab character is not allowed in input at position {context.Position}");

            if (IsOperatorChar(c))
                return ReadOperator(context);

            return ReadWord(context);
        }

        private static bool IsOperatorChar(char c)
        {
            return c == '|' || c == '<' || c == '>';
        }

        private Token ReadOperator(TokenizeContext context)
        {
            char c = context.Current;

            switch (c)
            {
                case '|':
                    context.Advance();
                    return CreateToken(TokenKind.Pipe, "|", context.Position - 1, 1);

                case '<':
                    context.Advance();
                    return CreateToken(TokenKind.RedirectIn, "<", context.Position - 1, 1);

                case '>':
                    return ReadRedirectOut(context);

                default:
                    throw new ParseException($"Unexpected operator character: {c}");
            }
        }

        private Token ReadRedirectOut(TokenizeContext context)
        {
            int start = context.Position;

            if (context.HasNext && context.PeekNext() == '>')
            {
                context.Advance(2);
                return CreateToken(TokenKind.RedirectAppend, ">>", start, 2);
            }

            context.Advance();
            return CreateToken(TokenKind.RedirectOut, ">", start, 1);
        }

        private static Token CreateToken(TokenKind kind, string value, int start, int length)
        {
            return new Token(kind, value, new SourceSpan(start, length));
        }



        private Token ReadWord(TokenizeContext context)
        {
            var builder = new StringBuilder();
            var segments = new List<WordTokenSegment>();
            var segmentBuilder = new StringBuilder();
            int start = context.Position;
            bool wasQuoted = false;
            var currentQuoteKind = QuoteKind.None;

            while (context.HasMore && !IsWordTerminator(context.Current))
            {
                ProcessWordCharacter(context, builder, segments, segmentBuilder, ref wasQuoted, ref currentQuoteKind);
            }

            FlushSegment(segments, segmentBuilder, currentQuoteKind);

            return CreateWordToken(builder.ToString(), start, context.Position, wasQuoted, segments);
        }

        private void ProcessWordCharacter(
            TokenizeContext context,
            StringBuilder builder,
            List<WordTokenSegment> segments,
            StringBuilder segmentBuilder,
            ref bool wasQuoted,
            ref QuoteKind currentQuoteKind)
        {
            char c = context.Current;

            if (c == '\t')
                throw new ParseException($"Tab character is not allowed in input at position {context.Position}");

            if (c == '\\')
            {
                ProcessEscape(context, builder, segmentBuilder);
                return;
            }

            if (c == '"')
            {
                FlushSegment(segments, segmentBuilder, currentQuoteKind);
                wasQuoted = true;
                currentQuoteKind = QuoteKind.Double;
                context.Advance();
                ReadDoubleQuotedContent(context, builder, segmentBuilder);
                FlushSegment(segments, segmentBuilder, currentQuoteKind);
                currentQuoteKind = QuoteKind.None;
                return;
            }

            if (c == '\'')
            {
                FlushSegment(segments, segmentBuilder, currentQuoteKind);
                wasQuoted = true;
                currentQuoteKind = QuoteKind.Single;
                context.Advance();
                ReadSingleQuotedContent(context, builder, segmentBuilder);
                FlushSegment(segments, segmentBuilder, currentQuoteKind);
                currentQuoteKind = QuoteKind.None;
                return;
            }

            builder.Append(c);
            segmentBuilder.Append(c);
            context.Advance();
        }

        /// <summary>
        /// セグメントビルダーの内容をセグメントリストに追加します
        /// </summary>
        /// <param name="segments">セグメントリスト</param>
        /// <param name="segmentBuilder">セグメントビルダー</param>
        /// <param name="quoteKind">クォート種別</param>
        private static void FlushSegment(List<WordTokenSegment> segments, StringBuilder segmentBuilder, QuoteKind quoteKind)
        {
            if (segmentBuilder.Length > 0)
            {
                segments.Add(new WordTokenSegment(segmentBuilder.ToString(), quoteKind));
                segmentBuilder.Clear();
            }
        }

        private static bool IsWordTerminator(char c)
        {
            return c == ' ' || c == '|' || c == '<' || c == '>';
        }

        private static Token CreateWordToken(string value, int start, int end, bool wasQuoted, List<WordTokenSegment> segments)
        {
            var span = new SourceSpan(start, end - start);

            if (value == "--" && !wasQuoted)
                return new Token(TokenKind.EndOfOptions, "--", span);

            return new Token(TokenKind.Word, value, span, wasQuoted, segments);
        }



        private static void ProcessEscape(TokenizeContext context, StringBuilder builder, StringBuilder segmentBuilder)
        {
            if (!context.HasNext)
                throw new ParseException($"Escape character at end of input at position {context.Position}");

            context.Advance();
            char escapedChar = context.Current;

            // \$ はエスケープされた $ として記録（展開しない）
            if (escapedChar == '$')
            {
                builder.Append(escapedChar);
                // セグメントには \$ をそのまま追加（展開時にリテラル$として扱う）
                segmentBuilder.Append('\\');
                segmentBuilder.Append(escapedChar);
            }
            else
            {
                builder.Append(escapedChar);
                segmentBuilder.Append(escapedChar);
            }

            context.Advance();
        }



        private void ReadDoubleQuotedContent(TokenizeContext context, StringBuilder builder, StringBuilder segmentBuilder)
        {
            int quoteStart = context.Position - 1;

            while (context.HasMore)
            {
                char c = context.Current;

                if (c == '"')
                {
                    context.Advance();
                    return;
                }

                if (c == '\\')
                {
                    ProcessDoubleQuoteEscape(context, builder, segmentBuilder);
                    continue;
                }

                builder.Append(c);
                segmentBuilder.Append(c);
                context.Advance();
            }

            throw new ParseException($"Unclosed double quote starting at position {quoteStart}");
        }

        private static void ProcessDoubleQuoteEscape(TokenizeContext context, StringBuilder builder, StringBuilder segmentBuilder)
        {
            if (context.HasNext)
            {
                char next = context.PeekNext();
                if (next == '"' || next == '\\')
                {
                    context.Advance(2);
                    builder.Append(next);
                    segmentBuilder.Append(next);
                    return;
                }

                // \$ はエスケープされた $ として記録
                if (next == '$')
                {
                    context.Advance(2);
                    builder.Append(next);
                    segmentBuilder.Append('\\');
                    segmentBuilder.Append(next);
                    return;
                }
            }

            builder.Append(context.Current);
            segmentBuilder.Append(context.Current);
            context.Advance();
        }

        private void ReadSingleQuotedContent(TokenizeContext context, StringBuilder builder, StringBuilder segmentBuilder)
        {
            int quoteStart = context.Position - 1;

            while (context.HasMore)
            {
                char c = context.Current;

                if (c == '\'')
                {
                    context.Advance();
                    return;
                }

                builder.Append(c);
                segmentBuilder.Append(c);
                context.Advance();
            }

            throw new ParseException($"Unclosed single quote starting at position {quoteStart}");
        }



        private class TokenizeContext
        {
            private readonly string input;
            private readonly int length;

            /// <summary>
            /// 現在の位置
            /// </summary>
            public int Position { get; private set; }

            /// <summary>
            /// まだ読み取り可能な文字が残っているか
            /// </summary>
            public bool HasMore => Position < length;

            /// <summary>
            /// 次の文字が存在するか
            /// </summary>
            public bool HasNext => Position + 1 < length;

            /// <summary>
            /// 現在の文字
            /// </summary>
            public char Current => input[Position];

            /// <summary>
            /// トークナイズ用コンテキストを初期化します
            /// </summary>
            /// <param name="input">入力文字列</param>
            public TokenizeContext(string input)
            {
                this.input = input;
                this.length = input.Length;
                Position = 0;
            }

            /// <summary>
            /// 次の文字を取得します
            /// </summary>
            public char PeekNext() => input[Position + 1];

            /// <summary>
            /// 指定数だけ位置を進めます
            /// </summary>
            /// <param name="count">進める文字数</param>
            public void Advance(int count = 1) => Position += count;
        }

    }
}
