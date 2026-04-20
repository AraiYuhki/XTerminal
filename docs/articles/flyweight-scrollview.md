# FlyweightScrollView

大量のデータを効率的に表示するための高パフォーマンスな仮想スクロールコンポーネントです。

## 概要

FlyweightScrollViewは**フライウェイトパターン**を使用しており、表示されているアイテムのみをレンダリングします。そのため、以下の用途に最適です。

- 数千行に及ぶターミナル出力
- ログビューアー
- 大規模なリストやテーブル
- 多数のアイテムを持つあらゆるスクロール可能なコンテンツ

## 特徴

- **仮想スクロール** - 表示されているアイテムのみをレンダリングします。
- **効率的なメモリ使用** - UI要素を再利用します。
- **CircularBuffer** - 古いエントリを自動的に削除する固定サイズバッファです。
- **垂直・水平サポート** - 両方のスクロール方向をサポートしています。
- **データバインディング** - ObservableCollection と連携して動作します。

## コンポーネント

### FlyweightScrollView

メインのスクロールビューコンポーネントです。

| コンポーネント | 説明 |
|-----------|-------------|
| `FlyweightVerticalScrollView` | 垂直スクロール |
| `FlyweightHorizontalScrollView` | 水平スクロール |

### CircularBuffer

いっぱいになると古いエントリを自動的に削除する固定サイズバッファです。

```csharp
using Xeon.Common.FlyweightScrollView.Model;

// 最大1000アイテムのバッファを作成
var buffer = new CircularBuffer<string>(1000);

// アイテムの追加
buffer.Add("Line 1");
buffer.Add("Line 2");

// いっぱいになると、古いアイテムから自動的に削除されます
for (int i = 0; i < 2000; i++)
{
    buffer.Add($"Line {i}");  // 最後の1000個だけが残ります
}

// アイテムへのアクセス
string first = buffer[0];
int count = buffer.Count;  // 最大 1000
```

## セットアップ

### 1. アイテムビューの作成

`FlyweightScrollViewItemBase` を継承したスクリプトを作成します。

```csharp
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.FlyweightScrollView;

public class LogItemView : FlyweightScrollViewItemBase<string>
{
    [SerializeField] private Text _text;

    public override void Bind(string data)
    {
        _text.text = data;
    }

    public override void Unbind()
    {
        _text.text = string.Empty;
    }
}
```

### 2. アイテムプレハブの作成

1. UI要素（例：Textを持つPanel）を作成します。
2. アイテムビュースクリプトを追加します。
3. RectTransformのサイズを設定します（これがアイテムの高さ/幅になります）。
4. プレハブとして保存します。

### 3. ScrollView のセットアップ

```csharp
using UnityEngine;
using Xeon.Common.FlyweightScrollView;
using Xeon.Common.FlyweightScrollView.Model;

public class TerminalDisplay : MonoBehaviour
{
    [SerializeField] private FlyweightVerticalScrollView _scrollView;
    [SerializeField] private LogItemView _itemPrefab;

    private CircularBuffer<string> _logBuffer;

    void Start()
    {
        // バッファの作成 (最大1000行)
        _logBuffer = new CircularBuffer<string>(1000);

        // スクロールビューの初期化
        _scrollView.Initialize<string, LogItemView>(_itemPrefab, _logBuffer);
    }

    public void AddLog(string message)
    {
        _logBuffer.Add(message);

        // 下端までスクロール (オプション)
        _scrollView.ScrollToEnd();
    }
}
```

## Terminal との統合

### 基本的なターミナル表示

