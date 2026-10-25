namespace WorldArtCh3;

public static partial class Ground
{
    public static void AddLit(int x, int y, int w, int h) => LitWindows.Add((x, y, w, h));

    // ---- stone texture (re-used from the chapter-1 generator) -------------------------------------------------
    public static Rgba CobblePx(int x, int y, bool slab, int seed = 3)
    {
        int rowH = slab ? 7 : 8;
        int ry = y / rowH, ly = y % rowH;
        int off = (int)(Fx.R(ry, 0, seed) * 13);
        int sx = -off, id = 0;
        while (true)
        {
            int w = (slab ? 12 : 9) + (int)(Fx.R(ry, id, seed + 1) * (slab ? 12 : 6));
            if (x < sx + w) return StonePx(x - sx, ly, w, rowH, Fx.H(ry, id, seed + 2), slab);
            sx += w; id++;
        }
    }

    static Rgba StonePx(int lx, int ly, int w, int h, uint hash, bool slab)
    {
        if (lx == 0 || ly == 0) return K.Mortar;
        bool corner = (lx == 1 || lx == w - 1) && (ly == 1 || ly == h - 1);
        if (corner) return K.Mortar;
        int baseIdx = 2 + (int)(hash % 3);
        if (slab) baseIdx = 3 + (int)(hash % 2);
        int idx = baseIdx;
        if (ly == 1 || lx == 1) idx++;
        if (ly == h - 1 || lx == w - 1) idx--;
        int sx = 2 + (int)((hash >> 4) % (uint)Math.Max(1, w - 4)), sy = 2 + (int)((hash >> 9) % (uint)Math.Max(1, h - 3));
        if ((hash >> 14 & 3) == 0 && ((lx == sx && ly == sy) || (lx == sx + 1 && ly == sy + 1))) idx--;
        if ((hash >> 18 & 3) == 0 && lx == sx + 1 && ly == sy) idx++;
        var c = K.Stone[Math.Clamp(idx, 0, K.Stone.Length - 1)];
        return Rgba.Mul(c, 1.03, 0.98, 0.90);
    }

    static bool Ell(int x, int y, double cx, double cy, double rx, double ry)
    {
        double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry;
        return dx * dx + dy * dy < 1;
    }

    // ---- the street ------------------------------------------------------------------------------------------
    static void Street()
    {
        // north pavement
        for (int y = G.BaseY; y < 158; y++)
            for (int x = 0; x < G.W; x++)
            {
                var c = CobblePx(x, y, true, 5);
                c = Rgba.Mul(c, 0.96, 0.95, 0.97);
                P(x, y, c, Mat.Sidewalk);
            }
        // north kerb: lit top, shaded face, dark gutter line
        for (int x = 0; x < G.W; x++)
        {
            int seg = (x / 22);
            int sh = (int)(Fx.H(seg, 1, 31) % 2);
            P(x, 158, K.Stone[5], Mat.Curb);
            P(x, 159, K.Stone[4], Mat.Curb);
            P(x, 160, K.Stone[3 - sh], Mat.Curb);
            P(x, 161, K.Stone[2 - sh], Mat.Curb);
            P(x, 162, K.Stone[0], Mat.Curb);
            P(x, 163, K.Dark[1], Mat.Road);
            if (x % 22 == 0) { P(x, 159, K.Mortar, Mat.Curb); P(x, 160, K.Mortar, Mat.Curb); P(x, 161, K.Mortar, Mat.Curb); }
        }
        // carriageway
        for (int y = 164; y < G.RoadY1; y++)
            for (int x = 0; x < G.W; x++)
            {
                var c = CobblePx(x, y, false, 9);
                double wear = Fx.Fbm(x / 22.0, y / 9.0, 17);
                if (wear > 0.62) c = Rgba.Mul(c, 1.07, 1.05, 0.98);
                else if (wear < 0.34) c = Rgba.Mul(c, 0.88, 0.86, 0.92);
                // darker gutter bands at both kerbs
                if (y < 168 || y > G.RoadY1 - 5) c = Rgba.Mul(c, 0.82, 0.8, 0.86);
                P(x, y, c, Mat.Road);
            }
        // south pavement
        for (int x = 0; x < G.W; x++) { P(x, G.RoadY1, K.Dark[1], Mat.Curb); P(x, G.RoadY1 + 1, K.Stone[0], Mat.Curb); P(x, G.RoadY1 + 2, K.Stone[3], Mat.Curb); P(x, G.RoadY1 + 3, K.Stone[5], Mat.Curb); }
        for (int y = G.RoadY1 + 4; y < G.LedgeY; y++)
            for (int x = 0; x < G.W; x++)
                P(x, y, Rgba.Mul(CobblePx(x, y, true, 21), 0.97, 0.96, 0.98), Mat.Sidewalk);

        RoadDetail();
        Hopscotch(196, 304);
        Litter();
    }

