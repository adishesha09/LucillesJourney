namespace WorldArt;

/// <summary>Vegetable garden: raised beds (dry/watered), barrel, compost, scarecrow, fence, gate, tools.</summary>
public static class Garden
{
    static Rgba Hx(string h) => Rgba.Hex(h);
    public const int BedW = 76, D = 20, BedH = 56 + D;

    public static void Build()
    {
        S.Cur = "garden";
        string[] crops = { "tomato", "corn", "greens", "carrot" };
        for (int i = 0; i < 4; i++)
        {
            S.Add($"prop_bed_{i + 1}_dry", Bed(crops[i], false, i));
            S.Add($"prop_bed_{i + 1}_watered", Bed(crops[i], true, i));
        }
        S.Add("prop_rain_barrel", RainBarrel());
        S.Add("prop_compost", Compost());
        S.Add("prop_scarecrow", Scarecrow());
        S.Add("prop_fence_h", FenceH());
        S.Add("prop_fence_v", FenceV());
        S.Add("prop_fence_post", FencePost());
        S.Add("prop_gate", Gate());
        S.Add("prop_watering_can", WateringCan());
        S.Add("prop_bucket", Bucket());
        S.Add("prop_basket", BasketApples());
        S.Add("prop_trellis", Trellis());
    }

    // ---- raised beds --------------------------------------------------------------------------------------
    static Canvas Bed(string crop, bool wet, int seed)
    {
        var c = new Canvas(BedW, BedH);
        var wd = K.Pine;
        // soil surface
        for (int y = 24; y < 50 + D; y++)
            for (int x = 0; x < BedW; x++) c.Set(x, y, wd[2]);
        for (int y = 26; y < 48 + D; y++)
            for (int x = 2; x < BedW - 2; x++) c.Set(x, y, SoilPx(x, y, wet, seed));
        // inner shadow cast by the back and left boards
        for (int x = 2; x < BedW - 2; x++) { c.Set(x, 26, wet ? K.WetSoil2 : K.Dirt[1]); if (x % 2 == 0) c.Set(x, 27, wet ? K.WetSoil2 : K.Dirt[2]); }
        for (int y = 26; y < 48 + D; y++) { c.Set(2, y, wet ? K.WetSoil2 : K.Dirt[1]); }
        // rims (top faces of the boards)
        for (int x = 0; x < BedW; x++) { c.Set(x, 24, wd[4]); c.Set(x, 25, wd[3]); }
        for (int y = 24; y < 50 + D; y++) { c.Set(0, y, wd[4]); c.Set(1, y, wd[3]); c.Set(BedW - 2, y, wd[2]); c.Set(BedW - 1, y, wd[1]); }
        for (int x = 0; x < BedW; x++) { c.Set(x, 48 + D, wd[4]); c.Set(x, 49 + D, wd[3]); }
        // front face: two planks
        for (int y = 50 + D; y < 56 + D; y++)
            for (int x = 0; x < BedW; x++)
            {
                int ly = y - D;
                int idx = ly == 50 ? 3 : ly == 55 ? 0 : ly == 52 ? 1 : 2;
                if (x == 0) idx = Math.Min(4, idx + 1); if (x == BedW - 1) idx = Math.Max(0, idx - 1);
                if ((ly == 51 || ly == 53 || ly == 54) && Fx.R(x / 5, y, seed + 70) > 0.8) idx = Math.Max(0, idx - 1);
                c.Set(x, y, wd[idx]);
            }
        foreach (var nx in new[] { 4, BedW - 5 }) { c.Set(nx, 51 + D, K.Iron[3]); c.Set(nx, 54 + D, K.Iron[3]); }
        c.VL(BedW / 2, 50 + D, 6, wd[1]);
        // corner posts
        foreach (var px in new[] { 0, BedW - 3 })
            for (int y = 21; y < 56 + D; y++)
                for (int i = 0; i < 3; i++) c.Set(px + i, y, y == 21 ? wd[4] : i == 0 ? wd[4] : i == 2 ? wd[1] : wd[3]);

        switch (crop)
        {
            case "tomato": Tomatoes(c, wet); break;
            case "corn": Corn(c, wet); break;
            case "greens": Greens(c, wet); break;
            default: Carrots(c, wet); break;
        }
        if (wet) Drops(c, seed);
        return U.Finish(c, 0.38);
    }

