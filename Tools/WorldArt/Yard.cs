namespace WorldArt;

/// <summary>Front of the house and the yard: chimney, smoke, window boxes, lantern, pots, flower clumps, well, wood, bench, laundry.</summary>
public static class Yard
{
    static Rgba Hx(string h) => Rgba.Hex(h);

    public static void Build()
    {
        S.Cur = "house_front";
        S.Add("prop_chimney", Chimney());
        S.Add("prop_windowbox_a", WindowBox(30, new[] { K.RedR[2], Hx("EAA5B5") }, 21));
        S.Add("prop_windowbox_b", WindowBox(24, new[] { K.Gold, K.Cream }, 22));
        S.Add("prop_lantern", Lantern());
        S.Add("prop_pot_red", Pot(K.RedR[3], new[] { K.RedR[2], K.RedR[3] }, 31));
        S.Add("prop_pot_blue", Pot(K.Blue, new[] { Hx("B8A0E0"), Hx("D4C0F0") }, 32));
        S.Add("prop_flowers_red", Clump(26, 20, new[] { K.RedR[1], K.RedR[2], K.RedR[3], K.RedR[4] }, false, 41));
        S.Add("prop_flowers_yellow", Clump(24, 18, new[] { Hx("C9952F"), K.Gold, Hx("F2D26B"), Hx("FBE9A8") }, false, 42));
        S.Add("prop_flowers_pink", Clump(26, 20, new[] { K.Pink[0], K.Pink[1], K.Pink[2], K.Pink[3] }, false, 43));
        S.Add("prop_flowers_lavender", Clump(22, 20, new[] { K.Purple[1], K.Purple[2], K.Purple[3], Hx("DCD0F0") }, true, 44));
        S.Add("prop_flowers_white", Clump(24, 18, new[] { K.Linen[1], K.Linen[3], K.Linen[4], K.Cream }, false, 45));
        for (int f = 0; f < 4; f++) S.Add($"prop_smoke_{f}", Smoke(f));
        S.Cur = "yard";
        S.Add("prop_well", Well());
        S.Add("prop_woodstack", WoodStack());
        S.Add("prop_chopping_block", ChoppingBlock());
        S.Add("prop_logs_split", SplitLogs());
        S.Add("prop_bench", Bench());
        S.Add("prop_wheelbarrow", Wheelbarrow());
        S.Cur = "animated";
        for (int f = 0; f < 2; f++) S.Add($"prop_laundry_{f}", Laundry(f));
    }

    // ---- chimney & smoke ----------------------------------------------------------------------------------
    static Canvas Chimney()
    {
        var c = new Canvas(24, 30);
        var br = K.Roof; var st = K.Stone;
        // brick shaft
        for (int y = 8; y < 30; y++)
            for (int x = 2; x < 22; x++)
            {
                int row = (y - 8) / 4, ly = (y - 8) % 4;
                int gx = x + (row % 2) * 4, lx = gx % 8;
                int idx = x < 5 ? 3 : x < 12 ? 2 : x < 18 ? 1 : 0;
                if (ly == 3) idx = 0; else if (lx == 0) idx = Math.Max(0, idx - 1);
                else if (Fx.R(gx / 8, row, 70) > 0.8) idx = Math.Min(3, idx + 1);
                var col = br[Math.Clamp(idx, 0, 4)];
                if (ly == 3 || lx == 0) col = Rgba.Mul(col, 0.8, 0.78, 0.82);
                c.Set(x, y, col);
            }
        // soot-darkened top, shadow cast by the cap
        for (int x = 2; x < 22; x++) { c.Set(x, 13, br[0].Shade(0.5)); if (x % 2 == 0) c.Set(x, 14, br[0].Shade(0.3)); }
        // stone cap with a flue opening on top
        c.Rect(0, 4, 24, 5, st[3]); c.HL(0, 4, 24, st[5]); c.HL(0, 8, 24, st[1]); c.VL(0, 4, 5, st[4]); c.VL(23, 4, 5, st[1]);
        c.Rect(1, 1, 22, 4, st[2]); c.HL(1, 1, 22, st[4]);
        c.Ellipse(12, 3.4, 8, 2.8, st[4]); c.Ellipse(12, 3.8, 6.4, 2.2, K.Dark[0]); c.HL(8, 4, 6, K.Dark[1]);
        c.HL(0, 8, 24, st[0]);
        // whitewash streak and a sooty smear
        c.VL(4, 15, 5, K.Plaster[3]); c.VL(5, 17, 3, K.Plaster[2]);
        return U.Finish(c, 0.4);
    }

