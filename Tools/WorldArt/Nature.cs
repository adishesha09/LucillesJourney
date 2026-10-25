namespace WorldArt;

/// <summary>Border vegetation: trees, bushes, hedges, rocks, tall grass, flower patches, butterflies, low stone walls.</summary>
public static class Nature
{
    static Rgba Hx(string h) => Rgba.Hex(h);

    public static void Build()
    {
        S.Cur = "border";
        S.Add("prop_tree_broad", TreeBroad());
        S.Add("prop_tree_fruit", TreeFruit());
        S.Add("prop_tree_cypress", Cypress());
        S.Add("prop_bush_round", Bush(30, 22, false, 11));
        S.Add("prop_bush_berry", Bush(26, 20, true, 17));
        S.Add("prop_hedge", Hedge(32, 22, 5));
        S.Add("prop_hedge_short", Hedge(18, 22, 9));
        S.Add("prop_rock_a", Rock(20, 13, 3, true));
        S.Add("prop_rock_b", Rock(12, 8, 5, false));
        S.Add("prop_rock_c", Rock(28, 17, 8, true));
        S.Add("prop_grass_tall_a", TallGrass(14, 12, 2));
        S.Add("prop_grass_tall_b", TallGrass(18, 14, 6));
        S.Add("prop_wildflowers_a", Wildflowers(24, 11, 1, new[] { K.Cream, K.Gold, Hx("F6C6CF") }));
        S.Add("prop_wildflowers_b", Wildflowers(22, 11, 4, new[] { Hx("B8D2DC"), Hx("9F8CC4"), K.Cream }));
        S.Add("prop_stonewall_a", StoneWall(48, 18, 4));
        S.Add("prop_stonewall_b", StoneWall(32, 18, 9));
        S.Cur = "animated";
        for (int f = 0; f < 2; f++)
        {
            S.Add($"prop_butterfly_a_{f}", Butterfly(f, K.Gold, K.Orange));
            S.Add($"prop_butterfly_b_{f}", Butterfly(f, Hx("EAA5B5"), Hx("B8D2DC")));
            S.Add($"prop_flowers_sway_{f}", SwayFlowers(f));
        }
    }

    // ---- foliage helpers ----------------------------------------------------------------------------------
    static int RampIdx(Rgba[] r, Rgba c) => Array.IndexOf(r, c);

