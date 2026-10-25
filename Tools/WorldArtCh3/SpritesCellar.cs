namespace WorldArtCh3;

/// <summary>Cellar furniture and the quiet neighbours sheltering in it.</summary>
public static partial class Sprites
{
    static void Cellar()
    {
        ShelfJars();
        Jugs();
        for (int v = 0; v < 3; v++) Bedroll(v);
        Pillar();
        for (int v = 0; v < 3; v++) HangLantern(v);
        DoorLeaf();
        Bundles();
        BlanketPile();
        Candles();
        SleepMat();
    }

    static void SleepMat()
    {
        // a patchwork quilt laid flat as the children's bed
        int w = 60, h = 30;
        var c = C(w, h);
        var cols = new[] { K.BlueR, K.TealR, K.Mustard, K.RedR, K.Purple };
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                if ((i < 2 && j < 2) || (i >= w - 2 && j < 2) || (i < 2 && j >= h - 2) || (i >= w - 2 && j >= h - 2)) continue;
                int bi = i / 10, bj = j / 10;
                var ramp = cols[(int)(Fx.H(bi, bj, 9) % 5)];
                int idx = 1;
                if (i % 10 == 0 || j % 10 == 0) idx = 0;
                else if (i % 10 == 1 || j % 10 == 1) idx = 2;
                if (j == 0 || i == 0) idx = 2; if (j == h - 1 || i == w - 1) idx = 0;
                if (i % 10 == 5 && j % 10 == 5) idx = Math.Min(3, idx + 1);
                c.Set(i, j, ramp[Math.Clamp(idx, 0, 3)]);
            }
        Reg("prop_c3_mat", c, Inner);
    }

    static void Jar(Canvas c, int x, int y, int w, int h, Rgba[] ramp)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = i == 0 ? 3 : i == w - 1 ? 0 : 2;
                if (j == 0) idx = 3;
                if (j == 1 && i > 0 && i < w - 1) idx = 4;
                if (i == 1 && j > 1 && j < h - 2) idx = 3;
                c.Set(x + i, y + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
            }
        c.Rect(x + 1, y - 1, Math.Max(1, w - 2), 1, K.Wood[3]);          // lid
    }

    static void ShelfJars()
    {
        int w = 44, h = 44;
        var c = C(w, h);
        // frame
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++)
            {
                bool post = i < 3 || i >= w - 3;
                if (j >= h - 3 || post) c.Set(i, j, K.Wood[post ? (i < 2 || (i >= w - 3 && i < w - 1) ? 3 : 1) : 1]);
                else c.Set(i, j, K.Wood[0].Shade(0.3));
            }
        c.Rect(0, 0, w, 3, K.Wood[4]); c.Rect(0, 3, w, 1, K.Wood[1]);
        // three shelf boards
        foreach (var y in new[] { 14, 27, 40 })
        {
            c.Rect(0, y, w, 2, K.Wood[4]); c.Rect(0, y + 2, w, 2, K.Wood[1]);
        }
        var cols = new[] { K.Mustard, K.RedR, K.TealR, K.Purple, K.Fruit };
        // row 1 jars, row 2 tins and a loaf, row 3 sacks and crocks
        for (int i = 0; i < 6; i++) Jar(c, 4 + i * 6 + (i > 2 ? 1 : 0), 5, 5, 9, cols[i % 5]);
        for (int i = 0; i < 4; i++) { c.Rect(5 + i * 7, 19, 6, 8 - 1, K.Iron[2 + (i % 2)]); c.Rect(5 + i * 7, 19, 6, 1, K.Iron[4]); c.Rect(6 + i * 7, 22, 4, 2, K.Terra); }
        c.Ellipse(36, 24, 4.5, 2.6, K.Orange); c.Ellipse(36, 23, 3.5, 1.6, K.Gold);
        Bag(c, 4, 31, 11, 9, 8);
        for (int i = 0; i < 3; i++) { c.Ellipse(24 + i * 6, 35, 3, 4, K.Terra); c.Ellipse(24 + i * 6, 33, 2, 1.3, K.RedR[3]); c.Rect(22 + i * 6, 31, 4, 1, K.Wood[2]); }
        for (int j = 31; j < 39; j++) for (int i = 33; i < 42; i++) if ((i + j) % 3 != 0) c.Set(i, j, i % 2 == 0 ? K.Leaf[3] : K.Leaf[2]);   // hanging herbs
        Reg("prop_c3_shelf_jars", c, Inner);
    }

    static void Jugs()
    {
        var c = C(28, 24);
        void Clay(int cx, int by, int bw, int bh, Rgba[] ramp)
        {
            for (int j = 0; j < bh; j++)
                for (int i = 0; i < bw; i++)
                {
                    double nx = (i + .5 - bw / 2.0) / (bw / 2.0), ny = (j + .5 - bh / 2.0) / (bh / 2.0);
                    double sh = 0.9 - Math.Abs(ny) * 0.25;
                    if (Math.Abs(nx) > sh) continue;
                    double l = -nx * 0.7 - ny * 0.3;
                    c.Set(cx - bw / 2 + i, by - bh + j, ramp[l > 0.3 ? 3 : l > -0.2 ? 2 : l > -0.55 ? 1 : 0]);
                }
            int nw = Math.Max(3, bw / 2);
            c.Rect(cx - nw / 2, by - bh - 3, nw, 3, ramp[2]); c.Rect(cx - nw / 2 - 1, by - bh - 4, nw + 2, 1, ramp[3]);
            c.Set(cx + bw / 2, by - bh + 2, ramp[1]); c.Set(cx + bw / 2 + 1, by - bh + 3, ramp[1]); c.Set(cx + bw / 2, by - bh + 5, ramp[1]);
        }
        Clay(8, 23, 10, 13, K.RedR);
        Clay(19, 22, 9, 11, K.Mustard);
        // a glass demijohn with water
        c.Ellipse(14, 18, 5, 3, K.TealR[2]);
        Clay(14, 22, 8, 9, K.TealR);
        Reg("prop_c3_jugs", c, Inner);
    }

    static void Bedroll(int v)
    {
        var rolls = new[] { K.RedR, K.TealR, K.Mustard };
        var r = rolls[v];
        int w = 32, h = 16;
        var c = C(w, h);
        // mattress with rounded corners, blanket over the lower two thirds, pillow at the head
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                if ((i < 2 && j < 2) || (i >= w - 2 && j < 2) || (i < 2 && j >= h - 2) || (i >= w - 2 && j >= h - 2)) continue;
                var col = K.Linen[2];
                if (j == 0) col = K.Linen[3]; else if (j >= h - 2) col = K.Linen[0]; else if (i == 0) col = K.Linen[3]; else if (i == w - 1) col = K.Linen[1];
                if (i > 9)
                {
                    int idx = j < 3 ? 3 : j > h - 4 ? 0 : 2;
                    col = r[idx];
                    if ((i - 10) % 6 == 0 && j > 1 && j < h - 2) col = r[Math.Max(0, idx - 1)];
                    if (i == 10) col = r[Math.Max(0, idx - 2)];
                    if (j == 6 + v % 2 && i % 3 != 0) col = K.Cream;                 // woven stripe
                }
                else if (i > 2 && i < 8 && j > 3 && j < h - 4) col = i < 5 ? K.Cream : K.Linen[3];   // pillow
                c.Set(i, j, col);
            }
        // folded corner
        c.Tri(w - 7, 2, w - 2, 2, w - 2, 7, r[4]);
        Reg($"prop_c3_bedroll_{v}", c, Inner);
    }

    static void Pillar()
    {
        int w = 16, h = 62;
        var c = C(w, h);
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int ww = (j < 6 || j >= h - 8) ? w : 10;
                int x0 = (w - ww) / 2;
                if (i < x0 || i >= x0 + ww) continue;
                int li = i - x0;
                int idx = li == 0 ? 4 : li == ww - 1 ? 0 : li < 3 ? 3 : li > ww - 4 ? 1 : 2;
                if ((j / 8) % 2 == 1 && j > 6 && j < h - 8) idx = Math.Max(0, idx - (li == 0 ? 0 : 1));
                if (j == 0) idx = 4; if (j == 5 || j == h - 8) idx = 1;
                if (j >= h - 2) idx = 0;
                c.Set(i, j, K.Stone[Math.Clamp(idx, 0, 5)]);
            }
        for (int j = 8; j < h - 10; j += 8) c.HL(3, j, 10, K.Mortar);
        Reg("prop_c3_pillar", c, Inner);
    }

    static void HangLantern(int v)
    {
        // v0 unlit, v1/v2 lit flicker; tall (chain reaches the vault ceiling)
        bool on = v > 0;
        var c = C(14, 44);
        for (int j = 0; j < 26; j++) c.Set(7, j, j % 2 == 0 ? K.Iron[3] : K.Iron[1]);
        c.Rect(4, 26, 6, 2, K.Iron[2]); c.Rect(3, 28, 8, 2, K.Iron[3]);
        c.Rect(3, 30, 8, 11, K.Iron[1]);
        var gl = on ? new[] { H("FFF0B8"), H("FFD27A"), H("F2A850") } : new[] { H("8A5E3C"), H("6A4A34"), H("4A3A32") };
        if (v == 2) gl = new[] { H("FFE9A0"), H("F6BE62"), H("E08A4C") };
        for (int j = 0; j < 9; j++) for (int i = 0; i < 6; i++) c.Set(4 + i, 31 + j, gl[j < 3 ? 0 : j < 6 ? 1 : 2]);
        c.Rect(6, 30, 2, 10, K.Iron[0]);
        c.Rect(4, 41, 6, 2, K.Iron[2]); c.Set(6, 43, K.Iron[1]);
        c.SelOutline(0.38);
        var g = Dusk.Tint(c, Inner);
        if (on) for (int j = 31; j < 40; j++) for (int i = 4; i < 10; i++) { var px = c.Get(i, j); if (px.A != 0 && px.R > 150) g.Set(i, j, px); }
        S.Add(v == 0 ? "prop_c3_lantern_hang_off" : $"prop_c3_lantern_hang_on_{v - 1}", g);
    }

    static void DoorLeaf()
    {
        // heavy plank door swung open against the left cheek wall at the foot of the stairs
        var c = C(8, 40);
        for (int j = 0; j < 40; j++)
            for (int i = 0; i < 8; i++)
            {
                int idx = i == 0 ? 3 : i == 7 ? 0 : (i % 3 == 0 ? 1 : 2);
                if (j < 2) idx = 4; if (j >= 38) idx = 0;
                c.Set(i, j, K.Wood[idx]);
            }
        foreach (var y in new[] { 6, 20, 33 }) { c.Rect(0, y, 8, 2, K.Iron[2]); c.Set(1, y, K.Iron[4]); c.Set(6, y + 1, K.Iron[4]); }
        Reg("prop_c3_cellar_door_open", c, Inner);
    }

    static void Bundles()
    {
        var s = C(18, 14);
        s.Rect(0, 3, 18, 11, K.Brown); s.Rect(0, 3, 18, 2, K.Tan); s.Rect(0, 12, 18, 2, K.Brown2);
        s.Rect(7, 0, 4, 4, K.Iron[2]); s.Rect(8, 0, 2, 1, K.Iron[4]);
        s.Rect(2, 7, 14, 1, K.Brown2); s.Set(2, 9, K.Gold); s.Set(15, 9, K.Gold);
        s.Rect(0, 3, 1, 11, K.Tan);
        Reg("prop_c3_suitcase", s, Inner);
        var b = C(16, 14);
        b.Ellipse(8, 8, 7, 5.5, K.Terra); b.Ellipse(7, 7, 6, 4, K.RedR[3]);
        b.Ellipse(8, 3, 3, 2.4, K.RedR[2]); b.Rect(6, 9, 5, 1, K.Cream); b.Rect(5, 5, 1, 5, K.Cream);
        Reg("prop_c3_bundle", b, Inner);
    }

    static void BlanketPile()
    {
        var c = C(26, 12);
        var cols = new[] { K.TealR, K.RedR, K.Mustard };
        for (int k = 0; k < 3; k++)
        {
            int y = 8 - k * 3;
            for (int j = 0; j < 4; j++) for (int i = 0; i < 24 - k * 2; i++) c.Set(1 + k + i, y + j, cols[k][j == 0 ? 3 : j == 3 ? 0 : 2]);
            for (int i = 3; i < 22; i += 4) c.Set(1 + k + i, y + 1, cols[k][1]);
        }
        Reg("prop_c3_blanket_pile", c, Inner);
    }

    static void Candles()
    {
        // a candle stub in a saucer on the floor (static, tiny)
        var c = C(8, 10);
        c.Rect(1, 8, 6, 2, K.Stone[3]); c.Rect(2, 4, 3, 5, K.Cream); c.Rect(2, 4, 3, 1, K.Parch);
        c.Set(3, 3, K.Brown2); c.Set(3, 2, H("FFD27A")); c.Set(3, 1, H("FFF0B8"));
        c.SelOutline(0.38);
        var g = Dusk.Tint(c, Inner);
        g.Set(3, 2, H("FFD27A")); g.Set(3, 1, H("FFF0B8"));
        S.Add("prop_c3_candle", g);
    }

    // =====================================================================================================================
    //  People
    // =====================================================================================================================
    static readonly Rgba SkinA = H("E3B48A"), SkinB = H("C98F66"), SkinC = H("EFC9A0"), Cheek = H("E8A090");

    static void Head(Canvas c, int cx, int top, Rgba skin, Rgba[] hair, bool closedEyes, int hairStyle)
    {
        // 8 wide, 8 tall head
        for (int j = 0; j < 8; j++)
            for (int i = 0; i < 8; i++)
            {
                if ((i == 0 || i == 7) && (j == 0 || j == 7)) continue;
                c.Set(cx - 4 + i, top + j, skin);
            }
        // hair cap and sides
        for (int i = 0; i < 8; i++) { c.Set(cx - 4 + i, top, hair[1]); c.Set(cx - 4 + i, top + 1, hair[2]); }
        for (int j = 0; j < 4; j++) { c.Set(cx - 4, top + 1 + j, hair[1]); c.Set(cx + 3, top + 1 + j, hair[0]); }
        if (hairStyle == 1) for (int j = 4; j < 7; j++) { c.Set(cx - 4, top + j, hair[1]); c.Set(cx + 3, top + j, hair[0]); }
        c.Set(cx - 3, top + 2, hair[3]); c.Set(cx - 2, top + 1, hair[3]);
        // face
        if (closedEyes) { c.Set(cx - 2, top + 4, K.Brown2); c.Set(cx - 1, top + 4, K.Brown2); c.Set(cx + 1, top + 4, K.Brown2); c.Set(cx + 2, top + 4, K.Brown2); }
        else { c.Set(cx - 2, top + 4, K.Brown3); c.Set(cx + 1, top + 4, K.Brown3); }
        c.Set(cx - 3, top + 5, Cheek); c.Set(cx + 2, top + 5, Cheek);
        c.Set(cx - 1, top + 6, skin.Shade(0.7)); c.Set(cx, top + 6, skin.Shade(0.7));
    }

    static readonly Rgba[] HairBrown = { H("3A2820"), H("5A4030"), H("7A5A44"), H("9A7A5C") };
    static readonly Rgba[] HairDark = { H("1E1818"), H("2E2624"), H("4A3E3A"), H("6A5E56") };
    static readonly Rgba[] HairGrey = { H("7A7882"), H("9A98A2"), H("B8B6BE"), H("D8D6DC") };

    static void Folk()
    {
        for (int f = 0; f < 2; f++) { Huddle(f); Elder(f); Mother(f); }
    }

    static void Huddle(int f)
    {
        int w = 34, h = 24;
        var c = C(w, h);
        int lift = f;
        // padded bedding under them
        for (int j = 0; j < 6; j++)
            for (int i = 0; i < w; i++)
            {
                double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j - 2.5) / 3.3;
                if (dx * dx + dy * dy > 1) continue;
                c.Set(i, h - 6 + j, j < 2 ? K.Linen[2] : j < 5 ? K.Linen[1] : K.Linen[0]);
            }
        // children sit shoulder to shoulder, heads inclined together
        Head(c, 12, 4 - lift, SkinA, HairBrown, true, 1);
        Head(c, 21, 5 - lift, SkinB, HairDark, true, 0);
        // shoulders (yellow / blue tops from the character sheets)
        for (int j = 0; j < 4; j++) for (int i = 0; i < 9; i++) { c.Set(8 + i, 12 - lift + j, i < 2 ? K.Mustard[3] : K.Mustard[2 - (j > 2 ? 1 : 0)]); c.Set(17 + i, 13 - lift + j, i < 2 ? K.BlueR[3] : K.BlueR[2 - (j > 2 ? 1 : 0)]); }
        // a wool blanket pulled round both of them, scalloped top edge
        for (int j = 0; j < 9; j++)
            for (int i = 0; i < 30; i++)
            {
                int x = 2 + i, top = 14 - lift + (int)(Math.Sin(i * 0.5) * 0.7) + (i < 3 || i > 26 ? 2 : 0);
                int y = top + j;
                if (y >= h - 3) continue;
                int idx = j == 0 ? 4 : j > 6 ? 0 : j > 4 ? 1 : 2;
                if ((i * 3 + j * 5) % 11 == 0) idx = Math.Max(0, idx - 1);
                var col = K.RedR[Math.Clamp(idx, 0, 4)];
                if (j == 5 || j == 6) col = (i / 2) % 2 == 0 ? K.Cream.Shade(0.3) : K.Parch.Shade(0.4);      // woven border
                c.Set(x, y, col);
            }
        // small hands holding the blanket edge
        c.Set(14, 16 - lift, SkinA); c.Set(15, 16 - lift, SkinA); c.Set(19, 17 - lift, SkinB); c.Set(20, 17 - lift, SkinB);
        Reg($"npc_c3_children_huddle_{f}", c, Inner, true, true);
    }

    static void Elder(int f)
    {
        int w = 16, h = 26;
        var c = C(w, h);
        int lift = f;
        // legs and shoes (seated, knees forward)
        for (int j = 0; j < 8; j++) { c.Rect(4, 17 + j, 3, 1, K.Brown); c.Rect(9, 17 + j, 3, 1, K.Brown2); }
        c.Rect(3, 24, 5, 2, K.Brown3); c.Rect(8, 24, 5, 2, K.Brown3);
        c.Rect(3, 15, 10, 3, K.Brown);                                // thighs
        // body in a woolly cardigan
        for (int j = 0; j < 10; j++) for (int i = 0; i < 10; i++)
            {
                int x = 3 + i, y = 8 - lift + j;
                int idx = i < 2 ? 3 : i > 7 ? 0 : 2; if (j == 0) idx = 3;
                if (i == 5) idx = 0;                                  // open front
                if (i == 5 && j > 0) c.Set(x, y, K.Cream); else c.Set(x, y, K.BlueR[idx]);
            }
        // arms resting, hands on the knees
        c.Rect(2, 10 - lift, 2, 6, K.BlueR[1]); c.Rect(12, 10 - lift, 2, 6, K.BlueR[0]);
        c.Rect(2, 16 - lift, 3, 2, SkinB); c.Rect(11, 16 - lift, 3, 2, SkinB);
        Head(c, 8, 1 - lift, SkinB, HairGrey, true, 0);
        // eyebrows & a few lines
        c.Set(5, 4 - lift, HairGrey[3]); c.Set(6, 4 - lift, HairGrey[3]); c.Set(9, 4 - lift, HairGrey[3]); c.Set(10, 4 - lift, HairGrey[3]);
        Reg($"npc_c3_elder_{f}", c, Inner, true, true);
    }

    static void Mother(int f)
    {
        int w = 20, h = 28;
        var c = C(w, h);
        int lift = f;
        for (int j = 0; j < 7; j++) { c.Rect(5, 20 + j, 4, 1, K.Brown); c.Rect(11, 20 + j, 4, 1, K.Brown2); }
        c.Rect(4, 26, 5, 2, K.Brown3); c.Rect(11, 26, 5, 2, K.Brown3);
        // long skirt over the knees
        for (int j = 0; j < 8; j++) for (int i = 0; i < 14; i++)
            {
                int idx = i < 2 ? 3 : i > 11 ? 0 : 2; if (j == 7) idx = 0;
                c.Set(3 + i, 17 + j, K.TealR[idx]);
            }
        // torso
        for (int j = 0; j < 9; j++) for (int i = 0; i < 10; i++)
            {
                int idx = i < 2 ? 3 : i > 7 ? 1 : 2;
                c.Set(5 + i, 9 - lift + j, K.Terra.Shade(i > 7 ? 0.5 : 0.0).Light(i < 2 ? 0.4 : 0));
            }
        // headscarf + face
        Head(c, 10, 1 - lift, SkinA, new[] { K.Mustard[0], K.Mustard[1], K.Mustard[2], K.Mustard[3] }, true, 1);
        for (int j = 0; j < 4; j++) { c.Set(5, 5 - lift + j, K.Mustard[1]); c.Set(14, 5 - lift + j, K.Mustard[0]); }
        // baby wrapped in a cream shawl, cradled in the arms
        for (int j = 0; j < 8; j++) for (int i = 0; i < 10; i++)
            {
                double nx = (i - 4.5) / 5.2, ny = (j - 3.5) / 4.2;
                if (nx * nx + ny * ny > 1) continue;
                c.Set(4 + i, 13 - lift + j, nx - ny * 0.5 < -0.2 ? K.Linen[4] : nx > 0.4 ? K.Linen[1] : K.Linen[3]);
            }
        c.Rect(5, 13 - lift, 4, 3, SkinC); c.Set(6, 14 - lift, K.Brown2); c.Set(5, 13 - lift, K.Brown); c.Set(6, 13 - lift, K.Brown);
        if (f == 1) c.Set(9, 14 - lift, SkinC);
        // arm around the bundle
        c.Rect(4, 17 - lift, 11, 2, K.Terra); c.Rect(4, 17 - lift, 11, 1, K.RedR[3]);
        c.Rect(12, 17 - lift, 3, 2, SkinA);
        Reg($"npc_c3_mother_{f}", c, Inner, true, true);
    }
}
