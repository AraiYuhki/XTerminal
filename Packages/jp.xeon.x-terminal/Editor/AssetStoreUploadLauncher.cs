using UnityEditor;

namespace Xeon.XTerminal.Editor
{
    // Asset Store へのアップロードを補助するエディタユーティリティ。
    // Tools > XTerminal > Open Asset Store Uploader からアップロードウィンドウを開きます。
    // upload-asset-store.sh からも -executeMethod で呼び出せます。
    internal static class AssetStoreUploadLauncher
    {
        [MenuItem("Tools/XTerminal/Open Asset Store Uploader")]
        public static void Launch()
        {
            EditorApplication.ExecuteMenuItem("Tools/Asset Store/Uploader");
        }
    }
}
