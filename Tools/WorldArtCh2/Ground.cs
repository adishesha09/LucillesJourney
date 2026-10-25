namespace WorldArtCh2;

public enum Mat : byte { Grass, Dirt, Cobble, Slab, Wall, Forest, Stone, Pave }

/// <summary>Fixed world geometry (image coordinates: origin top-left, y down; 16 px per unit).</summary>
public static class Geo
{
    public const int SqX0 = 96, SqX1 = 704, SqY0 = 112, SqY1 = 510;      // the cobbled square
    public const int NX0 = 352, NX1 = 448;                              // north road mouth
    public const int SX0 = 372, SX1 = 428;                              // south entrance
    public const int StrY0 = 198, StrY1 = 254;                          // side streets (road surface)
    public const int WellX = 400, WellY = 336;
    public const int WallY0 = 510, WallY1 = 526;                        // south wall
}

public static partial class Ground
{
    public static Canvas Cv = new(G.W, G.H);
    public static Mat[,] M = new Mat[G.W, G.H];
    static bool[,] forest = new bool[G.W, G.H];
    static double[,] wear = new double[G.W, G.H];
    static double[,] shade = new double[G.W, G.H];

    static bool Ell(double x, double y, double cx, double cy, double rx, double ry, double nz = 0)
    {
        double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry;
        return dx * dx + dy * dy + nz * 0.03 < 1;
    }

    public static void Build()
    {
        for (int y = 0; y < G.H; y++) for (int x = 0; x < G.W; x++) M[x, y] = Mat.Grass;
        MaskRoads(); MaskSquare(); MaskWear(); MaskForest();
        RenderGrass(); RenderDirt(); RenderCobble(); RenderSlab();
        Decorate();
        PaintBuildings();
        PaintWalls();
        RenderForest();
        ApplyLight();
    }

    // ---- masks ----------------------------------------------------------------------------------------------
    static bool InRoad(int x, int y, double nz)
    {
        if (x >= Geo.NX0 + nz && x < Geo.NX1 - nz && y < Geo.SqY0 + 2) return true;
        if (y >= Geo.StrY0 + nz && y < Geo.StrY1 - nz && (x < Geo.SqX0 + 2 || x >= Geo.SqX1 - 2)) return true;
        if (x >= Geo.SX0 + nz && x < Geo.SX1 - nz && y >= Geo.SqY1 - 2) return true;
        return false;
    }

