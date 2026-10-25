using static MenuArt.Kit;

namespace MenuArt;

public static class MenuSky
{
    public const int SunX = 140, SunY = 130;

    public static Canvas Sky()
    {
        var c = new Canvas(384, 216);
        var stops = new (double, Rgba)[]
        {
            (0, C("1A2224")), (12, C("1D282B")), (26, C("2B3A3F")), (40, C("37495A")), (54, C("474D68")),
            (68, C("62577A")), (82, C("82607F")), (95, C("A66A7C")), (106, C("C47A72")), (116, C("D98F4B")),
            (124, C("E8AC55")), (131, C("F2D26B")), (140, C("EEC261")), (160, C("D98F4B")), (216, C("B8683F")),
        };
        for (int y = 0; y < 216; y++)
            for (int x = 0; x < 384; x++)
            {
                double dx = x - SunX;
                // the horizon glow climbs higher around the sun so the dither contours bow upward
                double lift = 24 * Gauss(dx, 85) * Smooth(40, 125, y + 20);
                double wob = (N(x / 70.0, y / 2.5, 3) - 0.5) * 3.0;
                c.Set(x, y, Grad(stops, y + lift + wob, x, y, 1.9));
            }
        return c;
    }

    public static (Canvas, Canvas) Stars()
    {
        var f0 = new Canvas(384, 216); var f1 = new Canvas(384, 216);
        var rnd = new Random(2024);
        var pts = new List<(int x, int y)>();
        var cream = C("F4EAD2"); var gold = C("F2E2A8"); var dim = C("8C8C96"); var dimGold = C("B8A98A");
        bool Title(int x, int y) => x >= 80 && x <= 304 && y >= 12 && y <= 70;
        bool Far(int x, int y, int d) { foreach (var p in pts) if (Math.Abs(p.x - x) < d && Math.Abs(p.y - y) < d) return false; return true; }
        int singles = 0, tries = 0, plusN = 0;
        // twinkle plus shapes first so they get good spacing
        while (plusN < 10 && tries++ < 5000)
        {
            int x = 6 + rnd.Next(372), y = 4 + (int)(Math.Pow(rnd.NextDouble(), 1.25) * 92);
            if (Title(x - 4, y - 4) || Title(x + 4, y + 4) || Title(x, y) || !Far(x, y, 28)) continue;
            pts.Add((x, y));
            bool alt = plusN % 2 == 0;
            var arm = plusN % 3 == 0 ? gold : cream;
            void Plus(Canvas c) { c.Set(x, y, cream); c.Set(x - 1, y, arm); c.Set(x + 1, y, arm); c.Set(x, y - 1, arm); c.Set(x, y + 1, arm); }
            var big = alt ? f0 : f1; var small = alt ? f1 : f0;
            Plus(big);
            small.Set(x, y, dimGold);
            plusN++;
        }
        tries = 0;
        while (singles < 84 && tries++ < 30000)
        {
            int x = 2 + rnd.Next(380), y = 2 + (int)(Math.Pow(rnd.NextDouble(), 1.35) * 118);
            if (Title(x, y) && rnd.NextDouble() < 0.6) continue;
            if (!Far(x, y, 7)) continue;
            // thin out towards the bright horizon
            if (rnd.NextDouble() > 1.15 - y / 110.0) continue;
            pts.Add((x, y));
            var col = rnd.NextDouble() < 0.3 ? gold : cream;
            switch (singles % 4)
            {
                case 0: f0.Set(x, y, col); f1.Set(x, y, col); break;
                case 1: f0.Set(x, y, col); f1.Set(x, y, dim); break;
                case 2: f0.Set(x, y, dim); f1.Set(x, y, col); break;
                default: if (singles % 8 == 3) f0.Set(x, y, col); else f1.Set(x, y, col); break;
            }
            singles++;
        }
        return (f0, f1);
    }

