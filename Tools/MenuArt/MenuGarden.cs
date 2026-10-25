using static MenuArt.Kit;

namespace MenuArt;

/// <summary>menu_ground.png (448x216): the family garden at dusk, sun low in the distance at the centre-left (everything is back-lit).</summary>
public static class MenuGarden
{
    public record struct Light(int X, int Y, int W, int H, int R);
    public static readonly List<Light> Lights = new();
    public static (int x, int y) Chimney = (305, 114);
    public static (int x, int y) DoorCentre = (324, 168);

    const int W = 448, H = 216, HB = 178, GT = 158, SunX = MenuSky.SunX + 32;
    static Canvas c = new(W, H);

    static void Dot(int x, int y, Rgba col) => c.Set(x, y, col);

    static readonly Rgba Rim = C("E8A068"), RimSoft = C("C88A62"), Amber = C("D98F4B"), Gold = C("F2D26B"), Cream = C("F4EAD2");
    static readonly Rgba WoodD = C("3A2A2C"), WoodM = C("4F3A38"), WoodL = C("6A4C42"), WoodRim = C("C0804E");

    public static Canvas Ground()
    {
        c = new Canvas(W, H);
        Lights.Clear();
        Hedge();
        Cypress(16, 166, 62, 5.2, 1); Cypress(417, 166, 70, 5.6, 2); Cypress(441, 166, 48, 4.6, 3);
        Grass();
        PathStamp();
        Shadows();
        Beds();
        House();
        Fence();
        Well();
        Scarecrow();
        Props();
        FruitTree();
        Flowers();
        Tufts();
        return c;
    }

    // ------------------------------------------------------------------ backdrop: hedge & cypress
    static int HedgeTop(int x) => 152 + (int)Math.Round(3.2 * N(x / 8.0, 2, 51) + 2.0 * N(x / 3.0, 5, 52) - 2);

    static void Hedge()
    {
        var ramp = Ramp("3B5C46", "2F4E42", "263F39", "1D3030");
        for (int x = 0; x < W; x++)
        {
            int top = HedgeTop(x);
            for (int y = top; y <= 168; y++)
            {
                double t = (y - top) / 12.0;
                double lf = (Fx.R(x / 2, y / 2, 61) - 0.5) * 0.25;
                var col = DRamp(ramp, t + lf, x, y, 1.4);
                c.Set(x, y, col);
            }
            // sun-facing rim: top edge + steps where the left neighbour is lower
            int left = HedgeTop(x - 1);
            for (int y = top; y < Math.Max(top + 1, left); y++)
                if (Fx.R(x, y, 62) < 0.8) c.Set(x, y, Fx.R(x, y, 63) < 0.5 ? C("B8A860") : C("8E9C56"));
            if (Fx.R(x, 0, 64) < 0.08) c.Set(x, top + 2 + (x % 3), C("D4849A")); // a few blossoms
        }
    }

    static void Cypress(int cx, int baseY, int height, double halfW, int seed)
    {
        var layer = new Canvas(W, H);
        var clumps = new List<Clump>();
        int n = height / 4;
        var rnd = new Random(seed * 17);
        for (int i = 0; i < n; i++)
        {
            double t = i / (double)(n - 1);
            double hw = halfW * (0.55 + 0.45 * Math.Sin(Math.PI * Math.Pow(t, 0.7) * 0.95 + 0.25)) * (1 - 0.7 * Math.Pow(t, 3.2));
            double y = baseY - 4 - t * (height - 8);
            clumps.Add(new Clump(cx + (rnd.NextDouble() - 0.5) * 1.2, y, Math.Max(2.2, hw)));
            clumps.Add(new Clump(cx + (rnd.NextDouble() - 0.5) * 2, y + 2, Math.Max(2.0, hw * 0.8)));
        }
        clumps.Sort((a, b) => a.Y.CompareTo(b.Y));
        Foliage(layer, clumps, Ramp("152424", "1C3430", "27483B", "355C46"), C("D2B062"), C("98A556"), -1, 70 + seed);
        c.Blit(layer, 0, 0);
        // trunk stub
        c.Rect(cx - 1, baseY - 2, 2, 4, C("2A2022"));
    }

    // ------------------------------------------------------------------ ground plane
    static void Grass()
    {
        var ramp = Ramp("5E7050", "4F6646", "415841", "354A3A", "2B3E33", "22332C");
        var warm = C("7C7E4E");
        for (int x = 0; x < W; x++)
        {
            int top = GT + (int)Math.Round(2.2 * N(x / 15.0, 4, 81) - 1);
            for (int y = top; y < H; y++)
            {
                double t = (y - GT) / 58.0;
                t = t * 0.95 + (N(x / 26.0, y / 8.0, 82) - 0.5) * 0.32;
                var col = DRamp(ramp, t, x, y, 1.5);
                double wf = Gauss(x - SunX, 120) * (1 - (y - GT) / 16.0);
                if (wf > 0 && wf * 0.7 > Bay8(x, y)) col = warm;
                c.Set(x, y, col);
            }
        }
        // blade tufts: dark base, thin warm sun-side highlight; bigger and denser towards the viewer
        var rnd = new Random(88);
        for (int i = 0; i < 1500; i++)
        {
            int x = rnd.Next(W), y = GT + 1 + (int)(Math.Pow(rnd.NextDouble(), 0.85) * 56);
            double depth = (y - GT) / 58.0;
            int h = 1 + (int)(depth * 3.2 * rnd.NextDouble()) + (depth > 0.5 ? 1 : 0);
            var dark = DRamp(ramp, depth + 0.28, x, y, 1.0);
            for (int j = 0; j < h; j++) c.Set(x, y - j, dark);
            if (rnd.NextDouble() < 0.7) c.Set(x - 1, y - h + 1 + (h > 2 ? 1 : 0), Fx.R(x, y, 83) < 0.4 ? C("A4A45A") : C("7E8C50"));
            if (h >= 3 && rnd.NextDouble() < 0.5) c.Set(x + 1, y - h + 1, dark);
        }
    }

    // ------------------------------------------------------------------ path from the doorway to the horizon
    static readonly (double x, double y)[] PathPts =
    {
        (323, 178), (322, 184), (316, 189), (300, 192), (278, 192), (254, 188), (232, 181), (214, 173), (200, 166), (192, 161), (188, 158),
    };

