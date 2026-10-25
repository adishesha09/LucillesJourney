namespace WorldArtCh3;

/// <summary>Sprite kit: helpers, plus the street-level props.</summary>
public static partial class Sprites
{
    // ---- palette shortcuts ---------------------------------------------------------------------------------------
    static Rgba H(string h) => Rgba.Hex(h);
    static readonly Rgba[] Burlap = { H("8E7650"), H("A88C5E"), H("C2A672"), H("D8C08C"), H("EAD8A8") };
    static readonly Rgba[] Brick = { H("7A3A32"), H("9A4A3C"), H("B65E48"), H("CC7658"), H("DE9070") };
    static readonly Rgba[] Concrete = { H("5E5A5C"), H("7A7678"), H("9A9696"), H("B6B2AE"), H("D0CCC4") };
    static readonly Rgba[] Plast = { H("C8B8A0"), H("DCCFB8"), H("EBE0CA"), H("F6EEDC"), H("FFFAEA") };
    static readonly Rgba[] Dust = { H("A89C90"), H("C2B8AA"), H("D8D0C0"), H("EAE4D6"), H("F6F2E8") };

    static Canvas C(int w, int h) => new(w, h);

    static Func<Rgba, Rgba> Inner => c => Dusk.Mix(Dusk.Street(c), Dusk.Cellar(c), 0.45);

    /// <summary>Outline, grade and register a sprite.</summary>
    static void Reg(string name, Canvas c, Func<Rgba, Rgba>? grade = null, bool outline = true, bool diag = false)
    {
        if (outline) c.SelOutline(0.38, diag);
        S.Add(name, Dusk.Tint(c, grade ?? Dusk.Street));
    }
    /// <summary>Register without any grading (light and dust effects, final colours).</summary>
    static void RegRaw(string name, Canvas c) => S.Add(name, c);

    /// <summary>Register a chapter-1 sprite from disk, graded, under a chapter-3 name.</summary>
    static void Reuse(string diskName, string newName, Func<Rgba, Rgba>? grade = null)
    {
        string p = Path.Combine(Program.AssetDir, diskName + ".png");
        var c = Png.Decode(File.ReadAllBytes(p)) ?? throw new Exception("cannot decode " + p);
        S.Add(newName, Dusk.Tint(c, grade ?? Dusk.Street));
    }

    public static void Build()
    {
        S.Cur = "reused"; Reused();
        S.Cur = "street"; Street();
        S.Cur = "damage"; Damage();
        S.Cur = "cellar"; Cellar();
        S.Cur = "folk"; Folk();
        S.Cur = "fx"; Effects();
    }

    // ---- chapter-1 art, re-tinted for dusk ------------------------------------------------------------------------
    static void Reused()
    {
        foreach (var n in new[] { "tree_broad", "tree_cypress", "bush_round", "fence_h", "fence_post", "bench", "plant_pot", "pot_red", "chair_front", "chair_left", "chair_right",
                     "stonewall_a", "stonewall_b", "rock_a", "rock_b", "rock_c", "chimney", "wheelbarrow", "bucket", "sacks", "basket", "woodstack", "veg_crate", "hedge_short", "grass_tall_a", "grass_tall_b", "broom", "rain_barrel" })
            Reuse("prop_" + n, "prop_c3_" + n);
        foreach (var n in new[] { "rain_barrel", "kitchen_shelf", "stool", "rug_bedroom", "rug_kitchen", "bench", "bucket", "basket", "sacks", "plant_pot", "table_empty", "chair_front", "chair_left", "chair_right", "floorlamp", "basket_logs", "veg_crate" })
            Reuse("prop_" + n, "prop_c3_in_" + n, Inner);
    }

