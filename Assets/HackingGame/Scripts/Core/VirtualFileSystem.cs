using System.Collections.Generic;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ゲーム内の仮想ファイルシステム
    /// ノードごとのファイル一覧とその内容を管理する
    /// </summary>
    public class VirtualFileSystem
    {
        // nodeIp → (fileName → content)
        private readonly Dictionary<string, Dictionary<string, string>> nodeFiles;

        public VirtualFileSystem()
        {
            nodeFiles = new Dictionary<string, Dictionary<string, string>>();
        }

        public void AddFile(string nodeIp, string fileName, string content)
        {
            if (!nodeFiles.ContainsKey(nodeIp))
                nodeFiles[nodeIp] = new Dictionary<string, string>();

            nodeFiles[nodeIp][fileName] = content;
        }

        public IEnumerable<string> ListFiles(string nodeIp)
        {
            if (nodeFiles.TryGetValue(nodeIp, out var files))
                return files.Keys;

            return System.Array.Empty<string>();
        }

        public bool TryReadFile(string nodeIp, string fileName, out string content)
        {
            content = null;

            if (!nodeFiles.TryGetValue(nodeIp, out var files))
                return false;

            return files.TryGetValue(fileName, out content);
        }

        public bool FileExists(string nodeIp, string fileName)
        {
            if (!nodeFiles.TryGetValue(nodeIp, out var files))
                return false;

            return files.ContainsKey(fileName);
        }
    }
}
