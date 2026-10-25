namespace WorldArtCh6;

public static class Npcs
{
    static Rgba H(string h) => Rgba.Hex(h);

    record Look(Rgba[] Skin, Rgba[] Hair, int Style, Rgba[] Top, Rgba[] Bottom, bool Skirt, int Carry, Rgba CarryCol, Rgba Shoe, bool Apron = false, Rgba[]? Scarf = null, bool Cane = false, bool Glasses = false);

    // skin ramps are (hi, mid, lo)
    static readonly Rgba[] SkinA = { H("E8BC93"), H("D19A6D"), H("B0795A") };
    static readonly Rgba[] SkinB = { H("C98E64"), H("A8704A"), H("7E4E36") };
    static readonly Rgba[] SkinC = { H("F0CBA6"), H("DDA980"), H("BC8566") };
    static readonly Rgba[] SkinD = { H("8E5C42"), H("6E4430"), H("52301F") };

    public static void Build()
    {
        // evening shopper with a basket, long skirt and a shawl
        Make("npc_c6_woman_basket", new Look(SkinA, new[] { H("4A3B32"), H("33292A"), H("73594A") }, 2, new[] { H("3E5A6E"), H("4F7189"), H("6B8FA8") },
            new[] { H("5E4E80"), H("7C6AA3"), H("9F8CC4") }, true, 1, H("C79E68"), K.Brown2, false, new[] { K.RedR[1], K.RedR[2], K.RedR[3] }));
        // man in a long coat with a shoulder bag, waiting for someone
        Make("npc_c6_man_coat", new Look(SkinB, new[] { H("2B2321"), H("1A1514"), H("4A3B32") }, 0, new[] { H("5E4A3C"), H("7A6048"), H("937659") },
            new[] { H("3A3A4A"), H("4E4E62"), H("686880") }, false, 2, K.Terra, K.Brown3));
        // teenager with a backpack
        Make("npc_c6_youth", new Look(SkinC, new[] { H("6A5444"), H("4A3B32"), H("8E6A44") }, 0, new[] { H("A04545"), H("C05A4A"), H("D9786A") },
            new[] { H("3E5A6E"), H("4F7189"), H("6B8FA8") }, false, 3, K.Teal, K.Brown2));
        // seamstress / shopkeeper in an apron, hair tied up, glasses
        Make("npc_c6_shopkeeper", new Look(SkinD, new[] { H("2B2321"), H("1A1514"), H("5E4A3C") }, 2, new[] { H("2B5654"), H("3A726F"), H("4B8F8A") },
            new[] { H("5E4A3C"), H("7A6048"), H("937659") }, true, 0, K.Cream, K.Brown2, true, null, false, true));
        // elderly neighbour with a cane and a headscarf
        Make("npc_c6_neighbour", new Look(SkinC, new[] { H("B8B0A8"), H("8F8880"), H("D6D0C8") }, 4, new[] { H("7A3535"), H("A04545"), H("C05A4A") },
            new[] { H("4A3B32"), H("6A5444"), H("8A7058") }, true, 0, K.Cream, K.Brown2, false, new[] { K.Linen[0], K.Linen[1], K.Linen[2] }, true));
    }

    static void Make(string name, Look l)
    {
        for (int f = 0; f < 2; f++) S.Add($"{name}_{f}", Person(l, f));
    }