    // ---- sandbags ---------------------------------------------------------------------------------------------
    static void Bag(Canvas c, int x, int y, int w, int h, int seed)
    {
        var tones = new[] { Burlap[2], Burlap[1], Burlap[3], H("B79C68") };
        var b = tones[(int)(Fx.H(seed, x, y) % 4)];
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double nx = (i + 0.5 - w / 2.0) / (w / 2.0), ny = (j + 0.5 - h / 2.0) / (h / 2.0);
                if (nx * nx + ny * ny > 1.05) continue;
                double l = -(nx * 0.45 + ny * 0.85);
                var col = l > 0.45 ? b.Light(0.9) : l > 0.05 ? b : l > -0.4 ? b.Shade(0.55) : b.Shade(1.0);
                if (Fx.R(x + i, y + j, seed) > 0.93) col = col.Shade(0.3);
                c.Set(x + i, y + j, col);
            }
        c.Set(x, y + h / 2, b.Shade(1.1)); c.Set(x + w - 1, y + h / 2 - 1, b.Shade(1.1));
        for (int i = w / 2 - 2; i <= w / 2 + 2; i++) c.Set(x + i, y + h / 2 + 1, b.Shade(0.9));
    }

    static Canvas SandbagsWide(int w, int rows, int seed)
    {
        int bw = 11, bh = 7;
        var c = C(w, rows * (bh - 2) + 4);
        for (int r = 0; r < rows; r++)
        {
            int y = 2 + r * (bh - 2);
            int off = (r & 1) * (bw / 2);
            for (int x = -off; x < w; x += bw - 1) Bag(c, x, y, bw, bh, seed + r * 7);
        }
        return c;
    }

    static Canvas SandbagsNS(int w, int h, int seed)
    {
        var c = C(w, h);
        int rows = (h - 5) / 6;
        for (int r = 0; r < rows; r++)
        {
            int y = 2 + r * 6;
            Bag(c, (r % 2) * 2, y, w - 2, 9, seed + r);
        }
        return c;
    }

    static void Street()
    {
        Reg("prop_c3_sandbags_ns", SandbagsNS(20, 41, 5));
        Reg("prop_c3_sandbags_wide", SandbagsWide(52, 3, 9));
        Reg("prop_c3_sandbags_low", SandbagsWide(32, 2, 13));
        TakeCover();
        Handcart();
        Apples();
        Siren();
        StreetLamp(true); StreetLamp(false);
        StairParts();
        Crates();
        Cloth();
        RubbleSet();
        BrokenWall();
        MaterialsPile();
    }

    // ---- building materials stacked in front of the empty house (it becomes rubble after the blast) ---------------------
    static void MaterialsPile()
    {
        int w = 66, h = 34;
        var c = C(w, h);
        // pallet
        for (int i = 2; i < w - 2; i++) { c.Set(i, h - 3, K.Wood[3]); c.Set(i, h - 2, K.Wood[1]); c.Set(i, h - 1, K.Wood[0]); }
        for (int i = 4; i < w - 4; i += 12) c.Rect(i, h - 1, 4, 1, K.Wood[0]);
        // two stacks of bricks
        void Bricks(int x0, int rows, int cols)
        {
            for (int r = 0; r < rows; r++)
                for (int k = 0; k < cols; k++)
                {
                    int bx = x0 + k * 7 + (r % 2) * 3, by = h - 3 - (r + 1) * 4;
                    for (int j = 0; j < 4; j++) for (int i = 0; i < 7; i++)
                        {
                            int idx = j == 0 ? 3 : j == 3 ? 0 : 2;
                            if (i == 0) idx = Math.Max(0, idx - 1);
                            c.Set(bx + i, by + j, Brick[Math.Clamp(idx + (int)(Fx.H(k, r, x0) % 2) - (idx == 0 ? 0 : 0), 0, 4)]);
                        }
                }
        }
        Bricks(4, 5, 4); Bricks(36, 3, 3);
        // plank stack, ends visible
        for (int j = 0; j < 4; j++) c.Rect(24, h - 4 - 12 + j * 3, 30, 3, K.Wood[j % 2 == 0 ? 3 : 2]);
        for (int j = 0; j < 4; j++) { c.Rect(24, h - 4 - 12 + j * 3, 30, 1, K.Wood[4]); c.Set(23, h - 4 - 12 + j * 3, K.Wood[0]); }
        // folded tarp and a sack of lime on top
        c.Rect(26, h - 4 - 16, 24, 4, K.TealR[2]); c.Rect(26, h - 4 - 16, 24, 1, K.TealR[3]); c.Rect(26, h - 4 - 13, 24, 1, K.TealR[0]);
        Bag(c, 48, 3, 12, 8, 41);
        Reg("prop_c3_materials", c);
    }

    // ---- the "take cover" sandbag lean-to ------------------------------------------------------------------------
    static void TakeCover()
    {
        int w = 56, h = 40;
        var c = C(w, h);
        c.Rect(8, 12, w - 16, 20, H("2E2638"));
        for (int i = 8; i < w - 8; i += 2) c.Set(i, 31, H("3A3048"));
        // sandbag stacks left and right and a low front row, leaving a doorway in the middle
        for (int r = 0; r < 3; r++)
        {
            int y = 12 + r * 6;
            Bag(c, (r % 2) * -1, y, 15, 8, 3 + r); Bag(c, w - 15 + (r % 2), y, 15, 8, 9 + r);
        }
        foreach (var bx in new[] { 0, 9, 38, 47 }) Bag(c, bx, 31, 10, 8, 21 + bx);
        // roof: timber planks sloping, with a tarp tied over the left half
        for (int j = 0; j < 12; j++)
            for (int i = 0; i < w - 2; i++)
            {
                int yy = 1 + j + (i < 3 || i > w - 6 ? 1 : 0);
                int idx = j == 0 ? 4 : j > 9 ? 0 : 2 + (j % 4 == 3 ? -1 : 0);
                if (i % 13 == 0) idx = Math.Max(0, idx - 1);
                c.Set(1 + i, yy, K.Wood[idx]);
            }
        for (int j = 0; j < 9; j++)
            for (int i = 0; i < 28; i++)
            {
                int x = 3 + i, y = 2 + j + (i > 20 ? (i - 20) / 3 : 0);
                int idx = j == 0 ? 3 : j > 7 ? 0 : 2;
                if ((i + j) % 9 == 0) idx = Math.Max(0, idx - 1);
                c.Set(x, y, K.TealR[idx]);
            }
        for (int i = 6; i < 26; i += 6) c.Set(3 + i, 3, K.Iron[2]);
        // a bag on the roof holds the planks down
        Bag(c, 36, 3, 11, 6, 31);
        Reg("prop_c3_cover", c);
    }

    // ---- stalled handcart ----------------------------------------------------------------------------------------
    static void Handcart()
    {
        int w = 46, h = 28;
        var c = C(w, h);
        for (int i = 0; i < 18; i++) { c.Set(1 + i, 22 + i / 9, K.Wood[2]); c.Set(1 + i, 23 + i / 9, K.Wood[0]); }
        for (int j = 0; j < 22; j++)
            for (int i = 14; i < 43; i++)
            {
                double top = 10 - (i - 14) * 5.0 / 28, bot = 24 - (i - 14) * 6.0 / 29;
                int y = j + 4;
                if (y < top || y > bot) continue;
                int row = (int)((y - top) / 4);
                int idx = row == 0 ? 4 : ((y - top) % 4 >= 3 ? 0 : 2 + (row % 2));
                if ((i + row * 5) % 11 == 0) idx = 1;
                c.Set(i, y, K.Wood[idx]);
            }
        for (int i = 14; i < 43; i++) { int y = (int)(10 - (i - 14) * 5.0 / 28); c.Set(i, y, K.Wood[4]); c.Set(i, y - 1, K.Wood[5]); }
        Bag(c, 18, 2, 12, 9, 3); Bag(c, 28, 0, 12, 9, 4);
        c.Disc(36, 21, 6, K.Iron[1]); c.Disc(36, 21, 5, K.Wood[3]); c.Disc(36, 21, 4, K.Wood[1]);
        for (int a = 0; a < 6; a++) { double t = a * Math.PI / 3; c.Line(36, 21, 36 + (int)Math.Round(Math.Cos(t) * 5), 21 + (int)Math.Round(Math.Sin(t) * 5), K.Wood[4]); }
        c.Disc(36, 21, 1, K.Iron[3]);
        c.Ellipse(14, 25, 7, 2.4, K.Iron[1]); c.Ellipse(14, 24.5, 6, 1.8, K.Wood[2]); c.Ellipse(14, 24.5, 2.5, 0.9, K.Iron[2]);
        c.Rect(13, 20, 3, 5, K.Iron[2]);
        Reg("prop_c3_handcart", c);
    }

    static void Apples()
    {
        var c = C(26, 12);
        foreach (var (x, y, col) in new[] { (3, 4, 2), (9, 7, 1), (14, 3, 2), (19, 8, 1), (22, 4, 2), (6, 9, 1), (12, 8, 2) })
        {
            var r = col == 2 ? K.RedR : K.Mustard;
            c.Rect(x, y, 4, 3, r[2]); c.Set(x, y, r[1]); c.Set(x + 3, y, r[1]); c.Set(x, y + 2, r[0]); c.Set(x + 3, y + 2, r[0]); c.Set(x + 1, y, r[4]);
            c.Set(x + 1, y - 1, K.Leaf[2]);
        }
        Reg("prop_c3_apples", c);
    }

    // ---- siren pole -----------------------------------------------------------------------------------------------
    static void Siren()
    {
        int w = 20, h = 62;
        var c = C(w, h);
        Draw.Cyl(c, 8, 14, 4, h - 14, K.Iron, 3, false);
        c.Rect(5, h - 3, 10, 3, K.Stone[2]); c.Rect(5, h - 3, 10, 1, K.Stone[4]);
        c.Rect(3, 10, 14, 2, K.Iron[2]);
        for (int side = 0; side < 2; side++)
        {
            int x0 = side == 0 ? 0 : 12;
            for (int j = 0; j < 9; j++)
            {
                int hw = 2 + j / 2;
                for (int i = 0; i < hw * 2; i++)
                {
                    int idx = i < 2 ? 4 : i > hw * 2 - 3 ? 1 : 3;
                    if (j > 6) idx = Math.Max(0, idx - 1);
                    c.Set(x0 + 4 - hw + i, j + 1, K.Plaster[Math.Clamp(idx, 0, 4)].Shade(0.2));
                }
            }
            c.Ellipse(x0 + 4, 10, 4.5, 1.4, K.Iron[1]);
        }
        for (int j = 22; j < h - 4; j++) c.Set(13, j, K.Line2);
        c.Rect(11, 30, 4, 6, K.Iron[3]); c.Rect(11, 30, 4, 1, K.Iron[4]);
        Reg("prop_c3_siren", c);
    }

    // ---- street lamps (lit before the blast, dim after) ----------------------------------------------------------
    static void StreetLamp(bool lit)
    {
        int w = 14, h = 42;
        var c = C(w, h);
        Draw.Cyl(c, 5, 14, 4, h - 14, K.Iron, 4, false);
        c.Rect(3, h - 3, 8, 3, K.Iron[2]); c.Rect(3, h - 3, 8, 1, K.Iron[3]);
        c.Rect(4, h - 7, 6, 2, K.Iron[3]);
        c.Rect(2, 4, 10, 11, K.Iron[1]);
        var glass = lit ? new[] { H("FFF0B8"), H("FFD27A"), H("F2A850") } : new[] { H("C8884A"), H("A0683C"), H("7A4E34") };
        for (int j = 0; j < 9; j++) for (int i = 0; i < 8; i++) c.Set(3 + i, 5 + j, glass[j < 3 ? 0 : j < 6 ? 1 : 2]);
        for (int j = 4; j < 15; j++) c.Set(7, j, K.Iron[1]);
        c.Rect(1, 2, 12, 2, K.Iron[3]); c.Rect(3, 0, 8, 2, K.Iron[2]); c.Set(6, 0, K.Iron[4]);
        c.Rect(4, 15, 6, 1, K.Iron[0]);
        c.SelOutline(0.38, false);
        var g = Dusk.Tint(c);
        for (int j = 5; j < 14; j++) for (int i = 3; i < 11; i++)
        {
            var px = c.Get(i, j);
            if (px.A == 0) continue;
            if (lit && px.R > 150 && px.G > 120) g.Set(i, j, px);                 // glass stays luminous
            else if (!lit && px.R > 120 && px.G > 80 && px.B < 100) g.Set(i, j, Dusk.Mix(Dusk.Street(px), px, 0.5));
        }
        S.Add(lit ? "prop_c3_streetlamp_lit" : "prop_c3_streetlamp_dim", g);
    }

    // ---- stairwell head: posts, lintel, hanging lantern ----------------------------------------------------------
    static void StairParts()
    {
        var p = C(10, 44);
        Draw.Cyl(p, 1, 3, 8, 41, K.Wood, 6);
        p.Rect(0, 0, 10, 3, K.Wood[4]); p.Rect(0, 3, 10, 1, K.Wood[0]);
        p.Rect(0, 41, 10, 3, K.Stone[3]); p.Rect(0, 43, 10, 1, K.Stone[0]);
        p.Rect(1, 20, 8, 1, K.Iron[2]); p.Set(5, 21, K.Iron[3]);
        Reg("prop_c3_stair_post", p);
        var l = C(64, 16);
        for (int j = 0; j < 12; j++)
            for (int i = 0; i < 64; i++)
            {
                int idx = j == 0 ? 4 : j >= 10 ? 0 : 2 + (j % 4 == 2 ? -1 : 0);
                if (i % 17 == 0) idx = Math.Max(0, idx - 1);
                if (i < 3 || i > 60) idx = Math.Max(0, idx - 1);
                l.Set(i, 2 + j, K.Wood[idx]);
            }
        for (int i = 3; i < 61; i += 18) { l.Set(i, 5, K.Iron[3]); l.Set(i, 9, K.Iron[3]); }
        l.Rect(30, 14, 4, 2, K.Iron[2]);
        Reg("prop_c3_stair_lintel", l);
        for (int v = 0; v < 3; v++)
        {
            bool on = v > 0;
            var c = C(12, 22);
            c.Rect(5, 0, 2, 7, K.Iron[3]);
            for (int j = 1; j < 7; j += 2) c.Set(4, j, K.Iron[2]);
            c.Rect(2, 7, 8, 2, K.Iron[2]); c.Rect(2, 9, 8, 10, K.Iron[1]);
            var gl = on ? new[] { H("FFF0B8"), H("FFD27A"), H("F2A850") } : new[] { H("8A5E3C"), H("6A4A34"), H("4A3A32") };
            if (v == 2) gl = new[] { H("FFE9A0"), H("F6BE62"), H("E08A4C") };
            for (int j = 0; j < 8; j++) for (int i = 0; i < 6; i++) c.Set(3 + i, 10 + j, gl[j < 3 ? 0 : j < 6 ? 1 : 2]);
            c.Rect(5, 9, 2, 9, K.Iron[0]);
            c.Rect(3, 19, 6, 2, K.Iron[2]);
            c.SelOutline(0.38);
            var g = Dusk.Tint(c);
            if (on) for (int j = 10; j < 18; j++) for (int i = 3; i < 9; i++) { var px = c.Get(i, j); if (px.A != 0 && px.R > 150) g.Set(i, j, px); }
            S.Add(v == 0 ? "prop_c3_stair_lantern_off" : $"prop_c3_stair_lantern_on_{v - 1}", g);
        }
    }

    // ---- wooden crates --------------------------------------------------------------------------------------------
    static void CrateBox(Canvas c, int x, int y, int w, int h, int seed)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx;
                bool edge = i < 2 || i >= w - 2 || j < 2 || j >= h - 2;
                if (edge) idx = (i < 2 || j < 2) ? 3 : 1;
                else
                {
                    int plank = (j - 2) / 4;
                    idx = 2 + (int)(Fx.H(plank, seed, 3) % 2) - (((j - 2) % 4 == 3) ? 2 : 0);
                }
                c.Set(x + i, y + j, K.Wood[Math.Clamp(idx, 0, 5)]);
            }
        c.Set(x + 1, y + 1, K.Iron[3]); c.Set(x + w - 2, y + 1, K.Iron[3]); c.Set(x + 1, y + h - 2, K.Iron[3]); c.Set(x + w - 2, y + h - 2, K.Iron[3]);
    }

    static void Crates()
    {
        var a = C(18, 16); CrateBox(a, 0, 0, 18, 16, 1); Reg("prop_c3_crate_a", a);
        var b = C(22, 18); CrateBox(b, 0, 0, 22, 18, 2); Reg("prop_c3_crate_b", b);
        var s = C(36, 40); CrateBox(s, 2, 22, 22, 18, 3); CrateBox(s, 16, 24, 18, 16, 4); CrateBox(s, 6, 6, 20, 17, 5);
        Bag(s, 22, 12, 11, 9, 6);
        Reg("prop_c3_crates_stack", s);
        var k = C(22, 24);
        CrateBox(k, 1, 9, 20, 15, 7);
        for (int j = 0; j < 8; j++)
            for (int i = 0; i < 18; i++)
            {
                int idx = j < 2 ? 3 : j > 5 ? 0 : 2;
                var col = (i / 3 + j / 3) % 2 == 0 ? K.TealR[Math.Min(4, idx + 1)] : K.TealR[idx];
                if (j == 3 && i % 3 == 1) col = K.Cream;
                k.Set(2 + i, 2 + j, col);
            }
        for (int i = 0; i < 18; i += 2) k.Set(2 + i, 9, K.TealR[0]);
        Reg("prop_c3_blanket_crate", k);
    }

    // ---- hanging cloth (animated) -----------------------------------------------------------------------------------
    static void Cloth()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = C(24, 24);
            for (int j = 0; j < 22; j++)
                for (int i = 0; i < 20; i++)
                {
                    double sway = Math.Sin(j * 0.35 + f * Math.PI) * 1.2;
                    int xx = 2 + i + (int)Math.Round(sway * j / 22.0);
                    int idx = 3;
                    int fold = (i + (f == 0 ? 0 : 2)) % 6;
                    if (fold == 0) idx = 2; else if (fold == 1) idx = 4; else if (fold == 5) idx = 2;
                    if (j == 0) idx = 1;
                    if (j >= 20 && (i + j) % 2 == 0) idx = 2;
                    c.Set(xx, 1 + j, K.Linen[Math.Clamp(idx, 0, 4)]);
                }
            for (int i = 1; i < 23; i += 5) c.Set(i + 1, 1, K.Iron[3]);
            Reg($"prop_c3_cloth_sheet_{f}", c);
            var s = C(44, 20);
            var cols = new[] { K.RedR, K.BlueR, K.Mustard, K.TealR };
            for (int k = 0; k < 4; k++)
            {
                int x0 = 2 + k * 10;
                for (int j = 0; j < 12 - (k % 2) * 3; j++)
                    for (int i = 0; i < 8; i++)
                    {
                        double sway = (f == 0 ? 1 : -1) * j * 0.08 * (k % 2 == 0 ? 1 : -1);
                        int idx = i < 2 ? 3 : i > 5 ? 1 : 2; if (j == 0) idx = 1;
                        s.Set(x0 + i + (int)Math.Round(sway), 3 + j, cols[k][idx]);
                    }
                s.Set(x0 + 2, 2, K.Wood[4]); s.Set(x0 + 5, 2, K.Wood[4]);
            }
            for (int i = 0; i < 44; i++) s.Set(i, 2, K.Line2);
            Reg($"prop_c3_cloth_shirts_{f}", s);
            var a = C(48, 20);
            for (int j = 0; j < 14; j++)
                for (int i = 0; i < 48; i++)
                {
                    int stripe = (i / 6) % 2;
                    int idx = stripe == 0 ? 2 : 3;
                    if (j == 0) idx = 4; else if (j > 11) idx = Math.Max(0, idx - 2); else if (j > 8) idx = Math.Max(0, idx - 1);
                    var ramp = stripe == 0 ? K.RedR : K.Linen;
                    int y = 1 + j + (int)(Math.Sin((i + f * 6) * 0.5) * 0.9 * (j > 11 ? 1 : 0));
                    a.Set(i, y, ramp[Math.Clamp(idx, 0, 4)]);
                }
            for (int i = 0; i < 48; i += 6) { int y = 15 + (int)(Math.Sin((i + f * 6) * 0.5) * 0.9); for (int k = 0; k < 4; k++) a.Set(i + 1 + k, y + 1, (i / 6) % 2 == 0 ? K.RedR[1] : K.Linen[1]); }
            Reg($"prop_c3_awning_{f}", a);
        }
    }

    // ---- rubble ----------------------------------------------------------------------------------------------------
    static void Chunk(Canvas c, int x, int y, int w, int h, Rgba[] ramp, int seed, bool fresh)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                if ((i == 0 || i == w - 1) && (j == 0 || j == h - 1)) continue;
                int idx = 2;
                if (j == 0 || i == 0) idx = 3; else if (j == h - 1 || i == w - 1) idx = 0; else if (j == h - 2) idx = 1;
                if (Fx.R(x + i, y + j, seed) > 0.9) idx--;
                var col = ramp[Math.Clamp(idx, 0, ramp.Length - 1)];
                if (fresh && (i + j) % 2 == 0 && j < h / 2 && Fx.R(x + i, y + j, seed + 3) > 0.45) col = Dust[2 + (int)(Fx.R(i, j, seed + 5) * 2)];
                c.Set(x + i, y + j, col);
            }
    }

    static readonly Rgba[] BrickOld = { H("4E3C3A"), H("66504A"), H("7C645A"), H("937A6C"), H("A8907F") };
    static readonly Rgba[] PlastOld = { H("847A70"), H("9A9084"), H("B0A698"), H("C4BAAC"), H("D4CCBE") };

    static Canvas Rubble(int w, int h, int seed, bool fresh, bool beam = false)
    {
        var c = C(w, h);
        var rnd = new Random(seed);
        int n = w * h / 18;
        var items = new List<(int x, int y, int cw, int ch, int kind)>();
        for (int i = 0; i < n; i++)
        {
            double a = rnd.NextDouble() * Math.PI * 2, r = Math.Sqrt(rnd.NextDouble());
            double cx = w / 2.0 + Math.Cos(a) * r * (w / 2.0 - 4), cy = h - h * 0.32 + Math.Sin(a) * r * (h * 0.30) - r * h * 0.22 * (1 - r);
            int cw = 4 + rnd.Next(6), ch = 3 + rnd.Next(3);
            items.Add(((int)cx - cw / 2, (int)cy - ch / 2, cw, ch, rnd.Next(5)));
        }
        for (int i = 0; i < w; i++)
            for (int j = h - 6; j < h; j++)
            {
                double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j - (h - 3)) / 3.0;
                if (dx * dx + dy * dy > 1) continue;
                c.Set(i, j, Concrete[(int)(Fx.R(i, j, seed) * 3)]);
            }
        foreach (var it in items.OrderBy(t => t.y))
        {
            Rgba[] ramp = fresh
                ? it.kind switch { 0 or 3 => Brick, 1 or 4 => Concrete, _ => Plast }
                : it.kind switch { 0 => BrickOld, 2 => PlastOld, _ => Concrete };
            Chunk(c, it.x, it.y, it.cw, it.ch, ramp, seed + it.x, fresh);
        }
        if (beam)
            for (int i = 0; i < w / 2; i++) { int x = w / 3 + i, y = h / 2 - i / 3; for (int k = 0; k < 3; k++) c.Set(x, y + k, K.Wood[k == 0 ? 3 : k == 1 ? 2 : 0]); }
        if (!fresh) for (int i = 0; i < w; i += 5) { int x = i + rnd.Next(3), y = h - 3 - rnd.Next(4); if (c.Opaque(x, y)) { c.Set(x, y - 1, K.Leaf[2]); c.Set(x + 1, y - 2, K.Leaf[3]); } }
        return c;
    }

    static void RubbleSet()
    {
        Reg("prop_c3_rubble_old_a", Rubble(30, 16, 1, false));
        Reg("prop_c3_rubble_old_b", Rubble(38, 20, 2, false, true));
        Reg("prop_c3_rubble_old_big", Rubble(64, 36, 3, false, true));
    }

    static void BrokenWall()
    {
        int w = 48, h = 24;
        var c = C(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int top = x < 22 ? 4 : x < 32 ? 4 + (x - 21) * 7 / 10 : 12 + (x % 3);
                if (x > 40) top = 17 + (x % 2) * 2;
                if (y < top) continue;
                int row = (y - 4) / 6, ly = (y - 4) % 6;
                var col = WallBlock(x + row * 7, ly, row);
                if (y == top) col = K.Stone[4];
                c.Set(x, y, col);
            }
        Chunk(c, 30, 16, 8, 6, Concrete, 4, false); Chunk(c, 38, 18, 7, 5, Concrete, 5, false); Chunk(c, 24, 18, 6, 5, Brick, 6, false);
        Reg("prop_c3_wall_broken", c);
    }

    static Rgba WallBlock(int x, int ly, int row)
    {
        int bw = 12, id = x / bw, lx = x % bw;
        int idx = 2 + (int)(Fx.H(id, row, 3) % 3);
        if (ly == 0) idx++;
        if (ly == 5) idx--;
        if (lx == 0 || ly == 5) return K.Mortar;
        return K.Stone[Math.Clamp(idx, 0, 5)];
    }
}
