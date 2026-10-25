namespace WorldArtCh3;

/// <summary>Overlays cut from the finished ground: the wall of the middle house gives way after the first blast.</summary>
public static class Overlays
{
    static readonly (double x, double y)[] Hole =
    {
        (338, 28), (360, 28), (364, 37), (378, 34), (384, 45), (397, 49), (392, 62), (399, 77), (390, 90), (395, 104), (381, 113),
        (371, 108), (362, 119), (348, 112), (338, 117), (334, 102), (341, 92), (330, 82), (336, 70), (327, 60), (334, 48), (330, 37),
    };

    public static void Build()
    {
        var g = Ground.Cv;
        int x0 = 322, y0 = 24, w = 82, h = 100;
        var mask = new Canvas(g.W, g.H);
        mask.Poly(Hole, Rgba.Hex("FFFFFF"));
        var o = new Canvas(w, h);
        double Dist(int x, int y)
        {
            // distance to the nearest masked pixel (rim thickness)
            for (int d = 1; d <= 3; d++)
                for (int j = -d; j <= d; j++) for (int i = -d; i <= d; i++)
                    if (Math.Max(Math.Abs(i), Math.Abs(j)) == d && mask.Opaque(x + i, y + j)) return d;
            return 99;
        }
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
            {
                Rgba c;
                if (mask.Opaque(x, y))
                {
                    if (y < 54)
                    {
                        c = Dusk.Mix(Ground.SkyPx(x, y), Rgba.Hex("3A2A48"), 0.5 - Math.Clamp((y - 28) / 40.0, 0, 0.3));   // roof gone: smoky, fire-lit sky behind
                        // charred rafters
                        if ((x + y / 3) % 11 == 0 && y > 30) c = Rgba.Hex("2C2230");
                        if (y == 40 + (x % 5 == 0 ? 1 : 0) && x % 3 != 0) c = Rgba.Hex("2C2230");
                    }
                    else
                    {
                        bool upper = y < 89;
                        double gl = Math.Exp(-(Math.Pow((x - 364) / 24.0, 2) + Math.Pow((y - 116) / 22.0, 2)));
                        double gl2 = Math.Exp(-(Math.Pow((x - 388) / 10.0, 2) + Math.Pow((y - 60) / 10.0, 2))) * 0.6;   // glow through the roof
                        c = upper ? Rgba.Hex("3C2C46") : Rgba.Hex("261C2A");
                        if ((x / 3) % 2 == 0) c = Dusk.Mix(c, Rgba.Hex("54425E"), 0.45);            // wallpaper stripes
                        if (upper && y > 78) c = Dusk.Mix(c, Rgba.Hex("24181E"), 0.55);              // wainscot
                        // furniture left standing: wardrobe and a hanging frame
                        if (upper && x >= 372 && x < 386 && y > 60 && y < 88) c = (x == 372 || y == 61) ? Rgba.Hex("7A5238") : Rgba.Hex("4A3226");
                        if (upper && x >= 348 && x < 356 && y >= 62 && y < 70) c = (x == 348 || x == 355 || y == 62 || y == 69) ? Rgba.Hex("C8A050") : Rgba.Hex("6A5A78");
                        // ceiling beam stump and the broken floor slab
                        if (y >= 54 && y < 57) c = Rgba.Hex("2A2028");
                        if (y >= 88 && y < 92) c = y == 88 ? Rgba.Hex("7A5A44") : Rgba.Hex("4A3428");
                        if (!upper && y >= 92 && x % 14 == 3) c = Rgba.Hex("1A1420");                // joists
                        c = Dusk.Mix(c, Rgba.Hex("E2824A"), Math.Clamp(gl * 0.85 + gl2 + Fx.Bay(x, y) * 0.1, 0, 0.8));
                        if ((x + y) % 5 == 0 && Fx.R(x, y, 3) > 0.6) c = Dusk.Mix(c, Rgba.Hex("000000"), 0.3);   // soot / texture
                    }
                }
                else
                {
                    double d = Dist(x, y);
                    if (d > 2) continue;
                    // torn edge: exposed brick, plaster crumbs and soot
                    if (Fx.R(x, y, 5) < 0.28 * d) continue;
                    var baseC = g.Get(x, y);
                    c = d == 1 ? Dusk.Street(Rgba.Mul(K.Roof[1 + (int)(Fx.H(x / 3, y / 2, 3) % 2)], 0.95, 0.92, 0.95)) : Dusk.Mix(baseC, Rgba.Hex("D8D0D8"), 0.55);
                    if (y < 54) c = Dusk.Street(Rgba.Mul(K.Roof[0], 0.9, 0.9, 0.9));
                    if (d == 1 && Fx.R(x, y, 6) > 0.7) c = Dusk.Street(K.Mortar);
                }
                o.Set(x - x0, y - y0, c);
            }
        // plaster dust smeared down the wall under the hole
        for (int y = 100; y < y0 + h; y++)
            for (int x = 330; x < 400; x++)
            {
                if (o.Opaque(x - x0, y - y0)) continue;
                double f = (Fx.Fbm(x / 6.0, y / 5.0, 12) - 0.35) * 2 * (1 - (y - 100) / 24.0);
                if (f > 0.35 && ((x + y) & 1) == 0) o.Set(x - x0, y - y0, Dusk.Mix(g.Get(x, y), Rgba.Hex("E6DED8"), 0.6));
            }
        // trim to the occupied area, keep an even width
        var b = o.Bounds() ?? throw new Exception("empty ruin overlay");
        int bx = b.x - (b.w % 2), bw = b.w + (b.w % 2);
        var cut = o.Crop(bx, b.y, bw, b.h);
        S.Add("fx_c3_ruin_front", cut);
        Plan.Put("fx_c3_ruin_front", x0 + bx + bw / 2, y0 + b.y + b.h, "floor", "ch3_took_cover");
    }
}
