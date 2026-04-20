# パイプラインとリダイレクト

XTerminalは、強力なコマンド連携を可能にするLinuxライクなパイプラインとリダイレクトをサポートしています。

## パイプライン

パイプ演算子 (`|`) を使用してコマンドをつなげます。あるコマンドの出力が次のコマンドの入力になります。

### 基本構文

```bash
command1 | command2 | command3
```

### 例

```bash
# オブジェクトを検索してフィルタリング
hierarchy -r | grep -p "Player"

# 複数のフィルタをつなげる
hierarchy -r -l | grep -p "Enemy" | grep -p "Active: True"

# 結果をカウント
hierarchy -r | grep -p "Collider" | count

# ファイル内容を処理
cat config.json | grep -p "setting"
```

### 動作の仕組み

```
┌─────────┐    stdout    ┌─────────┐    stdout    ┌─────────┐
│ Command1 │────────────▶│ Command2 │────────────▶│ Command3 │
└─────────┘              └─────────┘              └─────────┘
                               ▲                        │
                            stdin                    stdout
                                                       ▼
                                                    [出力]
```

1. Command1 が stdout に書き込みます
2. Command2 が stdin (Command1 の出力) から読み取ります
3. Command3 が stdin (Command2 の出力) から読み取ります
4. 最終的な出力がターミナルに表示されます

## リダイレクト

### 出力リダイレクト (`>`)

コマンドの出力をファイルに書き込みます。既存の内容は**上書き**されます。

```bash
# シーン階層をファイルに保存
hierarchy -r > hierarchy.txt

# フィルタリング結果を保存
hierarchy -r | grep -p "Player" > players.txt

# 設定をエクスポート
property list /Settings GameConfig > config_dump.txt
```

### 追記リダイレクト (`>>`)

コマンドの出力をファイルに追記します。既存の内容は**保持**されます。

```bash
# ログファイルに追加
echo "Session started" >> session.log

# シーン階層のスナップショットを追記
hierarchy -r >> snapshots.txt

# レポートを作成
echo "=== Players ===" >> report.txt
hierarchy -n "Player*" >> report.txt
echo "=== Enemies ===" >> report.txt
hierarchy -n "Enemy*" >> report.txt
```

### 入力リダイレクト (`<`)

ファイルの内容をコマンドの入力として読み取ります。

```bash
# ファイル内を検索
grep -p "error" < log.txt

# ファイル内容を処理
count < data.txt

# ファイルの単語数をカウント
count -w < document.txt
```

## 組み合わせた使用例

### パイプライン + 出力リダイレクト

```bash
# フィルタリングして保存
hierarchy -r | grep -p "UI" > ui_objects.txt

# 処理して保存
cat data.txt | grep -p "important" | count > result.txt
```

### 入力リダイレクト + パイプライン

```bash
# ファイルを読み取ってフィルタリング
grep -p "error" < log.txt | count

# パイプラインを通じてファイルを処理
count -w < document.txt
```

### 複雑なチェイン

```bash
# 全体分析パイプライン
hierarchy -r -l | grep -p "Enemy" | grep -v "Disabled" > active_enemies.txt

# 多段階処理
cat input.txt | grep -p "data" | count > analysis.txt
```

## 実用的な例

### シーン分析

```bash
# 全階層をエクスポート
hierarchy -r -l > scene_dump.txt

# タイプ別にオブジェクトをカウント
echo "Rigidbody count:" > physics_report.txt
hierarchy -c Rigidbody | count >> physics_report.txt
echo "Collider count:" >> physics_report.txt
hierarchy -c Collider | count >> physics_report.txt
```

### デバッグログ

```bash
# デバックスナップショットの作成
echo "=== Debug Snapshot $(date) ===" >> debug.log
hierarchy -r -l >> debug.log
echo "" >> debug.log
```

### 設定のエクスポート

```bash
# すべての設定をエクスポート
property list /GameManager Settings > settings.txt
property list /AudioManager AudioSettings >> settings.txt
property list /GraphicsManager GraphicsSettings >> settings.txt
```

### バッチ処理

```bash
# すべてのUI要素を検索して文書化
hierarchy -c "UnityEngine.UI.Image" > ui_images.txt
hierarchy -c "UnityEngine.UI.Text" > ui_texts.txt
hierarchy -c "UnityEngine.UI.Button" > ui_buttons.txt
```

## エラーハンドリング

### Stderr と Stdout

- **Stdout**: 通常の出力（パイプラインを流れます）
- **Stderr**: エラーメッセージ（直接表示されます）

```bash
# エラーはパイプを通りません
nonexistent_command | grep -p "test"
# Error: Command 'nonexistent_command' not found
# (grep には何も渡されません)
```

### パイプライン内の終了コード

パイプライン全体の終了コードは、最後のコマンドの終了コードになります。

```bash
# grep が何も見つけられなくても、終了コードは Success です
# (grep は何も出力しませんが、エラーにはなりません)
hierarchy | grep -p "NonExistent"
```

## ヒント

1. **フィルタリングにパイプラインを使う** - すべてをメモリにロードするのを避けます
2. **大きな出力はリダイレクトする** - 表示する代わりにファイルに保存します
3. **段階的にチェインをつなぐ** - 複雑なパイプラインを一歩ずつ構築します
4. **中間結果を確認する** - デバッグ時には最後のリダイレクトを外して確認します

### パイプラインのデバッグ

```bash
# 完全なパイプライン
hierarchy -r | grep -p "Player" | grep -p "Active" > result.txt

# ステップ 1 のデバッグ
hierarchy -r

# ステップ 2 のデバッグ
hierarchy -r | grep -p "Player"

# ステップ 3 のデバッグ
hierarchy -r | grep -p "Player" | grep -p "Active"

# リダイレクトを含めた最終形
hierarchy -r | grep -p "Player" | grep -p "Active" > result.txt
```