    static Rgba SoilPx(int x, int y, bool wet, int seed)
    {
        int row = (y - 26) / 7, ly = (y - 26) % 7;      // 3 gentle furrow rows
        double n = Fx.VN(x / 3.0, y / 2.0, 200 + seed);
        if (wet)
        {
            Rgba c = ly == 0 ? K.WetSoilHi : ly <= 2 ? K.WetSoil : ly == 6 ? K.WetSoil2 : K.WetSoil;
            if (n > 0.78) c = K.WetSoilHi; else if (n < 0.22 && ly > 2) c = K.WetSoil2;
            if (Fx.R(x, y, 210 + seed) > 0.985) c = Hx("8FC9D6");          // glistening water
            else if (Fx.R(x, y, 211 + seed) > 0.975) c = K.WetSoilHi;
            return c;
        }
        else
        {
            Rgba c = ly == 0 ? K.Dirt[5] : ly <= 2 ? K.Dirt[4] : ly == 6 ? K.Dirt[2] : K.Dirt[3];
            if (n > 0.75) c = K.Dirt[5]; else if (n < 0.2) c = K.Dirt[2];
            // dry crack lines
            if (((x * 3 + y * 5 + seed) % 19 == 0 && ly > 2 && ly < 6) || ((x + y * 7) % 23 == 0 && Fx.R(x, y, 215) > 0.4)) c = K.Dirt[1];
            if (Fx.R(x, y, 212 + seed) > 0.985) c = Hx("E8D9B5");
            return c;
        }
    }

    static void Drops(Canvas c, int seed)
    {
        var pts = new List<(int x, int y)>();
        for (int i = 0; i < 18; i++)
            pts.Add((5 + (int)(Fx.R(i, 1, 300 + seed) * 66), 6 + (int)(Fx.R(i, 2, 300 + seed) * (32 + D))));
        foreach (var (x, y) in pts)
        {
            if (c.Opaque(x, y)) { c.Set(x, y, K.WaterHi); continue; }      // dew on a leaf
            c.Set(x, y, K.WaterHi); c.Set(x, y + 1, K.Water);
        }
    }

    static void Blob(Canvas c, double cx, double cy, double rx, double ry, Rgba[] r, int seed, double bias = 0.0) =>
        Draw.Blob(c, cx, cy, rx, ry, r, seed, bias, 2);

    static void Tomatoes(Canvas c, bool wet)
    {
        TomatoRow(c, wet, new[] { 11, 29, 47, 65 }, 42, 0);
        TomatoRow(c, wet, new[] { 20, 38, 56 }, 64, 10);
    }

    static readonly Rgba[] DryLeaf = { Hx("5F7A35"), Hx("7A9440"), Hx("93AB4D"), Hx("A9BE5C"), Hx("C0CF73"), Hx("D6DC8C") };

    static void TomatoRow(Canvas c, bool wet, int[] xs, int by, int seedOff)
    {
        for (int i = 0; i < xs.Length; i++)
        {
            int sx = xs[i], top = wet ? by - 30 : by - 25;
            // stake with tie ribbons
            for (int y = top - 3; y <= by; y++) { c.Set(sx, y, K.Pine[3]); c.Set(sx + 1, y, K.Pine[1]); }
            c.Set(sx, top - 4, K.Pine[4]); c.Set(sx + 1, top - 4, K.Pine[2]);
            var ramp = wet ? K.Leaf : DryLeaf;
            int lean = wet ? 0 : (i % 2 == 0 ? 1 : -1);
            // foliage: spreading skirt, bushy body, small crown
            Blob(c, sx + 0.5, by - 5, 9.5, 5, ramp, 400 + i + seedOff, -0.1);
            Blob(c, sx + 0.5 + lean, by - 13, wet ? 8.5 : 7.5, wet ? 8 : 6.5, ramp, 410 + i + seedOff, 0.0);
            Blob(c, sx + 0.5 + lean * 2, top + 5.5, wet ? 6 : 5, wet ? 5.5 : 4.5, ramp, 420 + i + seedOff, 0.05);
            if (!wet)
            {   // yellowing, brown-tipped leaves
                foreach (var (dx, dy) in new[] { (-9, -4), (9, -5), (-6, -14), (7, -17), (-3, -8) })
                { c.Set(sx + dx, by + dy, K.Dirt[1]); c.Set(sx + dx + Math.Sign(dx), by + dy + 1, K.Dirt[0]); c.Set(sx + dx - Math.Sign(dx), by + dy, Hx("C9B04A")); }
            }
            // tomatoes
            var fruit = wet ? new[] { (-6, top + 13), (5, top + 9), (-4, top + 19), (6, top + 17) } : new[] { (-5, top + 14), (5, top + 19) };
            foreach (var (dx, dy) in fruit)
            {
                int x = sx + dx, y = dy;
                var col = wet ? K.RedR[2] : (dy % 2 == 0 ? K.Orange : Hx("C9B04A"));
                c.Rect(x, y, 3, 3, col); c.Set(x, y, Rgba.Clear); c.Set(x + 2, y + 2, wet ? K.RedR[0] : K.Terra);
                c.Set(x, y, col); c.Set(x + 1, y, wet ? K.RedR[4] : Hx("F2D26B")); c.Set(x + 1, y - 1, K.DGreen);
                if (wet) c.Set(x + 1, y + 1, K.RedR[3]);
            }
        }
    }

