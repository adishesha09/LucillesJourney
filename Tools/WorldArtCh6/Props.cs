namespace WorldArtCh6;

public static class Decor
{
    static Rgba H(string h) => Rgba.Hex(h);

    public static void Build()
    {
        Bicycle();
        Bin();
        ShoeRack();
        RoofTank();
        Antenna();
        Skylight();
        Vent();
        Runner();
        Handcart();
    }

    /// <summary>Long worn runner rug that leads from the front door into the lobby.</summary>
    static void Runner()
    {
        int w = 22, h = 76;
        var c = new Canvas(w, h);
        var field = new[] { H("3A5A6A"), H("4B7384"), H("5E8A9A") };
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                if (j < 3 || j >= h - 3) { c.Set(i, j, (i % 2 == 0) ? K.Cream : K.Linen[1]); continue; }     // fringe
                bool border = i < 3 || i >= w - 3 || j < 6 || j >= h - 6;
                var col = field[1];
                if (i == 0 || i == w - 1) col = field[0];
                else if (border) col = ((i + j) % 6 < 3) ? K.Terra : K.RedR[1];
                else
                {
                    int cy = (j - 6) % 14, dx = Math.Abs(i - 11), dy = Math.Abs(cy - 7);
                    if (dx + dy <= 4 && dx + dy >= 3) col = K.Cream;
                    else if (dx + dy < 3) col = K.Gold;
                    else if (Fx.R(i, j, 361) > 0.9) col = field[2];
                }
                c.Set(i, j, col);
            }
        // threadbare patches
        for (int k = 0; k < 14; k++) { int x = 4 + (int)(Fx.R(k, 0, 362) * 14), y = 8 + (int)(Fx.R(k, 1, 362) * 60); c.Set(x, y, K.Sand); }
        S.Add("prop_c6_runner", c);
    }

    static void Handcart()
    {
        var c = new Canvas(40, 30);
        // cart bed
        for (int j = 0; j < 12; j++)
            for (int i = 4; i < 34; i++)
            {
                int idx = j == 0 ? 4 : j < 7 ? 3 : j == 11 ? 0 : 2;
                if (i == 4) idx = Math.Min(5, idx + 1); if (i == 33) idx = Math.Max(0, idx - 2);
                if (j >= 7 && j < 11 && (i - 4) % 8 == 0) idx = 1;
                c.Set(i, 8 + j, K.Wood[idx]);
            }
        // goods: a sack of grain and crates of vegetables, tarp-covered
        for (int j = 0; j < 7; j++) for (int i = 0; i < 12; i++) { if ((i == 0 || i == 11) && (j == 0)) continue; c.Set(7 + i, 2 + j, j == 0 ? K.Linen[4] : i < 3 ? K.Linen[3] : i > 8 ? K.Linen[1] : K.Linen[2]); }
        c.Set(12, 2, K.Brown); c.Set(13, 2, K.Brown);
        for (int j = 0; j < 7; j++) for (int i = 0; i < 12; i++) c.Set(21 + i, 2 + j, j < 3 ? ((i + j) % 4 == 0 ? K.Green : K.DGreen) : (i % 4 == 0 ? K.Wood[1] : K.Wood[3]));
        c.Set(24, 3, K.Orange); c.Set(28, 2, K.Terra); c.Set(30, 4, K.Terra);
        // handles
        for (int i = 0; i < 8; i++) { c.Set(i - 4 + 4, 18 - i / 3, K.Wood[2]); }
        c.Line(5, 17, 0, 24, K.Wood[3]); c.Line(5, 18, 0, 25, K.Wood[1]);
        // wheel
        for (int a = 0; a < 360; a += 5)
        {
            int x = 24 + (int)Math.Round(Math.Cos(a * Math.PI / 180) * 7), y = 22 + (int)Math.Round(Math.Sin(a * Math.PI / 180) * 7);
            c.Set(x, y, K.Wood[0]); c.Set(24 + (int)Math.Round(Math.Cos(a * Math.PI / 180) * 6), 22 + (int)Math.Round(Math.Sin(a * Math.PI / 180) * 6), K.Wood[2]);
        }
        for (int a = 0; a < 180; a += 30) for (int r = -5; r <= 5; r++) c.Set(24 + (int)Math.Round(Math.Cos(a * Math.PI / 180) * r), 22 + (int)Math.Round(Math.Sin(a * Math.PI / 180) * r), K.Wood[4]);
        c.Set(24, 22, K.Iron[3]);
        // prop stick at the front
        for (int j = 20; j < 29; j++) c.Set(8, j, K.Wood[1]);
        S.Add("prop_c6_handcart", Kit.Outlined(c).Trim());
    }

    static void Bicycle()
    {
        var c = new Canvas(34, 22);
        void Wheel(int cx, int cy)
        {
            for (int a = 0; a < 360; a += 6)
            {
                int x = cx + (int)Math.Round(Math.Cos(a * Math.PI / 180) * 7.5), y = cy + (int)Math.Round(Math.Sin(a * Math.PI / 180) * 7.5);
                c.Set(x, y, K.Iron[1]);
            }
            for (int a = 0; a < 180; a += 45)
                for (int r = -5; r <= 5; r += 2)
                    c.Set(cx + (int)Math.Round(Math.Cos(a * Math.PI / 180) * r), cy + (int)Math.Round(Math.Sin(a * Math.PI / 180) * r), K.Iron[3]);
            c.Set(cx, cy, K.Gold);
        }
        Wheel(8, 13); Wheel(26, 13);
        var fr = K.RedR[2]; var frLo = K.RedR[0];
        c.Line(8, 13, 15, 6, fr); c.Line(15, 6, 24, 6, fr); c.Line(24, 6, 26, 13, fr); c.Line(15, 6, 17, 13, frLo); c.Line(17, 13, 24, 6, fr); c.Line(8, 13, 17, 13, frLo);
        c.Line(24, 6, 23, 3, K.Iron[2]); c.Line(21, 3, 25, 3, K.Iron[2]);          // handlebar
        c.Line(15, 6, 14, 3, K.Iron[2]); c.Line(12, 2, 16, 2, K.Brown2);            // saddle
        // front basket
        for (int j = 0; j < 5; j++) for (int i = 0; i < 6; i++) c.Set(27 + i, 3 + j, (i + j) % 2 == 0 ? K.Pine[3] : K.Pine[1]);
        c.Set(29, 2, K.Green); c.Set(30, 2, K.DGreen);
        S.Add("prop_c6_bicycle", Kit.Outlined(c).Trim());
    }

    static void Bin()
    {
        var c = new Canvas(14, 18);
        for (int j = 3; j < 18; j++) for (int i = 1; i < 13; i++) c.Set(i, j, i < 3 ? K.Iron[3] : i > 9 ? K.Iron[0] : (j % 5 == 0 ? K.Iron[1] : K.Iron[2]));
        for (int i = 0; i < 14; i++) { c.Set(i, 2, K.Iron[4]); c.Set(i, 3, K.Iron[3]); c.Set(i, 4, K.Iron[1]); }
        for (int i = 3; i < 11; i++) c.Set(i, 1, K.Iron[3]);
        c.Set(6, 0, K.Iron[4]); c.Set(7, 0, K.Iron[4]);
        c.Set(10, 7, K.Cream); c.Set(11, 7, K.Cream);
        S.Add("prop_c6_bin", Kit.Outlined(c));
    }

    static void ShoeRack()
    {
        var c = new Canvas(24, 12);
        for (int i = 0; i < 24; i++) { c.Set(i, 5, K.Wood[4]); c.Set(i, 6, K.Wood[2]); c.Set(i, 11, K.Wood[0]); c.Set(i, 10, K.Wood[2]); }
        for (int j = 6; j < 12; j++) { c.Set(1, j, K.Wood[3]); c.Set(2, j, K.Wood[1]); c.Set(21, j, K.Wood[3]); c.Set(22, j, K.Wood[1]); }
        // shoes on top, boots on the lower slat
        void Shoe(int x, int y, Rgba col, Rgba hi) { for (int i = 0; i < 6; i++) { c.Set(x + i, y, hi); c.Set(x + i, y + 1, col); } c.Set(x, y - 1, col); c.Set(x + 1, y - 1, col); }
        Shoe(3, 3, K.Brown, K.Tan); Shoe(10, 3, K.Terra, K.RedR[3]); Shoe(15, 3, K.Teal, K.TealR[3]);
        Shoe(4, 8, K.Brown2, K.Brown); Shoe(12, 8, K.Brown2, K.Brown);
        S.Add("prop_c6_shoerack", Kit.Outlined(c));
    }

    static void RoofTank()
    {
        var c = new Canvas(26, 38);
        // legs
        foreach (var lx in new[] { 5, 10, 15, 20 }) for (int j = 26; j < 38; j++) c.Set(lx, j, lx < 12 ? K.Wood[2] : K.Wood[0]);
        for (int i = 4; i < 22; i++) { c.Set(i, 31, K.Wood[1]); }
        // barrel
        for (int j = 8; j < 28; j++)
            for (int i = 2; i < 24; i++)
            {
                double t = 1 - (i - 2) / 22.0;
                int idx = t > 0.75 ? 4 : t > 0.5 ? 3 : t > 0.25 ? 2 : 1;
                if (j == 8 || j == 27) idx = 1;
                if (j == 12 || j == 22) idx = 0;
                c.Set(i, j, K.Wood[Math.Clamp(idx, 0, 5)]);
            }
        // cone lid
        for (int j = 0; j < 8; j++) for (int i = 2 + (7 - j) / 1; i < 24 - (7 - j); i++) c.Set(i, j, i < 11 ? K.Iron[3] : K.Iron[1]);
        c.Set(12, 0, K.Iron[4]); c.Set(13, 0, K.Iron[4]);
        // iron hoops
        for (int j = 13; j < 22; j += 8) for (int i = 2; i < 24; i++) c.Set(i, j, K.Iron[2]);
        S.Add("prop_c6_tank", Kit.Outlined(c));
    }

    static void Antenna()
    {
        var c = new Canvas(16, 34);
        for (int j = 0; j < 34; j++) c.Set(8, j, K.Iron[3]);
        foreach (var (y, w) in new[] { (3, 6), (8, 7), (13, 5), (18, 4) }) for (int i = -w; i <= w; i++) c.Set(8 + i, y, K.Iron[(i + w) % 2 == 0 ? 4 : 2]);
        for (int j = 28; j < 34; j++) for (int i = 5; i < 12; i++) c.Set(i, j, K.Iron[1]);
        c.Line(8, 12, 3, 33, K.Iron[1]); c.Line(8, 12, 13, 33, K.Iron[1]);
        S.Add("prop_c6_antenna", Kit.Outlined(c));
    }

    static void Skylight()
    {
        var c = new Canvas(26, 16);
        for (int j = 0; j < 16; j++)
            for (int i = 0; i < 26; i++)
            {
                bool frame = i < 2 || i >= 24 || j < 2 || j >= 14;
                if (frame) { c.Set(i, j, j >= 14 ? K.Iron[0] : i < 2 || j < 2 ? K.Iron[3] : K.Iron[1]); continue; }
                double t = (j - 2) / 12.0;
                var col = t < 0.5 ? H("F8D790") : H("EFBE72");
                if (i % 8 == 5) col = K.Iron[2];
                c.Set(i, j, col);
            }
        S.Add("prop_c6_skylight", Kit.Outlined(c));
    }

    static void Vent()
    {
        var c = new Canvas(14, 14);
        for (int j = 0; j < 14; j++) for (int i = 2; i < 12; i++) c.Set(i, j, i < 5 ? K.Iron[3] : i > 9 ? K.Iron[0] : K.Iron[2]);
        for (int i = 0; i < 14; i++) { c.Set(i, 0, K.Iron[4]); c.Set(i, 1, K.Iron[3]); c.Set(i, 2, K.Iron[1]); }
        for (int j = 5; j < 13; j += 3) for (int i = 4; i < 10; i++) c.Set(i, j, K.Iron[0]);
        S.Add("prop_c6_vent", Kit.Outlined(c));
    }
}

