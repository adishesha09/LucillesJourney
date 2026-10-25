using System.Globalization;

namespace AudioGen;

sealed record FileInfoRow(string Name, string Kind, string Path, int Sr, double Dur, double PeakDb, double RmsDb,
    int FullScale, double SeamStep, double RmsStep, double MaxStep, double SeamRmsRatio, double StartAbs, double EndAbs);

static class Analysis
{
    static readonly string[] Expected =
    {
        "music_menu","music_ch1","music_ch2","music_ch3","music_ch4","music_ch5","music_ch6","music_choice","music_ending",
        "amb_ch1","amb_ch2","amb_ch3","amb_ch4","amb_ch5","amb_ch6",
        "sfx_footstep_1","sfx_footstep_2","sfx_interact","sfx_water_pour","sfx_plate","sfx_paper","sfx_bag","sfx_key_lock",
        "sfx_water_jug","sfx_cloth","sfx_distant_boom","sfx_transition",
        "ui_hover","ui_click","ui_panel_open","ui_panel_close","ui_tab","ui_advance","ui_objective","ui_memory",
        "ui_keepsake","ui_achievement","ui_select","ui_confirm","ui_locked",
    };

    public static string KindOf(string name) => name.StartsWith("music_") ? "music" : name.StartsWith("amb_") ? "amb" : "sfx";

    public static (List<FileInfoRow> rows, int failures) Run(string audioDir)
    {
        var rows = new List<FileInfoRow>();
        int fails = 0;
        void Fail(string m) { fails++; Console.WriteLine("  !! FAIL: " + m); }

        Console.WriteLine();
        Console.WriteLine($"{"file",-18}{"sr",6}{"dur s",8}{"peak dB",9}{"rms dB",8}  loop seam (step / rmsStep / maxStep, first-vs-last 100ms rms dB)");
        foreach (var name in Expected)
        {
            string path = Path.Combine(audioDir, name + ".wav");
            if (!File.Exists(path)) { Fail(name + " missing"); continue; }
            var (d, sr, ch, bits) = Wav.Read(path);
            string kind = KindOf(name);
            int n = d.Length;
            var x = new float[n];
            int full = 0;
            double sum2 = 0, peak = 0;
            for (int i = 0; i < n; i++)
            {
                x[i] = d[i] / 32768f;
                if (Math.Abs((int)d[i]) >= 32767) full++;
                peak = Math.Max(peak, Math.Abs(x[i]));
                sum2 += (double)x[i] * x[i];
            }
            double rms = Math.Sqrt(sum2 / n), dur = (double)n / sr;
            double peakDb = 20 * Math.Log10(Math.Max(peak, 1e-9)), rmsDb = 20 * Math.Log10(Math.Max(rms, 1e-9));
            double seam = 0, rmsStep = 0, maxStep = 0, ratio = 0, s0 = Math.Abs(x[0]), s1 = Math.Abs(x[n - 1]);
            string loopInfo = "";
            if (kind != "sfx")
            {
                seam = Math.Abs(x[0] - x[n - 1]);
                double ds = 0;
                for (int i = 1; i < n; i++)
                {
                    double st = Math.Abs(x[i] - x[i - 1]);
                    ds += st * st;
                    maxStep = Math.Max(maxStep, st);
                }
                rmsStep = Math.Sqrt(ds / (n - 1));
                int w = sr / 10;
                double a = 0, b = 0;
                for (int i = 0; i < w; i++) { a += (double)x[i] * x[i]; b += (double)x[n - 1 - i] * x[n - 1 - i]; }
                double aDb = 10 * Math.Log10(Math.Max(a / w, 1e-18)), bDb = 10 * Math.Log10(Math.Max(b / w, 1e-18));
                ratio = seam / Math.Max(rmsStep, 1e-9);
                loopInfo = $"  seam {seam:0.00000} / {rmsStep:0.00000} / {maxStep:0.00000} (x{ratio:0.00})  first {aDb:0.0} last {bDb:0.0}";
                // a click would be a step larger than any step occurring elsewhere in the file
                if (seam > maxStep) Fail($"{name}: wrap-around step {seam:0.00000} exceeds the largest step in the file {maxStep:0.00000}");
                // note onsets at bar 1 vs. decaying tails at the end legitimately differ in level; informational only
                if (Math.Abs(aDb - bDb) > 6) loopInfo += "  (level differs: note onset at start / sparse tail)";
            }
            Console.WriteLine($"{name,-18}{sr,6}{dur,8:0.00}{peakDb,9:0.0}{rmsDb,8:0.0}{loopInfo}");

            int expectSr = kind == "sfx" ? Dsp.SfxSr : Dsp.MusicSr;
            if (sr != expectSr) Fail($"{name}: sample rate {sr}");
            if (ch != 1 || bits != 16) Fail($"{name}: format ch={ch} bits={bits}");
            if (full > 0) Fail($"{name}: {full} clipped samples");
            if (peak > 0.9) Fail($"{name}: peak above -1 dBFS");
            if (rms < 2e-4) Fail($"{name}: silent (rms {rmsDb:0.0} dB)");
            if (kind == "music" && (dur < 45 || dur > 75)) Fail($"{name}: duration {dur:0.0}s out of 45-75");
            if (kind == "amb" && (dur < 30 || dur > 40)) Fail($"{name}: duration {dur:0.0}s out of 30-40");
            if (kind == "sfx" && (s0 > 0.01 || s1 > 0.01)) Fail($"{name}: edge samples not ~0 ({s0:0.0000}, {s1:0.0000})");
            rows.Add(new FileInfoRow(name, kind, path, sr, dur, peakDb, rmsDb, full, seam, rmsStep, maxStep, ratio, s0, s1));
        }
        Console.WriteLine();
        Console.WriteLine(fails == 0 ? "ANALYSIS: all checks passed" : $"ANALYSIS: {fails} failure(s)");
        return (rows, fails);
    }
}
