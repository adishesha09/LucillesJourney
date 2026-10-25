namespace MenuArt;

/// <summary>Shared helpers: ordered dithering, gradients, noise, clumped foliage, polygons.</summary>
public static class Kit
{
    public static Rgba C(string h) => Rgba.Hex(h);
    public static Rgba[] Ramp(params string[] hex) => hex.Select(C).ToArray();

    static readonly int[] B8 =
    {
         0, 32,  8, 40,  2, 34, 10, 42,
        48, 16, 56, 24, 50, 18, 58, 26,
        12, 44,  4, 36, 14, 46,  6, 38,
        60, 28, 52, 20, 62, 30, 54, 22,
         3, 35, 11, 43,  1, 33,  9, 41,
        51, 19, 59, 27, 49, 17, 57, 25,
        15, 47,  7, 39, 13, 45,  5, 37,
        63, 31, 55, 23, 61, 29, 53, 21,
    };
    /// <summary>8x8 Bayer threshold in (0,1).</summary>
    public static double Bay8(int x, int y) => (B8[((y & 7) << 3) | (x & 7)] + 0.5) / 64.0;

    public static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
    public static double Smooth(double a, double b, double x) { double t = Clamp01((x - a) / (b - a)); return t * t * (3 - 2 * t); }
    public static double Lerp(double a, double b, double t) => a + (b - a) * t;
    public static Rgba Mix(Rgba a, Rgba b, double t) => new(
        (byte)Math.Round(Lerp(a.R, b.R, t)), (byte)Math.Round(Lerp(a.G, b.G, t)), (byte)Math.Round(Lerp(a.B, b.B, t)));

    /// <summary>Picks b over a with probability t using the ordered-dither matrix (hard edged).</summary>
    public static Rgba Pick(Rgba a, Rgba b, double t, int x, int y) => t > Bay8(x, y) ? b : a;

    /// <summary>Ordered-dither between neighbouring colours of an evenly spaced ramp. sharp&gt;1 narrows the dither band.</summary>
    public static Rgba DRamp(Rgba[] cols, double t, int x, int y, double sharp = 1.6)
    {
        t = Clamp01(t);
        double f = t * (cols.Length - 1);
        int i = Math.Min((int)f, cols.Length - 2);
        double fr = f - i;
        fr = Clamp01((fr - 0.5) * sharp + 0.5);
        return fr > Bay8(x, y) ? cols[i + 1] : cols[i];
    }

    /// <summary>Piecewise gradient with explicit y stops, dithered between stop colours.</summary>
    public static Rgba Grad((double at, Rgba c)[] stops, double v, int x, int y, double sharp = 1.7)
    {
        if (v <= stops[0].at) return stops[0].c;
        for (int i = 0; i < stops.Length - 1; i++)
            if (v < stops[i + 1].at)
            {
                double fr = (v - stops[i].at) / (stops[i + 1].at - stops[i].at);
                fr = Clamp01((fr - 0.5) * sharp + 0.5);
                return fr > Bay8(x, y) ? stops[i + 1].c : stops[i].c;
            }
        return stops[^1].c;
    }

    public static double N(double x, double y, int s) => Fx.VN(x, y, s);
    public static double Fbm1(double x, int s, double lam)
    {
        double v = Fx.VN(x / lam, s * 3.7, s) * 0.62 + Fx.VN(x / (lam * 0.45), s * 3.7 + 11, s + 5) * 0.27 + Fx.VN(x / (lam * 0.2), s * 3.7 + 23, s + 9) * 0.11;
        return Clamp01((v - 0.5) * 1.8 + 0.5); // value noise sits near 0.5; stretch it so ridges use their full range
    }

    /// <summary>Value noise on a lattice that wraps every <paramref name="cellsX"/> cells horizontally (seamless tiling).</summary>
    public static double WrapNoise(double x, double y, double cellW, double cellH, int cellsX, int seed)
    {
        double gx = x / cellW, gy = y / cellH;
        int xi = (int)Math.Floor(gx), yi = (int)Math.Floor(gy);
        double fx = gx - xi, fy = gy - yi;
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        int W(int v) => ((v % cellsX) + cellsX) % cellsX;
        double a = Fx.R(W(xi), yi, seed), b = Fx.R(W(xi + 1), yi, seed), c = Fx.R(W(xi), yi + 1, seed), d = Fx.R(W(xi + 1), yi + 1, seed);
        double top = a + (b - a) * fx, bot = c + (d - c) * fx;
        return top + (bot - top) * fy;
    }

    public static bool InPoly(IReadOnlyList<(double x, double y)> p, double qx, double qy)
    {
        bool inside = false;
        for (int i = 0, j = p.Count - 1; i < p.Count; j = i++)
            if ((p[i].y > qy) != (p[j].y > qy) && qx < (p[j].x - p[i].x) * (qy - p[i].y) / (p[j].y - p[i].y) + p[i].x) inside = !inside;
        return inside;
    }

