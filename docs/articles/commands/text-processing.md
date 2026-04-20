# テキスト処理

テキスト出力の処理とフィルタリングを行うためのコマンドです。

## echo

テキストを標準出力に出力します。

### 書式

```bash
echo [-n] [string...]
```

### 説明

指定された文字列をスペースで区切って標準出力に出力し、（`-n` が指定されていない限り）末尾に改行を追加します。

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-n` | `--newline` | 末尾の改行を出力しない |

### 引数

| 引数 | 説明 |
|----------|-------------|
| `string` | 出力する1つ以上の文字列。複数の文字列はスペースで結合されます。 |

### 使用例

```bash
# シンプルな出力
echo Hello, World!
# 出力: Hello, World!

# 複数の引数
echo Hello World from XTerminal
# 出力: Hello World from XTerminal

# 末尾の改行なしで出力
echo -n "No newline here"

# 内容を含むファイルを作成
echo "Configuration data" > config.txt

# ファイルに追記
echo "Additional line" >> log.txt

# パイプラインで使用
echo "Hello" | grep -p "ell"
# 出力: Hello
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 常に成功します |

---

## grep

パターンに一致する行を検索します。

### 書式

```bash
grep -p <pattern> [-i] [-v] [-c]
```

### 説明

入力行をフィルタリングし、指定された正規表現パターンに一致する行（`-v` の場合は一致しない行）のみを出力します。標準入力から読み取ります。

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-p` | `--pattern` | **必須。** 検索する正規表現パターン |
| `-i` | `--ignorecase` | パターンマッチング時に大文字小文字を区別しない |
| `-v` | `--invert` | 一致を反転し、一致しない行を選択する |
| `-c` | `--count` | 一致した行数のみを表示する |

### パターンの構文

grep は .NET 正規表現を使用します。一般的なパターンは以下の通りです。

| パターン | 説明 |
|---------|-------------|
| `.` | 任意の1文字に一致 |
| `*` | 直前の要素の0回以上の繰り返しに一致 |
| `+` | 直前の要素の1回以上の繰り返しに一致 |
| `?` | 直前の要素の0回または1回に一致 |
| `^` | 行の先頭に一致 |
| `$` | 行の末尾に一致 |
| `[abc]` | セット内の任意の文字に一致 |
| `[^abc]` | セットに含まれない任意の文字に一致 |
| `\d` | 任意の数字に一致 |
| `\w` | 任意の単語構成文字に一致 |
| `\s` | 任意の空白文字に一致 |
| `(a\|b)` | a または b に一致 |

### 使用例

```bash
# 基本的なパターン検索
cat file.txt | grep -p "error"

# 大文字小文字を区別せずに検索
cat log.txt | grep -p "warning" -i

# 一致を反転（特定の行を除外）
ls -la | grep -p "\.meta" -v

# 一致した行数のみカウント
hierarchy -r | grep -p "Enemy" -c
# 出力: 15

# 正規表現パターン
cat code.cs | grep -p "public class \w+"
cat log.txt | grep -p "^\[ERROR\]"
cat data.txt | grep -p "id: \d+"

# 特定のテキストで始まる行を検索
cat file.txt | grep -p "^TODO"

# 特定のテキストで終わる行を検索
cat file.txt | grep -p "\.cs$"

# OR パターンを使用
cat log.txt | grep -p "(error|warning|critical)" -i
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 1行以上一致した |
| 2 | 一致する行がなかった |
| 1 | 無効なパターンまたはその他のエラー |

---

## パイプラインの使用例

### テキストファイルの操作

```bash
# ログファイルからエラーを検索
cat application.log | grep -p "Exception"

# コード内の TODO コメントを検索
cat script.cs | grep -p "// TODO"

# 数字を含む行を抽出
cat data.txt | grep -p "\d+"

# 空行を検索
cat file.txt | grep -p "^$"

# 空行以外を検索
cat file.txt | grep -p "^$" -v
```

### Unity ヒエラルキーのフィルタリング

```bash
# 名前に "Player" を含むすべてのオブジェクトを検索
hierarchy -r | grep -p "Player"

# パターンでオブジェクトを検索
hierarchy -r | grep -p "Enemy_\d+"

# "Untagged" 以外のタグを持つオブジェクトを検索
hierarchy -r -l | grep -p "Untagged" -v

# シーン内の敵の数をカウント
hierarchy -r | grep -p "Enemy" -c
```

### ファイルリストのフィルタリング

```bash
# C# ファイルを検索
ls -R | grep -p "\.cs$"

# meta ファイルを除外
ls -la | grep -p "\.meta$" -v

# 特定の接頭辞を持つファイルを検索
ls | grep -p "^Player"

# サイズパターンでファイルを検索
ls -lh | grep -p "MB"
```

### 複数のフィルタのチェイン

```bash
# アクティブなプレイヤーを検索
hierarchy -r -l | grep -p "Player" | grep -p "Active"

# 警告を除外してエラーメッセージを検索
cat log.txt | grep -p "error" -i | grep -p "warning" -v

# 複雑なフィルタリング
ls -la | grep -p "\.cs$" | grep -p "Test" -v
```

### 他のコマンドとの組み合わせ

```bash
# ファイルを検索して内容を検索
find -n "*.cs" | cat | grep -p "class"

# 特定のアイテムをカウント
hierarchy -c Rigidbody | grep -p "/" -c

# 検索結果を保存
hierarchy -r | grep -p "Enemy" > enemies.txt

# 多段階フィルタリング
cat config.json | grep -p "\"enabled\"" | grep -p "true" -c
```

### 実用的なユースケース

```bash
# スクリプトが欠落しているすべての GameObject を検索
component list /Root -v | grep -p "Missing"

# 特定のコンポーネントを持つオブジェクトを検索
hierarchy -r -l | grep -p "Rigidbody"

# デバッグ出力のフィルタリング
property list /Player Rigidbody | grep -p "mass\|drag\|velocity"

# ヒエラルキーパス内のパターン検索
hierarchy -r | grep -p "UI/.*Button"
```
