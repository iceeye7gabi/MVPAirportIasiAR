# Hackathon Demo Script — Iași Airport AR Guide (Demo)

> **Notă (2026):** UI-ul curent are 3 secțiuni (Descoperă / FAQ / Despre). Scriptul de mai jos descrie flow-ul complet original (navigație AR, staff, chatbot) — util pentru prezentări viitoare când reactivăm aceste module.

**Duration:** ~5 minutes  
**Important opening line:**  
"This demo uses a fictional airport layout. It does not represent the real Iași Airport."

---

## Flow 1 — AR Navigation (60s)

1. Launch app → Main Menu shows title + compliance disclaimer.
2. Tap **Start AR Navigation**.
3. Select **Gate A1**.
4. Point out:
   - Route summary (Entrance → Info → Check-in → Security → Gate A1)
   - Step instruction
   - Estimated demo distance
   - Disclaimer: "Demo route only"
5. In Editor: show 3D graph + blue arrows. On device: show AR arrows on floor.

**Say:**  
"We simulate indoor guidance with step-by-step instructions and rerouting logic, without using any real airport map."

---

## Flow 2 — Chatbot to Navigation (60s)

1. Back → **Airport Assistant Chatbot**.
2. Tap suggested question or type: **"Cum ajung la cafenea?"**
3. Chatbot explains the simulated cafe zone in Romanian.
4. Tap **Navigate There**.
5. App opens navigation toward **Cafe**.

**Say:**  
"The assistant is rule-based and offline — no external API — and can hand off directly to AR navigation."

---

## Flow 3 — Dynamic Layout Update (90s)

1. Open **Staff Mode / Map Editor**.
2. Find connection **`checkin_security`**.
3. Tap **Closed** → **Save Map**.
4. Start navigation to **Gate A1** again.
5. Show rerouted path via **Check-in → Cafe → Toilets → Gate A1**.
6. Optional: mark another corridor closed until no route remains and show:
   - "No available route in the simulated layout. Please ask airport staff for assistance."

**Say:**  
"When corridors change during airport reconstruction, staff can update the map and passengers get a new route instantly."

---

## Flow 4 — Compliance Explanation (45s)

1. Open Chatbot.
2. Ask: **"Este aceasta harta reală?"**
3. Read chatbot answer about simulated layout and future authorized data.

**Say:**  
"We deliberately use fictional data for compliance. In production, the same architecture connects to an airport-provided indoor map."

---

## Flow 5 — Airport Tour (optional, 45s)

1. Open **Airport Tour**.
2. Step through Entrance, Check-in, Security, Gates, Baggage, Taxi Exit.
3. Tap **Navigate Here** on one stop.

**Say:**  
"The tour mode helps first-time passengers understand key public zones in the demo layout."

---

## Closing (15s)

"This MVP proves AR indoor navigation, dynamic rerouting, passenger chatbot assistance, and staff map editing — all on a safe simulated layout ready to be replaced with authorized airport data."

---

## Troubleshooting during demo

| Issue | Fix |
|-------|-----|
| Blank screen on Play | Run **Airport AR → Create Main Scene** |
| No 3D graph visible | Open Navigation panel; editor fallback auto-enables in Editor |
| AR not working on device | Confirm ARCore + AR Session Origin in scene |
| Route not changing after staff edit | Tap **Save Map**, then **Recalculate Route** |
