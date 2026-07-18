#!/bin/bash
# Set Unity version from git tag in ProjectSettings.asset
# Usage: ./scripts/set-version.sh [version]
# If no version given, extracts from GITHUB_REF or latest git tag

set -euo pipefail

PROJECT_FILE="ProjectSettings/ProjectSettings.asset"

if [ -n "${1:-}" ]; then
    RAW_VERSION="$1"
else
    if [[ "${GITHUB_REF:-}" == refs/tags/v* ]]; then
        RAW_VERSION="${GITHUB_REF#refs/tags/}"
    else
        TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0")
        RAW_VERSION="$TAG"
    fi
fi

VERSION="${RAW_VERSION#v}"
VERSION_RAW=$(echo "$VERSION" | tr -d -c '0-9')
VERSION_CODE=$((10#$VERSION_RAW))
VERSION_CODE="${VERSION_CODE:-0}"

echo "Setting bundleVersion: $VERSION"
echo "Setting AndroidBundleVersionCode: $VERSION_CODE"

sed -i "s/^  bundleVersion: .*/  bundleVersion: $VERSION/" "$PROJECT_FILE"
sed -i "s/^  AndroidBundleVersionCode: .*/  AndroidBundleVersionCode: $VERSION_CODE/" "$PROJECT_FILE"

echo "Done. ProjectSettings.asset updated."