    static void RoadDetail()
    {
        // manhole cover
        void Manhole(int cx, int cy)
        {
            for (int y = cy - 8; y <= cy + 8; y++)
                for (int x = cx - 10; x <= cx + 10; x++)
                {
                    double dx = (x + .5 - cx) / 10.0, dy = (y + .5 - cy) / 7.0, d = dx * dx + dy * dy;
                    if (d > 1.15) continue;
                    Rgba c;
                    if (d > 1.0) c = K.Iron[0];
                    else if (d > 0.78) c = (dx * 0.6 + dy * 0.8) < 0 ? K.Iron[3] : K.Iron[1];
                    else
                    {
                        c = K.Iron[2];
                        if (((x - cx) + 2 * (y - cy)) % 4 == 0 || ((x - cx) - 2 * (y - cy)) % 4 == 0) c = K.Iron[1];
                        if (d < 0.1) c = K.Iron[4];
                    }
                    P(x, y, c, Mat.Road);
                }
        }
        Manhole(252, 226);
        Manhole(598, 252);
        // storm drains in the north kerb
        foreach (var gx in new[] { 128, 510 })
            for (int i = 0; i < 12; i++) { P(gx + i, 161, K.Iron[0], Mat.Curb); if (i % 2 == 0) P(gx + i, 160, K.Iron[1], Mat.Curb); }
        // old cracks running across the cobbles
        void RoadCrack(int x, int y, int len, int dir)
        {
            var rnd = new Random(x * 7 + y);
            for (int i = 0; i < len; i++)
            {
                P(x, y, K.Dark[1], Mat.Road); if (i % 3 == 0) P(x + 1, y, K.Dark[0], Mat.Road);
                x += dir; if (rnd.Next(3) == 0) y += rnd.Next(2) * 2 - 1;
                y = Math.Clamp(y, 166, G.RoadY1 - 3);
            }
        }
        RoadCrack(70, 200, 40, 1); RoadCrack(600, 178, 30, -1); RoadCrack(380, 262, 26, 1);
        // patched cobbles (flat tar patches)
        foreach (var (cx, cy, rx, ry) in new[] { (180, 190, 12, 6), (530, 236, 14, 7), (700, 190, 10, 5) })
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                    if (Ell(x, y, cx, cy, rx, ry)) P(x, y, K.Dark[1 + (int)(Fx.R(x, y, 5) * 2)], Mat.Road);
    }