public static class StreetProps
{
    static Rgba H(string h) => Rgba.Hex(h);

    public static void Build()
    {
        for (int f = 0; f < 2; f++) LampPost(f);
        NoticeEasel(false);
        NoticeEasel(true);
        ClothRail();
        for (int f = 0; f < 2; f++) AlleyLine(f);
        SignHanging();
    }

    static void LampPost(int f)
    {
        var c = new Canvas(18, 56);
        var I = K.Iron;
        // plinth + pole
        for (int j = 50; j < 56; j++) for (int i = 4; i < 14; i++) c.Set(i, j, j == 50 ? I[4] : i < 6 ? I[3] : i > 11 ? I[0] : I[2]);
        for (int j = 46; j < 50; j++) for (int i = 6; i < 12; i++) c.Set(i, j, i < 8 ? I[3] : i > 9 ? I[0] : I[2]);
        for (int j = 14; j < 46; j++) { c.Set(8, j, I[4]); c.Set(9, j, I[2]); c.Set(10, j, I[0]); }
        // collar
        for (int i = 6; i < 13; i++) { c.Set(i, 22, I[3]); c.Set(i, 23, I[1]); }
        // lantern: glass cage with warm light
        for (int j = 3; j < 14; j++)
            for (int i = 4; i < 15; i++)
            {
                bool frame = i == 4 || i == 14 || j == 3 || j == 13;
                var glow = j < 7 ? E.WarmHi : j < 11 ? E.Warm : E.WarmLow;
                if (f == 1 && j > 9) glow = E.Warm;
                c.Set(i, j, frame ? I[1] : glow);
            }
        c.Set(9, 3, I[4]);
        for (int i = 3; i < 16; i++) { c.Set(i, 2, I[3]); c.Set(i, 1, I[1]); }
        for (int i = 6; i < 13; i++) c.Set(i, 0, I[2]);
        c.Set(8, 0, I[4]);
        for (int i = 7; i < 12; i++) c.Set(i, 14, I[1]);
        // scroll bracket
        c.Set(5, 24, I[3]); c.Set(4, 25, I[3]); c.Set(4, 26, I[2]); c.Set(14, 24, I[3]); c.Set(15, 25, I[3]); c.Set(15, 26, I[2]);
        S.Add($"prop_c6_lamppost_{f}", Kit.Outlined(c));
    }