    static void Corn(Canvas c, bool wet)
    {
        var ramp = wet ? K.Leaf : DryLeaf;
        CornRow(c, wet, ramp, new[] { 8, 20, 32, 44, 56, 68 }, 40, 0);
        CornRow(c, wet, ramp, new[] { 14, 26, 38, 50, 62 }, 56, 1);
        // front row: low bean bushes with pods
        for (int i = 0; i < 6; i++)
        {
            double bx = 9 + i * 12.5;
            Blob(c, bx, 67, 6, wet ? 3.6 : 2.8, ramp, 450 + i, 0.0);
            if (wet) { c.Set((int)bx - 2, 66, K.Cream); c.Set((int)bx + 2, 68, Hx("F6C6CF")); }
            c.VL((int)bx + 1, 69, 2, wet ? Hx("9CC873") : Hx("B9C768"));
        }
    }

    static void CornRow(Canvas c, bool wet, Rgba[] ramp, int[] xs, int baseY, int rowSeed)
    {
        for (int i = 0; i < xs.Length; i++)
        {
            int sx = xs[i] + (i % 2), by = baseY - (i % 2) * 2;
            int h = (wet ? 34 : 28) - (i % 3) * 2 - rowSeed * 3;
            for (int j = 0; j < h; j++) { c.Set(sx, by - j, ramp[j % 6 == 0 ? 2 : 3]); c.Set(sx + 1, by - j, ramp[1]); }
            // blades arcing outwards, alternating sides
            for (int k = 0; k < 5; k++)
            {
                int y0 = by - 5 - k * 5; int dir = k % 2 == 0 ? -1 : 1;
                int len = wet ? 9 - k : 8 - k;
                if (len < 3) continue;
                for (int t = 1; t <= len; t++)
                {
                    int x = sx + (dir > 0 ? 1 : 0) + dir * t;
                    double arc = wet ? -t * 0.35 + t * t * 0.05 : t * 0.55;
                    int y = y0 + (int)Math.Round(arc);
                    c.Set(x, y, ramp[t < len / 2 ? 3 : 4]); c.Set(x, y + 1, ramp[1]);
                    if (!wet && t == len) c.Set(x, y + 1, K.Dirt[1]);
                }
            }
            // husk and cob
            int cy = by - h * 2 / 3;
            int cd = i % 2 == 0 ? 1 : -1;
            int cx = sx + (cd > 0 ? 2 : -2);
            c.Rect(cx - (cd < 0 ? 0 : 0), cy, 2, 5, ramp[1]); c.VL(cx, cy, 5, ramp[3]);
            c.Set(cx + (cd > 0 ? 0 : 1), cy - 1, K.Gold); c.Set(cx, cy, K.Gold); c.Set(cx + 1, cy + 1, Hx("E0B040"));
            // tassel
            int ty = by - h;
            for (int t = 0; t < 4; t++) { c.Set(sx - 2 + t * 1, ty + (t == 0 || t == 3 ? 1 : 0), K.Parch); }
            c.Set(sx, ty - 1, K.Gold); c.Set(sx + 1, ty - 1, K.Sand);
        }
    }

