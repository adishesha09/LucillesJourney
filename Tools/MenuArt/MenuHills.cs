using static MenuArt.Kit;

namespace MenuArt;

public static class MenuHills
{
    /// <summary>Lit windows of the far village, in hills_far pixel coordinates (filled while drawing).</summary>
    public static readonly List<(int x, int y)> FarWindows = new();

    const int SX = MenuSky.SunX + 32; // sun x inside the 448 px hill layers (layer rests at screen x = -32)

    static void Rim(Canvas c, int x, int y, Rgba col) => c.Set(x, y, col);

    static void PaintRange(Canvas c, Func<double, double> ridge, Rgba[] ramp, double span, Rgba warm, double warmAmt, Rgba rim, double rimAmt, int seed)
    {
        for (int x = 0; x < c.W; x++)
        {
            int yr = (int)Math.Round(ridge(x));
            double slope = ridge(x + 2) - ridge(x - 2); // negative: rises to the right => left slope faces the sun on the right side
            bool faceSun = x > SX ? slope < -0.3 : slope > 0.3;
            double ls = faceSun ? Clamp01(Math.Abs(slope) / 3.0) : 0;
            double glowFall = Gauss(x - SX, 150);
            for (int y = Math.Max(0, yr); y < c.H; y++)
            {
                double depth = y - yr;
                var col = DRamp(ramp, depth / span, x, y, 1.5);
                double wp = 0;
                if (faceSun) wp = Clamp01(1 - depth / (4 + 9 * ls)) * (0.35 + 0.65 * ls) * glowFall * warmAmt;
                wp += Clamp01(1 - depth / 7.0) * 0.45 * Gauss(x - SX, 90) * warmAmt;
                if (wp > 0 && wp > Bay8(x, y)) col = warm;
                if (depth < 1 && Gauss(x - SX, 130) * rimAmt * (0.6 + 0.4 * ls) > Fx.R(x, y, seed)) col = rim;
                c.Set(x, y, col);
            }
        }
    }

    static void TinyTrees(Canvas c, Func<double, double> ridge, int x0, int x1, Rgba dark, Rgba lit, int seed, int hMin, int hMax)
    {
        var rnd = new Random(seed);
        for (int x = x0; x < x1; x += 2 + rnd.Next(3))
        {
            int yr = (int)Math.Round(ridge(x)); int h = hMin + rnd.Next(hMax - hMin + 1);
            int w = 1 + h / 2;
            for (int j = 0; j < h; j++)
            {
                int half = (int)Math.Round(w * (0.5 + 0.5 * Math.Sin(Math.PI * Math.Min(1, (j + 1.2) / (h + 0.6)))) / 2.0);
                for (int i = -half; i <= half; i++)
                    c.Set(x + i, yr - h + j + 1, dark);
            }
            if (x > SX) c.Set(x - 1, yr - h + 1, lit); else c.Set(x + 1, yr - h + 1, lit);
        }
    }

    public static Canvas Far()
    {
        var c = new Canvas(448, 216);
        FarWindows.Clear();
        double R1(double x)
        {
            double n = Fbm1(x, 11, 64);
            double r = 140 - 42 * Math.Pow(n, 1.6) * (0.75 + 0.5 * Smooth(160, 430, x));
            return Lerp(r, 131.0, Gauss(x - SX, 30) * 0.92);
        }
        double R2(double x)
        {
            double n = Fbm1(x, 17, 55);
            double r = 150 - 28 * Math.Pow(n, 1.5);
            return Lerp(r, 138.5, Gauss(x - SX, 28) * 0.9);
        }
        double R3(double x)
        {
            double n = Fbm1(x, 23, 48);
            double r = 155 - 18 * n;
            r = Lerp(r, 141.0, Gauss(x - 118, 24) * 0.85);   // village shoulder
            return Lerp(r, 146, Gauss(x - SX, 30) * 0.7);
        }
        PaintRange(c, R1, Ramp("8A74A4", "9C80A4", "B48CA0", "D09C9C"), 46, C("EBA67E"), 0.85, C("F6C48C"), 0.9, 1);
        TinyTrees(c, R1, 300, 440, C("6C5C90"), C("B88AA0"), 5, 2, 3);
        PaintRange(c, R2, Ramp("62568A", "74608C", "8E6E90", "B08090"), 38, C("DE8E78"), 0.7, C("EDAA84"), 0.7, 2);
        TinyTrees(c, R2, 24, 120, C("4E467A"), C("9A7490"), 6, 2, 4);
        TinyTrees(c, R2, 230, 330, C("4E467A"), C("9A7490"), 7, 2, 4);
        PaintRange(c, R3, Ramp("463E70", "564A7C", "6E5686", "8C6488"), 34, C("BC7688"), 0.55, C("DC9A80"), 0.6, 3);
        TinyTrees(c, R3, 6, 86, C("3A3462"), C("8A6480"), 8, 2, 4);
        TinyTrees(c, R3, 150, 205, C("3A3462"), C("8A6480"), 9, 2, 4);
        TinyTrees(c, R3, 250, 440, C("3A3462"), C("8A6480"), 10, 2, 5);
        Village(c, R3);
        return c;
    }

