namespace WorldArtCh3;

public static partial class Ground
{
    public static readonly (int cx, int cy)[] Vents = { (262, 346), (524, 346), (650, 346) };

    static void Vault()
    {
        RockFill();
        Floor();
        Walls();
        foreach (var (cx, cy) in Vents) Vent(cx, cy);
    }

    // ---- earth and rock beyond the vault ---------------------------------------------------------------------
    static void RockFill()
    {
        for (int y = G.LedgeY + 10; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (M[x, y] != Mat.Earth) continue;
                double a = Fx.Fbm(x / 16.0, y / 12.0, 91), b = Fx.VN(x / 5.0, y / 4.0, 92);
                double v = a * 0.85 + b * 0.15 + Fx.Bay(x, y) * 0.025;
                int idx = v < 0.38 ? 0 : v < 0.55 ? 1 : 2;
                var c = Rgba.Mul(K.Dark[idx], 0.8, 0.78, 0.84);
                if (Fx.R(x / 6, y / 5, 93) > 0.86 && Fx.VN(x / 5.0, y / 4.0, 94) > 0.45) c = Rgba.Mul(K.Stone[1 + (int)(Fx.R(x / 6, y / 5, 95) * 2)], 0.8, 0.78, 0.8);
                P(x, y, c, Mat.Earth);
            }
    }

    // ---- vault floor ------------------------------------------------------------------------------------------
    /// <summary>Large irregular flagstones laid in a loose running bond.</summary>
    static Rgba FlagPx(int x, int y)
    {
        const int rowH = 15;
        int ry = (y - G.VaultFloorY) / rowH, ly = (y - G.VaultFloorY) % rowH;
        int off = (int)(Fx.R(ry, 0, 133) * 30);
        int sx = -off, id = 0;
        int w;
        while (true)
        {
            w = 20 + (int)(Fx.R(ry, id, 134) * 14);
            if (x < sx + w) break;
            sx += w; id++;
        }
        int lx = x - sx;
        uint h = Fx.H(ry, id, 135);
        double tone = 0.80 + (h % 100) / 100.0 * 0.30;
        if (lx == 0 || ly == 0) return Rgba.Mul(K.Mortar, 0.65, 0.62, 0.7);
        int idx = 2;
        if (ly == 1 || lx == 1) idx = 3;
        else if (ly == rowH - 1 || lx == w - 1) idx = 1;
        else if (Fx.VN(x / 6.0, y / 5.0, 136) > 0.7) idx = 3;
        // chipped corners
        if ((lx < 3 && ly < 3) || (lx > w - 4 && ly > rowH - 4)) idx = 1;
        return Rgba.Mul(K.Stone[idx], tone, tone * 0.97, tone * 0.93);
    }