    /// <summary>Concentric dithered glow; additive/screen overlay. Sun disc is half hidden by the far hills.</summary>
    public static Canvas SunGlow()
    {
        var c = new Canvas(384, 216);
        var bands = Ramp("F8DE8C", "F2D26B", "F0B65E", "E99A52", "D98456", "C0604F");
        for (int y = 0; y < 216; y++)
            for (int x = 0; x < 384; x++)
            {
                double dx = x - SunX, dy = y - SunY;
                double tr = Math.Sqrt(dx * dx + dy * dy);
                if (tr <= 12)
                {
                    double t = tr / 12;
                    c.Set(x, y, t < 0.45 ? C("FFF3C6") : t < 0.8 ? C("FBE59A") : C("F6D878"));
                    continue;
                }
                double d = Math.Sqrt(dx * dx + dy * dy * 1.8);
                double v = (d - 12) / 118;
                if (v >= 1) continue;
                double dens = Math.Pow(1 - v, 2.1) * 0.96;
                // wide, flatter bloom hugging the horizon
                dens += 0.22 * Gauss(dy, 9) * Gauss(dx, 140) * (1 - v);
                dens = Math.Floor(dens * 10) / 10.0;
                if (dens <= Bay8(x, y)) continue;
                double bi = Math.Pow(v, 0.8) * (bands.Length - 1);
                c.Set(x, y, bands[Math.Min((int)Math.Round(bi), bands.Length - 1)]);
            }
        return c;
    }

    public static void Cloud(Canvas c, double cx, double cy, double hw, double th, double bh, int seed, Rgba[] ramp, Rgba rim, Rgba rim2)
    {
        const int W = 768;
        int n = ramp.Length;
        double WrapD(double a, double b) { double d = (a - b) % W; if (d > W / 2) d -= W; if (d < -W / 2) d += W; return d; }
        for (int y = 0; y < c.H; y++)
            for (int x = 0; x < W; x++)
            {
                double dx = WrapD(x, cx);
                if (Math.Abs(dx) > hw) continue;
                double u = dx / hw;
                double env = Math.Pow(1 - u * u, 0.75);
                double nz = WrapNoise(x + seed * 36, 0, 12, 1, 64, seed) - 0.5;
                double nz2 = WrapNoise(x + seed * 12, 0, 6, 1, 128, seed + 3) - 0.5;
                double top = th * env * (0.72 + nz * 1.1) + nz2 * 1.4;
                double bot = bh * Math.Pow(env, 0.6) * (0.9 + nz2 * 0.5);
                double dy = y + 0.5 - cy;
                if (dy < -top || dy > bot) continue;
                if (top < 0.6 || bot < 0.4) { }
                // vertical tone: 0 top (dusky, shadowed) .. 1 underside (sun-lit)
                double tt = (dy + top) / Math.Max(0.5, top + bot);
                double streak = WrapNoise(x, y, 16, 1.6, 48, seed + 9) - 0.5;
                double t = Clamp01(tt * 0.92 + streak * 0.30 + (Bay8(x, y) - 0.5) * 0.10 - 0.04 + 0.08 * (1 - env));
                int idx = Math.Clamp((int)(t * n), 0, n - 1);
                c.Set(x, y, ramp[idx]);
            }
        // sun-lit rim: bottom edge and tapered ends
        var snap = c.Clone();
        for (int y = 0; y < c.H; y++)
            for (int x = 0; x < W; x++)
            {
                if (!snap.Opaque(x, y)) continue;
                bool below = !snap.Opaque(x, y + 1), left = !snap.Opaque((x + W - 1) % W, y), right = !snap.Opaque((x + 1) % W, y), above = !snap.Opaque(x, y - 1);
                if (below && Fx.R(x, y, seed + 5) < 0.88) c.Set(x, y, ((x + y) & 1) == 0 ? rim : rim2);
                else if ((left || right) && !above && Fx.R(x, y, seed + 8) < 0.5) c.Set(x, y, rim2);
            }
    }

    public static Canvas Clouds()
    {
        var c = new Canvas(768, 72);
        var ramp = Ramp("5E4C70", "7A5E80", "96687F", "B2757F", "CC8A7C", "E3A074", "F2BE82");
        var rim = C("F8DA90"); var rim2 = C("F2B878");
        // cx, cy, halfWidth, topH, bottomH  (wide, flat stratus)
        var defs = new (double cx, double cy, double hw, double th, double bh)[]
        {
            (64, 30, 72, 14, 7), (214, 50, 56, 9, 5), (338, 26, 84, 15, 8), (474, 56, 48, 8, 4.5),
            (568, 34, 68, 13, 7), (694, 14, 44, 8, 5),
        };
        int s = 1;
        foreach (var d in defs) Cloud(c, d.cx, d.cy, d.hw, d.th, d.bh, s++ * 7, ramp, rim, rim2);
        return c;
    }

