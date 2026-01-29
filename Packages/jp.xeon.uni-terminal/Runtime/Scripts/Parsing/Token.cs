using System.Collections.Generic;

namespace Xeon.UniTerminal.Parsing
{
    /// <summary>
    /// CLIトークナイザーのトークン種別
    /// </summary>
    public enum TokenKind
    {
        /// <summary>
        /// ワードトークン（コマンド、引数、オプション等）
        /// </summary>
        Word,

        /// <summary>
        /// パイプ演算子: |
        /// </summary>
        Pipe,

        /// <summary>
        /// 標準入力リダイレクト: <
        /// </summary>
        RedirectIn,

        /// <summary>
        /// 標準出力上書き: >
        /// </summary>
        RedirectOut,

        /// <summary>
        /// 標準出力追記: >>
        /// </summary>
        RedirectAppend,

        /// <summary>
        /// オプション終端マーカー: --
        /// </summary>
        EndOfOptions
    }

    /// <summary>
    /// クォート種別を表します
    /// </summary>
    public enum QuoteKind
    {
        /// <summary>
        /// クォートなし（変数展開あり）
        /// </summary>
        None,

        /// <summary>
        /// シングルクォート（変数展開なし）
        /// </summary>
        Single,

        /// <summary>
        /// ダブルクォート（変数展開あり）
        /// </summary>
        Double
    }

    /// <summary>
    /// ワードトークン内のセグメントを表します
    /// </summary>
    public readonly struct WordTokenSegment
    {
        /// <summary>
        /// セグメントの値
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// クォート種別
        /// </summary>
        public QuoteKind QuoteKind { get; }

        /// <summary>
        /// 変数展開が可能かどうか
        /// </summary>
        public bool CanExpand => QuoteKind != QuoteKind.Single;

        /// <summary>
        /// セグメントを初期化します
        /// </summary>
        /// <param name="value">セグメントの値</param>
        /// <param name="quoteKind">クォート種別</param>
        public WordTokenSegment(string value, QuoteKind quoteKind)
        {
            Value = value;
            QuoteKind = quoteKind;
        }

        /// <summary>
        /// 表示用文字列を生成します
        /// </summary>
        public override string ToString() => $"[{QuoteKind}] \"{Value}\"";
    }

    /// <summary>
    /// ソース入力内の位置範囲を表します
    /// </summary>
    public readonly struct SourceSpan
    {
        /// <summary>
        /// 開始位置
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// 長さ
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 終端位置
        /// </summary>
        public int End => Start + Length;

        /// <summary>
        /// ソース範囲を初期化します
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="length">長さ</param>
        public SourceSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// 表示用文字列を生成します
        /// </summary>
        public override string ToString() => $"[{Start}..{End})";
    }

    /// <summary>
    /// 入力からのトークンを表します
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// トークンの種別
        /// </summary>
        public TokenKind Kind { get; }

        /// <summary>
        /// トークンの値（Wordトークンの場合は処理済みテキスト）
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// 元の入力内でのトークンのソース範囲
        /// </summary>
        public SourceSpan Span { get; }

        /// <summary>
        /// このトークンがクォートされていたかどうか
        /// </summary>
        public bool WasQuoted { get; }

        /// <summary>
        /// ワードトークンのセグメント情報（変数展開用）
        /// </summary>
        public IReadOnlyList<WordTokenSegment> Segments { get; }

        /// <summary>
        /// トークンを初期化します
        /// </summary>
        /// <param name="kind">トークン種別</param>
        /// <param name="value">トークン値</param>
        /// <param name="span">元の入力内での範囲</param>
        /// <param name="wasQuoted">クォートされていたかどうか</param>
        /// <param name="segments">セグメント情報</param>
        public Token(TokenKind kind, string value, SourceSpan span, bool wasQuoted = false, IReadOnlyList<WordTokenSegment> segments = null)
        {
            Kind = kind;
            Value = value;
            Span = span;
            WasQuoted = wasQuoted;
            Segments = segments;
        }

        /// <summary>
        /// 表示用文字列を生成します
        /// </summary>
        public override string ToString() => $"{Kind}: \"{Value}\" {Span}";
    }
}
