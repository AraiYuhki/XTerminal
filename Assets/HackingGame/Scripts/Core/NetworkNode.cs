namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ネットワーク上のノード（サーバー）を表すモデル
    /// </summary>
    public class NetworkNode
    {
        public string IpAddress { get; }
        public string NodeType { get; }
        public string Description { get; }
        public bool IsCracked { get; private set; }
        public CrackDifficulty Difficulty { get; }

        public NetworkNode(string ipAddress, string nodeType, string description, CrackDifficulty difficulty)
        {
            IpAddress = ipAddress;
            NodeType = nodeType;
            Description = description;
            Difficulty = difficulty;
        }

        public void MarkCracked()
        {
            IsCracked = true;
        }
    }

    public enum CrackDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
