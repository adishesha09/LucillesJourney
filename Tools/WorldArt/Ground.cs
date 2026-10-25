namespace WorldArt;

public enum Mat : byte { Grass, Dirt, Paver, Earth, Forest, Tile, Plank, PlankB, Wall, Soil, Stone, Thresh }

/// <summary>Fixed world geometry (image coordinates: origin top-left, y down; 16 px tiles).</summary>
public static class G
{
    public const int W = 704, H = 480;
    public const int HX0 = 48, HX1 = 432, HY0 = 48, HY1 = 320;
    public const int EaveY = 48, FaceY = 64, FloorY = 96, FloorBot = 296;
    public const int DoorX0 = 160, DoorX1 = 192;                       // front doorway (32 px)
    public const int DivX = 216;                                       // kitchen | living partition x..x+8
    public const int DivGY0 = 208, DivGY1 = 240;                       // doorway in partition
    public const int BedWX = 312;                                      // bedroom west partition x..x+8
    public const int BedWallY = 192;                                   // bedroom south wall y..y+32
    public const int BedGX0 = 320, BedGX1 = 352;                       // bedroom doorway
    public const int GardenX0 = 464, GardenX1 = 670, GardenY0 = 60, GardenY1 = 302;
    public const int GateX0 = 552, GateX1 = 584;                       // garden gate (south fence)
}

public static partial class Ground
{
    public static Canvas Cv = new(G.W, G.H);
    public static Mat[,] M = new Mat[G.W, G.H];
    static bool[,] forest = new bool[G.W, G.H];
    static bool[,] dirt = new bool[G.W, G.H];

    // ---- shapes -------------------------------------------------------------------------------------------
    static double SegDist(double px, double py, double ax, double ay, double bx, double by)
    {
        double dx = bx - ax, dy = by - ay, l2 = dx * dx + dy * dy;
        double t = l2 == 0 ? 0 : Math.Clamp(((px - ax) * dx + (py - ay) * dy) / l2, 0, 1);
        double qx = ax + t * dx - px, qy = ay + t * dy - py;
        return Math.Sqrt(qx * qx + qy * qy);
    }

    static readonly (double x, double y)[][] Paths =
    {
        // main path: step -> south gate
        new[] { (176.0, 326.0), (176.0, 372.0), (178.0, 420.0), (176.0, 470.0) },
        // east path: main path -> garden gate
        new[] { (178.0, 378.0), (250.0, 386.0), (330.0, 388.0), (420.0, 384.0), (500.0, 376.0), (562.0, 366.0), (568.0, 330.0), (568.0, 304.0) },
        // little spur to the well
        new[] { (176.0, 398.0), (140.0, 402.0), (112.0, 408.0) },
        // spur to the wood stack
        new[] { (176.0, 350.0), (130.0, 356.0), (92.0, 366.0) },
    };
    static readonly double[] PathW = { 30, 26, 12, 11 };

    public static readonly (int x, int y, int w, int h)[] PaverRects =
    {
        (140, 334, 72, 22),   // porch apron
    };
    public static readonly (int cx, int cy, int rx, int ry)[] PaverEllipses =
    {
        (100, 412, 30, 18),   // around the well
        (566, 322, 24, 12),   // in front of the garden gate
    };
    public static readonly (int x, int y, int w, int h)[] SoilRects =
    {
        (56, 324, 88, 14),    // flower bed west of the step
        (212, 324, 212, 14),  // flower bed east of the step
    };

