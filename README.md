# 🎸 TheBluesTraderV2

> **Major Upgrade to AudioTapeReader.**  
> *Real-Time Order-Flow Dynamics Sonifier for NinjaTrader 8. Translates market microstructure, CVD delta, and physical tape momentum into music.*

---

## 📖 Overview

Auditory aid for the tape.

**TheBluesTraderV2** bridges order-flow physics with the **Piano Keyboard**:
* **Price Moves UP (Bullish):** Hands physically glide to the **RIGHT** across the keys $\rightarrow$ pitches strictly ascend into bright, light treble registers.
* **Price Moves DOWN (Bearish):** Hands physically glide to the **LEFT** across the keys $\rightarrow$ pitches strictly descend into the warm, resonant acoustic bass.
* **Stepwise Harmonic Walk:** A built-in slew rate limiter prevents teleportation. If the tape turns around from $-3$ to $+2$, the chords sequentially walk one step at a time ($\mathbf{-3} \rightarrow \mathbf{-2} \rightarrow \mathbf{-1} \rightarrow \mathbf{0} \rightarrow \mathbf{+1} \rightarrow \mathbf{+2}$), preserving smooth voice leading during reversals.
* **Rhythmic Quantization Grid:** Notes are clocked to a musical tempo grid (e.g., $133\text{ BPM}$ / $450\text{ ms}$). Micro-ticks pool into an **Energy Accumulator**, striking in rhythm with dynamic pick attack velocity scaled to institutional volume.

---

## ✨ Key Features

### 1. Vehicle Physics Engine
* **Tape Gas (`tapeScore`):** Recent net CVD delta percentage, measuring instantaneous aggression.
* **Momentum (`momV`):** Inertial driven velocity modeled with drag friction.
* **Price Velocity (`pVelV`):** ATR-normalized traction reading (distinguishes moving markets from stationary absorption).
* **State Machine:**
  * **Cruising / Idle:** Equilibrium flow.
  * **Accelerating:** Gas floored in direction of momentum.
  * **Coasting:** Gas released, drifting on momentum.
  * **Braking:** Opposing delta slamming into existing price momentum (voiced as a tight, percussive bass choke).
  * **Absorption (Spinning Wheels):** Heavy gas floored, zero price traction (voiced as a guide-tone stutter).
  * **Momentum Drift (Free Roll):** Price rolling on thin liquidity with no gas.

---

### 2. Six Zero-Conflict Scale Worlds
Every scale is locked to a single harmonic system—**zero accidentals, zero clashing semitones**:

| Progression Style | Musical Personality & Feel | Trading Psychology |
| :--- | :--- | :--- |
| **`Dorian`** *(Default)* | **Soulful Jazz Club** (*Kind of Blue* / Santana) | Minor root ($Dm$), but features a sweet, bright natural 6th ($B$). Cool, relaxed, never depressive. Stage +3 gives a bright major pop on buyer surges. |
| **`Diatonic`** | **Pure Daylight & Clarity** (C Major / Ionian) | Grounded on Middle C. All natural white keys. Classical, crystal clear, zero ambiguity. |
| **`Aeolian`** | **Cinematic Drama** (A Minor / Hans Zimmer) | Serious, epic film score gravity. Deep left steps sound ominous; bull breakouts hit triumphant major landings at Stage +2 ($C$) and Stage +5 ($F$). |
| **`Mixolydian`** | **Southern Rock & Blues Roll** (G Dominant) | Major feel with a flat 7th ($F\natural$). Upbeat rolling groove (Allman Brothers / Grateful Dead). |
| **`BlackKeysPentatonic`** | **Ambient Spa / Zen Bells** (F♯ Pentatonic) | Uses **only the 5 black keys** on the piano. Zero half-steps anywhere. Mathematically impossible to play a conflicting note. Perfect for high-volatility, stressful chop. |
| **`QuartalHarmony`** | **Modern Acoustic Jazz** (Stacked 4ths) | Stacked in perfect fourths ($D\text{--}G\text{--}C$) like McCoy Tyner with John Coltrane. Open, glassy, architectural, neither cheesy major nor sad minor. |

---

### 3. The Physical 13-Stage Keyboard Map

```
RIGHT (Bullish / Ascending Pitch)
  [+6]  Peak High Climax
  [+5]  High Treble Extension
  [+4]  Strong Bullish Trend
  [+3]  Clean Breakout Lift
  [+2]  Momentum Building
  [+1]  First Bullish Pop
  [ 0]  EQUILIBRIUM CENTER (Middle C/D Home Base)
  [-1]  Initial Pullback Dip
  [-2]  Selling Traction
  [-3]  Bearish Trend Developing
  [-4]  Heavy Selling Pressure
  [-5]  Liquidity Flush
  [-6]  Deep Acoustic Bass Floor
LEFT (Bearish / Descending Pitch)
```

---

### 4. Rhythmic Quantization & Energy Accumulator
* **Tempo Grid (`RhythmPaceMs`):** Clocks sound to a human tempo (default: $450\text{ ms} \approx 133\text{ BPM}$). High-frequency tick bursts pool together instead of flooding the MIDI bus.
* **Energy Accumulator:** Trades between beats accumulate volume. Heavy blocks strike with loud, punchy velocity; light prints pluck softly.
* **Syncopated Events (`SyncopateMajorEvents`):** Urgent structural events (reversals, breakout signals) can strike early on syncopated off-beats.
* **Chord Dwell (`ChordDwellMs`):** Controls the transition rate ($600\text{ ms}$) between stepwise chord stages during market turns.

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
| | `MoveThresh` | `2.0` | Threshold for price roll vs. absorption. |
| **4. Audio Engine** | `EnableAudio` | `true` | Toggle MIDI audio on/off. |
| | `MidiInstrument` | `28` | General MIDI patch (`28` = Clean Electric Guitar, `0` = Grand Piano, `16` = Organ). |
| | `ProgressionStyle`| `Dorian` | `Dorian`, `Diatonic`, `Aeolian`, `Mixolydian`, `BlackKeysPentatonic`, or `QuartalHarmony`. |
| | `VoicingMode` | `DynamicEventsAndMelody` | Melodic solo runs on routine trades; voiced chords on major sweeps/events. |
| | `RhythmPaceMs` | `450` | Beat tempo interval in milliseconds ($450\text{ ms} \approx 133\text{ BPM}$). |
| | `SyncopateMajorEvents` | `true` | Allow urgent breakouts to strike on syncopated off-beats. |
| | `ChordDwellMs` | `600` | Stepwise transition speed per chord during market turnarounds. |
| | `BaseVelocity` | `80` | Base MIDI strike loudness ($0\text{–}127$). |
| | `MinVolumeFilter` | `1` | Ignore trade executions below this volume threshold. |

---

## 📄 License

GNU General Public License v3.0 (GPL-3.0). See `LICENSE` for more information.