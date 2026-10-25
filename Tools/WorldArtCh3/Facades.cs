namespace WorldArtCh3;

/// <summary>The row of tall houses on the north side of the street (painted straight into the ground layer).</summary>
public static class Facades
{
    static Canvas Cv => Ground.Cv;
    static void P(int x, int y, Rgba c)
    {
        if (x < 0 || y < 0 || x >= G.W || y >= G.H) return;
        Cv.Set(x, y, c); Ground.M[x, y] = Mat.Facade;
    }

    public static readonly Rgba[] Ochre = { Rgba.Hex("9A7A46"), Rgba.Hex("B8955A"), Rgba.Hex("D2AE72"), Rgba.Hex("E4C58A"), Rgba.Hex("F2DCAA") };
    public static readonly Rgba[] Rose = { Rgba.Hex("A87868"), Rgba.Hex("C49484"), Rgba.Hex("DAB09C"), Rgba.Hex("EAC6B2"), Rgba.Hex("F6DCCA") };
    public static readonly Rgba[] Slate = { Rgba.Hex("3E4658"), Rgba.Hex("525B70"), Rgba.Hex("68728A"), Rgba.Hex("818CA4"), Rgba.Hex("9CA7BC") };
    public static readonly Rgba[] Sage = { Rgba.Hex("7E8C74"), Rgba.Hex("97A58A"), Rgba.Hex("B0BEA2"), Rgba.Hex("C8D4B8"), Rgba.Hex("DDE6CE") };

    public record B(string Id, int X0, int X1, int RoofTop, int Eave, Rgba[] Wall, Rgba[] Roof, int Seed);
    public static readonly B[] Row =
    {
        new("A", 0, 170, 16, 46, K.Peach, K.Roof, 11),
        new("B", 170, 300, 34, 58, Ochre, K.Roof, 23),
        new("R", 300, 412, 28, 54, Sage, K.Roof, 37),
        new("C", 412, 560, 22, 50, K.BlueWall, Slate, 41),
        new("D", 560, 760, 14, 44, Rose, K.Roof, 53),
    };

    public const int UpperTop = 66, LowerTop = 100, WinW = 16, WinH = 22;

    // window sites: building id -> (centre x, floor 0=upper/1=lower, kind, shutter paint index)
    public static readonly Dictionary<string, (int cx, int floor, char kind, int sh)[]> Windows = new()
    {
        ["A"] = new[] { (22, 0, 'S', 0), (62, 0, 'H', 2), (108, 0, 'L', 1), (148, 0, 'T', 3), (22, 1, 'D', 1), (62, 1, 'S', 0), (148, 1, 'B', 4) },
        ["B"] = new[] { (200, 0, 'S', 2), (240, 0, 'L', 0), (276, 0, 'T', 2), (200, 1, 'H', 0), (276, 1, 'S', 4) },
        ["R"] = new[] { (326, 0, 'S', 1), (356, 0, 'L', 1), (386, 0, 'T', 1), (326, 1, 'D', 3), (394, 1, 'S', 1) },
        ["C"] = new[] { (438, 0, 'T', 3), (478, 0, 'S', 0), (520, 0, 'H', 3), (544, 1, 'S', 0), (438, 1, 'L', 3), (466, 1, 'D', 3) },
        ["D"] = new[] { (586, 0, 'S', 4), (626, 0, 'H', 1), (666, 0, 'L', 2), (706, 0, 'T', 4), (742, 0, 'S', 1), (586, 1, 'B', 4), (742, 1, 'D', 1) },
    };
    public static readonly Dictionary<string, (int cx, int w, int ramp)> Doors = new()
    {
        ["A"] = (104, 20, 0), ["B"] = (236, 20, 2), ["R"] = (356, 20, 1), ["C"] = (494, 22, 3), ["D"] = (666, 22, 4)
    };

    static readonly Rgba[][] Paints = { K.TealR, K.RedR, K.Mustard, K.BlueR, K.Wood };

    public static void Paint()
    {
        foreach (var b in Row)
        {
            Roof(b);
            Wall(b);
            Foundation(b);
            Quoins(b);
            StringCourse(b);
            foreach (var w in Windows[b.Id]) Window(w.cx, w.floor == 0 ? UpperTop : LowerTop, w.kind, Paints[w.sh]);
            Door(b);
            Scars(b);
        }
        Balcony();
    }