    static void Greens(Canvas c, bool wet)
    {
        var green = wet ? K.Leaf : DryLeaf;
        var red = new[] { Hx("4E2A45"), Hx("6E3A55"), Hx("8F4A64"), Hx("B2667A"), Hx("D08B94"), Hx("E8B0B0") };
        for (int row = 0; row < 4; row++)
            for (int i = 0; i < 6; i++)
            {
                double cx = 7 + i * 12.2 + (row % 2 == 1 ? 6 : 0), cy = 35 + row * 10;
                if (cx > 70) continue;
                bool redHead = (i + row * 2) % 4 == 1;
                var ramp = redHead ? red : green;
                double rx = wet ? 6.3 : 6.6, ry = wet ? 5 : 3.3;
                if (!wet) cy += 1;
                Blob(c, cx, cy, rx, ry, ramp, 500 + i + row * 7, wet ? 0.05 : -0.05);
                // curled heart of the head
                int hx = (int)cx, hy = (int)(cy - (wet ? 1.5 : 0.5));
                c.Set(hx - 1, hy, ramp[4]); c.Set(hx, hy - 1, ramp[4]); c.Set(hx + 1, hy, ramp[4]); c.Set(hx, hy, ramp[3]);
                c.Set(hx - 2, hy + 1, ramp[2]); c.Set(hx + 2, hy + 1, ramp[1]);
                if (wet) { c.Set(hx - 3, hy - 2, ramp[4]); c.Set(hx + 3, hy - 3, ramp[4]); }
                else { c.Set((int)(cx - rx), hy + 2, K.Dirt[1]); c.Set((int)(cx + rx - 1), hy + 2, K.Dirt[1]); }
            }
        // chive tufts along the back edge
        for (int i = 0; i < 5; i++)
        {
            int x = 6 + i * 15;
            for (int j = 0; j < (wet ? 7 : 4); j++) { c.Set(x + (j % 2), 29 - j, green[3]); c.Set(x + 2 - (j > 3 ? 1 : 0), 29 - j + 1, green[1]); }
            if (wet) { c.Set(x, 21, Hx("B8A0E0")); c.Set(x + 1, 21, Hx("D4C0F0")); }
        }
    }

    static void Carrots(Canvas c, bool wet)
    {
        var ramp = wet ? K.Leaf : DryLeaf;
        for (int row = 0; row < 5; row++)
            for (int i = 0; i < 8; i++)
            {
                int bx = 7 + i * 9 + (row % 2) * 4, by = 31 + row * 8;
                if (bx > 70) continue;
                int len = wet ? 12 - row / 2 : 9 - (row % 2);
                // feathery fronds: five strokes fanning out
                int[] dxs = wet ? new[] { -3, -1, 0, 1, 3 } : new[] { -5, -3, 0, 3, 5 };
                for (int k = 0; k < dxs.Length; k++)
                {
                    int dx = dxs[k];
                    int l = len - (k == 0 || k == 4 ? 3 : k == 2 ? 0 : 1);
                    for (int t = 0; t < l; t++)
                    {
                        double fx = wet ? dx * t / (double)len : dx * Math.Sin(Math.Min(1.0, t / (double)l) * 1.3) * 0.9;
                        double fy = wet ? t : t * 0.55 + (dx != 0 ? Math.Abs(dx) * 0.25 * t / l : 0);
                        int x = bx + (int)Math.Round(fx), y = by - (int)Math.Round(fy);
                        c.Set(x, y, ramp[t < l / 2 ? 2 : (t == l - 1 ? 5 : 4)]);
                        if (wet && t % 3 == 1) c.Set(x + (dx >= 0 ? 1 : -1), y, ramp[3]);
                        if (!wet && t == l - 1 && dx != 0) c.Set(x, y + 1, K.Dirt[1]);
                    }
                }
                // orange shoulder peeking out of the soil
                if ((i + row) % 2 == 0) { c.Set(bx - 1, by + 1, K.Orange); c.Set(bx, by + 1, Hx("F4B07A")); c.Set(bx + 1, by + 1, K.Orange); c.Set(bx, by + 2, Hx("B8662F")); }
            }
    }

    // ---- barrel, compost, scarecrow ----------------------------------------------------------------------
    static Canvas RainBarrel()
    {
        var c = new Canvas(24, 30);
        var w = K.Wood;
        // body staves (slightly bulging)
        for (int y = 7; y < 29; y++)
        {
            double t = (y - 7) / 21.0; double bulge = 1.2 * Math.Sin(t * Math.PI);
            int half = (int)Math.Round(8.5 + bulge);
            for (int x = 12 - half; x < 12 + half; x++)
            {
                double u = (x - (12 - half)) / (2.0 * half);
                int idx = u < 0.18 ? 4 : u < 0.4 ? 3 : u < 0.7 ? 2 : u < 0.88 ? 1 : 0;
                if ((x - (12 - half)) % 4 == 3) idx = Math.Max(0, idx - 1);
                c.Set(x, y, w[idx]);
            }
        }
        // iron hoops
        foreach (var y in new[] { 10, 11, 24, 25 })
        {
            double t = (y - 7) / 21.0; int half = (int)Math.Round(8.5 + 1.2 * Math.Sin(t * Math.PI));
            for (int x = 12 - half; x < 12 + half; x++) c.Set(x, y, y % 2 == 0 || y == 11 && false ? K.Iron[3] : K.Iron[1]);
            c.Set(12 - half, y, K.Iron[4]);
        }
        // open top with water
        c.Ellipse(12, 8, 9.5, 4.5, w[4]);
        c.Ellipse(12, 8.3, 8.2, 3.5, K.Dark[1]);
        c.Ellipse(12, 8.6, 7.6, 3.0, K.Water);
        c.Rect(8, 7, 5, 1, K.WaterHi); c.HL(14, 9, 3, Hx("6FB0C0")); c.Set(10, 8, K.WaterHi);
        // wooden tap
        c.Rect(2, 19, 3, 2, K.Iron[3]); c.Set(1, 21, K.Iron[2]); c.Set(1, 22, K.Water);
        // drip stain on the staves
        c.VL(6, 12, 8, w[0]); c.VL(18, 13, 6, w[0]);
        return U.Finish(c, 0.38);
    }