    public readonly record struct Clump(double X, double Y, double R);

    public static List<Clump> MakeClumps(double cx, double cy, double rx, double ry, int n, double rMin, double rMax, int seed)
    {
        var rnd = new Random(seed);
        var l = new List<Clump>();
        for (int i = 0; i < n; i++)
        {
            double a = rnd.NextDouble() * Math.PI * 2, d = Math.Sqrt(rnd.NextDouble());
            double r = rMin + (rMax - rMin) * rnd.NextDouble();
            double ex = Math.Cos(a) * d * (rx - r * 0.6), ey = Math.Sin(a) * d * (ry - r * 0.6);
            l.Add(new Clump(cx + ex, cy + ey, r));
        }
        l.Sort((p, q) => p.Y.CompareTo(q.Y));
        return l;
    }

    /// <summary>
    /// Back-lit foliage mass: dark body in hue-shifted tones, with a thin warm rim on each clump's sun-facing side.
    /// ramp = dark..light (>=4); rim = warm rim colour; sunDir = -1 sun on left, +1 sun on right.
    /// </summary>
    public static void Foliage(Canvas c, List<Clump> clumps, Rgba[] ramp, Rgba rim, Rgba rim2, int sunDir, int seed, double rimAmt = 1.0)
    {
        int n = ramp.Length;
        foreach (var k in clumps)
        {
            int x0 = (int)(k.X - k.R - 1), x1 = (int)(k.X + k.R + 2), y0 = (int)(k.Y - k.R - 1), y1 = (int)(k.Y + k.R + 2);
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    double dx = (x + .5 - k.X) / k.R, dy = (y + .5 - k.Y) / k.R;
                    if (dx * dx + dy * dy > 1) continue;
                    double light = -(sunDir * dx * 0.85 + dy * 0.55);
                    double t = 0.30 + 0.40 * light + (Fx.R(x / 2, y / 2, seed) - 0.5) * 0.18 + (Bay8(x, y) - 0.5) * 0.14;
                    int idx = Math.Clamp((int)Math.Floor(t * n), 0, n - 1);
                    c.Set(x, y, ramp[idx]);
                }
        }
        // thin sun-side rim on the silhouette and on clump tops (painted from the final mask)
        var snap = c.Clone();
        bool Solid(int x, int y) => snap.Opaque(x, y);
        foreach (var k in clumps)
        {
            int x0 = (int)(k.X - k.R - 2), x1 = (int)(k.X + k.R + 3), y0 = (int)(k.Y - k.R - 2), y1 = (int)(k.Y + k.R + 3);
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    if (!Solid(x, y)) continue;
                    double dx = (x + .5 - k.X) / k.R, dy = (y + .5 - k.Y) / k.R, r2 = dx * dx + dy * dy;
                    if (r2 > 1 || r2 < 0.55) continue;
                    double light = -(sunDir * dx * 0.9 + dy * 0.45);
                    if (light < 0.62) continue;
                    // only if the pixel toward the sun inside the same clump edge is the ramp's darker neighbour => thin crescent
                    double dx2 = (x + .5 + sunDir * -1.6 - k.X) / k.R, dy2 = (y + .5 + 0.9 - k.Y) / k.R;
                    bool edge = dx2 * dx2 + dy2 * dy2 > 1;
                    bool silhouette = !Solid(x + sunDir * -1, y) || !Solid(x, y - 1);
                    if (!(edge || silhouette)) continue;
                    if (Fx.R(x, y, seed + 31) > 0.62 * rimAmt + 0.3) continue;
                    c.Set(x, y, silhouette || Bay8(x, y) > 0.5 ? rim : rim2);
                }
        }
    }

    /// <summary>Mask-driven drop of a darker colour on a canvas region using dither on the edge (ground shadows).</summary>
    public static void ShadowEllipse(Canvas c, double cx, double cy, double rx, double ry, Rgba[] darkRamp, double slant = 0)
    {
        for (int y = (int)(cy - ry - 2); y <= (int)(cy + ry + 2); y++)
            for (int x = (int)(cx - rx - ry * Math.Abs(slant) - 3); x <= (int)(cx + rx + ry * Math.Abs(slant) + 3); x++)
            {
                if (!c.Opaque(x, y)) continue;
                double dx = (x + .5 - cx - slant * (y - cy)) / rx, dy = (y + .5 - cy) / ry;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d > 1.0) continue;
                double depth = 1 - d;
                var cur = c.Get(x, y);
                int lv = depth > 0.45 ? 2 : depth > 0.2 ? 1 : 0;
                if (lv == 0 && Bay8(x, y) > 0.5) continue;
                c.Set(x, y, Mix(cur, darkRamp[Math.Min(lv, darkRamp.Length - 1)], lv == 2 ? 0.72 : lv == 1 ? 0.55 : 0.38));
            }
    }

    public static double Gauss(double d, double s) => Math.Exp(-(d * d) / (2 * s * s));
}
