# BasalOptimizer

Diese README beschreibt die Funktionsweise, Konfiguration und Nutzung der Klasse **BasalOptimizer** zur Ermittlung einer empfohlenen 24‑Stunden‑Basalratenkurve (I.E./h pro Stunde des Tages) aus asynchronen Zeitreihen von Glukosemessungen, Basalprofilen und Bolusereignissen.

---

## Kurzbeschreibung

**Ziel**  
Aus Zeiten ohne aktiven Bolus und mit stabiler, im Zielbereich liegender Glukose repräsentative Basalraten pro Stunde des Tages ableiten.

**Eingangsdaten**  
- Glukosemessungen in **mmol/L** mit Zeitstempel  
- Basalprofil als Liste von Stufen mit Startzeit und `UnitsPerHour`  
- Bolusereignisse mit Zeitstempel, `Units` und Dauer in Minuten

**Ausgabe**  
`Dictionary<int,double>`: Stunde des Tages (0..23) → empfohlene Basalrate in I.E./h

---

## Architektur und Ablauf

### 1. Rasteraufbau und Synchronisation
- Erzeuge ein regelmäßiges Zeitraster mit `GridStep` (Standard 5 Minuten).  
- Interpoliere Glukosewerte linear auf das Raster nach Umrechnung **mmol/L → mg/dl** (Faktor **18.0**).  
- Wende Basal als stufenförmiges Profil an: die zuletzt gültige Basalstufe gilt bis zur nächsten Änderung.  
- Markiere Rasterpunkte als ausgeschlossen, wenn sie in einem Bolus‑Ausschlussintervall liegen, die Glukose außerhalb des Zielbereichs liegt oder die Glukose nicht interpolierbar ist.

### 2. Bolus‑Ausschlusslogik
- Für jeden Bolus wird ein Ausschlussintervall erzeugt von  
  `Bolus.Time` bis `max(Bolus.Time + Bolus.Minutes, Bolus.Time + ExcludeAfterBolus)`.  
- Standardwert: `ExcludeAfterBolus = 4 Stunden`. Dadurch werden Bolus‑on‑Board‑Effekte berücksichtigt.

### 3. Stundenvalidierung
- Gruppiere Rasterpunkte nach Stunde (Datum + Stunde).  
- Für jede Stunde:
  - Bestimme `validPts` = Punkte mit `IsExcluded == false` und `GlucoseMgDl.HasValue`.  
  - Berechne `validFraction = validPts.Count / expectedPointsPerHour` (bei 5‑Minuten‑Raster = 12).  
  - Verwerfe die Stunde, wenn `validFraction < MinValidFractionPerHour`.  
  - Prüfe Stabilität: verwerfe, wenn `|gEnd − gStart| > StabilityDeltaMgPerHour`.  
  - Falls gültig: berechne `meanBasal` = Durchschnitt von `BasalUnitsPerHour` über `validPts`.

### 4. Aggregation und Glättung
- Sammle `meanBasal`‑Werte über mehrere Tage für jede Stunde des Tages.  
- Berechne den **Median** pro Stunde als Empfehlung.  
- Optional: Glätte das Profil mit einem 3‑stündigen gleitenden Mittelwert.

---

## Konfigurierbare Parameter

- **GridStep** `TimeSpan` — Rasterauflösung, Standard `TimeSpan.FromMinutes(5)`  
- **TargetMinMgDl** `double` — Untere Zielgrenze Glukose in mg/dl, Standard `70.0`  
- **TargetMaxMgDl** `double` — Obere Zielgrenze Glukose in mg/dl, Standard `150.0`  
- **ExcludeAfterBolus** `TimeSpan` — Nachwirkzeit nach Bolus, Standard `TimeSpan.FromHours(4)`  
- **StabilityDeltaMgPerHour** `double` — Maximal erlaubte Glukoseänderung pro Stunde, Standard `15.0`  
- **MinValidFractionPerHour** `double` — Mindestanteil gültiger 5‑Minuten‑Punkte pro Stunde, Standard `0.6`  
- **MaxInterpolationGapMinutes** `double` — Maximal erlaubte Lücke für lineare Interpolation, Standard `20.0`  
- **ApplySmoothing** `bool` — 3‑stündige Glättung des finalen Profils, Standard `true`

**Empfehlungen**  
- `GridStep`: 5 Minuten.  
- `MinValidFractionPerHour`: 0.5–0.75 je nach Datenqualität (0.6 guter Startwert).  
- `MaxInterpolationGapMinutes`: 15–30 Minuten.  
- `StabilityDeltaMgPerHour`: 10–20 mg/dl.

---

## API Referenz

**Daten hinzufügen**
```csharp
void AddGlucoseEntries(IEnumerable<GlucoseConcentrationEntry> entries)
void AddBasalEntries(IEnumerable<InsulinInfusionEntry> entries)
void AddBolusEntries(IEnumerable<InsulinBolusEntry> entries)
