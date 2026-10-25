namespace AudioGen;

enum BQ { LP, HP, BP }

static class Dsp
{
    public const int MusicSr = 22050;
    public const int SfxSr = 44100;

    public static Random Rng(string name)
    {
        uint h = 2166136261;
        foreach (char c in name) { h ^= c; h *= 16777619; }
        return new Random((int)(h & 0x7fffffff));
    }

    public static double Gauss(Random r)
    {
        double u1 = 1 - r.NextDouble(), u2 = r.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    }

    public static float[] WhiteNoise(Random r, int n)
    {
        var x = new float[n];
        for (int i = 0; i < n; i++) x[i] = (float)(r.NextDouble() * 2 - 1);
        return x;
    }

    public static float[] Tile3(float[] x)
    {
        var y = new float[x.Length * 3];
        for (int i = 0; i < 3; i++) Array.Copy(x, 0, y, i * x.Length, x.Length);
        return y;
    }

    // Runs a (time-invariant, short-memory) filter over three copies of the loop and keeps the last copy,
    // so filter state wraps around seamlessly.
    public static float[] Circ(float[] x, Func<float[], float[]> f)
    {
        var y = f(Tile3(x));
        var r = new float[x.Length];
        Array.Copy(y, 2 * x.Length, r, 0, x.Length);
        return r;
    }

    public static void AddCirc(float[] dst, double startSample, float[] src, double gain = 1)
    {
        int n = dst.Length;
        long s = (long)Math.Round(startSample) % n;
        if (s < 0) s += n;
        int idx = (int)s;
        float g = (float)gain;
        for (int i = 0; i < src.Length; i++)
        {
            dst[idx] += src[i] * g;
            idx++;
            if (idx == n) idx = 0;
        }
    }

    public static void Add(float[] dst, int start, float[] src, double gain = 1)
    {
        float g = (float)gain;
        for (int i = 0; i < src.Length; i++)
        {
            int j = start + i;
            if (j < 0) continue;
            if (j >= dst.Length) break;
            dst[j] += src[i] * g;
        }
    }

    public static float[] Biquad(float[] x, BQ kind, double fc, double q, int sr)
    {
        fc = Math.Min(fc, 0.45 * sr);
        double w0 = 2 * Math.PI * fc / sr, cw = Math.Cos(w0), sw = Math.Sin(w0), alpha = sw / (2 * q);
        double b0, b1, b2, a0 = 1 + alpha, a1 = -2 * cw, a2 = 1 - alpha;
        switch (kind)
        {
            case BQ.LP: b0 = (1 - cw) / 2; b1 = 1 - cw; b2 = b0; break;
            case BQ.HP: b0 = (1 + cw) / 2; b1 = -(1 + cw); b2 = b0; break;
            default: b0 = alpha; b1 = 0; b2 = -alpha; break;
        }
        b0 /= a0; b1 /= a0; b2 /= a0; a1 /= a0; a2 /= a0;
        var y = new float[x.Length];
        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
        for (int i = 0; i < x.Length; i++)
        {
            double xi = x[i];
            double yi = b0 * xi + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = xi; y2 = y1; y1 = yi;
            y[i] = (float)yi;
        }
        return y;
    }

    public static float[] CircBiquad(float[] x, BQ kind, double fc, double q, int sr) =>
        Circ(x, t => Biquad(t, kind, fc, q, sr));

    public static float[] OnePoleLP(float[] x, double fc, int sr)
    {
        double a = 1 - Math.Exp(-2 * Math.PI * fc / sr);
        var y = new float[x.Length];
        double s = 0;
        for (int i = 0; i < x.Length; i++) { s += a * (x[i] - s); y[i] = (float)s; }
        return y;
    }

    public static float Rms(float[] x)
    {
        double s = 0;
        foreach (var v in x) s += (double)v * v;
        return (float)Math.Sqrt(s / Math.Max(1, x.Length));
    }

    public static float Peak(float[] x)
    {
        float p = 0;
        foreach (var v in x) { float a = Math.Abs(v); if (a > p) p = a; }
        return p;
    }

    public static float[] ScaleRms(float[] x, double rms)
    {
        float r = Rms(x);
        if (r < 1e-12f) return x;
        float g = (float)(rms / r);
        for (int i = 0; i < x.Length; i++) x[i] *= g;
        return x;
    }

    public static float[] Normalize(float[] x, double dbfs)
    {
        float p = Peak(x);
        if (p < 1e-12f) return x;
        float g = (float)(Math.Pow(10, dbfs / 20) / p);
        for (int i = 0; i < x.Length; i++) x[i] *= g;
        return x;
    }

