#!/bin/bash
# ===========================================================================
# XTerminal Asset Store Upload
#
# Unity Editor を起動し、Asset Store Uploader ウィンドウを自動で開きます。
# ログイン後、アップロード先のパッケージ（プロジェクト）を選択してください。
#
# Usage:
#   ./upload-asset-store.sh
#
# Unity の実行ファイルパスを環境変数で指定できます:
#   UNITY_PATH=/path/to/Unity ./upload-asset-store.sh
#
# Unity Hub 経由でインストールされている場合、バージョンを指定:
#   UNITY_VERSION=6000.3.2f1 ./upload-asset-store.sh
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_PATH="$(cd "$SCRIPT_DIR/.." && pwd)"
EXECUTE_METHOD="Xeon.XTerminal.Editor.AssetStoreUploadLauncher.Launch"

# ===========================================================================
# Unity 実行ファイルの検索
# ===========================================================================
find_unity() {
    # 環境変数が設定されている場合はそれを使用
    if [ -n "${UNITY_PATH:-}" ]; then
        echo "$UNITY_PATH"
        return
    fi

    local version="${UNITY_VERSION:-}"

    # Linux: Unity Hub のデフォルトインストールパス
    local linux_hub_base="$HOME/Unity/Hub/Editor"
    if [ -n "$version" ] && [ -f "$linux_hub_base/$version/Editor/Unity" ]; then
        echo "$linux_hub_base/$version/Editor/Unity"
        return
    fi

    # Linux: バージョン指定なしで最新を検索
    if [ -d "$linux_hub_base" ]; then
        local found
        found=$(find "$linux_hub_base" -maxdepth 2 -name "Unity" -type f 2>/dev/null | sort -rV | head -1)
        if [ -n "$found" ]; then
            echo "$found"
            return
        fi
    fi

    # macOS: Unity Hub のデフォルトインストールパス
    local mac_hub_base="/Applications/Unity/Hub/Editor"
    if [ -n "$version" ]; then
        local mac_path="$mac_hub_base/$version/Unity.app/Contents/MacOS/Unity"
        if [ -f "$mac_path" ]; then
            echo "$mac_path"
            return
        fi
    fi

    if [ -d "$mac_hub_base" ]; then
        local found
        found=$(find "$mac_hub_base" -maxdepth 3 -name "Unity" -type f 2>/dev/null | sort -rV | head -1)
        if [ -n "$found" ]; then
            echo "$found"
            return
        fi
    fi

    # PATH 上の Unity
    if command -v Unity &>/dev/null; then
        command -v Unity
        return
    fi

    echo ""
}

# ===========================================================================
# メイン処理
# ===========================================================================
echo "=== XTerminal Asset Store Upload ==="
echo ""

UNITY_BIN="$(find_unity)"

if [ -z "$UNITY_BIN" ]; then
    echo "エラー: Unity の実行ファイルが見つかりません。"
    echo ""
    echo "以下のいずれかの方法で指定してください:"
    echo ""
    echo "  1. 環境変数 UNITY_PATH:"
    echo "       export UNITY_PATH=/path/to/Unity"
    echo "       $0"
    echo ""
    echo "  2. 環境変数 UNITY_VERSION (Unity Hub 使用時):"
    echo "       export UNITY_VERSION=6000.3.2f1"
    echo "       $0"
    exit 1
fi

echo "Unity         : $UNITY_BIN"
echo "プロジェクト  : $PROJECT_PATH"
echo ""
echo "Unity を起動しています..."
echo "起動後、Asset Store Uploader が自動で開きます。"
echo "ログインし、アップロード先のパッケージを選択してください。"
echo ""

"$UNITY_BIN" \
    -projectPath "$PROJECT_PATH" \
    -executeMethod "$EXECUTE_METHOD" \
    &

echo "Unity の起動を開始しました（PID: $!）"