    static Canvas Compost()
    {
        var c = new Canvas(36, 26);
        var w = K.Wood;
        // back planks
        c.Rect(0, 0, 36, 10, w[1]); c.HL(0, 0, 36, w[3]); c.HL(0, 4, 36, w[0]); c.HL(0, 5, 36, w[2]); c.HL(0, 9, 36, w[0]);
        for (int x = 3; x < 36; x += 7) c.Set(x, 2, w[0]);
        // heap
        for (int y = 3; y < 22; y++)
            for (int x = 1; x < 35; x++)
            {
                double nx = (x - 18) / 17.0, ny = (y - 20) / 17.0;
                double s = nx * nx * 0.9 + ny * ny * 1.1;
                if (y < 6 + Math.Abs(nx) * 10 || s > 1) continue;
                double r = Fx.VN(x / 2.0, y / 2.0, 520);
                Rgba col = r < 0.25 ? K.Brown2 : r < 0.5 ? K.Soil : r < 0.7 ? K.SoilDark : r < 0.82 ? K.SoilLight : K.Brown;
                if (y < 12 && Fx.R(x, y, 521) > 0.5) col = K.SoilLight;
                c.Set(x, y, col);
            }
        // scraps: leaves, peel, eggshell, straw
        foreach (var (x, y) in new[] { (9, 13), (20, 9), (26, 15), (14, 17), (30, 18) }) { c.Set(x, y, K.Green); c.Set(x + 1, y, K.DGreen); }
        foreach (var (x, y) in new[] { (12, 10), (23, 13), (17, 14) }) { c.Set(x, y, K.Orange); c.Set(x + 1, y, Hx("F4B07A")); }
        foreach (var (x, y) in new[] { (16, 8), (8, 17) }) { c.Set(x, y, K.Cream); c.Set(x + 1, y, K.Parch); }
        for (int x = 6; x < 30; x += 4) { c.Set(x, 7 + (x % 3), K.Gold); c.Set(x + 1, 8 + (x % 3), Hx("E0B040")); }
        // front boards (open gap shows the heap)
        c.Rect(0, 19, 36, 4, w[2]); c.HL(0, 19, 36, w[4]); c.HL(0, 22, 36, w[0]);
        c.Rect(0, 23, 36, 3, w[1]); c.HL(0, 23, 36, w[2]); c.HL(0, 25, 36, w[0]);
        for (int x = 4; x < 36; x += 9) { c.Set(x, 20, w[0]); c.Set(x, 24, w[0]); }
        // corner posts
        foreach (var px in new[] { 0, 33 }) for (int y = 0; y < 26; y++) { c.Set(px, y, w[4]); c.Set(px + 1, y, w[3]); c.Set(px + 2, y, w[0]); }
        return U.Finish(c, 0.38);
    }

