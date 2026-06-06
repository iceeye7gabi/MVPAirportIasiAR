# Testing Guide — Ghid Aeroport Iași (MVP simplificat)

## Versiunea curentă

Aplicația are **3 secțiuni** în meniul principal:

1. **Descoperă aeroportul** — camera telefonului
2. **Întrebări și răspunsuri** — FAQ despre Aeroportul Iași
3. **Despre aplicație** — text descriptiv

## Test în Unity Editor

1. Deschide `Assets/Scenes/MainScene.unity`
2. **Play**
3. Verifică:
   - [ ] Apare titlul „Ghid Aeroport Iași”
   - [ ] Cele 3 butoane răspund
   - [ ] FAQ: întrebări sugerate + răspunsuri în română
   - [ ] Despre: text vizibil + Înapoi
   - [ ] Descoperă: mesaj că camera e doar pe telefon (normal în Editor)

## Test pe iPhone

Vezi **[IOS_AR_TESTING.md](IOS_AR_TESTING.md)** pentru build Xcode.

Checklist pe device:

- [ ] Meniul principal se încarcă
- [ ] **Descoperă aeroportul** → popup permisiune cameră → **Permite**
- [ ] Feed cameră vizibil (nu ecran negru)
- [ ] **Înapoi** oprește camera
- [ ] FAQ: „Unde se află Aeroportul Iași?” → răspuns
- [ ] **Despre aplicație** → text + Înapoi

### Dacă camera e neagră

1. **Setări → [app] → Cameră → ON**
2. Închide complet app-ul și redeschide
3. Rebuild din Unity dacă ai pull recent

## Test pe Android (opțional / viitor)

Vezi **[ANDROID_AR_TESTING.md](ANDROID_AR_TESTING.md)**. Modul Descoperă folosește `WebCamTexture` — funcționează și fără ARCore pentru preview cameră.

## Cod legacy (nu în meniul curent)

Următoarele module există în cod dar nu sunt expuse în UI-ul simplificat:

- Navigație AR cu săgeți (`ARNavigationManager`)
- Staff map editor (`StaffMapEditor`)
- Tur aeroport (`AirportTourManager`)

Pot fi reactivate incremental la iterații viitoare.
