namespace AudioGen;

static class Sfx
{
    const int Sr = Dsp.SfxSr;

    public record Item(string Name, float[] Data, double Db, double FadeOutMs);

    public static List<Item> RenderAll()
    {
        var l = new List<Item>
        {
            new("sfx_footstep_1", Footstep("sfx_footstep_1", 105, 62, 520, 0.35, 1900), -3, 15),
            new("sfx_footstep_2", Footstep("sfx_footstep_2", 125, 70, 380, 0.5, 2300), -3, 15),
            new("sfx_interact", Interact(), -3, 25),
            new("sfx_water_pour", Pour(), -3, 30),
            new("sfx_plate", Plate(), -3, 40),
            new("sfx_paper", Paper(), -3, 40),
            new("sfx_bag", Bag(), -3, 40),
            new("sfx_key_lock", KeyLock(), -3, 40),
            new("sfx_water_jug", Jug(), -3, 40),
            new("sfx_cloth", Cloth(), -3, 40),
            new("sfx_distant_boom", Boom(), -9, 150),
            new("sfx_transition", Transition(), -6, 40),
            new("ui_hover", Hover(), -6, 8),
            new("ui_click", Click(), -3, 20),
            new("ui_panel_open", Panel(true), -3, 20),
            new("ui_panel_close", Panel(false), -3, 20),
            new("ui_tab", Tab(), -3, 10),
            new("ui_advance", Advance(), -3, 12),
            new("ui_objective", Objective(), -3, 60),
            new("ui_memory", Memory(), -3, 150),
            new("ui_keepsake", Keepsake(), -3, 60),
            new("ui_achievement", Achievement(), -3, 150),
            new("ui_select", Select(), -3, 20),
            new("ui_confirm", Confirm(), -3, 50),
            new("ui_locked", Locked(), -3, 25),
        };
        return l;
    }

    // ---------- helpers ----------
    static int T(double s) => (int)Math.Round(s * Sr);
    static float[] Buf(double s) => new float[T(s)];

    static void Tone(float[] b, double f, double amp, double tau, double start = 0, double att = 0.002, double phase = 0) =>
        Inst.Partial(b, f, amp, tau, phase, Sr, T(start), att);

    static void Sweep(float[] b, double f0, double f1, double tauP, double amp, double tauA, double start = 0, double att = 0.003)
    {
        int s = T(start);
        double ph = 0;
        for (int i = s; i < b.Length; i++)
        {
            double t = (double)(i - s) / Sr;
            double e = Math.Exp(-t / tauA);
            if (e < 1e-4) break;
            double f = f1 + (f0 - f1) * Math.Exp(-t / tauP);
            ph += 2 * Math.PI * f / Sr;
            double a = t < att ? 0.5 - 0.5 * Math.Cos(Math.PI * t / att) : 1;
            b[i] += (float)(amp * a * e * Math.Sin(ph));
        }
    }

    // exponential-decay noise burst
    static void Burst(float[] b, float[] noise, double amp, double start, double att, double tau)
    {
        int s = T(start);
        for (int i = 0; i < noise.Length && s + i < b.Length; i++)
        {
            double t = (double)i / Sr;
            b[s + i] += (float)(noise[i] * amp * (1 - Math.Exp(-t / att)) * Math.Exp(-t / tau));
        }
    }

    static void Bubble(float[] b, double start, double f0, double amp, double tau)
    {
        int s = T(start);
        int len = (int)(tau * 6 * Sr);
        double ph = 0;
        for (int i = 0; i < len && s + i < b.Length; i++)
        {
            double t = (double)i / Sr;
            double f = f0 * (1 + 5 * t);
            ph += 2 * Math.PI * f / Sr;
            b[s + i] += (float)(amp * (1 - Math.Exp(-t / 0.0008)) * Math.Exp(-t / tau) * Math.Sin(ph));
        }
    }

