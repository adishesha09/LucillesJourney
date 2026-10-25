namespace WorldArtCh6;

public static partial class Ground
{
    // ---- thin public wrappers used by Buildings / Street ----------------------------------------------------
    public static void SetExtPublic(int x, int y, bool e) { if (In(x, y)) Ext[x, y] = e; }
    public static void PxPublic(int x, int y, Rgba c, Mat m, bool ext) => Px(x, y, c, m, ext);
    public static void LitPublic(int x, int y, Rgba c) => Lit(x, y, c);

    public static void DiscWall(int cx, int cy, double r, Rgba mid, Rgba hi, Rgba lo)
    {
        for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
            for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
            {
                double dx = x + .5 - cx - .5, dy = y + .5 - cy - .5;
                if (dx * dx + dy * dy > r * r) continue;
                var c = dx + dy < -r * 0.6 ? hi : dx + dy > r * 0.5 ? lo : mid;
                if (In(x, y)) { Cv.Set(x, y, c); M[x, y] = Mat.Wall; }
            }
    }

    public static void DiscLit(int cx, int cy, double r, Rgba mid, Rgba hi, Rgba lo)
    {
        for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
            for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
            {
                double dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy > r * r) continue;
                Lit(x, y, dx + dy < -r * 0.6 ? hi : dx + dy > r * 0.5 ? lo : mid);
            }
    }

    // ---- cobbles ---------------------------------------------------------------------------------------------
    public static Rgba CobblePxPublic(int x, int y, bool slab) => CobblePx(x, y, slab, 3);

    static Rgba CobblePx(int x, int y, bool slab, int seed)
    {
        int rowH = slab ? 7 : 8;
        int ry = y / rowH, ly = y % rowH;
        int off = (int)(Fx.R(ry, 0, seed) * 13);
        int sx = -off, id = 0;
        while (true)
        {
            int w = (slab ? 12 : 9) + (int)(Fx.R(ry, id, seed + 1) * (slab ? 12 : 6));
            if (x < sx + w) return StonePx(x - sx, ly, w, rowH, Fx.H(ry, id, seed + 2), slab, K.Stone, K.Mortar);
            sx += w; id++;
        }
    }

    static Rgba StonePx(int lx, int ly, int w, int h, uint hash, bool slab, Rgba[] ramp, Rgba mortar)
    {
        if (lx == 0 || ly == 0) return mortar;
        bool corner = (lx == 1 || lx == w - 1) && (ly == 1 || ly == h - 1);
        if (corner) return mortar;
        int baseIdx = ramp.Length > 5 ? 2 + (int)(hash % 3) : 1 + (int)(hash % 3);
        if (slab) baseIdx = ramp.Length > 5 ? 3 + (int)(hash % 2) : 2 + (int)(hash % 2);
        int idx = baseIdx;
        if (ly == 1 || lx == 1) idx++;
        if (ly == h - 1 || lx == w - 1) idx--;
        int sx = 2 + (int)((hash >> 4) % (uint)Math.Max(1, w - 4)), sy = 2 + (int)((hash >> 9) % (uint)Math.Max(1, h - 3));
        if ((hash >> 14 & 3) == 0 && ((lx == sx && ly == sy) || (lx == sx + 1 && ly == sy + 1))) idx--;
        if ((hash >> 18 & 3) == 0 && lx == sx + 1 && ly == sy) idx++;
        var c = ramp[Math.Clamp(idx, 0, ramp.Length - 1)];
        return ramp == K.Stone ? Rgba.Mul(c, 1.03, 0.98, 0.90) : c;
    }

    public static Rgba CobbleDark(int x, int y)
    {
        int rowH = 8, ry = y / rowH, ly = y % rowH;
        int off = (int)(Fx.R(ry, 0, 211) * 13), sx = -off, id = 0;
        while (true)
        {
            int w = 8 + (int)(Fx.R(ry, id, 212) * 6);
            if (x < sx + w) return StonePx(x - sx, ly, w, rowH, Fx.H(ry, id, 213), false, E.Cobble, E.Cobble[0].Shade(0.6));
            sx += w; id++;
        }
    }

    public static Rgba PaveSlab(int x, int y)
    {
        int rowH = 12, ry = y / rowH, ly = y % rowH;
        int off = ry % 2 == 0 ? 0 : 9, gx = x + off, lx = gx % 18, id = gx / 18;
        uint h = Fx.H(id, ry, 221);
        var joint = E.Paving[0];
        if (ly == 0 || lx == 0) return joint;
        int idx = 2 + (int)(h % 3) - 1;
        if (ly == 1 || lx == 1) idx++;
        if (ly == rowH - 1 || lx == 17) idx--;
        if ((h >> 5 & 7) == 0 && lx == 7 + (int)(h >> 8 & 3) && ly == 5) idx--;
        if (Fx.VN(x / 9.0, y / 7.0, 222) > 0.78 && idx >= 2) idx--;
        return E.Paving[Math.Clamp(idx, 0, 4)];
    }

    public static Rgba StairWood(int x, int tone, int shade)
    {
        int t = Math.Clamp(tone + 1 - shade / 2, 0, 5);
        var c = K.Wood[t];
        if ((x / 9 + tone) % 3 == 0 && tone == 2) c = K.Wood[Math.Max(0, t - 1)];
        return c;
    }

    // ---- lights --------------------------------------------------------------------------------------------------
    public static void LightSource(double cx, double cy, double rx, double ry, double k, Rc? clip = null) => Lights.Add(cx, cy, rx, ry, k, clip, false);
    public static void LightCool(double cx, double cy, double rx, double ry, double k, Rc? clip = null) => Lights.Add(cx, cy, rx, ry, k, clip, true);
}

