using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Xeon.XTerminal
{
    /// <summary>
    /// 文字列からComponent型を解決するユーティリティ
    /// </summary>
    public static class TypeResolver
    {
        private static readonly string[] DefaultNamespaces = new[]
        {
            "UnityEngine",
            "UnityEngine.UI",
            "UnityEngine.EventSystems",
            "TMPro"
        };

        // IL2CPP環境（WebGL等）ではリフレクションによる型解決が失敗するため、
        // よく使われるコンポーネント型を直接マッピングして確実に解決できるようにする
        private static readonly Dictionary<string, Type> WellKnownComponentTypes =
            BuildWellKnownComponentTypes();

        private static Dictionary<string, Type> BuildWellKnownComponentTypes()
        {
            var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            RegisterType(map, typeof(Rigidbody));
            RegisterType(map, typeof(Rigidbody2D));
            RegisterType(map, typeof(BoxCollider));
            RegisterType(map, typeof(SphereCollider));
            RegisterType(map, typeof(CapsuleCollider));
            RegisterType(map, typeof(MeshCollider));
            RegisterType(map, typeof(BoxCollider2D));
            RegisterType(map, typeof(CircleCollider2D));
            RegisterType(map, typeof(PolygonCollider2D));
            RegisterType(map, typeof(CharacterController));
            RegisterType(map, typeof(AudioSource));
            RegisterType(map, typeof(AudioListener));
            RegisterType(map, typeof(Camera));
            RegisterType(map, typeof(Light));
            RegisterType(map, typeof(MeshFilter));
            RegisterType(map, typeof(MeshRenderer));
            RegisterType(map, typeof(SkinnedMeshRenderer));
            RegisterType(map, typeof(SpriteRenderer));
            RegisterType(map, typeof(LineRenderer));
            RegisterType(map, typeof(TrailRenderer));
            RegisterType(map, typeof(ParticleSystem));
            RegisterType(map, typeof(Animator));
            RegisterType(map, typeof(Animation));
            RegisterType(map, typeof(Canvas));
            RegisterType(map, typeof(CanvasRenderer));
            RegisterType(map, typeof(RectTransform));

            // UI
            RegisterType(map, typeof(CanvasScaler));
            RegisterType(map, typeof(GraphicRaycaster));
            RegisterType(map, typeof(Image));
            RegisterType(map, typeof(RawImage));
            RegisterType(map, typeof(Text));
            RegisterType(map, typeof(Button));
            RegisterType(map, typeof(Toggle));
            RegisterType(map, typeof(Slider));
            RegisterType(map, typeof(Scrollbar));
            RegisterType(map, typeof(Dropdown));
            RegisterType(map, typeof(InputField));
            RegisterType(map, typeof(ScrollRect));
            RegisterType(map, typeof(Mask));
            RegisterType(map, typeof(RectMask2D));
            RegisterType(map, typeof(HorizontalLayoutGroup));
            RegisterType(map, typeof(VerticalLayoutGroup));
            RegisterType(map, typeof(GridLayoutGroup));
            RegisterType(map, typeof(ContentSizeFitter));
            RegisterType(map, typeof(AspectRatioFitter));

            // EventSystem
            RegisterType(map, typeof(EventSystem));
            RegisterType(map, typeof(StandaloneInputModule));
            return map;
        }

        private static void RegisterType(Dictionary<string, Type> map, Type type)
        {
            map[type.Name] = type;
            map[type.FullName] = type;
        }

        /// <summary>
        /// 型名からComponent型を解決します
        /// </summary>
        /// <param name="typeName">型名（例: "Rigidbody", "MyGame.MyComponent"）</param>
        /// <param name="customNamespace">カスタム名前空間（オプション）</param>
        /// <returns>解決されたType、見つからない場合はnull</returns>
        public static Type ResolveComponentType(string typeName, string customNamespace = null)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            // 直接マッピングから検索（IL2CPP環境でも確実に動作する）
            if (WellKnownComponentTypes.TryGetValue(typeName, out var wellKnownType))
                return wellKnownType;

            // フルネームで指定された場合
            if (typeName.Contains("."))
                return FindTypeInAllAssemblies(typeName);

            // カスタム名前空間が指定された場合
            if (!string.IsNullOrEmpty(customNamespace))
            {
                var type = FindTypeInAllAssemblies($"{customNamespace}.{typeName}");
                if (type != null)
                    return type;
            }

            // デフォルト名前空間を検索
            foreach (var ns in DefaultNamespaces)
            {
                var type = FindTypeInAllAssemblies($"{ns}.{typeName}");
                if (type != null)
                    return type;
            }

            // 名前空間なしで全アセンブリ検索（名前のみマッチ）
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) &&
                                       typeof(Component).IsAssignableFrom(t));
                    if (type != null)
                        return type;
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// フルネームで全アセンブリから型を検索します
        /// </summary>
        private static Type FindTypeInAllAssemblies(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(fullName);
                    if (type != null && typeof(Component).IsAssignableFrom(type))
                        return type;
                }
                catch
                {
                    // エラーは無視
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// 一般的なコンポーネント型名のリストを取得します（補完用）
        /// WellKnownComponentTypesと同期しており、補完で表示される型は確実に解決可能
        /// </summary>
        public static string[] GetCommonComponentNames()
        {
            return WellKnownComponentTypes
                .Where(kv => !kv.Key.Contains("."))
                .Select(kv => kv.Key)
                .OrderBy(name => name)
                .ToArray();
        }

        /// <summary>
        /// 型名からアセット型を解決します
        /// </summary>
        /// <param name="typeName">型名（例: "Texture2D", "Material"）</param>
        /// <returns>解決されたType、見つからない場合はnull</returns>
        public static Type ResolveAssetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            // フルネームで指定された場合
            if (typeName.Contains("."))
                return FindAssetTypeInAllAssemblies(typeName);

            // よく使われるアセット型のエイリアス
            var aliasType = typeName.ToLowerInvariant() switch
            {
                "texture" => typeof(Texture),
                "texture2d" => typeof(Texture2D),
                "sprite" => typeof(Sprite),
                "material" => typeof(Material),
                "mesh" => typeof(Mesh),
                "audioclip" or "audio" => typeof(AudioClip),
                "shader" => typeof(Shader),
                "font" => typeof(Font),
                "prefab" or "gameobject" => typeof(GameObject),
                "animationclip" or "animation" => typeof(AnimationClip),
                "scriptableobject" or "so" => typeof(ScriptableObject),
                "textasset" or "text" => typeof(TextAsset),
                _ => null
            };

            if (aliasType != null)
                return aliasType;

            // デフォルト名前空間を検索
            foreach (var ns in DefaultNamespaces)
            {
                var type = FindAssetTypeInAllAssemblies($"{ns}.{typeName}");
                if (type != null)
                    return type;
            }

            // 名前空間なしで全アセンブリ検索
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) &&
                                       typeof(UnityEngine.Object).IsAssignableFrom(t));
                    if (type != null)
                        return type;
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// フルネームで全アセンブリからアセット型を検索します
        /// </summary>
        private static Type FindAssetTypeInAllAssemblies(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(fullName);
                    if (type != null && typeof(UnityEngine.Object).IsAssignableFrom(type))
                        return type;
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// 一般的なアセット型名のリストを取得します（補完用）
        /// </summary>
        public static string[] GetCommonAssetTypeNames()
        {
            return new[]
            {
                "Texture2D", "Texture", "Sprite", "Material", "Mesh",
                "AudioClip", "Shader", "Font", "GameObject", "Prefab",
                "AnimationClip", "ScriptableObject", "TextAsset"
            };
        }
    }
}