    static void Hopscotch(int cx, int cy)
    {
        var chalk = Rgba.Hex("F4E9E4"); var pink = Rgba.Hex("EAA5B5");
        // squares laid out along the pavement: 1, 2, 3+4, 5, 6+7, 8
        var cells = new (int dx, int dy, int w)[] { (0, 0, 9), (9, 0, 9), (18, 0, 9), (18, -9, 9), (27, 0, 9), (36, 0, 9), (36, -9, 9), (45, 0, 9), (54, 0, 12) };
        foreach (var (dx, dy, w) in cells)
        {
            for (int i = 0; i < w; i++) { P(cx + dx + i, cy + dy - 8, chalk); P(cx + dx + i, cy + dy, chalk); }
            for (int j = -8; j <= 0; j++) { P(cx + dx, cy + dy + j, chalk); P(cx + dx + w - 1, cy + dy + j, chalk); }
        }
        foreach (var (x, y) in new[] { (cx + 4, cy - 5), (cx + 13, cy - 5), (cx + 22, cy - 5), (cx + 22, cy - 14), (cx + 31, cy - 5), (cx + 49, cy - 5) }) { P(x, y, pink); P(x + 1, y + 1, pink); P(x, y + 2, pink); }
    }

    static void Litter()
    {
        // a few scraps of paper and leaves on the cobbles
        var paper = new[] { Rgba.Hex("E8DCC0"), Rgba.Hex("D8CCA8"), Rgba.Hex("C9BC9A") };
        for (int gy = 140; gy < G.LedgeY - 6; gy += 11)
            for (int gx = 4; gx < G.W; gx += 13)
            {
                int x = gx + (int)(Fx.R(gx, gy, 41) * 12), y = gy + (int)(Fx.R(gx, gy, 42) * 10);
                if (Fx.R(gx, gy, 43) > 0.09) continue;
                if (x < 20 || x > G.W - 20) continue;
                if (M[x, y] != Mat.Road && M[x, y] != Mat.Sidewalk) continue;
                var c = paper[(int)(Fx.H(gx, gy, 44) % 3)];
                P(x, y, c); P(x + 1, y, c); P(x + 1, y + 1, paper[1]); P(x + 2, y + 1, paper[2]);
            }
        var leaf = new[] { Rgba.Hex("B8873C"), Rgba.Hex("9C6A32"), Rgba.Hex("8A5A2C") };
        for (int gy = 140; gy < G.LedgeY - 6; gy += 9)
            for (int gx = 4; gx < G.W; gx += 11)
            {
                int x = gx + (int)(Fx.R(gx, gy, 45) * 10), y = gy + (int)(Fx.R(gx, gy, 46) * 8);
                if (Fx.R(gx, gy, 47) > 0.07 || x < 20 || x > G.W - 20) continue;
                if (M[x, y] != Mat.Road && M[x, y] != Mat.Sidewalk) continue;
                P(x, y, leaf[(int)(Fx.H(gx, gy, 48) % 3)]); P(x + 1, y, leaf[1]); P(x, y + 1, leaf[2]);
            }
    }

    // ---- south edge of the street, stone coping, masonry wall and the stairwell ---------------------------------
    static void SouthEdge()
    {
        // coping along the ledge (top surface lit, face shaded)
        for (int x = 0; x < G.W; x++)
        {
            if (x >= G.StairX0 - 6 && x < G.StairX1 + 6) continue;
            int blk = x / 20, jx = 3 + (int)(Fx.R(blk, 1, 61) * 14);
            bool joint = x % 20 == jx;
            int tone = 3 + (int)(Fx.H(blk, 2, 62) % 2);
            for (int j = 0; j < 4; j++) P(x, G.LedgeY + j, joint ? K.Stone[1] : K.Stone[j == 0 ? Math.Min(5, tone + 1) : tone], Mat.Ledge);
            P(x, G.LedgeY + 4, K.Stone[1], Mat.Ledge);
            P(x, G.LedgeY + 5, joint ? K.Stone[0] : K.Stone[2], Mat.Ledge);
            P(x, G.LedgeY + 6, K.Stone[0], Mat.Ledge);
            for (int j = 7; j < 10; j++) P(x, G.LedgeY + j, ((x + j) & 1) == 0 ? K.Stone[0] : K.Stone[1], Mat.Ledge);
        }
        // masonry (the north wall of the cellar seen from inside)
        for (int y = G.LedgeY + 10; y < G.VaultFloorY; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (x >= G.StairX0 - 6 && x < G.StairX1 + 6) continue;
                P(x, y, WallBlockPx(x, y), Mat.Masonry);
            }
        // vault ribs: lighter piers with a capital and a plinth
        foreach (var rx in new[] { 62, 190, 330, 580, 690 })
            Rib(rx);
        StairWell();
    }

