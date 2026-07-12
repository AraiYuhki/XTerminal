using Object = UnityEngine.Object;

namespace Xeon.XTerminal
{
    /// <summary>
    /// UnityバージョンごとのオブジェクトID取得APIの差異を吸収する互換ヘルパー
    /// Unity 6.4でObject.GetInstanceIDが非推奨化、Unity 6.5でコンパイルエラーとなったため、
    /// Unity 6.4以降はGetEntityId、それ以前はGetInstanceIDを使用します
    /// </summary>
    public static class EntityIdCompat
    {
        /// <summary>
        /// オブジェクトを一意に識別するIDを取得します
        /// Unity 6.5以降のEntityIdは64bit値のためlongで返します
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>オブジェクトの識別ID</returns>
        public static long GetEntityIdCompat(this Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return unchecked((long)UnityEngine.EntityId.ToULong(obj.GetEntityId()));
#else
            return obj.GetInstanceID();
#endif
        }
    }
}
