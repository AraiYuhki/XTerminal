using System.Collections.Generic;

namespace Xeon.XTerminal.HackingGame
{
    /// <summary>
    /// ステージ1のネットワーク構成と仮想ファイルシステムを生成する
    /// 新ステージを追加する場合はこのクラスにメソッドを追加する
    /// </summary>
    public static class StageLoader
    {
        // ステージ1の各ノードIPアドレス定数
        public const string GatewayIp = "10.0.0.1";
        public const string DatabaseIp = "10.0.0.5";
        public const string ArchiveIp = "10.0.0.9";

        // DatabaseのExtractに必要な鍵（Archiveのvault.keyに埋め込まれている）
        public const string MasterKey = "cipher2049";

        public static (List<NetworkNode> nodes, VirtualFileSystem fs) LoadStage1()
        {
            var nodes = CreateNodes();
            var fs = CreateFileSystem();
            return (nodes, fs);
        }

        private static List<NetworkNode> CreateNodes()
        {
            return new List<NetworkNode>
            {
                new NetworkNode(GatewayIp,  "GATEWAY", "メインゲートウェイ。ファイアウォール強固。",    CrackDifficulty.Hard),
                new NetworkNode(DatabaseIp, "DATABASE","暗号化ボルトを持つデータベースサーバー。",         CrackDifficulty.Medium),
                new NetworkNode(ArchiveIp,  "ARCHIVE", "状態不明のアーカイブサーバー。何かが眠っている。", CrackDifficulty.Easy),
            };
        }

        private static VirtualFileSystem CreateFileSystem()
        {
            var fs = new VirtualFileSystem();

            AddGatewayFiles(fs);
            AddDatabaseFiles(fs);
            AddArchiveFiles(fs);

            return fs;
        }

        private static void AddGatewayFiles(VirtualFileSystem fs)
        {
            fs.AddFile(GatewayIp, "firewall.rules",
                "# Firewall Configuration v4.2\n" +
                "DENY  ALL  INBOUND  DEFAULT\n" +
                "ALLOW TCP  443  FROM 192.168.0.0/24\n" +
                "ALLOW TCP  22   FROM 10.0.0.0/8\n" +
                "BLOCK ICMP ALL\n" +
                "# Last updated: 2026-03-28 by admin");

            fs.AddFile(GatewayIp, "access.log",
                "[2026-03-31 02:14:08] FAILED LOGIN: root@unknown\n" +
                "[2026-03-31 02:14:09] FAILED LOGIN: root@unknown\n" +
                "[2026-03-31 02:14:11] FAILED LOGIN: admin@unknown\n" +
                "[2026-03-31 03:55:02] SESSION OPEN: system@10.0.0.5\n" +
                "[2026-03-31 04:01:33] FILE ACCESS: /archive/vault.key\n" +
                "[2026-03-31 04:01:34] SESSION CLOSE: system@10.0.0.5");

            fs.AddFile(GatewayIp, "network.map",
                "== NETWORK TOPOLOGY ==\n" +
                "10.0.0.1  GATEWAY   [ONLINE]\n" +
                "10.0.0.5  DATABASE  [ONLINE]\n" +
                "10.0.0.9  ARCHIVE   [ONLINE]\n" +
                "=====================");
        }

        private static void AddDatabaseFiles(VirtualFileSystem fs)
        {
            fs.AddFile(DatabaseIp, "vault.enc",
                "[ENCRYPTED CONTENT - AES-256]\n" +
                "Requires master key to decrypt.\n" +
                "Use: extract --key <key> vault.enc");

            fs.AddFile(DatabaseIp, "users.db",
                "id,username,role,last_login\n" +
                "1,admin,superuser,2026-03-30\n" +
                "2,aria,system,2026-03-31\n" +
                "3,guest,readonly,2026-02-14");

            fs.AddFile(DatabaseIp, "schema.sql",
                "CREATE TABLE vault (\n" +
                "  id       INTEGER PRIMARY KEY,\n" +
                "  payload  BLOB NOT NULL,\n" +
                "  created  TIMESTAMP DEFAULT CURRENT_TIMESTAMP\n" +
                ");\n" +
                "-- key stored externally at ARCHIVE node");
        }

        private static void AddArchiveFiles(VirtualFileSystem fs)
        {
            fs.AddFile(ArchiveIp, "system.log",
                "[2026-03-28 11:00:00] SYSTEM INIT\n" +
                "[2026-03-28 11:00:05] VAULT KEY GENERATED\n" +
                "[2026-03-29 09:45:12] BACKUP CREATED: vault.key\n" +
                "[2026-03-30 22:30:00] ARIA MONITORING ENABLED\n" +
                "[2026-03-31 04:01:33] KEY ACCESSED BY: system@10.0.0.5\n" +
                "-- END OF LOG --");

            fs.AddFile(ArchiveIp, "backup.cfg",
                "# Archive Backup Configuration\n" +
                "SCHEDULE=daily\n" +
                "TARGET=/archive/\n" +
                "RETENTION=30d\n" +
                "ENCRYPT=false\n" +
                "# Note: vault.key is stored unencrypted for recovery purposes");

            fs.AddFile(ArchiveIp, "vault.key",
                "# Master Vault Key File\n" +
                "# Generated: 2026-03-28\n" +
                "# WARNING: Do not share this file\n" +
                "#\n" +
                $"master_key={MasterKey}\n" +
                "#\n" +
                "# Key rotation scheduled: 2026-06-28");
        }
    }
}
