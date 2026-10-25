namespace AudioGen;

static class Inst
{
    public static double Freq(string note)
    {
        int i = 0;
        int semi = char.ToUpperInvariant(note[i++]) switch
        {
            'C' => 0, 'D' => 2, 'E' => 4, 'F' => 5, 'G' => 7, 'A' => 9, 'B' => 11,
            _ => throw new ArgumentException("bad note " + note)
        };
        while (i < note.Length && (note[i] == '#' || note[i] == 'b'))
        {
            semi += note[i] == '#' ? 1 : -1;
            i++;
        }
        int oct = int.Parse(note.Substring(i));
        int midi = 12 * (oct + 1) + semi;
        return 440.0 * Math.Pow(2, (midi - 69) / 12.0);
    }

    // Adds a decaying sine partial (recursive oscillator, cheap) into buf starting at startIdx.
    public static void Partial(float[] buf, double f, double amp, double tau, double phase, int sr,
        int startIdx = 0, double attackSec = 0.003)
    {
        if (f >= 0.48 * sr || amp == 0) return;
        double w = 2 * Math.PI * f / sr;
        double c = Math.Cos(w), s = Math.Sin(w);
        double re = Math.Cos(phase), im = Math.Sin(phase);
        double d = tau > 0 ? Math.Exp(-1.0 / (tau * sr)) : 1;
        double a = amp;
        int att = Math.Max(1, (int)(attackSec * sr));
        for (int i = startIdx; i < buf.Length; i++)
        {
            int k = i - startIdx;
            double env = k < att ? 0.5 - 0.5 * Math.Cos(Math.PI * k / att) : 1;
            buf[i] += (float)(a * env * im);
            double nre = re * c - im * s, nim = re * s + im * c;
            re = nre; im = nim;
            a *= d;
            if (a < 1e-6 && k > att) break;
        }
    }

    public static float[] Piano(double f, double dur, double vel, int sr, Random r)
    {
        double tau1 = Math.Clamp(2.4 * Math.Pow(261.6 / f, 0.4), 0.9, 4.5);
        double len = Math.Min(dur + 1.4, tau1 * 4 + 0.2);
        int n = (int)(len * sr);
        var b = new float[n];
        double B = f < 200 ? 0.0004 : 0.0001;
        double bright = 0.42 + 0.3 * vel;
        for (int s = -1; s <= 1; s += 2)
        {
            double det = 1 + s * 0.0004;
            for (int k = 1; k <= 12; k++)
            {
                double fk = f * k * Math.Sqrt(1 + B * k * k) * det;
                if (fk > 4500) break;
                double amp = Math.Pow(k, -1.15) * Math.Pow(bright, k - 1) * 0.5;
                double tau = tau1 / (1 + 0.8 * (k - 1));
                Partial(b, fk, amp, tau, r.NextDouble() * 6.28, sr, 0, 0.004);
            }
        }
        // soft felt thump
        var th = Dsp.OnePoleLP(Dsp.WhiteNoise(r, (int)(0.03 * sr)), 700, sr);
        for (int i = 0; i < th.Length && i < n; i++) b[i] += th[i] * (float)(0.35 * Math.Exp(-i / (0.006 * sr)));
        int ds = (int)(dur * sr);
        for (int i = ds; i < n; i++) b[i] *= (float)Math.Exp(-(i - ds) / (0.45 * sr));
        Dsp.Fade(b, sr, 0, 120);
        return Dsp.Normalize(b, 20 * Math.Log10(0.6 * vel));
    }

    // Karplus-Strong string with fractional-delay tuning allpass.
    public static float[] Pluck(double f, double dur, double vel, int sr, Random r)
    {
        double t60 = Math.Clamp(3.2 * Math.Pow(220 / f, 0.3), 1.4, 4.2);
        double len = Math.Min(dur + 1.2, t60);
        int n = (int)(len * sr);
        double D = sr / f - 0.5;
        int L = (int)Math.Floor(D);
        double frac = D - L;
        if (frac < 0.1) { L -= 1; frac += 1; }
        double a = (1 - frac) / (1 + frac);
        var buf = new float[L];
        var ex = Dsp.WhiteNoise(r, L);
        for (int pass = 0; pass < 2; pass++)
        {
            float prev = 0;
            for (int i = 0; i < L; i++) { float cur = ex[i]; ex[i] = 0.5f * (cur + prev); prev = cur; }
        }
        double mean = 0; foreach (var v in ex) mean += v; mean /= L;
        for (int i = 0; i < L; i++) buf[i] = (float)(ex[i] - mean);
        double g = Math.Pow(10, -3.0 / (f * t60));
        var o = new float[n];
        int idx = 0;
        double px = 0, apx = 0, apy = 0;
        for (int i = 0; i < n; i++)
        {
            double x = buf[idx];
            double lp = 0.5 * (x + px); px = x;
            double y = a * lp + apx - a * apy;
            apx = lp; apy = y;
            buf[idx] = (float)(y * g);
            o[i] = (float)x;
            if (++idx == L) idx = 0;
        }
        o = Dsp.OnePoleLP(o, 2200 + 2600 * vel, sr);
        int ds = (int)(dur * sr);
        for (int i = ds; i < n; i++) o[i] *= (float)Math.Exp(-(i - ds) / (0.35 * sr));
        // gentle 3 ms attack, 100 ms release
        Dsp.Fade(o, sr, 3, 100);
        return Dsp.Normalize(o, 20 * Math.Log10(0.65 * vel));
    }

