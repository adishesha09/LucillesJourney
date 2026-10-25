using static MenuArt.Kit;

namespace MenuArt;

public static class EndingLand
{
    /// <summary>Town windows (lit) in ending_hills_far pixel coordinates.</summary>
    public static readonly List<(int x, int y)> TownWindows = new();

    public const int VX = 224, VY = 129, SunLayerX = EndingSky.SunStartX + 32;

    static double RoadT(double y) => Clamp01((y - VY) / (216.0 - VY));
    public static double RoadX(double y) { double t = RoadT(y); return VX + 20 * Math.Sin(t * Math.PI * 0.95) * Math.Pow(1 - t, 0.8); }
    public static double RoadW(double y) { double t = RoadT(y); return 5 + 150 * Math.Pow(t, 1.35); }

    // ------------------------------------------------------------------------------------------ far hills + town
    static void Range(Canvas c, Func<double, double> ridge, Rgba[] ramp, double span, Rgba warm, double warmAmt, int seed)
    {
        for (int x = 0; x < c.W; x++)
        {
            double yrD = ridge(x); int yr = (int)Math.Round(yrD);
            double slope = ridge(x + 2) - ridge(x - 2);
            bool faceSun = x > SunLayerX ? slope < -0.25 : slope > 0.25;
            double ls = faceSun ? Clamp01(Math.Abs(slope) / 2.5) : 0;
            for (int y = Math.Max(0, yr); y < c.H; y++)
            {
                double depth = y - yrD;
                var col = DRamp(ramp, depth / span, x, y, 1.5);
                double wp = Clamp01(1 - depth / 5.0) * 0.5 * Gauss(x - SunLayerX, 120) * warmAmt;
                if (faceSun) wp += Clamp01(1 - depth / (3 + 6 * ls)) * ls * 0.8 * Gauss(x - SunLayerX, 150) * warmAmt;
                if (wp > Bay8(x, y)) col = warm;
                c.Set(x, y, col);
            }
        }
    }

    public static Canvas Far()
    {
        var c = new Canvas(448, 216);
        TownWindows.Clear();
        double R1(double x) => 128 - 32 * Math.Pow(Fbm1(x, 41, 70), 1.5) * Smooth(60, 220, Math.Abs(x - VX));
        double R2(double x)
        {
            double r = 133 - 20 * Math.Pow(Fbm1(x, 43, 55), 1.3) * Smooth(40, 170, Math.Abs(x - VX));
            return Lerp(r, 127.5, Gauss(x - VX, 52) * 0.95);
        }
        double R3(double x) => 138 - 9 * Fbm1(x, 47, 40) * Smooth(10, 120, Math.Abs(x - VX)) - 0 * x;
        Range(c, R1, Ramp("A5B4C4", "B3BEC8", "C3C8CC", "D2CFCB"), 40, C("EFD0A6"), 0.8, 1);
        Range(c, R2, Ramp("8EA2B2", "9DB0B8", "AEBDBA", "C0C8BE"), 34, C("EBCBA0"), 0.35, 2);
        Town(c);
        Range(c, R3, Ramp("7C9890", "89A596", "98B09A", "A8BAA2"), 30, C("E8D0A0"), 0.6, 3);
        // tiny tree rows on the near ridge
        var rnd = new Random(31);
        for (int x = 4; x < 448; x += 2 + rnd.Next(3))
        {
            if (Math.Abs(x - VX) < 22) continue;
            int yr = (int)Math.Round(R3(x)); int h = 2 + rnd.Next(3);
            for (int j = 0; j < h; j++) { int half = j == 0 ? 0 : 1; for (int i = -half; i <= half; i++) c.Set(x + i, yr - h + j + 1, C("5E7E74")); }
            c.Set(x + (x > SunLayerX ? -1 : 1), yr - h + 1, C("C8D0A0"));
        }
        return c;
    }