    /// <summary>Draw lobes (back to front), then darken the underside and the cast crease between lobes.</summary>
    static Canvas CrownCanvas(int w, int h, (double cx, double cy, double rx, double ry, double bias)[] lobes, Rgba[] ramp, int seed)
    {
        var c = new Canvas(w, h);
        int s = seed;
        foreach (var l in lobes) Draw.Blob(c, l.cx, l.cy, l.rx, l.ry, ramp, s++, l.bias, 3);
        // underside: pixels with transparent space two rows below go one tone darker
        var src = c.Clone();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!src.Opaque(x, y)) continue;
                bool under1 = !src.Opaque(x, y + 1), under2 = !src.Opaque(x, y + 2);
                var cur = src.Get(x, y); int i = RampIdx(ramp, cur);
                if (i < 0) continue;
                if (under1) c.Set(x, y, ramp[Math.Max(0, i - 2)]);
                else if (under2 && ((x + y) & 1) == 0) c.Set(x, y, ramp[Math.Max(0, i - 1)]);
                else if (!src.Opaque(x - 1, y - 1) && !src.Opaque(x, y - 1)) c.Set(x, y, ramp[Math.Min(ramp.Length - 1, i + 1)]);
            }
        // leaf clump sparkles on the lit upper-left of the crown
        for (int y = 2; y < h - 3; y++)
            for (int x = 2; x < w - 3; x++)
            {
                if (!c.Opaque(x, y) || Fx.R(x, y, seed + 40) > 0.045) continue;
                int i = RampIdx(ramp, c.Get(x, y)); if (i < 2) continue;
                var hi = ramp[Math.Min(ramp.Length - 1, i + 1)];
                c.Set(x, y, hi); c.Set(x + 1, y, hi);
                if (c.Opaque(x - 1, y + 1)) c.Set(x - 1, y + 1, ramp[Math.Max(0, i - 1)]);
                if (c.Opaque(x + 1, y + 1)) c.Set(x + 1, y + 1, ramp[Math.Max(0, i - 1)]);
            }
        return c;
    }

    static void TrunkRoots(Canvas c, int cx, int top, int bottom, int w)
    {
        var wd = K.Wood;
        int x0 = cx - w / 2;
        for (int y = top; y <= bottom; y++)
        {
            int flare = y >= bottom - 4 ? (bottom - y <= 1 ? 3 : 2) - (bottom - y) / 3 : 0;
            if (y > bottom - 3) flare = (y - (bottom - 3)) + 1;
            int xa = x0 - flare, xb = x0 + w - 1 + flare;
            for (int x = xa; x <= xb; x++)
            {
                double t = (double)(x - xa) / Math.Max(1, xb - xa);
                int idx = t < 0.2 ? 3 : t < 0.45 ? 2 : t < 0.75 ? 1 : 0;
                if (Fx.R(x, y / 3, 61) > 0.82 && idx > 0) idx--;
                if (idx > 0 && y % 7 == 3 && x == x0 + w / 2) idx--;
                c.Set(x, y, wd[idx]);
            }
        }
        // two bark furrows
        for (int y = top + 3; y < bottom - 2; y += 5) { c.Set(x0 + 2, y, wd[0]); c.Set(x0 + 2, y + 1, wd[0]); }
        for (int y = top + 6; y < bottom - 2; y += 6) { c.Set(x0 + w - 3, y, wd[0]); c.Set(x0 + w - 3, y + 1, wd[0]); }
    }

    static Canvas TreeBroad()
    {
        var c = new Canvas(68, 84);
        TrunkRoots(c, 33, 46, 82, 9);
        // branch stubs
        for (int i = 0; i < 6; i++) { c.Set(26 - i, 52 - i, K.Wood[1]); c.Set(27 - i, 52 - i, K.Wood[2]); c.Set(40 + i, 50 - i, K.Wood[1]); c.Set(39 + i, 50 - i, K.Wood[0]); }
        var lobes = new (double, double, double, double, double)[]
        {
            (34, 30, 31, 26, -0.05),
            (14, 40, 14, 12, -0.12), (54, 40, 14, 12, -0.18),
            (22, 22, 17, 15, 0.0), (47, 20, 17, 15, -0.08),
            (34, 12, 18, 12, 0.08),
            (34, 36, 20, 13, -0.16),
            (18, 32, 11, 9, 0.04), (50, 30, 10, 9, -0.05),
        };
        var crown = CrownCanvas(68, 84, lobes, K.Leaf, 100);
        // crown shade onto the trunk top
        for (int y = 44; y < 56; y++)
            for (int x = 24; x < 44; x++)
                if (!crown.Opaque(x, y) && c.Opaque(x, y) && y < 54) c.Set(x, y, c.Get(x, y).Shade(0.9));
        c.Blit(crown, 0, 0);
        return U.Finish(c, 0.34).Trim();
    }

    static Canvas TreeFruit()
    {
        var c = new Canvas(58, 72);
        TrunkRoots(c, 29, 42, 70, 8);
        for (int i = 0; i < 8; i++) { c.Set(25 - i / 2, 46 - i, K.Wood[1]); c.Set(33 + i / 2, 45 - i, K.Wood[0]); c.Set(24 - i / 2, 46 - i, K.Wood[2]); }
        var lobes = new (double, double, double, double, double)[]
        {
            (29, 28, 25, 22, 0.0),
            (13, 34, 11, 10, -0.1), (45, 34, 11, 10, -0.15),
            (20, 18, 13, 12, 0.02), (39, 17, 13, 12, 0.0),
            (29, 31, 17, 10, -0.18),
        };
        var crown = CrownCanvas(58, 72, lobes, K.LeafWarm, 130);
        c.Blit(crown, 0, 0);
        // red apples with a highlight, a few blossoms
        var pts = new[] { (12, 30), (20, 40), (30, 34), (40, 42), (46, 30), (24, 24), (36, 22), (16, 18), (44, 20), (30, 46), (8, 38) };
        foreach (var (x, y) in pts)
        {
            if (!c.Opaque(x, y)) continue;
            c.Rect(x, y, 3, 3, K.RedR[2]); c.Set(x, y, Rgba.Clear); c.Set(x + 2, y + 2, K.RedR[1]);
            c.Set(x, y, K.RedR[2]); c.Set(x + 1, y, K.RedR[4]); c.Set(x + 1, y + 1, K.RedR[3]); c.Set(x + 2, y + 2, K.RedR[0]);
        }
        return U.Finish(c, 0.34).Trim();
    }

    static Canvas Cypress()
    {
        var c = new Canvas(26, 72);
        TrunkRoots(c, 13, 64, 71, 4);
        var cr = new Canvas(26, 72);
        double[] cy = { 10, 18, 26, 34, 42, 50, 58 };
        double[] rx = { 4.5, 6.5, 8, 9, 10, 10.5, 9 };
        double[] ry = { 9, 9, 9, 9, 9, 9, 8 };
        for (int i = 0; i < cy.Length; i++)
            Draw.Blob(cr, 13 + (i % 2 == 0 ? 0.4 : -0.4), cy[i], rx[i], ry[i], K.Cypress, 150 + i, i * 0.01 - 0.05, 2);
        // vertical flame streaks
        for (int x = 4; x < 22; x++)
            for (int y = 2; y < 66; y++)
            {
                if (!cr.Opaque(x, y)) continue;
                int i = RampIdx(K.Cypress, cr.Get(x, y)); if (i < 0) continue;
                if ((x + y / 4) % 5 == 0 && i > 0 && Fx.R(x, y / 2, 155) > 0.35) cr.Set(x, y, K.Cypress[i - 1]);
                else if ((x + y / 5) % 6 == 2 && i < 4 && Fx.R(x, y / 3, 156) > 0.5) cr.Set(x, y, K.Cypress[i + 1]);
            }
        for (int y = 0; y < 72; y++)
            for (int x = 0; x < 26; x++)
                if (cr.Opaque(x, y) && !cr.Opaque(x, y + 1)) { var p = cr.Get(x, y); int i = RampIdx(K.Cypress, p); if (i > 0) cr.Set(x, y, K.Cypress[i - 1]); }
        c.Blit(cr, 0, 0);
        return U.Finish(c, 0.34).Trim();
    }

    static Canvas Bush(int w, int h, bool berries, int seed)
    {
        var c = new Canvas(w, h);
        var lobes = new (double, double, double, double, double)[]
        {
            (w / 2.0, h - 9, w / 2.0 - 1, 8.5, 0.0),
            (w * 0.28, h - 11, w * 0.26, 8, -0.05),
            (w * 0.7, h - 11, w * 0.26, 8, -0.1),
            (w * 0.5, h - 14, w * 0.3, 7.5, 0.05),
        };
        var crown = CrownCanvas(w, h, lobes, K.LeafWarm, seed);
        c.Blit(crown, 0, 0);
        if (berries)
        {
            var pts = new[] { (5, h - 12), (10, h - 8), (15, h - 14), (19, h - 9), (22, h - 12), (8, h - 16), (13, h - 6) };
            foreach (var (x, y) in pts) if (c.Opaque(x, y)) { c.Set(x, y, K.RedR[3]); c.Set(x + 1, y, K.RedR[2]); c.Set(x + 1, y + 1, K.RedR[0]); c.Set(x, y, K.RedR[4]); }
        }
        else
        {
            var pts = new[] { (6, h - 13), (14, h - 15), (21, h - 10), (10, h - 9) };
            foreach (var (x, y) in pts) if (c.Opaque(x, y)) { c.Set(x, y, K.Cream); c.Set(x + 1, y, Hx("F6C6CF")); c.Set(x, y + 1, K.Gold); }
        }
        return U.Finish(c, 0.34);
    }

    static Canvas Hedge(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        var lobes = new List<(double, double, double, double, double)>();
        for (double x = 5; x < w; x += 8) lobes.Add((x, h - 14, 7.5, 7, 0.0 + ((int)x % 2) * 0.03));
        for (double x = 4; x < w; x += 7) lobes.Add((x + 1, h - 8, 6.5, 6.5, -0.05));
        var crown = CrownCanvas(w, h, lobes.ToArray(), K.LeafWarm, seed);
        c.Blit(crown, 0, 0);
        // flat clipped bottom
        for (int x = 0; x < w; x++) { c.Set(x, h - 1, K.LeafWarm[0]); if (x % 2 == 0) c.Set(x, h - 2, K.LeafWarm[1]); }
        return U.Finish(c, 0.34);
    }

    static Canvas Rock(int w, int h, int seed, bool moss)
    {
        var c = new Canvas(w, h);
        double cx = w / 2.0, cy = h * 0.58, rx = w / 2.0 - 0.5, ry = h * 0.52;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry;
                double shape = nx * nx + ny * ny * (ny < 0 ? 1.0 : 1.5) + (Fx.R(x, y, seed) - 0.5) * 0.12;
                if (shape > 1) continue;
                double light = -(nx * 0.55 + ny * 0.7);
                double t = 0.52 + 0.45 * light + (Fx.R(x / 2, y / 2, seed + 3) - 0.5) * 0.2;
                int idx = Math.Clamp((int)(t * 6), 0, 5);
                if (ny > 0.55) idx = Math.Min(idx, 1);
                c.Set(x, y, Rgba.Mul(K.Stone[Math.Clamp(idx, 0, 5)], 1.02, 0.97, 0.92));
            }
        if (moss)
            for (int y = 0; y < h / 2 + 1; y++)
                for (int x = 0; x < w; x++)
                    if (c.Opaque(x, y) && Fx.VN(x / 2.5, y / 2.0, seed + 9) > 0.62 && y < h * 0.55) c.Set(x, y, y == 0 || !c.Opaque(x, y - 1) ? K.LeafWarm[4] : K.LeafWarm[(x + y) % 3 == 0 ? 3 : 2]);
        // crack
        if (w > 15) { c.Line(w / 2, 3, w / 2 + 2, 7, K.Stone[0]); }
        return U.Finish(c, 0.4);
    }

    static Canvas TallGrass(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        int blades = w / 2 + 1;
        for (int b = 0; b < blades; b++)
        {
            int bx = 1 + b * (w - 3) / Math.Max(1, blades - 1);
            int bh = h - 3 - (int)(Fx.R(b, 0, seed) * (h * 0.5));
            int lean = (int)(Fx.R(b, 1, seed) * 5) - 2;
            for (int j = 0; j < bh; j++)
            {
                int x = bx + lean * j / Math.Max(1, bh) , y = h - 1 - j;
                int idx = j > bh * 0.65 ? 5 : j > bh * 0.3 ? 4 : 2;
                c.Set(x, y, K.Grass[Math.Clamp(idx, 0, 6)]);
                if (j < bh * 0.5) c.Set(x + 1, y, K.Grass[1]);
            }
            if (b % 3 == 0) { int x = bx + lean, y = h - bh - 1; c.Set(x, y, K.Gold); c.Set(x, y - 1, Hx("E0B040")); }
        }
        // dark base
        for (int x = 0; x < w; x++) if (Fx.R(x, 0, seed + 2) > 0.3) c.Set(x, h - 1, K.Grass[0]);
        return c;
    }

    static Canvas Wildflowers(int w, int h, int seed, Rgba[] cols)
    {
        var c = new Canvas(w, h);
        int n = w / 3;
        for (int i = 0; i < n; i++)
        {
            int x = 1 + (int)(Fx.R(i, 0, seed) * (w - 3)), y = 3 + (int)(Fx.R(i, 1, seed) * (h - 4));
            for (int j = y + 1; j < h && j <= y + 3; j++) c.Set(x, j, K.Grass[1 + (j - y) % 2]);
            c.Set(x - 1, y + 2, K.Grass[3]); c.Set(x + 1, y + 1, K.Grass[4]);
            var col = cols[i % cols.Length];
            c.Set(x, y, col); c.Set(x - 1, y, col); c.Set(x + 1, y, col); c.Set(x, y - 1, col);
            c.Set(x, y, i % 2 == 0 ? K.Gold : K.Cream);
        }
        for (int x = 0; x < w; x++) if (Fx.R(x, 3, seed + 5) > 0.5) c.Set(x, h - 1, K.Grass[1]);
        return c;
    }

    static Canvas StoneWall(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        // cap course (lit top), body (two courses), shadowed foot
        int capH = 5;
        int x = 0, id = 0;
        while (x < w)
        {
            int sw = 9 + (int)(Fx.R(id, 0, seed) * 6);
            sw = Math.Min(sw, w - x); if (w - x - sw < 5) sw = w - x;
            for (int j = 0; j < capH; j++)
                for (int i = 0; i < sw; i++)
                {
                    int idx = j == 0 ? 5 : j == 1 ? 4 : j == capH - 1 ? 2 : 3;
                    if (i == 0) idx = Math.Max(1, idx - 2); else if (i == sw - 1) idx = Math.Max(1, idx - 1);
                    c.Set(x + i, j + 1, Rgba.Mul(K.Stone[idx], 1.03, 0.98, 0.9));
                }
            x += sw; id++;
        }
        for (int row = 0; row < 2; row++)
        {
            int y0 = capH + 1 + row * 5, rh = row == 1 ? h - y0 : 5;
            int off = row == 0 ? 4 : 0;
            int sx = -off; int sid = 0;
            while (sx < w)
            {
                int sw = 8 + (int)(Fx.R(sid, row + 1, seed) * 7);
                for (int j = 0; j < rh; j++)
                    for (int i = 0; i < sw; i++)
                    {
                        int xx = sx + i; if (xx < 0 || xx >= w) continue;
                        int idx = 2 + (int)(Fx.H(sid, row, seed) % 2);
                        if (i == 0 || j == rh - 1) idx = 0; else if (j == 0) idx = idx + 1; else if (i == sw - 1) idx = 1;
                        if (row == 1 && j >= rh - 2) idx = Math.Min(idx, 1);
                        var col = Rgba.Mul(K.Stone[idx], 1.02, 0.97, 0.9);
                        if (j == 0 && i > 0 && Fx.R(sid, 5, seed) > 0.7) col = K.LeafWarm[2];
                        c.Set(xx, y0 + j, col);
                    }
                sx += sw; sid++;
            }
        }
        // moss tufts on top
        for (int i = 2; i < w - 2; i += 7) { c.Set(i, 0, K.LeafWarm[3]); c.Set(i + 1, 0, K.LeafWarm[4]); c.Set(i, 1, K.LeafWarm[2]); }
        return U.Finish(c, 0.4);
    }

    // ---- animated -----------------------------------------------------------------------------------------
    static Canvas Butterfly(int f, Rgba wing, Rgba edge)
    {
        var c = new Canvas(8, 8);
        var body = K.Brown3;
        if (f == 0)
        {   // wings open
            c.Rect(0, 1, 3, 3, wing); c.Rect(5, 1, 3, 3, wing); c.Rect(1, 4, 2, 2, edge); c.Rect(5, 4, 2, 2, edge);
            c.Set(0, 1, Rgba.Clear); c.Set(7, 1, Rgba.Clear); c.Set(1, 2, K.Cream); c.Set(6, 2, K.Cream);
            c.Rect(3, 1, 2, 5, body); c.Set(3, 0, body); c.Set(4, 0, body);
        }
        else
        {   // wings raised
            c.Rect(2, 0, 2, 4, wing); c.Rect(4, 0, 2, 4, wing); c.Rect(2, 4, 1, 2, edge); c.Rect(5, 4, 1, 2, edge);
            c.Rect(3, 2, 2, 5, body); c.Set(2, 0, Rgba.Clear); c.Set(5, 0, Rgba.Clear); c.Set(3, 1, K.Cream);
        }
        return c;
    }

    static Canvas SwayFlowers(int f)
    {
        var c = new Canvas(22, 22);
        var cols = new[] { Hx("EAA5B5"), K.Cream, K.Gold, Hx("B8A0E0"), Hx("F4B07A") };
        int[] bx = { 3, 8, 12, 16, 19 };
        int[] hh = { 13, 17, 14, 18, 12 };
        for (int i = 0; i < bx.Length; i++)
        {
            double amp = (i % 2 == 0 ? 1.0 : -1.0) * (f == 0 ? 1.6 : -1.6);
            int baseY = 21;
            int Off(int j) => (int)Math.Round(amp * (j / (double)hh[i]) * (j / (double)hh[i]));
            for (int j = 0; j < hh[i]; j++)
            {
                int x = bx[i] + Off(j), y = baseY - j;
                c.Set(x, y, j < 3 ? K.Grass[1] : K.Grass[3]);
                if (j == 3 || j == 7) { c.Set(x - 1, y - 1, K.Grass[4]); c.Set(x + 1, y - 1, K.Grass[2]); c.Set(x - 2, y, K.Grass[3]); }
            }
            int tx = bx[i] + Off(hh[i]), ty = baseY - hh[i];
            var col = cols[i];
            c.Rect(tx - 1, ty - 1, 3, 3, col); c.Set(tx - 1, ty - 1, Rgba.Clear); c.Set(tx + 1, ty + 1, Rgba.Clear);
            c.Set(tx, ty, i % 2 == 0 ? K.Gold : K.Orange);
            c.Set(tx, ty - 2, col); c.Set(tx - 2, ty, col); c.Set(tx + 2, ty, col); c.Set(tx, ty + 2, col);
        }
        return c;
    }
}