    public static float[] Fade(float[] x, int sr, double inMs, double outMs)
    {
        int ni = Math.Min(x.Length / 2, (int)(inMs * sr / 1000)), no = Math.Min(x.Length / 2, (int)(outMs * sr / 1000));
        for (int i = 0; i < ni; i++)
        {
            double t = (i + 0.5) / ni;
            x[i] *= (float)(0.5 - 0.5 * Math.Cos(Math.PI * t));
        }
        for (int i = 0; i < no; i++)
        {
            double t = (i + 0.5) / no;
            x[x.Length - 1 - i] *= (float)(0.5 - 0.5 * Math.Cos(Math.PI * t));
        }
        return x;
    }

    // Periodic smooth random signal in roughly [-1, 1].
    public static float[] SlowNoise(Random r, int n, double fc, int sr)
    {
        var w = WhiteNoise(r, n);
        var y = Circ(w, t => OnePoleLP(OnePoleLP(t, fc, sr), fc, sr));
        double s = 0;
        foreach (var v in y) s += (double)v * v;
        double std = Math.Sqrt(s / n);
        for (int i = 0; i < n; i++) y[i] = (float)Math.Clamp(y[i] / (3 * std), -1, 1);
        return y;
    }

    // Noise through a filter, scaled to a given RMS.
    public static float[] Filtered(Random r, int n, BQ kind, double fc, double q, int sr, double rms = 0.3)
    {
        var y = Biquad(WhiteNoise(r, n), kind, fc, q, sr);
        return ScaleRms(y, rms);
    }

    public static float[] CircFiltered(Random r, int n, BQ kind, double fc, double q, int sr, double rms = 0.3)
    {
        var y = CircBiquad(WhiteNoise(r, n), kind, fc, q, sr);
        return ScaleRms(y, rms);
    }

    public static float[] Reverb(float[] x, int sr, double rt60, double damp = 0.4)
    {
        double[] combMs = { 29.7, 37.1, 41.1, 43.7, 53.3, 61.7 };
        double[] apMs = { 5.0, 1.7 };
        float[] Proc(float[] t)
        {
            var o = new float[t.Length];
            foreach (var ms in combMs)
            {
                int d = (int)(ms * sr / 1000);
                var buf = new float[d];
                int idx = 0;
                double lp = 0;
                double g = Math.Pow(10, -3.0 * d / sr / rt60);
                for (int i = 0; i < t.Length; i++)
                {
                    float v = buf[idx];
                    lp = v * (1 - damp) + lp * damp;
                    buf[idx] = (float)(t[i] + g * lp);
                    o[i] += v;
                    if (++idx == d) idx = 0;
                }
            }
            foreach (var ms in apMs)
            {
                int d = (int)(ms * sr / 1000);
                var buf = new float[d];
                int idx = 0;
                for (int i = 0; i < t.Length; i++)
                {
                    float v = buf[idx];
                    float w = o[i] + 0.5f * v;
                    o[i] = v - 0.5f * w;
                    buf[idx] = w;
                    if (++idx == d) idx = 0;
                }
            }
            for (int i = 0; i < o.Length; i++) o[i] *= 0.3f;
            return o;
        }
        return Circ(x, Proc);
    }

    public static float[] HannBump(int n, int center, int width)
    {
        var e = new float[n];
        for (int k = -width / 2; k < width / 2; k++)
        {
            int i = ((center + k) % n + n) % n;
            e[i] += (float)(0.5 + 0.5 * Math.Cos(2 * Math.PI * k / width));
        }
        return e;
    }

    public static float[] Mul(float[] a, float[] b)
    {
        var y = new float[a.Length];
        for (int i = 0; i < a.Length; i++) y[i] = a[i] * b[i];
        return y;
    }
}

// State-variable filter for per-sample cutoff sweeps (Cytomic TPT).
sealed class Svf
{
    double ic1, ic2;
    public double Bp, Lp, Hp;

    public void Tick(double x, double fc, double q, int sr)
    {
        double g = Math.Tan(Math.PI * Math.Min(fc, 0.45 * sr) / sr), k = 1 / q;
        double a1 = 1 / (1 + g * (g + k)), a2 = g * a1, a3 = g * a2;
        double v3 = x - ic2;
        double v1 = a1 * ic1 + a2 * v3;
        double v2 = ic2 + a2 * ic1 + a3 * v3;
        ic1 = 2 * v1 - ic1;
        ic2 = 2 * v2 - ic2;
        Bp = k * v1; // unity peak gain
        Lp = v2;
        Hp = x - k * v1 - v2;
    }
}
