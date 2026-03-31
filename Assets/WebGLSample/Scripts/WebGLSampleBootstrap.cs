using UnityEngine;

namespace Xeon.XTerminal.WebGLSample
{
    /// <summary>
    /// WebGLサンプル用のブートストラップ
    /// XTerminalにWebGLサンプルコマンドを登録する
    /// </summary>
    public class WebGLSampleBootstrap : MonoBehaviour
    {
        [SerializeField] private Sample.XTerminal terminal;

        private void Start()
        {
            if (terminal == null)
            {
                terminal = FindAnyObjectByType<Sample.XTerminal>();
                if (terminal == null)
                {
                    Debug.LogError("[WebGLSampleBootstrap] XTerminal component not found in scene.");
                    return;
                }
            }

            RegisterCommands();
        }

        private void RegisterCommands()
        {
            var registry = terminal.Terminal.Registry;
            registry.RegisterCommand<SampleWebGLCommand>();
        }
    }
}
