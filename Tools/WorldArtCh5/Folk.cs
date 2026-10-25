namespace WorldArtCh5;

/// <summary>Camp residents: simple chibi figures matching the children's proportions, each with a 2-frame idle.</summary>
public static class Folk
{
    public record Look(Rgba[] Skin, Rgba[] Hair, int Style, Rgba[] Top, Rgba[] Bottom, bool Skirt, Rgba[]? Scarf = null, bool Beard = false);

    static readonly Rgba Blush = Rgba.Hex("E7A09A"), Eye = Rgba.Hex("2A1F22"), Shoe = Rgba.Hex("5A4236"), Shoe2 = Rgba.Hex("3C2C28");

    static void Head(Canvas c, int x0, int y0, Look L)
    {
        var skin = L.Skin;
        c.RectN(x0, y0, 8, 8, skin[1]);
        c.VL(x0, y0 + 1, 6, skin[0]); c.VL(x0 + 7, y0 + 1, 6, skin[0]);
        c.HL(x0 + 1, y0 + 7, 6, skin[0]);
        var hair = L.Hair;
        switch (L.Style)
        {
            case 0: // short
                c.RectN(x0 - 1, y0 - 1, 10, 4, hair[1]); c.HL(x0, y0 - 1, 5, hair[2]);
                c.VL(x0 - 1, y0 + 2, 2, hair[0]); c.VL(x0 + 8, y0 + 2, 2, hair[0]);
                c.HL(x0 + 1, y0 + 2, 6, hair[0]);
                break;
            case 1: // long
                c.RectN(x0 - 1, y0 - 1, 10, 4, hair[1]); c.HL(x0, y0 - 1, 5, hair[2]);
                c.Rect(x0 - 1, y0 + 2, 2, 7, hair[1]); c.Rect(x0 + 7, y0 + 2, 2, 7, hair[0]);
                c.HL(x0 + 1, y0 + 2, 6, hair[0]);
                break;
            case 2: // headscarf
                {
                    var sc = L.Scarf ?? C.Clay;
                    c.RectN(x0 - 1, y0 - 1, 10, 5, sc[2]); c.HL(x0, y0 - 1, 5, sc[3]);
                    c.Rect(x0 - 1, y0 + 3, 2, 5, sc[1]); c.Rect(x0 + 7, y0 + 3, 2, 5, sc[0]);
                    c.HL(x0 + 1, y0 + 3, 6, sc[1]);
                    c.Set(x0 + 1, y0 + 8, sc[1]); c.Set(x0 + 6, y0 + 8, sc[0]);
                    break;
                }
            case 3: // bun
                c.RectN(x0 - 1, y0 - 1, 10, 4, hair[1]); c.HL(x0, y0 - 1, 5, hair[2]);
                c.VL(x0 - 1, y0 + 2, 3, hair[0]); c.VL(x0 + 8, y0 + 2, 3, hair[0]);
                c.HL(x0 + 1, y0 + 2, 6, hair[0]);
                c.Rect(x0 + 3, y0 - 3, 3, 2, hair[1]); c.Set(x0 + 3, y0 - 3, hair[2]);
                break;
            case 4: // cap
                {
                    var cap = L.Scarf ?? C.Rust;
                    c.RectN(x0 - 1, y0 - 1, 10, 4, cap[2]); c.HL(x0, y0 - 1, 5, cap[3]);
                    c.HL(x0 - 1, y0 + 2, 10, cap[0]);
                    c.VL(x0 - 1, y0 + 3, 2, hair[0]); c.VL(x0 + 8, y0 + 3, 2, hair[0]);
                    break;
                }
        }
        // face
        int ey = L.Style == 2 ? y0 + 4 : y0 + 3;
        c.Set(x0 + 2, ey, Eye); c.Set(x0 + 2, ey + 1, Eye); c.Set(x0 + 5, ey, Eye); c.Set(x0 + 5, ey + 1, Eye);
        c.Set(x0 + 1, ey + 2, Blush); c.Set(x0 + 6, ey + 2, Blush);
        c.HL(x0 + 3, ey + 3, 2, skin[0]);
        if (L.Beard)
        {
            var g = C.HairGr;
            c.Rect(x0 + 1, ey + 2, 6, 3, g[1]); c.HL(x0 + 2, ey + 2, 4, g[2]); c.Set(x0 + 1, ey + 2, skin[1]); c.Set(x0 + 6, ey + 2, skin[1]);
            c.HL(x0 + 3, ey + 3, 2, g[0]);
        }
    }

