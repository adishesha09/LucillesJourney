namespace AudioGen;

static class Ambience
{
    const int Sr = Dsp.MusicSr;

    public static List<(string name, float[] data)> RenderAll() => new()
    {
        ("amb_ch1", Ch1()), ("amb_ch2", Ch2()), ("amb_ch3", Ch3()),
        ("amb_ch4", Ch4()), ("amb_ch5", Ch5()), ("amb_ch6", Ch6()),
    };

    static int N(double sec) => (int)Math.Round(sec * Sr);

    static float[] Gust(Random r, int n, double fc, double depth)
    {
        var s = Dsp.SlowNoise(r, n, fc, Sr);
        var g = new float[n];
        for (int i = 0; i < n; i++) g[i] = (float)(1 - depth + depth * (0.5 + 0.5 * s[i]));
        return g;
    }

    static void Mix(float[] dst, float[] src, double gain)
    {
        for (int i = 0; i < dst.Length; i++) dst[i] += src[i] * (float)gain;
    }

    static float[] Wind(Random r, int n, double c1, double c2, double gustHz, double depth, double rumble)
    {
        var y = new float[n];
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.BP, c1, 0.8, Sr), Gust(r, n, gustHz, depth)), 1.0);
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.BP, c2, 1.0, Sr), Gust(r, n, gustHz * 1.3, depth)), 0.45);
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.LP, 130, 0.7, Sr), Gust(r, n, gustHz * 0.8, depth)), rumble);
        return y;
    }

    static float[] Chirp(double f0, double f1, double dur, double vibHz, double vibDepth, double amp, Random r)
    {
        int n = (int)(dur * Sr);
        var o = new float[n];
        double ph = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / n;
            double f = f0 + (f1 - f0) * t + vibDepth * Math.Sin(2 * Math.PI * vibHz * t * dur);
            ph += 2 * Math.PI * f / Sr;
            double env = Math.Pow(Math.Sin(Math.PI * t), 1.5);
            o[i] = (float)(amp * env * Math.Sin(ph));
        }
        return o;
    }

    static float[] Finish(float[] x, double db, double lp = 5500)
    {
        x = Dsp.CircBiquad(x, BQ.HP, 25, 0.7, Sr);
        x = Dsp.CircBiquad(x, BQ.LP, lp, 0.6, Sr);
        return Dsp.Normalize(x, db);
    }

    static float[] Ch1()
    {
        var r = Dsp.Rng("amb_ch1");
        int n = N(36);
        var y = Dsp.CircBiquad(Wind(r, n, 420, 900, 0.18, 0.7, 0.5), BQ.LP, 1500, 0.7, Sr);
        for (int m = 0; m < 9; m++)
        {
            double t0 = r.NextDouble() * 36;
            double baseF = 2300 + r.NextDouble() * 900;
            int count = 1 + r.Next(3);
            double t = t0;
            for (int c = 0; c < count; c++)
            {
                double dur = 0.07 + r.NextDouble() * 0.07;
                bool up = r.NextDouble() < 0.5;
                var ch = Chirp(up ? baseF : baseF + 500, up ? baseF + 500 : baseF, dur, 38 + r.NextDouble() * 20, 90, 0.42 + r.NextDouble() * 0.2, r);
                Dsp.AddCirc(y, t * Sr, ch);
                t += dur + 0.07 + r.NextDouble() * 0.06;
            }
        }
        return Finish(y, -11, 5000);
    }

    static float[] Murmur(Random r, int n, double[] centers, double[] gains, double syllBase)
    {
        var y = new float[n];
        for (int j = 0; j < centers.Length; j++)
        {
            var band = Dsp.CircFiltered(r, n, BQ.BP, centers[j], 1.4, Sr);
            var swell = Dsp.SlowNoise(r, n, 0.25 + 0.07 * j, Sr);
            var syl = Dsp.SlowNoise(r, n, syllBase + 0.5 * j, Sr);
            var env = new float[n];
            for (int i = 0; i < n; i++) env[i] = (0.3f + 0.7f * (0.5f + 0.5f * swell[i])) * (0.45f + 0.55f * (0.5f + 0.5f * syl[i]));
            Mix(y, Dsp.Mul(band, env), gains[j]);
        }
        return y;
    }

    static float[] Ch2()
    {
        var r = Dsp.Rng("amb_ch2");
        int n = N(32);
        var y = Murmur(r, n, new[] { 240.0, 360, 520, 720, 980, 1350 }, new[] { 0.7, 1.0, 1.0, 0.8, 0.5, 0.25 }, 2.4);
        Mix(y, Dsp.CircFiltered(r, n, BQ.LP, 700, 0.7, Sr), 0.35);
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.BP, 500, 0.8, Sr), Gust(r, n, 0.12, 0.8)), 0.4);
        return Finish(y, -11, 3500);
    }

    static float[] Ch3()
    {
        var r = Dsp.Rng("amb_ch3");
        int n = N(40);
        var y = Dsp.CircFiltered(r, n, BQ.LP, 220, 0.7, Sr);
        Mix(y, Dsp.CircFiltered(r, n, BQ.BP, 110, 1.0, Sr), 0.7);
        var rum = Dsp.CircFiltered(r, n, BQ.LP, 60, 0.7, Sr);
        var sw = new float[n];
        int w = N(6);
        foreach (double c in new[] { 0.12, 0.45, 0.78 })
            Mix(sw, Dsp.HannBump(n, (int)(c * n + r.Next(N(2))), w), 0.9);
        for (int i = 0; i < n; i++) sw[i] += 0.12f;
        Mix(y, Dsp.Mul(rum, sw), 1.5);
        Mix(y, Inst.Periodic(n, Sr, (50.0, 0.07, 2, 0.4, 0.3), (100.0, 0.03, 3, 1.2, 0.3)), 1.0);
        return Finish(y, -12, 1200);
    }

    static float[] Ch4()
    {
        var r = Dsp.Rng("amb_ch4");
        int n = N(36);
        var y = Wind(r, n, 330, 750, 0.15, 0.75, 0.6);
        int steps = 58;
        for (int k = 0; k < steps; k++)
        {
            bool left = k % 2 == 0;
            double t = k * 36.0 / steps + (r.NextDouble() - 0.5) * 0.03;
            int len = N(0.25);
            var s = new float[len];
            Inst.Partial(s, left ? 68 : 76, 1.0, 0.055, 0, Sr, 0, 0.004);
            Inst.Partial(s, left ? 112 : 122, 0.45, 0.03, 1, Sr, 0, 0.004);
            var nz = Dsp.Filtered(r, len, BQ.LP, 420, 0.7, Sr, 0.3);
            var scuff = Dsp.Filtered(r, len, BQ.BP, 1700, 0.8, Sr, 0.3);
            for (int i = 0; i < len; i++)
            {
                double tt = (double)i / Sr;
                double att = 1 - Math.Exp(-tt / 0.004);
                s[i] += (float)(nz[i] * 1.6 * att * Math.Exp(-tt / 0.03) + scuff[i] * 0.55 * att * Math.Exp(-tt / 0.05));
            }
            Dsp.Fade(s, Sr, 2, 30);
            Dsp.AddCirc(y, t * Sr, s, left ? 0.6 : 0.7);
        }
        return Finish(y, -11, 3000);
    }

    static float[] Ch5()
    {
        var r = Dsp.Rng("amb_ch5");
        int n = N(38);
        var y = Murmur(r, n, new[] { 260.0, 420, 640 }, new[] { 0.9, 0.8, 0.4 }, 2.0);
        // cloth-flap wind: band noise with fast flutter that only appears during gusts
        var wind = Dsp.CircFiltered(r, n, BQ.BP, 600, 0.7, Sr);
        var gate = Dsp.SlowNoise(r, n, 0.12, Sr);
        var flutter = Dsp.SlowNoise(r, n, 7.0, Sr);
        var wenv = new float[n];
        for (int i = 0; i < n; i++)
        {
            double g = Math.Clamp(0.5 + 0.9 * gate[i], 0, 1);
            wenv[i] = (float)(0.25 + 0.75 * g * (0.55 + 0.45 * flutter[i]));
        }
        Mix(y, Dsp.Mul(wind, wenv), 0.9);
        Mix(y, Dsp.CircFiltered(r, n, BQ.LP, 150, 0.7, Sr), 0.35);
        // fire crackle: sparse tiny impulses through a band-pass
        var imp = new float[n];
        int count = (int)(38 * 9);
        for (int c = 0; c < count; c++)
        {
            int pos = r.Next(n);
            double a = 0.04 + 0.5 * Math.Pow(r.NextDouble(), 4);
            imp[pos] += (float)(a * (r.NextDouble() < 0.5 ? -1 : 1));
        }
        var crack = Dsp.CircBiquad(imp, BQ.BP, 1800, 0.9, Sr);
        Mix(y, crack, 10.0);
        var bed = Dsp.Mul(Dsp.CircFiltered(r, n, BQ.LP, 450, 0.7, Sr), Gust(r, n, 1.5, 0.6));
        Mix(y, bed, 0.25);
        return Finish(y, -11, 3800);
    }

    static float[] Ch6()
    {
        var r = Dsp.Rng("amb_ch6");
        int n = N(34);
        var y = Dsp.CircFiltered(r, n, BQ.LP, 300, 0.7, Sr);
        Mix(y, Dsp.CircFiltered(r, n, BQ.BP, 1800, 0.8, Sr, 0.05), 1.0);
        Mix(y, Inst.Periodic(n, Sr, (62.0, 0.12, 2, 0.0, 0.3), (124.0, 0.06, 3, 1.0, 0.3), (186.0, 0.03, 5, 2.0, 0.3)), 1.0);
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.LP, 180, 0.7, Sr), Gust(r, n, 0.1, 0.5)), 0.45);
        var swell = new float[n];
        int w = N(7);
        foreach (double c in new[] { 0.28, 0.74 })
            Mix(swell, Dsp.HannBump(n, (int)(c * n), w), 1.0);
        Mix(y, Dsp.Mul(Dsp.CircFiltered(r, n, BQ.BP, 450, 1.0, Sr), swell), 0.3);
        return Finish(y, -12, 2500);
    }
}
