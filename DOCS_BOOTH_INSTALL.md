# XTerminal インストール手順 (BOOTH版)

BOOTHで配布されている `.tgz` ファイル（Unityパッケージ）を、Unity Package Manager (UPM) を使用してインストールする方法を説明します。

## 1. 前提条件

XTerminalは以下のパッケージに依存しています。

- **Unity UI (uGUI)** (com.unity.ugui)
    - Unity Editor の `Window > Package Manager` を開き、 `Unity Registry` から `Unity UI` を選択して `Install` してください。
    - ※Unity 2023以降では標準で含まれている場合があります。

### オプション: UniTask サポート
プロジェクトに [UniTask](https://github.com/Cysharp/UniTask) がインストールされている場合、XTerminal の UniTask 拡張機能が自動的に有効になります。

## 2. インストール手順

1.  Unity プロジェクトを開きます。
2.  メニューから **Window > Package Manager** を選択します。
3.  Package Manager ウィンドウの左上にある **「+」** ボタンをクリックします。
4.  **Add package from tarball...** を選択します。
5.  ダウンロードした `jp.xeon.x-terminal-1.0.1.tgz`（または最新バージョン）を選択して「開く」をクリックします。
6.  インストールが完了すると、Package Manager のリストに `XTerminal` が表示されます。

## 3. サンプルの導入（任意）

XTerminal には「基本の使用方法」サンプルが含まれています。

1.  Package Manager で `XTerminal` を選択します。
2.  右側の詳細パネルにある **Samples** セクションを展開します。
3.  **Import** ボタンをクリックします。
4.  サンプルはプロジェクトの `Assets/Samples/XTerminal/[Version]/基本の使用方法` にインポートされます。

## 4. トラブルシューティング

- **エラーが出る場合**: Unity のコンソールを確認してください。uGUI パッケージが正しくインストールされているか確認してください。
- **再インストール**: 古いバージョンを削除してから、新しい `.tgz` ファイルを上記の手順で再度選択してください。

---
© 2026 Xeon - [GitHub Repository](https://github.com/AraiYuhki/XTerminal)