    static Canvas Scarecrow()
    {
        var c = new Canvas(26, 46);
        var w = K.Wood; var st = K.Mustard;
        // pole and crossbar
        for (int y = 12; y < 45; y++) { c.Set(12, y, w[3]); c.Set(13, y, w[2]); c.Set(14, y, w[0]); }
        for (int x = 1; x < 25; x++) { c.Set(x, 19, w[3]); c.Set(x, 20, w[2]); c.Set(x, 21, w[0]); }
        // straw hands
        for (int i = 0; i < 4; i++) { c.Set(0 + i / 2, 22 + i, st[3]); c.Set(1, 23 + i, st[2]); c.Set(24 - i / 2, 22 + i, st[3]); c.Set(23, 23 + i, st[2]); }
        c.Set(0, 20, st[4]); c.Set(25, 20, st[4]);
        // tunic: linen with a terracotta patch and teal sleeves
        for (int y = 19; y < 33; y++)
        {
            int half = y < 22 ? 11 : 6 - (y - 22) / 6;
            if (y < 22) half = 11 - (y == 19 ? 1 : 0);
            for (int x = 13 - half; x < 13 + half; x++)
            {
                bool sleeve = y < 25 && (x < 8 || x >= 18);
                var ramp = sleeve ? K.TealR : K.Linen;
                int idx = sleeve ? (x < 8 ? 2 : 1) : (x < 11 ? 3 : x < 14 ? 2 : 1);
                if (y == 19) idx++;
                if (y == 32 || x == 13 - half) idx = Math.Max(0, idx - (y == 32 ? 2 : 0));
                c.Set(x, y, ramp[Math.Clamp(idx, 0, 4)]);
            }
        }
        c.Rect(15, 25, 4, 4, K.Terra); c.HL(15, 25, 4, K.RedR[3]); c.Set(15, 25, K.RedR[4]); c.Set(17, 27, K.Cream); // stitched patch
        c.Set(16, 26, K.Cream);
        for (int x = 7; x < 19; x += 2) c.Set(x, 33, st[3]);                                                         // straw hem
        // trousers
        for (int y = 33; y < 41; y++)
        {
            for (int x = 8; x < 12; x++) c.Set(x, y, K.BlueR[x == 8 ? 3 : 2]);
            for (int x = 15; x < 19; x++) c.Set(x, y, K.BlueR[x == 15 ? 2 : 0]);
        }
        c.Rect(9, 36, 2, 2, K.Mustard[3]);
        for (int i = 0; i < 4; i++) { c.Set(8 + i, 41, st[3]); c.Set(15 + i, 41, st[2]); }
        c.Set(9, 42, st[4]); c.Set(17, 42, st[2]);
        // sack head
        c.Ellipse(13, 12, 5.5, 5.5, K.Parch);
        c.Ellipse(12.5, 11.5, 4.5, 4.5, K.Linen[2]);
        c.Rect(7, 14, 12, 1, K.Sand);
        c.Rect(10, 10, 2, 2, K.Brown3); c.Rect(15, 10, 2, 2, K.Brown3);
        c.Set(10, 10, K.Brown); c.Set(15, 10, K.Brown);
        for (int x = 10; x < 17; x++) c.Set(x, 14 - (x == 10 || x == 16 ? 1 : 0), K.Brown2);
        c.Set(13, 12, K.Sand);
        // straw tufts under the hat
        for (int i = 0; i < 4; i++) { c.Set(7 - (i % 2), 9 + i, st[3]); c.Set(19 + (i % 2), 9 + i, st[2]); }
        // wide straw hat with a red band
        c.Ellipse(13, 7, 11, 3, st[3]);
        c.Ellipse(13, 6.6, 10.2, 2.2, st[4]);
        c.Rect(8, 1, 10, 6, st[2]); c.Rect(9, 1, 8, 1, st[3]); c.HL(8, 6, 10, K.Terra); c.HL(8, 5, 10, K.RedR[3]);
        c.VL(8, 1, 6, st[3]); c.VL(17, 1, 6, st[1]);
        c.Set(2, 7, st[1]); c.Set(23, 8, st[1]);
        // a crow on the right arm
        c.Rect(19, 14, 4, 4, K.Iron[1]); c.Set(23, 15, K.Iron[1]); c.Set(24, 16, K.Gold); c.Rect(18, 17, 2, 1, K.Iron[0]);
        c.Set(20, 15, K.Iron[3]); c.Set(21, 14, K.Iron[3]); c.Set(22, 16, K.Cream);
        return U.Finish(c, 0.38);
    }

    // ---- fence & gate -------------------------------------------------------------------------------------
    static Canvas FenceH()
    {
        var c = new Canvas(16, 20);
        var p = K.Pine;
        // rails behind
        c.Rect(0, 6, 16, 2, p[1]); c.HL(0, 6, 16, p[2]); c.Rect(0, 14, 16, 2, p[1]); c.HL(0, 14, 16, p[2]);
        // pickets: 3 wide, 1 gap, pointed tops
        for (int i = 0; i < 4; i++)
        {
            int x = i * 4;
            for (int y = 3; y < 20; y++)
            {
                c.Set(x, y, p[4]); c.Set(x + 1, y, p[3]); c.Set(x + 2, y, p[1]);
            }
            c.Set(x + 1, 2, p[4]); c.Set(x, 3, p[4]);
            c.Set(x + 2, 19, p[0]); c.Set(x + 1, 19, p[1]); c.Set(x, 19, p[1]);
            c.Set(x + 1, 6, p[1]); c.Set(x + 1, 14, p[1]);
            if (i % 2 == 0) c.Set(x + 1, 10, p[2]);
        }
        return U.Finish(c, 0.38);
    }

