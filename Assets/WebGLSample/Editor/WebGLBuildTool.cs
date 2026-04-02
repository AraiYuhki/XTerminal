using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Xeon.XTerminal.WebGLSample.Editor
{
    /// <summary>
    /// WebGLビルド用のEditorツール
    /// ビルドターゲットごとにシーンと出力先を切り替えてビルドする
    /// </summary>
    public static class WebGLBuildTool
    {
        private const string WebGLTemplateName = "XTerminal";

        private static readonly BuildTarget[] targets =
        {
            new("WebGL Sample", "Assets/WebGLSample/Scenes/WebGLSample.unity", "WebGLBuild/Sample"),
            new("Hacking Game", "Assets/HackingGame/Scenes/HackingGame.unity", "WebGLBuild/HackingGame"),
        };

        // -- Menu Items --

        [MenuItem("Tools/XTerminal/Build WebGL/Sample", priority = 200)]
        public static void BuildSample() => ExecuteBuild(targets[0]);

        [MenuItem("Tools/XTerminal/Build WebGL/Hacking Game", priority = 201)]
        public static void BuildHackingGame() => ExecuteBuild(targets[1]);

        [MenuItem("Tools/XTerminal/Build WebGL/All", priority = 210)]
        public static void BuildAll()
        {
            foreach (var target in targets)
                ExecuteBuild(target);
        }

        // -- CLI Entry Points --

        /// <summary>
        /// -executeMethod Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildSampleFromCLI
        /// </summary>
        public static void BuildSampleFromCLI() => BuildFromCommandLine(targets[0]);

        /// <summary>
        /// -executeMethod Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildHackingGameFromCLI
        /// </summary>
        public static void BuildHackingGameFromCLI() => BuildFromCommandLine(targets[1]);

        /// <summary>
        /// -executeMethod Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildAllFromCLI
        /// </summary>
        public static void BuildAllFromCLI()
        {
            foreach (var target in targets)
                BuildFromCommandLine(target);
        }

        private static void BuildFromCommandLine(BuildTarget target)
        {
            var args = System.Environment.GetCommandLineArgs();
            var outputOverride = GetCommandLineArg(args, "-buildOutput");
            var outputPath = outputOverride ?? GetAbsoluteOutputPath(target.DefaultOutputDir);
            ExecuteBuild(target, outputPath);
        }

        private static void ExecuteBuild(BuildTarget target)
        {
            ExecuteBuild(target, GetAbsoluteOutputPath(target.DefaultOutputDir));
        }

        private static void ExecuteBuild(BuildTarget target, string outputPath)
        {
            Debug.Log($"[WebGLBuildTool] === Building: {target.Name} ===");
            Debug.Log($"[WebGLBuildTool] Scene:  {target.ScenePath}");
            Debug.Log($"[WebGLBuildTool] Output: {outputPath}");

            if (!File.Exists(target.ScenePath))
            {
                Debug.LogError($"[WebGLBuildTool] Scene not found: {target.ScenePath}");
                return;
            }

            ConfigureWebGLSettings();

            var options = new BuildPlayerOptions
            {
                scenes = new[] { target.ScenePath },
                locationPathName = outputPath,
                target = UnityEditor.BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[WebGLBuildTool] Build succeeded: {target.Name} → {outputPath}");
            else
                Debug.LogError($"[WebGLBuildTool] Build failed: {target.Name} ({report.summary.result})");
        }

        private static void ConfigureWebGLSettings()
        {
            PlayerSettings.WebGL.template = $"PROJECT:{WebGLTemplateName}";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        }

        private static string GetAbsoluteOutputPath(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePath);
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

        /// <summary>
        /// ビルドターゲットの定義
        /// </summary>
        private readonly struct BuildTarget
        {
            public string Name { get; }
            public string ScenePath { get; }
            public string DefaultOutputDir { get; }

            public BuildTarget(string name, string scenePath, string defaultOutputDir)
            {
                Name = name;
                ScenePath = scenePath;
                DefaultOutputDir = defaultOutputDir;
            }
        }
    }
}
