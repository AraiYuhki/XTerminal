# ユーティリティ

ヘルプや履歴管理のための一般的なユーティリティコマンドです。

## help

コマンドのヘルプ情報を表示します。

### 書式

```bash
help [command]
```

### 説明

引数がない場合は、利用可能なすべてのコマンドとその簡潔な説明の一覧を表示します。コマンド名が指定された場合は、そのコマンドの詳細なヘルプ（使用法、オプション、使用例など）を表示します。

### 引数

| 引数 | 説明 |
|----------|-------------|
| `command` | オプション。ヘルプを表示するコマンドの名前。 |

### 使用例

```bash
# 利用可能なすべてのコマンドを一覧表示
help
```

**出力例:**
```
Available commands:
  cat        Concatenate and display file contents
  cd         Change working directory
  component  Manage GameObject components
  diff       Compare files line by line
  echo       Echo arguments to stdout
  find       Search for files in a directory hierarchy
  go         GameObject operations
  grep       Filter lines matching a pattern
  help       Display help for commands
  hierarchy  Display scene hierarchy
  history    Display or manage command history
  less       View file contents page by page
  ls         List directory contents
  property   Get or set component property values
  pwd        Print current working directory
  transform  Manipulate GameObject transforms

Use 'help <command>' for detailed information.
```

```bash
# 特定のコマンドのヘルプを表示
help ls
```

**出力例:**
```
ls - List directory contents

Usage: ls [options] [path...]

Options:
  -a, --all             Do not ignore entries starting with .
  -l, --long            Use a long listing format
  -h, --human-readable  Print sizes in human readable format
  -r, --reverse         Reverse order while sorting
  -R, --recursive       List subdirectories recursively
  -S, --sort            Sort by: name, size, time
```

```bash
# Unity コマンドのヘルプを表示
help hierarchy
help component
help property
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | 不明なコマンドが指定されました |
| 2 | レジストリが設定されていません |

---

## history

コマンド履歴を表示または管理します。

### 書式

```bash
history [-c] [-d position] [-n count] [-r]
```

### 説明

コマンド履歴を、行番号と共に一覧表示します。履歴のクリアや、特定のエントリの削除にも使用できます。

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-c` | `--clear` | すべての履歴エントリを消去します |
| `-d` | `--delete` | 指定された位置 (1ベース) の履歴エントリを削除します |
| `-n` | `--number` | 最後の N 件のエントリのみを表示します |
| `-r` | `--reverse` | 履歴を逆順（新しい順）で表示します |

### 使用例

```bash
# 履歴全体を表示
history
```

**出力例:**
```
    1  ls -la
    2  cd ~/Projects
    3  hierarchy -r
    4  go create Player
    5  component add /Player Rigidbody
    6  property set /Player Rigidbody mass 10
```

```bash
# 最後の5件を表示
history -n 5
```

**出力例:**
```
    2  cd ~/Projects
    3  hierarchy -r
    4  go create Player
    5  component add /Player Rigidbody
    6  property set /Player Rigidbody mass 10
```

```bash
# 履歴を逆順で表示
history -r
```

**出力例:**
```
    6  property set /Player Rigidbody mass 10
    5  component add /Player Rigidbody
    4  go create Player
    3  hierarchy -r
    2  cd ~/Projects
    1  ls -la
```

```bash
# 特定のエントリを削除
history -d 3

# すべての履歴を消去
history -c
```

### 履歴のフォーマット

各履歴エントリは以下のように表示されます：
- **行番号** (1ベース、5文字分で右揃え)
- **コマンド** (入力された通りのコマンド)

### 注意事項

- 履歴は Terminal インスタンスごとに保持されます。
- 履歴の消去や削除には、Terminal に履歴コールバックが設定されている必要があります。
- 現在実行中のコマンドは、通常、実行が完了するまで履歴には含まれません。

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 2 | 履歴操作がサポートされていないか、位置が範囲外です |

---

## 実用的な例

### 最近の作業の確認

```bash
# 最近実行したコマンドを確認
history -n 10

# 履歴から特定のコマンドを探す (grep との組み合わせ)
history | grep -p "component"
```

### 履歴の整理

```bash
# 間違えて入力したコマンドを削除
history -d 5

# 履歴をリセット
history -c
```

### コマンドヘルプの活用

```bash
# 利用可能なコマンドを調べる
help

# 特定のコマンドについて学ぶ
help transform

# Unity 固有のコマンドを確認
help hierarchy
help go
help component
help property
```

### クイックリファレンス

```bash
# よく使われるヘルプの検索
help ls          # ファイル一覧表示のオプション
help find        # ファイル検索のオプション
help grep        # パターンマッチングの構文
help diff        # ファイル比較のオプション
help hierarchy   # シーンヒエラルキーの表示
help go          # GameObject 操作
help transform   # Transform 操作
help component   # コンポーネント管理
help property    # リフレクションを介したプロパティアクセス
```