    static void Town(Canvas c)
    {
        var rnd = new Random(77);
        var haze = C("D2C6BC");
        var walls = new[] { C("BBA9AA"), C("C9B8AA"), C("AEA6B0"), C("D2C0B0"), C("B6AAB8") };
        var roofs = new[] { C("9A7478"), C("86808E"), C("A27C7A"), C("8A7A88") };
        var warm = C("F4D6A6"); var gold = C("F2D26B"); var amber = C("E8A858"); var dark = C("6E6478");
        var chimneys = new List<(int x, int y)>();
        var cands = new List<(int x, int y)>();
        // low town wall with a gate
        for (int x = 178; x < 272; x++) for (int y = 123; y < 128; y++) c.Set(x, y, y == 123 ? C("D4C4B4") : x % 11 < 1 ? C("A89AA0") : C("B8A9A8"));
        // two rows of buildings, back row taller and hazier
        for (int row = 0; row < 2; row++)
        {
            int x = 178 + rnd.Next(3);
            int baseY = row == 0 ? 123 : 128;
            while (x < 268)
            {
                int w = 6 + rnd.Next(5), hw = (row == 0 ? 5 : 3) + rnd.Next(row == 0 ? 4 : 3);
                bool gable = rnd.NextDouble() < 0.75;
                if (row == 1 && x + w > 217 && x < 232) { x += 3; continue; } // keep the gate clear
                var wall = Mix(walls[rnd.Next(walls.Length)], haze, row == 0 ? 0.45 : 0.22);
                var roof = Mix(roofs[rnd.Next(roofs.Length)], haze, row == 0 ? 0.4 : 0.15);
                for (int j = 0; j < hw; j++)
                    for (int i = 0; i < w; i++)
                    {
                        var col = i == 0 ? warm : (j < 2 && i > 0 && Bay8(x + i, j) < 0.25) ? Mix(wall, warm, 0.5) : wall;
                        if (i >= w - 2) col = Mix(col, C("8E8290"), 0.25);
                        c.Set(x + i, baseY - 1 - j, col);
                    }
                int rh = gable ? (w + 2) / 3 + 1 : 1;
                for (int j = 0; j < rh; j++)
                    for (int i = -1 + (gable ? j : 0); i < w + 1 - (gable ? j : 0); i++)
                        c.Set(x + i, baseY - hw - 1 - j, j == 0 && i < 0 ? warm : (i <= -1 + j + 0 && j > 0) ? warm : (i > w / 2 && gable ? Mix(roof, C("6E6478"), 0.25) : roof));
                if (gable) c.Set(x + (w - 1) / 2, baseY - hw - rh, Mix(roof, warm, 0.5));
                // windows
                for (int k = 0; k < 2; k++)
                {
                    int wxp = x + 1 + k * 3 + rnd.Next(2), wyp = baseY - hw + 2 + rnd.Next(Math.Max(1, hw - 4));
                    if (wxp >= x + w - 1) continue;
                    c.Set(wxp, wyp, dark); c.Set(wxp, wyp + 1, dark); cands.Add((wxp, wyp));
                }
                if (rnd.NextDouble() < 0.28 && gable) chimneys.Add((x + w - 2, baseY - hw - rh + 1));
                x += w + (rnd.NextDouble() < 0.3 ? 1 : 0);
            }
        }
        // bell tower, clock-face neutral
        const int tx = 246, ty = 106;
        for (int y = ty; y < 124; y++) for (int i = 0; i < 8; i++) c.Set(tx + i, y, i == 0 ? warm : i > 5 ? C("A39AA6") : C("C6B8B0"));
        for (int i = 0; i < 8; i++) c.Set(tx + i, ty, C("A8989A"));
        foreach (int ax in new[] { 1, 4 }) { c.Set(tx + ax + 1, ty + 3, dark); c.Set(tx + ax + 1, ty + 4, dark); c.Set(tx + ax + 2, ty + 3, dark); c.Set(tx + ax + 2, ty + 4, dark); }
        for (int y = 0; y < 5; y++) for (int i = 0; i < 5; i++) { double d = Math.Abs(i - 2) + Math.Abs(y - 2); if (d <= 3 && !(i % 4 == 0 && y % 4 == 0)) c.Set(tx + 2 + i - 1, ty + 8 + y, C("F8F0DC")); }
        c.Set(tx + 3, ty + 9, C("5A4A50")); c.Set(tx + 3, ty + 10, C("5A4A50")); c.Set(tx + 4, ty + 10, C("5A4A50"));
        c.Rect(tx + 2, ty + 14, 1, 3, dark); c.Rect(tx + 5, ty + 14, 1, 3, dark);
        for (int j = 0; j < 6; j++) for (int i = j; i < 9 - j; i++) { int px = tx - 1 + i; c.Set(px, ty - 1 - j, i == j || (j == 0 && i == 0) ? warm : (px > tx + 3 ? C("7C7A90") : C("8A8AA0"))); }
        c.Set(tx + 3, ty - 7, C("5A5668")); c.Set(tx + 3, ty - 8, C("5A5668")); c.Set(tx + 3, ty - 9, warm);
        // gate: dark arch with a warm lantern light inside
        for (int y = 121; y < 128; y++) for (int x = 221; x < 228; x++) { if (y == 121 && (x == 221 || x == 227)) continue; c.Set(x, y, C("5E566A")); }
        for (int y = 123; y < 128; y++) { c.Set(224, y, C("F2D26B")); if (y >= 125) { c.Set(223, y, C("E8A858")); c.Set(225, y, C("E8A858")); } }
        // lit windows spread over the town (picked from the real wall windows), and lanterns beside the gate
        int lit = 0, lastX = -99;
        foreach (var (wx, wy) in cands.OrderBy(p => p.x))
        {
            if (lit >= 7 || wx - lastX < 9 || (wx > 214 && wx < 232)) continue;
            var col = lit % 3 == 1 ? amber : gold;
            c.Set(wx, wy, col); c.Set(wx, wy + 1, col == gold ? amber : col); TownWindows.Add((wx, wy)); lit++; lastX = wx;
        }
        c.Set(253 - 2, 111, gold); c.Set(253 - 2, 112, amber); TownWindows.Add((251, 111)); // tower belfry lamp
        c.Set(220, 122, gold); c.Set(228, 122, gold); TownWindows.Add((220, 122)); TownWindows.Add((228, 122));
        // thin chimney smoke drifting right
        foreach (var (cx, cy) in chimneys.Take(4))
        {
            c.Set(cx, cy, C("8E6A70")); c.Set(cx, cy - 1, C("8E6A70"));
            for (int i = 0; i < 16; i++)
            {
                double t = i / 15.0;
                int sx = cx + (int)Math.Round(5 * t * t + Math.Sin(t * 5 + cx) * 0.8), sy = cy - 2 - i;
                if (i % 3 == 2 && t > 0.5) continue;
                c.Set(sx, sy, t < 0.5 ? C("E2D6D0") : C("ECE0D2"));
            }
        }
    }