    static Canvas Smoke(int f)
    {
        var c = new Canvas(30, 28);
        var cols = new[] { Hx("FBF3E8"), Hx("E4DDE4"), Hx("CBC4D4"), Hx("B2ABC0") };
        for (int i = 0; i < 3; i++)
        {
            double age = (f / 12.0 + i / 3.0) % 1.0;
            double cx = 11 + age * 8 + Math.Sin(age * 5 + i * 1.7) * 2.6, cy = 25 - age * 21;
            double r = 2.4 + age * 4.4;
            for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
                for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
                {
                    double dx = x + .5 - cx, dy = y + .5 - cy, d = Math.Sqrt(dx * dx + dy * dy);
                    if (d > r) continue;
                    // fade out by dithering
                    if (age > 0.55 && ((x + y) & 1) == 0 && Fx.R(x, y, 90 + i) < (age - 0.5) * 1.6) continue;
                    if (age > 0.82 && Fx.R(x, y, 95 + i) < 0.6) continue;
                    double light = -(dx * 0.5 + dy * 0.7) / r;
                    int idx = Math.Clamp((int)((0.45 + light * 0.5) * 4), 0, 3);
                    idx = 3 - idx;
                    if (age > 0.5) idx = Math.Min(3, idx + 1);
                    c.Set(x, y, cols[Math.Clamp(idx, 0, 3)]);
                }
        }
        return c;
    }

    // ---- porch dressing -----------------------------------------------------------------------------------
    static Canvas WindowBox(int w, Rgba[] blooms, int seed)
    {
        var c = new Canvas(w, 17);
        var t = K.TealR;
        // flowers & foliage heap
        for (int x = 0; x < w; x++)
        {
            int hgt = 4 + (int)(Fx.R(x / 3, 0, seed) * 4);
            for (int j = 0; j < hgt; j++)
            {
                int y = 9 - j;
                int idx = j > hgt - 2 ? 4 : j > 1 ? 3 : 1;
                c.Set(x, y, K.Leaf[Math.Clamp(idx - (x % 3 == 0 ? 1 : 0), 0, 5)]);
            }
        }
        for (int i = 0; i < w / 3; i++)
        {
            int x = 1 + i * 3 + (int)(Fx.R(i, 1, seed) * 2), y = 1 + (int)(Fx.R(i, 2, seed) * 4);
            var col = blooms[i % blooms.Length];
            c.Rect(x, y, 3, 2, col); c.Set(x, y, Rgba.Clear); c.Set(x + 1, y - 1, col); c.Set(x + 1, y, Rgba.Mul(col, 1.15, 1.1, 1.1)); c.Set(x + 2, y + 1, Rgba.Mul(col, 0.8, 0.75, 0.8));
        }
        // box
        c.Rect(0, 9, w, 7, t[2]); c.HL(0, 9, w, t[4]); c.HL(0, 10, w, t[3]);
        c.HL(0, 15, w, t[0]); c.VL(0, 10, 6, t[3]); c.VL(w - 1, 10, 6, t[1]);
        c.HL(1, 12, w - 2, t[1]);
        for (int x = 4; x < w - 2; x += 8) { c.Set(x, 11, K.Mustard[3]); c.Set(x, 14, K.Mustard[3]); }
        // trailing ivy
        for (int x = 3; x < w - 3; x += 9) { c.Set(x, 16, K.Leaf[3]); c.Set(x + 1, 16, K.Leaf[2]); }
        return U.Finish(c, 0.4);
    }

