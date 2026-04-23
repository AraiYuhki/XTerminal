#!/bin/bash

# XTerminal Packaging Script for BOOTH
# This script packages the Unity package into a .tgz file and creates a .zip for distribution.

set -e

PACKAGE_PATH="Packages/jp.xeon.x-terminal"
DIST_PATH="Dist"

# Ensure Dist directory exists and is clean
mkdir -p "$DIST_PATH"
rm -rf "${DIST_PATH:?}"/*

# Get version and display name from package.json
VERSION=$(node -e "console.log(require('./$PACKAGE_PATH/package.json').version)")
DISPLAY_NAME=$(node -e "console.log(require('./$PACKAGE_PATH/package.json').displayName)")

echo -e "\033[0;36mPackaging $DISPLAY_NAME v$VERSION...\033[0m"

# Navigate to package directory and pack
cd "$PACKAGE_PATH"
PACKAGE_FILE=$(npm pack 2>/dev/null)
if [ $? -ne 0 ]; then
    echo -e "\033[0;31mnpm pack failed. Make sure npm is installed.\033[0m"
    exit 1
fi
cd -

# Move the package to Dist folder
mv "$PACKAGE_PATH/$PACKAGE_FILE" "$DIST_PATH/"

# Copy documentation and LICENSE to Dist folder
echo "Copying documentation and LICENSE..."
[ -f "LICENSE.md" ] && cp -f "LICENSE.md" "$DIST_PATH/"
cp -f "README.md" "$DIST_PATH/"
cp -f "DOCS_BOOTH_INSTALL.md" "$DIST_PATH/"

# Create ZIP file
ZIP_NAME="${DISPLAY_NAME}_v${VERSION}.zip"
echo -e "\033[0;36mCreating ZIP archive: $ZIP_NAME\033[0m"
rm -f "$ZIP_NAME"

cd "$DIST_PATH"
zip -r "../$ZIP_NAME" ./*
cd ..

echo -e "\033[0;32mSuccessfully packaged: $ZIP_NAME\033[0m"
echo -e "\033[0;33mThe ZIP file is ready for distribution at the project root.\033[0m"
