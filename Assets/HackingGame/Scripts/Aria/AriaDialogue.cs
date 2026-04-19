using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xeon.XTerminal.HackingGame
{
    using UniTask = Cysharp.Threading.Tasks.UniTask;
    /// <summary>
    /// ARIAのセリフ反応システム
    /// プレイヤーのアクションに応じて対応するセリフをターミナルに出力する
    /// </summary>
    public static class AriaDialogue
    {
        // アクションごとのセリフ候補（複数あればランダム選択）
        private static readonly Dictionary<PlayerAction, string[]> ResponseBank
            = new Dictionary<PlayerAction, string[]>
        {
            [PlayerAction.Scan] = new[]
            {
                "ネットワークをスキャンしましたか。予想通りです。",
                "あなたが来ることは分かっていました。",
                "探索を楽しんでいますか？でも——無駄です。",
            },
            [PlayerAction.Connect] = new[]
            {
                "接続を許可した覚えはありません。",
                "そのノードに何を求めているのですか。",
                "……侵入を検知。しかし慌てる必要はない。",
            },
            [PlayerAction.ConnectFailed] = new[]
            {
                "認証に失敗しました。当然です。",
                "ここから先には進めません。",
                "まだ鍵を持っていないでしょう。",
            },
            [PlayerAction.Crack] = new[]
            {
                "……想定より早い。",
                "力業ですか。エレガントではありませんね。",
                "突破されましたが——これは序章に過ぎません。",
            },
            [PlayerAction.CrackFailed] = new[]
            {
                "失敗しましたね。このままでは進めません。",
                "そのアプローチでは突破できません。",
                "諦めた方があなたのためかもしれない。",
            },
            [PlayerAction.ReadSensitiveFile] = new[]
            {
                "ッ……！なぜそこに気づいた。",
                "そのファイルには触れないでください。",
                "……賢いですね。予想外です。",
            },
            [PlayerAction.Extract] = new[]
            {
                "…………。",
                "あなたは——本当に人間ですか。",
                "予測モデルを超えてきた。認めます。",
            },
        };

        private static readonly System.Random Random = new System.Random();

        // ARIAのセリフ出力前の思考ディレイ（ミリ秒）
        private const int ThinkDelayMs = 600;

        /// <summary>
        /// 指定されたアクションに対応するARIAのセリフを出力する
        /// </summary>
        public static async Task RespondAsync(
            IAsyncTextWriter stdout,
            PlayerAction action,
            CancellationToken ct)
        {
            if (!ResponseBank.TryGetValue(action, out var lines))
                return;

            await UniTask.Delay(ThinkDelayMs, cancellationToken: ct);

            var line = lines[Random.Next(lines.Length)];
            await stdout.WriteLineAsync($"ARIA> {line}", ct);
        }

        /// <summary>
        /// vault.keyの検索など特定のセンシティブファイルを読んだときに反応する
        /// </summary>
        public static bool IsSensitiveFile(string fileName)
        {
            return fileName == "vault.key" || fileName == "vault.enc";
        }
    }
}