    static Canvas FenceV()
    {
        var c = new Canvas(8, 16);
        var p = K.Pine;
        // a rail running away from the viewer (top faces) with a picket standing at the near end
        c.Rect(2, 0, 3, 16, p[3]); c.VL(2, 0, 16, p[4]); c.VL(4, 0, 16, p[2]);
        c.Rect(1, 4, 5, 2, p[2]); c.HL(1, 4, 5, p[4]);
        // post
        c.Rect(1, 6, 5, 9, p[3]); c.VL(1, 6, 9, p[4]); c.VL(5, 6, 9, p[1]); c.HL(1, 6, 5, p[4]); c.HL(1, 14, 5, p[0]);
        c.Rect(2, 9, 3, 1, p[1]);
        c.Set(3, 15, p[0]); c.Set(2, 15, p[0]); c.Set(4, 15, p[0]);
        return U.Finish(c, 0.38);
    }

    static Canvas FencePost()
    {
        var c = new Canvas(8, 24);
        var p = K.Pine;
        for (int y = 3; y < 24; y++) for (int x = 0; x < 6; x++) c.Set(x + 1, y, x == 0 ? p[4] : x == 5 ? p[0] : x == 1 ? p[3] : p[2]);
        // pointed cap
        c.Rect(1, 2, 6, 1, p[4]); c.Rect(2, 1, 4, 1, p[4]); c.Rect(3, 0, 2, 1, p[3]);
        c.HL(1, 23, 6, p[0]);
        c.Set(2, 12, p[1]); c.Set(3, 13, p[1]);
        return U.Finish(c, 0.38);
    }

    static Canvas Gate()
    {
        var c = new Canvas(44, 24);
        var p = K.Pine;
        // posts
        foreach (var px in new[] { 0, 37 })
        {
            for (int y = 3; y < 24; y++) for (int x = 0; x < 7; x++) c.Set(px + x, y, x == 0 ? p[4] : x == 6 ? p[0] : x == 1 ? p[3] : p[2]);
            c.Rect(px, 2, 7, 1, p[4]); c.Rect(px + 1, 1, 5, 1, p[4]); c.Rect(px + 2, 0, 3, 1, p[3]);
            c.HL(px, 23, 7, p[0]);
        }
        // open leaf, folded back against the left post: slim picket panel
        for (int y = 6; y < 23; y++) { c.Set(7, y, p[4]); c.Set(8, y, p[3]); c.Set(9, y, p[1]); }
        c.Rect(7, 8, 5, 2, p[3]); c.Rect(7, 16, 5, 2, p[3]); c.HL(7, 8, 5, p[4]); c.HL(7, 16, 5, p[4]);
        c.Set(10, 6, p[3]); c.Set(11, 8, p[1]);
        c.Set(8, 7, p[4]);
        // iron hinges and latch hook on the right post
        c.Rect(6, 8, 2, 1, K.Iron[3]); c.Rect(6, 17, 2, 1, K.Iron[3]);
        c.Rect(37, 11, 2, 3, K.Iron[2]); c.Set(37, 11, K.Iron[4]);
        // a little shadowed doorsill of packed earth between the posts is left to the ground
        return U.Finish(c, 0.38);
    }

    // ---- tools & small things ----------------------------------------------------------------------------
    static Canvas WateringCan()
    {
        var c = new Canvas(18, 16);
        var t = K.TealR;
        // body
        for (int y = 5; y < 15; y++)
            for (int x = 3; x < 12; x++)
            {
                int idx = x < 5 ? 4 : x < 7 ? 3 : x < 10 ? 2 : 1;
                if (y == 14) idx = 0; if (y == 5) idx = Math.Min(4, idx + 1);
                c.Set(x, y, t[idx]);
            }
        // rim and water mouth
        c.Rect(3, 4, 9, 2, t[4]); c.Rect(5, 3, 5, 1, K.Dark[1]); c.Rect(6, 4, 3, 1, K.Water);
        c.HL(3, 9, 9, t[1]); c.HL(3, 10, 9, t[3]);
        c.Set(4, 6, K.WaterHi); c.Set(4, 7, t[4]);
        // spout up to a brass rose
        c.Line(11, 12, 15, 5, t[2]); c.Line(12, 12, 16, 5, t[1]);
        c.Rect(14, 3, 4, 3, K.Mustard[3]); c.HL(14, 3, 4, K.Mustard[4]); c.HL(14, 5, 4, K.Mustard[1]);
        c.Set(15, 4, K.Mustard[0]); c.Set(17, 4, K.Mustard[0]);
        // handle loop
        c.Line(3, 7, 1, 8, t[2]); c.Line(1, 8, 1, 11, t[2]); c.Line(1, 11, 3, 13, t[1]);
        c.Line(4, 4, 7, 1, t[3]); c.Set(8, 1, t[3]); c.Set(9, 2, t[2]);
        // little flower painted on the side
        c.Set(7, 11, K.Cream); c.Set(6, 11, K.Gold); c.Set(8, 11, K.Gold); c.Set(7, 10, K.Gold); c.Set(7, 12, K.Gold);
        c.Set(7, 11, K.Terra);
        return U.Finish(c, 0.4);
    }

