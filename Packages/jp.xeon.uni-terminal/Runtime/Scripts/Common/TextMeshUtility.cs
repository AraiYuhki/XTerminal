using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Xeon.UniTerminal.Common
{
    public static class TextMeshUtility
    {
        public static int GetMaxCharacterCountInOneLine(this TMP_Text self, string sampleCharacters)
        {
            float maxWidth = self.rectTransform.rect.width;

            int low = 0;
            int high = sampleCharacters.Length;
            int result = 0;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                string test = sampleCharacters.Substring(0, mid);

                Vector2 size = self.GetPreferredValues(test);

                if (size.x <= maxWidth)
                {
                    result = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// テキストを指定した最大文字数でワードラップして複数行に分割する
        /// 英単語の途中では改行しないように試みる
        /// </summary>
        /// <param name="text">分割するテキスト</param>
        /// <param name="maxCharsPerLine">1行あたりの最大文字数</param>
        /// <returns>分割された行のリスト</returns>
        public static List<string> WrapText(string text, int maxCharsPerLine)
        {
            var estimatedLineCount = string.IsNullOrEmpty(text) || maxCharsPerLine <= 0
                ? 1
                : Mathf.CeilToInt(text.Length / (float)maxCharsPerLine) + 1;
            var lines = new List<string>(estimatedLineCount);

            if (string.IsNullOrEmpty(text) || maxCharsPerLine <= 0)
            {
                lines.Add(text ?? string.Empty);
                return lines;
            }

            if (text.Length <= maxCharsPerLine)
            {
                lines.Add(text);
                return lines;
            }

            var startIndex = 0;
            var length = text.Length;
            while (startIndex < length)
            {
                var endExclusive = Mathf.Min(startIndex + maxCharsPerLine, length);
                if (endExclusive >= length)
                {
                    lines.Add(text.Substring(startIndex, length - startIndex));
                    break;
                }

                var breakIndex = FindLastSpaceIndex(text, startIndex, endExclusive);
                if (breakIndex > startIndex)
                {
                    lines.Add(text.Substring(startIndex, breakIndex - startIndex));
                    startIndex = SkipLeadingSpaces(text, breakIndex + 1, length);
                    continue;
                }

                lines.Add(text.Substring(startIndex, endExclusive - startIndex));
                startIndex = endExclusive;
            }

            return lines;
        }

        /// <summary>
        /// ワードラップのための分割位置を探す
        /// </summary>
        private static int FindLastSpaceIndex(string text, int startIndex, int endExclusive)
        {
            var lastSpace = -1;
            for (var i = startIndex; i < endExclusive && i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    lastSpace = i;
                }
            }

            return lastSpace;
        }

        private static int SkipLeadingSpaces(string text, int startIndex, int length)
        {
            var index = startIndex;
            while (index < length && text[index] == ' ')
                index++;
            return index;
        }
    }
}