    static Rgba WallBlockPx(int x, int y)
    {
        int y0 = G.LedgeY + 10, rowH = 8;
        int row = (y - y0) / rowH, ly = (y - y0) % rowH;
        int off = (int)(Fx.R(row, 0, 71) * 18);
        int gx = x + off, id = 0, sx = 0;
        while (true)
        {
            int w = 16 + (int)(Fx.R(row, id, 72) * 14);
            if (gx < sx + w) break;
            sx += w; id++;
        }
        int lx = gx - sx;
        uint h = Fx.H(row, id, 73);
        int idx = 1 + (int)(h % 3);
        double mottle = Fx.VN(x / 7.0, y / 5.0, 74);
        if (mottle > 0.72) idx++; else if (mottle < 0.22) idx--;
        if (ly == 0) idx++;
        if (ly == rowH - 1) idx--;
        if (lx == 0) return K.Mortar.Shade(0.6);
        if (ly == rowH - 1 && ((x + y) & 1) == 0) return K.Mortar.Shade(0.4);
        if (lx == 1) idx++;
        // damp streaks running down from the ledge
        if (Fx.VN(x / 3.0, y / 18.0, 75) > 0.84 && y < G.LedgeY + 40) idx--;
        return Rgba.Mul(K.Stone[Math.Clamp(idx, 0, 5)], 0.98, 0.96, 0.92);
    }

    static void Rib(int cx)
    {
        int w = 10, x0 = cx - w / 2;
        int y0 = G.LedgeY + 10, y1 = G.VaultFloorY;
        if (x0 + w > G.StairX0 - 6 && x0 < G.StairX1 + 6) return;
        for (int y = y0; y < y1; y++)
            for (int i = 0; i < w; i++)
            {
                int idx = i == 0 ? 4 : i == w - 1 ? 1 : i == 1 ? 3 : 2;
                if (y < y0 + 4) idx = Math.Min(5, idx + 1);
                if ((y - y0) % 9 == 8) idx = Math.Max(0, idx - 1);
                P(x0 + i, y, K.Stone[idx], Mat.Masonry);
            }
        // capital (wider) and plinth
        for (int i = -2; i < w + 2; i++) { P(x0 + i, y0 + 4, K.Stone[4], Mat.Masonry); P(x0 + i, y0 + 5, K.Stone[3], Mat.Masonry); P(x0 + i, y0 + 6, K.Stone[1], Mat.Masonry); }
        for (int i = -2; i < w + 2; i++) { P(x0 + i, y1 - 5, K.Stone[4], Mat.Masonry); P(x0 + i, y1 - 4, K.Stone[3], Mat.Masonry); P(x0 + i, y1 - 3, K.Stone[2], Mat.Masonry); P(x0 + i, y1 - 2, K.Stone[1], Mat.Masonry); P(x0 + i, y1 - 1, K.Stone[0], Mat.Masonry); }
    }

