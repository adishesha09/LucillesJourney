namespace WorldArtCh3;

public enum Mat : byte { Sky, Facade, Sidewalk, Road, Curb, Ledge, Masonry, Stair, VFloor, Earth }

/// <summary>Dusk colour grading shared by the baked ground and by every re-tinted chapter-1 sprite.</summary>
public static class Dusk
{
    public static Rgba Street(Rgba c) => new(
        (byte)Math.Clamp(c.R * 0.64 + 6, 0, 255), (byte)Math.Clamp(c.G * 0.70 + 9, 0, 255), (byte)Math.Clamp(c.B * 0.88 + 26, 0, 255), c.A);
    public static Rgba Cellar(Rgba c) => new(
        (byte)Math.Clamp(c.R * 0.40 + 6, 0, 255), (byte)Math.Clamp(c.G * 0.46 + 8, 0, 255), (byte)Math.Clamp(c.B * 0.68 + 22, 0, 255), c.A);
    public static Rgba Mix(Rgba a, Rgba b, double t) => new(
        (byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t), a.A);
    /// <summary>Warm lamp light added with strength k (0..1).</summary>
    public static Rgba Warm(Rgba c, double k) => new(
        (byte)Math.Clamp(c.R * (1 + 0.55 * k) + 62 * k, 0, 255), (byte)Math.Clamp(c.G * (1 + 0.40 * k) + 38 * k, 0, 255), (byte)Math.Clamp(c.B * (1 + 0.05 * k) + 4 * k, 0, 255), c.A);
    public static Rgba Cool(Rgba c, double k) => new(
        (byte)Math.Clamp(c.R * (1 - 0.18 * k), 0, 255), (byte)Math.Clamp(c.G * (1 - 0.10 * k), 0, 255), (byte)Math.Clamp(c.B * (1 + 0.02 * k) + 6 * k, 0, 255), c.A);

    public static Canvas Tint(Canvas src, Func<Rgba, Rgba>? f = null)
    {
        f ??= Street;
        var o = new Canvas(src.W, src.H);
        for (int y = 0; y < src.H; y++) for (int x = 0; x < src.W; x++) { var p = src.Get(x, y); if (p.A != 0) o.Set(x, y, f(p)); }
        return o;
    }
}

public static partial class Ground
{
    public static Canvas Cv = new(G.W, G.H);
    public static Mat[,] M = new Mat[G.W, G.H];
    static readonly List<(int x, int y, int w, int h)> LitWindows = new();

    public static Rgba SkyPx(int x, int y)
    {
        double t = Math.Clamp(y / 50.0, 0, 1);
        Rgba[] stops = { Rgba.Hex("1F2250"), Rgba.Hex("3C3670"), Rgba.Hex("7A4E86"), Rgba.Hex("C8657A"), Rgba.Hex("F29A5C"), Rgba.Hex("FFC278") };
        double f = t * (stops.Length - 1) + Fx.Bay(x, y) * 0.55 + (Fx.Fbm(x / 30.0, y / 6.0, 3) - 0.5) * 0.5;
        int i = Math.Clamp((int)Math.Floor(f), 0, stops.Length - 2);
        double u = Math.Clamp(f - i, 0, 1);
        return Dusk.Mix(stops[i], stops[i + 1], u < 0.5 ? 0 : 1);
    }

    static void P(int x, int y, Rgba c, Mat m) { if (x < 0 || y < 0 || x >= G.W || y >= G.H) return; Cv.Set(x, y, c); M[x, y] = m; }
    static void P(int x, int y, Rgba c) { if (x < 0 || y < 0 || x >= G.W || y >= G.H) return; Cv.Set(x, y, c); }

    public static void Build()
    {
        for (int y = 0; y < G.H; y++) for (int x = 0; x < G.W; x++) { M[x, y] = Mat.Earth; Cv.Set(x, y, Rgba.Hex("3A3028")); }
        Sky();
        Facades.Paint();
        Street();
        SouthEdge();
        Vault();
        // cast shadows from the layout (before grading, so they shade the day colours)
        foreach (var s in Layout.Shadows)
        {
            if (s.Kind == 'e') ShadeEll(s.A, s.B, s.C, s.D, s.K);
            else ShadeRect((int)s.A, (int)s.B, (int)s.C, (int)s.D, s.K);
        }
        BuildingShadows();
        Grade();
        Lights();
    }

