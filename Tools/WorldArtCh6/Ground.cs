namespace WorldArtCh6;

public enum Mat : byte { Void, Roof, Wall, Plank, Tile, Thresh, Pave, Cobble, Alley, Step, Kerb }

/// <summary>Fixed world geometry (image coordinates: origin top-left, y down; 16 px tiles).</summary>
public static class Geo
{
    // building A: our rented room + the stair lobby
    public const int AX0 = 32, AX1 = 472;
    public const int EaveY = 48, FaceY = 64, FloorY = 96, FloorBot = 296, FrontBot = 336;
    public const int RoomX0 = 48, RoomX1 = 296;          // room floor
    public const int PartX = 296, PartW = 16;            // wall between room and lobby
    public const int LobbyX0 = 312, LobbyX1 = 456;
    public const int DoorY0 = 224, DoorY1 = 256;         // door in the partition
    public const int FrontX0 = 368, FrontX1 = 400;       // building entrance (south wall)
    // alley and the shop next door
    public const int AlleyX0 = 472, AlleyX1 = 496;
    public const int BX0 = 496, BX1 = 736, ShopFaceY = 256;
    // street
    public const int SideY0 = 336, SideY1 = 358, RoadY0 = 362, RoadY1 = 438, FarY1 = 456;
}

public static partial class Ground
{
    public static Canvas Cv = new(G.W, G.H);
    public static Mat[,] M = new Mat[G.W, G.H];
    static readonly bool[,] Ext = new bool[G.W, G.H];      // outdoor pixels get the cool dusk grade
    static readonly bool[,] Emissive = new bool[G.W, G.H]; // lit windows etc.: never graded or dimmed
    static readonly double[,] Lm = new double[G.W, G.H];   // warm lamp light
    static readonly double[,] Lc = new double[G.W, G.H];   // cool window light

    static bool In(int x, int y) => x >= 0 && y >= 0 && x < G.W && y < G.H;
    static void Px(int x, int y, Rgba c, Mat m, bool ext)
    {
        if (!In(x, y)) return;
        Cv.Set(x, y, c); M[x, y] = m; Ext[x, y] = ext;
    }
    static void Lit(int x, int y, Rgba c) { if (!In(x, y)) return; Cv.Set(x, y, c); Emissive[x, y] = true; }
    public static bool IsEmissive(int x, int y) => Emissive[x, y];
    public static bool IsExt(int x, int y) => Ext[x, y];
    public static double WarmAt(int x, int y) => Lm[x, y];
    public static double CoolAt(int x, int y) => Lc[x, y];
    public static void AddWarm(int x, int y, double p) => Lm[x, y] += p;
    public static void AddCool(int x, int y, double p) => Lc[x, y] += p;

    public static void Build()
    {
        Cv.Fill(Rgba.Hex("2B2F4A"));
        Buildings.Paint();
        Street.Paint();
        Grade();
        Buildings.Emit();
        Street.Emit();
        Lights.Apply();
        Lights.CastShadows();
    }