    // ---- roof -----------------------------------------------------------------------------------------------
    static void Roof(B b)
    {
        for (int y = b.RoofTop; y < b.Eave; y++)
            for (int x = b.X0; x < b.X1; x++)
            {
                int row = (y - b.RoofTop) / 7, ly = (y - b.RoofTop) % 7;
                int off = (row & 1) == 0 ? 0 : 4;
                int gx = x + off, lx = gx % 8, id = gx / 8;
                uint h = Fx.H(id, row, b.Seed);
                int bi = 2 + ((h & 7) == 0 ? 1 : (h & 7) == 1 ? -1 : 0);
                Rgba c;
                if (y == b.RoofTop) c = b.Roof[4];
                else if (y == b.RoofTop + 1) c = b.Roof[3];
                else if (ly == 0) c = b.Roof[Math.Min(4, bi + 1)];
                else if (ly <= 3) c = b.Roof[bi];
                else if (ly <= 5) c = b.Roof[Math.Max(0, bi - 1)];
                else c = b.Roof[0];
                if (ly >= 1 && ly <= 5 && lx == 0) c = b.Roof[Math.Max(0, bi - 2)];
                if (ly >= 1 && ly <= 3 && lx == 1) c = b.Roof[Math.Min(4, bi + 1)];
                if (x <= b.X0 + 1 || x >= b.X1 - 2) c = b.Roof[0];
                P(x, y, c);
            }
        for (int x = b.X0; x < b.X1; x++) P(x, b.Eave - 1, K.Wood[0]);
    }

    // ---- wall -----------------------------------------------------------------------------------------------
    static Rgba WallPx(B b, int x, int y)
    {
        var pl = b.Wall;
        int ly = y - b.Eave;
        if (ly == 0) return pl[0].Shade(0.5);
        if (ly == 1) return pl[1].Shade(0.4);
        if (ly == 2) return ((x + y) & 1) == 0 ? pl[1] : pl[2];
        if (ly == 3 && ((x + y) & 1) == 0) return pl[2];
        int idx = 3;
        double v = Fx.VN(x / 9.0, y / 6.0, b.Seed) + Fx.Bay(x, y) * 0.07;
        if (v > 0.70) idx = 2; else if (v < 0.22) idx = 4;
        int fromBase = G.BaseY - 8 - y;
        if (fromBase < 14 && Fx.VN(x / 5.0, y / 4.0, b.Seed + 5) + fromBase / 14.0 * 0.5 < 0.62) idx = Math.Max(0, idx - 1);
        if (fromBase < 5 && ((x + y) & 1) == 0) idx = Math.Max(0, idx - 1);
        if (idx == 3 && Fx.R(x / 3, y / 2, b.Seed + 9) > 0.94) idx = 2;
        return pl[Math.Clamp(idx, 0, 4)];
    }

    static void Wall(B b)
    {
        for (int y = b.Eave; y < G.BaseY - 8; y++)
            for (int x = b.X0; x < b.X1; x++) P(x, y, WallPx(b, x, y));
    }

    static void Foundation(B b)
    {
        for (int y = G.BaseY - 8; y < G.BaseY; y++)
            for (int x = b.X0; x < b.X1; x++)
            {
                int row = (y - (G.BaseY - 8)) / 4, ly = (y - (G.BaseY - 8)) % 4;
                int gx = x + (row == 0 ? 0 : 6), id = gx / 12, lx = gx % 12;
                uint h = Fx.H(id, row, b.Seed + 70);
                int idx = 2 + (int)(h % 3);
                Rgba c = K.Stone[Math.Clamp(idx, 0, 5)];
                if (ly == 3) c = K.Stone[0];
                else if (ly == 0) c = K.Stone[Math.Min(5, idx + 1)];
                if (lx == 0) c = K.Stone[0];
                P(x, y, c);
            }
    }

    static void Quoins(B b)
    {
        for (int side = 0; side < 2; side++)
        {
            int xs = side == 0 ? b.X0 : b.X1 - 5;
            for (int y = b.Eave + 4; y < G.BaseY - 12; y += 9)
            {
                int row = (y - b.Eave) / 9;
                int w = (row & 1) == 0 ? 5 : 4;
                int xo = side == 0 ? xs : xs + (5 - w);
                for (int j = 0; j < 8; j++)
                    for (int i = 0; i < w; i++)
                    {
                        int idx = j == 0 ? 4 : j == 7 ? 1 : 3;
                        if (i == 0 && side == 0) idx = Math.Max(0, idx - 1);
                        if (i == w - 1 && side == 1) idx = Math.Max(0, idx - 1);
                        P(xo + i, y + j, K.Plaster[Math.Clamp(idx, 0, 4)].Shade(0.15));
                    }
            }
        }
        if (b.X0 > 0) for (int y = b.Eave - 2; y < G.BaseY; y++) P(b.X0, y, K.Line2);
    }