    static (double x, double y) CR(int i, double t)
    {
        (double, double) P(int k) => PathPts[Math.Clamp(k, 0, PathPts.Length - 1)];
        var (p0, p1, p2, p3) = (P(i - 1), P(i), P(i + 1), P(i + 2));
        double t2 = t * t, t3 = t2 * t;
        double X(double a, double b, double cc, double d) => 0.5 * ((2 * b) + (-a + cc) * t + (2 * a - 5 * b + 4 * cc - d) * t2 + (-a + 3 * b - 3 * cc + d) * t3);
        return (X(p0.Item1, p1.Item1, p2.Item1, p3.Item1), X(p0.Item2, p1.Item2, p2.Item2, p3.Item2));
    }

    static bool[,] pathMask = new bool[W, H];

    static void PathStamp()
    {
        pathMask = new bool[W, H];
        for (int i = 0; i < PathPts.Length - 1; i++)
            for (int s = 0; s < 80; s++)
            {
                var (px, py) = CR(i, s / 80.0);
                double w = 3.2 + (py - GT) * 0.30;
                double rx = w / 2 + (Fx.R((int)px, (int)py, 91) - 0.5) * 0.8, ry = Math.Max(1.2, w * 0.27);
                for (int y = (int)(py - ry - 1); y <= py + ry + 1; y++)
                    for (int x = (int)(px - rx - 1); x <= px + rx + 1; x++)
                    {
                        double dx = (x + .5 - px) / rx, dy = (y + .5 - py) / ry;
                        if (dx * dx + dy * dy <= 1 && x >= 0 && y >= 0 && x < W && y < H) pathMask[x, y] = true;
                    }
            }
        // lit fan in front of the doorway is added to the colour pass below
        var ramp = Ramp("C9A074", "B88D6C", "9E7664", "846050", "6C4E4A");
        var edge = C("5C4244");
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (!pathMask[x, y]) continue;
                bool Nb(int xx, int yy) => xx >= 0 && yy >= 0 && xx < W && yy < H && pathMask[xx, yy];
                bool border = !Nb(x - 1, y) || !Nb(x + 1, y) || !Nb(x, y - 1) || !Nb(x, y + 1);
                double depth = Clamp01((y - GT) / 36.0);
                double toSun = Gauss(x - SunX, 110);
                double t = 0.35 + 0.55 * depth - 0.25 * toSun * (1 - depth) + (N(x / 6.0, y / 3.0, 92) - 0.5) * 0.4;
                var col = DRamp(ramp, t, x, y, 1.3);
                // door light fan (baked): warm, widening towards the viewer
                double fdx = (x - 323) / (4.0 + (y - 178) * 0.7);
                if (y >= 178 && y < 198 && Math.Abs(fdx) < 1.0)
                {
                    double f = (1 - Math.Abs(fdx)) * (1 - (y - 178) / 20.0);
                    if (f * 1.1 > Bay8(x, y)) col = f > 0.5 ? C("E0B060") : C("C89458");
                }
                if (border && Fx.R(x, y, 93) < 0.55) col = Mix(col, C("3F5A3E"), 0.55);
                else if (border) col = Mix(col, edge, 0.4);
                if (!border && Fx.R(x, y, 94) < 0.045) col = C("D8B48A"); // pebbles
                c.Set(x, y, col);
            }
    }

    // ------------------------------------------------------------------ cast shadows (sun is ahead-left: shadows run towards the viewer and to the right)
    static void Shadows()
    {
        var dk = new[] { C("1E2E28"), C("1A2A26"), C("162420") };
        ShadowEllipse(c, 254, 188, 34, 5.2, dk, 0.5);   // fruit tree
        ShadowEllipse(c, 348, 185, 76, 5.0, dk, 0.35);  // house
        ShadowEllipse(c, 158, 186, 10, 2.6, dk, 0.5);   // scarecrow
        ShadowEllipse(c, 58, 183, 18, 3.2, dk, 0.5);    // well
        ShadowEllipse(c, 424, 170, 22, 3.4, dk, 0.6);   // cypress
    }

    // ------------------------------------------------------------------ vegetable beds
    static void Beds()
    {
        Bed(74, 170, 62, 17, 0);
        Bed(147, 173, 60, 16, 1);
    }

    static void Bed(int x0, int y0, int w, int h, int kind)
    {
        var soil = Ramp("2E2226", "3E2E2E", "533C38", "6A4C42", "8A6650");
        var plant = Ramp("203629", "2B4636", "37573F", "4A6B45");
        for (int y = 0; y < h; y++)
        {
            double p = y / (double)(h - 1);
            int inset = (int)Math.Round(4 * (1 - p));
            for (int x = inset; x < w - inset; x++)
            {
                int row = y % 4;
                Rgba col = row == 0 ? soil[4] : row == 1 ? soil[3] : row == 2 ? soil[2] : soil[0];
                if (row == 1 && Fx.R(x, y, 111) < 0.3) col = soil[2];
                if (x == inset || x == w - inset - 1) col = row == 0 ? WoodRim : WoodM;
                c.Set(x0 + x, y0 + y, col);
            }
        }
        // front board
        c.HL(x0 + 0, y0 + h, w, WoodD); c.HL(x0 + 1, y0 + h - 1, w - 2, WoodM); c.Set(x0, y0 + h - 1, WoodRim);
        // plants along each ridge row
        for (int row = 0; row < 4; row++)
        {
            int ry = y0 + 2 + row * 4;
            double p = (ry - y0) / (double)h;
            int inset = (int)Math.Round(4 * (1 - p)) + 3;
            int step = kind == 0 ? 8 : 10;
            for (int x = x0 + inset + (row % 2) * 3; x < x0 + w - inset - 3; x += step)
            {
                if (kind == 0) Cabbage(x, ry, (x / step + row) % 3 == 0);
                else Bean(x, ry, row);
            }
        }
    }

    static void Cabbage(int x, int y, bool purple)
    {
        var d = purple ? Ramp("3A2A48", "54406A", "6E5688", "9A82A8") : Ramp("203629", "2F4E3A", "416448", "5E8052");
        for (int j = -3; j <= 0; j++)
            for (int i = -3; i <= 3; i++)
            {
                if (Math.Abs(i) * 1.0 + Math.Abs(j + 1) * 1.5 > 4.2) continue;
                int k = (i < 0 && j < -1) ? 2 : (i > 1 || j == 0) ? 0 : 1;
                c.Set(x + i, y + j, d[k]);
            }
        c.Set(x - 3, y - 2, Fx.R(x, y, 112) < 0.5 ? C("C8B060") : d[3]); c.Set(x - 2, y - 3, d[3]);
    }

    static void Bean(int x, int y, int row)
    {
        // pole with leafy runner (back-lit dark)
        c.VL(x, y - 11, 12, WoodM); c.VL(x - 1, y - 11, 12, Fx.R(x, y, 113) < 0.4 ? WoodRim : WoodD);
        for (int j = 0; j < 9; j++)
        {
            int o = (j % 3 == 0) ? 1 : (j % 3 == 1 ? 2 : 0);
            c.Set(x + o, y - 10 + j, C("2B4636")); c.Set(x - 2 - o + 1, y - 9 + j, C("37573F"));
            if (j % 4 == 1) c.Set(x - 3, y - 9 + j, C("A0A458"));
        }
        c.Set(x, y - 12, WoodM); c.Set(x - 1, y - 12, WoodRim);
    }

    // ------------------------------------------------------------------ the house
    static void House()
    {
        const int wx0 = 282, wx1 = 366, wy0 = 154, wy1 = HB;
        // --- chimney (behind roof)
        for (int y = 118; y < 142; y++)
            for (int x = 301; x < 309; x++)
            {
                bool mort = ((y - 118) % 3 == 2) || (((y - 118) / 3) % 2 == 0 ? (x - 301) % 4 == 3 : (x - 301) % 4 == 1);
                c.Set(x, y, x == 301 ? C("D9784F") : mort ? C("4C2630") : x >= 307 ? C("5A2C32") : C("7A4440"));
            }
        c.Rect(299, 116, 11, 3, C("5C4A54")); c.HL(299, 116, 11, C("D0A880")); c.VL(299, 117, 2, C("B08A74")); c.HL(299, 118, 11, C("3A2C3A"));
        c.Rect(301, 115, 7, 1, C("1E1418")); // flue shadow
        Chimney = (305, 114);
        Smoke(305, 114);

        // --- roof (trapezoid seen from the front)
        var roofPoly = new (double, double)[] { (274, 155), (374, 155), (357, 134), (291, 134) };
        for (int y = 134; y <= 155; y++)
            for (int x = 273; x <= 375; x++)
            {
                if (!InPoly(roofPoly, x + .5, y + .5)) continue;
                int row = (y - 134) / 3, ry = (y - 134) % 3;
                int seam = (x + (row % 2) * 3) % 6;
                Rgba col = ry == 2 ? C("4C2630") : ry == 0 ? C("8E4642") : C("6E3438");
                if (seam == 0 && ry != 2) col = C("55292F");
                if (Fx.R(x, y, 121) < 0.06) col = C("7E3C3E");
                // right side a touch darker (away from the sun)
                if (x > 340 && ry == 1 && Bay8(x, y) < 0.35) col = C("5A2C32");
                c.Set(x, y, col);
            }
        // sun-facing rim along the left slope + the ridge line
        for (int y = 134; y <= 155; y++)
        {
            double xl = 291 + (274 - 291) * (y - 134) / 21.0;
            int ix = (int)Math.Round(xl);
            c.Set(ix, y, Rim); c.Set(ix + 1, y, ((y & 1) == 0) ? C("D9784F") : C("B25A48"));
            if (y % 2 == 0) c.Set(ix + 2, y, C("8E4642"));
        }
        for (int x = 292; x < 357; x++) { c.Set(x, 134, x < 330 ? Rim : RimSoft); if (x % 3 != 0) c.Set(x, 135, C("A64F46")); }
        // eave overhang
        c.HL(274, 154, 101, C("2E1A22")); c.HL(275, 155, 99, C("3B222A"));

        // --- walls
        for (int y = 156; y < wy1; y++)
            for (int x = wx0; x < wx1; x++)
            {
                double eave = Clamp01(1 - (y - 156) / 8.0);
                double low = Smooth(wy1 - 8, wy1, y);
                double mott = (N(x / 5.0, y / 4.0, 131) - 0.5) * 0.34;
                double t = 0.52 + mott - eave * 0.5 - low * 0.35 + (x - 282) / 84.0 * -0.12;
                var col = DRamp(Ramp("4E3C4E", "5E4A5C", "76606C", "8E7580", "A68A90"), t, x, y, 1.5);
                c.Set(x, y, col);
            }
        c.VL(wx0, 156, wy1 - 156, Rim); c.VL(wx0 + 1, 156, wy1 - 156, RimSoft);
        for (int y = 156; y < wy1; y += 2) c.Set(wx0 + 1, y, C("A87868"));
        c.VL(wx1 - 1, 156, wy1 - 156, C("3C2E3E"));
        // quoins
        for (int k = 0; k < 6; k++)
        {
            int qy = 157 + k * 4;
            c.Rect(wx0 + 2, qy, k % 2 == 0 ? 4 : 3, 3, C("8C7478")); c.HL(wx0 + 2, qy, k % 2 == 0 ? 4 : 3, C("B09890"));
            c.Rect(wx1 - 5, qy, k % 2 == 0 ? 3 : 4, 3, C("5E4A58")); 
            if (k % 2 == 1) c.Rect(wx1 - 6, qy, 4, 3, C("5E4A58"));
        }
        // foundation
        for (int x = wx0; x < wx1; x++) { c.HL(x, wy1 - 3, 1, C("3E3240")); c.HL(x, wy1 - 2, 1, ((x / 5) % 2 == 0) ? C("4A3C4C") : C("40344A")); c.HL(x, wy1 - 1, 1, C("2C2230")); }
        c.HL(wx0, wy1 - 3, wx1 - wx0, C("6A5668"));

        // --- openings
        Door(318, 158, 12, 20);
        Window(289, 160, 10, 10, true);
        Window(341, 160, 10, 10, true);
        SmallWindow(306, 163, 7, 7);

        // right-hand barrel + left woodpile
        Barrel(372, HB);
        WoodPile(262, HB);
    }

    static void Smoke(int x, int y)
    {
        var col = C("5A4468"); var col2 = C("6E5472");
        for (int i = 0; i < 60; i++)
        {
            double t = i / 59.0;
            double sx = x + 20 * t * t + 3.2 * Math.Sin(t * 7), sy = y - 4 - t * 34;
            double r = 1.8 + t * 4.4;
            for (int yy = (int)(sy - r - 1); yy <= sy + r + 1; yy++)
                for (int xx = (int)(sx - r - 1); xx <= sx + r + 1; xx++)
                {
                    double d = Math.Sqrt((xx + .5 - sx) * (xx + .5 - sx) + (yy + .5 - sy) * (yy + .5 - sy) * 1.5);
                    if (d > r) continue;
                    double dens = (1 - d / r) * (1 - t * 0.88) * 1.25;
                    if (dens > Bay8(xx, yy)) c.Set(xx, yy, dens > 0.55 ? col : col2);
                }
        }
    }

    static void Door(int x0, int y0, int w, int h)
    {
        var interior = Ramp("F8DE8C", "F2D26B", "ECB958", "E2A04C", "D08844", "A8683A");
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // arched head
                if (y == 0 && (x < 3 || x >= w - 3)) continue;
                if (y == 1 && (x < 2 || x >= w - 2)) continue;
                if (y == 2 && (x < 1 || x >= w - 1)) continue;
                double t = y / (double)(h - 1);
                var col = DRamp(interior, t * 0.9 + (x > w / 2 ? 0.07 : 0), x0 + x, y0 + y, 1.4);
                c.Set(x0 + x, y0 + y, col);
            }
        // frame in lit stone (left side catches the sun)
        for (int y = 0; y < h; y++) { c.Set(x0 - 1, y0 + y, y > 2 ? C("B49A94") : C("8E7480")); c.Set(x0 + w, y0 + y, C("4A3A4A")); }
        c.HL(x0 + 2, y0 - 1, w - 4, C("A28890")); c.Set(x0, y0, C("8E7480")); c.Set(x0 + w - 1, y0, C("4A3A4A")); c.Set(x0 + 1, y0 - 1, C("8E7480")); c.Set(x0 + w - 2, y0 - 1, C("4A3A4A"));
        c.Set(x0 - 1, y0 + 2, C("8E7480")); c.Set(x0 + w, y0 + 2, C("4A3A4A")); c.Set(x0, y0 + 1, C("8E7480")); c.Set(x0 + w - 1, y0 + 1, C("4A3A4A"));
        // interior: back wall shelf, table and a stool as dark shapes
        c.Rect(x0 + 7, y0 + 14, 4, 1, C("5A3A2E")); c.Rect(x0 + 7, y0 + 15, 1, 4, C("5A3A2E")); c.Rect(x0 + 10, y0 + 15, 1, 4, C("5A3A2E")); // table
        c.Rect(x0 + 7, y0 + 13, 2, 1, C("F8DE8C")); // loaf / lamp highlight
        c.Rect(x0 + 3, y0 + 15, 3, 1, C("5A3A2E")); c.Rect(x0 + 3, y0 + 16, 1, 3, C("5A3A2E")); c.Rect(x0 + 5, y0 + 16, 1, 3, C("5A3A2E")); // stool
        c.Rect(x0 + 2, y0 + 5, 8, 1, C("C48A48")); // lintel shadow gradient
        c.HL(x0, y0 + h - 1, w, C("7A5238"));
        // open door leaf, seen nearly edge-on
        c.Rect(x0, y0 + 3, 3, h - 3, WoodM); c.VL(x0, y0 + 3, h - 3, WoodRim); c.VL(x0 + 2, y0 + 3, h - 3, WoodD);
        for (int y = y0 + 5; y < y0 + h - 2; y += 4) c.HL(x0, y, 3, WoodD);
        // steps
        c.Rect(x0 - 2, y0 + h, w + 4, 1, C("9A8088")); c.HL(x0 - 2, y0 + h, w + 4, C("C0A090"));
        c.Rect(x0 - 3, y0 + h + 1, w + 6, 1, C("6A5668")); c.HL(x0 - 3, y0 + h + 1, w + 6, C("8A7078"));
        Lights.Add(new Light(x0, y0, w, h, 15));
        DoorCentre = (x0 + w / 2, y0 + h / 2);
    }

    static void Window(int x0, int y0, int w, int h, bool shutters)
    {
        var glow = Ramp("F8DE8C", "F2D26B", "ECBA58", "D98F4B");
        // surround + sill
        c.Rect(x0 - 1, y0 - 1, w + 2, h + 2, C("A58C92")); c.HL(x0 - 1, y0 - 1, w + 2, C("C4A898")); c.VL(x0 + w, y0 - 1, h + 2, C("5E4A58"));
        c.Rect(x0, y0, w, h, WoodD);
        for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                double t = (y - 1) / (double)(h - 3) * 0.9 + (x > w / 2 ? 0.06 : 0);
                c.Set(x0 + x, y0 + y, DRamp(glow, t, x0 + x, y0 + y, 1.4));
            }
        c.Rect(x0 + w / 2 - 1, y0 + 1, 1, h - 2, C("5A3A2E")); c.HL(x0 + 1, y0 + h / 2, w - 2, C("5A3A2E"));
        c.Rect(x0 + 2, y0 + 2, 2, 2, C("FBEFC2")); c.Set(x0 + w - 4, y0 + 2, C("FBE9A8"));
        c.HL(x0 - 2, y0 + h + 1, w + 4, C("B8A098")); c.HL(x0 - 2, y0 + h + 2, w + 4, C("3A2C3A"));
        if (shutters)
            foreach (var sx in new[] { x0 - 5, x0 + w + 2 })
            {
                for (int y = 0; y < h + 2; y++)
                    for (int x = 0; x < 3; x++)
                        c.Set(sx + x, y0 - 1 + y, (y % 2 == 0) ? C("2E5252") : C("234242"));
                c.VL(sx, y0 - 1, h + 2, sx < x0 ? C("78BAB0") : C("3A6A68")); c.VL(sx + 2, y0 - 1, h + 2, C("1C3434"));
            }
        // flower box
        int bx = x0 - 1, by = y0 + h + 3;
        c.Rect(bx, by, w + 2, 3, WoodM); c.HL(bx, by, w + 2, WoodL); c.VL(bx, by, 3, WoodRim); c.HL(bx, by + 2, w + 2, WoodD);
        var fl = new[] { C("D4849A"), Cream, Gold, C("9F8CC4"), C("D98F4B") };
        for (int i = 0; i < w + 2; i += 2)
        {
            c.Set(bx + i, by - 1, C("2F4E3C")); if (i % 4 == 0) c.Set(bx + i + 1, by - 1, C("3F6648"));
            c.Set(bx + i, by - 2, fl[(i / 2 + x0) % fl.Length]);
            if (i % 4 == 2) c.Set(bx + i, by - 3, fl[(i / 2 + x0 + 2) % fl.Length]);
        }
        Lights.Add(new Light(x0 + 1, y0 + 1, w - 2, h - 2, 9));
    }

    static void SmallWindow(int x0, int y0, int w, int h)
    {
        var glow = Ramp("F8DE8C", "F2D26B", "E6A850");
        c.Rect(x0 - 1, y0 - 1, w + 2, h + 2, C("9A8088")); c.HL(x0 - 1, y0 - 1, w + 2, C("BCA090")); c.VL(x0 + w, y0 - 1, h + 2, C("5E4A58"));
        c.Rect(x0, y0, w, h, WoodD);
        for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++) c.Set(x0 + x, y0 + y, DRamp(glow, (y - 1) / (double)(h - 3), x0 + x, y0 + y, 1.4));
        c.VL(x0 + w / 2, y0 + 1, h - 2, C("5A3A2E"));
        c.Set(x0 + 1, y0 + 1, C("FBEFC2"));
        c.HL(x0 - 1, y0 + h + 1, w + 2, C("B8A098"));
        Lights.Add(new Light(x0 + 1, y0 + 1, w - 2, h - 2, 7));
    }

    static void Barrel(int x, int by)
    {
        for (int y = 0; y < 11; y++)
        {
            int inset = y == 0 || y == 10 ? 1 : 0;
            for (int i = inset; i < 9 - inset; i++)
            {
                var col = i == 0 ? WoodRim : i < 3 ? WoodL : i < 6 ? WoodM : WoodD;
                if (y == 2 || y == 8) col = i == 0 ? Rim : C("2A2026");
                c.Set(x + i, by - 11 + y, col);
            }
        }
        c.HL(x + 1, by - 11, 7, C("9A7A60")); c.HL(x + 1, by - 12, 7, C("8A6A58"));
    }

    static void WoodPile(int x, int by)
    {
        var ends = Ramp("5A4036", "7A5A48", "9A7458");
        for (int row = 0; row < 3; row++)
            for (int i = 0; i < 5 - row; i++)
            {
                int cx = x + i * 3 + row * 2, cy = by - 2 - row * 3;
                c.Rect(cx, cy - 1, 3, 3, WoodD); c.Set(cx + 1, cy, ends[1]); c.Set(cx, cy - 1, ends[2]); c.Set(cx + 1, cy - 1, ends[1]);
                if (i == 0) c.Set(cx - 1 + 0, cy, WoodRim);
            }
    }

    // ------------------------------------------------------------------ fence with an open gate for the path
    static void Fence()
    {
        const int top = 180, bot = 189;
        void Picket(int x)
        {
            for (int y = top; y <= bot; y++)
            {
                c.Set(x, y, C("6A4C42")); c.Set(x + 1, y, WoodM); c.Set(x + 2, y, WoodD);
            }
            c.Set(x + 1, top - 1, WoodM); c.Set(x, top, WoodRim); c.Set(x, top + 1, Rim);
            if (Fx.R(x, 0, 141) < 0.5) c.Set(x, top + 3, WoodRim);
        }
        void Post(int x)
        {
            for (int y = top - 3; y <= bot + 1; y++) { c.Set(x, y, C("7A5448")); c.Set(x + 1, y, WoodM); c.Set(x + 2, y, WoodM); c.Set(x + 3, y, WoodD); }
            c.HL(x - 1, top - 4, 6, C("5A4036")); c.HL(x - 1, top - 5, 5, C("8A6450")); c.Set(x - 1, top - 4, Rim); c.Set(x, top - 5, Rim); c.VL(x, top - 3, 5, WoodRim);
        }
        void Rails(int xa, int xb)
        {
            for (int x = xa; x < xb; x++) { c.Set(x, top + 2, x % 7 == 0 ? WoodRim : C("5A4038")); c.Set(x, top + 3, WoodD); c.Set(x, top + 6, C("5A4038")); c.Set(x, top + 7, WoodD); }
        }
        const int gl = 313, gr = 335;
        for (int x = 258; x < gl - 3; x += 4) { Picket(x); }
        Rails(258, gl); Post(258); Post(gl - 3);
        for (int x = gr + 5; x < 398; x += 4) Picket(x);
        Rails(gr, 398); Post(gr); Post(396);
        // open gate leaf swung towards the viewer, hinged on the right post
        for (int k = 0; k < 3; k++)
        {
            int gx = gr + 5 + k * 4;
            for (int y = top + 1 + k; y <= bot + 1 + k / 2; y++) { c.Set(gx, y, C("6A4C42")); c.Set(gx + 1, y, WoodM); c.Set(gx + 2, y, WoodD); }
            c.Set(gx + 1, top + k, WoodM); c.Set(gx, top + 1 + k, Rim);
        }
        for (int x = gr + 4; x < gr + 17; x++) { int dy = (x - gr - 4) / 4; c.Set(x, top + 3 + dy, C("5A4038")); c.Set(x, top + 7 + dy, C("5A4038")); }
    }

    // ------------------------------------------------------------------ the well
    static void Well()
    {
        const int cx = 50, by = 179;
        // stone ring (back rim, water, front wall)
        var st = Ramp("3E343E", "54464E", "6E5C60", "8E7A76", "B8A08C");
        for (int y = 0; y < 11; y++)
            for (int x = -11; x <= 11; x++)
            {
                double ex = x / 11.5, ey = (y - 5.0) / (y < 5 ? 5.6 : 6.0);
                if (y < 5 && ex * ex + ey * ey > 1) continue;
                if (y >= 5 && Math.Abs(ex) > 1) continue;
                if (y > 8 && (ex * ex + (y - 8) * (y - 8) / 6.5) > 1) continue;
                int row = (y) / 3; int off = (row % 2) * 3;
                bool joint = (y % 3 == 2) || ((x + off + 40) % 6 == 0);
                int k = x < -6 ? 3 : x < -1 ? 2 : x < 5 ? 1 : 0;
                var col = st[Math.Clamp(k + (joint ? -1 : 0) + (Fx.R(x, y, 151) < 0.12 ? 1 : 0), 0, 4)];
                if (y < 5) col = st[1];
                c.Set(cx + x, by - 11 + y, col);
            }
        // water in the opening, catching the sky
        for (int y = 3; y < 7; y++) for (int x = -7; x <= 7; x++)
            {
                double ex = x / 7.2, ey = (y - 5) / 2.2; if (ex * ex + ey * ey > 1) continue;
                c.Set(cx + x, by - 11 + y, C("1E2A32"));
            }
        c.Set(cx - 3, by - 7, C("D9A060")); c.Set(cx - 2, by - 7, C("C48A50")); c.Set(cx + 3, by - 8, C("8A6A70"));
        for (int x = -11; x <= 11; x++) { double ex = x / 11.5; int ty = (int)Math.Round(5.0 - 5.6 * Math.Sqrt(Math.Max(0, 1 - ex * ex))); c.Set(cx + x, by - 11 + ty, x < -2 ? C("D2BC9C") : C("8E7A76")); }
        c.Set(cx - 11, by - 6, Rim); c.Set(cx - 11, by - 5, Rim); c.Set(cx - 10, by - 7, Rim);
        // posts, crossbar, tiled roof
        foreach (var px in new[] { cx - 9, cx + 8 })
            for (int y = by - 30; y < by - 8; y++) { c.Set(px, y, px < cx ? WoodRim : C("5A4038")); c.Set(px + 1, y, WoodM); c.Set(px + 2, y, WoodD); }
        c.HL(cx - 9, by - 27, 20, WoodD); c.HL(cx - 9, by - 28, 20, WoodM);
        var roofPoly = new (double, double)[] { (cx - 15, by - 29), (cx + 16, by - 29), (cx + 4, by - 40), (cx - 3, by - 40) };
        for (int y = by - 41; y <= by - 28; y++) for (int x = cx - 16; x <= cx + 17; x++)
            {
                if (!InPoly(roofPoly, x + .5, y + .5)) continue;
                int ry = (y - (by - 41)) % 3;
                c.Set(x, y, ry == 2 ? C("4C2630") : ry == 0 ? C("8E4642") : C("6E3438"));
            }
        for (int y = by - 40; y <= by - 29; y++) { int xl = (int)Math.Round(cx - 3 + (cx - 15 - (cx - 3)) * (y - (by - 40)) / 11.0); c.Set(xl, y, Rim); c.Set(xl + 1, y, C("D9784F")); }
        c.HL(cx - 3, by - 40, 7, Rim);
        c.HL(cx - 15, by - 28, 31, C("2E1A22"));
        // rope + bucket
        c.VL(cx + 1, by - 27, 7, C("B09068")); c.Rect(cx - 1, by - 20, 5, 3, WoodM); c.HL(cx - 1, by - 20, 5, WoodRim); c.HL(cx - 1, by - 18, 5, WoodD);
    }

    // ------------------------------------------------------------------ the scarecrow
    static void Scarecrow()
    {
        const int cx = 142, by = 184;
        var dark = C("2E2432"); var coat = C("3E2E40"); var patch = C("5A4250"); var straw = C("D9A850"); var strawL = C("F2D26B");
        // pole
        for (int y = by - 30; y <= by; y++) { c.Set(cx, y, C("5A3E32")); c.Set(cx + 1, y, WoodD); }
        c.VL(cx - 1, by - 24, 24, WoodRim);
        // crossbar
        c.HL(cx - 11, by - 22, 24, WoodD); c.HL(cx - 11, by - 23, 24, C("5A3E32")); c.Set(cx - 11, by - 23, Rim);
        // coat
        for (int y = by - 22; y < by - 10; y++)
            for (int x = cx - 5; x <= cx + 6; x++)
            {
                int taper = (y - (by - 22)) > 8 ? 1 : 0;
                if (x < cx - 5 + taper || x > cx + 6 - taper) continue;
                c.Set(x, y, x < cx - 2 ? coat : x > cx + 3 ? dark : C("34283A"));
            }
        c.VL(cx - 5, by - 22, 8, RimSoft); c.Set(cx - 5, by - 22, Rim); c.Set(cx - 4, by - 21, C("B07A52"));
        c.Rect(cx - 3, by - 19, 3, 3, patch); c.Rect(cx + 2, by - 15, 2, 2, C("6A5A3E"));
        c.HL(cx - 5, by - 10, 12, dark); for (int x = cx - 5; x <= cx + 6; x += 2) c.Set(x, by - 9, dark); c.Set(cx - 3, by - 8, dark); c.Set(cx + 3, by - 8, dark);
        // sleeves
        for (int s = -1; s <= 1; s += 2)
            for (int i = 1; i <= 6; i++)
            {
                int x = cx + s * (5 + i); int y = by - 23 + (i > 4 ? 1 : 0);
                c.Set(x, y, coat); c.Set(x, y + 1, i % 2 == 0 ? dark : coat); if (i > 2) c.Set(x, y + 2, dark);
            }
        foreach (var (sx, sy) in new[] { (cx - 12, by - 23), (cx - 13, by - 22), (cx - 12, by - 21), (cx + 12, by - 23), (cx + 13, by - 22), (cx + 12, by - 21), (cx + 13, by - 24) })
        { c.Set(sx, sy, ((sx + sy) & 1) == 0 ? straw : strawL); }
        c.Set(cx - 14, by - 23, straw);
        // head (sack) + hat
        c.Disc(cx + 0.5, by - 27, 3.4, C("3A2C30")); c.Set(cx - 3, by - 28, C("C8A060")); c.Set(cx - 2, by - 29, C("B08A58")); c.Set(cx - 3, by - 27, Rim);
        c.Set(cx - 1, by - 27, C("F2D26B")); c.Set(cx + 2, by - 27, C("F2D26B")); // stitched eyes catching the last light
        c.HL(cx - 7, by - 31, 15, C("3E3026")); c.HL(cx - 7, by - 32, 15, C("5A4636"));
        for (int y = by - 38; y < by - 32; y++) for (int x = cx - 3 - (y - (by - 38)) / 3; x <= cx + 3 + (y - (by - 38)) / 3; x++) c.Set(x, y, x < cx - 1 ? C("5A4636") : C("3E3026"));
        c.HL(cx - 3, by - 38, 5, Rim); c.Set(cx - 4, by - 37, Rim); c.Set(cx - 5, by - 36, RimSoft); c.Set(cx - 7, by - 32, Rim); c.Set(cx - 6, by - 32, C("D9A860"));
        c.HL(cx - 3, by - 33, 7, C("8A3E36")); // hat band
        // straw sticking out under the hat and from the coat hem
        foreach (var (sx, sy) in new[] { (cx - 4, by - 30), (cx + 5, by - 30), (cx + 6, by - 29), (cx - 3, by - 9), (cx + 2, by - 8), (cx - 6, by - 10) }) c.Set(sx, sy, straw);
        // a crow keeping watch on the right arm
        int bx = cx + 10, bby = by - 24;
        c.Rect(bx, bby - 2, 4, 2, C("1E1A24")); c.Set(bx + 4, bby - 2, C("1E1A24")); c.Set(bx + 5, bby - 1, C("C89050")); c.Set(bx - 1, bby - 1, C("1E1A24")); c.Set(bx, bby - 3, C("1E1A24")); c.Set(bx + 1, bby - 3, Rim);
    }

    // ------------------------------------------------------------------ the fruit tree
    static void FruitTree()
    {
        const int tx = 236, by = 183;
        // trunk and branches
        for (int y = 148; y <= by; y++)
        {
            int w = y > by - 6 ? 6 : y > 160 ? 5 : 4;
            for (int i = 0; i < w; i++) c.Set(tx - w / 2 + i, y, i == 0 ? C("B07A52") : i < w - 2 ? C("4A3228") : C("2A2022"));
        }
        for (int i = 0; i < 12; i++) { c.Set(tx - 3 - i / 2, 152 - i, C("3A2A28")); c.Set(tx + 3 + i / 2, 150 - i, C("2A2022")); }
        c.Set(tx - 5, by - 1, C("2A2022")); c.Set(tx + 4, by - 1, C("2A2022")); c.Set(tx - 6, by, C("2A2022")); c.Set(tx + 5, by, C("2A2022"));
        var layer = new Canvas(W, H);
        var cl = MakeClumps(tx, 136, 28, 21, 19, 6.5, 11, 201);
        Foliage(layer, cl, Ramp("1B2F30", "253E3A", "30504A", "3E6250", "567C54"), C("E0B866"), C("A8AC5C"), -1, 202);
        // fruit: warm dots on the sun-lit lower-left of the canopy
        var rnd = new Random(203);
        int placed = 0;
        for (int tries = 0; tries < 400 && placed < 13; tries++)
        {
            int fx = tx - 26 + rnd.Next(52), fy = 118 + rnd.Next(38);
            if (!layer.Opaque(fx, fy) || !layer.Opaque(fx + 1, fy + 1) || !layer.Opaque(fx - 1, fy)) continue;
            var body = placed % 3 == 0 ? C("C05A4A") : C("D98F4B");
            layer.Set(fx, fy, body); layer.Set(fx + 1, fy, body); layer.Set(fx, fy + 1, body); layer.Set(fx + 1, fy + 1, Mix(body, C("5A2C32"), 0.5));
            layer.Set(fx, fy, placed % 3 == 0 ? C("E58C66") : C("F2C27A"));
            placed++;
        }
        c.Blit(layer, 0, 0);
    }

    // ------------------------------------------------------------------ small props: wheelbarrow, watering can, bench
    static void Props()
    {
        // wheelbarrow of pumpkins
        int wx = 100, by = 200;
        for (int j = 0; j < 6; j++)
            for (int i = j / 2; i < 19 - j / 2; i++) c.Set(wx + i, by - 12 + j, i < 3 ? WoodRim : j == 5 ? WoodD : i < 12 ? WoodM : WoodD);
        c.HL(wx, by - 12, 19, C("7A5A48")); c.Set(wx, by - 12, Rim);
        foreach (var (px, py, r) in new[] { (wx + 5, by - 13, 3), (wx + 11, by - 14, 3), (wx + 15, by - 12, 2) })
        {
            c.Disc(px, py, r + 0.3, C("A8602E")); c.Set(px - 1, py - 1, C("F2B36A")); c.Set(px - 2, py, C("D98F4B")); c.Set(px - 1, py, C("D98F4B")); c.Set(px, py - r, C("3A5A38"));
        }
        c.Disc(wx + 17, by - 3, 3.2, C("1C1618")); c.Disc(wx + 17, by - 3, 1.2, WoodM); c.Set(wx + 15, by - 4, Rim); c.Set(wx + 15, by - 5, WoodRim);
        c.Line(wx + 1, by - 7, wx - 8, by - 3, WoodM); c.Line(wx + 1, by - 8, wx - 8, by - 4, WoodRim);
        c.VL(wx + 3, by - 7, 6, WoodD); c.VL(wx + 2, by - 7, 6, WoodM);
        // watering can by the well
        int cx = 72, cy = 190;
        c.Rect(cx, cy - 6, 7, 6, C("2E5252")); c.VL(cx, cy - 6, 6, C("78BAB0")); c.HL(cx, cy - 6, 7, C("5A9A94")); c.HL(cx, cy - 1, 7, C("1C3434")); c.VL(cx + 6, cy - 6, 6, C("1C3434"));
        c.Line(cx - 1, cy - 4, cx - 5, cy - 8, C("2E5252")); c.Set(cx - 6, cy - 9, C("78BAB0")); c.Set(cx - 5, cy - 9, C("5A9A94"));
        for (int i = 0; i < 5; i++) { c.Set(cx + 7 + (i < 3 ? 0 : 1), cy - 7 + i / 2 - (i == 0 ? 0 : 0), C("1C3434")); }
        c.Set(cx + 2, cy - 7, C("1C3434")); c.Set(cx + 3, cy - 8, C("1C3434")); c.Set(cx + 4, cy - 7, C("1C3434"));
        // bench under the fruit tree
        int bx = 239, bb = 186;
        c.Rect(bx, bb - 7, 17, 2, WoodM); c.HL(bx, bb - 7, 17, WoodRim); c.HL(bx, bb - 5, 17, WoodD);
        c.Rect(bx + 1, bb - 5, 2, 5, WoodD); c.Rect(bx + 14, bb - 5, 2, 5, WoodD); c.VL(bx + 1, bb - 5, 5, WoodM);
        c.Rect(bx, bb - 12, 17, 1, WoodM); c.HL(bx, bb - 13, 17, WoodRim); c.VL(bx + 1, bb - 12, 5, WoodD); c.VL(bx + 15, bb - 12, 5, WoodD);
    }

    // ------------------------------------------------------------------ flowers & foreground tufts
    static void Flowers()
    {
        var rnd = new Random(301);
        var cols = new[] { Cream, C("D4849A"), Gold, C("9F8CC4"), C("E9A878"), C("F6C6CF") };
        void Tuft(int x, int y)
        {
            var col = cols[rnd.Next(cols.Length)];
            int h = 2 + rnd.Next(3);
            for (int j = 0; j < h; j++) c.Set(x, y - j, j == 0 ? C("23382B") : C("2F4E3A"));
            c.Set(x - 1, y - 1, C("37573F")); c.Set(x + 1, y - 2, C("2F4E3A"));
            c.Set(x, y - h, col); if (rnd.NextDouble() < 0.55) c.Set(x - 1, y - h, Mix(col, Cream, 0.3)); if (rnd.NextDouble() < 0.3) c.Set(x + 1, y - h + 1, col);
        }
        // along the base of the fence, around the well, beds and near the gate
        for (int x = 260; x < 396; x += 3 + rnd.Next(4)) { if (x > 310 && x < 340) continue; Tuft(x, 191 + rnd.Next(3)); }
        for (int i = 0; i < 9; i++) Tuft(30 + rnd.Next(44), 181 + rnd.Next(10));
        for (int i = 0; i < 12; i++) Tuft(78 + rnd.Next(130), 190 + rnd.Next(14));
        for (int i = 0; i < 10; i++) Tuft(214 + rnd.Next(44), 192 + rnd.Next(12));
        for (int i = 0; i < 14; i++) Tuft(300 + rnd.Next(120), 196 + rnd.Next(12));
        for (int i = 0; i < 8; i++) Tuft(10 + rnd.Next(60), 195 + rnd.Next(14));
        for (int i = 0; i < 6; i++) Tuft(400 + rnd.Next(40), 175 + rnd.Next(30));
    }

    static void Tufts() { }

    // ------------------------------------------------------------------ foreground grass strip (448x32)
    public static Canvas ForegroundGrass()
    {
        var g = new Canvas(448, 32);
        var rnd = new Random(555);
        var back = C("27402F"); var mid = C("1D3328"); var front = C("131F1B");
        void Blade(int x, int h, int lean, Rgba body, Rgba lit, Rgba? tip, int wBase)
        {
            for (int j = 0; j < h; j++)
            {
                double t = j / (double)h;
                int ox = (int)Math.Round(lean * Math.Pow(t, 1.7));
                int y = 31 - j;
                int w = t < 0.35 ? wBase : 1;
                for (int i = 0; i < w; i++) g.Set(x + ox + i, y, body);
                if (t > 0.3 && t < 0.85 && (j & 1) == 0) g.Set(x + ox - 1, y, body); // slight blade swell
                if (t > 0.2 && lit.A != 0 && Fx.R(x, j, 561) < 0.6) g.Set(x + ox, y, lit);
                if (tip.HasValue && t > 0.82) g.Set(x + ox, y, tip.Value);
            }
        }
        for (int x = 0; x < 448; x += 1 + rnd.Next(2))
            Blade(x, 5 + rnd.Next(13), rnd.Next(-3, 4), back, C("5E7E4A"), rnd.NextDouble() < 0.2 ? C("C8A060") : null, 2);
        // tall wildflowers between the layers
        var heads = new[] { Cream, C("D4849A"), Gold, C("9F8CC4"), C("F6C6CF") };
        for (int i = 0; i < 22; i++)
        {
            int x = 6 + rnd.Next(436), h = 13 + rnd.Next(14), lean = rnd.Next(-2, 3);
            for (int j = 0; j < h; j++) g.Set(x + (int)Math.Round(lean * Math.Pow(j / (double)h, 1.5)), 31 - j, j > 3 ? C("2B4A34") : mid);
            int hx = x + lean, hy = 31 - h;
            var col = heads[i % heads.Length];
            g.Set(hx, hy, Gold); g.Set(hx - 1, hy, col); g.Set(hx + 1, hy, col); g.Set(hx, hy - 1, col); g.Set(hx, hy + 1, Mix(col, C("B08A60"), 0.4));
            if (i % 3 == 0) { g.Set(hx - 1, hy - 1, Mix(col, Cream, 0.5)); g.Set(hx + 1, hy + 1, col); }
        }
        for (int x = 0; x < 448; x += 1 + rnd.Next(3))
            Blade(x, 4 + rnd.Next(11), rnd.Next(-3, 4), mid, C("3E6240"), rnd.NextDouble() < 0.14 ? C("D98F4B") : null, 2);
        for (int x = 0; x < 448; x += 2 + rnd.Next(3))
            Blade(x, 3 + rnd.Next(8), rnd.Next(-2, 3), front, Rgba.Clear, rnd.NextDouble() < 0.1 ? C("B8864A") : null, 2);
        // solid soil/grass base so no gap shows through
        for (int x = 0; x < 448; x++) for (int y = 28; y < 32; y++) if (!g.Opaque(x, y) || y > 29) g.Set(x, y, y > 29 ? front : mid);
        return g;
    }

    // ------------------------------------------------------------------ additive glow overlay (same coordinates as the ground)
    public static Canvas WindowGlow()
    {
        var g = new Canvas(W, H);
        void Halo(Light l)
        {
            for (int y = l.Y - l.R - 1; y <= l.Y + l.H + l.R; y++)
                for (int x = l.X - l.R - 1; x <= l.X + l.W + l.R; x++)
                {
                    double dx = Math.Max(0, Math.Max(l.X - x, x - (l.X + l.W - 1))), dy = Math.Max(0, Math.Max(l.Y - y, y - (l.Y + l.H - 1)));
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    if (d <= 0 || d > l.R) continue;
                    double v = d / l.R;
                    double dens = Math.Floor(Math.Pow(1 - v, 1.6) * 0.95 * 8 + 0.5) / 8.0;
                    if (dens <= Bay8(x, y)) continue;
                    g.Set(x, y, v < 0.34 ? C("F2D26B") : v < 0.7 ? C("E9B050") : C("D98F4B"));
                }
        }
        foreach (var l in Lights) Halo(l);
        // light cast on the ground and fence in front of the open door
        for (int y = 179; y < 203; y++)
            for (int x = 296; x < 352; x++)
            {
                double hw = 5 + (y - 179) * 1.05, dx = Math.Abs(x - 323.5) / hw;
                if (dx >= 1) continue;
                double f = (1 - dx) * Math.Pow(1 - (y - 179) / 24.0, 1.3) * 0.8;
                f = Math.Floor(f * 7) / 7.0;
                if (f > Bay8(x, y)) g.Set(x, y, f > 0.45 ? C("E9B050") : C("D98F4B"));
            }
        // the far village windows (hills_far coordinates; aligned when both layers rest at the same offset)
        foreach (var (wx, wy) in MenuHills.FarWindows) Halo(new Light(wx, wy, 1, 1, 2));
        return g;
    }
}
