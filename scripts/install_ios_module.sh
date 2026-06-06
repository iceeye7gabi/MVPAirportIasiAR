#!/bin/bash
# Install iOS Build Support for Unity Hub editor 2022.3.62f3 on Apple Silicon Mac.
# Unity Hub often reports iOS as "Installed" without copying any files.
set -euo pipefail

UNITY_VERSION="2022.3.62f3"
UNITY_HASH="96770f904ca7"
UNITY_ROOT="/Applications/Unity/Hub/Editor/${UNITY_VERSION}"
HUB_IOS="${UNITY_ROOT}/PlaybackEngines/iOSSupport"
APP_IOS_LINK="${UNITY_ROOT}/Unity.app/Contents/PlaybackEngines/iOSPlayer"
MODULES_JSON="${UNITY_ROOT}/modules.json"
PKG_URL="https://download.unity3d.com/download_unity/${UNITY_HASH}/MacEditorTargetInstaller/UnitySetup-iOS-Support-for-Editor-${UNITY_VERSION}.pkg"
PKG_FILE="/tmp/UnitySetup-iOS-Support-for-Editor-${UNITY_VERSION}.pkg"
EXTRACT_DIR="/tmp/unity-ios-pkg-manual"

echo "=== Unity iOS Build Support installer (Hub layout) ==="
echo "Unity editor: ${UNITY_ROOT}"
echo "Target folder: ${HUB_IOS}"

if [ ! -d "${UNITY_ROOT}/Unity.app" ]; then
  echo "ERROR: Unity ${UNITY_VERSION} not found."
  exit 1
fi

if [ -d "${HUB_IOS}" ]; then
  echo "iOSSupport already exists. Nothing to do."
  exit 0
fi

if [ ! -f "${PKG_FILE}" ]; then
  echo "Downloading iOS Build Support (~900 MB)..."
  curl -L --fail --progress-bar -o "${PKG_FILE}" "${PKG_URL}"
else
  echo "Using cached pkg: ${PKG_FILE}"
fi

echo "Extracting module..."
rm -rf "${EXTRACT_DIR}"
mkdir -p "${EXTRACT_DIR}"
xar -xf "${PKG_FILE}" -C "${EXTRACT_DIR}"
PAYLOAD="${EXTRACT_DIR}/TargetSupport.pkg.tmp/Payload"
mkdir -p "${EXTRACT_DIR}/TargetSupport.pkg.tmp/extracted"
(
  cd "${EXTRACT_DIR}/TargetSupport.pkg.tmp/extracted"
  cat "${PAYLOAD}" | gunzip -c | cpio -id >/dev/null 2>&1
)
SRC="${EXTRACT_DIR}/TargetSupport.pkg.tmp/extracted/iOSSupport"
if [ ! -d "${SRC}" ]; then
  echo "ERROR: iOSSupport not found in pkg."
  exit 1
fi

echo "Installing to Hub PlaybackEngines..."
mkdir -p "${UNITY_ROOT}/PlaybackEngines"
cp -R "${SRC}" "${HUB_IOS}"

echo "Linking into editor PlaybackEngines..."
mkdir -p "$(dirname "${APP_IOS_LINK}")"
rm -rf "${APP_IOS_LINK}"
ln -sf "../../../PlaybackEngines/iOSSupport" "${APP_IOS_LINK}"

if [ -f "${MODULES_JSON}" ]; then
  python3 - <<PY
import json
path = "${MODULES_JSON}"
with open(path) as f:
    mods = json.load(f)
for m in mods:
    if m.get("id") == "ios":
        m["selected"] = True
        m["isInstalled"] = True
with open(path, "w") as f:
    json.dump(mods, f, separators=(",", ":"))
print("Updated modules.json (ios selected + isInstalled)")
PY
fi

if [ -d "${HUB_IOS}" ]; then
  echo ""
  echo "SUCCESS. Restart Unity, then:"
  echo "  Airport AR → Switch Platform to iOS"
else
  echo "ERROR: Install failed."
  exit 1
fi