    // ------------------------------------------------------------------------------------------ mid hills: fields, hedges, tree rows
    public static Canvas Mid()
    {
        var c = new Canvas(448, 216);
        double Ridge(double x) => 139 - 30 * Fbm1(x, 51, 80) * (0.15 + 0.85 * Smooth(30, 190, Math.Abs(x - VX)));
        var sets = new (Rgba dark, Rgba light)[]
        {
            (C("8CA088"), C("9CB08E")), (C("B0AC80"), C("C2BE90")), (C("9C8E84"), C("AE9E90")), (C("7E9E96"), C("90B0A6")), (C("A0A888"), C("B2BA98")), (C("B8AE86"), C("C8BE96")), (C("98929E"), C("A8A2AE")),
        };
        var hedge = C("5E7A6C"); var haze = C("C6C0B6"); var warm = C("F2D8A8");
        for (int x = 0; x < c.W; x++)
        {
            double yrD = Ridge(x); int yr = (int)Math.Round(yrD);
            double slope = Ridge(x + 2) - Ridge(x - 2);
            bool faceSun = x > SunLayerX ? slope < -0.3 : slope > 0.3;
            for (int y = yr; y < c.H; y++)
            {
                double depth = y - yrD;
                double u = (x - VX) / (depth * 0.55 + 12.0);
                double rowF = Math.Log2(depth / 5.0 + 1) * 1.4;
                int row = (int)Math.Floor(rowF);
                int cell = (int)Math.Floor(u * 1.0);
                uint hsh = Fx.H(cell, row, 177);
                var set = sets[hsh % (uint)sets.Length];
                double phase = (y - 0.8 * yrD) / (2.4 + depth * 0.05 + (hsh >> 8) % 3 * 0.4) * Math.PI * 2;
                var col = DRamp(new[] { set.dark, set.light }, 0.5 + 0.5 * Math.Sin(phase), x, y, 1.3);
                double dk = Smooth(8, 50, depth) * 0.6;
                if (dk > Bay8(x + 3, y)) col = Mix(col, C("5E7A70"), 0.55);
                double eu = Math.Abs(u - Math.Round(u)) * (depth * 0.55 + 12.0); double er = Math.Abs(rowF - Math.Round(rowF));
                if ((eu < 0.7 || er < 0.04) && depth > 1.5 && Fx.R(x, y, 5) < 0.85) col = hedge;
                double hz = Clamp01(1 - depth / 9.0) * 0.8;
                if (hz > Bay8(x, y)) col = haze;
                if (depth < 1.0) col = faceSun && Gauss(x - SunLayerX, 150) > Bay8(x, y) * 0.9 ? warm : C("B8BCB0");
                else if (depth < 2.6 && faceSun && Gauss(x - SunLayerX, 140) * 0.7 > Bay8(x + 1, y)) col = Mix(col, warm, 0.5);
                c.Set(x, y, col);
            }
        }
        // tree rows along the hedges and ridge (dark sage balls, rim light towards the rising sun)
        var rnd = new Random(88);
        void Tree(int tx, int ty, int r)
        {
            var d0 = C("3C5A56"); var d1 = C("4C6C64"); var l0 = C("E8D8A8"); var l1 = C("B8C090");
            int side = tx > SunLayerX ? -1 : 1;
            c.Rect(tx, ty, 1, 2, C("4A4A48"));
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    double dd = x * x / (double)(r * r) + y * y * 1.15 / (double)(r * r);
                    if (dd > 1) continue;
                    bool lit = (x * side * -1 + y * 0.6) < -r * 0.45 && dd > 0.35;
                    c.Set(tx + x, ty - r + y, lit ? ((x + y) % 2 == 0 ? l0 : l1) : (x * side > r * 0.15 ? d0 : d1));
                }
        }
        for (int i = 0; i < 46; i++)
        {
            int tx = 6 + rnd.Next(436); if (Math.Abs(tx - VX) < 26) continue;
            int ty = (int)Math.Round(Ridge(tx)) + 2 + rnd.Next(9);
            Tree(tx, ty, 2 + rnd.Next(3));
        }
        return c;
    }

    // ------------------------------------------------------------------------------------------ road (foreground)
    static double Margin(double t) => 5 + 40 * Math.Pow(t, 1.15);
    static double HedgeW(double t) => 3 + 12 * t;
    static double HedgeH(double t) => 3 + 15 * Math.Pow(t, 1.2);
    static double VergeHalf(double y) { double t = RoadT(y); return RoadW(y) / 2 + Margin(t); }

    static Canvas road = new(448, 216);

    public static Canvas Road()
    {
        road = new Canvas(448, 216);
        Verge();
        Hedges();
        RoadSurface();
        Posts();
        Flowers();
        FrameTrees();
        return road;
    }

    // grass strip along both sides of the road
    static void Verge()
    {
        var ramp = Ramp("A2B49A", "92A98E", "7F9A84", "6C8A76", "5A7A6A", "476858", "3C5A50");
        var rnd = new Random(61);
        for (int y = VY; y < 216; y++)
        {
            double xc = RoadX(y), hv = VergeHalf(y);
            double depth = Clamp01((y - 130) / 86.0);
            for (int x = (int)(xc - hv); x <= (int)(xc + hv); x++)
            {
                double tt = Math.Sqrt(depth) * 0.95 + (N(x / 30.0, y / 8.0, 67) - 0.5) * 0.28;
                road.Set(x, y, DRamp(ramp, tt, x, y, 1.5));
            }
        }
        for (int i = 0; i < 1100; i++)
        {
            int y = VY + 3 + (int)(Math.Pow(rnd.NextDouble(), 0.8) * 83);
            double xc = RoadX(y), hv = VergeHalf(y);
            int x = (int)(xc + (rnd.NextDouble() * 2 - 1) * hv);
            double depth = (y - 130) / 86.0;
            int h = 1 + (int)(depth * 5 * rnd.NextDouble());
            var body = DRamp(Ramp("6C8A76", "5A7A6A", "476858", "3C5A50"), depth + 0.15, x, y, 1.0);
            for (int j = 0; j < h; j++) road.Set(x, y - j, body);
            if (h >= 2 && rnd.NextDouble() < 0.6) road.Set(x + 1, y - h + 1, rnd.NextDouble() < 0.5 ? C("D8D49A") : C("B4C490"));
            if (h >= 3 && rnd.NextDouble() < 0.4) road.Set(x - 1, y - h + 2, body);
        }
    }

    // hedgerows run along both sides of the lane, converging on the town gate; the mid-hill fields show beyond them
    static void Hedges()
    {
        var ramp = Ramp("A8C090", "86A884", "6A8E78", "52786A", "3E5E58", "30494A");
        for (int y = VY; y < 216; y++)
        {
            double xc = RoadX(y), t = RoadT(y), hv = VergeHalf(y), bw = HedgeW(t), hh = HedgeH(t);
            for (int side = -1; side <= 1; side += 2)
            {
                int xa = (int)Math.Round(xc + side * hv), xb = (int)Math.Round(xc + side * (hv + bw));
                for (int x = Math.Min(xa, xb); x <= Math.Max(xa, xb); x++)
                {
                    int cell = 2 + (int)(t * 3);
                    double h = hh * (0.8 + 0.4 * N(x / (2.0 + 3 * t), 3, 41)) + (Fx.R(x / cell, 5, 45) - 0.5) * (1 + 2 * t);
                    for (int j = 0; j < (int)h; j++)
                    {
                        int py = y - j;
                        double rel = j / h; // 0 bottom .. 1 top
                        double v = (1 - rel) * 0.7 + 0.1 + (Fx.R(x / cell, py / cell, 42) - 0.5) * 0.5 + (Fx.R(x, py, 46) - 0.5) * 0.1;
                        // clump lit edges: the upper-left pixel of each leaf clump
                        if (x % cell == 0 && py % cell == 0) v -= 0.28;
                        bool inner = side < 0 ? x >= xb - 1 : x <= xb + 1;
                        if (rel > 0.82) v -= 0.4;
                        else if (inner && rel > 0.4) v -= 0.14;
                        road.Set(x, py, DRamp(ramp, v, x, py, 1.3));
                        if (j == (int)h - 1 && Fx.R(x, py, 43) < 0.5) road.Set(x, py, Fx.R(x, py, 44) < 0.5 ? C("F0D8A8") : C("C8D098"));
                    }
                }
            }
        }
    }


    static void RoadSurface()
    {
        var ramp = Ramp("DDCBA0", "CFBB90", "BFA97E", "AC946C", "947D5E", "7C6A52");
        var rutC = C("7A6654"); var rutD = C("5E5046"); var wet = C("8A7660");
        for (int y = VY; y < 216; y++)
        {
            double xc = RoadX(y), w = RoadW(y), t = RoadT(y);
            int xa = (int)(xc - w / 2 - 3), xb = (int)(xc + w / 2 + 3);
            for (int x = xa; x <= xb; x++)
            {
                double u = (x + .5 - xc) / (w / 2);
                double edge = 1 - Math.Abs(u);
                double jit = (N(x / 5.0, y / 3.0, 71) - 0.5) * 0.22;
                if (edge < 0.0 + jit) continue;
                var col = DRamp(ramp, Math.Pow(t, 0.8) * 0.85 + (N(x / 7.0, y / 2.5, 72) - 0.5) * 0.3 + 0.05, x, y, 1.5);
                // wheel ruts, central hump
                double ru = Math.Abs(Math.Abs(u) - 0.52);
                if (t > 0.08 && ru < 0.13 && edge > 0.12) col = ru < 0.07 ? (Fx.R(x, y, 73) < 0.25 ? rutC : rutD) : rutC;
                if (t > 0.08 && ru >= 0.13 && ru < 0.17 && Math.Abs(u) < 0.7 && Bay8(x, y) < 0.35) col = DRamp(ramp, 0.0, x, y, 1.0);
                if (Math.Abs(u) < 0.28 && t > 0.25 && N(x / 4.0, y / 2.0, 74) > 0.6) col = Mix(col, C("7F9A7A"), 0.55);
                // edge: dither into the verge, with a thin shadow line
                if (edge < 0.12 + jit && Bay8(x, y) < 0.6) col = Mix(col, C("7C6A52"), 0.5);
                if (edge < 0.04 + jit) col = Mix(col, C("5A7A5E"), 0.6);
                if (Fx.R(x, y, 75) < 0.03 * (0.4 + t)) col = Fx.R(x, y, 76) < 0.5 ? C("EADBB4") : C("6C5A4A");
                road.Set(x, y, col);
            }
        }
        // puddles mirroring the pale dawn sky
        foreach (var (pt, side) in new[] { (0.16, -1), (0.31, 1), (0.47, -1), (0.62, 1), (0.77, -1), (0.9, 1) })
        {
            double y = VY + pt * (216 - VY), xc = RoadX(y), w = RoadW(y);
            double cx = xc + side * 0.52 * w / 2 + (pt > 0.7 ? side * 2 : 0), rx = Math.Max(2.5, 0.09 * w * (1 + pt)), ry = Math.Max(1.2, rx * 0.24);
            for (int yy = (int)(y - ry - 1); yy <= y + ry + 1; yy++)
                for (int xx = (int)(cx - rx - 1); xx <= cx + rx + 1; xx++)
                {
                    double dx = (xx + .5 - cx) / rx, dy = (yy + .5 - y) / ry;
                    double wob = (N(xx / 3.0, yy / 2.0, 83) - 0.5) * 0.5;
                    double d = Math.Sqrt(dx * dx + dy * dy) + wob;
                    if (d > 1) continue;
                    Rgba col;
                    if (d > 0.84) col = dy > 0 ? wet : C("A89478");
                    else
                    {
                        double ty = (dy + 1) / 2; // reflected sky: cream at the far edge, peach, then the cooler blue-grey overhead
                        col = DRamp(Ramp("F0E4C8", "E6CDB0", "CDB4A8", "A8A6AA", "8A98A8"), ty * 0.95 + (Fx.R(xx, yy, 82) - 0.5) * 0.1, xx, yy, 1.4);
                        if (dy < -0.5 && dx < 0.1 && dx > -0.7) col = C("FFF4DC");
                    }
                    road.Set(xx, yy, col);
                }
        }
        // grass tufts that creep into the road edge and the central hump
        var rnd = new Random(91);
        for (int i = 0; i < 260; i++)
        {
            int y = VY + 4 + rnd.Next(80); double xc = RoadX(y), w = RoadW(y), t = RoadT(y);
            double u = rnd.NextDouble() < 0.5 ? (rnd.NextDouble() * 0.5 - 0.25) : (rnd.NextDouble() < 0.5 ? -1 : 1) * (0.86 + rnd.NextDouble() * 0.12);
            int x = (int)(xc + u * w / 2); int h = 1 + (int)(t * 4 * rnd.NextDouble());
            for (int j = 0; j < h; j++) road.Set(x, y - j, C("5A7A5E"));
            road.Set(x + 1, y - h + 1, rnd.NextDouble() < 0.5 ? C("D8D49A") : C("8EA67A"));
        }
    }

    static void Posts()
    {
        var wood = C("6E6258"); var woodD = C("524A44"); var woodL = C("A89A82"); var rimC = C("F0D8A8");
        var tops = new List<(int x, int y)>();
        foreach (var (pt, side) in new[] { (0.14, -1), (0.27, -1), (0.43, -1), (0.62, -1), (0.85, -1), (0.2, 1), (0.4, 1), (0.66, 1) })
        {
            double y = VY + pt * (216 - VY), xc = RoadX(y), w = RoadW(y);
            int px = (int)Math.Round(xc + side * (w / 2 + 7 + pt * 14));
            int h = 4 + (int)(pt * 20), wd = 1 + (int)(pt * 3);
            int lean = (px % 3) - 1;
            for (int j = 0; j < h; j++)
                for (int i = 0; i < wd; i++)
                {
                    int ox = (int)Math.Round(lean * j / (double)h);
                    road.Set(px + i + ox, (int)y - j, i == 0 ? woodL : i == wd - 1 && wd > 1 ? woodD : wood);
                }
            road.Set(px - (side > 0 ? 0 : 0), (int)y - h, rimC); road.Set(px + 1, (int)y - h, wood);
            if (side < 0) tops.Add((px, (int)y - h + 1));
            // shadow on the ground towards us
            for (int i = 0; i < wd + 3; i++) road.Set(px + i, (int)y + 1, C("3C5A50"));
        }
        // sagging wire along the left posts
        for (int i = 0; i < tops.Count - 1; i++)
        {
            var a = tops[i]; var b = tops[i + 1];
            int n = Math.Max(Math.Abs(b.x - a.x), Math.Abs(b.y - a.y)) + 1;
            for (int s = 0; s <= n; s++)
            {
                double t = s / (double)n;
                road.Set((int)Math.Round(Lerp(a.x, b.x, t)), (int)Math.Round(Lerp(a.y, b.y, t) + Math.Sin(t * Math.PI) * (1 + i)), C("6C665E"));
            }
        }
    }

    static void Flowers()
    {
        var rnd = new Random(401);
        var cols = new[] { C("F4EAD2"), C("F6C6CF"), C("F2D26B"), C("B8A8E0"), C("EAA5B5"), C("FFF8E8") };
        for (int i = 0; i < 90; i++)
        {
            int y = 136 + (int)(Math.Pow(rnd.NextDouble(), 0.7) * 78); double depth = (y - 130) / 86.0;
            double xc = RoadX(y), w = RoadW(y), hv = VergeHalf(y);
            int x = (int)(xc + (rnd.NextDouble() * 2 - 1) * (hv - 1));
            if (Math.Abs(x - xc) < w / 2 + 3) continue;
            var col = cols[rnd.Next(cols.Length)];
            int h = 1 + (int)(depth * 5);
            for (int j = 0; j < h; j++) road.Set(x, y - j, C("4C6C58"));
            if (depth > 0.55) { road.Set(x - 1, y - h, col); road.Set(x + 1, y - h, col); road.Set(x, y - h - 1, col); road.Set(x, y - h, C("F2D26B")); }
            else road.Set(x, y - h, col);
            if (depth > 0.3 && rnd.NextDouble() < 0.5) road.Set(x + 1, y - h + 2, Mix(col, C("FFF8E8"), 0.4));
        }
    }

    static void FrameTrees()
    {
        var ramp = Ramp("142C34", "1E3F48", "2A5552", "397068", "528A7C");
        var rimA = C("F0D0A0"); var rimB = C("C8B27A");
        // left tree (sun is ahead and centre, so the lit side faces the middle)
        var layer = new Canvas(448, 216);
        void Trunk(int x0, int y0, int w, int lean, bool leftSide)
        {
            for (int y = y0; y < 216; y++)
            {
                double t = (y - y0) / (double)(216 - y0);
                int ox = (int)Math.Round(lean * (1 - t) * (1 - t));
                int ww = w + (int)(t * t * 12) + (y > 200 ? 3 : 0);
                for (int i = 0; i < ww; i++)
                {
                    double u = leftSide ? 1 - (i + 0.5) / ww : (i + 0.5) / ww; // 0 = lit side (facing the sun), 1 = shaded side
                    int k = Math.Clamp((int)(u * 3.2 + (Fx.R(i, y / 4, 91) - 0.5) * 0.9), 0, 3);
                    var col = ramp[Math.Min(3, 3 - k)];
                    if (Fx.R(i, y / 3, 92) > 0.84) col = ramp[Math.Max(0, 1 - k / 2)];
                    if (k == 0 && Fx.R(i, y, 93) < 0.5) col = ramp[4];
                    bool litEdge = leftSide ? i == ww - 1 : i == 0;
                    if (litEdge) col = y % 3 == 0 ? rimB : rimA;
                    layer.Set(x0 + ox + i, y, col);
                }
            }
            // thick limbs reaching into the canopy
            foreach (var (lx, ly) in new[] { (leftSide ? 36 : 412, 96), (leftSide ? 34 : 414, 112) })
            {
                int n = 22;
                for (int s = 0; s <= n; s++)
                {
                    double tt = s / (double)n;
                    int px = (int)Math.Round(Lerp(x0 + w / 2 + (leftSide ? 3 : -3), lx + (leftSide ? 16 : -16), tt)), py = (int)Math.Round(Lerp(ly + 12, ly - 24, tt));
                    layer.Rect(px, py, 3, 3, ramp[1]); layer.Set(leftSide ? px + 2 : px, py, ramp[3]);
                }
            }
        }
        Trunk(22, 66, 9, 8, true);
        Trunk(417, 70, 9, -6, false);
        var cl = MakeClumps(30, 52, 44, 46, 26, 8, 13, 501);
        Foliage(layer, cl, ramp, rimA, rimB, +1, 502, 1.0);
        var cr = MakeClumps(420, 58, 42, 44, 24, 8, 13, 511);
        Foliage(layer, cr, ramp, rimA, rimB, -1, 512, 1.0);
        // low branches
        foreach (var (bx, by, dir) in new[] { (36, 140, 1), (414, 146, -1) })
            for (int i = 0; i < 16; i++) { layer.Rect(bx + dir * i - (dir < 0 ? 1 : 0), by - i / 3, 2, 2, ramp[1]); }
        road.Blit(layer, 0, 0);
        // shaded bushes in the lower corners
        var bush = new Canvas(448, 216);
        Foliage(bush, MakeClumps(24, 208, 56, 14, 11, 7, 11, 521), ramp, rimA, rimB, +1, 522, 0.8);
        Foliage(bush, MakeClumps(426, 208, 54, 14, 11, 7, 11, 531), ramp, rimA, rimB, -1, 532, 0.8);
        road.Blit(bush, 0, 0);
    }
}