    // ---- sky ------------------------------------------------------------------------------------------------
    static void Sky()
    {
        for (int y = 0; y < 56; y++) for (int x = 0; x < G.W; x++) P(x, y, SkyPx(x, y), Mat.Sky);
        // far skyline: violet silhouettes with a few warm windows
        var far = Rgba.Hex("3D3566"); var farHi = Rgba.Hex("524A7C");
        int xx = 0;
        while (xx < G.W)
        {
            int w = 14 + (int)(Fx.R(xx, 1, 301) * 26);
            int top = 28 + (int)(Fx.R(xx, 2, 302) * 14);
            bool slope = Fx.R(xx, 3, 303) > 0.6;
            for (int x = xx; x < Math.Min(G.W, xx + w); x++)
            {
                int t = top + (slope ? (x - xx) / 6 : 0);
                for (int y = t; y < 56; y++) P(x, y, y == t ? farHi : far, Mat.Sky);
            }
            for (int wx = xx + 3; wx < xx + w - 3; wx += 6)
                for (int wy = top + 5; wy < 52; wy += 7)
                    if (Fx.R(wx, wy, 304) > 0.82) { P(wx, wy, Rgba.Hex("E0944E"), Mat.Sky); P(wx + 1, wy, Rgba.Hex("E0944E"), Mat.Sky); }
            xx += w;
        }
        // distant smoke columns leaning slightly, lit from below by the glow
        foreach (var (cx, h, lean) in new[] { (136, 34, 0.15), (344, 40, -0.1), (468, 30, 0.2), (612, 38, -0.12), (724, 28, 0.1) })
            for (int y = 56 - h; y < 52; y++)
            {
                double k = (y - (56 - h)) / (double)h;
                double w = 2.5 + (1 - k) * 4.5 + (Fx.Fbm(y / 4.0, cx, 7) - 0.5) * 3;
                double c = cx + (56 - y) * lean * -1;
                for (int x = (int)(c - w); x <= (int)(c + w); x++)
                {
                    double a = 1 - Math.Abs(x - c) / (w + 0.5);
                    if (a < 0.25 + (Fx.R(x, y, 8) - 0.5) * 0.35) continue;
                    var b = Cv.Get(x, y);
                    var tone = Dusk.Mix(b, Rgba.Hex("4A3E66"), 0.55 * (0.5 + 0.5 * (1 - k)));
                    if (((x + y) & 1) == 0 || a > 0.7) P(x, y, tone, Mat.Sky);
                }
            }
    }

    // ---- shadows & light helpers ---------------------------------------------------------------------------
    static bool Shadable(int x, int y) => x >= 0 && y >= 0 && x < G.W && y < G.H && M[x, y] != Mat.Facade && M[x, y] != Mat.Sky;

    public static void ShadeEll(double cx, double cy, double rx, double ry, double k = 1.0)
    {
        for (int y = (int)(cy - ry - 2); y <= cy + ry + 2; y++)
            for (int x = (int)(cx - rx - 2); x <= cx + rx + 2; x++)
            {
                if (!Shadable(x, y)) continue;
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry, d = dx * dx + dy * dy;
                if (d <= 1.0 || (d <= 1.35 && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(k));
            }
    }
    public static void ShadeRect(int x0, int y0, int w, int h, double k = 1.0, bool fringe = true)
    {
        for (int y = y0 - 1; y <= y0 + h; y++)
            for (int x = x0 - 1; x <= x0 + w; x++)
            {
                if (!Shadable(x, y)) continue;
                bool inside = x >= x0 && y >= y0 && x < x0 + w && y < y0 + h;
                if (inside || (fringe && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(k));
            }
    }

    static void BuildingShadows()
    {
        // buildings throw a long cool shadow over the north pavement
        for (int y = G.BaseY; y < G.BaseY + 16; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Sidewalk) continue;
                double t = (y - G.BaseY) / 16.0;
                if (t < 0.5 || (t < 0.8 && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(0.9 - t * 0.5));
            }
        // the street ledge shades the south pavement edge lightly; the stair slot is deep shadow
        for (int y = G.LedgeY - 6; y < G.LedgeY; y++)
            for (int x = 0; x < G.W; x++)
                if (M[x, y] == Mat.Sidewalk && ((x + y) & 1) == 0) Cv.Set(x, y, Cv.Get(x, y).Shade(0.45));
    }

    // ---- dusk grade ----------------------------------------------------------------------------------------
    static void Grade()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] == Mat.Sky) continue;
                var c = Cv.Get(x, y);
                var s = Dusk.Street(c);
                if (y >= G.LedgeY - 4)
                {
                    var cl = Dusk.Cellar(c);
                    double t = Math.Clamp((y - (G.LedgeY - 4)) / 54.0, 0, 1);
                    t = t + Fx.Bay(x, y) * 0.08;
                    s = Dusk.Mix(s, cl, Math.Clamp(t, 0, 1));
                }
                Cv.Set(x, y, s);
            }
    }

    public static void WarmPool(double cx, double cy, double rx, double ry, double k, Func<int, int, bool>? mask = null)
    {
        for (int y = (int)(cy - ry); y <= cy + ry; y++)
            for (int x = (int)(cx - rx); x <= cx + rx; x++)
            {
                if (x < 0 || y < 0 || x >= G.W || y >= G.H || M[x, y] == Mat.Sky) continue;
                if (mask != null && !mask(x, y)) continue;
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry, d = Math.Sqrt(dx * dx + dy * dy);
                if (d >= 1) continue;
                double f = (1 - d) * k + Fx.Bay(x, y) * 0.16;
                // four quantised light steps for a pixel-art falloff
                int step = (int)Math.Floor(f * 4.0);
                if (step <= 0) continue;
                Cv.Set(x, y, Dusk.Warm(Cv.Get(x, y), Math.Min(step, 4) * 0.19));
            }
    }

    static void Lights()
    {
        // lit windows are repainted after grading so they stay luminous
        foreach (var (x, y, w, h) in LitWindows) Facades.PaintLitWindow(x, y, w, h);
        LightsStreet();
        LightsVault();
    }
}