    static void StairWell()
    {
        int x0 = G.StairX0, x1 = G.StairX1;
        // cheek walls: lit cap with a dark inner edge, running the full length of the stair
        for (int y = G.LedgeY - 2; y < G.StairBot; y++)
            for (int i = -6; i < 0; i++)
            {
                int idx = i == -6 ? 4 : i == -5 ? 3 : i == -1 ? 0 : 2;
                if ((y - G.LedgeY) % 11 == 0) idx = 1;
                P(x0 + i, y, K.Stone[idx], Mat.Masonry);
                int j = i + 6;
                int idx2 = j == 0 ? 0 : j == 5 ? 1 : j == 4 ? 2 : 3;
                if ((y - G.LedgeY) % 11 == 5) idx2 = 1;
                P(x1 + j, y, K.Stone[Math.Clamp(idx2, 0, 5)], Mat.Masonry);
            }
        // paved threshold at street level, then the steps going down
        for (int y = G.LedgeY - 2; y < G.StairTop; y++)
            for (int x = x0; x < x1; x++) P(x, y, Rgba.Mul(CobblePx(x, y, true, 21), 0.97, 0.96, 0.98), Mat.Stair);
        int n = (G.StairBot - G.StairTop) / 5;
        for (int s = 0; s < n; s++)
        {
            int ys = G.StairTop + s * 5;
            for (int x = x0; x < x1; x++)
                for (int j = 0; j < 5; j++)
                {
                    Rgba c;
                    if (j == 0) c = K.Stone[5];
                    else if (j < 3) c = K.Stone[4 - (Fx.R(x / 6, s, 81) > 0.8 ? 1 : 0)];
                    else if (j == 3) c = K.Stone[2];
                    else c = K.Stone[0];
                    // worn centre / chipped corners
                    if (j >= 1 && j < 3 && x - x0 > 10 && x - x0 < 22 && Fx.R(x, s, 82) > 0.55) c = K.Stone[5];
                    P(x, ys + j, c, Mat.Stair);
                }
        }
        // iron handrail on the left cheek wall (just a short stub of posts and a rail)
        for (int y = G.StairTop + 4; y < G.StairBot - 8; y++) P(x0 + 1, y, K.Iron[(y % 12) < 2 ? 4 : 3], Mat.Stair);
        for (int y = G.StairTop + 4; y < G.StairBot - 8; y += 14) { P(x0, y, K.Iron[1], Mat.Stair); P(x0 + 1, y, K.Iron[4], Mat.Stair); P(x0 + 2, y, K.Iron[1], Mat.Stair); }
        // bottom landing: a dark threshold of timber
        for (int x = x0; x < x1; x++) { P(x, G.StairBot - 2, K.Wood[1], Mat.Stair); P(x, G.StairBot - 1, K.Wood[0], Mat.Stair); }
    }

    // ---- warm pools on the street (baked at the "dimmer" level; fx_c3_lampglow adds the rest before the blast) ----
    public static readonly (int x, int y)[] Lamps = { (96, 156), (330, 156), (560, 156), (704, 156), (384, 306) };

    static void LightsStreet()
    {
        Facades.RimLight();
        foreach (var (lx, ly) in Lamps)
            WarmPool(lx, ly + 2, 40, 20, 0.62);
        // window spill on the pavement
        foreach (var (wx, wy, ww, wh) in LitWindows)
            if (wy > 90) WarmPool(wx + ww / 2, G.BaseY + 8, 24, 10, 0.5, (x, y) => M[x, y] == Mat.Sidewalk);
        // stairwell: warm light climbs the steps from the cellar
        WarmPool((G.StairX0 + G.StairX1) / 2.0, G.StairBot - 6, 30, 44, 0.5, (x, y) => M[x, y] == Mat.Stair);
        // reflections in puddles (painted after grading so they stay luminous)
        foreach (var (cx, cy, rx, ry) in new[] { (190, 252, 16, 5), (560, 196, 22, 6), (660, 262, 14, 4), (330, 270, 10, 3) })
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (!Ell(x, y, cx, cy, rx + (Fx.R(x, y, 9) - 0.5) * 1.5, ry)) continue;
                    double t = (double)(y - (cy - ry)) / (2 * ry + 1);
                    var c = Dusk.Mix(Rgba.Hex("6C7CB0"), Rgba.Hex("2E3868"), t);
                    if (t > 0.1 && t < 0.35 && ((x + y) & 1) == 0) c = Dusk.Mix(c, Rgba.Hex("F0A468"), 0.6);
                    if (((x + y) & 1) == 0 && t > 0.55 && t < 0.7) c = Rgba.Hex("9AA6D0");
                    if (M[x, y] == Mat.Road) Cv.Set(x, y, c);
                }
    }
}
