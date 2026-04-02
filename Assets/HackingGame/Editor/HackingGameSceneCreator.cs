using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Xeon.Common.FlyweightScrollView;
using Xeon.XTerminal.Sample;

namespace Xeon.XTerminal.HackingGame.Editor
{
    /// <summary>
    /// ハッキングミニゲーム用のシーンを自動生成するEditorユーティリティ
    /// </summary>
    public static class HackingGameSceneCreator
    {
        private const string ScenePath = "Assets/HackingGame/Scenes/HackingGame.unity";
        private const string XTerminalPrefabPath = "Assets/Sample/Prefabs/XTerminal.prefab";
        private const string MessagePrefabPath = "Assets/Sample/Prefabs/Message.prefab";

        [MenuItem("Tools/XTerminal/Create HackingGame Scene", priority = 300)]
        public static void CreateScene()
        {
            if (File.Exists(ScenePath))
            {
                if (!EditorUtility.DisplayDialog(
                        "シーン上書き確認",
                        $"{ScenePath} は既に存在します。上書きしますか？",
                        "上書き", "キャンセル"))
                    return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();
            SetupXTerminal(canvas);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[HackingGameSceneCreator] Scene created: {ScenePath}");
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.06f, 0.14f);
            camera.orthographic = true;
            cameraGo.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        private static GameObject CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.layer = LayerMask.NameToLayer("UI");

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            return canvasGo;
        }

        private static void SetupXTerminal(GameObject canvas)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(XTerminalPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[HackingGameSceneCreator] XTerminal prefab not found: {XTerminalPrefabPath}");
                return;
            }

            var xTerminalGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            xTerminalGo.name = "XTerminal";

            // XTerminalをCanvas全体に広げる
            var rt = xTerminalGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            // HackingGameBootstrapを追加
            var bootstrapGo = new GameObject("HackingGameBootstrap");
            bootstrapGo.transform.SetParent(canvas.transform, false);
            var bootstrap = bootstrapGo.AddComponent<HackingGameBootstrap>();

            // XTerminalコンポーネントの参照を設定
            var xTerminal = xTerminalGo.GetComponent<Sample.XTerminal>();
            if (xTerminal != null)
            {
                var so = new SerializedObject(bootstrap);
                var terminalProp = so.FindProperty("terminal");
                terminalProp.objectReferenceValue = xTerminal;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