    static void Bell(float[] b, double f, double amp, double tau, double start, double[] ratios = null, double[] amps = null, double[] taus = null)
    {
        ratios ??= new[] { 1.0, 2.0, 3.01, 4.15 };
        amps ??= new[] { 1.0, 0.3, 0.12, 0.05 };
        taus ??= new[] { 1.0, 0.6, 0.35, 0.2 };
        for (int k = 0; k < ratios.Length; k++)
            Tone(b, f * ratios[k], amp * amps[k], tau * taus[k], start, 0.003);
    }

    static float[] Smooth(float[] x, double fc) => Dsp.OnePoleLP(x, fc, Sr);

    static float[] Lp(float[] x, double fc) => Dsp.Biquad(x, BQ.LP, fc, 0.7, Sr);

    static float Hann(double t, double c, double w)
    {
        double d = (t - c) / w;
        return Math.Abs(d) >= 0.5 ? 0 : (float)(0.5 + 0.5 * Math.Cos(2 * Math.PI * d));
    }

    // Layered band-passed noise with crinkly amplitude modulation and Hann-shaped swells.
    static float[] Rustle(Random r, double dur, (double fc, double q, double g)[] bands, double crinkleHz,
        (double c, double w, double a)[] bumps, double floor = 0.3)
    {
        int n = T(dur);
        var y = new float[n];
        foreach (var (fc, q, g) in bands)
        {
            var nz = Dsp.Filtered(r, n, BQ.BP, fc, q, Sr, 0.3);
            for (int i = 0; i < n; i++) y[i] += nz[i] * (float)g;
        }
        var cr = Smooth(Dsp.WhiteNoise(r, n), crinkleHz);
        double mean = 0;
        foreach (var v in cr) mean += Math.Abs(v);
        mean /= n;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / Sr;
            double env = 0;
            foreach (var (c, w, a) in bumps) env += a * Hann(t, c, w);
            double m = floor + (1 - floor) * Math.Min(1.0, Math.Abs(cr[i]) / (2.2 * mean));
            y[i] *= (float)(env * m);
        }
        return y;
    }

    // ---------- gameplay ----------
    static float[] Footstep(string name, double f0, double f1, double nCut, double scuffAmp, double scuffFc)
    {
        var r = Dsp.Rng(name);
        var b = Buf(0.16);
        Sweep(b, f0, f1, 0.035, 1.0, 0.03);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.LP, nCut, 0.7, Sr, 0.3), 1.4, 0, 0.003, 0.02);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.BP, scuffFc, 0.8, Sr, 0.3), scuffAmp, 0.004, 0.008, 0.03);
        return b;
    }

    static float[] Interact()
    {
        var r = Dsp.Rng("sfx_interact");
        var b = Rustle(r, 0.25, new[] { (900.0, 0.7, 1.0), (380.0, 0.6, 0.8) }, 25, new[] { (0.1, 0.2, 1.0) }, 0.5);
        Tone(b, 150, 0.12, 0.03, 0.03, 0.004);
        return Lp(b, 3500);
    }

    static float[] Pour()
    {
        var r = Dsp.Rng("sfx_water_pour");
        var b = Buf(1.25);
        var hiss = Dsp.Filtered(r, b.Length, BQ.BP, 1600, 0.6, Sr, 0.3);
        var low = Dsp.Filtered(r, b.Length, BQ.LP, 800, 0.7, Sr, 0.3);
        for (int i = 0; i < b.Length; i++) b[i] = hiss[i] * 0.3f + low[i] * 0.25f;
        double t = 0.04;
        while (t < 1.15)
        {
            t += -Math.Log(1 - r.NextDouble()) / 85.0;
            double u = r.NextDouble();
            Bubble(b, t, 450 * Math.Pow(4.5, u), 0.1 + 0.3 * r.NextDouble(), 0.006 + 0.02 * u);
        }
        for (int i = 0; i < b.Length; i++)
        {
            double tt = (double)i / Sr;
            double e = Math.Min(1.0, tt / 0.12);
            if (tt > 0.95) e *= 0.5 + 0.5 * Math.Cos(Math.PI * (tt - 0.95) / 0.3);
            b[i] *= (float)e;
        }
        return Lp(b, 6000);
    }

    static float[] Plate()
    {
        var r = Dsp.Rng("sfx_plate");
        var b = Buf(0.45);
        double[] ratios = { 1, 2.32, 4.25 }, amps = { 1, 0.6, 0.3 }, taus = { 0.14, 0.09, 0.045 };
        void Clink(double fc, double amp, double start)
        {
            for (int k = 0; k < 3; k++) Tone(b, fc * ratios[k], amp * amps[k], taus[k], start, 0.0005);
            Tone(b, 420, amp * 0.4, 0.05, start, 0.001);
            Burst(b, Dsp.Filtered(r, T(0.02), BQ.HP, 1500, 0.7, Sr, 0.3), amp * 0.5, start, 0.0003, 0.002);
        }
        Clink(1100, 1.0, 0);
        Clink(1180, 0.45, 0.11);
        return Lp(b, 7000);
    }

    static float[] Paper()
    {
        var r = Dsp.Rng("sfx_paper");
        var b = Rustle(r, 0.65, new[] { (3000.0, 0.6, 1.0), (1800.0, 0.7, 0.8), (4800.0, 0.8, 0.25) }, 70,
            new[] { (0.08, 0.14, 0.9), (0.24, 0.2, 1.0), (0.42, 0.24, 0.8) }, 0.15);
        return Lp(b, 6500);
    }

    static float[] Bag()
    {
        var r = Dsp.Rng("sfx_bag");
        var b = Rustle(r, 0.75, new[] { (700.0, 0.6, 1.0), (1500.0, 0.7, 0.6) }, 30,
            new[] { (0.1, 0.24, 1.0), (0.26, 0.2, 0.7), (0.62, 0.16, 0.4) }, 0.3);
        double t = 0.4;
        for (int i = 0; i < 6; i++)
        {
            var tick = Dsp.Filtered(r, T(0.02), BQ.BP, 1400, 2.5, Sr, 0.3);
            Burst(b, tick, 1.6 - 0.15 * i, t, 0.0005, 0.003);
            t += 0.042 + r.NextDouble() * 0.015;
        }
        return Lp(b, 5000);
    }

    static float[] KeyLock()
    {
        var r = Dsp.Rng("sfx_key_lock");
        var b = Buf(0.85);
        Tone(b, 1750, 0.35, 0.03, 0, 0.0005);
        Tone(b, 1750 * 2.76, 0.12, 0.015, 0, 0.0005);
        Burst(b, Dsp.Filtered(r, T(0.02), BQ.HP, 2000, 0.7, Sr, 0.3), 0.4, 0, 0.0003, 0.002);
        // turning scrape with ratchet modulation
        var svf = new Svf();
        int s0 = T(0.15), s1 = T(0.5);
        for (int i = s0; i < s1; i++)
        {
            double t = (double)(i - s0) / (s1 - s0);
            svf.Tick(r.NextDouble() * 2 - 1, 900 + 800 * t, 2.0, Sr);
            double env = Hann(t, 0.5, 1.0);
            b[i] += (float)(svf.Bp * env * (0.6 + 0.4 * Math.Sin(2 * Math.PI * 38 * t * 0.35)) * 0.55);
        }
        // lock click and thunk
        Tone(b, 1500, 0.5, 0.005, 0.55, 0.0004);
        Burst(b, Dsp.Filtered(r, T(0.02), BQ.BP, 2200, 0.8, Sr, 0.3), 0.8, 0.55, 0.0003, 0.003);
        Tone(b, 320, 0.9, 0.035, 0.6, 0.002);
        Tone(b, 640, 0.25, 0.02, 0.6, 0.002);
        return Lp(b, 6500);
    }

    static float[] Jug()
    {
        var r = Dsp.Rng("sfx_water_jug");
        var b = Buf(1.05);
        double t = 0.04;
        while (t < 0.9)
        {
            t += -Math.Log(1 - r.NextDouble()) / 55.0;
            double f0 = (250 + 600 * t) * (0.8 + 0.4 * r.NextDouble());
            Bubble(b, t, f0, 0.15 + 0.25 * r.NextDouble(), 0.01 + 0.02 * r.NextDouble());
        }
        var svf = new Svf();
        for (int i = 0; i < b.Length; i++)
        {
            double tt = (double)i / Sr;
            svf.Tick(r.NextDouble() * 2 - 1, 300 + 400 * tt, 2.0, Sr);
            double g = (0.6 + 0.4 * Math.Sin(2 * Math.PI * 6.5 * tt)) * Math.Min(1.0, tt / 0.1);
            b[i] += (float)(svf.Bp * g * 0.45);
        }
        var slosh = Dsp.Filtered(r, b.Length, BQ.BP, 450, 0.8, Sr, 0.3);
        for (int i = 0; i < b.Length; i++) b[i] += slosh[i] * Hann((double)i / Sr, 0.78, 0.3) * 1.0f;
        for (int i = 0; i < b.Length; i++)
        {
            double tt = (double)i / Sr;
            double e = Math.Min(1.0, tt / 0.08);
            if (tt > 0.85) e *= 0.5 + 0.5 * Math.Cos(Math.PI * (tt - 0.85) / 0.2);
            b[i] *= (float)e;
        }
        return Lp(b, 5500);
    }

    static float[] Cloth()
    {
        var r = Dsp.Rng("sfx_cloth");
        var b = Rustle(r, 0.75, new[] { (260.0, 0.5, 1.0), (700.0, 0.6, 0.9), (1300.0, 0.7, 0.4) }, 14,
            new[] { (0.16, 0.32, 1.0), (0.46, 0.36, 0.9) }, 0.35);
        return Lp(b, 2500);
    }

    static float[] Boom()
    {
        var r = Dsp.Rng("sfx_distant_boom");
        var b = Buf(1.3);
        Sweep(b, 62, 38, 0.15, 1.0, 0.22, 0, 0.03);
        Tone(b, 120, 0.3, 0.12, 0, 0.03);
        var rum = Dsp.Filtered(r, b.Length, BQ.LP, 90, 0.7, Sr, 0.3);
        for (int i = 0; i < b.Length; i++)
        {
            double t = (double)i / Sr;
            b[i] += (float)(rum[i] * 2.0 * (1 - Math.Exp(-t / 0.04)) * Math.Exp(-t / 0.45));
        }
        return Dsp.Biquad(b, BQ.LP, 160, 0.7, Sr);
    }

    static float[] Transition()
    {
        var r = Dsp.Rng("sfx_transition");
        var b = Buf(1.55);
        double[] fs = { 220, 330, 440, 660 }, amps = { 1.0, 0.7, 0.5, 0.2 };
        for (int k = 0; k < fs.Length; k++)
        {
            Tone(b, fs[k] * 0.998, amps[k] * 0.5, 0, 0, 0.001, k);
            Tone(b, fs[k] * 1.002, amps[k] * 0.5, 0, 0, 0.001, k * 2);
        }
        var svf = new Svf();
        for (int i = 0; i < b.Length; i++)
        {
            double t = (double)i / Sr / 1.55;
            svf.Tick(r.NextDouble() * 2 - 1, 300 + 900 * t, 0.9, Sr);
            b[i] += (float)(svf.Bp * 0.25);
            b[i] *= (float)Math.Pow(Math.Sin(Math.PI * t), 2);
        }
        return Lp(b, 3000);
    }

    // ---------- UI ----------
    static float[] Hover()
    {
        var r = Dsp.Rng("ui_hover");
        var b = Buf(0.07);
        Tone(b, 1320, 1.0, 0.008, 0, 0.001);
        Tone(b, 2640, 0.15, 0.004, 0, 0.001);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.BP, 2500, 0.8, Sr, 0.3), 0.15, 0, 0.0005, 0.003);
        return Lp(b, 5000);
    }

    static float[] Click()
    {
        var b = Buf(0.18);
        Sweep(b, 560, 523, 0.008, 1.0, 0.045, 0, 0.001);
        Tone(b, 1046, 0.3, 0.03, 0, 0.001);
        Tone(b, 1569, 0.12, 0.02, 0, 0.001);
        return Lp(b, 5000);
    }

    static float[] Panel(bool open)
    {
        var r = Dsp.Rng(open ? "ui_panel_open" : "ui_panel_close");
        var b = Buf(0.22);
        double fa = open ? 380 : 760, fb = open ? 760 : 380;
        double ph = 0;
        var svf = new Svf();
        for (int i = 0; i < T(0.16); i++)
        {
            double t = (double)i / Sr;
            double u = t / 0.16;
            double f = fa * Math.Pow(fb / fa, u);
            ph += 2 * Math.PI * f / Sr;
            double env = Math.Sin(Math.PI * Math.Min(1.0, u)) * (open ? 0.5 + 0.5 * u : 1.0 - 0.5 * u);
            svf.Tick(r.NextDouble() * 2 - 1, 600 + 1400 * (open ? u : 1 - u), 1.0, Sr);
            b[i] += (float)((0.8 * Math.Sin(ph) + 0.15 * Math.Sin(2 * ph)) * env + svf.Bp * 0.2 * env);
        }
        Tone(b, fb, 0.5, 0.05, open ? 0.13 : 0.11, 0.003);
        return Lp(b, 5000);
    }

    static float[] Tab()
    {
        var r = Dsp.Rng("ui_tab");
        var b = Buf(0.1);
        Tone(b, 980, 1.0, 0.012, 0, 0.0007);
        Tone(b, 1560, 0.6, 0.008, 0, 0.0007);
        Tone(b, 3100, 0.2, 0.004, 0, 0.0007);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.LP, 3000, 0.7, Sr, 0.3), 0.3, 0, 0.0003, 0.002);
        return Lp(b, 5000);
    }

    static float[] Advance()
    {
        var r = Dsp.Rng("ui_advance");
        var b = Buf(0.11);
        var svf = new Svf();
        for (int i = 0; i < T(0.08); i++)
        {
            double t = (double)i / Sr;
            svf.Tick(r.NextDouble() * 2 - 1, 3200 - 1600 * Math.Min(1.0, t / 0.06), 1.2, Sr);
            b[i] += (float)(svf.Bp * (1 - Math.Exp(-t / 0.004)) * Math.Exp(-t / 0.025));
        }
        Tone(b, 260, 0.3, 0.015, 0, 0.002);
        return Lp(b, 5500);
    }

    static float[] Objective()
    {
        var b = Buf(0.55);
        var ratios = new[] { 1.0, 2.0, 3.0 };
        var amps = new[] { 1.0, 0.25, 0.08 };
        var taus = new[] { 1.0, 0.55, 0.3 };
        Bell(b, 523.25, 0.8, 0.25, 0, ratios, amps, taus);
        Bell(b, 783.99, 1.0, 0.28, 0.14, ratios, amps, taus);
        return b;
    }

    static float[] Memory()
    {
        var r = Dsp.Rng("ui_memory");
        var src = Buf(1.25);
        double[] notes = { 523.25, 587.33, 659.25, 783.99, 880, 1046.5, 1174.66, 1318.5, 1567.98 };
        var ratios = new[] { 1.0, 2.0, 4.0 };
        var amps = new[] { 1.0, 0.25, 0.06 };
        var taus = new[] { 1.0, 0.5, 0.25 };
        for (int i = 0; i < notes.Length; i++)
            Bell(src, notes[i], 0.35 + 0.05 * i, 0.4, 0.03 + i * 0.06 + (r.NextDouble() - 0.5) * 0.015, ratios, amps, taus);
        for (int j = 0; j < 7; j++)
            Bell(src, notes[3 + r.Next(6)], 0.16 + 0.06 * r.NextDouble(), 0.35, 0.5 + j * 0.09 + r.NextDouble() * 0.03, ratios, amps, taus);
        Tone(src, 261.63, 0.5, 0.7, 0, 0.06);
        Tone(src, 392.0, 0.4, 0.7, 0.02, 0.06);
        Tone(src, 659.25, 0.25, 0.6, 0.03, 0.08);
        var o = new float[src.Length];
        Dsp.Add(o, 0, src, 1.0);
        Dsp.Add(o, T(0.13), src, 0.35);
        Dsp.Add(o, T(0.27), src, 0.18);
        for (int i = 0; i < o.Length; i++)
        {
            double t = (double)i / Sr;
            o[i] *= (float)(1 + 0.25 * Math.Sin(2 * Math.PI * 8.5 * t) * Math.Exp(-t / 0.7));
        }
        return Lp(o, 7000);
    }

    static float[] Keepsake()
    {
        var r = Dsp.Rng("ui_keepsake");
        var b = Buf(0.65);
        Burst(b, Dsp.Filtered(r, T(0.1), BQ.BP, 900, 0.8, Sr, 0.3), 1.0, 0, 0.003, 0.018);
        Tone(b, 200, 0.4, 0.02, 0, 0.002);
        Tone(b, 392, 0.7, 0.28, 0.05, 0.006);
        Tone(b, 784, 0.28, 0.16, 0.05, 0.006);
        Tone(b, 1176, 0.08, 0.1, 0.05, 0.006);
        return Lp(b, 5000);
    }

    static float[] Achievement()
    {
        var b = Buf(1.3);
        Bell(b, 659.25, 0.7, 0.45, 0);
        Bell(b, 783.99, 0.8, 0.5, 0.17);
        Bell(b, 1046.5, 1.0, 0.6, 0.34);
        Tone(b, 261.63, 0.18, 0.8, 0.3, 0.1);
        Tone(b, 392.0, 0.14, 0.8, 0.3, 0.1);
        return Lp(b, 6500);
    }

    static float[] Select()
    {
        var r = Dsp.Rng("ui_select");
        var b = Buf(0.16);
        Sweep(b, 240, 150, 0.04, 1.0, 0.045, 0, 0.002);
        Tone(b, 300, 0.2, 0.03, 0, 0.002);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.LP, 1500, 0.7, Sr, 0.3), 0.5, 0, 0.0005, 0.005);
        return Lp(b, 3500);
    }

    static float[] Confirm()
    {
        var b = Buf(0.55);
        void Note(double f, double start, double amp)
        {
            Tone(b, f, amp, 0.22, start, 0.006);
            Tone(b, f * 2, amp * 0.35, 0.14, start, 0.006);
            Tone(b, f * 3, amp * 0.1, 0.08, start, 0.006);
        }
        Note(220, 0, 0.8);
        Note(329.63, 0.11, 1.0);
        return Lp(b, 3500);
    }

    static float[] Locked()
    {
        var r = Dsp.Rng("ui_locked");
        var b = Buf(0.22);
        Sweep(b, 95, 60, 0.05, 1.0, 0.05, 0, 0.004);
        Burst(b, Dsp.Filtered(r, b.Length, BQ.LP, 300, 0.7, Sr, 0.3), 0.8, 0, 0.002, 0.015);
        return Lp(b, 800);
    }
}