```csharp
using UnityEngine;
using Xeon.XTerminal;
using Xeon.Common.FlyweightScrollView;
using Xeon.Common.FlyweightScrollView.Model;
using System.Threading;

public class TerminalUI : MonoBehaviour
{
    [SerializeField] private FlyweightVerticalScrollView _scrollView;
    [SerializeField] private LogItemView _itemPrefab;
    [SerializeField] private InputField _inputField;

    private Terminal _terminal;
    private CircularBuffer<string> _outputBuffer;

    void Start()
    {
        _outputBuffer = new CircularBuffer<string>(1000);
        _scrollView.Initialize<string, LogItemView>(_itemPrefab, _outputBuffer);

        _terminal = new Terminal(
            Application.dataPath,
            Application.dataPath,
            true
        );

        _inputField.onEndEdit.AddListener(OnCommandSubmit);
    }

    async void OnCommandSubmit(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        _inputField.text = "";
        _outputBuffer.Add($"> {command}");

        var stdout = new ListTextWriter();
        var stderr = new ListTextWriter();

        await _terminal.ExecuteAsync(
            command,
            stdout,
            stderr,
            destroyCancellationToken
        );

        // 出力行を追加
        foreach (var line in stdout.Lines)
        {
            _outputBuffer.Add(line);
        }

        // エラー行を追加
        foreach (var line in stderr.Lines)
        {
            _outputBuffer.Add($"[Error] {line}");
        }

        _scrollView.ScrollToEnd();
        _inputField.ActivateInputField();
    }
}
```

### UniTask を使用する場合

```csharp
#if UNI_TERMINAL_UNI_TASK_SUPPORT
using Cysharp.Threading.Tasks;

async UniTaskVoid ExecuteCommand(string command)
{
    var stdout = new UniTaskListTextWriter();
    var stderr = new UniTaskListTextWriter();

    await _terminal.ExecuteUniTaskAsync(command, stdout, stderr);

    foreach (var line in stdout.Lines)
    {
        _outputBuffer.Add(line);
    }

    _scrollView.ScrollToEnd();
}
#endif
```

## API リファレンス

### FlyweightScrollViewBase

| メソッド | 説明 |
|--------|-------------|
| `Initialize<TData, TView>(prefab, collection)` | データソースを使用して初期化 |
| `ScrollToEnd()` | 最後のアイテムまでスクロール |
| `ScrollToStart()` | 最初のアイテムまでスクロール |
| `ScrollToIndex(int index)` | 指定したインデックスまでスクロール |
| `Refresh()` | 表示されているアイテムを強制リフレッシュ |

### CircularBuffer<T>

| プロパティ/メソッド | 説明 |
|-----------------|-------------|
| `Capacity` | 最大アイテム数 |
| `Count` | 現在のアイテム数 |
| `Add(T item)` | アイテムを追加 (いっぱいの場合、最も古いものを削除) |
| `Clear()` | すべてのアイテムを削除 |
| `this[int index]` | 指定インデックスのアイテムを取得 |

### FlyweightScrollViewItemBase<T>

| メソッド | 説明 |
|--------|-------------|
| `Bind(T data)` | アイテムが表示されたときに呼び出されます |
| `Unbind()` | アイテムが非表示になったときに呼び出されます |

## パフォーマンスのヒント

1. **適切なバッファサイズを設定する** - メモリと履歴の長さのバランスを考えます。
2. **アイテムビューをシンプルに保つ** - アイテムあたりのコンポーネント数を最小限にします。
3. **オブジェクトプールを使用する** - FlyweightScrollView はこれを自動的に処理します。
4. **追加をバッチ化する** - 複数のアイテムを追加してから、Refresh を一度だけ呼び出します。

```csharp
// 良い例: まとめて追加
foreach (var line in lines)
{
    _buffer.Add(line);
}
_scrollView.ScrollToEnd();  // 1回のリフレッシュ

// 避けるべき例: 追加のたびにリフレッシュ
foreach (var line in lines)
{
    _buffer.Add(line);
    _scrollView.ScrollToEnd();  // 複数回のリフレッシュが発生
}
```

## プレハブ

XTerminal には、すぐに使用できるプレハブが含まれています。

- `FlyweightVerticalScrollView.prefab`
- `FlyweightHorizontalScrollView.prefab`

場所: `Packages/jp.xeon.x-terminal/Runtime/Prefabs/`
