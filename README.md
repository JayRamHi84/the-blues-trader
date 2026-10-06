# 🎸 TheBluesTraderV2

> **Upgrade to AudioTapeReader.**  
> *This is an Order-Flow Audio Dynamics player for NinjaTrader 8. Transforms market microstructure, CVD delta, and tape momentum into dynamic "music".*

---

## 📖 Overview

* **Market Flow Governs the Chord:** Heavy selling flow drives the harmony downward into deep, moody minor and altered blues chords ($Gm9$, $Fm9$, $Cm11$). Bullish buying climbs into soaring dominant 9th and Lydian 13th chords ($F9$, $G9$, $C13$).
* **Musical Rhythmic Quantization:** Notes are quantized to a musical tempo grid (e.g., $133\text{ BPM}$ / $450\text{ ms}$). Micro-ticks pool into an **Energy Accumulator**, striking in time with dynamic velocity and pick attack scaled to institutional volume. Market stress conditions (Absorption, Braking) are expressed through authentic blues articulations (rhythmic guitar chops and guide-tone stutters) rather than dissonant clashing frequencies.

---

## ✨ Key Features

### 1. Vehicle Physics Engine
* **Tape Gas (`tapeScore`):** Recent net CVD delta percentage, measuring instantaneous aggression.
* **Momentum (`momV`):** Inertial driven velocity modeled with drag friction.
* **Price Velocity (`pVelV`):** ATR-normalized traction reading (detects whether price is actually rolling or stuck).
* **State Machine:**
  * **Cruising / Idle:** Equilibrium flow.
  * **Accelerating:** Gas floored in direction of momentum.
  * **Coasting:** Gas released, drifting on momentum.
  * **Braking:** Opposing delta slamming into existing price momentum.
  * **Absorption (Spinning Wheels):** Heavy gas floored, zero price traction.
  * **Momentum Drift (Free Roll):** Price rolling on thin liquidity with no gas.

### 2. Three Selectable Progression Engines
Selectable directly in the indicator parameters:

| Style | Harmonic Rule Set | Ideal Market Context |
| :--- | :--- | :--- |
| **Blues** | Pure Dominant-family ($7, 9, 13, \text{altered}$). Bullish uses natural extensions ($9, 13$); bearish uses dark alterations ($\flat9, \sharp9, \flat13, \sharp11$). Extremes ($\pm5, \pm6$) stay unresolved. | Classic trending sessions with sustained delta pressure. |
| **Jazz** | Major Lydian color on the bull side ($C\text{maj}9 \rightarrow F\text{maj}7\sharp11 \rightarrow C\text{maj}13\sharp11$) vs. dark $ii\text{-}V$ grammar on the bear side ($Cm9 \rightarrow Dm7\flat5 \rightarrow G7\flat13$). Neutral $C6/9$ center. | High-contrast acoustic listening; distinct bull/bear textures. |
| **Hybrid** *(Default)* | Uses **Blues** grammar for ordinary flow (Stages $\pm1$ to $\pm3$) and shifts to **Jazz** grammar at extreme regimes (Stages $\pm4$ to $\pm6$). | Daytrading & Scalping. The shift in chord family instantly signals that a major move is breaking out. |

### 3. The 13-Stage Progression Ladder

```
[+6]  Cmaj13#11 / Bb13 (Cosmic Climax)
[+5]  G7#9 Hendrix Screamer / Abmaj7#11
[+4]  F13 / Ebmaj9#11 (High Altitude Push)
[+3]  A13 / G13 (Breakout Euphoria)
[+2]  G9 / Fmaj7#11 (Dominant Climax)
[+1]  F9 / Cmaj9 (Momentum Lift)
[ 0]  C7 / C6/9 (EQUILIBRIUM CENTER)
[-1]  C7#9 / Cm9 (Minor Hesitation)
[-2]  Bb7#9 / Abmaj7#11 (Bearish Push)
[-3]  Ab7 / Fm9 (Deep Selling)
[-4]  G7b9 / Dm7b5 (Liquidity Cascade)
[-5]  Gb7#11 / G7b13 (Capitulation Flush)
[-6]  G7alt / Cm11 Sub-Bass (The Abyss)
```

### 4. Rhythmic Quantization & Energy Accumulation
* **Tempo Grid (`RhythmPaceMs`):** Quantizes sound to a human tempo (e.g., $450\text{ ms} \approx 133\text{ BPM}$). High-frequency tick bursts pool together instead of flooding the MIDI bus.
* **Energy Accumulator:** Trades during the beat window accumulate volume. Heavy blocks trigger sharp guitar pick strikes; low-volume chop plucks softly.
* **Syncopated Events (`SyncopateMajorEvents`):** Urgent market events (reversals, breakout signals) can strike early on syncopated off-beats.
* **Chord Dwell (`ChordDwellMs`):** Minimum hold time ($800\text{ ms}$) to prevent chords from jittering across thresholds on fast tick charts.

---

## 🛠 Installation in NinjaTrader 8

1. Clone or download this repository.
2. Copy **`TheBluesTraderV2.cs`** into your NinjaTrader indicators directory:
   ```
   Documents\NinjaTrader 8\bin\Custom\Indicators\
   ```
3. Open NinjaTrader 8.
4. Press **`F5`** inside the **NinjaScript Editor** (Tools $\rightarrow$ New NinjaScript Editor) to compile.
5. Add **TheBluesTraderV2** to any chart panel.

---

## ⚙️ Parameters

| Group | Parameter | Default | Description |
| :--- | :--- | :--- | :--- |
| **1. Tape / Session** | `TapeLen` | `2` | Number of bars for net delta CVD integration. |
| | `SessionStart` | `18:00:00` | Globex session reset time. |
| **2. Vehicle Dynamics** | `ThrottleGain` | `1.0` | Sensitivity scalar for order flow acceleration. |
| | `Drag` | `0.20` | Damping friction applied to momentum. |
| | `GasThresh` | `10.0` | Minimum net CVD score to flag active gas. |
| | `StrongLevel` | `40.0` | Extreme force threshold for momentum lines. |
| **3. Price Velocity** | `VelLen` | `2` | Price displacement lookback bars. |
| | `AtrLen` | `7` | ATR normalization period. |
| **4. Audio Engine** | `EnableAudio` | `true` | Toggle MIDI audio on/off. |
| | `MidiInstrument` | `28` | General MIDI patch (`28` = Clean Electric Guitar, `16` = Organ). |
| | `ProgressionStyle`| `Hybrid` | `Hybrid`, `Blues`, or `Jazz`. |
| | `VoicingMode` | `DynamicEventsAndMelody` | Melody runs on small trades; full voiced chords on sweeps/events. |
| | `RhythmPaceMs` | `450` | Beat interval in milliseconds ($450\text{ ms} \approx 133\text{ BPM}$). |
| | `SyncopateMajorEvents` | `true` | Allow urgent breakouts to strike on syncopated off-beats. |
| | `ChordDwellMs` | `800` | Minimum chord hold time to eliminate threshold jitter. |
| | `BaseVelocity` | `80` | Base MIDI strike loudness ($0\text{–}127$). |

---

## 📄 License

GNU General Public License V3. See `LICENSE` for more information.