# 変数対応 実装計画書

## 概要

UniTerminalに「シェル変数（ユーザー定義変数）」を導入し、入力文字列内の変数展開と、変数の設定・削除・一覧表示を提供します。既存のパーサー／バインダー／実行パイプラインに大きな影響を与えず、軽量な前処理と新規コマンドの追加で対応する方針です。

## 目的

- コマンドライン内で `$NAME` や `${NAME}` を使用して変数値を展開できるようにする。
- ユーザーが `set`/`unset`/`env` コマンドで変数を管理できるようにする。
- 既存の引用符（`'`/`"`）やエスケープの挙動と整合する実装にする。

## 非目的

- OS環境変数との双方向同期（必要になった場合は将来拡張で対応）。
- Bash互換の複雑な展開（配列、パラメータ展開修飾子、コマンド置換など）。
- 変数によるコマンド名置換のセキュリティ強化（今回は最小限の機能提供に留める）。

## 仕様（初期スコープ）

### 変数の定義

- 変数名は `[A-Za-z_][A-Za-z0-9_]*` を許可。
- 変数の値は任意の文字列（空文字含む）。
- 未定義の変数は空文字に展開する。

### 変数展開

- 展開記法: `$NAME`、`${NAME}`。
- `\$` はリテラル `$`。
- 単一引用符 `'...'` 内は展開しない。
- 二重引用符 `"..."` 内は展開する。
- 入力内で「未クォート／二重クォート／単一クォート」が混在するトークンは、該当セグメントごとに展開ルールを適用する。

### 変数管理コマンド

- `set NAME=VALUE` : 変数の作成・更新。
- `unset NAME` : 変数の削除。
- `env` : 変数一覧を `NAME=VALUE` 形式で表示（ソート順は名称の昇順）。

## 実装方針

### 1. 変数ストアの導入

- `VariableStore`（仮称）を `Core/` に新規追加。
  - `Get(string name)`, `Set(string name, string value)`, `Unset(string name)`, `Enumerate()` を提供。
  - 変数名のバリデーションを集中管理し、エラー時は `RuntimeException` を使用。
- `Terminal` が `VariableStore` を保持し、`PipelineExecutor` へ渡す。
- `CommandContext` に `Variables` を追加し、各コマンドが参照できるようにする。

### 2. 変数展開の実装

- トークナイズ後に `VariableExpander` を走らせる方式を採用。
  - `TokenKind.Word` の `Value` を展開対象とする。
  - `Token` に「クォート種別」の情報を持たせるのではなく、トークナイズ時に「展開可能かどうか」のセグメント情報を保持する新しい構造を用意する（例: `WordTokenSegment` の配列）。
- 展開アルゴリズムは `StringBuilder` ベースで実装し、GCを抑える。
- 変数未定義時は空文字に置換し、エラーにはしない。

### 3. コマンド追加

- `BuiltInCommands/SetCommand.cs`
  - `set NAME=VALUE` の形式を解析。
  - `=` が含まれない場合は `UsageError`。
- `BuiltInCommands/UnsetCommand.cs`
  - 1つ以上の変数名を受け取り削除。
- `BuiltInCommands/EnvCommand.cs`
  - `VariableStore` の内容を昇順表示。
- `Terminal.RegisterBuiltInCommands()` に上記3コマンドを追加。

### 4. 補完

- `CompletionEngine` に `$` から始まるトークンの場合の変数補完を追加。
- 変数候補は `VariableStore.Enumerate()` から取得。

## 変更対象（予定）

- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Core/VariableStore.cs`（新規）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Core/CommandContext.cs`（Variables追加）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Parsing/Tokenizer.cs`（セグメント情報を保持できるよう拡張）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Parsing/Token.cs`（Wordトークンにセグメント情報を追加）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Parsing/Parser.cs`（展開を適用する流れを追加）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Execution/PipelineExecutor.cs`（変数ストアの受け渡し）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/BuiltInCommands/SetCommand.cs`（新規）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/BuiltInCommands/UnsetCommand.cs`（新規）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/BuiltInCommands/EnvCommand.cs`（新規）
- `Packages/jp.xeon.uni-terminal/Runtime/Scripts/Completion/CompletionEngine.cs`（変数補完）

## 実装順序

1. `VariableStore` と `CommandContext` の拡張
2. `PipelineExecutor` から `CommandContext` への変数受け渡し
3. `Tokenizer` / `Token` の拡張（セグメント情報保持）
4. `VariableExpander` 実装と `Parser` への組み込み
5. `set`/`unset`/`env` コマンドの追加と登録
6. 補完機能の拡張
7. ドキュメント・テスト追加

## テスト方針

### 解析・展開テスト（Editor）

- `$NAME` が未定義の場合は空文字になる。
- `'${NAME}'` は展開されない。
- `"${NAME}"` は展開される。
- `echo foo$BAR` のように未クォートの連結が正しく展開される。
- `\$NAME` がリテラルとして扱われる。

### コマンドテスト

- `set AAA=123` → `env` に `AAA=123` が表示される。
- `unset AAA` → `env` に表示されない。
- `set AAA` のように `=` が無い場合は `UsageError`。

## 互換性・拡張性

- 将来拡張として `${NAME:-default}` や `${NAME?err}` のようなパラメータ展開に対応可能。
- OS環境変数の読み込みは `VariableStore` の初期化時に注入する形で追加できる。

## 実装上の注意点

- トークン単位の拡張ではなく、クォート単位の拡張制御が必要なため、トークナイズの拡張は必須。
- 既存の `Token.Value` をそのまま利用するとクォート境界の情報が失われるため、セグメント化が最も安全。
- GC削減のため、展開処理は `StringBuilder` と `List<WordTokenSegment>` を使い回す設計にする。
