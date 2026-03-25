#!/bin/bash
# ===========================================================================
# XTerminal Documentation Deploy
#
# ビルド済みの _site ディレクトリをデプロイします。
# 先に build-docs.sh を実行してください。
#
# Usage:
#   ./deploy-docs.sh           # GitHub Pages にデプロイ
#   ./deploy-docs.sh --server  # レンタルサーバーにデプロイ
#
# レンタルサーバー設定（環境変数 or ~/.xterminal-deploy.conf で指定）:
#   DEPLOY_SERVER_HOST   - サーバーホスト名 (例: example.com)
#   DEPLOY_SERVER_USER   - SSHユーザー名
#   DEPLOY_SERVER_PATH   - デプロイ先ディレクトリ (例: /var/www/html/docs)
#   DEPLOY_SERVER_PORT   - SSHポート番号 (デフォルト: 22)
#   DEPLOY_SERVER_KEY    - SSH秘密鍵のパス (省略時はデフォルト鍵を使用)
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DOCS_DIR="$PROJECT_ROOT/docs"
SITE_DIR="$DOCS_DIR/_site"

# GitHub Pages 設定
EXTERNAL_REPO="git@github.com:AraiYuhki/XTerminal_Reference.git"
DEPLOY_BRANCH="gh-pages"

SERVER_MODE=false

for arg in "$@"; do
    case "$arg" in
        --server) SERVER_MODE=true ;;
        *)
            echo "不明なオプション: $arg"
            echo "使用方法: $0 [--server]"
            exit 1
            ;;
    esac
done

# ===========================================================================
# ビルド出力の確認
# ===========================================================================
check_site_dir() {
    if [ ! -d "$SITE_DIR" ]; then
        echo "エラー: ビルド出力が見つかりません ($SITE_DIR)"
        echo "先に build-docs.sh を実行してください。"
        exit 1
    fi
}

# ===========================================================================
# レンタルサーバー設定の読み込み
# ===========================================================================
load_server_config() {
    local conf_file="$HOME/.xterminal-deploy.conf"
    if [ -f "$conf_file" ]; then
        echo "サーバー設定を読み込んでいます: $conf_file"
        # shellcheck source=/dev/null
        source "$conf_file"
    fi

    if [ -z "${DEPLOY_SERVER_HOST:-}" ] || [ -z "${DEPLOY_SERVER_USER:-}" ] || [ -z "${DEPLOY_SERVER_PATH:-}" ]; then
        echo ""
        echo "エラー: レンタルサーバーの設定が不足しています。"
        echo ""
        echo "以下のいずれかの方法で設定してください:"
        echo ""
        echo "  1. 環境変数:"
        echo "       export DEPLOY_SERVER_HOST=example.com"
        echo "       export DEPLOY_SERVER_USER=username"
        echo "       export DEPLOY_SERVER_PATH=/var/www/html/docs"
        echo ""
        echo "  2. 設定ファイル ($conf_file):"
        echo "       DEPLOY_SERVER_HOST=example.com"
        echo "       DEPLOY_SERVER_USER=username"
        echo "       DEPLOY_SERVER_PATH=/var/www/html/docs"
        echo "       DEPLOY_SERVER_PORT=22  # 省略可"
        echo "       DEPLOY_SERVER_KEY=~/.ssh/id_rsa  # 省略可"
        echo ""
        exit 1
    fi

    DEPLOY_SERVER_PORT="${DEPLOY_SERVER_PORT:-22}"
    DEPLOY_SERVER_KEY="${DEPLOY_SERVER_KEY:-}"
}

# ===========================================================================
# rsync でレンタルサーバーにデプロイ
# ===========================================================================
deploy_to_server() {
    echo "レンタルサーバーにデプロイしています..."
    echo "  ホスト: ${DEPLOY_SERVER_USER}@${DEPLOY_SERVER_HOST}:${DEPLOY_SERVER_PATH}"
    echo "  ポート: ${DEPLOY_SERVER_PORT}"
    echo ""

    local rsync_opts=(-avz --delete --progress)
    local ssh_opts="-p ${DEPLOY_SERVER_PORT} -o StrictHostKeyChecking=no"

    if [ -n "${DEPLOY_SERVER_KEY}" ]; then
        ssh_opts="$ssh_opts -i ${DEPLOY_SERVER_KEY}"
    fi

    rsync_opts+=(-e "ssh $ssh_opts")

    # リモートディレクトリを作成
    ssh -p "${DEPLOY_SERVER_PORT}" \
        ${DEPLOY_SERVER_KEY:+-i "${DEPLOY_SERVER_KEY}"} \
        "${DEPLOY_SERVER_USER}@${DEPLOY_SERVER_HOST}" \
        "mkdir -p '${DEPLOY_SERVER_PATH}'"

    # rsync でアップロード（trailing slash で中身のみ転送）
    rsync "${rsync_opts[@]}" \
        "$SITE_DIR/" \
        "${DEPLOY_SERVER_USER}@${DEPLOY_SERVER_HOST}:${DEPLOY_SERVER_PATH}/"

    echo ""
    echo "=== レンタルサーバーへのデプロイ完了 ==="
    echo "URL: https://${DEPLOY_SERVER_HOST}${DEPLOY_SERVER_PATH}"
}

# ===========================================================================
# GitHub Pages にデプロイ
# ===========================================================================
deploy_to_github_pages() {
    echo "GitHub Pages にデプロイしています..."

    local temp_dir
    temp_dir=$(mktemp -d)
    trap 'rm -rf "$temp_dir"' EXIT

    echo "デプロイ先リポジトリをクローンしています..."
    if git clone --branch "$DEPLOY_BRANCH" --depth 1 "$EXTERNAL_REPO" "$temp_dir" 2>/dev/null; then
        find "$temp_dir" -mindepth 1 -maxdepth 1 -not -name '.git' -exec rm -rf {} +
    else
        git clone --depth 1 "$EXTERNAL_REPO" "$temp_dir"
        cd "$temp_dir"
        git checkout --orphan "$DEPLOY_BRANCH"
        git rm -rf .
        cd "$OLDPWD"
    fi

    echo "ビルド出力をコピーしています..."
    cp -r "$SITE_DIR"/. "$temp_dir"/

    cd "$temp_dir"
    git add -A

    if git diff --cached --quiet; then
        echo "変更がないため、デプロイをスキップします。"
        exit 0
    fi

    local commit_date
    commit_date=$(date +%Y-%m-%d\ %H:%M:%S)
    git commit -m "Update documentation ($commit_date)"
    git push origin "$DEPLOY_BRANCH"

    echo ""
    echo "=== GitHub Pages へのデプロイ完了 ==="
    echo "URL: https://araiyuhki.github.io/XTerminal_Reference"
}

# ===========================================================================
# メイン処理
# ===========================================================================
echo "=== XTerminal Documentation Deploy ==="
echo ""

check_site_dir

if $SERVER_MODE; then
    load_server_config
    deploy_to_server
else
    deploy_to_github_pages
fi
