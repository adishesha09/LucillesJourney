using System.Globalization;
using System.Text;

namespace AudioGen;

enum Ins { Piano, Pluck }

record Track(Ins I, double BlockBeats, string[] Blocks, double Gain, double Humanize = 0.012);

sealed class Piece
{
    public string Name;
    public double Bpm;
    public int Bars;
    public (string Notes, double Beats)[] Chords = Array.Empty<(string, double)>();
    public List<Track> Tracks = new();
    public double PadGain = 1, PadBright = 1500, PadAttack = 2.0, PadRev = 0.25;
    public double RevRt = 2.6, RevWet = 0.45, Target = -9, InstLp = 4500;
    public Func<double, double> Energy = _ => 1.0;
    public Func<int, float[]> Drone;
    public double DroneGain = 1;
}

static class Music
{
    const int Sr = Dsp.MusicSr;

    public static List<(string name, float[] data)> RenderAll()
    {
        var list = new List<(string, float[])>();
        foreach (var p in Pieces()) list.Add((p.Name, Render(p)));
        return list;
    }

    static float[] Render(Piece p)
    {
        var rng = Dsp.Rng(p.Name);
        double bs = 60.0 / p.Bpm;
        int n = (int)Math.Round(p.Bars * 4 * bs * Sr);
        double total = p.Bars * 4;

        var pad = new float[n];
        double beat = 0;
        foreach (var (notes, beats) in p.Chords)
        {
            var freqs = notes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Inst.Freq).ToArray();
            double e = p.Energy(beat / total);
            var v = Inst.Pad(freqs, beats * bs, p.PadAttack, p.PadAttack, p.PadBright, Sr, rng, e);
            Dsp.AddCirc(pad, (beat * bs - p.PadAttack / 2) * Sr, v);
            beat += beats;
        }
        if (p.Chords.Length > 0 && Math.Abs(beat - total) > 1e-9)
            throw new Exception($"{p.Name}: chords cover {beat} beats, expected {total}");

        var inst = new float[n];
        foreach (var tr in p.Tracks)
        {
            for (int bi = 0; bi < tr.Blocks.Length; bi++)
            {
                foreach (var tok in tr.Blocks[bi].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var f = tok.Split(':');
                    double b0 = double.Parse(f[0], CultureInfo.InvariantCulture) + bi * tr.BlockBeats;
                    double freq = Inst.Freq(f[1]);
                    double dur = double.Parse(f[2], CultureInfo.InvariantCulture) * bs;
                    double vel = f.Length > 3 ? double.Parse(f[3], CultureInfo.InvariantCulture) : 0.6;
                    vel *= p.Energy(b0 / total) * tr.Gain * (1 + (rng.NextDouble() - 0.5) * 0.16);
                    double t = b0 * bs + (rng.NextDouble() - 0.5) * 2 * tr.Humanize;
                    var note = tr.I == Ins.Piano ? Inst.Piano(freq, dur, vel, Sr, rng) : Inst.Pluck(freq, dur, vel, Sr, rng);
                    Dsp.AddCirc(inst, t * Sr, note);
                }
            }
        }