    static Canvas Lantern()
    {
        var c = new Canvas(14, 34);
        var ir = K.Iron;
        // post with scrolled foot
        for (int y = 12; y < 33; y++) { c.Set(6, y, ir[3]); c.Set(7, y, ir[2]); c.Set(8, y, ir[0]); }
        c.Rect(4, 31, 6, 2, ir[2]); c.HL(4, 31, 6, ir[4]); c.HL(4, 33, 6, ir[0]);
        c.Set(3, 30, ir[2]); c.Set(10, 30, ir[1]);
        // bracket arms
        c.Line(7, 20, 2, 16, ir[2]); c.Line(7, 20, 12, 16, ir[2]);
        // lantern body: warm glass
        c.Rect(3, 3, 8, 9, Hx("F9E093")); c.Rect(4, 4, 6, 7, Hx("FDEFB4")); c.Rect(5, 5, 4, 5, Hx("FFF8D6"));
        c.Set(7, 8, K.Cream); c.Set(8, 9, Hx("F2D26B"));
        c.VL(3, 3, 9, ir[2]); c.VL(10, 3, 9, ir[0]); c.VL(7, 3, 9, ir[1]);
        c.HL(3, 11, 8, ir[1]); c.HL(3, 3, 8, ir[2]);
        // cap and ring
        c.Rect(2, 2, 10, 1, ir[3]); c.Rect(4, 1, 6, 1, ir[3]); c.Rect(6, 0, 2, 1, ir[4]);
        c.Rect(4, 12, 6, 1, ir[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas Pot(Rgba potCol, Rgba[] blooms, int seed)
    {
        var c = new Canvas(14, 20);
        var l = K.Leaf;
        c.Ellipse(7, 7, 6, 5, l[3]); c.Ellipse(5, 6, 3.6, 3.6, l[4]); c.Ellipse(9.5, 5.5, 3.2, 3.2, l[2]);
        for (int i = 0; i < 6; i++)
        {
            int x = 2 + (int)(Fx.R(i, 0, seed) * 9), y = 1 + (int)(Fx.R(i, 1, seed) * 7);
            var col = blooms[i % blooms.Length];
            c.Set(x, y, col); c.Set(x + 1, y, col); c.Set(x, y - 1, col); c.Set(x + 1, y + 1, Rgba.Mul(col, 0.8, 0.8, 0.85)); c.Set(x, y, K.Cream);
        }
        // pot
        var pr = potCol == K.Blue ? K.BlueR : K.Roof;
        c.Rect(2, 11, 10, 3, pr[3]); c.HL(2, 11, 10, pr[4]); c.HL(2, 13, 10, pr[1]);
        c.Rect(3, 14, 8, 5, pr[2]); c.VL(3, 14, 5, pr[3]); c.VL(10, 14, 5, pr[1]); c.HL(3, 18, 8, pr[0]);
        c.Set(6, 16, pr[1]);
        return U.Finish(c, 0.4);
    }

    /// <summary>Flower bush for the beds along the house: foliage mound with many blooms (or lavender spikes).</summary>
    static Canvas Clump(int w, int h, Rgba[] bl, bool spikes, int seed)
    {
        var c = new Canvas(w, h);
        var ramp = K.LeafWarm;
        if (!spikes)
        {
            Draw.Blob(c, w / 2.0, h - 6, w / 2.0 - 1, 6, ramp, seed, 0.0, 2);
            Draw.Blob(c, w * 0.32, h - 8, w * 0.22, 6, ramp, seed + 1, 0.05, 2);
            Draw.Blob(c, w * 0.68, h - 9, w * 0.22, 6, ramp, seed + 2, 0.0, 2);
            // blooms
            for (int i = 0; i < w; i++)
            {
                int x = 1 + (int)(Fx.R(i, 0, seed) * (w - 3)), y = 1 + (int)(Fx.R(i, 1, seed) * (h - 6));
                if (!c.Opaque(x, y) && !c.Opaque(x, y + 1)) continue;
                var col = bl[(int)(Fx.R(i, 2, seed) * 3)];
                c.Rect(x, y, 3, 3, col); c.Set(x, y, Rgba.Clear); c.Set(x + 2, y + 2, Rgba.Clear);
                c.Set(x + 1, y + 1, bl[3]); c.Set(x + 1, y, bl[2]); c.Set(x + 2, y + 1, bl[0]); c.Set(x, y + 1, bl[2]);
            }
        }
        else
        {
            Draw.Blob(c, w / 2.0, h - 4, w / 2.0 - 1, 4, ramp, seed, -0.05, 2);
            for (int i = 0; i < 6; i++)
            {
                int bx = 3 + i * (w - 6) / 5, top = 2 + (int)(Fx.R(i, 0, seed) * 6);
                int lean = i % 2 == 0 ? 0 : 1;
                for (int y = top; y < h - 4; y++)
                {
                    int x = bx + (y < top + 6 && lean == 1 ? (y - top) / 4 : 0);
                    if (y < top + 8) { c.Set(x, y, bl[(y - top) % 3 == 0 ? 3 : (y - top) % 3 == 1 ? 2 : 1]); c.Set(x + 1, y, bl[(y - top) % 2 == 0 ? 1 : 0]); }
                    else c.Set(x, y, ramp[3]);
                }
            }
        }
        // grounding row of dark leaf
        for (int x = 2; x < w - 2; x++) if (c.Opaque(x, h - 2)) c.Set(x, h - 1, ramp[0]);
        return U.Finish(c, 0.38);
    }

    // ---- yard ---------------------------------------------------------------------------------------------
    static Canvas Well()
    {
        var c = new Canvas(44, 52);
        var st = K.Stone;
        // stone ring (cylinder)
        double cx = 22, ty = 42, by = 49, rx = 16, ry = 7;
        for (int y = 30; y < 52; y++)
            for (int x = 3; x < 41; x++)
            {
                double dx = (x + .5 - cx) / rx;
                bool inTop = dx * dx + ((y + .5 - ty) / ry) * ((y + .5 - ty) / ry) <= 1;
                bool inBody = y >= ty && y <= by && Math.Abs(dx) <= 1;
                bool inBot = dx * dx + ((y + .5 - by) / ry) * ((y + .5 - by) / ry) <= 1 && y >= by;
                if (!(inTop || inBody || inBot)) continue;
                if (inTop && !(y >= ty && inBody))
                {
                    // rim top surface
                    c.Set(x, y, st[4]);
                    continue;
                }
                double t = (dx + 1) / 2;
                int row = (y - 36) / 5, ly = (y - 36) % 5;
                int off = (row % 2) * 6;
                int idx = t < 0.2 ? 4 : t < 0.42 ? 3 : t < 0.7 ? 2 : t < 0.88 ? 1 : 0;
                int bx = (x + off) % 12;
                var col = Rgba.Mul(st[Math.Clamp(idx, 0, 5)], 1.03, 0.98, 0.9);
                if (ly == 4 || bx == 0) col = K.Mortar.Shade(0.2 + t * 0.4);
                else if (ly == 0) col = Rgba.Mul(st[Math.Clamp(idx + 1, 0, 5)], 1.03, 0.98, 0.9);
                c.Set(x, y, col);
            }
        // dark water mouth
        c.Ellipse(cx, ty - 0.5, 13, 5.2, st[4]);
        c.Ellipse(cx, ty, 11.8, 4.4, K.Dark[0]);
        c.Ellipse(cx, ty + 0.9, 10.2, 3.4, K.TealR[0]);
        c.Ellipse(cx - 1, ty + 1.2, 6.5, 1.8, K.TealR[1]); c.HL(17, (int)ty + 1, 4, K.TealR[3]);
        c.HL(11, (int)ty - 3, 9, K.Dark[1]);
        // wooden posts
        foreach (var px in new[] { 5, 36 })
            for (int y = 11; y < 41; y++) for (int x = 0; x < 4; x++) c.Set(px + x, y, K.Wood[x == 0 ? 4 : x == 1 ? 3 : x == 2 ? 2 : 0]);
        // roof: terracotta pitched plane
        c.Poly(new (double, double)[] { (0, 14), (7, 2), (37, 2), (44, 14) }, K.Roof[2]);
        for (int y = 2; y < 14; y++)
            for (int x = 0; x < 44; x++)
            {
                if (!c.Opaque(x, y) || c.Get(x, y) != K.Roof[2]) continue;
                int row = (y - 2) / 3, ly = (y - 2) % 3;
                int idx = ly == 2 ? 1 : ly == 0 ? 3 : 2;
                if (((x + row * 3) % 6) == 0) idx = Math.Max(0, idx - 1);
                if (row == 0 && ly == 0) idx = 4;
                c.Set(x, y, K.Roof[idx]);
            }
        for (int x = 1; x < 43; x++) if (c.Opaque(x, 13)) { c.Set(x, 13, K.Roof[0]); }
        // eave shadow on the posts and beam
        c.Rect(5, 14, 36, 3, K.Wood[1]); c.HL(5, 14, 36, K.Wood[3]); c.HL(5, 16, 36, K.Wood[0]);
        // windlass with rope, bucket
        c.Rect(7, 21, 30, 3, K.Wood[3]); c.HL(7, 21, 30, K.Wood[5]); c.HL(7, 23, 30, K.Wood[0]);
        for (int x = 14; x < 30; x += 2) { c.Set(x, 22, K.Parch); c.Set(x + 1, 22, K.Sand); }
        c.Rect(5, 20, 3, 5, K.Iron[2]); c.Rect(37, 20, 3, 5, K.Iron[1]); c.Set(5, 20, K.Iron[4]);
        c.Line(22, 24, 22, 30, K.Tan); c.Line(23, 24, 23, 30, K.Sand);
        c.Rect(18, 30, 8, 6, K.Wood[2]); c.VL(18, 30, 6, K.Wood[4]); c.VL(25, 30, 6, K.Wood[0]); c.HL(18, 31, 8, K.Iron[3]); c.HL(18, 34, 8, K.Iron[2]);
        c.Ellipse(22, 30, 4, 1.2, K.Dark[1]);
        c.Line(18, 30, 21, 26, K.Iron[2]); c.Line(25, 30, 23, 26, K.Iron[2]);
        return U.Finish(c, 0.38);
    }

    static Canvas WoodStack()
    {
        var c = new Canvas(42, 28);
        var pn = K.Pine; var w = K.Wood;
        // shadowy back
        c.Rect(1, 5, 40, 22, w[0]);
        // rows of log ends, staggered
        int[] rowY = { 22, 15, 8 };
        for (int r = 0; r < 3; r++)
        {
            for (int i = 0; i < 6; i++)
            {
                double cx = 4 + i * 7.4 + (r % 2) * 3.7, cy = rowY[r];
                if (cx > 38) continue;
                double rad = 3.9 - (Fx.R(i, r, 80) * 0.5);
                c.Disc(cx, cy, rad + 0.6, w[1]);
                c.Disc(cx, cy, rad, pn[2 + ((i + r) % 2)]);
                c.Disc(cx - 0.4, cy - 0.4, rad - 1.6, pn[3]);
                c.Set((int)cx, (int)cy, pn[1]); c.Set((int)cx + 1, (int)cy - 1, pn[1]);
                c.Set((int)(cx - rad), (int)(cy - rad / 2), pn[4]);
                c.Set((int)(cx + rad - 1), (int)(cy + rad / 2), w[1]);
            }
        }
        // top layer logs lying lengthwise (top surface)
        c.Rect(1, 0, 40, 6, w[3]);
        for (int x = 1; x < 41; x++) { c.Set(x, 0, w[5]); c.Set(x, 1, w[4]); c.Set(x, 4, w[2]); c.Set(x, 5, w[0]); }
        for (int x = 1; x < 41; x += 8) c.VL(x, 1, 5, w[0]);
        for (int x = 5; x < 40; x += 9) { c.Set(x, 2, w[5]); c.Set(x + 1, 2, w[4]); }
        // side stakes
        foreach (var px in new[] { 0, 40 }) for (int y = 2; y < 28; y++) { c.Set(px, y, w[2]); c.Set(px + 1, y, w[0]); }
        c.HL(0, 27, 42, w[0]);
        return U.Finish(c, 0.38);
    }

    static Canvas ChoppingBlock()
    {
        var c = new Canvas(20, 20);
        var pn = K.Pine; var w = K.Wood;
        // stump side
        for (int y = 8; y < 19; y++)
            for (int x = 3; x < 15; x++)
            {
                int idx = x < 6 ? 3 : x < 10 ? 2 : x < 13 ? 1 : 0;
                if (y > 16) idx = Math.Max(0, idx - 1);
                if ((x + y / 2) % 5 == 0) idx = Math.Max(0, idx - 1);
                c.Set(x, y, w[idx]);
            }
        c.Ellipse(9, 18, 6.5, 1.8, w[0]);
        // top rings
        c.Ellipse(9, 8, 6.4, 3.2, pn[3]);
        c.Ellipse(9, 8, 4.4, 2.2, pn[2]); c.Ellipse(9, 8, 2.2, 1.1, pn[3]); c.Set(9, 8, pn[1]);
        c.HL(4, 6, 4, pn[4]);
        c.Set(6, 10, pn[1]); c.Set(12, 9, pn[1]);
        // axe: handle and head sunk into the top
        c.Line(9, 8, 16, 1, w[4]); c.Line(10, 8, 17, 1, w[2]);
        c.Rect(7, 5, 4, 4, K.Iron[3]); c.HL(7, 5, 4, K.Iron[4]); c.VL(7, 5, 4, K.Iron[4]); c.Set(10, 8, K.Iron[1]); c.Set(6, 6, K.Iron[4]);
        return U.Finish(c, 0.38);
    }

    static Canvas SplitLogs()
    {
        var c = new Canvas(18, 10);
        var pn = K.Pine; var w = K.Wood;
        void Log(int x, int y, int len, int th)
        {
            c.Rect(x, y, len, th, w[3]); c.HL(x, y, len, w[5]); c.HL(x, y + th - 1, len, w[0]);
            c.Rect(x + len - 1, y, 1, th, pn[3]); c.Set(x + len - 1, y + th / 2, pn[1]);
            c.Set(x + 3, y + 1, w[1]); c.Set(x + 6, y + th - 2, w[2]);
        }
        Log(1, 5, 11, 4); Log(5, 2, 12, 3); Log(0, 1, 6, 3);
        return U.Finish(c, 0.4);
    }

    static Canvas Bench()
    {
        var c = new Canvas(42, 26);
        var p = K.Wood; var l = K.Pine;
        // legs and struts
        foreach (var px in new[] { 3, 34 }) for (int y = 16; y < 26; y++) for (int x = 0; x < 4; x++) c.Set(px + x, y, p[x == 0 ? 3 : x == 3 ? 0 : 2]);
        c.Rect(6, 22, 30, 1, p[1]);
        // backrest posts and slats
        foreach (var px in new[] { 3, 34 }) for (int y = 0; y < 16; y++) for (int x = 0; x < 4; x++) c.Set(px + x, y, p[x == 0 ? 4 : x == 3 ? 1 : 3]);
        for (int s = 0; s < 2; s++)
        {
            int y = 2 + s * 5;
            c.Rect(3, y, 36, 4, l[3]); c.HL(3, y, 36, l[4]); c.HL(3, y + 3, 36, l[1]);
            c.VL(3, y, 4, l[4]); c.VL(38, y, 4, l[1]);
            c.Set(10, y + 1, l[2]); c.Set(25, y + 2, l[2]);
        }
        // seat: top face and front edge
        c.Rect(1, 12, 40, 4, l[4]); c.HL(1, 12, 40, Hx("F6DDB0")); c.HL(1, 15, 40, l[2]);
        c.HL(1, 14, 40, l[3]);
        for (int x = 12; x < 40; x += 14) c.VL(x, 12, 3, l[2]);
        c.Rect(1, 16, 40, 3, l[2]); c.HL(1, 16, 40, l[3]); c.HL(1, 18, 40, l[0]);
        return U.Finish(c, 0.38);
    }

    static Canvas Wheelbarrow()
    {
        var c = new Canvas(34, 20);
        var w = K.Wood;
        // tray with soil/leaves
        c.Poly(new (double, double)[] { (6, 3), (28, 3), (25, 13), (9, 13) }, K.TealR[2]);
        c.Poly(new (double, double)[] { (7, 3), (27, 3), (26, 5), (8, 5) }, K.Soil);
        c.HL(8, 4, 18, K.SoilLight); c.Set(12, 3, K.Green); c.Set(19, 3, K.DGreen); c.Set(23, 3, K.Green);
        c.HL(7, 5, 20, K.TealR[4]);
        for (int x = 9; x < 26; x++) c.Set(x, 12, K.TealR[0]);
        c.VL(8, 6, 6, K.TealR[3]); c.VL(25, 6, 6, K.TealR[1]);
        // handles and leg
        c.Line(26, 10, 33, 6, w[3]); c.Line(26, 11, 33, 7, w[1]);
        c.Line(8, 10, 1, 7, w[3]); c.Line(8, 11, 1, 8, w[1]);
        c.Rect(10, 13, 2, 6, w[2]); c.Rect(22, 13, 2, 6, w[1]);
        // wheel at the front
        c.Disc(4, 15, 3.4, K.Dark[1]); c.Disc(4, 15, 2.2, K.Iron[3]); c.Set(4, 15, K.Iron[1]);
        c.HL(3, 18, 3, K.Dark[0]);
        return U.Finish(c, 0.4);
    }

    // ---- washing line --------------------------------------------------------------------------------------
    static Canvas Laundry(int f)
    {
        const int W = 108, H = 44;
        var c = new Canvas(W, H);
        double ph = f * Math.PI;
        // poles
        foreach (var px in new[] { 2, 102 })
        {
            for (int y = 3; y < H; y++) for (int x = 0; x < 4; x++) c.Set(px + x, y, K.Wood[x == 0 ? 4 : x == 3 ? 0 : x == 1 ? 3 : 2]);
            c.Set(px - 1, 3, K.Wood[2]); c.Set(px + 4, 3, K.Wood[1]); c.Set(px, 2, K.Wood[5]); c.Set(px + 1, 2, K.Wood[4]);
            c.HL(px - 1, H - 1, 6, K.Wood[0]);
        }
        int LineY(int x) => 5 + (int)Math.Round(3.0 * Math.Sin((x - 4) / 98.0 * Math.PI));
        void Cloth(int x0, int w, int h, Rgba[] ramp, int style, int seed)
        {
            var cl = new Canvas(w + 6, h + 4);
            for (int y = 0; y < h; y++)
            {
                double sway = Math.Sin(ph + y * 0.22 + seed) * 1.0 * Math.Min(1, y / (h * 0.6)) + (f == 0 ? 0 : 0.7);
                int sh = (int)Math.Round(sway);
                int xa = 0, xb = w;
                if (style == 1) { xa = y < h / 3 ? -2 : 1; xb = y < h / 3 ? w + 2 : w - 1; }          // tunic with sleeves
                if (style == 2) { xa = (int)(y * 0.28); xb = w - (int)(y * 0.28); }                      // dress
                for (int x = xa; x < xb; x++)
                {
                    double u = (double)(x - xa) / Math.Max(1, xb - xa);
                    int idx = u < 0.25 ? 3 : u < 0.6 ? 2 : 1;
                    if (y % 5 == 4 && style == 3) idx = 1;
                    if (style == 3 && (y / 3) % 2 == 0) idx = Math.Max(0, idx - 1);
                    if (y == 0) idx = Math.Min(4, idx + 1);
                    if (y > 2 && (x - xa) % 5 == 3 && style == 0) idx = Math.Max(0, idx - 1);        // folds
                    cl.Set(x + 3 + sh, y + 2, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
                }
            }
            cl.SelOutline(0.45);
            c.Blit(cl, x0 - 3, LineY(x0 + w / 2) - 2);
            c.Set(x0 + 1, LineY(x0 + 1) + 1, K.Wood[4]); c.Set(x0 + w - 2, LineY(x0 + w - 2) + 1, K.Wood[4]);
        }
        Cloth(10, 20, 22, K.Linen, 0, 1);
        Cloth(36, 14, 14, K.BlueR, 1, 2);
        Cloth(56, 12, 17, K.RedR, 2, 3);
        Cloth(72, 6, 8, K.Mustard, 0, 4);
        Cloth(82, 6, 8, K.TealR, 0, 5);
        Cloth(92, 8, 13, K.Linen, 3, 6);
        // the line
        for (int x = 3; x < W - 3; x++) { c.Set(x, LineY(x), K.Tan); if (x % 3 == 0) c.Set(x, LineY(x) - 1, K.Sand); }
        return c;
    }
}
