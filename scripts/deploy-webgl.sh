#!/bin/bash
# ===========================================================================
# XTerminal WebGL Deploy to XServer
#
# WebGL ビルド出力を XServer（レンタルサーバー）にアップロードします。
# 先に build-webgl.sh を実行してください。
#
# Usage:
#   ./deploy-webgl.sh                   # デフォルト設定でデプロイ
#   ./deploy-webgl.sh -b /path/to/build # ビルドディレクトリを指定
#   ./deploy-webgl.sh --dry-run         # 実際にはアップロードしない（確認用）
#
# サーバー設定（環境変数 or ~/.xterminal-webgl-deploy.conf で指定）:
# サーバー設定（環境変数 or scripts/.xterminal-webgl-deploy.conf で指定）:
#   WEBGL_SERVER_HOST   - サーバーホスト名 (例: example.xsrv.jp)
#   WEBGL_SERVER_USER   - SSHユーザー名 (例: example)
#   WEBGL_SERVER_PATH   - デプロイ先ディレクトリ (例: /home/example/example.xsrv.jp/public_html/xterminal)
#   WEBGL_SERVER_PORT   - SSHポート番号 (デフォルト: 10022)
#   WEBGL_SERVER_KEY    - SSH秘密鍵のパス (省略時はデフォルト鍵を使用)
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BUILD_DIR="$PROJECT_ROOT/WebGLBuild"
DRY_RUN=false

# ===========================================================================
# 引数パース
# ===========================================================================
while [[ $# -gt 0 ]]; do
    case "$1" in
        -b|--build-dir)
            BUILD_DIR="$2"
            shift 2
            ;;
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        -h|--help)
            echo "使用方法: $0 [-b build_dir] [--dry-run]"
            echo ""
            echo "オプション:"
            echo "  -b, --build-dir PATH  ビルドディレクトリ (デフォルト: $BUILD_DIR)"
            echo "  --dry-run             実際にはアップロードしない（確認用）"
            echo "  -h, --help            このヘルプを表示"
            exit 0
            ;;
        *)
            echo "不明なオプション: $1"
            echo "使用方法: $0 [-b build_dir] [--dry-run]"
            exit 1
            ;;
    esac
done

# ===========================================================================
# ビルド出力の確認
# ===========================================================================
check_build_dir() {
    if [ ! -d "$BUILD_DIR" ]; then
        echo "エラー: ビルド出力が見つかりません ($BUILD_DIR)"
        echo "先に build-webgl.sh を実行してください。"
        exit 1
    fi

    # index.html の存在を確認
    if [ ! -f "$BUILD_DIR/index.html" ]; then
        echo "エラー: index.html が見つかりません ($BUILD_DIR/index.html)"
        echo "ビルドが正しく完了していない可能性があります。"
        exit 1
    fi

    echo "ビルドディレクトリ: $BUILD_DIR"
    echo "ファイル数: $(find "$BUILD_DIR" -type f | wc -l | tr -d ' ')"
    echo "合計サイズ: $(du -sh "$BUILD_DIR" | cut -f1)"
    echo ""
}

# ===========================================================================
# サーバー設定の読み込み
# ===========================================================================
load_server_config() {
    local conf_file="$SCRIPT_DIR/.xterminal-webgl-deploy.conf"
    if [ -f "$conf_file" ]; then
        echo "サーバー設定を読み込んでいます: $conf_file"
        # shellcheck source=/dev/null
        source "$conf_file"
    fi

    if [ -z "${WEBGL_SERVER_HOST:-}" ] || [ -z "${WEBGL_SERVER_USER:-}" ] || [ -z "${WEBGL_SERVER_PATH:-}" ]; then
        echo ""
        echo "エラー: サーバーの設定が不足しています。"
        echo ""
        echo "以下のいずれかの方法で設定してください:"
        echo ""
        echo "  1. 環境変数:"
        echo "       export WEBGL_SERVER_HOST=example.xsrv.jp"
        echo "       export WEBGL_SERVER_USER=example"
        echo "       export WEBGL_SERVER_PATH=/home/example/example.xsrv.jp/public_html/xterminal"
        echo ""
        echo "  2. 設定ファイル ($conf_file):"
        echo "       WEBGL_SERVER_HOST=example.xsrv.jp"
        echo "       WEBGL_SERVER_USER=example"
        echo "       WEBGL_SERVER_PATH=/home/example/example.xsrv.jp/public_html/xterminal"
        echo "       WEBGL_SERVER_PORT=10022  # 省略可 (デフォルト: 10022)"
        echo "       WEBGL_SERVER_KEY=~/.ssh/id_rsa  # 省略可"
        echo ""
        echo "  XServer の SSH 接続情報はサーバーパネルの"
        echo "  「SSH設定」から確認できます。"
        exit 1
    fi

    # XServer のデフォルト SSH ポートは 10022
    WEBGL_SERVER_PORT="${WEBGL_SERVER_PORT:-10022}"
    WEBGL_SERVER_KEY="${WEBGL_SERVER_KEY:-}"
}

