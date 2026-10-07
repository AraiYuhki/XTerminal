using UnityEngine;

namespace Xeon.XTerminal
{
    /// <summary>
    /// UnityEngine.Objectを識別するint型IDを取得するユーティリティ
    /// Unity 6000.6以降はGetInstanceID()とEntityIdのint変換が廃止されたため、バージョン差異をここで吸収します
    /// </summary>
    public static class ObjectIdUtility
    {
        /// <summary>
        /// オブジェクトのIDを取得します
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>オブジェクトのID</returns>
        public static int GetId(Object obj)
        {
#if UNITY_6000_6_OR_NEWER
            // EntityIdは64bitだが、下位32bit（EntityId.ToString()の前半部分と同じ値）は
            // 生存中のオブジェクト間で一意なため、従来のint型IDとして扱う
            return unchecked((int)EntityId.ToULong(obj.GetEntityId()));
#else
            return ObjectIdUtility.GetId(obj);
#endif
        }
    }
}
