// TheBluesTraderV2
// JayRamHi84
// Licensed under the terms of the project LICENSE file.

// ⚠️ Risk & Financial Disclaimer
//This software is for educational, research, and entertainment purposes only. 
//It does not constitute financial, investment, or trading advice. 
//Futures, options, and equities trading involve substantial risk of loss and are not suitable for every investor. 
//The author assumes no responsibility or liability for any financial losses incurred from using this indicator.

#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript
{
    public enum BluesVoicingMode
    {
        DynamicEventsAndMelody,
        PrimarilyChords
    }

    public enum BluesProgressionStyle
    {
        Diatonic,             // C Major / Ionian (Classical daylight, pure clean)
        Dorian,               // D Dorian (Kind of Blue / Santana / Cool Jazz)
        Aeolian,              // A Aeolian (Cinematic Drama / Hans Zimmer / Epic Minor)
        Mixolydian,           // G Mixolydian (Classic Rock / Allman Brothers / Blues Groove)
        BlackKeysPentatonic,  // F# Pentatonic (Zero-conflict Zen / Ambient / Spa)
        QuartalHarmony        // Stacked 4ths (McCoy Tyner / Modern Acoustic Jazz / Glassy & Open)
    }
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class TheBluesTraderV2 : Indicator
    {
        #region Win32 MIDI API P/Invoke
        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(out IntPtr lphMidiOut, int uDeviceID, IntPtr dwCallback, IntPtr dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        private static extern int midiOutShortMsg(IntPtr hMidiOut, int dwMsg);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(IntPtr hMidiOut);

        private IntPtr midiHandle = IntPtr.Zero;
        #endregion

        #region Musical Structure Definitions
        private const double G = 0.00001;

        private struct HarmonicStage
        {
            public string Name;
            public int[] ChordNotes;
            public int[] MelodyScale;

            public HarmonicStage(string name, int[] chordNotes, int[] melodyScale)
            {
                Name = name;
                ChordNotes = chordNotes;
                MelodyScale = melodyScale;
            }
        }

        // ==========================================================================
        // 1. C DIATONIC (C MAJOR / IONIAN)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Pure Daylight, Classical Clarity, Balanced & Transparent.
        // PSYCHOLOGY: Middle C is balanced home base. Right-hand moves up the white keys 
        //             on buying, left-hand slides into the warm piano bass on selling.
        //             Zero accidentals, zero clashes, crystal clear trend recognition.
        // ==========================================================================
        private readonly HarmonicStage[] diatonicLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: Dm [Deep Low]",    new int[] { 45, 50, 53, 57 }, new int[] { 45, 48, 50, 53, 55, 57, 60 }),
            new HarmonicStage("Stage -5: Em [Low]",         new int[] { 40, 47, 52, 55 }, new int[] { 40, 43, 47, 48, 50, 52, 55 }),
            new HarmonicStage("Stage -4: F [Low]",          new int[] { 41, 48, 53, 57 }, new int[] { 41, 45, 48, 50, 52, 53, 57 }),
            new HarmonicStage("Stage -3: G [Low]",          new int[] { 43, 50, 55, 59 }, new int[] { 43, 47, 50, 52, 55, 57, 59 }),
            new HarmonicStage("Stage -2: Am [Low]",         new int[] { 45, 52, 57, 60 }, new int[] { 45, 48, 52, 53, 55, 57, 60 }),
            new HarmonicStage("Stage -1: Bdim [Low]",       new int[] { 47, 53, 59, 62 }, new int[] { 47, 50, 53, 55, 57, 59, 62 }),
            new HarmonicStage("Stage  0: Tonic C [Center]", new int[] { 48, 52, 55, 60 }, new int[] { 48, 52, 55, 57, 60, 62, 64 }),
            new HarmonicStage("Stage +1: Dm [Ascending]",   new int[] { 50, 57, 62, 65 }, new int[] { 50, 53, 57, 60, 62, 65, 67 }),
            new HarmonicStage("Stage +2: Em [Ascending]",   new int[] { 52, 59, 64, 67 }, new int[] { 52, 55, 59, 60, 62, 64, 67 }),
            new HarmonicStage("Stage +3: F [Ascending]",    new int[] { 53, 60, 65, 69 }, new int[] { 53, 57, 60, 62, 64, 65, 69 }),
            new HarmonicStage("Stage +4: G [Ascending]",    new int[] { 55, 62, 67, 71 }, new int[] { 55, 59, 62, 64, 65, 67, 71 }),
            new HarmonicStage("Stage +5: Am [High]",        new int[] { 57, 64, 69, 72 }, new int[] { 57, 60, 64, 65, 67, 69, 72 }),
            new HarmonicStage("Stage +6: Bdim [High Peak]", new int[] { 59, 65, 71, 74 }, new int[] { 59, 62, 65, 67, 69, 71, 74 })
        };

        // ==========================================================================
        // 2. D DORIAN (KIND OF BLUE / CARLOS SANTANA)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Soulful Jazz Club, Late Night, Cool & Sophisticated.
        // PSYCHOLOGY: Grounded on Dm. It is minor, but features a bright natural 6th (B).
        //             Never sounds depressive. Stage +3 gives that signature Santana 
        //             bright G Major pop when buying pressure surges.
        // ==========================================================================
        private readonly HarmonicStage[] dorianLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: Em [Deep Low]",       new int[] { 40, 47, 52 }, new int[] { 40, 43, 47, 48, 50, 52, 55 }),
            new HarmonicStage("Stage -5: F [Low]",            new int[] { 41, 48, 53 }, new int[] { 41, 45, 48, 50, 52, 53, 57 }),
            new HarmonicStage("Stage -4: G [Low]",            new int[] { 43, 50, 55 }, new int[] { 43, 47, 50, 52, 55, 57, 59 }),
            new HarmonicStage("Stage -3: Am [Low]",           new int[] { 45, 52, 57 }, new int[] { 45, 48, 52, 53, 55, 57, 60 }),
            new HarmonicStage("Stage -2: Bdim [Low]",         new int[] { 47, 53, 59 }, new int[] { 47, 50, 53, 55, 57, 59, 62 }),
            new HarmonicStage("Stage -1: C [Low bVII]",       new int[] { 48, 55, 60 }, new int[] { 48, 52, 55, 57, 60, 62, 64 }),
            new HarmonicStage("Stage  0: Tonic Dm [Center]",  new int[] { 50, 57, 62 }, new int[] { 50, 53, 55, 57, 60, 62, 65 }),
            new HarmonicStage("Stage +1: Em [Ascending]",     new int[] { 52, 59, 64 }, new int[] { 52, 55, 57, 59, 62, 64, 67 }),
            new HarmonicStage("Stage +2: F [Ascending Lift]", new int[] { 53, 60, 65 }, new int[] { 53, 57, 60, 62, 64, 65, 69 }),
            new HarmonicStage("Stage +3: G [Santana Pop]",    new int[] { 55, 62, 67 }, new int[] { 55, 59, 62, 65, 67, 71, 74 }),
            new HarmonicStage("Stage +4: Am [High v]",        new int[] { 57, 64, 69 }, new int[] { 57, 60, 64, 67, 69, 72, 76 }),
            new HarmonicStage("Stage +5: Bdim [High vi°]",    new int[] { 59, 65, 71 }, new int[] { 59, 62, 65, 69, 71, 74, 77 }),
            new HarmonicStage("Stage +6: C [High Peak]",      new int[] { 60, 67, 72 }, new int[] { 60, 64, 67, 71, 72, 76, 79 })
        };

        // ==========================================================================
        // 3. A AEOLIAN (CINEMATIC DRAMA / HANS ZIMMER)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Epic Film Score, Intense Emotional Gravity, Heroic & Serious.
        // PSYCHOLOGY: Natural minor centered on Am. Moving left into deep bass sounds 
        //             like impending doom. Moving right delivers triumphant major breakouts
        //             at Stage +2 (C Major) and Stage +5 (F Major heroic surge).
        // ==========================================================================
        private readonly HarmonicStage[] aeolianLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: Bdim [Deep Floor]",   new int[] { 47, 53, 59 }, new int[] { 45, 47, 50, 53, 55, 57, 59 }),
            new HarmonicStage("Stage -5: C [Low bIII]",        new int[] { 48, 55, 60 }, new int[] { 47, 48, 52, 55, 57, 60, 62 }),
            new HarmonicStage("Stage -4: Dm [Low iv]",         new int[] { 50, 57, 62 }, new int[] { 48, 50, 53, 57, 58, 62, 65 }),
            new HarmonicStage("Stage -3: Em [Low v]",          new int[] { 52, 59, 64 }, new int[] { 50, 52, 55, 59, 60, 64, 67 }),
            new HarmonicStage("Stage -2: F [Low bVI]",         new int[] { 53, 60, 65 }, new int[] { 52, 53, 57, 60, 62, 65, 69 }),
            new HarmonicStage("Stage -1: G [Low bVII]",        new int[] { 55, 62, 67 }, new int[] { 53, 55, 59, 62, 64, 67, 71 }),
            new HarmonicStage("Stage  0: Tonic Am [Center]",   new int[] { 57, 64, 69 }, new int[] { 53, 57, 60, 64, 65, 69, 72 }),
            new HarmonicStage("Stage +1: Bdim [Ascending]",    new int[] { 59, 65, 71 }, new int[] { 55, 59, 62, 65, 67, 71, 74 }),
            new HarmonicStage("Stage +2: C [Major Breakout]",  new int[] { 60, 67, 72 }, new int[] { 57, 60, 64, 67, 69, 72, 76 }),
            new HarmonicStage("Stage +3: Dm [Ascending]",      new int[] { 62, 69, 74 }, new int[] { 58, 62, 65, 69, 70, 74, 77 }),
            new HarmonicStage("Stage +4: Em [Ascending]",      new int[] { 64, 71, 76 }, new int[] { 60, 64, 67, 71, 72, 76, 79 }),
            new HarmonicStage("Stage +5: F [Heroic Surge]",    new int[] { 65, 72, 77 }, new int[] { 62, 65, 69, 72, 74, 77, 81 }),
            new HarmonicStage("Stage +6: G [High Climax]",     new int[] { 67, 74, 79 }, new int[] { 64, 67, 71, 74, 76, 79, 83 })
        };

        // ==========================================================================
        // 4. G MIXOLYDIAN (SOUTHERN ROCK / BLUES GROOVE)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Upbeat Jam Band, Grateful Dead, Allman Brothers Blues Roll.
        // PSYCHOLOGY: Rooted on G Major with a flat 7th (F natural). Natural, rolling 
        //             blues bounce that keeps trading sessions lively without fatigue.
        // ==========================================================================
        private readonly HarmonicStage[] mixolydianLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: Am [Deep Ground]",    new int[] { 45, 52, 57 }, new int[] { 43, 45, 48, 52, 55, 57, 60 }),
            new HarmonicStage("Stage -5: Bdim [Low]",          new int[] { 47, 53, 59 }, new int[] { 45, 47, 50, 53, 55, 59, 62 }),
            new HarmonicStage("Stage -4: C [Low IV]",          new int[] { 48, 55, 60 }, new int[] { 47, 48, 52, 55, 57, 60, 64 }),
            new HarmonicStage("Stage -3: Dm [Low v]",          new int[] { 50, 57, 62 }, new int[] { 48, 50, 53, 57, 59, 62, 65 }),
            new HarmonicStage("Stage -2: Em [Low vi]",         new int[] { 52, 59, 64 }, new int[] { 50, 52, 55, 59, 60, 64, 67 }),
            new HarmonicStage("Stage -1: F [Low bVII]",        new int[] { 53, 60, 65 }, new int[] { 52, 53, 57, 60, 62, 65, 69 }),
            new HarmonicStage("Stage  0: Tonic G [Center]",    new int[] { 55, 62, 67 }, new int[] { 53, 55, 59, 62, 65, 67, 71 }),
            new HarmonicStage("Stage +1: Am [Ascending]",      new int[] { 57, 64, 69 }, new int[] { 55, 57, 60, 64, 67, 69, 72 }),
            new HarmonicStage("Stage +2: Bdim [Ascending]",    new int[] { 59, 65, 71 }, new int[] { 57, 59, 62, 65, 69, 71, 74 }),
            new HarmonicStage("Stage +3: C [Sweet Major Lift]",new int[] { 60, 67, 72 }, new int[] { 59, 60, 64, 67, 71, 72, 76 }),
            new HarmonicStage("Stage +4: Dm [Ascending]",      new int[] { 62, 69, 74 }, new int[] { 60, 62, 65, 69, 72, 74, 77 }),
            new HarmonicStage("Stage +5: Em [Ascending]",      new int[] { 64, 71, 76 }, new int[] { 62, 64, 67, 71, 74, 76, 79 }),
            new HarmonicStage("Stage +6: F [Rock Cadence]",    new int[] { 65, 72, 77 }, new int[] { 64, 65, 69, 72, 76, 77, 81 })
        };

        // ==========================================================================
        // 5. BLACK KEYS PENTATONIC (ZERO-CONFLICT ZEN / AMBIENT SPA)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Temple Bells, Ambient Floating Chimes, Brian Eno, Total Peace.
        // PSYCHOLOGY: Uses exclusively the 5 black keys (F#, G#, A#, C#, D#). Because 
        //             there are ZERO half-steps anywhere, it is physically impossible to 
        //             play a clashing note. Perfect for high-volatility, stressful chop.
        // ==========================================================================
        private readonly HarmonicStage[] blackKeysLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: F#sus2 [Deep Gong]",  new int[] { 42, 49, 54 }, new int[] { 42, 46, 49, 51, 54, 58, 61 }),
            new HarmonicStage("Stage -5: G#m [Low Bell]",      new int[] { 44, 51, 56 }, new int[] { 44, 46, 49, 51, 54, 56, 61 }),
            new HarmonicStage("Stage -4: A#m [Low Float]",     new int[] { 46, 53, 58 }, new int[] { 46, 49, 51, 54, 56, 58, 63 }),
            new HarmonicStage("Stage -3: C#sus2 [Low Chime]",  new int[] { 49, 56, 61 }, new int[] { 49, 51, 54, 56, 58, 61, 66 }),
            new HarmonicStage("Stage -2: D#m [Low Zen]",       new int[] { 51, 58, 63 }, new int[] { 51, 54, 56, 58, 61, 63, 66 }),
            new HarmonicStage("Stage -1: F#sus2 [Low Mid]",    new int[] { 54, 61, 66 }, new int[] { 54, 56, 58, 61, 63, 66, 70 }),
            new HarmonicStage("Stage  0: Tonic F# [Zen]",      new int[] { 54, 58, 61 }, new int[] { 54, 58, 61, 63, 66, 70, 73 }),
            new HarmonicStage("Stage +1: G#m [Ascending]",     new int[] { 56, 63, 68 }, new int[] { 56, 58, 61, 63, 66, 68, 73 }),
            new HarmonicStage("Stage +2: A#m [Ascending]",     new int[] { 58, 65, 70 }, new int[] { 58, 61, 63, 66, 68, 70, 75 }),
            new HarmonicStage("Stage +3: C#sus2 [Clear Air]",  new int[] { 61, 68, 73 }, new int[] { 61, 63, 66, 68, 70, 73, 78 }),
            new HarmonicStage("Stage +4: D#m [High Shimmer]",  new int[] { 63, 70, 75 }, new int[] { 63, 66, 68, 70, 73, 75, 78 }),
            new HarmonicStage("Stage +5: F#sus2 [High Glass]", new int[] { 66, 73, 78 }, new int[] { 66, 68, 70, 73, 75, 78, 82 }),
            new HarmonicStage("Stage +6: G#m [Sky Temple]",    new int[] { 68, 75, 80 }, new int[] { 68, 70, 73, 75, 78, 80, 85 })
        };

        // ==========================================================================
        // 6. QUARTAL HARMONY (MCCOY TYNER / MODERN JAZZ FOURTHS)
        // --------------------------------------------------------------------------
        // VIBE & FEEL: Modern Acoustic Concert Grand, Open, Glassy, Sophisticated.
        // PSYCHOLOGY: Stacked in perfect fourths (D-G-C, E-A-D) like McCoy Tyner in 
        //             A Love Supreme. Neither cheesy major nor depressive minor; 
        //             it creates a sleek, high-tech architectural acoustic space.
        // ==========================================================================
        private readonly HarmonicStage[] quartalLadder = new HarmonicStage[]
        {
            new HarmonicStage("Stage -6: E-4th [Deep Glass]",  new int[] { 40, 45, 50 }, new int[] { 40, 43, 47, 50, 52, 55, 57 }),
            new HarmonicStage("Stage -5: F-4th [Low Stack]",   new int[] { 41, 47, 52 }, new int[] { 41, 45, 48, 52, 53, 57, 60 }),
            new HarmonicStage("Stage -4: G-4th [Low Stack]",   new int[] { 43, 48, 53 }, new int[] { 43, 47, 50, 53, 55, 59, 62 }),
            new HarmonicStage("Stage -3: A-4th [Low Stack]",   new int[] { 45, 50, 55 }, new int[] { 45, 48, 52, 55, 57, 60, 64 }),
            new HarmonicStage("Stage -2: B-4th [Low Stack]",   new int[] { 47, 52, 57 }, new int[] { 47, 50, 53, 57, 59, 62, 65 }),
            new HarmonicStage("Stage -1: C-4th [Low Center]",  new int[] { 48, 53, 59 }, new int[] { 48, 52, 55, 59, 60, 64, 67 }),
            new HarmonicStage("Stage  0: D-4th [Quartal Home]",new int[] { 50, 55, 60 }, new int[] { 50, 53, 57, 60, 62, 65, 69 }),
            new HarmonicStage("Stage +1: E-4th [Ascending]",   new int[] { 52, 57, 62 }, new int[] { 52, 55, 59, 62, 64, 67, 71 }),
            new HarmonicStage("Stage +2: F-4th [Ascending]",   new int[] { 53, 59, 64 }, new int[] { 53, 57, 60, 64, 65, 69, 72 }),
            new HarmonicStage("Stage +3: G-4th [Airy Lift]",   new int[] { 55, 60, 65 }, new int[] { 55, 59, 62, 65, 67, 71, 74 }),
            new HarmonicStage("Stage +4: A-4th [High Modern]", new int[] { 57, 62, 67 }, new int[] { 57, 60, 64, 67, 69, 72, 76 }),
            new HarmonicStage("Stage +5: B-4th [High Crystal]",new int[] { 59, 64, 69 }, new int[] { 59, 62, 65, 69, 71, 74, 77 }),
            new HarmonicStage("Stage +6: C-4th [Sky Climax]",  new int[] { 60, 65, 71 }, new int[] { 60, 64, 67, 71, 72, 76, 79 })
        };
        #endregion

        #region Internal State & Rhythmic Accumulator Variables
        private Series<double> deltHist;
        private Series<double> momV;
        private Series<double> trSeries;
        private Series<double> stateCodeSeries;
        private Series<double> signalCodeSeries;

        private int warmupBars;
        private double lastPrice = 0.0;
        private int lastDirection = 0;
        private int lastVehicleState = 0;
        private int lastSignalCode = 0;
        private int currentLadderIndex = 6;
        private int melodyStepIndex = 3;

        private DateTime lastBeatPlayTime = DateTime.MinValue;
        private DateTime lastStageChangeTime = DateTime.MinValue;
        private long accumulatedBeatVolume = 0;
        private int accumulatedNetTicks = 0;
        private double peakGasInBeat = 0;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = "TheBluesTraderV2: Keyboard-slider order flow sonifier with pure consonant diatonic & quartal modes.";
                Name                        = "TheBluesTraderV2";
                Calculate                   = Calculate.OnEachTick;
                IsOverlay                   = false;
                DisplayInDataBox            = true;
                DrawOnPricePanel            = false;
                PaintPriceMarkers           = false;
                ScaleJustification          = ScaleJustification.Right;
                IsSuspendedWhileInactive    = true;

                // 1. Tape / Session Defaults
                TapeLen                     = 2;
                SessionStart                = new TimeSpan(18, 0, 0);

                // 2. Vehicle Dynamics Defaults
                ThrottleGain                = 1.0;
                Drag                        = 0.20;
                GasThresh                   = 10.0;
                CruiseBand                  = 1.5;
                CoastThresh                 = 3.0;
                StrongLevel                 = 40.0;

                // 3. Price Velocity Defaults
                VelLen                      = 2;
                AtrLen                      = 7;
                VelScale                    = 100.0;
                MoveThresh                  = 2.0;

                // 4. Musical & Rhythmic Engine Defaults
                EnableAudio                 = true;
                MidiInstrument              = 28; // Electric Guitar (Clean)
                ProgressionStyle            = BluesProgressionStyle.Dorian;
                VoicingMode                 = BluesVoicingMode.DynamicEventsAndMelody;
                RhythmPaceMs                = 450; // Musical tempo grid (450ms ≈ 133 BPM)
                SyncopateMajorEvents        = true;
                ChordDwellMs                = 600; // Stepwise transition rate per chord
                BaseVelocity                = 80;
                MinVolumeFilter             = 1;

                // Plots
                AddPlot(new Stroke(Brushes.Cyan, 3), PlotStyle.Line, "Momentum");
                AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "Zero");
                AddPlot(new Stroke(Brushes.DarkGreen, DashStyleHelper.Dash, 1), PlotStyle.Line, "StrongUp");
                AddPlot(new Stroke(Brushes.DarkRed, DashStyleHelper.Dash, 1), PlotStyle.Line, "StrongDn");
            }
            else if (State == State.DataLoaded)
            {
                deltHist        = new Series<double>(this);
                momV            = new Series<double>(this);
                trSeries        = new Series<double>(this);
                stateCodeSeries = new Series<double>(this);
                signalCodeSeries= new Series<double>(this);

                warmupBars = Math.Max(AtrLen, VelLen) + 2;
                lastPrice = 0.0;
                lastDirection = 0;
                lastVehicleState = 0;
                lastSignalCode = 0;
                currentLadderIndex = 6;
                melodyStepIndex = 3;

                accumulatedBeatVolume = 0;
                accumulatedNetTicks = 0;
                peakGasInBeat = 0;
                lastBeatPlayTime = DateTime.MinValue;
                lastStageChangeTime = DateTime.MinValue;

                if (EnableAudio)
                {
                    int result = midiOutOpen(out midiHandle, -1, IntPtr.Zero, IntPtr.Zero, 0);
                    if (result == 0 && midiHandle != IntPtr.Zero)
                    {
                        int programChangeMsg = 0xC0 | ((MidiInstrument & 0x7F) << 8);
                        midiOutShortMsg(midiHandle, programChangeMsg);
                    }
                }
            }
            else if (State == State.Terminated)
            {
                if (midiHandle != IntPtr.Zero)
                {
                    midiOutShortMsg(midiHandle, 0x007BB0); // All Notes Off
                    midiOutClose(midiHandle);
                    midiHandle = IntPtr.Zero;
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < warmupBars)
                return;

            // ==========================================================================
            // (1) TAPE GAS (Net Delta %)
            // ==========================================================================
            double rng = High[0] - Low[0];
            double delt = rng <= 0 ? 0 : Volume[0] * ((Close[0] - Low[0]) - (High[0] - Close[0])) / rng;
            deltHist[0] = delt;

            double recentCVD = 0;
            double recentVol = 0;
            int tapeWindow = Math.Min(TapeLen, CurrentBar + 1);
            for (int k = 0; k < tapeWindow; k++)
            {
                recentCVD += deltHist[k];
                recentVol += Volume[k];
            }
            double tapeScore = 100.0 * recentCVD / Math.Max(recentVol, G);

            bool newSess = IsSessionBoundary(Time[0], Time[1], CurrentBar >= 1, SessionStart);

            // ==========================================================================
            // (2) MOMENTUM
            // ==========================================================================
            double prevMomV = momV[1];
            double currentMomV = newSess
                ? (ThrottleGain * tapeScore)
                : (prevMomV + Drag * (ThrottleGain * tapeScore - prevMomV));

            momV[0] = currentMomV;
            double accV = currentMomV - prevMomV;
            double netForce = ThrottleGain * tapeScore - currentMomV;

            // ==========================================================================
            // (3) PRICE VELOCITY (ATR-normalized)
            // ==========================================================================
            double prevClose = Close[1];
            double tr = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - prevClose), Math.Abs(Low[0] - prevClose)));
            trSeries[0] = tr;

            double trSum = 0;
            int atrWindow = Math.Min(AtrLen, CurrentBar + 1);
            for (int k = 0; k < atrWindow; k++)
                trSum += trSeries[k];

            double atrv = trSum / Math.Max(atrWindow, 1);
            double closeDiff = Close[0] - Close[VelLen];
            double pVelV = VelScale * closeDiff / Math.Max(atrv * VelLen, G);

            // ==========================================================================
            // (4) VEHICLE STATE + TRACTION
            // ==========================================================================
            bool gasOn      = Math.Abs(tapeScore) >= GasThresh;
            bool moving     = Math.Abs(currentMomV) > CoastThresh;
            bool sameDir    = (tapeScore > 0 && currentMomV > 0) || (tapeScore < 0 && currentMomV < 0);
            bool braking    = moving && ((tapeScore > 0 && currentMomV < 0) || (tapeScore < 0 && currentMomV > 0));
            bool building   = sameDir && (accV * currentMomV) > 0 && Math.Abs(accV) > CruiseBand;

            int state = 0;
            if (braking) state = 3;
            else if (!gasOn && moving) state = 2;
            else if (gasOn && building) state = 1;
            else state = 0;

            stateCodeSeries[0] = state;

            bool gasStrong  = Math.Abs(tapeScore) >= StrongLevel;
            bool priceRolls = Math.Abs(pVelV) >= MoveThresh;
            bool spinning   = gasStrong && !priceRolls; // Absorption
            bool freeRoll   = !gasOn && priceRolls;     // Drift

            // ==========================================================================
            // (5) SIGNAL CLASSIFIER
            // ==========================================================================
            int baseSig = 0;
            if (state == 1 && currentMomV > 0) baseSig = 1;
            else if (state == 1 && currentMomV < 0) baseSig = -1;
            else if (state == 3 && currentMomV > 0) baseSig = -2;
            else if (state == 3 && currentMomV < 0) baseSig = 2;
            else if (state == 2) baseSig = 3;
            else baseSig = 0;

            int absorptionSig = tapeScore > 0 ? -2 : 2;
            int finalSig = 0;
            if (spinning) finalSig = absorptionSig;
            else if (freeRoll) finalSig = 4;
            else finalSig = baseSig;

            signalCodeSeries[0] = finalSig;

            // ==========================================================================
            // (6) HARMONIC ENGINE & STEPWISE HARMONIC SLEW LIMITER
            // ==========================================================================
            double tickSize = (Instrument != null && Instrument.MasterInstrument != null && Instrument.MasterInstrument.TickSize > 0)
                ? Instrument.MasterInstrument.TickSize
                : 0.25;

            double currentPrice = Close[0];
            int ticksMoved = 0;
            int direction = 0;

            if (lastPrice != 0.0)
            {
                if (currentPrice > lastPrice)
                {
                    direction = 1;
                    ticksMoved = (int)Math.Max(1, Math.Round((currentPrice - lastPrice) / tickSize));
                }
                else if (currentPrice < lastPrice)
                {
                    direction = -1;
                    ticksMoved = (int)Math.Max(1, Math.Round((lastPrice - currentPrice) / tickSize));
                }
            }

            // ACCUMULATE TICK ENERGY
            accumulatedBeatVolume += (long)Volume[0];
            accumulatedNetTicks += (direction * ticksMoved);
            if (Math.Abs(tapeScore) > Math.Abs(peakGasInBeat))
                peakGasInBeat = tapeScore;

            // COMPOSITE ORDER FLOW SCORE
            double compositeFlow = (0.50 * tapeScore) + (0.35 * currentMomV) + (0.15 * pVelV);
            double flowPerStage = StrongLevel / 3.0; // ~13.3 pts per stage
            int targetStage = (int)Math.Round(compositeFlow / Math.Max(1.0, flowPerStage));
            int targetIndex = Math.Max(0, Math.Min(12, targetStage + 6));

            // Select active ladder based on chosen style
            HarmonicStage[] activeLadder = dorianLadder;
            switch (ProgressionStyle)
            {
                case BluesProgressionStyle.Diatonic:
                    activeLadder = diatonicLadder;
                    break;
                case BluesProgressionStyle.Dorian:
                    activeLadder = dorianLadder;
                    break;
                case BluesProgressionStyle.Aeolian:
                    activeLadder = aeolianLadder;
                    break;
                case BluesProgressionStyle.Mixolydian:
                    activeLadder = mixolydianLadder;
                    break;
                case BluesProgressionStyle.BlackKeysPentatonic:
                    activeLadder = blackKeysLadder;
                    break;
                case BluesProgressionStyle.QuartalHarmony:
                    activeLadder = quartalLadder;
                    break;
            }

            // STEPWISE HARMONIC WALK (NO TELEPORTING)
            DateTime now = DateTime.UtcNow;
            if (targetIndex != currentLadderIndex)
            {
                bool hasDwellExpired = (now - lastStageChangeTime).TotalMilliseconds >= ChordDwellMs;

                if (hasDwellExpired)
                {
                    if (targetIndex > currentLadderIndex)
                        currentLadderIndex++;
                    else if (targetIndex < currentLadderIndex)
                        currentLadderIndex--;

                    lastStageChangeTime = now;
                    melodyStepIndex = activeLadder[currentLadderIndex].MelodyScale.Length / 2;
                }
            }

            HarmonicStage activeStage = activeLadder[currentLadderIndex];

            // ==========================================================================
            // (7) RHYTHMIC QUANTIZATION CLOCK & AUDIO TRIGGER
            // ==========================================================================
            double elapsedMs = (now - lastBeatPlayTime).TotalMilliseconds;
            bool isMajorEvent = (state != lastVehicleState)
                || (finalSig != lastSignalCode && finalSig != 0)
                || spinning
                || (state == 3)
                || (Math.Abs(accumulatedNetTicks) >= 3);

            bool timeForGridBeat = elapsedMs >= RhythmPaceMs;
            bool syncopatedHit = SyncopateMajorEvents && isMajorEvent && (elapsedMs >= (RhythmPaceMs * 0.45));

            if (State == State.Realtime && EnableAudio && midiHandle != IntPtr.Zero && accumulatedBeatVolume >= MinVolumeFilter)
            {
                if (timeForGridBeat || syncopatedHit)
                {
                    PlayQuantizedBeat(state, finalSig, spinning, freeRoll, peakGasInBeat, accumulatedBeatVolume, accumulatedNetTicks, activeStage, isMajorEvent);

                    accumulatedBeatVolume = 0;
                    accumulatedNetTicks = 0;
                    peakGasInBeat = 0;
                    lastBeatPlayTime = now;
                }
            }

            lastPrice = currentPrice;
            if (direction != 0) lastDirection = direction;
            lastVehicleState = state;
            lastSignalCode = finalSig;

            // ==========================================================================
            // (8) PLOTS & COLORS
            // ==========================================================================
            Values[0][0] = currentMomV;
            Values[1][0] = 0;
            Values[2][0] = StrongLevel;
            Values[3][0] = -StrongLevel;

            Brush momBrush;
            if (state == 3) momBrush = Brushes.Red;
            else if (state == 2) momBrush = Brushes.Orange;
            else if (state == 1) momBrush = Brushes.LimeGreen;
            else momBrush = Brushes.Cyan;

            PlotBrushes[0][0] = momBrush;

            // ==========================================================================
            // (9) HUD DASHBOARD
            // ==========================================================================
            string line1State = "CRUISING";
            if (state == 3) line1State = "BRAKING";
            else if (state == 2) line1State = "COASTING";
            else if (state == 1) line1State = "ACCELERATING";

            string line1 = string.Format(
                "{0}  gas {1}  mom {2}  accel {3}{4}  force {5}{6}  | pVel {7}{8}{9}",
                line1State,
                Math.Round(tapeScore, 0),
                Math.Round(currentMomV, 0),
                (accV > 0 ? "+" : ""), Math.Round(accV, 1),
                (netForce > 0 ? "+" : ""), Math.Round(netForce, 0),
                (pVelV > 0 ? "+" : ""), Math.Round(pVelV, 0),
                (spinning ? "  [ABSORPTION]" : (freeRoll ? "  [MOMENTUM DRIFT]" : ""))
            );

            string signalText = "HOLD";
            if (finalSig == 1) signalText = "LONG";
            else if (finalSig == -1) signalText = "SHORT";
            else if (finalSig == 2) signalText = "FADE: BUY";
            else if (finalSig == -2) signalText = "FADE: SELL";
            else if (finalSig == 3) signalText = "REDUCE";
            else if (finalSig == 4) signalText = "AVOID";

            string line2 = string.Format("SIGNAL: {0}  |  FLOW SCORE: {1:+0;-0;0} ({2})",
                signalText,
                Math.Round(compositeFlow, 0),
                compositeFlow > 5 ? "BULLISH" : (compositeFlow < -5 ? "BEARISH" : "NEUTRAL")
            );

            int bpm = (int)Math.Round(60000.0 / Math.Max(50, RhythmPaceMs));
            string line3 = string.Format("PIANO FLOW: {0}  |  Scale: {1}  |  Tempo: {2}ms ({3} BPM)",
                activeStage.Name,
                ProgressionStyle,
                RhythmPaceMs,
                bpm
            );

            string fullLabelText = line1 + "\n" + line2 + "\n" + line3;

            Brush labelBrush = Brushes.Cyan;
            if (finalSig == 1) labelBrush = Brushes.LimeGreen;
            else if (finalSig == -1) labelBrush = Brushes.IndianRed;
            else if (finalSig == 2 || finalSig == -2) labelBrush = Brushes.Magenta;
            else if (finalSig == 3) labelBrush = Brushes.Orange;
            else if (finalSig == 4) labelBrush = Brushes.Gray;

            Draw.TextFixed(this, "TheBluesTraderV2HUD", fullLabelText, TextPosition.TopLeft,
                labelBrush, new SimpleFont("Arial", 11), Brushes.Transparent, Brushes.Transparent, 0);
        }

        #region Quantized Audio Output Engine
        private void PlayQuantizedBeat(int state, int finalSig, bool spinning, bool freeRoll,
            double peakGas, long windowVolume, int netTicksInWindow, HarmonicStage stage, bool isMajorEvent)
        {
            double absGas = Math.Min(100.0, Math.Abs(peakGas));
            int dynamicVelocity = (int)Math.Max(42, Math.Min(120, BaseVelocity + (absGas * 0.35)));

            if (windowVolume > 50)
                dynamicVelocity = Math.Min(127, dynamicVelocity + 10);

            // 1. ABSORPTION: Rhythmic punchy double-accent on the active chord's guide tones
            if (spinning)
            {
                int root = stage.ChordNotes[0];
                int fifth = stage.ChordNotes.Length > 1 ? stage.ChordNotes[1] : root + 7;
                SendMidiNote(root, Math.Min(127, dynamicVelocity + 15));
                SendMidiNote(fifth, dynamicVelocity);
                return;
            }

            // 2. MOMENTUM DRIFT: Soft high-register chime note
            if (freeRoll)
            {
                int chimeNote = stage.MelodyScale[stage.MelodyScale.Length - 1];
                SendMidiNote(chimeNote, Math.Max(35, dynamicVelocity - 30));
                return;
            }

            // 3. BRAKING: Tight, percussive muted bass choke in the active key
            if (state == 3)
            {
                SendMidiNote(stage.ChordNotes[0], dynamicVelocity);
                if (stage.ChordNotes.Length > 2)
                    SendMidiNote(stage.ChordNotes[2], Math.Max(35, dynamicVelocity - 10));
                return;
            }

            // 4. CHORD VOICING vs STEPWISE MELODY ON THE BEAT
            if (VoicingMode == BluesVoicingMode.PrimarilyChords || isMajorEvent)
            {
                int notesToPlay = (Math.Abs(netTicksInWindow) > 1 || isMajorEvent)
                    ? stage.ChordNotes.Length
                    : Math.Min(4, stage.ChordNotes.Length);

                for (int i = 0; i < notesToPlay; i++)
                {
                    int note = stage.ChordNotes[i];
                    int noteVel = Math.Max(35, dynamicVelocity - (i * 5));
                    SendMidiNote(note, noteVel);
                }

                melodyStepIndex = Math.Min(stage.MelodyScale.Length - 1, 3);
            }
            else
            {
                // QUANTIZED MELODY SOLO
                int scaleLen = stage.MelodyScale.Length;

                if (netTicksInWindow > 0)
                {
                    int steps = Math.Min(2, netTicksInWindow);
                    melodyStepIndex = Math.Min(scaleLen - 1, melodyStepIndex + steps);
                }
                else if (netTicksInWindow < 0)
                {
                    int steps = Math.Min(2, Math.Abs(netTicksInWindow));
                    melodyStepIndex = Math.Max(0, melodyStepIndex - steps);
                }

                int melodicNote = stage.MelodyScale[melodyStepIndex];
                SendMidiNote(melodicNote, dynamicVelocity);
            }
        }

        private void SendMidiNote(int pitch, int velocity)
        {
            if (midiHandle == IntPtr.Zero) return;
            pitch = Math.Max(0, Math.Min(127, pitch));
            velocity = Math.Max(0, Math.Min(127, velocity));

            int msg = 0x90 | (pitch << 8) | (velocity << 16);
            midiOutShortMsg(midiHandle, msg);
        }
        #endregion

        private bool IsSessionBoundary(DateTime cur, DateTime prev, bool hasPrev, TimeSpan anchor)
        {
            double curSec = (cur.TimeOfDay - anchor).TotalSeconds;
            double prevSec = hasPrev ? (prev.TimeOfDay - anchor).TotalSeconds : -1;
            return curSec == 0 || (curSec >= 0 && prevSec < 0);
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Tape Length", Order = 1, GroupName = "1. Tape / Session")]
        public int TapeLen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session Start", Order = 2, GroupName = "1. Tape / Session")]
        public TimeSpan SessionStart { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Throttle Gain", Order = 1, GroupName = "2. Vehicle Dynamics")]
        public double ThrottleGain { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Drag", Order = 2, GroupName = "2. Vehicle Dynamics")]
        public double Drag { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Gas Thresh", Order = 3, GroupName = "2. Vehicle Dynamics")]
        public double GasThresh { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Cruise Band", Order = 4, GroupName = "2. Vehicle Dynamics")]
        public double CruiseBand { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Coast Thresh", Order = 5, GroupName = "2. Vehicle Dynamics")]
        public double CoastThresh { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Strong Level", Order = 6, GroupName = "2. Vehicle Dynamics")]
        public double StrongLevel { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vel Length", Order = 1, GroupName = "3. Price Velocity")]
        public int VelLen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATR Length", Order = 2, GroupName = "3. Price Velocity")]
        public int AtrLen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vel Scale", Order = 3, GroupName = "3. Price Velocity")]
        public double VelScale { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Move Thresh", Order = 4, GroupName = "3. Price Velocity")]
        public double MoveThresh { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Audio", Description = "Toggle MIDI audio playback", Order = 1, GroupName = "4. The Blues Trader Audio")]
        public bool EnableAudio { get; set; }

        [NinjaScriptProperty]
        [Range(0, 127)]
        [Display(Name = "MIDI Instrument", Description = "General MIDI patch (28 = Clean Electric Guitar, 0 = Grand Piano, 16 = Drawbar Organ)", Order = 2, GroupName = "4. The Blues Trader Audio")]
        public int MidiInstrument { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Progression Style", Description = "Harmonic scale world: Diatonic, Dorian, Aeolian, Mixolydian, BlackKeysPentatonic, QuartalHarmony", Order = 3, GroupName = "4. The Blues Trader Audio")]
        public BluesProgressionStyle ProgressionStyle { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Voicing Mode", Description = "Dynamic melody notes vs full chord voicings", Order = 4, GroupName = "4. The Blues Trader Audio")]
        public BluesVoicingMode VoicingMode { get; set; }

        [NinjaScriptProperty]
        [Range(100, 2000)]
        [Display(Name = "Rhythm Pace (ms)", Description = "Musical beat tempo in milliseconds (e.g. 450ms ≈ 133 BPM, 600ms = 100 BPM)", Order = 5, GroupName = "4. The Blues Trader Audio")]
        public int RhythmPaceMs { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Syncopate Major Events", Description = "Allow state changes or breakouts to strike early on syncopated off-beats", Order = 6, GroupName = "4. The Blues Trader Audio")]
        public bool SyncopateMajorEvents { get; set; }

        [NinjaScriptProperty]
        [Range(100, 5000)]
        [Display(Name = "Chord Dwell (ms)", Description = "Time between stepwise chord transitions (controls turnaround speed)", Order = 7, GroupName = "4. The Blues Trader Audio")]
        public int ChordDwellMs { get; set; }

        [NinjaScriptProperty]
        [Range(1, 127)]
        [Display(Name = "Base Velocity", Description = "Base strike loudness (0 - 127)", Order = 8, GroupName = "4. The Blues Trader Audio")]
        public int BaseVelocity { get; set; }

        [NinjaScriptProperty]
        [Range(1, long.MaxValue)]
        [Display(Name = "Min Volume Filter", Description = "Ignore trade executions below this volume", Order = 9, GroupName = "4. The Blues Trader Audio")]
        public long MinVolumeFilter { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Momentum { get { return Values[0]; } }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Zero { get { return Values[1]; } }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StrongUp { get { return Values[2]; } }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StrongDn { get { return Values[3]; } }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StateCode { get { return stateCodeSeries; } }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SignalCode { get { return signalCodeSeries; } }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TheBluesTraderV2[] cacheTheBluesTraderV2;
		public TheBluesTraderV2 TheBluesTraderV2(int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			return TheBluesTraderV2(Input, tapeLen, sessionStart, throttleGain, drag, gasThresh, cruiseBand, coastThresh, strongLevel, velLen, atrLen, velScale, moveThresh, enableAudio, midiInstrument, progressionStyle, voicingMode, rhythmPaceMs, syncopateMajorEvents, chordDwellMs, baseVelocity, minVolumeFilter);
		}

		public TheBluesTraderV2 TheBluesTraderV2(ISeries<double> input, int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			if (cacheTheBluesTraderV2 != null)
				for (int idx = 0; idx < cacheTheBluesTraderV2.Length; idx++)
					if (cacheTheBluesTraderV2[idx] != null && cacheTheBluesTraderV2[idx].TapeLen == tapeLen && cacheTheBluesTraderV2[idx].SessionStart == sessionStart && cacheTheBluesTraderV2[idx].ThrottleGain == throttleGain && cacheTheBluesTraderV2[idx].Drag == drag && cacheTheBluesTraderV2[idx].GasThresh == gasThresh && cacheTheBluesTraderV2[idx].CruiseBand == cruiseBand && cacheTheBluesTraderV2[idx].CoastThresh == coastThresh && cacheTheBluesTraderV2[idx].StrongLevel == strongLevel && cacheTheBluesTraderV2[idx].VelLen == velLen && cacheTheBluesTraderV2[idx].AtrLen == atrLen && cacheTheBluesTraderV2[idx].VelScale == velScale && cacheTheBluesTraderV2[idx].MoveThresh == moveThresh && cacheTheBluesTraderV2[idx].EnableAudio == enableAudio && cacheTheBluesTraderV2[idx].MidiInstrument == midiInstrument && cacheTheBluesTraderV2[idx].ProgressionStyle == progressionStyle && cacheTheBluesTraderV2[idx].VoicingMode == voicingMode && cacheTheBluesTraderV2[idx].RhythmPaceMs == rhythmPaceMs && cacheTheBluesTraderV2[idx].SyncopateMajorEvents == syncopateMajorEvents && cacheTheBluesTraderV2[idx].ChordDwellMs == chordDwellMs && cacheTheBluesTraderV2[idx].BaseVelocity == baseVelocity && cacheTheBluesTraderV2[idx].MinVolumeFilter == minVolumeFilter && cacheTheBluesTraderV2[idx].EqualsInput(input))
						return cacheTheBluesTraderV2[idx];
			return CacheIndicator<TheBluesTraderV2>(new TheBluesTraderV2(){ TapeLen = tapeLen, SessionStart = sessionStart, ThrottleGain = throttleGain, Drag = drag, GasThresh = gasThresh, CruiseBand = cruiseBand, CoastThresh = coastThresh, StrongLevel = strongLevel, VelLen = velLen, AtrLen = atrLen, VelScale = velScale, MoveThresh = moveThresh, EnableAudio = enableAudio, MidiInstrument = midiInstrument, ProgressionStyle = progressionStyle, VoicingMode = voicingMode, RhythmPaceMs = rhythmPaceMs, SyncopateMajorEvents = syncopateMajorEvents, ChordDwellMs = chordDwellMs, BaseVelocity = baseVelocity, MinVolumeFilter = minVolumeFilter }, input, ref cacheTheBluesTraderV2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TheBluesTraderV2 TheBluesTraderV2(int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			return indicator.TheBluesTraderV2(Input, tapeLen, sessionStart, throttleGain, drag, gasThresh, cruiseBand, coastThresh, strongLevel, velLen, atrLen, velScale, moveThresh, enableAudio, midiInstrument, progressionStyle, voicingMode, rhythmPaceMs, syncopateMajorEvents, chordDwellMs, baseVelocity, minVolumeFilter);
		}

		public Indicators.TheBluesTraderV2 TheBluesTraderV2(ISeries<double> input , int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			return indicator.TheBluesTraderV2(input, tapeLen, sessionStart, throttleGain, drag, gasThresh, cruiseBand, coastThresh, strongLevel, velLen, atrLen, velScale, moveThresh, enableAudio, midiInstrument, progressionStyle, voicingMode, rhythmPaceMs, syncopateMajorEvents, chordDwellMs, baseVelocity, minVolumeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TheBluesTraderV2 TheBluesTraderV2(int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			return indicator.TheBluesTraderV2(Input, tapeLen, sessionStart, throttleGain, drag, gasThresh, cruiseBand, coastThresh, strongLevel, velLen, atrLen, velScale, moveThresh, enableAudio, midiInstrument, progressionStyle, voicingMode, rhythmPaceMs, syncopateMajorEvents, chordDwellMs, baseVelocity, minVolumeFilter);
		}

		public Indicators.TheBluesTraderV2 TheBluesTraderV2(ISeries<double> input , int tapeLen, TimeSpan sessionStart, double throttleGain, double drag, double gasThresh, double cruiseBand, double coastThresh, double strongLevel, int velLen, int atrLen, double velScale, double moveThresh, bool enableAudio, int midiInstrument, BluesProgressionStyle progressionStyle, BluesVoicingMode voicingMode, int rhythmPaceMs, bool syncopateMajorEvents, int chordDwellMs, int baseVelocity, long minVolumeFilter)
		{
			return indicator.TheBluesTraderV2(input, tapeLen, sessionStart, throttleGain, drag, gasThresh, cruiseBand, coastThresh, strongLevel, velLen, atrLen, velScale, moveThresh, enableAudio, midiInstrument, progressionStyle, voicingMode, rhythmPaceMs, syncopateMajorEvents, chordDwellMs, baseVelocity, minVolumeFilter);
		}
	}
}

#endregion
