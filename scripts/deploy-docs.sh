#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DOCS_DIR="$PROJECT_ROOT/docs"
SITE_DIR="$DOCS_DIR/_site"
EXTERNAL_REPO="git@github.com:AraiYuhki/UniTerminal_Reference.git"
DEPLOY_BRANCH="gh-pages"

echo "=== UniTerminal Documentation Build & Deploy ==="

# DocFX がインストールされているか確認
if ! command -v docfx &> /dev/null; then
    echo "DocFX が見つかりません。インストールします..."
    if ! command -v dotnet &> /dev/null; then
        echo "エラー: .NET SDK がインストールされていません。"
        echo "https://dotnet.microsoft.com/download からインストールしてください。"
        exit 1
    fi
    dotnet tool install -g docfx
    echo "DocFX をインストールしました。"
fi

# 古いビルド出力を削除
if [ -d "$SITE_DIR" ]; then
    echo "古いビルド出力を削除しています..."
    rm -rf "$SITE_DIR"
fi

# DocFX でドキュメントをビルド
echo "ドキュメントをビルドしています..."
docfx "$DOCS_DIR/docfx.json"

if [ ! -d "$SITE_DIR" ]; then
    echo "エラー: ビルド出力が見つかりません ($SITE_DIR)"
    exit 1
fi

echo "ビルド完了: $SITE_DIR"

# --build-only オプションでビルドのみ実行
if [ "${1:-}" = "--build-only" ]; then
    echo "ビルドのみモード: デプロイをスキップします。"
    echo "ローカルプレビュー: docfx serve $SITE_DIR"
    exit 0
fi

# デプロイ用の一時ディレクトリを作成
TEMP_DIR=$(mktemp -d)
trap 'rm -rf "$TEMP_DIR"' EXIT

echo "デプロイ先リポジトリをクローンしています..."
if git clone --branch "$DEPLOY_BRANCH" --depth 1 "$EXTERNAL_REPO" "$TEMP_DIR" 2>/dev/null; then
    # 既存のファイルを削除（.git は除く）
    find "$TEMP_DIR" -mindepth 1 -maxdepth 1 -not -name '.git' -exec rm -rf {} +
else
    # ブランチが存在しない場合、新規作成
    git clone --depth 1 "$EXTERNAL_REPO" "$TEMP_DIR"
    cd "$TEMP_DIR"
    git checkout --orphan "$DEPLOY_BRANCH"
    git rm -rf .
fi

# ビルド出力をコピー
echo "ビルド出力をコピーしています..."
cp -r "$SITE_DIR"/. "$TEMP_DIR"/

# コミットしてプッシュ
cd "$TEMP_DIR"
git add -A

if git diff --cached --quiet; then
    echo "変更がないため、デプロイをスキップします。"
    exit 0
fi

COMMIT_DATE=$(date +%Y-%m-%d\ %H:%M:%S)
git commit -m "Update documentation ($COMMIT_DATE)"
git push origin "$DEPLOY_BRANCH"

echo "=== デプロイ完了 ==="
echo "https://araiyuhki.github.io/UniTerminal_Reference"
