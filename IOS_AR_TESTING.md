# iPhone AR Testing Guide (ARKit)

How to build and test the **Iași Airport AR Guide** demo on iPhone.

> **App curent:** meniul principal are 3 secțiuni — **Descoperă aeroportul** (cameră live), **Întrebări și răspunsuri**, **Despre aplicație**. Secțiunea Descoperă folosește camera telefonului (permisiune necesară).

> **Important:** Routes in the legacy navigation code use the fictional **Demo Airport Layout** only.

---

## Prerequisites

| Requirement | Details |
|-------------|---------|
| Mac | iOS builds require macOS + Xcode |
| Unity | **2022.3.62f3** |
| Xcode | Latest from Mac App Store (15.x or newer recommended) |
| iPhone | ARKit-compatible (iPhone 6s or newer) |
| Apple ID | Free account works for device testing (7-day cert) |
| Cable | USB / USB-C to connect iPhone to Mac |

Check ARKit support: [Apple ARKit device list](https://developer.apple.com/augmented-reality/arkit/)

---

## One-time Unity setup (on your Mac)

### 1. Open the project

Open `MVPAirportIasiAR` in Unity **2022.3.62f3**.

Wait for packages to import (including **ARKit**).

### 2. Configure iOS AR build

Menu: **Airport AR → Configure iOS AR Build**

This sets:
- Minimum iOS **13**
- **ARKit required**
- **Camera usage description** (App Store / iOS privacy prompt)
- Portrait orientation

Or run **Airport AR → Configure Mobile AR (Android + iOS)** for both platforms.

### 3. Enable ARKit loader (required once)

1. **Edit → Project Settings → XR Plug-in Management**
2. If prompted, install **XR Plug-in Management**
3. Select the **iOS** tab (Apple icon)
4. Check **ARKit**
5. Close Project Settings

### 4. Switch to iOS platform

**If Switch Platform is greyed out or fails**, Unity Hub may show iOS as "Installed" without actually copying files. Run in Terminal:

```bash
./scripts/install_ios_module.sh
```

Then restart Unity and use **Airport AR → Diagnose iOS Build Support** to verify.

Otherwise:

1. **File → Build Settings**
2. Select **iOS**
3. Click **Switch Platform** (or menu **Airport AR → Switch Platform to iOS**)

---

## Build and run on iPhone

### 5. Connect your iPhone

1. Connect iPhone to Mac with cable
2. On iPhone: **Trust This Computer** if prompted
3. Enable **Developer Mode** (iOS 16+):
   - **Settings → Privacy & Security → Developer Mode → On**
   - Restart iPhone if asked

### 6. Build Xcode project from Unity

1. **File → Build Settings**
2. Confirm **MainScene** is in **Scenes In Build**
3. Click **Build** (not Build And Run — Xcode handles device deploy)
4. Create/select folder e.g. `Builds/iOS`
5. Wait for Unity to generate the Xcode project

### 7. Open in Xcode and sign

1. Open `Builds/iOS/Unity-iPhone.xcodeproj` (or `.xcworkspace` if CocoaPods appear)
2. Select **Unity-iPhone** target
3. **Signing & Capabilities** tab:
   - Check **Automatically manage signing**
   - **Team:** your Apple ID (add via Xcode → Settings → Accounts if needed)
   - **Bundle Identifier:** change to something unique, e.g. `com.yourname.airportardemo`
4. At top toolbar, select your **iPhone** as run destination (not Simulator)

### 8. Run on device

1. Click **Run** (▶) in Xcode
2. On iPhone, if prompted: **Settings → General → VPN & Device Management** → trust developer app
3. Allow **Camera** when the app asks

> **Note:** ARKit does **not** work in the iOS Simulator. You must use a physical iPhone.

---

## Testing AR navigation on iPhone

Same flow as Android:

1. Launch app → **Start AR Navigation**
2. Select destination (e.g. **Gate A1** or **Information Desk** for shorter walk)
3. Navigation screen opens → **live camera feed** with UI overlay
4. **Point iPhone at the floor** — move slowly to scan (1–2 seconds)
5. Tap **Place Route Start**
6. **Blue arrows** appear on the floor
7. Walk along arrows; tap **Next Step (Demo)** to advance instructions

### What success looks like

- Camera feed visible behind semi-transparent UI
- Status: “Route placed. Follow the blue arrows.”
- Blue arrows anchored on detected horizontal surface
- Instructions update as you tap Next Step

### If Place Route Start fails

- Use good lighting
- Point at flat floor, table, or desk
- Move phone slowly side-to-side
- Tap **Place Route Start** again
- Avoid very reflective or featureless surfaces

---

## iPhone vs Android vs Editor

| | iPhone (ARKit) | Android (ARCore) | Unity Editor |
|--|----------------|------------------|----------------|
| Camera AR | Yes | Yes | No (3D fallback) |
| Build tool | Xcode | Build And Run | Play |
| Simulator | ARKit not supported | N/A | Editor fallback |
| Place Route Start | Required | Required | Not needed |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Unity missing iOS module | Unity Hub → Installs → Add **iOS Build Support** |
| Xcode signing error | Unique Bundle ID + Automatic signing + Apple ID team |
| “Untrusted Developer” on iPhone | Settings → General → VPN & Device Management → Trust |
| Black camera view | Allow camera permission; restart app |
| ARKit not loading | Enable ARKit in XR Plug-in Management iOS tab |
| Build fails on IL2CPP | Re-run **Configure iOS AR Build** |
| App works in Xcode but no AR | Must run on **physical device**, not Simulator |
| Developer Mode missing | Update iOS; enable in Privacy & Security settings |

---

## Optional: wireless deploy from Xcode

After first USB install:

1. Xcode → **Window → Devices and Simulators**
2. Select iPhone → enable **Connect via network**
3. Future runs can deploy over Wi‑Fi (same network as Mac)

---

## Quick checklist

- [ ] Unity iOS Build Support installed
- [ ] **Airport AR → Configure iOS AR Build** run
- [ ] ARKit enabled in XR Plug-in Management (iOS)
- [ ] Platform switched to iOS
- [ ] Xcode project built from Unity
- [ ] Signing configured with Apple ID
- [ ] iPhone selected (not Simulator)
- [ ] App installed and camera permission granted
- [ ] Floor scanned → **Place Route Start** → arrows visible

---

## Related docs

- Android testing: [ANDROID_AR_TESTING.md](ANDROID_AR_TESTING.md)
- General project setup: [README.md](README.md)
