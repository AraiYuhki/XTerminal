using System.Collections.Generic;
using System.Text;
using Xeon.UniTerminal;

namespace Xeon.UniTerminal.Parsing
{
    /// <summary>
    /// トークン内の変数を展開します
    /// </summary>
    public class VariableExpander
    {
        private readonly VariableStore variableStore;
        private readonly StringBuilder tokenBuilder = new StringBuilder();
        private readonly StringBuilder segmentBuilder = new StringBuilder();
        private readonly StringBuilder nameBuilder = new StringBuilder();

        /// <summary>
        /// 変数展開器を初期化します
        /// </summary>
        /// <param name="variableStore">変数ストア</param>
        public VariableExpander(VariableStore variableStore)
        {
            this.variableStore = variableStore ?? new VariableStore();
        }

        /// <summary>
        /// トークンリスト内の変数を展開します
        /// </summary>
        /// <param name="tokens">展開するトークンリスト</param>
        /// <returns>展開後のトークンリスト</returns>
        public List<Token> Expand(List<Token> tokens)
        {
            var result = new List<Token>(tokens.Count);

            foreach (var token in tokens)
            {
                result.Add(ExpandToken(token));
            }

            return result;
        }

        private Token ExpandToken(Token token)
        {
            if (token.Kind != TokenKind.Word || token.Segments == null || token.Segments.Count == 0)
                return token;

            tokenBuilder.Clear();

            foreach (var segment in token.Segments)
            {
                if (segment.CanExpand)
                    tokenBuilder.Append(ExpandSegment(segment.Value));
                else
                    tokenBuilder.Append(segment.Value);
            }

            return new Token(
                token.Kind,
                tokenBuilder.ToString(),
                token.Span,
                token.WasQuoted,
                token.Segments);
        }

        private string ExpandSegment(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.Contains("$"))
                return value;

            segmentBuilder.Clear();
            int i = 0;

            while (i < value.Length)
            {
                char c = value[i];

                // エスケープされた $ の処理
                if (c == '\\' && i + 1 < value.Length && value[i + 1] == '$')
                {
                    segmentBuilder.Append('$');
                    i += 2;
                    continue;
                }

                if (c == '$')
                {
                    i = ExpandVariable(value, i);
                    continue;
                }

                segmentBuilder.Append(c);
                i++;
            }

            return segmentBuilder.ToString();
        }

        private int ExpandVariable(string value, int startIndex)
        {
            int i = startIndex + 1;

            if (i >= value.Length)
            {
                segmentBuilder.Append('$');
                return i;
            }

            // ${NAME} 形式
            if (value[i] == '{')
                return ExpandBracedVariable(value, i + 1);

            // $NAME 形式
            return ExpandSimpleVariable(value, i);
        }

        private int ExpandBracedVariable(string value, int nameStart)
        {
            nameBuilder.Clear();
            int i = nameStart;

            while (i < value.Length && value[i] != '}')
            {
                nameBuilder.Append(value[i]);
                i++;
            }

            // 閉じ括弧がない場合はリテラルとして扱う
            if (i >= value.Length)
            {
                segmentBuilder.Append("${");
                segmentBuilder.Append(nameBuilder);
                return i;
            }

            string name = nameBuilder.ToString();
            if (VariableStore.IsValidName(name))
                segmentBuilder.Append(variableStore.Get(name));
            else
                segmentBuilder.Append("${").Append(name).Append('}');

            return i + 1;
        }

        private int ExpandSimpleVariable(string value, int nameStart)
        {
            nameBuilder.Clear();
            int i = nameStart;

            // 最初の文字は [A-Za-z_] でなければならない
            if (i < value.Length && IsValidFirstChar(value[i]))
            {
                nameBuilder.Append(value[i]);
                i++;

                // 残りの文字は [A-Za-z0-9_]
                while (i < value.Length && IsValidNameChar(value[i]))
                {
                    nameBuilder.Append(value[i]);
                    i++;
                }
            }

            if (nameBuilder.Length == 0)
            {
                segmentBuilder.Append('$');
                return nameStart;
            }

            string name = nameBuilder.ToString();
            segmentBuilder.Append(variableStore.Get(name));
            return i;
        }

        private static bool IsValidFirstChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
        }

        private static bool IsValidNameChar(char c)
        {
            return IsValidFirstChar(c) || (c >= '0' && c <= '9');
        }
    }
}
