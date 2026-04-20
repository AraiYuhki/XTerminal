# カスタムコマンド

XTerminalで独自のコマンドを作成し、登録する方法について説明します。

## 基本的なコマンド構造

すべてのコマンドは `ICommand` インターフェースを実装する必要があります。

```csharp
using Xeon.XTerminal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[Command("mycommand", "マイコマンドの説明")]
public class MyCommand : ICommand
{
    public string CommandName => "mycommand";
    public string Description => "マイコマンドの説明";

    public async Task<ExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken ct)
    {
        await context.Stdout.WriteLineAsync("MyCommand からの挨拶です！", ct);
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

## オプションの追加

`[Option]` 属性を使用して、コマンドラインオプションを定義します。

```csharp
[Command("greet", "ユーザーに挨拶する")]
public class GreetCommand : ICommand
{
    [Option("name", "n", Description = "挨拶する相手の名前")]
    public string Name;

    [Option("times", "t", Description = "挨拶する回数")]
    public int Times = 1;

    [Option("uppercase", "u", Description = "大文字で出力する")]
    public bool Uppercase;

    public string CommandName => "greet";
    public string Description => "ユーザーに挨拶する";

    public async Task<ExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken ct)
    {
        var greeting = $"Hello, {Name ?? "World"}!";

        if (Uppercase)
            greeting = greeting.ToUpper();

        for (int i = 0; i < Times; i++)
        {
            await context.Stdout.WriteLineAsync(greeting, ct);
        }

        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

**使用例:**

```bash
greet                          # Hello, World!
greet -n Alice                 # Hello, Alice!
greet --name=Bob --times=3     # Hello, Bob! (3回)
greet -n Unity -t 2 -u         # HELLO, UNITY! (2回)
```

## オプションの型

### サポートされている型

| 型 | 例 | 使い方 |
|------|---------|-------|
| `string` | `"hello"` | `--option=value` |
| `int` | `42` | `--count=42` |
| `float` | `3.14` | `--value=3.14` |
| `bool` | `true/false` | `--flag` (存在すれば true) |
| `Vector2` | `1,2` | `--pos=1,2` |
| `Vector3` | `1,2,3` | `--pos=1,2,3` |
| `Color` | `1,0,0,1` | `--color=1,0,0,1` |

### 位置引数

オプションフラグのない引数は、位置引数として `context.Arguments` に渡されます。

```csharp
[Command("move", "位置に移動する")]
public class MoveCommand : ICommand
{
    public string CommandName => "move";
    public string Description => "位置に移動する";

    public async Task<ExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken ct)
    {
        // move /Player 1,2,3
        // context.Arguments[0] = "/Player"
        // context.Arguments[1] = "1,2,3"

        if (context.Arguments.Count < 2)
        {
            await context.Stderr.WriteLineAsync(
                "Usage: move <object> <position>", ct);
            return ExitCode.UsageError;
        }

        var objectPath = context.Arguments[0];
        var position = context.Arguments[1];

        // ... 実装
        
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

## 標準入力（Stdin）からの読み取り

パイプライン内の前のコマンドからの入力を処理します。

```csharp
[Command("count", "行数または単語数をカウントする")]
public class CountCommand : ICommand
{
    [Option("words", "w", Description = "行数の代わりに単語数をカウントする")]
    public bool CountWords;

    public string CommandName => "count";
    public string Description => "行数または単語数をカウントする";

    public async Task<ExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken ct)
    {
        int count = 0;
        string line;

        while ((line = await context.Stdin.ReadLineAsync(ct)) != null)
        {
            if (CountWords)
            {
                count += line.Split(
                    new[] { ' ', '\t' },
                    StringSplitOptions.RemoveEmptyEntries
                ).Length;
            }
            else
            {
                count++;
            }
        }

        await context.Stdout.WriteLineAsync(count.ToString(), ct);
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

**使用例:**

```bash
hierarchy -r | count           # オブジェクト数をカウント
echo "Hello World" | count -w  # 単語数をカウント (2)
cat file.txt | count           # ファイルの行数をカウント
```

## タブ補完

`GetCompletions` を実装して、コンテキストに応じたサジェストを提供します。

```csharp
[Command("load", "シーンをロードする")]
public class LoadSceneCommand : ICommand
{
    [Option("scene", "s", Description = "シーン名")]
    public string SceneName;

    public string CommandName => "load";
    public string Description => "シーンをロードする";

    public async Task<ExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken ct)
    {
        // ... 実装
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        // --scene オプションを補完中かどうかをチェック
        if (context.CurrentOption == "scene" ||
            context.CurrentOption == "s")
        {
            // 利用可能なシーン名を返す
            var sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (name.StartsWith(context.PartialValue,
                    StringComparison.OrdinalIgnoreCase))
                {
                    yield return name;
                }
            }
        }
    }
}
```

## コマンドの登録

### 手動登録

```csharp
var terminal = new Terminal(
    Application.dataPath,
    Application.dataPath,
    registerBuiltInCommands: true
);

// 個別のコマンドを登録
terminal.Registry.Register<GreetCommand>();
terminal.Registry.Register<CountCommand>();
terminal.Registry.Register<LoadSceneCommand>();
```

### アセンブリ登録

アセンブリ内のすべてのコマンドを一括で登録します。

```csharp
// 現在のアセンブリ内のすべてのコマンドを登録
terminal.Registry.RegisterFromAssembly(
    typeof(MyCommand).Assembly
);

// 複数のアセンブリから登録
terminal.Registry.RegisterFromAssembly(
    typeof(GameCommands).Assembly
);
terminal.Registry.RegisterFromAssembly(
    typeof(DebugCommands).Assembly
);
```

## 終了コード

適切な終了コードを返します。

| コード | 定数 | 使用場面 |
|------|----------|-------------|
| 0 | `ExitCode.Success` | コマンドが正常に完了したとき |
| 1 | `ExitCode.UsageError` | 引数が無効または使い方が間違っているとき |
| 2 | `ExitCode.RuntimeError` | 実行時にエラーが発生したとき |

```csharp
public async Task<ExitCode> ExecuteAsync(
    CommandContext context,
    CancellationToken ct)
{
    if (context.Arguments.Count == 0)
    {
        await context.Stderr.WriteLineAsync(
            "Error: 必須引数が不足しています", ct);
        return ExitCode.UsageError;
    }

    try
    {
        // ... 処理の実行
        return ExitCode.Success;
    }
    catch (Exception ex)
    {
        await context.Stderr.WriteLineAsync(
            $"Error: {ex.Message}", ct);
        return ExitCode.RuntimeError;
    }
}
```

## CommandContext のプロパティ

| プロパティ | 型 | 説明 |
|----------|------|-------------|
| `Stdin` | `IAsyncTextReader` | 入力ストリーム |
| `Stdout` | `IAsyncTextWriter` | 出力ストリーム |
| `Stderr` | `IAsyncTextWriter` | エラー出力ストリーム |
| `Arguments` | `IReadOnlyList<string>` | 位置引数 |
| `WorkingDirectory` | `string` | 現在の作業ディレクトリ |
| `HomeDirectory` | `string` | ホームディレクトリ |
| `Terminal` | `Terminal` | Terminal インスタンス |

## ベストプラクティス

1. **async/await を適切に使う** - メインスレッドをブロックしない
2. **キャンセルをサポートする** - `ct.IsCancellationRequested` をチェックする
3. **エラーは Stderr に書き出す** - Stdout はデータ出力用に残す
4. **正しい終了コードを返す** - 適切なパイプライン処理のため
5. **補完を実装する** - ユーザーエクスペリエンスの向上のため
6. **コマンドの責務を絞る** - 1つのコマンドに1つの目的
