#!/bin/bash
# ===========================================================================
# XTerminal Documentation Build
#
# Usage:
#   ./build-docs.sh          # ドキュメントをビルド
#   ./build-docs.sh --serve  # ビルド後にローカルサーバーで確認
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DOCS_DIR="$PROJECT_ROOT/docs"
SITE_DIR="$DOCS_DIR/_site"

SERVE=false

for arg in "$@"; do
    case "$arg" in
        --serve) SERVE=true ;;
        *)
            echo "不明なオプション: $arg"
            echo "使用方法: $0 [--serve]"
            exit 1
            ;;
    esac
done

# ===========================================================================
# DocFX チェック・インストール
# ===========================================================================
ensure_docfx() {
    if ! command -v docfx &> /dev/null; then
        echo "DocFX が見つかりません。インストールします..."
        if ! command -v dotnet &> /dev/null; then
            echo "エラー: .NET SDK がインストールされていません。"
            echo "https://dotnet.microsoft.com/download からインストールしてください。"
            exit 1
        fi
        # Unity プロジェクトのルートには複数の .csproj があるため、
        # 一時ディレクトリで実行して dotnet の自動プロジェクト検出を回避する
        (cd /tmp && dotnet tool install -g docfx)
        echo "DocFX をインストールしました。"
    fi
}

# ===========================================================================
# メイン処理
# ===========================================================================
echo "=== XTerminal Documentation Build ==="
echo ""

ensure_docfx

# 古いビルド出力を削除
if [ -d "$SITE_DIR" ]; then
    echo "古いビルド出力を削除しています..."
    rm -rf "$SITE_DIR"
fi

# DocFX でビルド
echo "ドキュメントをビルドしています..."
docfx "$DOCS_DIR/docfx.json"

if [ ! -d "$SITE_DIR" ]; then
    echo "エラー: ビルド出力が見つかりません ($SITE_DIR)"
    exit 1
fi

echo ""
echo "=== ビルド完了 ==="
echo "出力先: $SITE_DIR"
echo ""

if $SERVE; then
    echo "ローカルサーバーを起動しています... (http://localhost:8080)"
    echo "終了するには Ctrl+C を押してください。"
    docfx serve "$SITE_DIR"
else
    echo "ローカルで確認する場合: $0 --serve"
    echo "デプロイする場合:       ./deploy-docs.sh [--server]"
fi