    static void Floor()
    {
        for (int y = G.VaultFloorY; y < G.VaultFloorEnd; y++)
            for (int x = G.VaultX0 + 8; x < G.VaultX1 - 8; x++)
            {
                var c = FlagPx(x, y);
                double dirt = Fx.Fbm(x / 16.0, y / 10.0, 35);
                if (dirt > 0.74)
                {
                    var d = K.Dirt[1 + (int)(Fx.R(x / 2, y / 2, 36) * 2)];
                    c = ((x + y) & 1) == 0 || dirt > 0.8 ? Rgba.Mul(d, 0.78, 0.74, 0.76) : c;
                }
                else if (dirt < 0.2 && ((x + y) & 1) == 0) c = Rgba.Mul(c, 0.86, 0.9, 0.84);
                if (y < G.VaultFloorY + 6 && Fx.VN(x / 4.0, y / 3.0, 37) > 0.6 && ((x + y) & 1) == 0) c = Rgba.Mul(K.Leaf[2], 0.85, 0.85, 0.85);
                P(x, y, c, Mat.VFloor);
            }
        for (int y = G.VaultFloorY; y < G.VaultFloorY + 9; y++)
            for (int x = G.VaultX0 + 8; x < G.VaultX1 - 8; x++)
            {
                double t = (y - G.VaultFloorY) / 9.0;
                if (t < 0.45 || (t < 0.8 && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(1.0 - t * 0.6));
            }
        for (int y = G.StairBot; y < G.StairBot + 10; y++)
            for (int x = G.StairX0; x < G.StairX1; x++)
            {
                int row = (y - G.StairBot) / 5, ly = (y - G.StairBot) % 5;
                int idx = ly == 4 ? 0 : ly == 0 ? 4 : 3;
                if ((x + row * 9) % 17 == 0) idx = 1;
                P(x, y, K.Wood[idx], Mat.VFloor);
            }
        for (int i = 0; i < 10; i++) { P(330 + i, 515, K.Iron[1], Mat.VFloor); P(330 + i, 516, K.Iron[(i % 2) * 2], Mat.VFloor); P(330 + i, 517, K.Iron[1], Mat.VFloor); }
        var rnd = new Random(5);
        foreach (var (x, y) in new[] { (180, 470), (560, 440), (620, 500) })
        {
            int cx = x, cy = y;
            for (int i = 0; i < 22; i++) { P(cx, cy, K.Dark[1], Mat.VFloor); cx += rnd.Next(3) - 1 + 1; if (rnd.Next(2) == 0) cy++; }
        }
    }

    // ---- vault side walls (stone cap seen from above), bottom wall -------------------------------------------------
    static void Walls()
    {
        for (int y = G.VaultFloorY - 4; y < G.VaultFloorEnd + 12; y++)
            for (int i = 0; i < 8; i++)
            {
                P(G.VaultX0 + i, y, CapPx(i, y, 8, 0), Mat.Masonry);
                P(G.VaultX1 - 8 + i, y, CapPx(i, y, 8, 1), Mat.Masonry);
            }
        for (int y = G.VaultFloorEnd; y < G.VaultFloorEnd + 12; y++)
            for (int x = G.VaultX0; x < G.VaultX1; x++)
            {
                int v = y - G.VaultFloorEnd;
                int idx = v == 0 ? 0 : v == 1 ? 4 : v < 7 ? 3 : 1;
                if (x % 18 == 0 && v > 1) idx = 1;
                if ((x + y) % 23 == 0 && v > 2 && v < 7) idx = 2;
                P(x, y, K.Stone[Math.Clamp(idx, 0, 5)], Mat.Masonry);
            }
    }

    static Rgba CapPx(int u, int v, int thick, int side)
    {
        int bl = 11, row = v / bl, lv = v % bl;
        int outer = side == 0 ? u : thick - 1 - u;
        int inner = thick - 1 - outer;
        int jx = 2 + (int)(Fx.R(row, side, 13) * (thick - 4));
        uint h = Fx.H(row, u > jx ? 1 : 0, 17);
        int idx = 3 + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        if (lv == bl - 1 || u == jx) return K.Stone[1];
        if (outer == 0) return K.Stone[0];
        if (inner == 0) return K.Stone[1];
        if (lv == 0) idx++;
        return K.Stone[Math.Clamp(idx, 0, 5)];
    }

    static void Vent(int cx, int cy)
    {
        int w = 16, h = 11, x0 = cx - w / 2, y0 = cy - h / 2;
        for (int i = -2; i < w + 2; i++) { P(x0 + i, y0 - 2, K.Stone[4], Mat.Masonry); P(x0 + i, y0 - 1, K.Stone[3], Mat.Masonry); P(x0 + i, y0 + h, K.Stone[1], Mat.Masonry); P(x0 + i, y0 + h + 1, K.Stone[0], Mat.Masonry); }
        for (int j = -2; j < h + 2; j++) { P(x0 - 2, y0 + j, K.Stone[3], Mat.Masonry); P(x0 - 1, y0 + j, K.Stone[2], Mat.Masonry); P(x0 + w, y0 + j, K.Stone[1], Mat.Masonry); P(x0 + w + 1, y0 + j, K.Stone[0], Mat.Masonry); }
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                var c = Dusk.Mix(Rgba.Hex("F4A468"), Rgba.Hex("7A5A90"), (double)j / h);
                P(x0 + i, y0 + j, c, Mat.Masonry);
            }
        for (int i = 2; i < w; i += 4) for (int j = 0; j < h; j++) P(x0 + i, y0 + j, K.Iron[1], Mat.Masonry);
        for (int i = 0; i < w; i++) P(x0 + i, y0 + h / 2, K.Iron[2], Mat.Masonry);
    }

    // ---- lantern pools in the vault (baked at a low level; fx_c3_lantern_glow lights it up when she shelters) -----
    public static readonly (int x, int y)[] VaultLamps = { (336, 470), (566, 468) };

    static void LightsVault()
    {
        foreach (var (lx, ly) in VaultLamps)
            WarmPool(lx, ly, 84, 50, 0.9, (x, y) => y >= G.VaultFloorY && M[x, y] == Mat.VFloor);
        foreach (var (cx, cy) in Vents) WarmPool(cx, cy + 36, 24, 18, 0.3, (x, y) => M[x, y] == Mat.VFloor);
    }
}