public static class Lights
{
    record Src(double Cx, double Cy, double Rx, double Ry, double K, Rc? Clip, bool Cool);
    static readonly List<Src> All = new();
    public static void Add(double cx, double cy, double rx, double ry, double k, Rc? clip, bool cool) => All.Add(new Src(cx, cy, rx, ry, k, clip, cool));

    static double Pool(Src s, int x, int y)
    {
        double dx = (x + .5 - s.Cx) / s.Rx, dy = (y + .5 - s.Cy) / s.Ry;
        double d = Math.Sqrt(dx * dx + dy * dy);
        if (d >= 1) return 0;
        double t = 1 - d;
        return s.K * t * (0.55 + 0.45 * t);
    }

    public static void Apply()
    {
        var Cv = Ground.Cv;
        // gather lights
        foreach (var s in All)
        {
            int x0 = Math.Max(0, (int)(s.Cx - s.Rx - 1)), x1 = Math.Min(G.W - 1, (int)(s.Cx + s.Rx + 1));
            int y0 = Math.Max(0, (int)(s.Cy - s.Ry - 1)), y1 = Math.Min(G.H - 1, (int)(s.Cy + s.Ry + 1));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    if (s.Clip is { } c && (x < c.X || y < c.Y || x >= c.X + c.W || y >= c.Y + c.H)) continue;
                    double p = Pool(s, x, y);
                    if (p <= 0) continue;
                    if (s.Cool) Ground.AddCool(x, y, p); else Ground.AddWarm(x, y, p);
                }
        }
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (Ground.IsEmissive(x, y)) continue;
                var c = Cv.Get(x, y);
                double bay = Fx.Bay(x, y);
                if (Ground.IsExt(x, y))
                {
                    double w = Ground.WarmAt(x, y);
                    int band = (int)Math.Floor(w * 4 + bay * 0.9);
                    if (band >= 1)
                    {
                        double t = Math.Min(1.0, band / 3.0);
                        var lifted = new Rgba((byte)Math.Clamp(c.R + 34 * t, 0, 255), (byte)Math.Clamp(c.G + 26 * t, 0, 255), (byte)Math.Clamp(c.B + 2 * t, 0, 255));
                        c = E.Lerp(lifted, E.Warm, 0.36 * t);
                    }
                    // gentle falloff towards the map edge keeps the eye on the story spaces
                    double edge = Math.Min(Math.Min(x, G.W - 1 - x), Math.Min(y, G.H - 1 - y));
                    if (edge < 14 && ((x + y) & 1) == 0 && w < 0.2) c = c.Shade(0.25 * (14 - edge) / 14);
                    Cv.Set(x, y, c);
                }
                else
                {
                    double v = 0.34 + Ground.WarmAt(x, y) + bay * 0.14;
                    if (Ground.M[x, y] == Mat.Wall) v -= 0.12;
                    int band = (int)Math.Floor(v * 5);
                    c = band switch
                    {
                        <= 0 => E.Lerp(c.Shade(0.60), Rgba.Hex("2E2A4A"), 0.25),
                        1 => E.Lerp(c.Shade(0.42), Rgba.Hex("2E2A4A"), 0.15),
                        2 => c.Shade(0.22),
                        3 => c.Shade(0.04),
                        4 => GlowIn(c, 0.55),
                        _ => GlowIn(c, band >= 6 ? 1.0 : 0.8)
                    };
                    double cl = Ground.CoolAt(x, y);
                    int cb = (int)Math.Floor(cl * 3 + bay * 0.9);
                    if (cb >= 1) c = E.Lerp(c, Rgba.Hex("8FA8E0"), 0.12 * Math.Min(cb, 3));
                    Cv.Set(x, y, c);
                }
            }
    }

    static Rgba GlowIn(Rgba c, double t)
    {
        var lifted = new Rgba((byte)Math.Clamp(c.R + 20 * t, 0, 255), (byte)Math.Clamp(c.G + 14 * t, 0, 255), (byte)Math.Clamp(c.B + 0 * t, 0, 255));
        return E.Lerp(lifted, E.Warm, 0.26 * t);
    }

    public static void CastShadows()
    {
        foreach (var s in Layout.Shadows)
        {
            if (s.Kind == 'e') ShadeEll(s.A, s.B, s.C, s.D, s.K);
            else ShadeRect((int)s.A, (int)s.B, (int)s.C, (int)s.D, s.K);
        }
    }

    static bool Shadable(int x, int y) => x >= 0 && y >= 0 && x < G.W && y < G.H && Ground.M[x, y] != Mat.Wall && !Ground.IsEmissive(x, y);

    static void ShadeEll(double cx, double cy, double rx, double ry, double k)
    {
        for (int y = (int)(cy - ry - 2); y <= cy + ry + 2; y++)
            for (int x = (int)(cx - rx - 2); x <= cx + rx + 2; x++)
            {
                if (!Shadable(x, y)) continue;
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry, d = dx * dx + dy * dy;
                if (d <= 1.0 || (d <= 1.35 && ((x + y) & 1) == 0)) Ground.Cv.Set(x, y, Ground.Cv.Get(x, y).Shade(k));
            }
    }

    static void ShadeRect(int x0, int y0, int w, int h, double k)
    {
        for (int y = y0 - 1; y <= y0 + h; y++)
            for (int x = x0 - 1; x <= x0 + w; x++)
            {
                if (!Shadable(x, y)) continue;
                bool inside = x >= x0 && y >= y0 && x < x0 + w && y < y0 + h;
                if (inside || ((x + y) & 1) == 0) Ground.Cv.Set(x, y, Ground.Cv.Get(x, y).Shade(k));
            }
    }
}