    public static Canvas Vignette()
    {
        var c = new Canvas(384, 216);
        var col = C("1A1412");
        for (int y = 0; y < 216; y++)
            for (int x = 0; x < 384; x++)
            {
                double nx = (x + .5 - 192) / 192.0, ny = (y + .5 - 108) / 108.0;
                double d = Math.Sqrt(nx * nx * 0.9 + ny * ny * 0.9) / Math.Sqrt(1.8);
                double p = 0.55 * Math.Pow(Smooth(0.62, 1.0, d), 1.2);
                p = Math.Floor(p * 16 + 0.5) / 16.0;
                if (p > Bay8(x, y)) c.Set(x, y, col);
            }
        return c;
    }

    public static Canvas Firefly()
    {
        var c = new Canvas(3, 3);
        c.Set(1, 1, C("FBE9A8"));
        foreach (var (x, y) in new[] { (1, 0), (0, 1), (2, 1), (1, 2) }) c.Set(x, y, C("E8B24C"));
        return c;
    }

    public static Canvas FireflyGlow()
    {
        var c = new Canvas(7, 7);
        for (int y = 0; y < 7; y++)
            for (int x = 0; x < 7; x++)
            {
                double d = Math.Sqrt((x - 3) * (x - 3) + (y - 3) * (y - 3));
                if (d <= 1.0) c.Set(x, y, C("F2D26B"));
                else if (d <= 2.3) { if (((x + y) & 1) == 0) c.Set(x, y, C("E9B050")); }
                else if (d <= 3.2) { if ((x & 1) == 1 && (y & 1) == 1) c.Set(x, y, C("D98F4B")); }
            }
        return c;
    }

    public static Canvas Ornament()
    {
        // left half drawn on 80x14, then mirrored for exact symmetry
        var h = new Canvas(80, 14);
        var cream = C("F4EAD2"); var gold = C("F2D26B"); var tan = C("A8895E"); var dkTan = C("7A5F48"); var amber = C("D98F4B");
        // stem: a long, slightly wavy line
        int Sy(int x) => 7 + (int)Math.Round(Math.Sin(x / 7.5) * 1.0);
        for (int x = 31; x <= 69; x++) h.Set(x, Sy(x), tan);
        // wheat: leaf pairs lean outward (to the left) and shrink along the sprig
        int[] gx = { 66, 59, 52, 46, 41, 36, 32 };
        int[] offs = { 0, 1, 2, 2, 3 };
        for (int i = 0; i < gx.Length; i++)
        {
            int x = gx[i], y = Sy(x), sz = i < 3 ? 4 : i < 6 ? 3 : 2;
            for (int k = 1; k <= sz; k++)
            {
                int ux = x - k, off = offs[k];
                h.Set(ux, y - off, k == sz ? cream : gold);
                h.Set(ux, y + off, k == sz ? gold : amber);
                if (k < sz) { h.Set(ux, y - off + 1, amber); h.Set(ux, y + off - 1, tan); }
            }
            h.Set(x, y - 1, gold); h.Set(x, y + 1, amber);
        }
        // terminal ear
        int ty = Sy(30);
        foreach (var (ex, ey) in new[] { (30, 0), (29, 0), (28, 0), (28, -1), (28, 1), (26, 0), (27, -1), (27, 1) }) h.Set(ex, ty + ey, ex >= 28 ? gold : cream);
        // dotted fade: spacing grows, brightness drops
        foreach (var (dx, col) in new[] { (22, cream), (18, gold), (14, tan), (10, tan), (6, dkTan), (2, dkTan) }) h.Set(dx, Sy(dx), col);
        h.Set(20, Sy(20) - 2, tan); h.Set(16, Sy(16) + 2, dkTan); h.Set(12, Sy(12) - 2, dkTan);
        // central lantern diamond (its right edge is the centre axis of the whole sprite)
        int[] half = { 1, 2, 3, 4, 5, 6, 6, 6, 6, 5, 4, 3, 2, 1 };
        for (int j = 0; j < 14; j++)
            for (int i = 0; i < half[j]; i++)
            {
                bool edge = i == half[j] - 1;
                int inner = Math.Min(j, 13 - j);
                Rgba col = edge ? dkTan : (i >= half[j] - 2 ? amber : (i < 2 && inner > 3 ? C("FBF1C8") : (i < 3 ? cream : gold)));
                if (j == 0 || j == 13) col = tan;
                h.Set(79 - i, j, col);
            }
        h.Set(79, 6, C("FFF8E0")); h.Set(79, 7, C("FFF8E0")); h.Set(78, 6, C("FBF1C8")); h.Set(78, 7, C("FBF1C8"));
        for (int x = 70; x <= 73; x++) h.Set(x, 7, amber);
        var c = new Canvas(160, 14);
        c.Blit(h, 0, 0); c.Blit(h.FlipH(), 80, 0);
        return c;
    }
}
