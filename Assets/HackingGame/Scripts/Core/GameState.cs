using System;
using System.Collections.Generic;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ハッキングゲームのグローバル状態を管理するシングルトン
    /// コマンドからはGameState.Instanceを通じてアクセスする
    /// </summary>
    public class GameState
    {
        public static GameState Instance { get; private set; }

        public List<NetworkNode> NetworkNodes { get; private set; }
        public NetworkNode ConnectedNode { get; private set; }
        public VirtualFileSystem FileSystem { get; private set; }
        public GamePhase Phase { get; private set; }
        public int Score { get; private set; }

        public bool IsConnected => ConnectedNode != null;

        public event Action<PlayerAction, NetworkNode> OnPlayerAction;

        private GameState() { }

        /// <summary>
        /// ゲームステートを初期化してシングルトンを生成する
        /// </summary>
        public static void Initialize(List<NetworkNode> nodes, VirtualFileSystem fs)
        {
            Instance = new GameState
            {
                NetworkNodes = nodes,
                FileSystem = fs,
                Phase = GamePhase.Scanning,
                Score = 0
            };
        }

        public void Connect(NetworkNode node)
        {
            ConnectedNode = node;
            Phase = GamePhase.Connected;
            OnPlayerAction?.Invoke(PlayerAction.Connect, node);
        }

        public void Disconnect()
        {
            ConnectedNode = null;
            Phase = GamePhase.Scanning;
        }

        public void CrackNode(NetworkNode node)
        {
            node.MarkCracked();
            Score += ScoreTable.CrackBonus;
            OnPlayerAction?.Invoke(PlayerAction.Crack, node);
        }

        public void CompleteExtraction()
        {
            Phase = GamePhase.Complete;
            Score += ScoreTable.ExtractionBonus;
            OnPlayerAction?.Invoke(PlayerAction.Extract, null);
        }

        public NetworkNode FindNode(string ipAddress)
        {
            return NetworkNodes.Find(n =>
                string.Equals(n.IpAddress, ipAddress, StringComparison.OrdinalIgnoreCase));
        }
    }

    public enum GamePhase
    {
        Scanning,
        Connected,
        Complete
    }

    public enum PlayerAction
    {
        Scan,
        Connect,
        ConnectFailed,
        Crack,
        CrackFailed,
        Extract,
        ReadSensitiveFile
    }

    // スコア定数をまとめた静的クラス
    public static class ScoreTable
    {
        public const int CrackBonus = 100;
        public const int ExtractionBonus = 500;
        public const int ScanBonus = 10;
    }
}
