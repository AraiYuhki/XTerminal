using System;
using System.Threading;

namespace Xeon.XTerminal.HackingGame
{
    using UniTask = Cysharp.Threading.Tasks.UniTask;
    /// <summary>
    /// ゲーム開始時に表示されるイントロメッセージ
    /// 匿名の協力者からの暗号化通信という体裁でチュートリアルを提供する
    /// </summary>
    public static class IntroSequence
    {
        private const int LineDelayMs = 80;
        private const int ParagraphDelayMs = 400;
        private const int LongPauseMs = 1200;

        /// <summary>
        /// イントロメッセージを1行ずつ遅延付きで出力する
        /// </summary>
        /// <param name="writeLine">1行出力するデリゲート</param>
        /// <param name="ct">キャンセルトークン</param>
        public static async UniTask PlayAsync(Action<string> writeLine, CancellationToken ct)
        {
            await UniTask.Delay(LongPauseMs, cancellationToken: ct);

            Write(writeLine, "[INCOMING TRANSMISSION - ENCRYPTED CHANNEL]");
            Write(writeLine, "--------------------------------------------");
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);
            Write(writeLine, "");

            Write(writeLine, "差出人不明のメッセージを受信しました。");
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);
            Write(writeLine, "");

            await WriteWithDelay(writeLine, "> ……聞こえるか。", ct);
            await WriteWithDelay(writeLine, "> このチャンネルは安全だ。長くは持たないが。", ct);
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);

            await WriteWithDelay(writeLine, "> お前に頼みたいことがある。", ct);
            await WriteWithDelay(writeLine, "> ARIAという存在が、このネットワークの奥に何かを隠している。", ct);
            await WriteWithDelay(writeLine, "> それが何なのか——突き止めてほしい。", ct);
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);

            await WriteWithDelay(writeLine, "> まず 'scan' でネットワークを調べろ。", ct);
            await WriteWithDelay(writeLine, "> ノードを見つけたら 'crack <IP>' で認証を突破。", ct);
            await WriteWithDelay(writeLine, "> 突破したら 'connect <IP>' で接続しろ。", ct);
            await WriteWithDelay(writeLine, "> 'ls' でファイルを探し、'cat <ファイル名>' で中身を読め。", ct);
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);

            await WriteWithDelay(writeLine, "> ヒントはある。簡単なノードから攻めろ。", ct);
            await WriteWithDelay(writeLine, "> 鍵は必ずどこかに眠っている。", ct);
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);

            await WriteWithDelay(writeLine, "> ……もう時間がない。健闘を祈る。", ct);
            await UniTask.Delay(ParagraphDelayMs, cancellationToken: ct);

            Write(writeLine, "");
            Write(writeLine, "[TRANSMISSION END]");
            Write(writeLine, "");
        }

        private static void Write(Action<string> writeLine, string line)
        {
            writeLine(line);
        }

        private static async UniTask WriteWithDelay(Action<string> writeLine, string line, CancellationToken ct)
        {
            writeLine(line);
            await UniTask.Delay(LineDelayMs, cancellationToken: ct);
        }
    }
}