    // Warm pad: detuned voices with a few harmonics, raised-cosine attack/release.
    // dur is the time at which release starts.
    public static float[] Pad(double[] freqs, double dur, double attack, double release, double brightHz,
        int sr, Random r, double amp)
    {
        int n = (int)((dur + release) * sr);
        var b = new float[n];
        double[] cents = { -6, 0, 6 };
        foreach (double f0 in freqs)
        {
            foreach (double c in cents)
            {
                double f = f0 * Math.Pow(2, (c + (r.NextDouble() - 0.5) * 2) / 1200);
                double ph = r.NextDouble() * 6.28;
                for (int h = 1; h <= 6; h++)
                    Partial(b, f * h, Math.Pow(h, -1.7) / freqs.Length, 0, ph * h, sr, 0, 0.001);
            }
        }
        b = Dsp.OnePoleLP(Dsp.OnePoleLP(b, brightHz, sr), brightHz, sr);
        int na = (int)(attack * sr), nr = (int)(release * sr), ns = (int)(dur * sr);
        for (int i = 0; i < n; i++)
        {
            double e = 1;
            if (i < na) e = 0.5 - 0.5 * Math.Cos(Math.PI * i / na);
            if (i >= ns) e *= 0.5 + 0.5 * Math.Cos(Math.PI * Math.Min(1.0, (i - ns) / (double)nr));
            b[i] *= (float)e;
        }
        float p = Dsp.Peak(b);
        float gain = (float)(0.22 * amp / Math.Max(p, 1e-9f));
        for (int i = 0; i < n; i++) b[i] *= gain;
        return b;
    }

    // Exactly loop-periodic partials: every frequency is quantised to an integer number of cycles per loop.
    public static float[] Periodic(int n, int sr, params (double f, double amp, int lfoCycles, double lfoPhase, double depth)[] parts)
    {
        var o = new float[n];
        double T = n / (double)sr;
        foreach (var p in parts)
        {
            long k = Math.Max(1, (long)Math.Round(p.f * T));
            double ph = p.lfoPhase;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / n;
                double m = 1 + p.depth * Math.Sin(2 * Math.PI * p.lfoCycles * t + ph);
                o[i] += (float)(p.amp * m * Math.Sin(2 * Math.PI * k * t + ph * 0.37));
            }
        }
        return o;
    }
}

static class Wav
{
    public static void Write(string path, float[] x, int sr)
    {
        using var fs = File.Create(path);
        using var w = new BinaryWriter(fs);
        int dataLen = x.Length * 2;
        w.Write(new[] { 'R', 'I', 'F', 'F' });
        w.Write(36 + dataLen);
        w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(sr);
        w.Write(sr * 2);
        w.Write((short)2);
        w.Write((short)16);
        w.Write(new[] { 'd', 'a', 't', 'a' });
        w.Write(dataLen);
        foreach (var v in x)
            w.Write((short)Math.Clamp((int)Math.Round(v * 32767.0), -32767, 32767));
    }

    public static (short[] data, int sr, int channels, int bits) Read(string path)
    {
        var b = File.ReadAllBytes(path);
        int pos = 12, sr = 0, ch = 0, bits = 0;
        short[] data = null;
        while (pos + 8 <= b.Length)
        {
            string id = System.Text.Encoding.ASCII.GetString(b, pos, 4);
            int len = BitConverter.ToInt32(b, pos + 4);
            if (id == "fmt ")
            {
                ch = BitConverter.ToInt16(b, pos + 10);
                sr = BitConverter.ToInt32(b, pos + 12);
                bits = BitConverter.ToInt16(b, pos + 22);
            }
            else if (id == "data")
            {
                data = new short[len / 2];
                Buffer.BlockCopy(b, pos + 8, data, 0, len);
            }
            pos += 8 + len;
        }
        return (data, sr, ch, bits);
    }
}
