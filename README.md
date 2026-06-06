# Iași Airport AR Guide — Demo

Hackathon MVP: aplicație mobilă demo pentru **Aeroportul Internațional Iași**.  
Nu folosește harta reală a aeroportului — datele de navigație rămân fictive; conținutul FAQ este informativ general.

## Ce face aplicația (versiunea curentă)

La deschidere, **3 secțiuni**:

| Buton | Funcție |
|-------|---------|
| **Descoperă aeroportul** | Deschide camera telefonului (previzualizare live) |
| **Întrebări și răspunsuri** | FAQ despre Aeroportul Iași (asistent local, fără API) |
| **Despre aplicație** | Descriere scurtă a prototipului |

> Navigația AR cu săgeți, mod staff și tur ghidat există în cod ca bază pentru iterări viitoare, dar **nu sunt în meniul principal** al acestei versiuni.

## Quick start (colegi)

### 1. Instalează Unity

- **Unity Hub** → **2022.3.62f3** (LTS)
- La deschidere proiectului, folosește exact această versiune

### 2. Deschide proiectul

```text
Clone repo → Unity Hub → Open → MVPAirportIasiAR
```

### 3. Rulează în Editor

1. Deschide `Assets/Scenes/MainScene.unity`
2. Apasă **Play**
3. Vei vedea meniul cu 3 butoane (camera funcționează doar pe telefon, nu în Editor)

### 4. Build pe iPhone (Mac + Xcode)

1. Unity **2022.3.62f3** + **iOS Build Support** (vezi [IOS_AR_TESTING.md](IOS_AR_TESTING.md))
2. Dacă Switch Platform eșuează: `./scripts/install_ios_module.sh`
3. **File → Build Settings → iOS → Build**
4. Deschide proiectul Xcode, sign cu Apple ID, rulează pe iPhone
5. La **Descoperă aeroportul** → permite **Cameră** când iOS întreabă

Ghid complet iOS: **[IOS_AR_TESTING.md](IOS_AR_TESTING.md)**  
Ghid Android (pentru viitor): **[ANDROID_AR_TESTING.md](ANDROID_AR_TESTING.md)**

## Structură proiect

```text
Assets/
  Scenes/MainScene.unity     Scena principală (GameObject DemoApp)
  Scripts/
    DemoAppBootstrap.cs        Construiește UI-ul la runtime
    UI/AppController.cs        Navigare între cele 3 secțiuni
    AR/SimpleCameraPreview.cs  Camera live (Descoperă)
    AR/AirportDiscoverController.cs
    Chatbot/ChatbotManager.cs  FAQ Aeroport Iași
    Map/, Navigation/, AR/      Cod pentru extinderi viitoare
  Resources/Data/              demo_airport_map.json (layout fictiv)
  XR/                          Setări ARKit / ARCore
  Editor/                      Meniuri Airport AR (setup iOS/Android)
scripts/
  install_ios_module.sh        Fix iOS Build Support pe Mac (Unity Hub)
docs/
  documentation.tex            Documentație business + tehnică
```

## Meniuri Unity utile

| Meniu | Rol |
|-------|-----|
| **Airport AR → Create Main Scene** | Recreează MainScene dacă lipsește |
| **Airport AR → Configure iOS AR Build** | Setări build iPhone |
| **Airport AR → Switch Platform to iOS** | Schimbă platforma (după iOS module) |
| **Airport AR → Diagnose iOS Build Support** | Verifică dacă iOSPlayer există pe disk |

## Disclaimer compliance

- Layout-ul de navigație din JSON este **simulat**, nu harta reală Iași
- FAQ-ul descrie informații generale despre aeroport
- Prototip hackathon — nu este aplicația oficială a aeroportului

## Documentație suplimentară

- [TESTING.md](TESTING.md) — checklist testare
- [DEMO_SCRIPT.md](DEMO_SCRIPT.md) — script prezentare (parțial depășit de UI simplificat)
- [docs/documentation.tex](docs/documentation.tex) — document LaTeX complet

## Cerințe

- Unity **2022.3.62f3**
- iPhone: iOS 13+, permisiune cameră, Xcode pe Mac pentru build
- Android (viitor): ARCore, API 24+