    static void Village(Canvas c, Func<double, double> ridge)
    {
        var wall = C("8E7898"); var wallLit = C("B08A9C"); var roof = C("3C3460"); var roof2 = C("4A3E6C");
        var gold = C("F2D26B"); var amber = C("D98F4B");
        // x, width, wall height, roof rows
        var houses = new (int x, int w, int h, int r)[] { (94, 5, 3, 1), (100, 6, 4, 2), (108, 4, 3, 1), (118, 6, 4, 2), (125, 5, 3, 1), (131, 5, 4, 2), (137, 4, 3, 1) };
        int ground = 141;
        foreach (var hs in houses)
        {
            for (int i = 0; i < hs.w; i++)
                for (int j = 0; j < hs.h; j++) c.Set(hs.x + i, ground - 1 - j, i == 0 ? wallLit : wall);
            for (int j = 0; j < hs.r; j++)
                for (int i = -1 + j; i < hs.w + 1 - j; i++) c.Set(hs.x + i, ground - hs.h - 1 - j, j == 0 ? roof : roof2);
        }
        // small bell tower
        for (int j = 0; j < 8; j++) { c.Set(113, ground - 1 - j, wall); c.Set(114, ground - 1 - j, j < 6 ? wall : roof); }
        c.Set(113, ground - 9, roof); c.Set(114, ground - 9, roof); c.Set(113, ground - 10, roof);
        c.Set(113, ground - 1 - 7, wallLit);
        // fill below the village so it sits on the ridge
        for (int x = 92; x < 142; x++) for (int y = ground; y < ground + 3; y++) c.Set(x, y, C("54487A"));
        void Win(int x, int y, Rgba col) { c.Set(x, y, col); FarWindows.Add((x, y)); }
        Win(102, ground - 2, gold); Win(120, ground - 3, gold); Win(121, ground - 2, amber); Win(132, ground - 2, gold);
        Win(95, ground - 2, amber);
    }

    // ---------------------------------------------------------------------------------------------------

