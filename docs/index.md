# XTerminal

Unity向けのLinuxライクな動作を持つ文字列ベースのCLI実行フレームワークです。

このドキュメントでは、組み込みコマンドの詳細と、Unityプロジェクトでの `Terminal` クラスの使用方法について説明します。

## 特徴

- **Linuxライクな構文** - パイプ (`|`)、リダイレクト (`>`, `>>`, `<`)
- **組み込みコマンド** - ファイル操作、テキスト処理、Unity固有のコマンド
- **拡張性** - カスタムコマンドを簡単に追加可能
- **非同期サポート** - async/await および UniTask との統合
- **タブ補完** - コンテキストに応じたコマンド補完

## クイックリンク

- [はじめに](articles/getting-started.md)
- [組み込みコマンド](articles/commands/index.md)
- [日本語ドキュメント](ja/index.md)

## インストール

### Package Manager経由 (Git URL)

```
https://github.com/AraiYuhki/XTerminal.git?path=Packages/jp.xeon.x-terminal
```

### Unity Asset Store経由

Asset Store ウィンドウで "XTerminal" を検索してください。

## 基本的な使い方

```csharp
using Xeon.XTerminal;

var terminal = new Terminal(
    workingDirectory: Application.dataPath,
    homeDirectory: Application.dataPath,
    registerBuiltInCommands: true
);

var stdout = new StringWriter();
var stderr = new StringWriter();

await terminal.ExecuteAsync("echo Hello, World!", stdout, stderr, ct);
```

## ライセンス

MITライセンス - [LICENSE](https://github.com/AraiYuhki/XTerminal/blob/main/Packages/jp.xeon.x-terminal/LICENSE.md) を参照してください。
