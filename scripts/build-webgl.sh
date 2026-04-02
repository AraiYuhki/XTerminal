#!/bin/bash
# ===========================================================================
# XTerminal WebGL Build
#
# Unity Editor を使用して WebGL ビルドを実行します。
#
# Usage:
#   ./build-webgl.sh sample         # WebGLサンプルをビルド
#   ./build-webgl.sh hacking        # ハッキングゲームをビルド
#   ./build-webgl.sh all            # すべてビルド
#   ./build-webgl.sh sample -o /out # 出力先を指定
#
# Unity の実行ファイルパスを環境変数で指定できます:
#   UNITY_PATH=/path/to/Unity ./build-webgl.sh sample
#   UNITY_VERSION=6000.3.2f1 ./build-webgl.sh sample
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_PATH="$(cd "$SCRIPT_DIR/.." && pwd)"
LOG_FILE="$PROJECT_PATH/webgl-build.log"

# ビルドターゲット → executeMethod のマッピング
declare -A TARGETS=(
    ["sample"]="Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildSampleFromCLI"
    ["hacking"]="Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildHackingGameFromCLI"
    ["all"]="Xeon.XTerminal.WebGLSample.Editor.WebGLBuildTool.BuildAllFromCLI"
)

# ===========================================================================
# 使い方の表示
# ===========================================================================
show_usage() {
    echo "使用方法: $0 <target> [-o output_path]"
    echo ""
    echo "ターゲット:"
    echo "  sample    WebGLサンプルをビルド   (→ WebGLBuild/Sample)"
    echo "  hacking   ハッキングゲームをビルド (→ WebGLBuild/HackingGame)"
    echo "  all       すべてビルド"
    echo ""
    echo "オプション:"
    echo "  -o PATH   ビルド出力先ディレクトリ (デフォルトを上書き)"
    echo "  -h        このヘルプを表示"
}

# ===========================================================================
# 引数パース
# ===========================================================================
if [ $# -eq 0 ]; then
    show_usage
    exit 1
fi

TARGET="$1"
shift

BUILD_OUTPUT=""

while getopts "o:h" opt; do
    case "$opt" in
        o) BUILD_OUTPUT="$OPTARG" ;;
        h)
            show_usage
            exit 0
            ;;
        *)
            echo "不明なオプション: -$OPTARG"
            exit 1
            ;;
    esac
done

EXECUTE_METHOD="${TARGETS[$TARGET]:-}"
if [ -z "$EXECUTE_METHOD" ]; then
    echo "エラー: 不明なターゲット '$TARGET'"
    echo ""
    show_usage
    exit 1
fi

# ===========================================================================
# Unity 実行ファイルの検索
# ===========================================================================
find_unity() {
    if [ -n "${UNITY_PATH:-}" ]; then
        echo "$UNITY_PATH"
        return
    fi

    local version="${UNITY_VERSION:-}"

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

    # Linux: Unity Hub のデフォルトインストールパス
    local linux_hub_base="$HOME/Unity/Hub/Editor"
    if [ -n "$version" ] && [ -f "$linux_hub_base/$version/Editor/Unity" ]; then
        echo "$linux_hub_base/$version/Editor/Unity"
        return
    fi

    if [ -d "$linux_hub_base" ]; then
        local found
        found=$(find "$linux_hub_base" -maxdepth 2 -name "Unity" -type f 2>/dev/null | sort -rV | head -1)
        if [ -n "$found" ]; then
            echo "$found"
            return
        fi
    fi

    if command -v Unity &>/dev/null; then
        command -v Unity
        return
    fi

    echo ""
}

# ===========================================================================
# メイン処理
# ===========================================================================
echo "=== XTerminal WebGL Build ==="
echo ""

UNITY_BIN="$(find_unity)"

if [ -z "$UNITY_BIN" ]; then
    echo "エラー: Unity の実行ファイルが見つかりません。"
    echo ""
    echo "以下のいずれかの方法で指定してください:"
    echo "  export UNITY_PATH=/path/to/Unity"
    echo "  export UNITY_VERSION=6000.3.2f1"
    exit 1
fi

echo "Unity          : $UNITY_BIN"
echo "プロジェクト   : $PROJECT_PATH"
echo "ターゲット     : $TARGET"
echo "ログファイル   : $LOG_FILE"
echo ""

BUILD_ARGS=(
    -quit
    -batchmode
    -nographics
    -projectPath "$PROJECT_PATH"
    -executeMethod "$EXECUTE_METHOD"
    -logFile "$LOG_FILE"
)

if [ -n "$BUILD_OUTPUT" ]; then
    echo "出力先         : $BUILD_OUTPUT"
    BUILD_ARGS+=(-buildOutput "$BUILD_OUTPUT")
fi

echo ""
echo "WebGL ビルドを開始しています..."
echo "（完了まで数分かかる場合があります）"
echo ""

"$UNITY_BIN" "${BUILD_ARGS[@]}"

BUILD_EXIT=$?

if [ $BUILD_EXIT -ne 0 ]; then
    echo "エラー: WebGL ビルドに失敗しました (exit code: $BUILD_EXIT)"
    echo "ログファイルを確認してください: $LOG_FILE"
    exit $BUILD_EXIT
fi

echo ""
echo "=== WebGL ビルド完了 ==="
echo ""
echo "ローカルで確認する場合:"
echo "  python3 -m http.server 8080 -d WebGLBuild/<target>"
echo "  ブラウザで http://localhost:8080 を開いてください"
echo ""
echo "XServer にデプロイする場合:"
echo "  ./deploy-webgl.sh -b WebGLBuild/<target>"