    public static Canvas Mid()
    {
        var c = new Canvas(448, 216);
        double Ridge(double x)
        {
            double n = Fbm1(x, 31, 80);
            double r = 156 - 24 * n;
            return Lerp(r, 148.0, Gauss(x - SX, 24) * 0.5);
        }
        var sets = new (Rgba dark, Rgba light)[]
        {
            (C("46543A"), C("586A40")), (C("2F4A3C"), C("3B5E45")), (C("2B4545"), C("356058")), (C("503F42"), C("66524E")), (C("5E5C3C"), C("746E44")),
        };
        var rimC = C("C8A862"); var rimDim = C("8E8A58"); var edgeDark = C("1F3331");
        for (int x = 0; x < c.W; x++)
        {
            double yrD = Ridge(x); int yr = (int)Math.Round(yrD);
            double slope = Ridge(x + 2) - Ridge(x - 2);
            bool faceSun = x > SX ? slope < -0.3 : slope > 0.3;
            for (int y = yr; y < c.H; y++)
            {
                double depth = y - yrD;
                double u = (x - 224) / (depth * 0.5 + 11.0);
                int row = (int)Math.Floor(Math.Log2(depth / 5.0 + 1) * 1.5);
                int cell = (int)Math.Floor(u * 1.05);
                uint hsh = Fx.H(cell, row, 77);
                var set = sets[hsh % (uint)sets.Length];
                // furrows follow the contour of the hill; spacing tightens towards the horizon
                double phase = (y - 0.78 * yrD) / (2.4 + depth * 0.05 + (hsh >> 8) % 3 * 0.4) * Math.PI * 2;
                double t = 0.5 + 0.5 * Math.Sin(phase);
                var col = DRamp(new[] { set.dark, set.light }, t, x, y, 1.3);
                // depth: lower down = darker, closer
                double dk = Smooth(14, 54, depth) * 0.55;
                if (dk > Bay8(x + 3, y)) col = Mix(col, C("1F302E"), 0.6);
                // field boundaries / hedgerows
                double uf = u * 1.05; double eu = Math.Abs(uf - Math.Round(uf)) * (depth * 0.5 + 11.0) / 1.05;
                double rowF = Math.Log2(depth / 5.0 + 1) * 1.5; double er = Math.Abs(rowF - Math.Round(rowF));
                if ((eu < 0.7 || er < 0.045) && depth > 1.5 && Fx.R(x, y, 5) < 0.85) col = edgeDark;
                if (depth < 1 && faceSun) col = Gauss(x - SX, 150) > Bay8(x, y) * 0.9 ? rimC : rimDim;
                else if (depth < 1) col = rimDim;
                else if (depth < 3 && faceSun && Gauss(x - SX, 140) * 0.8 > Bay8(x, y)) col = Mix(col, rimC, 0.45);
                c.Set(x, y, col);
            }
        }
        // winding dirt path climbing over the hill toward the horizon
        var rimPath = C("D4A478"); var lit = C("B98C70"); var body = C("93706A"); var shade = C("6E5660");
        double px0 = 214, py0 = 166, py1 = 138;
        for (int y = (int)py0; y >= (int)py1; y--)
        {
            double t = (py0 - y) / (py0 - py1);
            double cx = px0 - 14 * t + 11 * Math.Sin(t * Math.PI * 1.7 + 0.4) * (0.4 + 0.6 * t);
            double hw = 1.0 + 4.6 * (1 - t) * (1 - t) + 0.6;
            for (int x = (int)(cx - hw - 1); x <= (int)(cx + hw + 1); x++)
            {
                double d = Math.Abs(x + .5 - cx);
                if (d > hw + 0.4 * (Fx.R(x, y, 9) - 0.5)) continue;
                double u = (x + .5 - cx) / hw; // -1 left .. 1 right
                var col = u < -0.55 ? lit : u > 0.45 ? shade : body;
                if (((x + y) & 3) == 0 && u > -0.5 && u < 0.4) col = lit;
                if (y < py1 + 3) col = Mix(col, rimC, 0.25);
                c.Set(x, y, col);
            }
        }
        // a few tree silhouettes along the ridge and on the slope
        void Tree(int tx, int ty, int r)
        {
            var d0 = C("1B2E2D"); var d1 = C("26403A"); var l0 = C("A89A58"); var l1 = C("7E8450");
            c.Rect(tx, ty - 1, 1, 3, C("2A2224"));
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    double dd = (x * x) / (double)(r * r) + (y * y * 1.1) / (double)(r * r);
                    if (dd > 1) continue;
                    var col = (x * 0.8 + y * 0.6) > r * 0.25 ? d0 : d1;
                    c.Set(tx + x, ty - r - 1 + y + r, col);
                }
            // rim on the left/top (sun-facing)
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= 0; x++)
                {
                    double dd = (x * x) / (double)(r * r) + (y * y * 1.1) / (double)(r * r);
                    if (dd > 1 || dd < 0.45) continue;
                    if (!(x < -r * 0.35 || y < -r * 0.5)) continue;
                    if (x * 0.8 + y * 0.6 > -r * 0.2) continue;
                    c.Set(tx + x, ty - r - 1 + y + r, (x + y) % 2 == 0 ? l0 : l1);
                }
        }
        foreach (var (tx, r) in new[] { (22, 4), (31, 3), (60, 5), (94, 3), (150, 4), (158, 3), (268, 4), (296, 5), (305, 3), (344, 4), (391, 5), (430, 4) })
            Tree(tx, (int)Math.Round(Ridge(tx)) + 3 + (tx % 3), r);
        return c;
    }
}
