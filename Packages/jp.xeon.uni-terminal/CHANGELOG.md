# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-02-04

### Added

- **`pbcopy` コマンド**
  - パイプラインの出力をクリップボードにコピーする機能
  - 例: `echo Hello | pbcopy`

- **Transform演算機能**
  - `transform add/sub/mul/div` サブコマンドを追加
  - Position、Rotation、Scaleの四則演算をサポート
  - 例: `transform add /Player -p 1,0,0`

- **Property演算機能**
  - `property add/sub/mul/div` サブコマンドを追加
  - 数値型（int, float, double等）の四則演算
  - Vector型（Vector2, Vector3, Vector4, Vector2Int, Vector3Int）の四則演算
  - 例: `property mul /Player Transform localScale 2,2,2`

- **ValueConverter**
  - 汎用的な型変換・演算ユーティリティクラスを追加
  - 数値型・Vector型の加減乗除をサポート

### Fixed

- **`tail -f` コマンドの改善**
  - ファイル更新検出をFileSystemWatcherからポーリング方式に変更し、信頼性を向上
  - ファイルが切り詰められた場合に「tail: file truncated」メッセージを表示し、最後のN行を再出力
  - ファイル削除後の再作成時に正しく内容を取得
  - 複数行の追記時に正しく改行を処理

- **パーサーの負の値対応**
  - `-1,2,3` のような負の数値を含む引数がオプションとして誤認識される問題を修正
  - Vector形式の引数（カンマ区切り）を正しく値として認識

### Changed

- **コードの整理**
  - 不要な空行の削除
  - `#region` ディレクティブの削除

## [0.1.1] - 2025-01-27

### Fixed

- **Unity 6000.3+ Compatibility**
  - Added preprocessor directive to use `EditorUtility.EntityIdToObject` on Unity 6000.3.0+ and `EditorUtility.InstanceIDToObject` on earlier versions
  - Resolves deprecation warning in Unity 6.3 LTS

## [0.1.0] - 2025-01-15

### Added

- **Core Framework**
  - String-based CLI execution framework with Linux-like behavior
  - Pipeline support with pipe (`|`) and redirect (`>`, `>>`, `<`) operators
  - Async/await command execution
  - Tab completion for commands and paths

- **File Operation Commands**
  - `pwd` - Print working directory
  - `cd` - Change directory
  - `ls` - List directory contents
  - `cat` - Display file contents
  - `find` - Search for files
  - `less` - View files page by page
  - `diff` - Compare files

- **Text Processing Commands**
  - `echo` - Output text
  - `grep` - Pattern matching search

- **Utility Commands**
  - `help` - Display help information
  - `history` - Command history management

- **Unity-Specific Commands**
  - `hierarchy` - Display scene hierarchy with filtering options
  - `go` - GameObject operations (create, delete, find, clone, etc.)
  - `transform` - Transform manipulation (position, rotation, scale)
  - `component` - Component management (add, remove, enable/disable)
  - `property` - Property value operations via reflection

- **UniTask Support** (Optional)
  - `IUniTaskCommand` interface for UniTask-based async commands
  - `UniTaskCommandContext` for UniTask command execution
  - Automatic detection when UniTask is installed

- **FlyweightScrollView**
  - Virtual scrolling for efficient large data display
  - Vertical and horizontal scroll support
  - `CircularBuffer` for fixed-size log buffering
  - `ObservableCollection` integration

### Technical Details

- Minimum Unity version: 6000.0
- Supports custom command registration via `ICommand` interface
- Assembly definition files for proper code separation
- Comprehensive unit tests and PlayMode tests

[1.0.0]: https://github.com/AraiYuhki/UniTerminal/releases/tag/v1.0.0
[0.1.1]: https://github.com/AraiYuhki/UniTerminal/releases/tag/v0.1.1
[0.1.0]: https://github.com/AraiYuhki/UniTerminal/releases/tag/v0.1.0
