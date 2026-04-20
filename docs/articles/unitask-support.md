# UniTask サポート

XTerminalは、高パフォーマンスな非同期操作のために [UniTask](https://github.com/Cysharp/UniTask) とのオプションの統合を提供しています。

## 概要

UniTask サポートは、プロジェクトに UniTask がインストールされている場合に**自動的に有効**になります。追加の設定は必要ありません。

UniTask が検出されると、`UNI_TERMINAL_UNI_TASK_SUPPORT` シンボルが自動的に定義されます。

## インストール

### UniTask のインストール

Package Manager を介してプロジェクトに UniTask を追加します。

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

または `manifest.json` を介して追加します。

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
  }
}
```

XTerminal は自動的に UniTask を検出し、サポートを有効にします。

## UniTask による実行

### 基本的な使い方

```csharp
using Cysharp.Threading.Tasks;
using Xeon.XTerminal;

public class UniTaskExample : MonoBehaviour
{
    private Terminal _terminal;

    void Start()
    {
        _terminal = new Terminal(
            Application.dataPath,
            Application.dataPath,
            true
        );

        ExecuteCommand().Forget();
    }

    async UniTaskVoid ExecuteCommand()
    {
        var stdout = new UniTaskStringBuilderTextWriter();
        var stderr = new UniTaskStringBuilderTextWriter();

        var exitCode = await _terminal.ExecuteUniTaskAsync(
            "hierarchy -r",
            stdout,
            stderr
        );

        Debug.Log(stdout.ToString());
    }
}
```

### UniTask テキストライター

XTerminal は UniTask 専用のテキストライターを提供しています。

| クラス | 説明 |
|-------|-------------|
| `UniTaskStringBuilderTextWriter` | StringBuilder に書き込みます |
| `UniTaskListTextWriter` | List<string> に書き込みます |

```csharp
// StringBuilder への出力
var stdout = new UniTaskStringBuilderTextWriter();
await terminal.ExecuteUniTaskAsync("echo Hello", stdout, stderr);
string result = stdout.ToString();

// リストへの出力 (行単位)
var listWriter = new UniTaskListTextWriter();
await terminal.ExecuteUniTaskAsync("hierarchy -r", listWriter, stderr);
foreach (var line in listWriter.Lines)
{
    Debug.Log(line);
}
```

## UniTask コマンドの作成

UniTask を使用するコマンドには `IUniTaskCommand` を実装します。

```csharp
using Cysharp.Threading.Tasks;
using Xeon.XTerminal;
using System.Collections.Generic;
using System.Threading;

[Command("download", "リソースをダウンロードする")]
public class DownloadCommand : IUniTaskCommand
{
    [Option("url", "u", Description = "ダウンロードするURL")]
    public string Url;

    public string CommandName => "download";
    public string Description => "リソースをダウンロードする";

    public async UniTask<ExitCode> ExecuteAsync(
        UniTaskCommandContext context,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(Url))
        {
            await context.Stderr.WriteLineAsync("Error: URLが必要です", ct);
            return ExitCode.UsageError;
        }

        await context.Stdout.WriteLineAsync($"ダウンロード中: {Url}", ct);

        // UniTask の非同期操作を使用
        using var request = UnityWebRequest.Get(Url);
        await request.SendWebRequest().ToUniTask(cancellationToken: ct);

        if (request.result == UnityWebRequest.Result.Success)
        {
            await context.Stdout.WriteLineAsync(request.downloadHandler.text, ct);
            return ExitCode.Success;
        }
        else
        {
            await context.Stderr.WriteLineAsync($"Error: {request.error}", ct);
            return ExitCode.RuntimeError;
        }
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

## UniTaskCommandContext

`UniTaskCommandContext` は、UniTask 互換の I/O を提供します。

| プロパティ | 型 | 説明 |
|----------|------|-------------|
| `Stdin` | `IUniTaskTextReader` | 入力ストリーム |
| `Stdout` | `IUniTaskTextWriter` | 出力ストリーム |
| `Stderr` | `IUniTaskTextWriter` | エラー出力ストリーム |
| `Arguments` | `IReadOnlyList<string>` | 位置引数 |
| `WorkingDirectory` | `string` | 現在の作業ディレクトリ |
| `HomeDirectory` | `string` | ホームディレクトリ |
| `Terminal` | `Terminal` | Terminal インスタンス |

## インターフェースリファレンス

### IUniTaskTextWriter

```csharp
public interface IUniTaskTextWriter
{
    UniTask WriteAsync(string value, CancellationToken ct = default);
    UniTask WriteLineAsync(string value, CancellationToken ct = default);
    UniTask WriteLineAsync(CancellationToken ct = default);
}
```

### IUniTaskTextReader

```csharp
public interface IUniTaskTextReader
{
    UniTask<string> ReadLineAsync(CancellationToken ct = default);
    UniTask<string> ReadToEndAsync(CancellationToken ct = default);
}
```

## 標準コマンドと UniTask コマンドの混在

XTerminal は、標準の `ICommand` と `IUniTaskCommand` の両方をシームレスに処理します。

```csharp
// 両方のタイプを登録
terminal.Registry.Register<StandardCommand>();  // ICommand
terminal.Registry.Register<UniTaskCommand>();   // IUniTaskCommand

// UniTask で実行 - 両方のコマンドが動作します
await terminal.ExecuteUniTaskAsync("standard-cmd | unitask-cmd", stdout, stderr);
```

## パフォーマンスの利点

UniTask は以下のメリットを提供します。

- **ゼロアロケーション**な async/await
- Task ベースの非同期処理よりも**優れたパフォーマンス**
- **Unity に最適化**されたタイミングとスケジューリング
- Unity のライフサイクルと統合された**キャンセルサポート**

## 条件付きコンパイル

UniTask の有無に関わらず動作するコードを書く必要がある場合:

```csharp
#if UNI_TERMINAL_UNI_TASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

public class ConditionalExample : MonoBehaviour
{
#if UNI_TERMINAL_UNI_TASK_SUPPORT
    async UniTaskVoid Start()
    {
        var stdout = new UniTaskStringBuilderTextWriter();
        var stderr = new UniTaskStringBuilderTextWriter();
        await _terminal.ExecuteUniTaskAsync("help", stdout, stderr);
    }
#else
    async void Start()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        await _terminal.ExecuteAsync("help", stdout, stderr, destroyCancellationToken);
    }
#endif
}
```

## ベストプラクティス

1. **UI には UniTask を使う** - ターミナル出力表示のパフォーマンスが向上します。
2. **キャンセルを活用する** - CancellationToken を適切に渡します。
3. **適切なライターを使う** - 行単位の処理には `UniTaskListTextWriter` を使用します。
4. **ブロックしない** - `.Result` や `.Wait()` の代わりに `await` を使用します。
