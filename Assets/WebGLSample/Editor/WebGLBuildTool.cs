using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Xeon.XTerminal.WebGLSample.Editor
{
    /// <summary>
    /// WebGLビルド用のEditorツール
    /// メニューからWebGLビルドを実行できる
    /// </summary>
    public static class WebGLBuildTool
    {
        private const string DefaultOutputDir = "WebGLBuild";
        private const string WebGLTemplateName = "XTerminal";

        [MenuItem("Tools/XTerminal/Build WebGL", priority = 200)]
        public static void BuildWebGL()
        {
            var outputPath = EditorUtility.SaveFolderPanel(
                "WebGL Build Output",
                Path.GetDirectoryName(Application.dataPath),
                DefaultOutputDir);

            if (string.IsNullOrEmpty(outputPath))
                return;

            ExecuteBuild(outputPath);
        }

        [MenuItem("Tools/XTerminal/Build WebGL (Default Path)", priority = 201)]
        public static void BuildWebGLDefault()
        {
            var outputPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), DefaultOutputDir);
            ExecuteBuild(outputPath);
        }

        /// <summary>
        /// コマンドラインからのビルド実行用エントリポイント
        /// -executeMethod Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildFromCommandLine
        /// </summary>
        public static void BuildFromCommandLine()
        {
            var args = System.Environment.GetCommandLineArgs();
            var outputPath = GetCommandLineArg(args, "-buildOutput")
                ?? Path.Combine(Path.GetDirectoryName(Application.dataPath), DefaultOutputDir);

            ExecuteBuild(outputPath);
        }

        private static void ExecuteBuild(string outputPath)
        {
            Debug.Log($"[WebGLBuildTool] Build output: {outputPath}");

            ConfigureWebGLSettings();

            var scenes = GetBuildScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[WebGLBuildTool] No scenes found in Build Settings.");
                return;
            }

            Debug.Log($"[WebGLBuildTool] Building {scenes.Length} scene(s)...");
            foreach (var scene in scenes)
                Debug.Log($"  - {scene}");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"[WebGLBuildTool] Build succeeded: {outputPath}");
            else
                Debug.LogError($"[WebGLBuildTool] Build failed: {report.summary.result}");
        }

        private static void ConfigureWebGLSettings()
        {
            // WebGLテンプレートの設定
            var templatePath = $"PROJECT:{WebGLTemplateName}";
            PlayerSettings.WebGL.template = templatePath;

            // 圧縮設定: gzip（多くのサーバーで対応）
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;

            // デバッグシンボルを除外して軽量化
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

            // 例外処理: 明示的にスローされた例外のみ（パフォーマンス重視）
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            Debug.Log($"[WebGLBuildTool] WebGL template: {templatePath}");
        }

        private static string[] GetBuildScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        private static string GetCommandLineArg(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return null;
        }
    }
}