    // ---- main ---------------------------------------------------------------------------------------------
    public static void Build()
    {
        // 1) materials
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++) M[x, y] = Mat.Grass;
        MaskForest(); MaskDirt(); MaskPavers(); MaskGarden(); MaskHouseFloors();
        // 2) per-material rendering
        RenderGrass(); RenderDirt(); RenderEarth(); RenderPavers(); RenderFloors(); RenderSoil();
        Decorate();
        StepStones();
        HouseArt.Paint();
        RenderForest();
        ApplyLight();
    }

    static void MaskForest()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double nN = y - (38 + (Fx.Fbm(x / 16.0, 3, 91) - 0.45) * 18);
                double nW = x - (36 + (Fx.Fbm(5, y / 16.0, 92) - 0.45) * 18);
                double nE = (G.W - 1 - x) - (22 + (Fx.Fbm(8, y / 16.0, 93) - 0.45) * 16);
                double nS = (G.H - 1 - y) - (26 + (Fx.Fbm(x / 16.0, 9, 94) - 0.45) * 16);
                bool f = nN < 0 || nW < 0 || nE < 0 || nS < 0;
                if (f && y > 400 && x > 150 && x < 202 && y > 440) f = false;   // opening for the south gate road
                forest[x, y] = f;
                if (f) M[x, y] = Mat.Forest;
            }
    }

    static void MaskDirt()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] == Mat.Forest) continue;
                double nz = (Fx.Fbm(x / 10.0, y / 10.0, 41) - 0.5) * 6;
                bool d = false;
                for (int i = 0; i < Paths.Length && !d; i++)
                {
                    var p = Paths[i];
                    for (int s = 0; s + 1 < p.Length; s++)
                    {
                        double wv = PathW[i] * 0.5 + nz;
                        if (SegDist(x + .5, y + .5, p[s].x, p[s].y, p[s + 1].x, p[s + 1].y) < wv) { d = true; break; }
                    }
                }
                // worn patches: by the well, under the washing line, in front of the stack
                if (!d) d = Ell(x, y, 100, 412, 36, 22, nz) || Ell(x, y, 120, 360, 40, 14, nz);
                if (d) { M[x, y] = Mat.Dirt; dirt[x, y] = true; }
            }
    }

    static bool Ell(int x, int y, double cx, double cy, double rx, double ry, double nz = 0)
    {
        double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry;
        return dx * dx + dy * dy + nz * 0.03 < 1;
    }

    static void MaskPavers()
    {
        foreach (var r in PaverRects)
            for (int y = r.y; y < r.y + r.h; y++)
                for (int x = r.x; x < r.x + r.w; x++)
                {
                    bool corner = (x < r.x + 2 || x >= r.x + r.w - 2) && (y < r.y + 2 || y >= r.y + r.h - 2);
                    if (!corner && M[x, y] != Mat.Forest) M[x, y] = Mat.Paver;
                }
        foreach (var e in PaverEllipses)
            for (int y = e.cy - e.ry; y <= e.cy + e.ry; y++)
                for (int x = e.cx - e.rx; x <= e.cx + e.rx; x++)
                    if (Ell(x, y, e.cx, e.cy, e.rx, e.ry, (Fx.R(x, y, 5) - 0.5) * 4)) M[x, y] = Mat.Paver;
        foreach (var r in SoilRects)
            for (int y = r.y; y < r.y + r.h; y++)
                for (int x = r.x; x < r.x + r.w; x++) M[x, y] = Mat.Soil;
        // porch step slab
        for (int y = 320; y < 334; y++) for (int x = 148; x < 206; x++) M[x, y] = Mat.Stone;
    }

    static void MaskGarden()
    {
        for (int y = G.GardenY0; y < G.GardenY1; y++)
            for (int x = G.GardenX0; x < G.GardenX1; x++) M[x, y] = Mat.Earth;
        // path continues through the gate
        for (int y = G.GardenY1 - 6; y < G.GardenY1 + 4; y++)
            for (int x = G.GateX0; x < G.GateX1; x++) M[x, y] = Mat.Earth;
    }

    static void MaskHouseFloors()
    {
        for (int y = G.FloorY; y < G.FloorBot; y++)
            for (int x = 64; x < 416; x++)
            {
                Mat m;
                if (x < G.DivX + 4) m = Mat.Tile;
                else if (x >= G.BedWX + 4 && y < G.BedWallY + 4) m = Mat.PlankB;
                else m = Mat.Plank;
                M[x, y] = m;
            }
        // gap floors of the bedroom doorway (plank), partition doorway (threshold)
        for (int y = G.BedWallY; y < G.BedWallY + 32; y++)
            for (int x = G.BedGX0; x < G.BedGX1; x++) M[x, y] = Mat.PlankB;
        for (int y = G.DivGY0; y < G.DivGY1; y++)
            for (int x = G.DivX; x < G.DivX + 8; x++) M[x, y] = Mat.Thresh;
        for (int y = 296; y < 320; y++)
            for (int x = G.DoorX0; x < G.DoorX1; x++) M[x, y] = Mat.Stone;
    }

    // ---- grass --------------------------------------------------------------------------------------------
    static int GrassIdx(int x, int y)
    {
        double a = Fx.Fbm(x / 24.0, y / 24.0, 11), b = Fx.VN(x / 7.0, y / 5.0, 23), c = Fx.VN(x / 3.5, y / 3.0, 29);
        double v = a * 0.72 + b * 0.2 + c * 0.08 + Fx.Bay(x, y) * 0.035;
        if (v < 0.30) return 1;
        if (v < 0.40) return 2;
        if (v < 0.53) return 3;
        if (v < 0.63) return 4;
        if (v < 0.73) return 5;
        return 6;
    }

    static void RenderGrass()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (M[x, y] == Mat.Grass) Cv.Set(x, y, K.Grass[GrassIdx(x, y)]);
    }

    // ---- dirt ---------------------------------------------------------------------------------------------
    static int EdgeDist(Func<int, int, bool> inside, int x, int y, int max)
    {
        for (int d = 1; d <= max; d++)
            if (!inside(x - d, y) || !inside(x + d, y) || !inside(x, y - d) || !inside(x, y + d)) return d - 1;
        return max;
    }

    static void RenderDirt()
    {
        bool In(int x, int y) => x >= 0 && y >= 0 && x < G.W && y < G.H && M[x, y] == Mat.Dirt;
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Dirt) continue;
                int d = EdgeDist(In, x, y, 3);
                double a = Fx.Fbm(x / 13.0, y / 13.0, 61), b = Fx.VN(x / 4.0, y / 3.0, 67);
                double v = a * 0.8 + b * 0.2 + Fx.Bay(x, y) * 0.06;
                int idx = v < 0.36 ? 1 : v < 0.46 ? 2 : v < 0.60 ? 3 : v < 0.70 ? 4 : 5;
                if (d == 0) idx = Math.Max(0, idx - 2) ;
                else if (d == 1) idx = Math.Max(0, idx - 1);
                else if (d == 2 && ((x + y) & 1) == 0) idx = Math.Max(0, idx - 1);
                // subtle cart-wheel wear lines inside wide paths
                Cv.Set(x, y, K.Dirt[idx]);
            }
        // pebbles on a jittered grid
        for (int gy = 0; gy < G.H; gy += 9)
            for (int gx = 0; gx < G.W; gx += 9)
            {
                int x = gx + (int)(Fx.R(gx, gy, 71) * 8), y = gy + (int)(Fx.R(gx, gy, 72) * 8);
                if (Fx.R(gx, gy, 73) > 0.42) continue;
                if (!(In(x, y) && In(x + 2, y + 1) && In(x - 1, y - 1) && In(x + 3, y + 2))) continue;
                int t = (int)(Fx.R(gx, gy, 74) * 3);
                var hi = new[] { Rgba.Hex("E3DAC4"), Rgba.Hex("D6CCB4"), Rgba.Hex("CDB99A") }[t];
                Cv.Set(x, y, hi); Cv.Set(x + 1, y, hi);
                if (t != 2) Cv.Set(x + 1, y + 1, K.Dirt[0]);
                Cv.Set(x, y + 1, K.Dirt[1]); Cv.Set(x + 2, y + 1, K.Dirt[1]);
            }
        // grass fringe on the dirt rim (taller blades)
        for (int y = 1; y < G.H - 1; y++)
            for (int x = 1; x < G.W - 1; x++)
            {
                if (M[x, y] != Mat.Grass) continue;
                bool nearDirt = In(x - 1, y) || In(x + 1, y) || In(x, y + 1);
                if (nearDirt && Fx.R(x, y, 77) < 0.5) Cv.Set(x, y, K.Grass[Math.Max(0, GrassIdx(x, y) - 2)]);
            }
    }

    // ---- garden earth -------------------------------------------------------------------------------------
    static void RenderEarth()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Earth) continue;
                double a = Fx.Fbm(x / 9.0, y / 14.0, 81), b = Fx.VN(x / 2.5, y / 6.0, 83);
                double v = a * 0.75 + b * 0.25 + Fx.Bay(x, y) * 0.07;
                int idx = v < 0.38 ? 0 : v < 0.52 ? 1 : v < 0.68 ? 2 : 3;
                var col = K.Dirt[idx];
                // warm brownish tilled tint
                col = Rgba.Mul(col, 0.92, 0.84, 0.80);
                // tiny straw/pebble accents on a grid
                if (x % 11 == (int)(Fx.R(x / 11, y / 7, 85) * 11) && y % 7 == (int)(Fx.R(x / 11, y / 7, 86) * 7) && Fx.R(x / 11, y / 7, 87) < 0.5)
                    col = Rgba.Hex("D9C48A");
                Cv.Set(x, y, col);
            }
    }

    // ---- cobble pavers ------------------------------------------------------------------------------------
    static void RenderPavers()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (M[x, y] == Mat.Paver || M[x, y] == Mat.Stone) Cv.Set(x, y, CobblePx(x, y, M[x, y] == Mat.Stone));
    }

    public static Rgba CobblePx(int x, int y, bool slab, int seed = 3)
    {
        int rowH = slab ? 7 : 8;
        int ry = y / rowH, ly = y % rowH;
        // stone boundaries along the row
        int off = (int)(Fx.R(ry, 0, seed) * 13);
        int sx = -off, id = 0;
        while (true)
        {
            int w = (slab ? 12 : 9) + (int)(Fx.R(ry, id, seed + 1) * (slab ? 12 : 6));
            if (x < sx + w) { return StonePx(x - sx, ly, w, rowH, Fx.H(ry, id, seed + 2), slab); }
            sx += w; id++;
        }
    }

    static Rgba StonePx(int lx, int ly, int w, int h, uint hash, bool slab)
    {
        if (lx == 0 || ly == 0) return K.Mortar;
        bool corner = (lx == 1 || lx == w - 1) && (ly == 1 || ly == h - 1);
        if (corner) return K.Mortar;
        int baseIdx = 2 + (int)(hash % 3); // 2..4
        if (slab) baseIdx = 3 + (int)(hash % 2);
        int idx = baseIdx;
        if (ly == 1 || lx == 1) idx++;
        if (ly == h - 1 || lx == w - 1) idx--;
        // a speck or hairline crack per stone
        int sx = 2 + (int)((hash >> 4) % (uint)Math.Max(1, w - 4)), sy = 2 + (int)((hash >> 9) % (uint)Math.Max(1, h - 3));
        if ((hash >> 14 & 3) == 0 && ((lx == sx && ly == sy) || (lx == sx + 1 && ly == sy + 1))) idx--;
        if ((hash >> 18 & 3) == 0 && lx == sx + 1 && ly == sy) idx++;
        var c = K.Stone[Math.Clamp(idx, 0, K.Stone.Length - 1)];
        return Rgba.Mul(c, 1.03, 0.98, 0.90); // warm the stones a little
    }

    // ---- floors -------------------------------------------------------------------------------------------
    static void RenderFloors()
    {
        for (int y = G.FloorY; y < G.FloorBot; y++)
            for (int x = 64; x < 416; x++)
                switch (M[x, y])
                {
                    case Mat.Tile: Cv.Set(x, y, TilePx(x, y)); break;
                    case Mat.Plank: Cv.Set(x, y, PlankPx(x, y, false)); break;
                    case Mat.PlankB: Cv.Set(x, y, PlankPx(y, x, true)); break;
                    case Mat.Thresh: Cv.Set(x, y, K.Wood[((x + y) % 8 == 0) ? 2 : 3]); break;
                }
        // gap floors
        for (int y = G.BedWallY; y < G.BedWallY + 32; y++)
            for (int x = G.BedGX0; x < G.BedGX1; x++) Cv.Set(x, y, PlankPx(y, x, true));
        // wooden thresholds
        for (int y = G.DivGY0; y < G.DivGY1; y++)
            for (int x = G.DivX; x < G.DivX + 8; x++)
            {
                var t = K.Wood[(x - G.DivX) is 0 ? 4 : (x - G.DivX) is 7 ? 1 : 3];
                if ((y == G.DivGY0 || y == G.DivGY1 - 1) ) t = K.Wood[1];
                Cv.Set(x, y, t);
            }
    }

    static Rgba TilePx(int x, int y)
    {
        int lx = (x - 64) % 16, ly = (y - G.FloorY) % 16;
        int tx = (x - 64) / 16, ty = (y - G.FloorY) / 16;
        bool alt = ((tx + ty) & 1) == 1;
        var grout = Rgba.Hex("BFA97E");
        if (lx == 0 || ly == 0)
        {
            // teal studs at the grout crossings
            if (lx <= 1 && ly <= 1 && !(lx == 1 && ly == 1)) return K.Teal;
            return grout;
        }
        var a = Rgba.Hex("F0E3C2"); var b = Rgba.Hex("E1CFA5");
        var c = alt ? b : a;
        if (ly == 1 || lx == 1) c = alt ? Rgba.Hex("EBDCB6") : Rgba.Hex("FBF3DC");
        else if (ly == 15 || lx == 15) c = alt ? Rgba.Hex("CDB78C") : Rgba.Hex("D9C7A0");
        else
        {
            // gentle diagonal wear on every other tile
            if (!alt && (lx + ly) == 18 && Fx.R(tx, ty, 17) > 0.4) c = Rgba.Hex("E8D9B5");
            if (alt && lx == 7 && ly == 8) c = Rgba.Hex("D2BD92");
        }
        return c;
    }

    /// <summary>Horizontal planks (u runs along the plank). Swap u/v for vertical planks.</summary>
    static Rgba PlankPx(int px, int py, bool light)
    {
        // px: along-plank coordinate, py: across-plank coordinate
        var ramp = light ? K.Pine : K.Wood;
        int bi = light ? 2 : 3;              // base tone index
        int ph = 8;
        int row = py / ph, ly = py % ph;
        int off = (int)(Fx.R(row, 1, 31) * 60);
        int len = 56;
        int along = px + off;
        int id = Math.DivRem(along, len, out int lx);
        uint h = Fx.H(row, id, 33);
        int tone = bi + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        if (ly == ph - 1) return ramp[Math.Max(0, bi - 2)];            // seam between rows
        if (lx == 0) return ramp[Math.Max(0, bi - 2)];                 // butt joint
        int idx = tone;
        if (ly == 0) idx = tone + 1;                                   // lit top edge
        else
        {
            // grain: broken dashes
            uint gh = Fx.H(row, id * 7 + ly, 35);
            int gs = (int)(gh % 40), gl = 8 + (int)((gh >> 8) % 14);
            if ((ly == 2 || ly == 4 || ly == 6) && ((gh >> 16) & 3) != 0 && lx >= gs && lx < gs + gl) idx = tone - 1;
            else if (ly == 3 && ((gh >> 20) & 7) == 0 && lx > gs && lx < gs + 3) idx = tone + 1;
        }
        // knot
        if ((h >> 4 & 7) == 0)
        {
            int kx = 12 + (int)((h >> 8) % 30), ky = 3 + (int)((h >> 14) % 2);
            int dx = lx - kx, dy = ly - ky;
            if (dx * dx / 4.0 + dy * dy / 1.4 <= 1.0) idx = tone - 2;
            else if (dx * dx / 9.0 + dy * dy / 3.0 <= 1.0) idx = tone - 1;
        }
        // soft highlight pools of light
        if (Fx.VN(px / 28.0, py / 22.0, 37) > 0.74 && idx == tone) idx = tone + 1;
        return ramp[Math.Clamp(idx, 0, ramp.Length - 1)];
    }

    // ---- flower-bed soil ----------------------------------------------------------------------------------
    static void RenderSoil()
    {
        foreach (var r in SoilRects)
            for (int y = r.y; y < r.y + r.h; y++)
                for (int x = r.x; x < r.x + r.w; x++)
                {
                    double a = Fx.VN(x / 5.0, y / 3.0, 97);
                    var c = a < 0.35 ? K.SoilDark : a < 0.7 ? K.Soil : K.SoilLight;
                    if (y == r.y + r.h - 1) c = K.SoilDark;
                    Cv.Set(x, y, c);
                }
        // stone edging along the south edge of each bed
        foreach (var r in SoilRects)
        {
            int y = r.y + r.h;
            for (int x = r.x; x < r.x + r.w; x++)
            {
                int k = (x - r.x) % 8, id = (x - r.x) / 8;
                int w = 8; bool rnd = k == 0 || k == 7;
                var col = K.Stone[3 + (int)(Fx.R(id, 0, 99) * 2)];
                Cv.Set(x, y, rnd ? Rgba.Hex("7A7066") : col);
                Cv.Set(x, y + 1, k == 7 ? K.Mortar : K.Stone[2]);
                M[x, y] = Mat.Stone; M[x, y + 1] = Mat.Stone;
            }
            // west/east end stones
        }
    }

    // ---- stepping stones & decorations --------------------------------------------------------------------
    public static readonly (int x, int y)[] Steppers =
    {
        // aisle in the vegetable garden
        (568, 108), (568, 130), (568, 152), (568, 174), (568, 196), (568, 218), (568, 240), (568, 262),
        // along the south walkway
        (520, 286), (544, 284),
        // from the main path to the well/stack
        (150, 392), (132, 398),
        // bench corner
        (300, 424), (318, 430),
    };

    static void StepStones()
    {
        foreach (var (sx, sy) in Steppers)
        {
            int w = 12 + (int)(Fx.R(sx, sy, 3) * 3), h = 8;
            double cx = sx, cy = sy;
            // drop shadow first (bottom-right)
            for (int y = sy - h / 2 - 1; y <= sy + h / 2 + 2; y++)
                for (int x = sx - w / 2 - 1; x <= sx + w / 2 + 2; x++)
                    if (M[x, y] != Mat.Forest && Ell(x, y, cx + 1.5, cy + 1.8, w / 2.0, h / 2.0) && !Ell(x, y, cx, cy, w / 2.0, h / 2.0))
                        Cv.Set(x, y, Cv.Get(x, y).Shade(0.9));
            for (int y = sy - h / 2; y < sy + h / 2; y++)
                for (int x = sx - w / 2; x < sx + w / 2; x++)
                {
                    if (!Ell(x, y, cx, cy - 0.3, w / 2.0, h / 2.0)) continue;
                    double nx = (x + .5 - cx) / (w / 2.0), ny = (y + .5 - cy) / (h / 2.0);
                    double t = 0.55 - 0.3 * nx - 0.35 * ny + (Fx.R(x / 2, y / 2, 5) - 0.5) * 0.18;
                    int idx = Math.Clamp((int)(t * 6) + 1, 1, 5);
                    if (ny > 0.55) idx = Math.Max(1, idx - 1);
                    Cv.Set(x, y, Rgba.Mul(K.Stone[idx], 1.03, 0.98, 0.9));
                }
        }
    }

    // ---- grass decoration ---------------------------------------------------------------------------------
    static readonly (int dx, int dy, int tone)[][] Tufts =
    {
        new[] { (-2, -2, 2), (2, -2, 2), (-1, -1, 1), (1, -1, 1), (-1, 0, -1), (0, 0, -2), (1, 0, -1), (0, -1, -1) },
        new[] { (0, -3, 2), (0, -2, 1), (1, -2, 2), (0, -1, 0), (-1, -1, 1), (-1, 0, -1), (0, 0, -2), (1, 0, -1), (1, -1, -1) },
        new[] { (-1, -1, 1), (1, -1, 2), (0, -1, 0), (-1, 0, -1), (0, 0, -2), (1, 0, -1) },
        new[] { (-3, -1, 1), (-2, -1, 0), (-2, 0, -1), (-1, 0, -2), (0, 0, -1), (0, -1, 1), (1, -2, 2), (1, -1, 1), (1, 0, -1), (2, 0, -1) },
    };

    static bool GrassArea(int x, int y, int r) =>
        x - r >= 0 && y - r >= 0 && x + r < G.W && y + r < G.H && M[x, y] == Mat.Grass && M[x - r, y] == Mat.Grass && M[x + r, y] == Mat.Grass && M[x, y - r] == Mat.Grass && M[x, y + r] == Mat.Grass;

    static void Decorate()
    {
        var flowerCols = new[] { K.Cream, K.Gold, Rgba.Hex("EAA5B5"), Rgba.Hex("B8D2DC"), Rgba.Hex("F4B07A") };
        // tufts
        for (int gy = 0; gy < G.H; gy += 7)
            for (int gx = 0; gx < G.W; gx += 7)
            {
                int x = gx + (int)(Fx.R(gx, gy, 1) * 7), y = gy + (int)(Fx.R(gx, gy, 2) * 7);
                if (!GrassArea(x, y, 3)) continue;
                if (Fx.R(gx, gy, 3) > 0.38) continue;
                int bi = GrassIdx(x, y);
                var tuft = Tufts[(int)(Fx.H(gx, gy, 4) % (uint)Tufts.Length)];
                foreach (var (dx, dy, tone) in tuft) Cv.Set(x + dx, y + dy, K.Grass[Math.Clamp(bi + tone, 0, 6)]);
            }
        // clover patches (little 3-leaf clusters)
        for (int gy = 0; gy < G.H; gy += 13)
            for (int gx = 0; gx < G.W; gx += 13)
            {
                int x = gx + (int)(Fx.R(gx, gy, 6) * 11), y = gy + (int)(Fx.R(gx, gy, 7) * 11);
                if (!GrassArea(x, y, 3) || Fx.R(gx, gy, 8) > 0.3) continue;
                int bi = GrassIdx(x, y);
                var lt = K.Grass[Math.Min(6, bi + 2)]; var dk = K.Grass[Math.Max(0, bi - 2)];
                Cv.Set(x, y, lt); Cv.Set(x + 1, y, lt); Cv.Set(x - 1, y + 1, lt); Cv.Set(x, y + 1, dk); Cv.Set(x + 2, y + 1, lt); Cv.Set(x + 1, y + 1, lt); Cv.Set(x, y + 2, dk);
            }
        // flower meadows: clustered by a low-frequency mask so patches read as intentional
        for (int gy = 0; gy < G.H; gy += 4)
            for (int gx = 0; gx < G.W; gx += 4)
            {
                double m = Fx.Fbm(gx / 30.0, gy / 30.0, 201);
                if (m < 0.66) continue;
                int x = gx + (int)(Fx.R(gx, gy, 9) * 4), y = gy + (int)(Fx.R(gx, gy, 10) * 4);
                if (!GrassArea(x, y, 2) || Fx.R(gx, gy, 11) > 0.55) continue;
                int ci = (int)(Fx.VN(gx / 55.0, gy / 55.0, 202) * 5.99) % flowerCols.Length;
                if (Fx.R(gx, gy, 12) < 0.18) ci = (ci + 2) % flowerCols.Length;
                var fc = flowerCols[ci];
                int bi = GrassIdx(x, y);
                Cv.Set(x, y + 1, K.Grass[Math.Max(0, bi - 2)]);
                Cv.Set(x, y, fc);
                if (Fx.R(gx, gy, 13) < 0.35)
                {   // larger bloom
                    Cv.Set(x - 1, y, fc); Cv.Set(x + 1, y, fc); Cv.Set(x, y - 1, fc);
                    Cv.Set(x, y, ci == 0 ? K.Gold : K.Cream);
                }
            }
    }

    // ---- far-edge tree mass -------------------------------------------------------------------------------
    static void RenderForest()
    {
        const int cellW = 15, cellH = 13;
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Forest) continue;
                // nearest crown centre (staggered grid, jittered)
                double bestD = 1e9, second = 1e9, bcx = 0, bcy = 0; int bid = 0;
                int gx0 = x / cellW, gy0 = y / cellH;
                for (int gy = gy0 - 2; gy <= gy0 + 2; gy++)
                    for (int gx = gx0 - 2; gx <= gx0 + 2; gx++)
                    {
                        double cx = gx * cellW + ((gy & 1) * cellW / 2.0) + (Fx.R(gx, gy, 301) - 0.5) * 7;
                        double cy = gy * cellH + (Fx.R(gx, gy, 302) - 0.5) * 6;
                        double dx = x + .5 - cx, dy = (y + .5 - cy) * 1.15;
                        double d = Math.Sqrt(dx * dx + dy * dy);
                        if (d < bestD) { second = bestD; bestD = d; bcx = cx; bcy = cy; bid = gx * 977 + gy; }
                        else if (d < second) second = d;
                    }
                double R = 9.5;
                double nx = (x + .5 - bcx) / R, ny = (y + .5 - bcy) / R;
                double light = -(nx * 0.6 + ny * 0.85);
                double t = 0.52 + 0.30 * light - 0.14 * Math.Min(1, bestD / R);
                // darker toward the outer map edge, light facing in
                double edge = Math.Min(Math.Min(x, G.W - 1 - x), Math.Min(y, G.H - 1 - y));
                t -= Math.Clamp((34 - edge) / 34.0, 0, 1) * 0.28;
                t += (Fx.R(x / 3, y / 2, 303) - 0.5) * 0.18 + Fx.Bay(x, y) * 0.09;
                if (bestD / Math.Max(0.1, second) > 0.80) t -= 0.18;      // crease between crowns
                if ((Fx.H(bid, 0, 5) & 7) == 0 && bestD < 3) t += 0.0;
                int idx = Math.Clamp((int)(t * K.Leaf.Length), 0, K.Leaf.Length - 1);
                Cv.Set(x, y, K.Leaf[Math.Min(idx, 5)]);
            }
    }

    // ---- shadows & light ----------------------------------------------------------------------------------
    static bool Shadable(int x, int y) =>
        x >= 0 && y >= 0 && x < G.W && y < G.H && M[x, y] != Mat.Wall && M[x, y] != Mat.Forest;

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
    public static void LightPoly(IReadOnlyList<(double x, double y)> pts, double k = 1.0)
    {
        var tmp = new Canvas(G.W, G.H);
        tmp.Poly(pts, K.Cream);
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (tmp.Opaque(x, y) && Shadable(x, y)) Cv.Set(x, y, Cv.Get(x, y).Light(k));
    }

    static void ApplyLight()
    {
        // forest casts shadow onto grass south / east of it (light from top-left)
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
                if (hit <= 3 || ((x + y) & 1) == 0 && hit <= 5) Cv.Set(x, y, Cv.Get(x, y).Shade(hit <= 2 ? 1.0 : 0.7));
            }
        // house cast shadows on the yard
        ShadeRect(G.HX0 + 4, G.HY1, G.HX1 - G.HX0, 5, 0.9);     // south of front wall
        ShadeRect(G.HX1, G.HY0 + 14, 7, G.HY1 - G.HY0 - 12, 0.9);// east of east wall
        // prop shadows from the layout
        foreach (var s in Layout.Shadows)
        {
            if (s.Kind == 'e') ShadeEll(s.A, s.B, s.C, s.D, s.K);
            else ShadeRect((int)s.A, (int)s.B, (int)s.C, (int)s.D, s.K);
        }
        // interior wall shadows
        HouseArt.InteriorShadows();
    }
}
