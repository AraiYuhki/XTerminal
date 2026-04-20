# XTerminal ドキュメント

<p align="center">
  <img src="Images/key-image.jpg" alt="XTerminal キーイメージ" width="800">
</p>

XTerminalは、Unity向けのLinuxライクな動作を持つ文字列ベースのCLI実行フレームワークです。パイプライン、リダイレクト、拡張可能なカスタムコマンドをサポートしており、コマンドの実行が可能です。

## 目次

- [インストール](#インストール)
- [クイックスタート](#クイックスタート)
- [組み込みコマンド](#組み込みコマンド)
- [カスタムコマンドの作成](#カスタムコマンドの作成)
- [UniTaskサポート](#unitaskサポート)
- [FlyweightScrollView](#flyweightscrollview)
- [APIリファレンス](#apiリファレンス)

## インストール

### Package Manager経由

1. Window > Package Manager を開く
2. 「+」ボタン > 「Add package from git URL...」を選択
3. 以下のURLを入力:

```
https://github.com/AraiYuhki/XTerminal.git?path=Packages/jp.xeon.x-terminal
```

### manifest.json経由

`Packages/manifest.json` に以下を追加:

```json
{
  "dependencies": {
    "jp.xeon.x-terminal": "https://github.com/AraiYuhki/XTerminal.git?path=Packages/jp.xeon.x-terminal"
  }
}
```

## クイックスタート

### Terminalの初期化

```csharp
using Xeon.XTerminal;

var terminal = new Terminal(
    workingDirectory: Application.dataPath,
    homeDirectory: Application.dataPath,
    registerBuiltInCommands: true
);
```

### コマンドの実行

```csharp
using System.IO;

var stdout = new StringWriter();
var stderr = new StringWriter();

// コマンドを実行
var exitCode = await terminal.ExecuteAsync("echo Hello, World!", stdout, stderr, ct);

// 出力を取得
Debug.Log(stdout.ToString());  // "Hello, World!"
```

### パイプラインの使用

```csharp
// パイプでコマンドをつなげる
await terminal.ExecuteAsync("cat myfile.txt | grep --pattern=error | less", stdout, stderr, ct);
```

### リダイレクトの使用

```csharp
// ファイルへの出力
await terminal.ExecuteAsync("echo Hello > output.txt", stdout, stderr, ct);

// ファイルへの追記
await terminal.ExecuteAsync("echo World >> output.txt", stdout, stderr, ct);

// ファイルからの入力
await terminal.ExecuteAsync("grep --pattern=pattern < input.txt", stdout, stderr, ct);
```

## 組み込みコマンド

### ファイル操作

| コマンド | 説明 | オプション |
|---------|-------------|---------|
| `pwd` | 現在の作業ディレクトリを表示 | `-L`, `-P` |
| `cd` | ディレクトリを変更 | `-L`, `-P` |
| `ls` | ディレクトリの内容を一覧表示 | `-a`, `-l`, `-h`, `-r`, `-R`, `-S` |
| `cat` | ファイルの内容を表示 | - |
| `find` | ファイルを検索 | `-n`, `-i`, `-t`, `-d` |
| `less` | ファイルをページごとに表示 | `-n`, `-f`, `-N`, `-S` |
| `diff` | ファイルを比較 | `-u`, `-i`, `-b`, `-w`, `-q` |

### テキスト処理

| コマンド | 説明 | オプション |
|---------|-------------|---------|
| `echo` | テキストを出力 | `-n` |
| `grep` | パターンマッチング検索 | `--pattern`, `-i`, `-v`, `-c` |

### ユーティリティ

| コマンド | 説明 | オプション |
|---------|-------------|---------|
| `help` | ヘルプを表示 | - |
| `history` | コマンド履歴を表示 | `-c`, `-d`, `-n`, `-r` |
| `pbcopy` | クリップボードにコピー | - |

### Unity固有コマンド

| コマンド | 説明 | オプション |
|---------|-------------|---------|
| `hierarchy` | シーン階層を表示 | `-r`, `-d`, `-a`, `-l`, `-s`, `-n`, `-c`, `-t`, `-y` |
| `go` | GameObjectの操作 | `--primitive`, `-P`, `-t`, `-n`, `-c`, `-i`, `-s` |
| `transform` | Transformの操作 | `set`, `add`, `sub` サブコマンド; `-p`, `-P`, `-r`, `-R`, `-s`, `--parent`, `-w` |
| `component` | コンポーネント管理 | `-a`, `-v`, `-i`, `-n` |
| `property` | プロパティ操作 | `list`, `get`, `set`, `add`, `sub`, `mul`, `div` サブコマンド; `-a`, `-s`, `-n` |

### コマンドの使用例

#### hierarchy - シーン階層

```bash
# ルートオブジェクトを表示
hierarchy

# 再帰的に表示
hierarchy -r

# 詳細情報を表示
hierarchy -l

# 名前でフィルタリング（ワイルドカード対応）
hierarchy -n "Player*"

# コンポーネントでフィルタリング
hierarchy -c Rigidbody

# タグでフィルタリング
hierarchy -t Player
```

#### go - GameObject操作

```bash
# 新しいGameObjectを作成
go create MyObject

# プリミティブを作成
go create Cube --primitive=Cube

# 削除
go delete /MyObject

# 名前、タグ、コンポーネントで検索
go find -n "Enemy*"
go find -t Player
go find -c Rigidbody

# 複製
go clone /Original -n Clone --count 5

# アクティブ状態の切り替え
go active /MyObject --toggle
```

#### transform - Transform操作

```bash
# Transform情報を表示
transform /MyObject

# 位置を設定 (ワールド) - 後方互換性のために両方の構文が動作します
transform /MyObject -p 1,2,3
transform set /MyObject -p 1,2,3

# 位置を設定 (ローカル)
transform /MyObject -P 0,1,0

# 回転を設定
transform /MyObject -r 0,90,0

# スケールを設定
transform /MyObject -s 2,2,2

# 親を設定
transform /Child --parent /Parent

# 位置を加算 (インクリメント)
transform add /MyObject -p 1,0,0      # X軸方向に+1移動
transform add /MyObject -r 0,45,0     # Y軸を中心に+45度回転
transform add /MyObject -s 0.5        # スケールを0.5増加

# 位置を減算 (デクリメント)
transform sub /MyObject -p 0,1,0      # Y軸方向に-1移動
transform sub /MyObject -r 0,90,0     # Y軸を中心に-90度回転
```

#### component - コンポーネント管理

```bash
# コンポーネントを一覧表示
component list /MyObject

# コンポーネントを追加
component add /MyObject Rigidbody

# コンポーネントを削除
component remove /MyObject Rigidbody

# 有効化/無効化
component enable /MyObject BoxCollider
component disable /MyObject BoxCollider
```

#### property - プロパティ操作

```bash
# プロパティを一覧表示
property list /MyObject Rigidbody

# プロパティ値を取得
property get /MyObject Rigidbody mass

# プロパティ値を設定
property set /MyObject Rigidbody mass 10
property set /MyObject Transform position 1,2,3

# 算術演算 (add, sub, mul, div)
property add /MyObject Rigidbody mass 5       # massに5を加算
property sub /MyObject Rigidbody drag 0.1     # dragから0.1を減算
property mul /MyObject Rigidbody mass 2       # massを2倍にする
property div /MyObject Rigidbody mass 2       # massを2で割る

# 算術演算がサポートされている型:
# - 数値型: int, float, double, long, byte, short (add, sub, mul, div)
# - Vector型: Vector2, Vector3, Vector4, Vector2Int, Vector3Int (add, sub のみ)

# Vector演算の例
property add /MyObject Transform localScale 1,1,1    # スケールを増加
property sub /MyObject Transform position 0,1,0      # 下に1移動
```

## カスタムコマンドの作成

### 基本的なコマンド

```csharp
using Xeon.XTerminal;
using System.Threading;
using System.Threading.Tasks;

[Command("mycommand", "マイカスタムコマンド")]
public class MyCommand : ICommand
{
    [Option("message", "m", Description = "表示するメッセージ")]
    public string Message;

    [Option("count", "c", Description = "繰り返す回数")]
    public int Count = 1;

    public string CommandName => "mycommand";
    public string Description => "マイカスタムコマンド";

    public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
    {
        for (int i = 0; i < Count; i++)
        {
            await context.Stdout.WriteLineAsync(Message ?? "Hello!", ct);
        }
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

### コマンドの登録

```csharp
// 手動登録
terminal.Registry.Register<MyCommand>();

// アセンブリから自動登録
terminal.Registry.RegisterFromAssembly(typeof(MyCommand).Assembly);
```

## UniTaskサポート

プロジェクトにUniTaskがインストールされている場合、UniTaskサポートが自動的に有効になります。

### UniTaskコマンドの使用

```csharp
using Cysharp.Threading.Tasks;
using Xeon.XTerminal;

// UniTaskで実行
var exitCode = await terminal.ExecuteUniTaskAsync("echo Hello!", stdout, stderr);
```

### UniTaskコマンドの作成

```csharp
[Command("myasync", "UniTaskベースの非同期コマンド")]
public class MyUniTaskCommand : IUniTaskCommand
{
    public string CommandName => "myasync";
    public string Description => "UniTaskベースの非同期コマンド";

    public async UniTask<ExitCode> ExecuteAsync(UniTaskCommandContext context, CancellationToken ct)
    {
        await context.Stdout.WriteLineAsync("処理中...", ct);
        await UniTask.Delay(1000, cancellationToken: ct);
        await context.Stdout.WriteLineAsync("完了!", ct);
        return ExitCode.Success;
    }

    public IEnumerable<string> GetCompletions(CompletionContext context)
    {
        yield break;
    }
}
```

## FlyweightScrollView

大量のデータを効率的に表示するための仮想スクロールコンポーネントです。

### 特徴

- 大量データの効率的な表示（数万行にも対応）
- 垂直および水平スクロールのサポート
- 固定サイズのログバッファリング用 CircularBuffer
- ObservableCollection との統合

### 使い方

```csharp
using Xeon.Common.FlyweightScrollView;
using Xeon.Common.FlyweightScrollView.Model;

// バッファの作成 (最大1000行)
var logBuffer = new CircularBuffer<string>(1000);

// スクロールビューへのバインド
scrollView.Initialize<string, LogItemView>(logItemPrefab, logBuffer);

// ログの追加 (バッファがいっぱいになると古いエントリから自動削除)
logBuffer.Add("新しいログエントリ");
```

## APIリファレンス

### Terminal クラス

| メソッド | 説明 |
|--------|-------------|
| `ExecuteAsync(command, stdout, stderr, ct)` | コマンドを非同期で実行 |
| `ExecuteUniTaskAsync(command, stdout, stderr)` | UniTaskを使用して実行 (UniTaskが必要) |

### 終了コード

| コード | 定数 | 説明 |
|------|----------|-------------|
| 0 | `ExitCode.Success` | コマンド成功 |
| 1 | `ExitCode.UsageError` | 使用方法エラー |
| 2 | `ExitCode.RuntimeError` | 実行時エラー |

### 属性

| 属性 | 説明 |
|-----------|-------------|
| `[Command(name, description)]` | クラスをコマンドとしてマーク |
| `[Option(name, shortName)]` | フィールドをコマンドオプションとしてマーク |

## 動作要件

- Unity 6000.0 以降
- (オプション) UniTask 2.0 以降

## ライセンス

MITライセンス - 詳細は [LICENSE.md](../LICENSE.md) を参照してください。
