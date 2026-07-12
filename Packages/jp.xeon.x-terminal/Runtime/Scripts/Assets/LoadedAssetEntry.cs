using System;

namespace Xeon.XTerminal.Assets
{
    /// <summary>
    /// ロード済みアセットのエントリ
    /// </summary>
    public class LoadedAssetEntry
    {
        /// <summary>
        /// ロード済みアセット
        /// </summary>
        public UnityEngine.Object Asset { get; set; }

        /// <summary>
        /// インスタンスID（Unity 6.5以降はEntityId由来の64bit値）
        /// </summary>
        public long InstanceId { get; set; }

        /// <summary>
        /// アセット名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// プロバイダー固有のキー
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// アセット型
        /// </summary>
        public Type AssetType { get; set; }

        /// <summary>
        /// プロバイダー名
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// ロード時刻
        /// </summary>
        public DateTime LoadedAt { get; set; }

        /// <summary>
        /// 表示用文字列を生成します
        /// </summary>
        public override string ToString()
        {
            return $"{Name} #{InstanceId} ({AssetType?.Name ?? "Unknown"}) [{ProviderName}]";
        }
    }
}
