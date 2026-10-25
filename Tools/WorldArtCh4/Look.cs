namespace WorldArtCh4;

/// <summary>The Chapter 4 "grey pre-dawn" look: colour grade shared by ground and sprites, baked mist, lantern pools.</summary>
public static class Look
{
    /// <summary>Floor rectangles (image coords) where a rug used to lie: the boards underneath are less faded.</summary>
    public static readonly List<Rc> RugMarks = new();
    /// <summary>Warm light pools: centre, radii, strength.</summary>
    public static readonly List<(double x, double y, double rx, double ry, double k)> Pools = new();

    /// <summary>Desaturate, cool the mids, lift the shadows towards blue-violet. Applied to every sprite and the whole ground.</summary>
    public static Rgba Grade(Rgba c)
    {
        if (c.A == 0) return c;
        double r = c.R, g = c.G, b = c.B;
        double l = 0.30 * r + 0.59 * g + 0.11 * b;
        const double keep = 0.46;
        r = l + (r - l) * keep; g = l + (g - l) * keep; b = l + (b - l) * keep;
        r *= 0.88; g *= 0.94; b *= 1.03;
        double s = 1 - l / 255.0;
        r += 7 * s; g += 11 * s; b += 24 * s;
        return new Rgba((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    public static Canvas Grade(Canvas src)
    {
        var o = new Canvas(src.W, src.H);
        for (int y = 0; y < src.H; y++) for (int x = 0; x < src.W; x++) o.Set(x, y, Grade(src.Get(x, y)));
        return o;
    }

    /// <summary>Greens turn khaki and brown: crops that were not watered.</summary>
    public static Rgba Wilt(Rgba c)
    {
        if (c.A == 0) return c;
        if (c.G > c.R * 1.04 && c.G > c.B * 1.12)
            return new Rgba((byte)Math.Clamp(c.R * 1.18 + 14, 0, 255), (byte)Math.Clamp(c.G * 0.86, 0, 255), (byte)Math.Clamp(c.B * 0.72, 0, 255));
        return c;
    }

    static readonly Rgba MistCol = Rgba.Hex("C2CEDC"), MistDeep = Rgba.Hex("A9B8CC");

    public static void Apply()
    {
        var cv = Ground.Cv;
        // rug shadows: where the rugs lay, the boards are lighter and the edges are lined with dust
        foreach (var r in RugMarks)
            for (int y = r.Y; y < r.Y + r.H; y++)
                for (int x = r.X; x < r.X + r.W; x++)
                {
                    var m = Ground.M[x, y];
                    if (m != Mat.Tile && m != Mat.Plank && m != Mat.PlankB) continue;
                    var c = cv.Get(x, y);
                    bool edge = x == r.X || y == r.Y || x == r.X + r.W - 1 || y == r.Y + r.H - 1;
                    if (edge) { if (((x + y) & 1) == 0) cv.Set(x, y, c.Shade(0.35)); }
                    else cv.Set(x, y, new Rgba((byte)(c.R + (255 - c.R) * 0.12), (byte)(c.G + (255 - c.G) * 0.12), (byte)(c.B + (255 - c.B) * 0.10)));
                }

        // grade the whole picture
        for (int y = 0; y < G.H; y++) for (int x = 0; x < G.W; x++) cv.Set(x, y, Grade(cv.Get(x, y)));
        // indoors it is darker than the grey morning outside
        for (int y = G.FloorY - 2; y < G.FloorBot; y++)
            for (int x = 64; x < 416; x++)
            {
                var m = Ground.M[x, y];
                if (m == Mat.Tile || m == Mat.Plank || m == Mat.PlankB || m == Mat.Thresh) cv.Set(x, y, cv.Get(x, y).Shade(0.32));
            }

        // mist: thick on the road and at the far edges, thin over the yard and garden, none indoors
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                bool house = x >= G.HX0 - 4 && x <= G.HX1 + 4 && y <= G.HY1 + 2;
                double s = 0;
                double bank = Fx.Fbm(x / 46.0 + 3.1, y / 14.0, 311);
                // ground fog gathers low: ramps up southwards from the yard
                double south = Math.Clamp((y - 360) / 170.0, 0, 1);
                s = south * (0.35 + 0.9 * bank);
                // the road ends dissolve completely
                double roadD = Ground.RoadDist(x + .5, y + .5);
                double ends = Math.Max(Math.Clamp((140 - x) / 110.0, 0, 1), Math.Clamp((x - 550) / 110.0, 0, 1));
                if (y > 400) s = Math.Max(s, ends * Math.Clamp((y - 400) / 60.0, 0, 1) * (1.5 - Math.Clamp(roadD / 90.0, 0, 0.5)));
                // yard and garden: faint wisps
                if (y > 300 && y <= 470) s = Math.Max(s, 0.22 * Fx.Fbm(x / 38.0, y / 16.0, 313));
                if (x > 455 && y < 320) s = Math.Max(s, 0.16 * Fx.Fbm(x / 30.0, y / 18.0, 317));
                if (house) s *= 0.0;
                // soft shoulder of the house roof edge
                if (s <= 0.04) continue;
                // quantise to 4 levels with ordered dithering so it stays pixel art
                double q = s * 3.2 + Fx.Bay(x, y) * 0.9;
                int lv = Math.Clamp((int)Math.Floor(q), 0, 3);
                if (lv == 0) continue;
                double a = lv == 1 ? 0.22 : lv == 2 ? 0.42 : 0.62;
                var tone = ((x / 3 + y / 2) & 3) == 0 ? MistDeep : MistCol;
                var c = cv.Get(x, y);
                cv.Set(x, y, new Rgba((byte)(c.R + (tone.R - c.R) * a), (byte)(c.G + (tone.G - c.G) * a), (byte)(c.B + (tone.B - c.B) * a)));
            }

        // lantern pools (warm, dithered) – added last so the mist cannot wash them out
        foreach (var (px, py, rx, ry, k) in Pools)
            for (int y = (int)(py - ry - 1); y <= py + ry + 1; y++)
                for (int x = (int)(px - rx - 1); x <= px + rx + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= G.W || y >= G.H) continue;
                    double dx = (x + .5 - px) / rx, dy = (y + .5 - py) / ry, d2 = dx * dx + dy * dy;
                    if (d2 >= 1) continue;
                    double s = (1 - d2) * k;
                    int lv = Math.Clamp((int)Math.Floor(s * 4 + Fx.Bay(x, y) * 0.9), 0, 3);
                    if (lv == 0) continue;
                    double a = lv == 1 ? 0.14 : lv == 2 ? 0.26 : 0.38;
                    var c = cv.Get(x, y);
                    cv.Set(x, y, new Rgba((byte)(c.R + (255 - c.R) * a), (byte)(c.G + (206 - c.G) * a), (byte)(c.B + (130 - c.B) * a)));
                }

        // gentle vignette towards the corners
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double e = Math.Min(Math.Min(x, G.W - 1 - x) / 40.0, Math.Min(y, G.H - 1 - y) / 40.0);
                if (e >= 1 || (y > 440 && Ground.RoadDist(x + .5, y + .5) < 34)) continue;
                if (Fx.Bay(x, y) * 0.5 + 0.5 > e * 1.1 + 0.1) { var c = cv.Get(x, y); cv.Set(x, y, c.Shade(0.35)); }
            }
    }
}