    /// <summary>Outdoor surfaces get the blue evening cast; interiors stay warm.</summary>
    static void Grade()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
                if (Ext[x, y]) Cv.Set(x, y, E.Cool(Cv.Get(x, y), 1.0));
    }

    // ------------------------------------------------------------------------------------------------
    // shared surface painters
    // ------------------------------------------------------------------------------------------------
    public static Rgba PlankPx(int px, int py, Rgba[] ramp, int bi = 3)
    {
        int ph = 8, row = py / ph, ly = py % ph;
        int off = (int)(Fx.R(row, 1, 31) * 60), len = 56;
        int id = Math.DivRem(px + off, len, out int lx);
        uint h = Fx.H(row, id, 33);
        int tone = bi + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        if (ly == ph - 1 || lx == 0) return ramp[Math.Max(0, bi - 2)];
        int idx = tone;
        if (ly == 0) idx = tone + 1;
        else
        {
            uint gh = Fx.H(row, id * 7 + ly, 35);
            int gs = (int)(gh % 40), gl = 8 + (int)((gh >> 8) % 14);
            if ((ly == 2 || ly == 4 || ly == 6) && ((gh >> 16) & 3) != 0 && lx >= gs && lx < gs + gl) idx = tone - 1;
            else if (ly == 3 && ((gh >> 20) & 7) == 0 && lx > gs && lx < gs + 3) idx = tone + 1;
        }
        if ((h >> 4 & 7) == 0)
        {
            int kx = 12 + (int)((h >> 8) % 30), ky = 3 + (int)((h >> 14) % 2);
            int dx = lx - kx, dy = ly - ky;
            if (dx * dx / 4.0 + dy * dy / 1.4 <= 1.0) idx = tone - 2;
            else if (dx * dx / 9.0 + dy * dy / 3.0 <= 1.0) idx = tone - 1;
        }
        return ramp[Math.Clamp(idx, 0, ramp.Length - 1)];
    }

    public static Rgba TilePx(int x, int y, int x0, int y0)
    {
        int lx = (x - x0) % 16, ly = (y - y0) % 16, tx = (x - x0) / 16, ty = (y - y0) / 16;
        bool alt = ((tx + ty) & 1) == 1;
        var grout = Rgba.Hex("9C958E");
        if (lx == 0 || ly == 0)
        {
            if (lx <= 1 && ly <= 1 && !(lx == 1 && ly == 1)) return K.Teal;
            return grout;
        }
        var a = E.Lobby[3]; var b = E.Lobby[2];
        var c = alt ? b : a;
        if (ly == 1 || lx == 1) c = alt ? E.Lobby[3] : E.Lobby[4];
        else if (ly == 15 || lx == 15) c = alt ? E.Lobby[1] : E.Lobby[2];
        else
        {
            if (!alt && (lx + ly) == 18 && Fx.R(tx, ty, 17) > 0.4) c = E.Lobby[2];
            if (alt && lx == 7 && ly == 8) c = E.Lobby[1];
            if (Fx.R(x / 3, y / 3, 19) > 0.97) c = E.Lobby[1];
        }
        return c;
    }

    /// <summary>One roof slope: tile courses with a darker north slope, a lit south slope, a ridge cap and a gutter along the bottom edge.</summary>
    public static Rgba RoofBlockPx(int x, int y, int x0, int y0, int x1, int y1, int ridgeY, Rgba[] ramp, int seed, int tileW = 10, int rowH = 7)
    {
        int row = (y - y0) / rowH, ly = (y - y0) % rowH;
        // party walls at both ends
        if (x == x0) return ramp[4];
        if (x == x0 + 1) return ramp[0];
        if (x == x1 - 1) return ramp[0];
        if (x == x1 - 2) return ramp[3];
        if (y >= y1 - 3)
        {
            int g = y - (y1 - 3);
            return g == 0 ? ramp[4] : g == 1 ? ramp[1] : ramp[0];                // gutter
        }
        if (y >= ridgeY - 1 && y < ridgeY + 3)
        {
            int r = y - (ridgeY - 1);
            return r == 0 ? ramp[0] : r == 1 ? ramp[4] : r == 2 ? ramp[3] : ramp[0];   // ridge cap
        }
        bool upper = y < ridgeY;
        int off = (row & 1) == 0 ? 0 : tileW / 2;
        int gx = x - x0 + off, lx = gx % tileW, id = gx / tileW;
        uint h = Fx.H(id, row, seed);
        int b = (upper ? 1 : 2) + ((h & 7) == 0 ? 1 : (h & 7) == 1 ? -1 : 0);
        int idx = b;
        if (ly == 0) idx = b + 1;
        else if (ly >= rowH - 2) idx = b - 1;
        if (lx == 0) idx = b - 1;
        else if (lx == 1 && ly < rowH - 2) idx = b + 1;
        var c = ramp[Math.Clamp(idx, 0, 4)];
        // moss and weather stains in drifts
        double m = Fx.Fbm(x / 13.0, y / 9.0, seed + 9);
        if (m > 0.70 && ly < rowH - 1) c = Rgba.Mul(c, 0.84, 1.08, 0.90);
        else if (m < 0.22 && (x + y) % 2 == 0) c = c.Shade(0.25);
        return c;
    }

    /// <summary>Terracotta / slate roof tiles in scalloped rows.</summary>
    public static Rgba RoofPx(int x, int y, int x0, int y0, Rgba[] ramp, int seed, int tileW = 8, int rowH = 8)
    {
        int row = (y - y0) / rowH, ly = (y - y0) % rowH;
        int off = (row & 1) == 0 ? 0 : tileW / 2;
        int gx = x - x0 + off, lx = gx % tileW, id = gx / tileW;
        uint h = Fx.H(id, row, seed);
        int b = 2 + ((h & 7) == 0 ? 1 : (h & 7) == 1 ? -1 : 0);
        if (ly == 0) return ramp[Math.Min(4, b + 1)];
        if (ly <= 4) return ramp[Math.Clamp(b, 0, 4)];
        if (ly <= 6) return ramp[Math.Clamp(b - 1, 0, 4)];
        bool corner = lx <= 1 || lx >= tileW - 2;
        return corner ? ramp[1] : ramp[0];
    }
}