    static Canvas Bucket()
    {
        var c = new Canvas(14, 14);
        var w = K.Wood;
        for (int y = 4; y < 13; y++)
        {
            int half = 5 + (y - 4) / 5 * 0;
            int xa = 7 - 5 + (y > 9 ? 1 : 0), xb = 7 + 5 - (y > 9 ? 1 : 0);
            for (int x = xa; x < xb; x++)
            {
                int idx = x < xa + 2 ? 4 : x < xa + 5 ? 3 : x < xb - 2 ? 2 : 1;
                if ((x - xa) % 3 == 2) idx--;
                c.Set(x, y, w[Math.Clamp(idx, 0, 5)]);
            }
        }
        c.HL(2, 6, 10, K.Iron[3]); c.HL(3, 11, 8, K.Iron[2]);
        c.Ellipse(7, 4, 5, 2, w[4]); c.Ellipse(7, 4.2, 4, 1.4, K.Water); c.Set(6, 4, K.WaterHi);
        c.Line(2, 4, 4, 0, K.Brown); c.Line(4, 0, 9, 0, K.Brown); c.Line(9, 0, 12, 4, K.Brown);
        c.HL(3, 13, 8, w[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas BasketApples()
    {
        var c = new Canvas(18, 14);
        var s = K.Pine;
        // apples heaped inside
        U.Apple(c, 3, 4, K.RedR[2]); U.Apple(c, 7, 3, K.Terra); U.Apple(c, 11, 4, K.RedR[3]); U.Apple(c, 6, 5, K.Gold); U.Apple(c, 10, 6, K.RedR[2]);
        // wicker body
        for (int y = 7; y < 13; y++)
        {
            int inset = (y - 7) / 3;
            for (int x = 1 + inset; x < 17 - inset; x++)
            {
                int idx = ((x + (y % 2) * 2) % 4 < 2) ? 3 : 2;
                if (y == 7) idx = 4; if (y == 12) idx = 0; if (x == 1 + inset) idx = 4; if (x == 16 - inset) idx = 1;
                c.Set(x, y, s[idx]);
            }
        }
        c.HL(1, 7, 16, s[4]);
        // handle
        c.Line(3, 6, 5, 1, s[3]); c.Line(5, 1, 12, 1, s[3]); c.Line(12, 1, 14, 6, s[3]);
        c.Line(4, 6, 6, 2, s[1]); c.Line(13, 6, 12, 2, s[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas Trellis()
    {
        var c = new Canvas(14, 30);
        var p = K.Pine;
        foreach (var x in new[] { 1, 10 }) { for (int y = 2; y < 30; y++) { c.Set(x, y, p[3]); c.Set(x + 1, y, p[1]); } c.Set(x, 1, p[4]); }
        for (int y = 6; y < 28; y += 7) { c.HL(1, y, 11, p[2]); c.HL(1, y + 1, 11, p[0]); }
        // sweet peas climbing
        for (int y = 3; y < 28; y++)
        {
            int x = 3 + (int)(Math.Sin(y * 0.55) * 2.5) + 2;
            c.Set(x, y, K.Leaf[3]); c.Set(x + 1, y, K.Leaf[2]);
            if (y % 5 == 0) { c.Set(x - 2, y, K.Leaf[4]); c.Set(x + 3, y - 1, K.Leaf[3]); c.Set(x - 1, y, K.Leaf[3]); }
            if (y % 7 == 2) { c.Set(x + 2, y, Hx("EAA5B5")); c.Set(x + 3, y, Hx("F6C6CF")); c.Set(x + 2, y + 1, Hx("D4849A")); }
        }
        return U.Finish(c, 0.4);
    }
}
