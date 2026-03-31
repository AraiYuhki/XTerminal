using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.XTerminal.WebGLSample
{
    /// <summary>
    /// WebGL環境のブラウザ情報を表示するサンプルコマンド
    /// XTerminalのカスタムコマンド作成例として、WebGL固有の情報取得を実装
    /// </summary>
    [Command("sysinfo", "Display system and browser information")]
    public class SampleWebGLCommand : ICommand
    {
        [Option("all", "a", Description = "Show all available information")]
        public bool ShowAll;

        public string CommandName => "sysinfo";
        public string Description => "Display system and browser information";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            await context.Stdout.WriteLineAsync($"Platform:       {Application.platform}", ct);
            await context.Stdout.WriteLineAsync($"Unity Version:  {Application.unityVersion}", ct);
            await context.Stdout.WriteLineAsync($"Screen:         {Screen.width}x{Screen.height}", ct);
            await context.Stdout.WriteLineAsync($"DPI:            {Screen.dpi}", ct);

            if (!ShowAll)
                return ExitCode.Success;

            await context.Stdout.WriteLineAsync($"SystemLanguage: {Application.systemLanguage}", ct);
            await context.Stdout.WriteLineAsync($"Device Model:   {SystemInfo.deviceModel}", ct);
            await context.Stdout.WriteLineAsync($"Device Type:    {SystemInfo.deviceType}", ct);
            await context.Stdout.WriteLineAsync($"OS:             {SystemInfo.operatingSystem}", ct);
            await context.Stdout.WriteLineAsync($"GPU:            {SystemInfo.graphicsDeviceName}", ct);
            await context.Stdout.WriteLineAsync($"GPU API:        {SystemInfo.graphicsDeviceType}", ct);
            await context.Stdout.WriteLineAsync($"System Memory:  {SystemInfo.systemMemorySize} MB", ct);
            await context.Stdout.WriteLineAsync($"Persistent:     {Application.persistentDataPath}", ct);

            return ExitCode.Success;
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            return Enumerable.Empty<string>();
        }
    }
}