    static void NoticeEasel(bool taken)
    {
        var c = new Canvas(28, 38);
        // splayed legs
        for (int j = 24; j < 38; j++) { int d = (j - 24) / 5; c.Set(5 - d, j, K.Wood[3]); c.Set(6 - d, j, K.Wood[1]); c.Set(21 + d, j, K.Wood[1]); c.Set(22 + d, j, K.Wood[3]); }
        for (int j = 24; j < 36; j++) { c.Set(13, j, K.Wood[2]); c.Set(14, j, K.Wood[0]); }
        // board frame + cork
        for (int j = 0; j < 26; j++)
            for (int i = 2; i < 26; i++)
            {
                bool edge = i == 2 || i == 25 || j == 0 || j == 25;
                int idx = edge ? (i == 2 || j == 0 ? 4 : 1) : 3;
                if (!edge)
                {
                    double n = Fx.VN(i / 2.0, j / 2.0, 331);
                    var cork = n > 0.7 ? K.Tan : n < 0.25 ? H("D8C69A") : K.Sand;
                    c.Set(i, j, cork);
                }
                else c.Set(i, j, K.Wood[idx]);
            }
        if (!taken)
        {
            // the big job notice: pale sheet with a red header band and rows of squiggles, plus two small cards
            for (int j = 0; j < 18; j++) for (int i = 0; i < 13; i++) c.Set(8 + i, 4 + j, j < 4 ? K.Terra : (i == 0 || j == 17 ? H("DCCBA4") : H("FBF3DC")));
            for (int j = 2; j < 4; j++) for (int i = 2; i < 11; i += 2) c.Set(8 + i, 4 + j - 2, K.RedR[3]);
            for (int j = 6; j < 17; j += 2) for (int i = 2; i < 11; i++) if (Fx.R(i, j, 337) > 0.25) c.Set(8 + i, 4 + j, K.Brown);
            c.Set(14, 4, K.Gold); c.Set(14, 3, K.Gold); c.Set(13, 3, K.Orange); c.Set(15, 3, K.Orange);         // pin
            for (int j = 0; j < 7; j++) for (int i = 0; i < 4; i++) c.Set(4 + i, 5 + j, i == 0 ? H("F6E7A8") : H("EAD688"));
            for (int j = 0; j < 6; j++) for (int i = 0; i < 4; i++) c.Set(22 + i - 1, 14 + j, i == 0 ? H("CFE3EA") : H("B8D2DC"));
        }
        else
        {
            // notice gone: only pins and a torn corner remain, the small cards are still there
            foreach (var (x, y) in new[] { (14, 4), (9, 21), (19, 21) }) { c.Set(x, y, K.Gold); }
            for (int j = 0; j < 4; j++) c.Set(8, 4 + j, H("FBF3DC"));
            for (int j = 0; j < 2; j++) c.Set(9, 4 + j, H("DCCBA4"));
            for (int j = 0; j < 7; j++) for (int i = 0; i < 4; i++) c.Set(4 + i, 5 + j, i == 0 ? H("F6E7A8") : H("EAD688"));
            for (int j = 0; j < 6; j++) for (int i = 0; i < 4; i++) c.Set(22 + i - 1, 14 + j, i == 0 ? H("CFE3EA") : H("B8D2DC"));
        }
        var o = Kit.Outlined(c);
        S.Add(taken ? "prop_c6_notice_taken" : "prop_c6_notice", o);
    }

