# 組み込みコマンド

XTerminalは、カテゴリ別に整理された包括的な組み込みコマンドセットを提供します。

## コマンドカテゴリ

| カテゴリ | コマンド | 説明 |
|----------|----------|-------------|
| [ファイル操作](file-operations.md) | `pwd`, `cd`, `ls`, `cat`, `find`, `less`, `diff` | ファイルやディレクトリの移動・操作 |
| [テキスト処理](text-processing.md) | `echo`, `grep` | テキストの処理とフィルタリング |
| [ユーティリティ](#utilities) | `help`, `history` | 一般的なユーティリティ |
| [Unityコマンド](unity-commands.md) | `hierarchy`, `go`, `transform`, `component`, `property` | Unity固有の操作 |

## ユーティリティ

### help

コマンドのヘルプ情報を表示します。

```bash
# すべてのコマンドを一覧表示
help

# 特定のコマンドのヘルプを表示
help ls
help hierarchy
```

### history

コマンド履歴を管理します。

```bash
# コマンド履歴を表示
history

# 最後の N 件を表示
history -n 10

# 履歴をクリア
history -c

# 指定したインデックスのエントリを削除
history -d 5

# ファイルから履歴を読み込む
history -r ~/.terminal_history
```

**オプション:**

| オプション | 説明 |
|--------|-------------|
| `-c` | 履歴をクリアする |
| `-d <index>` | 指定したインデックスのエントリを削除する |
| `-n <count>` | 最後の N 件を表示する |
| `-r <file>` | ファイルから履歴を読み込む |

## 共通パターン

### コマンドの組み合わせ

```bash
# GameObjectを検索してフィルタリング
hierarchy -r | grep Enemy

# ファイルを一覧表示してフィルタリング
ls -la | grep .cs

# ヒエラルキーをファイルにエクスポート
hierarchy -r > hierarchy_dump.txt
```

### Unityオブジェクトの操作

```bash
# すべての Rigidbody オブジェクトを検索
hierarchy -c Rigidbody

# Transform 情報を取得
transform /Player -p

# コンポーネントのプロパティを変更
property set /Player Rigidbody mass 10
```