# ===========================================================================
# .htaccess の生成
# ===========================================================================
create_htaccess() {
    local htaccess_path="$BUILD_DIR/.htaccess"

    # 既に存在する場合はスキップ
    if [ -f "$htaccess_path" ]; then
        echo ".htaccess は既に存在します。スキップします。"
        return
    fi

    echo ".htaccess を生成しています..."

    cat > "$htaccess_path" << 'HTACCESS_EOF'
# XTerminal WebGL - gzip/Brotli 圧縮ファイルの配信設定
<IfModule mod_mime.c>
    # gzip 圧縮されたファイル
    AddEncoding gzip .gz
    AddType application/javascript .js.gz
    AddType application/wasm .wasm.gz
    AddType application/octet-stream .data.gz
    AddType application/json .json.gz

    # Brotli 圧縮されたファイル
    AddEncoding br .br
    AddType application/javascript .js.br
    AddType application/wasm .wasm.br
    AddType application/octet-stream .data.br
    AddType application/json .json.br

    # WASM MIME タイプ
    AddType application/wasm .wasm
</IfModule>

# CORS ヘッダー（SharedArrayBuffer に必要な場合）
<IfModule mod_headers.c>
    Header set Cross-Origin-Opener-Policy "same-origin"
    Header set Cross-Origin-Embedder-Policy "require-corp"
</IfModule>

# キャッシュ設定
<IfModule mod_expires.c>
    ExpiresActive On
    # Build ファイルは長期キャッシュ（ファイル名にハッシュが含まれるため）
    ExpiresByType application/javascript "access plus 1 year"
    ExpiresByType application/wasm "access plus 1 year"
    # HTML は短めに
    ExpiresByType text/html "access plus 1 hour"
</IfModule>
HTACCESS_EOF

    echo ".htaccess を生成しました。"
}

# ===========================================================================
# SSH オプションの構築
# ===========================================================================
build_ssh_opts() {
    SSH_OPTS=(-p "${WEBGL_SERVER_PORT}" -o StrictHostKeyChecking=no)
    if [ -n "${WEBGL_SERVER_KEY}" ]; then
        SSH_OPTS+=(-i "${WEBGL_SERVER_KEY}")
    fi
}

# ===========================================================================
# scp でサーバーにデプロイ
# ===========================================================================
deploy_to_server() {
    echo "XServer にデプロイしています..."
    echo "  ホスト: ${WEBGL_SERVER_USER}@${WEBGL_SERVER_HOST}:${WEBGL_SERVER_PATH}"
    echo "  ポート: ${WEBGL_SERVER_PORT}"
    echo ""

    build_ssh_opts

    if $DRY_RUN; then
        echo "[DRY RUN] 以下のファイルがアップロードされます:"
        find "$BUILD_DIR" -type f | while read -r f; do
            echo "  ${f#"$BUILD_DIR/"}"
        done
        echo ""
        echo "=== DRY RUN 完了 ==="
        echo "実際にデプロイするには --dry-run を外して実行してください。"
        return
    fi

    # リモートディレクトリを作成
    echo "リモートディレクトリを作成しています..."
    ssh "${SSH_OPTS[@]}" \
        "${WEBGL_SERVER_USER}@${WEBGL_SERVER_HOST}" \
        "mkdir -p '${WEBGL_SERVER_PATH}/Build'"

    # scp でアップロード（-r で再帰的に転送）
    echo "ファイルをアップロードしています..."
    scp -r "${SSH_OPTS[@]}" \
        "$BUILD_DIR/"* \
        "${WEBGL_SERVER_USER}@${WEBGL_SERVER_HOST}:${WEBGL_SERVER_PATH}/"

    # .htaccess は隠しファイルなので個別に転送
    if [ -f "$BUILD_DIR/.htaccess" ]; then
        scp "${SSH_OPTS[@]}" \
            "$BUILD_DIR/.htaccess" \
            "${WEBGL_SERVER_USER}@${WEBGL_SERVER_HOST}:${WEBGL_SERVER_PATH}/"
    fi

    echo ""
    echo "=== XServer へのデプロイ完了 ==="
    echo "URL: https://${WEBGL_SERVER_HOST}${WEBGL_SERVER_PATH#*public_html}"
}

# ===========================================================================
# メイン処理
# ===========================================================================
echo "=== XTerminal WebGL Deploy ==="
echo ""

check_build_dir
load_server_config
create_htaccess
deploy_to_server
