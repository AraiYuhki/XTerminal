using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xeon.UniTerminal
{
    /// <summary>
    /// シェル変数を管理するストア
    /// </summary>
    public class VariableStore
    {
        private static readonly Regex ValidNamePattern = new Regex(
            @"^[A-Za-z_][A-Za-z0-9_]*$",
            RegexOptions.Compiled);

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        /// <summary>
        /// 変数の値を取得します
        /// 未定義の変数は空文字を返します
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>変数の値（未定義の場合は空文字）</returns>
        public string Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return variables.TryGetValue(name, out var value) ? value : string.Empty;
        }

        /// <summary>
        /// 変数を設定します
        /// </summary>
        /// <param name="name">変数名</param>
        /// <param name="value">変数の値</param>
        /// <exception cref="ArgumentException">変数名が不正な場合</exception>
        public void Set(string name, string value)
        {
            ValidateName(name);
            variables[name] = value ?? string.Empty;
        }

        /// <summary>
        /// 変数を削除します
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>削除された場合はtrue</returns>
        public bool Unset(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return variables.Remove(name);
        }

        /// <summary>
        /// すべての変数を名前の昇順で列挙します
        /// </summary>
        /// <returns>変数名と値のペアのリスト</returns>
        public IEnumerable<KeyValuePair<string, string>> Enumerate()
        {
            var sorted = new List<KeyValuePair<string, string>>(variables);
            sorted.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return sorted;
        }

        /// <summary>
        /// 変数が存在するかどうかを確認します
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>存在する場合はtrue</returns>
        public bool Contains(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return variables.ContainsKey(name);
        }

        /// <summary>
        /// すべての変数をクリアします
        /// </summary>
        public void Clear()
        {
            variables.Clear();
        }

        /// <summary>
        /// 変数名が有効かどうかを検証します
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>有効な場合はtrue</returns>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return ValidNamePattern.IsMatch(name);
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

            if (!ValidNamePattern.IsMatch(name))
            {
                throw new ArgumentException(
                    $"Invalid variable name: '{name}'. Variable names must match [A-Za-z_][A-Za-z0-9_]*",
                    nameof(name));
            }
        }
    }
}