    static Canvas Person(Look l, int frame)
    {
        var c = new Canvas(16, 24);
        int bob = frame == 1 ? -1 : 0;           // breathing: the upper body rises a pixel
        var (skHi, skMid, skLo) = (l.Skin[0], l.Skin[1], l.Skin[2]);
        var (tLo, tMid, tHi) = (l.Top[0], l.Top[1], l.Top[2]);
        var (bLo, bMid, bHi) = (l.Bottom[0], l.Bottom[1], l.Bottom[2]);

        // legs / skirt
        if (l.Skirt)
        {
            for (int j = 0; j < 7; j++)
            {
                int half = 4 + j / 2;
                for (int i = -half; i < half; i++)
                {
                    int x = 8 + i; int y = 14 + j;
                    var col = i < -half / 2 ? bHi : i >= half / 2 ? bLo : bMid;
                    if ((i + j) % 5 == 0 && j > 1) col = i < 0 ? bMid : bLo;
                    c.Set(x, y, col);
                }
            }
            for (int j = 21; j < 22; j++) { c.Set(6, j, skMid); c.Set(7, j, skLo); c.Set(9, j, skMid); c.Set(10, j, skLo); }
        }
        else
        {
            for (int j = 15; j < 22; j++)
            {
                foreach (var lx in new[] { 5, 9 })
                {
                    c.Set(lx, j, bHi); c.Set(lx + 1, j, bMid); c.Set(lx + 2, j, bLo);
                }
            }
        }
        // shoes
        for (int i = 0; i < 4; i++) { c.Set(4 + i, 22, l.Shoe); c.Set(9 + i, 22, l.Shoe); }
        for (int i = 0; i < 4; i++) { c.Set(4 + i, 21 + 0, l.Skirt ? c.Get(4 + i, 21) : c.Get(4 + i, 21)); }
        c.Set(4, 22, Kit.Dk(l.Shoe, 1.4)); c.Set(9, 22, Kit.Dk(l.Shoe, 1.4));
        for (int i = 0; i < 4; i++) { c.Set(4 + i, 23, Kit.Dk(l.Shoe, 0.8)); c.Set(9 + i, 23, Kit.Dk(l.Shoe, 0.8)); }

        // torso (shifted by bob)
        int ty = 9 + bob;
        for (int j = 0; j < 8; j++)
            for (int i = 0; i < 8; i++)
            {
                int x = 4 + i, y = ty + j;
                var col = i < 2 ? tHi : i > 5 ? tLo : tMid;
                if (j == 0) col = tHi;
                c.Set(x, y, col);
            }
        if (l.Apron) for (int j = 2; j < 12; j++) for (int i = 5; i < 11; i++) c.Set(i, ty + j, i == 5 ? K.Linen[3] : i == 10 ? K.Linen[1] : K.Linen[2]);
        if (l.Scarf != null) for (int i = 4; i < 12; i++) { c.Set(i, ty, l.Scarf[2]); c.Set(i, ty + 1, i < 8 ? l.Scarf[1] : l.Scarf[0]); }
        // arms
        for (int j = 1; j < 7; j++) { c.Set(3, ty + j, tHi); c.Set(12, ty + j, tLo); }
        c.Set(3, ty + 7, skHi); c.Set(12, ty + 7, skMid);
        // carried things
        switch (l.Carry)
        {
            case 1: // basket on the right arm
                for (int j = 0; j < 4; j++) for (int i = 0; i < 4; i++) c.Set(12 + i - 0, ty + 7 + j, (i + j) % 2 == 0 ? l.CarryCol : K.Pine[1]);
                c.Set(12, ty + 6, K.Pine[3]); c.Set(15, ty + 6, K.Pine[3]); c.Set(13, ty + 5, K.Pine[3]); c.Set(14, ty + 5, K.Pine[3]);
                c.Set(13, ty + 6, K.Green); c.Set(14, ty + 6, K.Gold);
                break;
            case 2: // shoulder bag with a strap
                for (int k = 0; k < 7; k++) c.Set(11 - k / 1 * 1 + 0, ty + k - 0, K.Brown2);
                for (int j = 0; j < 5; j++) for (int i = 0; i < 5; i++) c.Set(0 + i, ty + 6 + j, i == 0 ? K.RedR[3] : i == 4 ? K.RedR[0] : l.CarryCol);
                break;
            case 3: // backpack straps and sides
                for (int j = 0; j < 7; j++) { c.Set(5, ty + j, l.CarryCol); c.Set(10, ty + j, l.CarryCol); }
                for (int j = 1; j < 8; j++) { c.Set(2, ty + j, l.CarryCol); c.Set(13, ty + j, Kit.Dk(l.CarryCol, 0.7)); }
                break;
        }
        // head
        int hy = 1 + bob;
        Kit.Ellipse(c, 8, hy + 4.2, 4.2, 4.6, skMid, skHi, skLo);
        // neck
        c.Set(7, ty - 1, skLo); c.Set(8, ty - 1, skLo);
        // face
        c.Set(6, hy + 4, K.Brown3); c.Set(9, hy + 4, K.Brown3);
        c.Set(7, hy + 6, skLo); c.Set(8, hy + 6, skLo);
        c.Set(5, hy + 5, Rgba.Mul(skHi, 1.0, 0.85, 0.85)); c.Set(10, hy + 5, Rgba.Mul(skMid, 1.0, 0.85, 0.85));
        if (l.Glasses) { for (int i = 5; i < 11; i++) c.Set(i, hy + 4, K.Iron[1]); c.Set(7, hy + 4, K.Iron[1]); c.Set(8, hy + 4, K.Iron[1]); c.Set(6, hy + 4, K.Brown3); c.Set(9, hy + 4, K.Brown3); }
        // hair
        var (hD, hM, hH) = (l.Hair[1], l.Hair[0], l.Hair[2]);
        switch (l.Style)
        {
            case 0: // short
                for (int i = 4; i < 12; i++) { c.Set(i, hy, hM); c.Set(i, hy + 1, hM); }
                for (int i = 5; i < 11; i++) c.Set(i, hy - 0, hD);
                c.Set(4, hy + 2, hM); c.Set(11, hy + 2, hD); c.Set(4, hy + 3, hM); c.Set(11, hy + 3, hD);
                for (int i = 6; i < 10; i++) c.Set(i, hy + 2, i < 8 ? hM : hD);
                c.Set(6, hy + 1, hH); c.Set(7, hy + 1, hH);
                break;
            case 1: // long
                for (int i = 4; i < 12; i++) { c.Set(i, hy, hM); c.Set(i, hy + 1, hM); }
                for (int j = 2; j < 11; j++) { c.Set(3, hy + j, hM); c.Set(12, hy + j, hD); c.Set(4, hy + j, hM); c.Set(11, hy + j, hD); }
                break;
            case 2: // bun
                for (int i = 4; i < 12; i++) { c.Set(i, hy, hM); c.Set(i, hy + 1, hM); }
                c.Set(4, hy + 2, hM); c.Set(11, hy + 2, hD);
                c.Set(7, hy - 1, hM); c.Set(8, hy - 1, hD); c.Set(7, hy - 2, hH); c.Set(8, hy - 2, hM);
                c.Set(6, hy + 1, hH); c.Set(7, hy + 1, hH);
                break;
            case 4: // headscarf
                for (int i = 4; i < 12; i++) { c.Set(i, hy, l.Scarf![2]); c.Set(i, hy + 1, l.Scarf[1]); }
                for (int j = 2; j < 6; j++) { c.Set(3, hy + j, l.Scarf![1]); c.Set(4, hy + j, l.Scarf[2]); c.Set(12, hy + j, l.Scarf[0]); c.Set(11, hy + j, l.Scarf[1]); }
                c.Set(5, hy + 1, hH); c.Set(10, hy + 1, hH);
                c.Set(7, hy + 2, l.Hair[2]); c.Set(8, hy + 2, l.Hair[2]);
                break;
        }
        if (l.Cane) { for (int j = 11; j < 23; j++) c.Set(14, j, K.Wood[2]); c.Set(13, 11, K.Wood[4]); c.Set(14, 10, K.Wood[4]); c.Set(13, ty + 7, skHi); }
        var o = Kit.Outlined(c);
        return o;
    }
}