    static void StringCourse(B b)
    {
        int y = 94;
        for (int x = b.X0 + 5; x < b.X1 - 5; x++)
        {
            P(x, y, b.Wall[4]); P(x, y + 1, b.Wall[3]); P(x, y + 2, b.Wall[1]);
            if (((x + y) & 1) == 0) P(x, y + 3, b.Wall[1].Shade(0.5));
        }
    }

    // ---- windows --------------------------------------------------------------------------------------------
    static void Window(int cx, int top, char kind, Rgba[] sh)
    {
        int x0 = cx - WinW / 2, w = WinW, h = WinH;
        for (int i = -2; i < w + 2; i++) { P(x0 + i, top - 3, K.Stone[4]); P(x0 + i, top - 2, K.Stone[3]); }
        for (int j = -1; j < h + 2; j++) { P(x0 - 2, top + j, K.Stone[3]); P(x0 - 1, top + j, K.Stone[2]); P(x0 + w, top + j, K.Stone[1]); P(x0 + w + 1, top + j, K.Stone[0]); }
        for (int i = -3; i < w + 3; i++) { P(x0 + i, top + h, K.Stone[4]); P(x0 + i, top + h + 1, K.Stone[2]); P(x0 + i, top + h + 2, K.Stone[0]); }
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++) P(x0 + i, top + j, Rgba.Hex("2E2A3A"));
        switch (kind)
        {
            case 'S': Shutter(x0, top, w, h, sh, 2); break;
            case 'H':
                Glass(x0, top, w, h);
                Shutter(x0, top, w / 2, h, sh, 1);
                for (int j = 0; j < h; j++) { P(x0 + w - 1, top + j, sh[1]); P(x0 + w, top + j, sh[0]); }
                break;
            case 'L': Glass(x0, top, w, h); Ground.AddLit(x0, top, w, h); break;
            case 'D': Glass(x0, top, w, h); Curtain(x0, top, w, h); break;
            case 'T': Glass(x0, top, w, h); Tape(x0, top, w, h); break;
            case 'B': Boards(x0, top, w, h); break;
        }
    }

    static void Glass(int x0, int top, int w, int h)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double t = (double)j / h;
                var c = Dusk.Mix(Rgba.Hex("5E6A9A"), Rgba.Hex("2E3258"), t);
                if (i + j < 9 && j > 1) c = Dusk.Mix(c, Rgba.Hex("E49A62"), 0.45 - (i + j) * 0.04);
                if (((i + j) & 1) == 0 && i + j >= 9 && i + j < 12) c = Dusk.Mix(c, Rgba.Hex("D07C74"), 0.3);
                P(x0 + i, top + j, c);
            }
        for (int j = 0; j < h; j++) P(x0 + w / 2, top + j, K.Wood[1]);
        for (int i = 0; i < w; i++) P(x0 + i, top + h / 2 - 1, K.Wood[1]);
        for (int i = 0; i < w; i++) P(x0 + i, top, K.Wood[2]);
    }

    static void Shutter(int x0, int top, int w, int h, Rgba[] sh, int leaves)
    {
        int lw = w / leaves;
        for (int l = 0; l < leaves; l++)
            for (int j = 0; j < h; j++)
                for (int i = 0; i < lw; i++)
                {
                    int idx = 2;
                    if (i == 0) idx = 3; else if (i == lw - 1) idx = 0;
                    else if (i % 3 == 0) idx = 1;
                    if (j < 2 || j >= h - 2) idx = Math.Min(4, idx + 1);
                    if ((j == h / 3 || j == h * 2 / 3) && i > 0 && i < lw - 1) idx = 0;
                    P(x0 + l * lw + i, top + j, sh[Math.Clamp(idx, 0, 4)]);
                }
        if (leaves == 2) { P(x0 + lw - 1, top + h / 2, K.Iron[4]); P(x0 + lw, top + h / 2, K.Iron[4]); }
    }

    static void Curtain(int x0, int top, int w, int h)
    {
        var lin = K.Linen;
        for (int j = 2; j < h - 1; j++)
        {
            int cw = 5 - (j > h / 2 ? 1 : 0);
            for (int i = 0; i < cw; i++) { P(x0 + i, top + j, lin[(i + j / 3) % 3]); P(x0 + w - 1 - i, top + j, lin[(i + j / 3 + 1) % 3]); }
        }
        for (int i = 0; i < w; i++) P(x0 + i, top + 1, K.Terra.Shade(0.4));
    }

    static void Tape(int x0, int top, int w, int h)
    {
        var paper = Rgba.Hex("E8DCC0"); var shade = Rgba.Hex("B5A688");
        for (int i = 0; i < w; i++)
        {
            int j1 = (int)(i * (h - 1.0) / (w - 1)), j2 = h - 1 - j1;
            P(x0 + i, top + j1, paper); P(x0 + i, top + Math.Min(h - 1, j1 + 1), shade);
            P(x0 + i, top + j2, paper); P(x0 + i, top + Math.Max(0, j2 - 1), shade);
        }
        for (int j = 0; j < h; j++) P(x0 + w / 2 - 1, top + j, paper);
        for (int i = 0; i < w; i++) P(x0 + i, top + h / 2, paper);
    }

    static void Boards(int x0, int top, int w, int h)
    {
        for (int j = 0; j < h; j += 5)
        {
            int jj = Math.Min(j, h - 5);
            int tilt = (j / 5) % 2 == 0 ? 1 : -1;
            for (int i = 0; i < w + 4; i++)
            {
                int yy = jj + (tilt > 0 ? i / 8 : 2 - i / 8);
                for (int k = 0; k < 4; k++)
                {
                    int idx = k == 0 ? 3 : k == 3 ? 0 : 2;
                    if (Fx.R(i, j, 91) > 0.9) idx = Math.Max(0, idx - 1);
                    int px = i - 2, py = yy + k;
                    if (px >= 0 && px < w && py >= 0 && py < h) P(x0 + px, top + py, K.Wood[idx]);
                }
            }
            P(x0 + 2, top + jj + 2, K.Iron[4]); P(x0 + w - 3, top + jj + 2, K.Iron[4]);
        }
    }

    // ---- doors ----------------------------------------------------------------------------------------------
    static void Door(B b)
    {
        var (cx, w, ri) = Doors[b.Id];
        var pr = Paints[ri];
        int x0 = cx - w / 2, top = 98, bot = G.BaseY - 8;
        int h = bot - top;
        for (int j = -3; j < h + 8; j++)
        {
            P(x0 - 3, top + j, K.Stone[3]); P(x0 - 2, top + j, K.Stone[2]);
            P(x0 + w + 1, top + j, K.Stone[1]); P(x0 + w, top + j, K.Stone[2]);
        }
        for (int i = -3; i < w + 3; i++) { P(x0 + i, top - 4, K.Stone[4]); P(x0 + i, top - 3, K.Stone[3]); P(x0 + i, top - 2, K.Stone[1]); }
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = 2, lx = i, ly = j;
                bool panelA = ly > 4 && ly < h / 2 - 2 && lx > 2 && lx < w - 3;
                bool panelB = ly > h / 2 + 1 && ly < h - 3 && lx > 2 && lx < w - 3;
                if (lx == 0 || lx == w - 1) idx = 0;
                else if (lx == 1) idx = 3;
                else if (ly < 2) idx = 3;
                else if (panelA || panelB)
                {
                    bool edge = ly == (panelA ? 5 : h / 2 + 2) || lx == 3;
                    bool lo = ly == (panelA ? h / 2 - 3 : h - 4) || lx == w - 4;
                    idx = edge ? 3 : lo ? 0 : 2;
                    if (panelA && ly > 6 && ly < 11 && lx > 5 && lx < w - 6) idx = 0;
                }
                P(x0 + i, top + j, pr[Math.Clamp(idx, 0, 4)]);
            }
        P(x0 + w - 4, top + h / 2, K.Gold); P(x0 + w - 4, top + h / 2 + 1, K.Mustard[1]);
        for (int j = 0; j < 3; j++)
            for (int i = -4; i < w + 4; i++)
                P(x0 + i, G.BaseY - 3 + j, K.Stone[j == 0 ? 4 : j == 1 ? 3 : 1]);
    }

    // ---- balcony (apartment A, upper floor) -----------------------------------------------------------------
    static void Balcony()
    {
        int x0 = 44, x1 = 168, y = 88;
        for (int x = x0; x < x1; x++)
        {
            P(x, y, K.Stone[4]); P(x, y + 1, K.Stone[3]); P(x, y + 2, K.Stone[2]); P(x, y + 3, K.Stone[0]);
            if (((x + y) & 1) == 0) P(x, y + 4, Cv.Get(x, y + 4).Shade(0.6));
        }
        for (int j = 0; j < 4; j++) { P(x0 - 1, y + j, K.Stone[1]); P(x1, y + j, K.Stone[0]); }
        for (int x = x0; x < x1; x++)
        {
            P(x, y - 11, K.Iron[3]); P(x, y - 10, K.Iron[1]);
            if ((x - x0) % 4 == 1) for (int j = -9; j < 0; j++) P(x, y + j, K.Iron[j % 3 == 0 ? 3 : 2]);
        }
        for (int j = -11; j < 0; j++) { P(x0, y + j, K.Iron[3]); P(x1 - 1, y + j, K.Iron[2]); }
    }

    // ---- old scars: hairline cracks and plaster that has fallen away (before the first blast) ---------------------
    static void Scars(B b)
    {
        var patches = b.Id switch
        {
            "A" => new[] { (126, 112, 18, 12), (10, 118, 10, 8) },
            "B" => new[] { (256, 118, 14, 9) },
            "R" => new[] { (336, 114, 10, 8) },
            "C" => new[] { (428, 116, 16, 10), (526, 110, 9, 8) },
            _ => new[] { (606, 113, 14, 9), (700, 116, 18, 8) },
        };
        foreach (var (px, py, pw, ph) in patches) BrickPatch(px, py, pw, ph, b.Seed);
        var rnd = new Random(b.Seed);
        for (int n = 0; n < 2; n++)
        {
            int x = b.X0 + 20 + rnd.Next(b.X1 - b.X0 - 40), y = 60 + rnd.Next(4);
            if (b.Id == "A" && y + 14 > 78) continue;
            Crack(x, y, 12 + rnd.Next(12), b.Wall[0].Shade(0.6));
        }
    }

    static void BrickPatch(int x0, int y0, int w, int h, int seed)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
            {
                double ex = Math.Min(Math.Min(x - x0, x0 + w - 1 - x), Math.Min(y - y0, y0 + h - 1 - y));
                if (ex < 1.5 * Fx.R(x, y, seed + 1)) continue;
                if (ex < 1) { P(x, y, K.Plaster[0].Shade(0.6)); continue; }
                int row = (y - y0) / 3, ly = (y - y0) % 3;
                int bx = (x - x0 + (row & 1) * 3) % 6;
                Rgba c = (ly == 2 || bx == 5) ? K.Mortar : K.Roof[1 + (int)(Fx.H(row, (x - x0 + (row & 1) * 3) / 6, seed) % 3)];
                P(x, y, Rgba.Mul(c, 0.92, 0.9, 0.95));
            }
    }

    public static void Crack(int x, int y, int len, Rgba col)
    {
        int cx = x, cy = y;
        var rnd = new Random(x * 31 + y);
        for (int i = 0; i < len; i++)
        {
            P(cx, cy, col);
            int d = rnd.Next(5);
            if (d == 0) cx--; else if (d == 1) cx++;
            cy++;
            if (d == 2) P(cx + 1, cy, col.Shade(0.2));
        }
    }

    // ---- lit windows & roof rim are repainted after the dusk grade ----------------------------------------------
    public static void PaintLitWindow(int x0, int top, int w, int h)
    {
        var amber = Rgba.Hex("FFD27A"); var deep = Rgba.Hex("E08A4C"); var cream = Rgba.Hex("FFF0B8");
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double t = (double)j / h;
                var c = Dusk.Mix(cream, deep, Math.Clamp(t * 1.1 + Math.Abs(i - w / 2.0) / w * 0.5, 0, 1));
                if (((i + j) & 1) == 0 && t > 0.3 && t < 0.4) c = amber;
                Cv.Set(x0 + i, top + j, c);
            }
        for (int j = 0; j < h - 3; j++) { int cw = 4 - j / 8; for (int i = 0; i < cw; i++) { Cv.Set(x0 + i, top + j, Rgba.Hex("D98A5A")); Cv.Set(x0 + w - 1 - i, top + j, Rgba.Hex("C97852")); } }
        for (int j = 0; j < h; j++) Cv.Set(x0 + w / 2, top + j, Rgba.Hex("7A4A30"));
        for (int i = 0; i < w; i++) { Cv.Set(x0 + i, top + h / 2 - 1, Rgba.Hex("7A4A30")); Cv.Set(x0 + i, top, Rgba.Hex("7A4A30")); }
    }

    public static void RimLight()
    {
        foreach (var b in Row)
            for (int x = b.X0 + 1; x < b.X1 - 1; x++)
            {
                Cv.Set(x, b.RoofTop, Dusk.Warm(Cv.Get(x, b.RoofTop), 0.9));
                Cv.Set(x, b.RoofTop + 1, Dusk.Warm(Cv.Get(x, b.RoofTop + 1), 0.45));
                if (((x + b.RoofTop) & 1) == 0) Cv.Set(x, b.RoofTop + 2, Dusk.Warm(Cv.Get(x, b.RoofTop + 2), 0.25));
            }
    }
}