    static void ClothRail()
    {
        var c = new Canvas(38, 34);
        var I = K.Iron;
        for (int j = 4; j < 34; j++) { c.Set(3, j, I[3]); c.Set(4, j, I[1]); c.Set(33, j, I[3]); c.Set(34, j, I[1]); }
        for (int i = 3; i < 35; i++) { c.Set(i, 4, I[4]); c.Set(i, 5, I[1]); }
        c.Set(2, 33, I[1]); c.Set(5, 33, I[1]); c.Set(32, 33, I[1]); c.Set(35, 33, I[1]);
        var cols = new[] { K.RedR, K.BlueR, K.Linen, K.Mustard, K.TealR, K.Purple };
        int x = 6;
        for (int g = 0; g < 6; g++)
        {
            var ramp = cols[g];
            int w = 5 + (g % 2), h = 14 + (g * 5) % 6;
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                {
                    int idx = i == 0 ? 3 : i == w - 1 ? 1 : 2;
                    if (j == 0) idx = 4;
                    if (j > h - 3) idx = Math.Max(0, idx - 1);
                    c.Set(x + i, 6 + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
                }
            c.Set(x + w / 2, 5, I[4]);
            x += w + 0;
        }
        S.Add("prop_c6_clothrail", Kit.Outlined(c));
    }

    static void AlleyLine(int f)
    {
        var c = new Canvas(44, 16);
        for (int i = 0; i < 44; i++) { int sag = (int)Math.Round(Math.Sin(i / 43.0 * Math.PI) * 2); c.Set(i, 2 + sag, K.Brown); }
        void Garment(int x, int w, int h, Rgba[] ramp, int sway)
        {
            for (int j = 0; j < h; j++) for (int i = 0; i < w; i++)
                {
                    int idx = i == 0 ? 3 : i == w - 1 ? 1 : 2; if (j == 0) idx = 4;
                    c.Set(x + i + (j > h / 2 ? sway : 0), 4 + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
                }
        }
        Garment(8, 7, 9, K.Linen, f);
        Garment(19, 5, 11, K.BlueR, -f);
        Garment(28, 6, 7, K.RedR, f);
        Garment(36, 4, 8, K.Mustard, 0);
        S.Add($"prop_c6_alley_line_{f}", Kit.Outlined(c));
    }

    static void SignHanging()
    {
        var c = new Canvas(14, 22);
        for (int i = 0; i < 14; i++) c.Set(i, 0, K.Iron[2]);
        c.Set(2, 1, K.Iron[3]); c.Set(11, 1, K.Iron[3]);
        for (int j = 3; j < 20; j++) for (int i = 1; i < 13; i++) c.Set(i, j, (i == 1 || j == 3) ? K.Wood[4] : (i == 12 || j == 19) ? K.Wood[1] : K.TealR[2]);
        // a spool icon
        for (int j = 6; j < 14; j++) for (int i = 4; i < 10; i++) c.Set(i, j, j == 6 || j == 13 ? K.Cream : (i == 4 || i == 9) ? K.Linen[1] : K.Terra);
        for (int i = 3; i < 11; i++) { c.Set(i, 5, K.Cream); c.Set(i, 14, K.Cream); }
        S.Add("prop_c6_sign_spool", Kit.Outlined(c).Trim());
    }
}