        var instLp = Dsp.Circ(inst, t => Dsp.OnePoleLP(t, p.InstLp, Sr));
        var mix = new float[n];
        var instWet = p.Tracks.Count > 0 ? Dsp.Reverb(instLp, Sr, p.RevRt) : new float[n];
        var padWet = p.Chords.Length > 0 && p.PadRev > 0 ? Dsp.Reverb(pad, Sr, p.RevRt) : new float[n];
        float[] drone = p.Drone?.Invoke(n);
        for (int i = 0; i < n; i++)
        {
            mix[i] = (float)(pad[i] * p.PadGain + instLp[i] + instWet[i] * p.RevWet + padWet[i] * p.PadRev * p.PadGain);
            if (drone != null) mix[i] += (float)(drone[i] * p.DroneGain);
        }
        Console.WriteLine($"  [{p.Name}] mix rms pad {Db(Dsp.Rms(pad) * (float)p.PadGain)} inst {Db(Dsp.Rms(instLp))} drone {(drone == null ? "-" : Db(Dsp.Rms(drone) * (float)p.DroneGain))} dB");
        mix = Dsp.CircBiquad(mix, BQ.HP, 28, 0.7, Sr);
        mix = Dsp.CircBiquad(mix, BQ.LP, 6500, 0.6, Sr);
        return Dsp.Normalize(mix, p.Target);
    }

    static string Db(float v) => (20 * Math.Log10(Math.Max(v, 1e-9))).ToString("0.0", CultureInfo.InvariantCulture);

    static (string, double)[] Chords8(params string[] chords) => chords.Select(c => (c, 8.0)).ToArray();

    static string Arp(Random r, string[] pool, int[] patA, int[] patB, double skip)
    {
        var sb = new StringBuilder();
        for (int s = 0; s < 32; s++)
        {
            if (s % 8 != 0 && r.NextDouble() < skip) continue;
            var pat = s < 16 ? patA : patB;
            double vel = (s % 2 == 0 ? 0.56 : 0.44) * (s % 8 == 0 ? 1.15 : 1.0);
            sb.Append(string.Create(CultureInfo.InvariantCulture, $"{s * 0.5}:{pool[pat[s % 8]]}:0.75:{vel} "));
        }
        return sb.ToString();
    }

    static IEnumerable<Piece> Pieces()
    {
        // ---------------- menu ----------------
        {
            var p = new Piece { Name = "music_menu", Bpm = 66, Bars = 16, PadGain = 0.9, PadBright = 1300, Target = -9, RevRt = 3.0, RevWet = 0.5 };
            p.Chords = Chords8("D3 A3 D4 F4", "Bb2 F3 A3 D4", "F3 C4 F4 A4", "C3 G3 D4 E4",
                               "D3 A3 D4 F4", "G2 D3 F3 Bb3", "Bb2 F3 D4 F4", "A2 E3 A3 D4");
            p.Tracks.Add(new Track(Ins.Piano, 8, new[] {
                "0.5:A4:2:0.55 3:F4:2:0.5 6:E4:2:0.5",
                "0:D4:2:0.5 2.5:F4:1.5:0.5 5:A4:3:0.55",
                "0:C5:2:0.55 3:A4:2:0.5 6:F4:2:0.5",
                "0.5:G4:2:0.5 3:E4:2:0.5 5.5:D4:2.5:0.5",
                "0:F4:2:0.5 2.5:A4:2:0.55 5:D5:3:0.55",
                "0:Bb4:2:0.55 3:A4:1.5:0.5 5:G4:3:0.5",
                "0:F4:2:0.5 3:D4:2:0.5 5.5:F4:2:0.5",
                "0:E4:2:0.5 2.5:D4:2:0.5 5:A3:3:0.5" }, 1.0));
            p.Tracks.Add(new Track(Ins.Piano, 8, new[] { "0:D3:6:0.4", "0:Bb2:6:0.4", "0:F3:6:0.4", "0:C3:6:0.4",
                "0:D3:6:0.4", "0:G2:6:0.4", "0:Bb2:6:0.4", "0:A2:6:0.4" }, 1.0));
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] { "4:D4:2:0.3", "", "4:A4:2:0.3", "", "4:D4:2:0.3", "", "4:F4:2:0.3", "" }, 1.0));
            yield return p;
        }
        // ---------------- ch1 ----------------
        {
            var p = new Piece { Name = "music_ch1", Bpm = 76, Bars = 16, PadGain = 0.75, PadBright = 1400, Target = -9, RevRt = 2.2, RevWet = 0.4 };
            p.Chords = Chords8("G3 D4 G4 B4", "E3 B3 E4 G4", "C3 G3 C4 E4", "D3 A3 D4 F#4",
                               "G3 D4 G4 B4", "A2 E3 A3 C4 G4", "C3 G3 C4 E4", "D3 A3 D4 F#4");
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] {
                "0:B4:1:0.6 1:D5:1:0.55 2:E5:2:0.6 4:D5:1:0.5 5:B4:1:0.5 6:A4:2:0.55",
                "0:G4:1:0.55 1:B4:1:0.5 2:A4:2:0.55 4:G4:1:0.5 5:E4:1:0.5 6:G4:2:0.55",
                "0:E4:1:0.55 1:G4:1:0.5 2:C5:2:0.6 4:B4:1:0.5 5:G4:1:0.5 6:A4:2:0.55",
                "0:F#4:1:0.55 1:A4:1:0.5 2:B4:2:0.6 4:A4:1:0.5 5:F#4:1:0.5 6:D4:2:0.55",
                "0:D5:1:0.6 1:B4:1:0.5 2:G4:2:0.55 4:A4:1:0.5 5:B4:1:0.5 6:D5:2:0.6",
                "0:C5:1:0.55 1:B4:1:0.5 2:A4:2:0.55 4:E4:1:0.5 5:G4:1:0.5 6:A4:2:0.55",
                "0:G4:1:0.55 1:E5:1:0.6 2:D5:2:0.55 4:C5:1:0.5 5:B4:1:0.5 6:G4:2:0.55",
                "0:A4:1:0.55 1:F#4:1:0.5 2:G4:2:0.55 4:A4:2:0.5 6:B4:2:0.6" }, 1.0));
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] { "0:G2:4:0.45 4:D3:4:0.4", "0:E3:4:0.45 4:B2:4:0.4", "0:C3:4:0.45 4:G2:4:0.4",
                "0:D3:4:0.45 4:A2:4:0.4", "0:G2:4:0.45 4:D3:4:0.4", "0:A2:4:0.45 4:E3:4:0.4", "0:C3:4:0.45 4:G2:4:0.4", "0:D3:4:0.45 4:A2:4:0.4" }, 1.0));
            yield return p;
        }
        // ---------------- ch2 ----------------
        {
            var r = Dsp.Rng("music_ch2_pattern");
            var p = new Piece { Name = "music_ch2", Bpm = 100, Bars = 20, PadGain = 0.55, PadBright = 1200, Target = -9, RevRt = 1.8, RevWet = 0.35 };
            p.Chords = new[] { ("D3 A3 D4 F4", 16.0), ("G3 B3 D4 G4", 16), ("Bb2 F3 A3 D4", 16), ("Eb3 Bb3 D4 G4", 16), ("A2 E3 A3 C#4", 16) };
            int[] a = { 0, 1, 2, 1, 3, 2, 1, 2 }, b = { 1, 2, 3, 2, 1, 2, 0, 1 };
            p.Tracks.Add(new Track(Ins.Pluck, 16, new[] {
                Arp(r, new[] { "D4", "F4", "A4", "D5" }, a, b, 0.2),
                Arp(r, new[] { "D4", "G4", "B4", "D5" }, a, b, 0.2),
                Arp(r, new[] { "D4", "F4", "A4", "Bb4" }, a, b, 0.2),
                Arp(r, new[] { "Eb4", "G4", "Bb4", "D5" }, a, b, 0.2),
                Arp(r, new[] { "E4", "A4", "C#5", "E5" }, a, b, 0.2) }, 0.7));
            p.Tracks.Add(new Track(Ins.Pluck, 16, new[] {
                "2:A4:2:0.7 9:F4:1.5:0.65 12:E4:3:0.65",
                "1:B4:2:0.7 6:D5:2:0.65 12:C5:3:0.65",
                "2:A4:2:0.7 8:F5:1.5:0.65 12:D5:3:0.65",
                "2:G4:2:0.7 6:D5:3:0.65 12:Bb4:3:0.65",
                "2:C#5:2:0.7 5:Bb4:0.75:0.55 6:A4:3:0.7 12:E4:3:0.65" }, 1.0));
            p.Tracks.Add(new Track(Ins.Pluck, 16, new[] { "0:D3:3:0.5 8:A2:3:0.45", "0:G2:3:0.5 8:D3:3:0.45", "0:Bb2:3:0.5 8:F3:3:0.45",
                "0:Eb3:3:0.5 8:Bb2:3:0.45", "0:A2:3:0.5 8:E3:3:0.45" }, 1.0));
            yield return p;
        }
        // ---------------- ch3 ----------------
        {
            var p = new Piece { Name = "music_ch3", Bpm = 48, Bars = 12, Target = -13, RevRt = 3.5, RevWet = 0.5, InstLp = 1200 };
            p.Tracks.Add(new Track(Ins.Piano, 8, new[] { "", "2:D3:5:0.32", "", "4:A2:5:0.3", "6:F3:4:0.28", "3:D3:6:0.3" }, 1.0));
            p.Drone = n => Inst.Periodic(n, Sr,
                (73.42, 0.9, 2, 0.3, 0.25), (73.52, 0.6, 3, 1.1, 0.3),
                (146.84, 0.35, 3, 2.0, 0.3), (220.3, 0.16, 5, 0.7, 0.35),
                (110.0, 0.3, 2, 2.5, 0.3), (110.07, 0.2, 3, 0.2, 0.3));
            yield return p;
        }
        // ---------------- ch4 ----------------
        {
            var p = new Piece { Name = "music_ch4", Bpm = 72, Bars = 16, PadGain = 0.8, PadBright = 750, PadAttack = 2.6, Target = -10, RevRt = 2.8, RevWet = 0.45, InstLp = 3000 };
            p.Chords = Chords8("A2 E3 A3 C4", "F2 C3 F3 A3", "C3 G3 C4 E4", "E3 B3 E4 G4",
                               "A2 E3 A3 C4", "D3 A3 D4 F4", "F2 C3 F3 A3", "E3 B3 E4 G4");
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] { "2:E4:3:0.55", "0:C5:2:0.5 5:A4:2:0.5", "3:G4:3:0.5", "1:B4:2:0.5",
                "2:C5:2:0.55 6:A4:2:0.5", "4:F4:3:0.5", "1:A4:2:0.5", "3:E4:4:0.5" }, 1.0));
            p.Tracks.Add(new Track(Ins.Piano, 4, Enumerable.Range(0, 16).Select(i => i % 2 == 0 ? "0:A2:3:0.3" : "0:E3:3:0.25").ToArray(), 1.0));
            yield return p;
        }
        // ---------------- ch5 ----------------
        {
            var p = new Piece { Name = "music_ch5", Bpm = 56, Bars = 14, PadGain = 0.7, PadBright = 1700, PadAttack = 3.0, Target = -12, RevRt = 3.2, RevWet = 0.5, InstLp = 3500 };
            p.Chords = new[] { ("A3 E4 B4 C5", 14.0), ("F3 C4 E4 A4", 14), ("D4 A4 D5 F5", 14), ("E4 B4 E5 A4", 14) };
            p.Tracks.Add(new Track(Ins.Piano, 14, new[] { "4:C5:4:0.42", "7:A4:4:0.4", "2:F5:3:0.38 9:D5:3:0.38", "6:E5:5:0.38" }, 1.0));
            yield return p;
        }
        // ---------------- ch6 ----------------
        {
            var p = new Piece { Name = "music_ch6", Bpm = 72, Bars = 16, PadGain = 0.7, PadBright = 1500, Target = -10, RevRt = 2.4, RevWet = 0.42 };
            p.Chords = Chords8("C3 G3 C4 E4", "A2 E3 A3 C4 G4", "F3 C4 F4 A4", "C3 G3 E4 A4",
                               "C3 G3 C4 E4", "A2 E3 A3 C4 G4", "F3 C4 F4 A4", "G2 D3 G3 B3 D4");
            p.Tracks.Add(new Track(Ins.Piano, 8, new[] {
                "0:C4:1:0.45 1:D4:1:0.45 2:E4:3:0.5 6:G4:2:0.5",
                "0:A3:1:0.45 1:C4:1:0.45 2:E4:3:0.5 6:D4:2:0.45",
                "0:C4:1:0.45 1:D4:1:0.45 2:F4:3:0.5 6:A4:2:0.5",
                "0:E4:1:0.45 1:G4:1:0.45 2:A4:3:0.5 6:G4:2:0.45",
                "0:C4:1:0.45 1:E4:1:0.45 2:G4:3:0.5 6:A4:2:0.5",
                "0:A3:1:0.45 1:C4:1:0.45 2:E4:2:0.5 4:G4:2:0.5 6:C5:2:0.5",
                "0:A4:1:0.45 1:C5:1:0.45 2:D5:3:0.5 6:C5:2:0.45",
                "0:B4:1:0.45 1:D5:1:0.45 2:E5:3:0.5 6:D5:2:0.45" }, 1.0));
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] { "0:C3:4:0.4 4:G3:4:0.35", "0:A2:4:0.4 4:E3:4:0.35", "0:F3:4:0.4 4:C3:4:0.35", "0:C3:4:0.4 4:G3:4:0.35",
                "0:C3:4:0.4 4:G3:4:0.35", "0:A2:4:0.4 4:E3:4:0.35", "0:F3:4:0.4 4:C3:4:0.35", "0:G2:4:0.4 4:D3:4:0.35" }, 1.0));
            yield return p;
        }
        // ---------------- choice ----------------
        {
            var p = new Piece { Name = "music_choice", Bpm = 60, Bars = 12, Target = -14, InstLp = 2000 };
            p.Drone = n => Inst.Periodic(n, Sr,
                (146.83, 0.9, 3, 0.2, 0.35), (146.87, 0.6, 4, 1.4, 0.4),
                (196.0, 0.7, 5, 2.2, 0.4), (196.05, 0.45, 3, 0.6, 0.4),
                (220.0, 0.6, 4, 3.0, 0.45), (329.63, 0.35, 6, 1.0, 0.5), (329.7, 0.25, 7, 2.6, 0.5),
                (73.42, 0.5, 2, 0.4, 0.2), (880.0, 0.05, 9, 0.0, 0.9), (987.77, 0.04, 11, 1.9, 0.9));
            yield return p;
        }
        // ---------------- ending ----------------
        {
            var p = new Piece { Name = "music_ending", Bpm = 60, Bars = 16, PadGain = 0.9, PadBright = 1300, PadAttack = 2.4, Target = -11, RevRt = 3.2, RevWet = 0.5,
                Energy = t => t < 0.5 ? 1.0 : 1.0 - 0.65 * ((t - 0.5) / 0.5) };
            p.Chords = Chords8("F3 C4 E4 A4", "D3 A3 C4 F4", "Bb2 F3 A3 D4", "G3 Bb3 D4 A4",
                               "F3 C4 E4 A4", "A2 E3 G3 C4", "Bb2 F3 C4 D4", "C3 G3 D4 F4");
            p.Tracks.Add(new Track(Ins.Piano, 8, new[] {
                "1:A4:2:0.5 4:C5:2:0.5 6.5:E5:1.5:0.5",
                "0:D5:2:0.5 3:C5:1.5:0.45 5:A4:3:0.5",
                "1:F4:2:0.5 4:A4:2:0.5 6.5:D5:1.5:0.5",
                "0:Bb4:2:0.5 3:A4:2:0.5 6:G4:2:0.5",
                "1:C5:2:0.5 4:A4:2:0.5 6:F4:2:0.5",
                "0:E5:2:0.5 3:C5:2:0.5 6:A4:2:0.5",
                "1:D5:2:0.5 4:C5:3:0.5",
                "2:G4:3:0.5 6:F4:2:0.5" }, 1.0));
            p.Tracks.Add(new Track(Ins.Pluck, 8, new[] { "4:F3:3:0.35", "", "4:Bb2:3:0.35", "", "4:F3:3:0.35", "", "4:Bb2:3:0.3", "" }, 1.0));
            yield return p;
        }
    }
}
