# Android AR Testing Guide (Phone Camera)

This guide explains how to build and test **real AR navigation** using your phone camera on an ARCore-compatible Android device.

> **Important:** The route layout is still the fictional **Demo Airport Layout**. You walk through blue AR arrows placed on your floor — this simulates airport navigation; it is not the real Iași Airport map.

---

## Prerequisites

| Requirement | Details |
|-------------|---------|
| Unity version | **2022.3.62f3** (via Unity Hub) |
| Phone | Android with **Google Play Services for AR (ARCore)** |
| USB | Cable to connect phone to Mac/PC |
| Space | Open floor area (~5–10 m) to walk the demo route |

Check ARCore support: [Google ARCore supported devices](https://developers.google.com/ar/devices)

---

## One-time Unity setup

### 1. Open the project

Open `MVPAirportIasiAR` in Unity **2022.3.62f3**.

### 2. Configure mobile AR build

**Android:** **Airport AR → Configure Android AR Build**  
**iPhone:** **Airport AR → Configure iOS AR Build**  
**Both:** **Airport AR → Configure Mobile AR (Android + iOS)**

This sets:
- Min Android SDK **24**
- **IL2CPP** + **ARM64**
- Portrait orientation

### 3. Enable ARCore loader (required once)

1. **Edit → Project Settings → XR Plug-in Management**
2. If prompted, install **XR Plug-in Management**
3. Open the **Android** tab (Android icon)
4. Check **ARCore**
5. Close Project Settings

### 4. Switch to Android platform

1. **File → Build Settings**
2. Select **Android**
3. Click **Switch Platform** (wait for reimport)

---

## Build and install on phone

### 5. Connect your phone

1. Enable **Developer options** on Android
2. Enable **USB debugging**
3. Connect via USB
4. Accept the debugging prompt on the phone

### 6. Build and run

1. **File → Build Settings**
2. Ensure **MainScene** is in **Scenes In Build**
3. Click **Build And Run**
4. Choose an output folder (e.g. `Builds/Android`)
5. Wait for build + install

On first launch, allow **Camera** permission when prompted.

---

## Testing AR navigation on the phone

### Step-by-step demo flow

1. **Launch the app** on your phone
2. Main menu appears → tap **Start AR Navigation**
3. Select a destination (e.g. **Gate A1**)
4. Navigation screen opens — you should see the **live camera feed** (semi-transparent UI overlay)
5. **Point the phone at the floor** and move it slowly to scan the surface (1–2 seconds)
6. Tap **Place Route Start**
7. **Blue arrows** appear on the floor in front of you
8. Read the step instruction (e.g. “Go forward toward Information Desk”)
9. **Walk along the arrows** in your room — each demo meter ≈ 1 real meter
10. Tap **Next Step (Demo)** as you progress through waypoints

### What success looks like

- Camera feed visible behind the UI
- Status text: “Route placed. Follow the blue arrows.”
- Blue arrow markers on the detected floor
- Route summary shows zone names
- Walking forward moves you through the arrow path

### If “Place Route Start” fails

- Improve lighting in the room
- Point at a flat floor or table surface
- Move the phone slowly in a small arc to help ARCore detect planes
- Tap **Place Route Start** again
- Try a different surface (avoid shiny/reflective floors)

---

## Additional tests on phone

### Test rerouting (Staff Mode)

1. **Back** → **Staff Mode**
2. Find **`checkin_security`** → tap **Closed** → **Save Map**
3. **Start AR Navigation** → **Gate A1**
4. **Place Route Start** again
5. Route should avoid Security and use Cafe/Toilets path

### Test chatbot → AR

1. **Airport Assistant Chatbot**
2. Type: `Cum ajung la cafenea?`
3. Tap **Navigate There**
4. **Place Route Start** → arrows toward Cafe zone

---

## Editor vs phone

| Feature | Unity Editor (Play) | Android phone |
|---------|---------------------|---------------|
| Camera feed | No — 3D graph fallback | Yes — live AR camera |
| AR arrows | Top-down demo view | On floor in front of you |
| Place Route Start | Not needed | Required once per session |
| Plane detection | No | Yes (ARCore) |

Use the **Editor** for UI/logic testing. Use the **phone** for the real AR camera experience.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Black screen in navigation | Grant camera permission; restart app |
| No camera feed | Confirm ARCore enabled in XR Plug-in Management |
| Build fails | Run **Airport AR → Configure Android AR Build** again |
| Phone not listed in Build And Run | Enable USB debugging; try another cable |
| App installs but crashes on start | Phone must support ARCore |
| No arrows after Place Route Start | Scan floor longer; tap Place Route Start again |
| Arrows float or drift | Normal for MVP without VPS; re-place route |

---

## Console logs (optional, USB debugging)

```bash
adb logcat -s Unity
```

Look for:
- `[MobileARSessionBootstrap] AR hierarchy created.`
- `[MobileARSessionBootstrap] AR active: True`
- `[MobileARSessionBootstrap] Route anchor placed on AR plane.`
- `[ARNavigationManager] Navigation started.`

---

## Quick checklist

- [ ] Unity 2022.3.62f3 installed
- [ ] **Airport AR → Configure Android AR Build** run
- [ ] ARCore enabled in XR Plug-in Management
- [ ] Android platform selected
- [ ] Phone connected, USB debugging on
- [ ] Build And Run succeeded
- [ ] Camera permission granted
- [ ] Floor scanned, **Place Route Start** tapped
- [ ] Blue arrows visible on floor
- [ ] Walked along demo route
