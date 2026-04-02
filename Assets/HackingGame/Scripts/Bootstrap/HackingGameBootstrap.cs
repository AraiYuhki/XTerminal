using System.Threading;
using UnityEngine;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ハッキングゲームの初期化とコマンド登録を担うブートストラップ
    /// ゲームシーンのGameObjectにアタッチして使用する
    ///
    /// セットアップ手順:
    ///   1. シーンにXTerminal (Sample.XTerminal) を配置
    ///   2. このコンポーネントをGameObjectにアタッチ
    ///   3. Inspectorで Terminal フィールドにXTerminalを設定（省略時は自動検索）
    ///
    /// 注意: ls / cat / grep は仮想FS版で同名の組み込みコマンドを上書きする。
    ///       これはゲームシーンにおいて意図的な動作である。
    /// </summary>
    public class HackingGameBootstrap : MonoBehaviour
    {
        [SerializeField] private Sample.XTerminal terminal;

        private async void Start()
        {
            if (terminal == null)
            {
                terminal = FindAnyObjectByType<Sample.XTerminal>();
                if (terminal == null)
                {
                    Debug.LogError("[HackingGameBootstrap] XTerminal component not found in scene.");
                    return;
                }
            }

            InitializeGame();
            RegisterCommands();
            await PlayIntro();
        }

        private void InitializeGame()
        {
            var (nodes, fs) = StageLoader.LoadStage1();
            GameState.Initialize(nodes, fs);
            Debug.Log("[HackingGameBootstrap] Stage 1 initialized.");
        }

        private void RegisterCommands()
        {
            var registry = terminal.Terminal.Registry;

            registry.RegisterCommand<ScanCommand>();
            registry.RegisterCommand<ConnectCommand>();
            registry.RegisterCommand<DisconnectCommand>();
            registry.RegisterCommand<CrackCommand>();
            registry.RegisterCommand<ExtractCommand>();
            registry.RegisterCommand<StatusCommand>();

            // 組み込みの ls/cat/grep を仮想FS版で差し替え
            registry.UnregisterCommand("ls");
            registry.UnregisterCommand("cat");
            registry.UnregisterCommand("grep");
            registry.RegisterCommand<VirtualLsCommand>();
            registry.RegisterCommand<VirtualCatCommand>();
            registry.RegisterCommand<VirtualGrepCommand>();

            Debug.Log("[HackingGameBootstrap] Commands registered.");
        }

        private async Awaitable PlayIntro()
        {
            try
            {
                await IntroSequence.PlayAsync(terminal.WriteOutputLine, destroyCancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                // シーン破棄時のキャンセルは無視
            }
        }
    }
}