    static void Torso(Canvas c, int x, int y, int w, int h, Look L, bool sleeves = true, int armRows = 0)
    {
        var t = L.Top;
        c.Rect(x, y, w, h, t[2]);
        c.VL(x, y, h, t[3]); c.VL(x + w - 1, y, h, t[1]); c.VL(x + w - 2, y, h, t[1]);
        c.HL(x, y + h - 1, w, t[0]);
        c.Set(x + w / 2 - 1, y, L.Skin[1]); c.Set(x + w / 2, y, L.Skin[1]);
        if (sleeves)
        {
            int ah = armRows == 0 ? h : armRows;
            c.Rect(x - 1, y + 1, 1, ah, t[1]); c.Rect(x + w, y + 1, 1, ah, t[0]);
            c.Set(x - 1, y + ah, L.Skin[1]); c.Set(x + w, y + ah, L.Skin[0]);
        }
    }

    /// <summary>Standing adult, 16x24.</summary>
    public static Canvas Stand(Look L, int frame, bool carry = false)
    {
        var c = new Canvas(16, 24);
        int bob = frame;
        int y0 = 2 + bob;
        Head(c, 4, y0, L);
        int ty = y0 + 8;
        Torso(c, 3, ty, 10, 17 - ty, L);
        // hands
        c.Set(2, 16, L.Skin[1]); c.Set(13, 16, L.Skin[0]);
        c.HL(3, 16, 10, L.Bottom[0]);                 // belt
        if (L.Skirt)
        {
            for (int y = 17; y <= 21; y++)
            {
                int x0 = y < 19 ? 3 : 2, x1 = y < 19 ? 12 : 13;
                for (int x = x0; x <= x1; x++)
                    c.Set(x, y, (x - x0) < 2 ? L.Bottom[3] : (x1 - x) < 3 ? L.Bottom[1] : L.Bottom[2]);
                if (y == 21) c.HL(x0, y, x1 - x0 + 1, L.Bottom[0]);
                if ((y == 18 || y == 20)) c.Set(6 + (y & 2), y, L.Bottom[1]);
            }
            c.Rect(5, 22, 2, 1, L.Skin[0]); c.Rect(9, 22, 2, 1, L.Skin[0]);
        }
        else
        {
            for (int y = 17; y <= 22; y++)
            {
                c.Rect(4, y, 4, 1, L.Bottom[2]); c.Rect(8, y, 4, 1, L.Bottom[1]);
                c.Set(4, y, L.Bottom[3]); c.Set(7, y, L.Bottom[0]);
            }
        }
        c.Rect(4, 23, 4, 1, Shoe); c.Rect(8, 23, 4, 1, Shoe2);
        if (carry)
        {
            // water jug in the right hand, hanging
            c.Rect(13, 16, 3, 5, K.Terra); c.VL(13, 16, 5, C.Clay[3]); c.HL(13, 20, 3, C.Clay[0]); c.Set(14, 15, K.Brown2);
        }
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Adult sitting cross-legged on the ground, 16x19.</summary>
    public static Canvas Sit(Look L, int frame, int extra = 0)
    {
        var c = new Canvas(16, 19);
        int bob = frame;
        int y0 = 2 + bob;
        Head(c, 4, y0, L);
        int ty = y0 + 8;
        Torso(c, 3, ty, 10, 14 - ty, L, true, 3);
        c.Set(2, 14, L.Skin[1]); c.Set(13, 14, L.Skin[0]);
        // crossed legs: wide lump with two knees
        for (int y = 14; y <= 18; y++)
        {
            int x0 = y == 14 ? 3 : y == 18 ? 1 : 0, x1 = y == 14 ? 12 : y == 18 ? 14 : 15;
            for (int x = x0; x <= x1; x++)
            {
                int tone = x - x0 < 2 ? 3 : (x1 - x < 3 ? 1 : 2);
                if (y == 18) tone = 0;
                var col = L.Bottom[Math.Clamp(tone, 0, L.Bottom.Length - 1)];
                if (L.Skirt && y >= 16 && (x + y) % 5 == 0) col = L.Bottom[Math.Max(0, tone - 1)];
                c.Set(x, y, col);
            }
        }
        c.Rect(0, 17, 3, 2, Shoe); c.Rect(13, 17, 3, 2, Shoe2);
        c.Set(7, 16, L.Bottom[0]); c.Set(8, 17, L.Bottom[0]);
        if (extra == 1)
        {
            // ladle/stirring stick in the raised right hand
            c.Set(14, 11 + frame, L.Skin[0]); c.Set(15, 11 + frame, L.Skin[0]);
            c.Line(15, 12 + frame, 15, 17, K.Wood[2]);
            c.Set(15, 12 + frame, K.Wood[4]);
        }
        else if (extra == 2)
        {
            // cane held upright
            c.Line(15, 10, 15, 18, K.Wood[2]); c.Set(15, 10, K.Wood[4]); c.Set(14, 10, K.Wood[3]);
        }
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Child standing / playing, 12x17. frame 1 raises the arms.</summary>
    public static Canvas Kid(Look L, int frame)
    {
        var c = new Canvas(12, 17);
        Head(c, 2, 2, L);
        int ty = 10;
        var t = L.Top;
        c.Rect(2, ty, 8, 4, t[2]); c.VL(2, ty, 4, t[3]); c.VL(9, ty, 4, t[1]); c.HL(2, ty + 3, 8, t[0]);
        c.Set(5, ty, L.Skin[1]); c.Set(6, ty, L.Skin[1]);
        if (frame == 0)
        {
            c.Rect(1, ty + 1, 1, 3, t[1]); c.Rect(10, ty + 1, 1, 3, t[0]);
            c.Set(1, ty + 3, L.Skin[1]); c.Set(10, ty + 3, L.Skin[0]);
        }
        else
        {
            c.Rect(0, ty - 1, 2, 2, t[1]); c.Rect(10, ty - 1, 2, 2, t[0]);
            c.Set(0, ty - 2, L.Skin[1]); c.Set(11, ty - 2, L.Skin[0]);
            c.Set(1, ty + 1, t[1]); c.Set(10, ty + 1, t[0]);
        }
        if (L.Skirt)
        {
            c.Rect(2, 14, 8, 2, L.Bottom[2]); c.VL(2, 14, 2, L.Bottom[3]); c.HL(2, 15, 8, L.Bottom[0]);
            c.Set(4, 16, Shoe); c.Set(7, 16, Shoe2); c.Set(5, 16, L.Skin[0]); c.Set(6, 16, L.Skin[0]);
        }
        else
        {
            c.Rect(3, 14, 3, 2, L.Bottom[2]); c.Rect(6, 14, 3, 2, L.Bottom[1]);
            c.Rect(3, 16, 3, 1, Shoe); c.Rect(6, 16, 3, 1, Shoe2);
        }
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Small child sitting on the ground hugging their knees, 12x15.</summary>
    public static Canvas KidSit(Look L, int frame)
    {
        var c = new Canvas(12, 15);
        Head(c, 2, 2 + frame, L);
        var t = L.Top;
        int ty = 10 + frame;
        c.Rect(2, ty, 8, 3 - frame, t[2]); c.VL(2, ty, 3 - frame, t[3]); c.VL(9, ty, 3 - frame, t[1]); c.HL(2, 12, 8, t[0]);
        // knees drawn up in front, arms wrapped around them
        c.Rect(2, 12, 8, 2, L.Bottom[2]); c.VL(2, 12, 2, L.Bottom[3]); c.VL(9, 12, 2, L.Bottom[1]);
        c.Rect(1, 11, 2, 3, t[1]); c.Rect(9, 11, 2, 3, t[0]);
        c.Set(5, 12, L.Skin[1]); c.Set(6, 12, L.Skin[1]);
        c.HL(3, 14, 6, L.Bottom[0]);
        c.Rect(2, 14, 2, 1, Shoe); c.Rect(8, 14, 2, 1, Shoe2);
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Mother sitting with a toddler in her lap, 18x20.</summary>
    public static Canvas MotherChild(Look M, Look T, int frame)
    {
        var c = new Canvas(18, 20);
        var m = Sit(M, frame);
        c.Blit(m, 1, 1);
        // toddler on the lap (right side), wrapped in a blanket
        int tx = 9, ty = 11 + frame;
        var skin = T.Skin;
        c.RectN(tx, ty, 6, 6, skin[1]); c.VL(tx + 5, ty + 1, 4, skin[0]);
        c.Rect(tx, ty - 1, 6, 2, T.Hair[1]); c.Set(tx, ty - 1, Rgba.Clear); c.Set(tx + 5, ty - 1, Rgba.Clear);
        c.Set(tx + 1, ty + 3, Eye); c.Set(tx + 4, ty + 3, Eye);
        c.Set(tx, ty + 4, Blush); c.Set(tx + 5, ty + 4, Blush);
        c.Rect(tx - 1, ty + 6, 8, 18 - ty - 5, C.Mustard[2]);
        c.VL(tx - 1, ty + 6, 18 - ty - 5, C.Mustard[3]); c.VL(tx + 6, ty + 6, 18 - ty - 5, C.Mustard[1]);
        c.HL(tx - 1, 18, 8, C.Mustard[0]);
        // little waving arm
        c.Set(tx + 7, ty + 4 - frame, skin[1]); c.Set(tx + 7, ty + 5 - frame, skin[0]);
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Make a 2-frame breathing idle from a one-frame sprite: frame 1 drops the head/body by one pixel.</summary>
    public static Canvas Breathe(Canvas src, int headRows)
    {
        var o = new Canvas(src.W, src.H);
        // keep feet area; squash by removing one torso row
        int cut = headRows + 3;
        for (int y = 0; y < src.H; y++)
        {
            int sy = y;
            if (y > 0 && y <= cut) sy = y - 1;           // upper part drops one row
            if (y == 0) { continue; }
            for (int x = 0; x < src.W; x++) { var p = src.Get(x, sy); if (!p.IsClear) o.Set(x, y, p); }
        }
        // restore the rows below the cut from the source
        for (int y = cut + 1; y < src.H; y++) for (int x = 0; x < src.W; x++) { var p = src.Get(x, y); o.Set(x, y, p); }
        return o;
    }

    // ---- donkey ----------------------------------------------------------------------------------------------------
    public static Canvas Donkey(int frame)
    {
        var c = new Canvas(30, 24);
        var body = new[] { Rgba.Hex("6F6258"), Rgba.Hex("857769"), Rgba.Hex("9C8D7C"), Rgba.Hex("B5A692") };
        var belly = Rgba.Hex("CDBFAA");
        // legs
        foreach (var lx in new[] { 7, 10, 20, 23 }) { c.Rect(lx, 15, 3, 8, body[1]); c.VL(lx, 15, 8, body[2]); c.Rect(lx, 22, 3, 2, C.Out); }
        // body
        c.Ellipse(15, 12, 11.5, 6.5, body[2]);
        c.Paint((x, y) => y >= 14 && y <= 17 && x >= 6 && x <= 25 && ((x - 15.5) * (x - 15.5) / 110.0 + (y - 12.5) * (y - 12.5) / 40.0) < 1, belly);
        c.Paint((x, y) => y <= 9 && ((x - 15.5) * (x - 15.5) / 132.0 + (y - 12.0) * (y - 12.0) / 42.0) < 1, body[3]);
        // pack blanket (so it reads as a working animal)
        c.Rect(11, 6, 9, 5, C.Clay[2]); c.HL(11, 6, 9, C.Clay[3]); c.HL(11, 10, 9, C.Clay[0]);
        c.Rect(13, 6, 2, 5, C.Mustard[2]);
        // neck + head (facing left)
        c.Poly(new[] { (5.0, 5.0), (9.0, 3.0), (11.0, 8.0), (7.0, 12.0) }, body[2]);
        c.Rect(0, 7, 8, 6, body[3]); c.Rect(0, 10, 4, 3, belly); c.Set(0, 12, body[0]);
        c.Set(3, 8, Eye); c.Set(2, 8, body[0]);
        // ears (flick on frame 1)
        c.Rect(6, 0 + frame, 2, 5 - frame, body[1]); c.Rect(9, 1, 2, 4, body[2]); c.Set(7, 1 + frame, belly);
        // mane
        c.VL(8, 3, 6, body[0]);
        // tail
        int tw = frame;
        c.Line(26, 9, 27 + tw, 14, body[0]); c.Line(27, 9, 28 + tw, 15, body[1]); c.Set(27 + tw, 15, body[0]);
        c.SelOutline(0.38);
        return c;
    }

    public static void Build()
    {
        S.Cur = "folk";
        Rgba[][] clothes = { C.Clay, C.Blue, C.Drab, C.Mustard, C.Teal, C.Plum, C.Rust, C.Cream, C.Ochre, C.Grey };
        var a = new Look(C.SkinA, C.HairDk, 2, C.Teal, C.Clay, true, C.Mustard);
        var b = new Look(C.SkinB, C.HairDk, 4, C.Drab, C.Rust, false, C.Grey);
        var c = new Look(C.SkinC, C.HairBr, 3, C.Plum, C.Grey, true);
        var d = new Look(C.SkinA, C.HairDk, 0, C.Blue, C.Rust, false);
        var e = new Look(C.SkinB, C.HairGr, 4, C.Rust, C.Grey, false, C.Grey, true);
        var f = new Look(C.SkinC, C.HairBr, 2, C.Mustard, C.Cream, true, C.Clay);
        var g = new Look(C.SkinB, C.HairDk, 1, C.Clay, C.Drab, true);
        var kg = new Look(C.SkinA, C.HairDk, 1, C.Mustard, C.Rust, true);
        var kb = new Look(C.SkinC, C.HairBr, 0, C.Teal, C.Grey, false);
        var kc = new Look(C.SkinB, C.HairDk, 0, C.Clay, C.Drab, false);
        var tod = new Look(C.SkinA, C.HairDk, 0, C.Mustard, C.Mustard, false);

        void Pair(string name, Func<int, Canvas> make) { for (int i = 0; i < 2; i++) S.Add($"npc_c5_{name}_{i}", make(i)); }
        Pair("woman_stand", i => Stand(a, i));
        Pair("man_stand", i => Stand(b, i));
        Pair("woman_carry", i => Stand(c, i, true));
        Pair("woman_sit_a", i => Sit(f, i));
        Pair("man_sit", i => Sit(d, i));
        Pair("elder_sit", i => Sit(e, i, 2));
        Pair("woman_sit_b", i => Sit(g, i));
        Pair("cook_sit", i => Sit(a, i, 1));
        Pair("mother_child", i => MotherChild(f, tod, i));
        Pair("kid_play_a", i => Kid(kg, i));
        Pair("kid_play_b", i => Kid(kb, i));
        Pair("kid_play_c", i => Kid(kc, i));
        Pair("kid_sit", i => KidSit(kc, i));
        Pair("donkey", Donkey);

        // Lucille's own children, derived from the existing standing sprites with a breathing idle
        foreach (var (key, src, rows) in new[] { ("kid_a", "child_a_stand_down", 11), ("kid_b", "child_b_stand_down", 11) })
        {
            var sp = S.Disk(src) ?? throw new Exception("missing " + src);
            S.Add($"npc_c5_{key}_0", sp);
            S.Add($"npc_c5_{key}_1", Breathe(sp, rows - 3));
        }
    }
}