public static class Fx6
{
    static Rgba H(string h) => Rgba.Hex(h);

    public static void Build()
    {
        Steam();
        Moth();
        Pigeon();
        Halo();
        Bird();
    }

    /// <summary>Small dusk bird gliding over the street (two wing positions).</summary>
    static void Bird()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = new Canvas(12, 8);
            var d = H("3A3A52");
            c.Set(5, 4, d); c.Set(6, 4, d); c.Set(7, 4, d);
            if (f == 0) { c.Set(4, 3, d); c.Set(3, 2, d); c.Set(2, 2, d); c.Set(1, 3, d); c.Set(8, 3, d); c.Set(9, 2, d); c.Set(10, 2, d); c.Set(11, 3, d); }
            else { c.Set(4, 5, d); c.Set(3, 6, d); c.Set(2, 6, d); c.Set(1, 5, d); c.Set(8, 5, d); c.Set(9, 6, d); c.Set(10, 6, d); c.Set(11, 5, d); }
            S.Add($"fx_c6_bird_{f}", c);
        }
    }

    /// <summary>Dithered lamp bloom: the hole in the middle lets the lantern show through.</summary>
    static void Halo()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = new Canvas(32, 32);
            double lim = f == 0 ? 1.0 : 0.9;
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    double d = Math.Sqrt((x - 15.5) * (x - 15.5) + (y - 15.5) * (y - 15.5)) / 16.0;
                    if (d < 0.44 || d > lim) continue;
                    if (d < 0.60 && ((x + y) & 1) == 0 && x % 2 == 0) c.Set(x, y, H("FFE9B0"));
                    else if (d < 0.80 && x % 2 == 0 && y % 2 == 0) c.Set(x, y, H("F8D690"));
                    else if (d < lim && x % 4 == 0 && y % 4 == 0) c.Set(x, y, H("E9B96E"));
                }
            S.Add($"fx_c6_halo_{f}", c);
        }
    }

    static void Steam()
    {
        var cols = new[] { H("F4EAD2"), H("E8D9B5"), H("DCCBA4") };
        for (int f = 0; f < 4; f++)
        {
            var c = new Canvas(14, 24);
            for (int j = 0; j < 22; j++)
            {
                double sway = Math.Sin((j + f * 5) / 3.6) * (1.2 + j * 0.08);
                int cx = 7 + (int)Math.Round(sway);
                int w = 1 + j / 8;
                double fade = 1 - j / 24.0;
                for (int i = -w; i <= w; i++)
                {
                    double r = Fx.R(i + f * 3, j, 401 + f);
                    if (r > fade * 0.95 + 0.1) continue;
                    c.Set(cx + i, 23 - j, cols[j < 7 ? 0 : j < 14 ? 1 : 2]);
                }
            }
            S.Add($"fx_c6_steam_{f}", c);
        }
    }

    static void Moth()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = new Canvas(8, 6);
            c.Set(3, 3, K.Brown2); c.Set(4, 3, K.Brown2); c.Set(3, 4, K.Brown2);
            if (f == 0) { c.Set(1, 2, H("FFF6D6")); c.Set(2, 2, H("FFF6D6")); c.Set(5, 2, H("FFF6D6")); c.Set(6, 2, H("FFF6D6")); c.Set(2, 3, H("E8D9B5")); c.Set(5, 3, H("E8D9B5")); }
            else { c.Set(1, 4, H("FFF6D6")); c.Set(2, 3, H("FFF6D6")); c.Set(5, 3, H("FFF6D6")); c.Set(6, 4, H("FFF6D6")); c.Set(2, 4, H("E8D9B5")); c.Set(5, 4, H("E8D9B5")); }
            S.Add($"fx_c6_moth_{f}", c);
        }
    }

    static void Pigeon()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = new Canvas(14, 12);
            var g = new[] { H("5A6283"), H("7D86A4"), H("9AA3BE"), H("C2C8DA") };
            Kit.Ellipse(c, 6.5, 7, 5, 3.4, g[1], g[2], g[0]);
            // neck / head (peck on frame 1)
            int hx = 11, hy = f == 0 ? 3 : 6;
            Kit.Ellipse(c, hx, hy + 1, 2, 2, g[2], g[3], g[1]);
            c.Set(hx + 1, hy, K.Brown3); c.Set(hx + 2, hy + 1, K.Orange);
            for (int j = 0; j < 3; j++) c.Set(hx - 1 + (f == 0 ? 0 : 1), hy + 2 + j + (f == 0 ? 0 : 0), g[1]);
            // tail + legs
            c.Set(0, 6, g[0]); c.Set(1, 6, g[0]); c.Set(0, 7, g[0]); c.Set(1, 8, g[0]);
            c.Set(5, 10, K.Orange); c.Set(5, 11, K.Orange); c.Set(8, 10, K.Orange); c.Set(8, 11, K.Orange);
            c.Set(5, 6, g[3]); c.Set(6, 6, g[3]); c.Set(4, 7, g[0]); c.Set(3, 7, g[0]);
            S.Add($"fx_c6_pigeon_{f}", Kit.Outlined(c));
        }
    }
}