    static void MaskRoads()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double nz = (Fx.Fbm(x / 10.0, y / 10.0, 41) - 0.5) * 6;
                if (InRoad(x, y, nz)) M[x, y] = Mat.Dirt;
            }
    }

    static void MaskSquare()
    {
        for (int y = Geo.SqY0; y < Geo.SqY1; y++)
            for (int x = Geo.SqX0; x < Geo.SqX1; x++) M[x, y] = Mat.Cobble;
        // pavement along the shopfronts and kerbs of the side streets
        for (int y = Geo.SqY0; y < Geo.SqY0 + 10; y++)
            for (int x = Geo.SqX0; x < Geo.SqX1; x++) if (!(x >= Geo.NX0 && x < Geo.NX1)) M[x, y] = Mat.Slab;
        foreach (var (ya, yb) in new[] { (190, Geo.StrY0), (Geo.StrY1, 262) })
            for (int y = ya; y < yb; y++) for (int x = 0; x < Geo.SqX0 + 8; x++) { M[x, y] = Mat.Slab; M[G.W - 1 - x, y] = Mat.Slab; }
        // dirt aprons where the roads fan out into the square
        for (int y = 100; y < 530; y++)
            for (int x = 80; x < 720; x++)
            {
                if (M[x, y] != Mat.Cobble && M[x, y] != Mat.Slab) continue;
                double nz = (Fx.R(x, y, 6) - 0.5) * 8;
                if (Ell(x, y, 400, Geo.SqY0 + 2, 60, 15, nz) || Ell(x, y, 400, Geo.SqY1 - 2, 40, 16, nz) ||
                    Ell(x, y, Geo.SqX0 + 2, 226, 15, 38, nz) || Ell(x, y, Geo.SqX1 - 2, 226, 15, 38, nz)) M[x, y] = Mat.Dirt;
            }
        // a slab avenue from the gate to the well and on to the north road
        for (int y = 132; y < 292; y++) for (int x = 378; x < 422; x++) if (M[x, y] == Mat.Cobble) M[x, y] = Mat.Slab;
        for (int y = 398; y < 492; y++) for (int x = 382; x < 418; x++) if (M[x, y] == Mat.Cobble) M[x, y] = Mat.Slab;
        // plaza around the well: pavers laid in rings
        for (int y = Geo.WellY - 48; y <= Geo.WellY + 48; y++)
            for (int x = Geo.WellX - 60; x <= Geo.WellX + 60; x++)
                if (Ell(x, y, Geo.WellX, Geo.WellY + 4, 56, 40)) M[x, y] = Mat.Pave;
    }

    static void MaskWear()
    {
        // trampled earth in front of the stalls and along the main axis; the south-east corner is neglected
        foreach (var (sx, sy) in Plan.StallBases)
            for (int y = sy - 4; y < sy + 26; y++)
                for (int x = sx - 44; x < sx + 44; x++)
                {
                    if (x < 0 || y < 0 || x >= G.W || y >= G.H) continue;
                    double d = Math.Pow((x - sx) / 40.0, 2) + Math.Pow((y - (sy + 10)) / 13.0, 2);
                    if (d < 1) wear[x, y] = Math.Max(wear[x, y], 1.0 - d);
                }
        for (int y = Geo.SqY0; y < Geo.SqY1; y++)
            for (int x = Geo.SqX0; x < Geo.SqX1; x++)
            {
                double axis = Math.Max(0, 1 - Math.Abs(x - 400 - Math.Sin(y / 40.0) * 6) / 34.0);
                double neglect = Math.Max(0, Math.Min((x - 540) / 120.0, (y - 380) / 100.0));
                double n = Fx.Fbm(x / 15.0, y / 15.0, 301);
                wear[x, y] = Math.Max(wear[x, y], Math.Clamp((n - 0.5) * 2.4 * (0.4 + axis * 0.9 + neglect * 1.4), 0, 1));
            }
    }

    static void MaskForest()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double nN = y - (12 + (Fx.Fbm(x / 16.0, 3, 91) - 0.45) * 16);
                double nS = (G.H - 1 - y) - (22 + (Fx.Fbm(x / 16.0, 9, 94) - 0.45) * 12);
                bool f = nN < 0 || nS < 0;
                if (y < 84 + (Fx.Fbm(x / 14.0, 2, 95) - 0.45) * 14 && (x < Geo.SqX0 || x >= Geo.SqX1)) f = true;
                if (y > 352 && x < 40 + (Fx.Fbm(5, y / 16.0, 92) - 0.45) * 18) f = true;
                if (y > 352 && x > G.W - 40 - (Fx.Fbm(8, y / 16.0, 93) - 0.45) * 18) f = true;
                if (M[x, y] == Mat.Dirt) f = false;
                forest[x, y] = f;
                if (f) M[x, y] = Mat.Forest;
            }
    }

    // ---- grass -------------------------------------------------------------------------------------------------
    static int GrassIdx(int x, int y)
    {
        double a = Fx.Fbm(x / 24.0, y / 24.0, 11), b = Fx.VN(x / 7.0, y / 5.0, 23), c = Fx.VN(x / 3.5, y / 3.0, 29);
        double v = a * 0.72 + b * 0.2 + c * 0.08 + Fx.Bay(x, y) * 0.035;
        return v < 0.30 ? 1 : v < 0.40 ? 2 : v < 0.53 ? 3 : v < 0.63 ? 4 : v < 0.73 ? 5 : 6;
    }

    static void RenderGrass()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (M[x, y] == Mat.Grass) Cv.Set(x, y, K.Grass[GrassIdx(x, y)]);
    }

    // ---- dirt ---------------------------------------------------------------------------------------------------
    static int EdgeDist(int x, int y, int max)
    {
        bool Inside(int xx, int yy) => xx < 0 || yy < 0 || xx >= G.W || yy >= G.H || M[xx, yy] == Mat.Dirt;
        for (int d = 1; d <= max; d++)
            if (!Inside(x - d, y) || !Inside(x + d, y) || !Inside(x, y - d) || !Inside(x, y + d)) return d - 1;
        return max;
    }

    static void RenderDirt()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Dirt) continue;
                int d = EdgeDist(x, y, 3);
                double a = Fx.Fbm(x / 13.0, y / 13.0, 61), b = Fx.VN(x / 4.0, y / 3.0, 67);
                double v = a * 0.8 + b * 0.2 + Fx.Bay(x, y) * 0.06;
                int idx = v < 0.36 ? 1 : v < 0.46 ? 2 : v < 0.60 ? 3 : v < 0.70 ? 4 : 5;
                if (d == 0) idx = Math.Max(0, idx - 2);
                else if (d == 1) idx = Math.Max(0, idx - 1);
                else if (d == 2 && ((x + y) & 1) == 0) idx = Math.Max(0, idx - 1);
                // cart-wheel ruts
                bool rutN = y < Geo.SqY0 && (Math.Abs(x - (386 + Math.Sin(y / 25.0) * 3)) < 1.2 || Math.Abs(x - (414 + Math.Sin(y / 25.0 + 1) * 3)) < 1.2);
                bool rutS = y >= Geo.SqY1 && (Math.Abs(x - (390 + Math.Sin(y / 22.0) * 2)) < 1.2 || Math.Abs(x - (410 + Math.Sin(y / 22.0 + 2) * 2)) < 1.2);
                bool rutW = (x < Geo.SqX0 || x >= Geo.SqX1) && (Math.Abs(y - 216) < 1.1 || Math.Abs(y - 236) < 1.1);
                if ((rutN || rutS || rutW) && idx > 0 && Fx.R(x, y, 62) > 0.12) idx--;
                Cv.Set(x, y, K.Dirt[idx]);
            }
        // pebbles
        for (int gy = 0; gy < G.H; gy += 9)
            for (int gx = 0; gx < G.W; gx += 9)
            {
                int x = gx + (int)(Fx.R(gx, gy, 71) * 8), y = gy + (int)(Fx.R(gx, gy, 72) * 8);
                if (Fx.R(gx, gy, 73) > 0.42) continue;
                if (x < 1 || y < 1 || x >= G.W - 4 || y >= G.H - 3) continue;
                if (!(M[x, y] == Mat.Dirt && M[x + 2, y + 1] == Mat.Dirt && M[x - 1, y - 1] == Mat.Dirt && M[x + 3, y + 2] == Mat.Dirt)) continue;
                int t = (int)(Fx.R(gx, gy, 74) * 3);
                var hi = new[] { Rgba.Hex("E3DAC4"), Rgba.Hex("D6CCB4"), Rgba.Hex("CDB99A") }[t];
                Cv.Set(x, y, hi); Cv.Set(x + 1, y, hi);
                if (t != 2) Cv.Set(x + 1, y + 1, K.Dirt[0]);
                Cv.Set(x, y + 1, K.Dirt[1]); Cv.Set(x + 2, y + 1, K.Dirt[1]);
            }
        // grass fringe on road rims
        for (int y = 1; y < G.H - 1; y++)
            for (int x = 1; x < G.W - 1; x++)
            {
                if (M[x, y] != Mat.Grass) continue;
                bool nearDirt = M[x - 1, y] == Mat.Dirt || M[x + 1, y] == Mat.Dirt || M[x, y + 1] == Mat.Dirt;
                if (nearDirt && Fx.R(x, y, 77) < 0.5) Cv.Set(x, y, K.Grass[Math.Max(0, GrassIdx(x, y) - 2)]);
            }
    }

    // ---- cobbles & slabs -------------------------------------------------------------------------------------------
    static readonly Rgba Mortar = Rgba.Hex("8C8070");

    static Rgba StonePx(int lx, int ly, int w, int h, uint hash, bool slab, double tint)
    {
        if (lx == 0 || ly == 0) return Mortar;
        bool corner = (lx == 1 || lx == w - 1) && (ly == 1 || ly == h - 1);
        if (corner) return Mortar;
        int baseIdx = slab ? 3 + (int)(hash % 2) : 2 + (int)(hash % 3);
        int idx = baseIdx;
        if (ly == 1 || lx == 1) idx++;
        if (ly == h - 1 || lx == w - 1) idx--;
        int sx = 2 + (int)((hash >> 4) % (uint)Math.Max(1, w - 4)), sy = 2 + (int)((hash >> 9) % (uint)Math.Max(1, h - 3));
        if ((hash >> 14 & 3) == 0 && ((lx == sx && ly == sy) || (lx == sx + 1 && ly == sy + 1))) idx--;
        if ((hash >> 18 & 3) == 0 && lx == sx + 1 && ly == sy) idx++;
        var c = K.Stone[Math.Clamp(idx, 0, K.Stone.Length - 1)];
        c = Rgba.Mul(c, 1.03 * tint, 1.0 * tint, 0.93 * tint);
        uint v = (hash >> 22) & 7;
        if (v == 0) c = Rgba.Mul(c, 1.05, 0.97, 0.86);
        else if (v == 1) c = Rgba.Mul(c, 0.94, 0.97, 1.03);
        else if (v == 2) c = Rgba.Mul(c, 0.92, 0.93, 0.96);
        return c;
    }

    public static Rgba CobblePx(int x, int y, bool slab, int seed = 3)
    {
        int rowH = 7;
        int yy = Math.Max(0, y + (int)Math.Round((Fx.VN(x / 37.0, y / 61.0, 7 + seed) - 0.5) * 7));   // rows wander a little
        int ry = yy / rowH, ly = yy % rowH;
        int off = (int)(Fx.R(ry, 0, seed) * 13);
        int sx = -off, id = 0;
        double tint = 0.93 + Fx.Fbm(x / 40.0, y / 40.0, 17) * 0.17;
        while (true)
        {
            int w = (slab ? 13 : 9) + (int)(Fx.R(ry, id, seed + 1) * (slab ? 12 : 6));
            if (x < sx + w) return StonePx(x - sx, ly, w, rowH, Fx.H(ry, id, seed + 2), slab, tint);
            sx += w; id++;
        }
    }

    /// <summary>Plaza pavers laid in concentric rings around the well, outermost ring an inlay of terracotta stones.</summary>
    static Rgba PavePx(int x, int y)
    {
        double dx = x + .5 - Geo.WellX, dy = (y + .5 - (Geo.WellY + 4)) / 0.72;
        double r = Math.Sqrt(dx * dx + dy * dy);
        int ri = (int)(r / 7.0); double lr = r - ri * 7.0;
        double a = (Math.Atan2(dy, dx) + Math.PI) / (2 * Math.PI);
        double circ = 2 * Math.PI * (ri * 7 + 3.5);
        int n = Math.Max(5, (int)Math.Round(circ / 11.0));
        double fa = a * n; int si = (int)fa; double la = (fa - si) * (circ / n);
        uint h = Fx.H(ri, si, 77);
        if (lr < 1.0 || la < 1.0) return Mortar;
        Rgba c;
        if (ri == 7)
        {
            c = K.Roof[1 + (int)(h % 3 == 0 ? 2 : h % 3 == 1 ? 1 : 0)];
            if (lr < 2.0) c = Rgba.Mul(c, 1.1, 1.08, 1.05);
            if (lr > 5.5) c = Rgba.Mul(c, 0.85, 0.82, 0.86);
            return Rgba.Mul(c, 0.96, 0.94, 0.94);
        }
        int idx = 3 + (int)(h % 2);
        if (lr < 2.2 || la < 2.2) idx++;
        if (lr > 5.3) idx--;
        c = K.Stone[Math.Clamp(idx, 0, 5)];
        double tint = 0.97 + Fx.Fbm(x / 30.0, y / 30.0, 19) * 0.1;
        return Rgba.Mul(c, 1.05 * tint, 1.0 * tint, 0.92 * tint);
    }

    static void RenderCobble()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                var m = M[x, y];
                if (m != Mat.Cobble && m != Mat.Pave) continue;
                var c = m == Mat.Pave ? PavePx(x, y) : CobblePx(x, y, false, 3);
                double w = wear[x, y];
                if (m == Mat.Cobble && w > 0.5)
                {
                    // packed earth showing through worn cobbles
                    double r = Fx.R(x, y, 401);
                    bool mortarPx = c == Mortar;
                    if (w > 0.78 && r < (w - 0.7) * 2.6) c = K.Dirt[2 + (int)(Fx.R(x / 2, y / 2, 402) * 2)];
                    else if (mortarPx && r < w * 0.9) c = K.Dirt[1 + (int)(r * 3) % 2];
                    else if (!mortarPx && ((x + y) & 1) == 0 && r < (w - 0.5) * 1.6) c = Rgba.Mul(c, 1.04, 1.0, 0.9);
                }
                Cv.Set(x, y, c);
            }
        // soft dark rim where the plaza meets the cobbles
        for (int y = Geo.WellY - 52; y <= Geo.WellY + 52; y++)
            for (int x = Geo.WellX - 64; x <= Geo.WellX + 64; x++)
            {
                if (x < 1 || y < 1 || x >= G.W - 1 || y >= G.H - 1 || M[x, y] != Mat.Cobble) continue;
                if (M[x - 1, y] == Mat.Pave || M[x + 1, y] == Mat.Pave || M[x, y - 1] == Mat.Pave || M[x, y + 1] == Mat.Pave) Cv.Set(x, y, K.Stone[1]);
            }
        // sparse weeds pushing between the cobbles in the neglected corner
        for (int y = 380; y < Geo.SqY1; y++)
            for (int x = 540; x < Geo.SqX1; x++)
            {
                if (M[x, y] != Mat.Cobble || Cv.Get(x, y) != Mortar) continue;
                double chance = Math.Min((x - 540) / 140.0, (y - 380) / 80.0);
                if (Fx.R(x, y, 403) < 0.17 * chance)
                {
                    Cv.Set(x, y, K.Grass[2 + (int)(Fx.R(x, y, 404) * 3)]);
                    if (Fx.R(x, y, 405) < 0.5) Cv.Set(x, y - 1, K.Grass[4]);
                }
            }
    }

    static void RenderSlab()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (M[x, y] == Mat.Slab) Cv.Set(x, y, CobblePx(x, y, true, 13));
        for (int y = 1; y < G.H - 1; y++)
            for (int x = 1; x < G.W - 1; x++)
            {
                if (M[x, y] != Mat.Slab) continue;
                if (M[x, y + 1] == Mat.Cobble) { Cv.Set(x, y, Rgba.Mul(K.Stone[4], 1.05, 1.0, 0.9)); Cv.Set(x, y + 1, K.Stone[1]); }
                else if (M[x - 1, y] == Mat.Cobble || M[x + 1, y] == Mat.Cobble) Cv.Set(x, y, K.Stone[1]);
                else if (M[x, y - 1] == Mat.Cobble) Cv.Set(x, y, Rgba.Mul(K.Stone[5], 1.04, 1.0, 0.92));
            }
    }

    // ---- decoration on grass --------------------------------------------------------------------------------------------
    static readonly (int dx, int dy, int tone)[][] Tufts =
    {
        new[] { (-2, -2, 2), (2, -2, 2), (-1, -1, 1), (1, -1, 1), (-1, 0, -1), (0, 0, -2), (1, 0, -1), (0, -1, -1) },
        new[] { (0, -3, 2), (0, -2, 1), (1, -2, 2), (0, -1, 0), (-1, -1, 1), (-1, 0, -1), (0, 0, -2), (1, 0, -1), (1, -1, -1) },
        new[] { (-1, -1, 1), (1, -1, 2), (0, -1, 0), (-1, 0, -1), (0, 0, -2), (1, 0, -1) },
    };

    static bool GrassArea(int x, int y, int r) =>
        x - r >= 0 && y - r >= 0 && x + r < G.W && y + r < G.H && M[x, y] == Mat.Grass && M[x - r, y] == Mat.Grass && M[x + r, y] == Mat.Grass && M[x, y - r] == Mat.Grass && M[x, y + r] == Mat.Grass;

    static void Decorate()
    {
        var flowerCols = new[] { K.Cream, K.Gold, Rgba.Hex("EAA5B5"), Rgba.Hex("B8D2DC"), Rgba.Hex("F4B07A") };
        for (int gy = 0; gy < G.H; gy += 7)
            for (int gx = 0; gx < G.W; gx += 7)
            {
                int x = gx + (int)(Fx.R(gx, gy, 1) * 7), y = gy + (int)(Fx.R(gx, gy, 2) * 7);
                if (!GrassArea(x, y, 3) || Fx.R(gx, gy, 3) > 0.38) continue;
                int bi = GrassIdx(x, y);
                var tuft = Tufts[(int)(Fx.H(gx, gy, 4) % (uint)Tufts.Length)];
                foreach (var (dx, dy, tone) in tuft) Cv.Set(x + dx, y + dy, K.Grass[Math.Clamp(bi + tone, 0, 6)]);
            }
        for (int gy = 0; gy < G.H; gy += 4)
            for (int gx = 0; gx < G.W; gx += 4)
            {
                double m = Fx.Fbm(gx / 30.0, gy / 30.0, 201);
                if (m < 0.64) continue;
                int x = gx + (int)(Fx.R(gx, gy, 9) * 4), y = gy + (int)(Fx.R(gx, gy, 10) * 4);
                if (!GrassArea(x, y, 2) || Fx.R(gx, gy, 11) > 0.55) continue;
                int ci = (int)(Fx.VN(gx / 55.0, gy / 55.0, 202) * 5.99) % flowerCols.Length;
                var fc = flowerCols[ci];
                int bi = GrassIdx(x, y);
                Cv.Set(x, y + 1, K.Grass[Math.Max(0, bi - 2)]);
                Cv.Set(x, y, fc);
                if (Fx.R(gx, gy, 13) < 0.35) { Cv.Set(x - 1, y, fc); Cv.Set(x + 1, y, fc); Cv.Set(x, y - 1, fc); Cv.Set(x, y, ci == 0 ? K.Gold : K.Cream); }
            }
    }

    // ---- buildings & walls ---------------------------------------------------------------------------------------------------
    static void PaintBuildings()
    {
        foreach (var b in Plan.Blds)
        {
            Buildings.Paint(Cv, b);
            for (int y = b.Top; y < b.Base; y++) for (int x = b.X; x < b.X + b.W; x++) M[x, y] = Mat.Wall;
        }
    }

    static void StoneBlock(int x0, int y0, int w, int h, int seed)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
            {
                int ly = y - y0;
                Rgba c;
                if (ly < 4) c = K.Stone[ly == 0 ? 5 : 4];
                else
                {
                    int row = (ly - 4) / 4, r = (ly - 4) % 4;
                    int off = (row & 1) * 6; int bw = 12;
                    int lx = (x - x0 + off) % bw;
                    int tone = 3 - (int)(Fx.R((x - x0 + off) / bw, row, seed) * 2);
                    if (r == 0) tone++;
                    if (r == 3 || lx == 0) tone = 1;
                    if (ly > h - 3) tone = Math.Max(0, tone - 1);
                    c = K.Stone[Math.Clamp(tone, 0, 5)];
                }
                if (ly == 3) c = K.Stone[2];
                Cv.Set(x, y, Rgba.Mul(c, 1.04, 1.0, 0.92));
                M[x, y] = Mat.Wall;
            }
    }

    static void PaintWalls()
    {
        StoneBlock(Geo.SqX0 - 4, Geo.WallY0, Geo.SX0 - 14 - (Geo.SqX0 - 4), Geo.WallY1 - Geo.WallY0, 31);
        StoneBlock(Geo.SX1 + 14, Geo.WallY0, Geo.SqX1 + 4 - (Geo.SX1 + 14), Geo.WallY1 - Geo.WallY0, 33);
        StoneBlock(90, 360, 12, Geo.WallY1 - 360, 35);
        StoneBlock(G.W - 102, 360, 12, Geo.WallY1 - 360, 37);
        foreach (int px in new[] { Geo.SX0 - 14, Geo.SX1 })
        {
            StoneBlock(px, 494, 14, 36, 41);
            Cv.Rect(px - 1, 490, 16, 5, K.Stone[5]); Cv.HL(px - 1, 494, 16, K.Stone[2]); Cv.HL(px - 1, 490, 16, Rgba.Hex("F2EAD8"));
            Cv.Rect(px + 4, 484, 6, 6, K.Stone[4]); Cv.HL(px + 4, 484, 6, K.Stone[5]); Cv.VL(px + 9, 485, 5, K.Stone[2]);
            for (int y = 484; y < 530; y++) for (int x = px - 1; x < px + 15; x++) if (Cv.Get(x, y).A != 0) M[x, y] = Mat.Wall;
        }
    }

    // ---- forest edge (crowns) -----------------------------------------------------------------------------------------
    static void RenderForest()
    {
        const int cellW = 15, cellH = 13;
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Forest) continue;
                double bestD = 1e9, second = 1e9, bcx = 0, bcy = 0;
                int gx0 = x / cellW, gy0 = y / cellH;
                for (int gy = gy0 - 2; gy <= gy0 + 2; gy++)
                    for (int gx = gx0 - 2; gx <= gx0 + 2; gx++)
                    {
                        double cx = gx * cellW + ((gy & 1) * cellW / 2.0) + (Fx.R(gx, gy, 301) - 0.5) * 7;
                        double cy = gy * cellH + (Fx.R(gx, gy, 302) - 0.5) * 6;
                        double dx = x + .5 - cx, dy = (y + .5 - cy) * 1.15;
                        double d = Math.Sqrt(dx * dx + dy * dy);
                        if (d < bestD) { second = bestD; bestD = d; bcx = cx; bcy = cy; }
                        else if (d < second) second = d;
                    }
                double R = 9.5;
                double nx = (x + .5 - bcx) / R, ny = (y + .5 - bcy) / R;
                double light = -(nx * 0.6 + ny * 0.85);
                double t = 0.52 + 0.30 * light - 0.14 * Math.Min(1, bestD / R);
                double edge = Math.Min(Math.Min(x, G.W - 1 - x), Math.Min(y, G.H - 1 - y));
                t -= Math.Clamp((34 - edge) / 34.0, 0, 1) * 0.28;
                t += (Fx.R(x / 3, y / 2, 303) - 0.5) * 0.18 + Fx.Bay(x, y) * 0.09;
                if (bestD / Math.Max(0.1, second) > 0.80) t -= 0.18;
                int idx = Math.Clamp((int)(t * K.Leaf.Length), 0, K.Leaf.Length - 1);
                Cv.Set(x, y, K.Leaf[Math.Min(idx, 5)]);
            }
    }

    // ---- shadows & light --------------------------------------------------------------------------------------------------
    static bool Shadable(int x, int y) =>
        x >= 0 && y >= 0 && x < G.W && y < G.H && M[x, y] != Mat.Wall && M[x, y] != Mat.Forest;

    static void Mark(int x, int y, double k) { if (x >= 0 && y >= 0 && x < G.W && y < G.H && shade[x, y] < k) shade[x, y] = k; }

    static void MarkEll(double cx, double cy, double rx, double ry, double k)
    {
        for (int y = (int)(cy - ry - 2); y <= cy + ry + 2; y++)
            for (int x = (int)(cx - rx - 2); x <= cx + rx + 2; x++)
            {
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry, d = dx * dx + dy * dy;
                if (d <= 1.0) Mark(x, y, k);
                else if (d <= 1.3 && ((x + y) & 1) == 0) Mark(x, y, k * 0.6);
            }
    }

    static void MarkPoly(IReadOnlyList<(double x, double y)> pts, double k)
    {
        var tmp = new bool[G.W, G.H];
        int n = pts.Count;
        double minY = pts.Min(p => p.y), maxY = pts.Max(p => p.y), minX = pts.Min(p => p.x), maxX = pts.Max(p => p.x);
        for (int y = Math.Max(0, (int)minY - 1); y <= Math.Min(G.H - 1, (int)maxY + 1); y++)
            for (int x = Math.Max(0, (int)minX - 1); x <= Math.Min(G.W - 1, (int)maxX + 1); x++)
            {
                double qx = x + .5, qy = y + .5; bool inside = false;
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    var (xi, yi) = pts[i]; var (xj, yj) = pts[j];
                    if ((yi > qy) != (yj > qy) && qx < (xj - xi) * (qy - yi) / (yj - yi) + xi) inside = !inside;
                }
                tmp[x, y] = inside;
            }
        for (int y = Math.Max(1, (int)minY - 1); y <= Math.Min(G.H - 2, (int)maxY + 1); y++)
            for (int x = Math.Max(1, (int)minX - 1); x <= Math.Min(G.W - 2, (int)maxX + 1); x++)
            {
                if (tmp[x, y]) Mark(x, y, k);
                else if (((x + y) & 1) == 0 && (tmp[x - 1, y] || tmp[x + 1, y] || tmp[x, y - 1] || tmp[x, y + 1])) Mark(x, y, k * 0.6);
            }
    }

    /// <summary>Soft cast shadow of a box-like object: hull of its footprint and the footprint shifted along the light direction.</summary>
    public static void BoxShadow(double x0, double y0, double x1, double y1, double height, double k = 0.85)
    {
        double sx = height * 0.55, sy = height * 0.32;
        var pts = new List<(double x, double y)> { (x0, y1), (x0, y0), (x1, y0), (x1, y1), (x0 + sx, y0 + sy), (x1 + sx, y0 + sy), (x1 + sx, y1 + sy), (x0 + sx, y1 + sy) };
        Layout.ShadowPoly(ConvexHull(pts), k);
    }

    static List<(double x, double y)> ConvexHull(List<(double x, double y)> p)
    {
        var s = p.OrderBy(a => a.x).ThenBy(a => a.y).ToList();
        double Cross((double x, double y) o, (double x, double y) a, (double x, double y) b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        var h = new List<(double x, double y)>();
        foreach (var q in s) { while (h.Count >= 2 && Cross(h[^2], h[^1], q) <= 0) h.RemoveAt(h.Count - 1); h.Add(q); }
        int t = h.Count + 1;
        for (int i = s.Count - 2; i >= 0; i--) { var q = s[i]; while (h.Count >= t && Cross(h[^2], h[^1], q) <= 0) h.RemoveAt(h.Count - 1); h.Add(q); }
        h.RemoveAt(h.Count - 1);
        return h;
    }

    static void ApplyLight()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] == Mat.Forest || M[x, y] == Mat.Wall) continue;
                int hit = 0;
                for (int t = 1; t <= 6; t++)
                {
                    bool f = (y - t >= 0 && forest[x, y - t]) || (x - t >= 0 && forest[x - t, y]) || (x - t >= 0 && y - t >= 0 && forest[x - t, y - t]);
                    if (f) { hit = t; break; }
                }
                if (hit == 0) continue;
                if (hit <= 3 || ((x + y) & 1) == 0 && hit <= 5) Mark(x, y, hit <= 2 ? 1.0 : 0.7);
            }
        // shopfronts and houses cast long soft shadows south-east onto the pavement and the street mouths
        foreach (var b in Plan.Blds)
        {
            MarkPoly(new (double, double)[] { (b.X, b.Base), (b.X + b.W, b.Base), (b.X + b.W + 20, b.Base + 15), (b.X + 20, b.Base + 15) }, 0.66);
            MarkPoly(new (double, double)[] { (b.X + b.W, b.Top + 30), (b.X + b.W + 16, b.Top + 44), (b.X + b.W + 16, b.Base + 15), (b.X + b.W, b.Base) }, 0.66);
        }
        foreach (var s in Layout.Shadows)
        {
            if (s.Kind == 'e') MarkEll(s.A, s.B, s.C, s.D, s.K);
            else
                for (int y = (int)s.B - 1; y <= s.B + s.D; y++)
                    for (int x = (int)s.A - 1; x <= s.A + s.C; x++)
                    {
                        bool inside = x >= s.A && y >= s.B && x < s.A + s.C && y < s.B + s.D;
                        if (inside) Mark(x, y, s.K); else if (((x + y) & 1) == 0) Mark(x, y, s.K * 0.6);
                    }
        }
        foreach (var p in Layout.PolyShadows) MarkPoly(p.Pts, p.K);
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (shade[x, y] <= 0 || !Shadable(x, y)) continue;
                Cv.Set(x, y, Cv.Get(x, y).Shade(shade[x, y]));
            }
        // warm sunlit wash on the open square (soft bright patches)
        for (int y = Geo.SqY0 + 10; y < Geo.SqY1; y++)
            for (int x = Geo.SqX0; x < Geo.SqX1; x++)
            {
                if (shade[x, y] > 0 || !Shadable(x, y)) continue;
                double n = Fx.Fbm(x / 34.0, y / 26.0, 511);
                if (n > 0.66 && ((x + y) & 1) == 0) Cv.Set(x, y, Cv.Get(x, y).Light(0.7));
            }
    }
}
