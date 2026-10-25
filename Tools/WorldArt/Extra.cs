namespace WorldArt;

/// <summary>Extra interior and garden dressing that fills the remaining empty zones.</summary>
public static class Extra
{
    static Rgba Hx(string h) => Rgba.Hex(h);

    public static void Build()
    {
        S.Cur = "house_kitchen";
        S.Add("prop_coatrack", CoatRack());
        S.Add("prop_sacks", Sacks());
        S.Add("prop_kitchen_island", Island());
        S.Cur = "house_living";
        S.Add("prop_desk", Desk());
        S.Cur = "garden";
        S.Add("prop_pumpkins", Pumpkins());
        S.Add("prop_sunflowers", Sunflowers());
    }

    static Canvas CoatRack()
    {
        var c = new Canvas(20, 40);
        var w = K.Wood;
        // pole, knob and feet
        for (int y = 5; y < 37; y++) { c.Set(9, y, w[4]); c.Set(10, y, w[1]); }
        c.Rect(8, 2, 4, 3, w[3]); c.Set(8, 2, w[5]); c.Set(9, 2, w[5]); c.Set(11, 4, w[0]);
        c.Rect(3, 36, 14, 2, w[2]); c.HL(3, 36, 14, w[4]); c.HL(3, 37, 14, w[0]); c.Rect(6, 34, 8, 2, w[3]);
        // hooks
        c.Line(9, 10, 4, 7, w[3]); c.Line(10, 10, 15, 7, w[2]); c.Line(9, 16, 5, 14, w[3]); c.Line(10, 16, 14, 14, w[2]);
        c.Set(4, 6, w[4]); c.Set(15, 6, w[2]);
        // straw hat on the left hook
        c.Ellipse(4.5, 9, 4.6, 1.6, K.Mustard[3]); c.Ellipse(4.5, 8.7, 3.6, 1.1, K.Mustard[4]);
        c.Rect(2, 4, 5, 4, K.Mustard[2]); c.HL(2, 4, 5, K.Mustard[3]); c.HL(2, 7, 5, K.Terra); c.VL(6, 4, 4, K.Mustard[1]);
        // coat hanging on the right hook
        for (int y = 9; y < 30; y++)
        {
            int half = y < 12 ? 3 : 4;
            for (int x = 15 - half; x < 15 + half; x++)
            {
                int idx = x < 13 ? 3 : x < 15 ? 2 : 1;
                if (y > 27) idx = Math.Max(0, idx - 1);
                if (y % 7 == 6) idx = Math.Max(0, idx - 1);
                c.Set(x, y, K.TealR[idx]);
            }
        }
        c.Rect(13, 9, 5, 2, K.Terra); c.HL(13, 9, 5, K.RedR[4]);              // collar scarf
        c.Rect(12, 20, 3, 3, K.TealR[1]); c.HL(12, 20, 3, K.TealR[3]);          // pocket
        c.VL(15, 12, 16, K.TealR[0]);
        c.Set(14, 16, K.Mustard[3]); c.Set(14, 21, K.Mustard[3]);
        // a scarf over the lower hook
        c.Rect(4, 14, 3, 9, K.Terra); c.VL(4, 14, 9, K.RedR[3]); c.VL(6, 14, 9, K.RedR[1]); c.HL(4, 22, 3, K.Cream); c.HL(4, 20, 3, K.Cream);
        // little boots by the foot
        foreach (var bx in new[] { 0, 15 })
        {
            c.Rect(bx, 34, 4, 5, K.Brown); c.Rect(bx + (bx == 0 ? 2 : -2), 37, 4, 2, K.Brown2);
            c.HL(bx, 34, 4, K.Tan); c.HL(bx + (bx == 0 ? 2 : -2), 38, 4, K.Brown3);
        }
        return U.Finish(c, 0.4);
    }

    static void Sack(Canvas c, int x, int y, int w, int h, Rgba[] ramp, Rgba tie)
    {
        for (int j = 0; j < h; j++)
        {
            double t = j / (double)h;
            double half = w / 2.0 * (t < 0.22 ? 0.42 : 0.68 + 0.32 * Math.Sin(Math.Min(1, (t - 0.22) / 0.78) * Math.PI / 2));
            if (t > 0.9) half -= 1;
            for (int i = 0; i < w; i++)
            {
                double dx = i + 0.5 - w / 2.0;
                if (Math.Abs(dx) > half) continue;
                double u = (dx + half) / (2 * half);
                int idx = u < 0.25 ? 3 : u < 0.6 ? 2 : u < 0.85 ? 1 : 0;
                if (j > 0 && (i + j) % 7 == 3 && Fx.R(i, j, 55) > 0.5) idx = Math.Max(0, idx - 1);
                if (j == h - 1) idx = 0;
                c.Set(x + i, y + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
            }
        }
        // tied neck and the gathered tuft
        int nx = x + w / 2;
        c.Rect(nx - 2, y + 3, 4, 1, tie); c.Set(nx - 1, y + 2, ramp[3]); c.Set(nx, y + 2, ramp[ramp.Length - 1]); c.Set(nx - 2, y + 1, ramp[2]); c.Set(nx + 1, y + 1, ramp[1]);
        c.Set(nx - 1, y + 4, tie);
        for (int i = 2; i < w - 2; i += 3) c.Set(x + i, y + h / 2, ramp[1]);
    }

    static Canvas Sacks()
    {
        var c = new Canvas(34, 28);
        Sack(c, 1, 13, 14, 14, K.Linen, K.Terra);
        Sack(c, 11, 15, 14, 12, K.Linen, K.Teal);
        Sack(c, 5, 2, 13, 14, new[] { Hx("B8A07A"), Hx("D2BB92"), Hx("E4D0A8"), Hx("F0E2BE"), K.Cream }, K.Brown);
        // barrel of apples
        var w = K.Wood;
        for (int y = 12; y < 27; y++)
            for (int x = 21; x < 33; x++)
            {
                double u = (x - 21) / 12.0;
                int idx = u < 0.2 ? 4 : u < 0.45 ? 3 : u < 0.75 ? 2 : u < 0.9 ? 1 : 0;
                if ((x - 21) % 4 == 3) idx = Math.Max(0, idx - 1);
                c.Set(x, y, w[idx]);
            }
        c.HL(21, 15, 12, K.Iron[3]); c.HL(21, 24, 12, K.Iron[2]);
        c.Ellipse(27, 12, 6.2, 3, w[4]);
        c.Ellipse(27, 12.2, 5.2, 2.2, K.Dark[1]);
        U.Apple(c, 23, 9, K.RedR[2]); U.Apple(c, 27, 8, K.Terra); U.Apple(c, 29, 10, K.RedR[3]); U.Apple(c, 25, 11, K.Gold);
        c.HL(21, 26, 12, w[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas Island()
    {
        var c = new Canvas(46, 34);
        var p = K.Pine;
        // worktop
        c.Rect(0, 0, 46, 13, p[3]); c.HL(0, 0, 46, p[4]); c.VL(0, 0, 13, p[4]); c.VL(45, 0, 13, p[2]);
        c.HL(1, 4, 44, p[2]); c.HL(1, 8, 44, p[2]);
        for (int x = 4; x < 44; x += 11) { c.Set(x, 2, p[2]); c.Set(x + 3, 6, p[4]); c.Set(x + 1, 10, p[4]); }
        c.Rect(0, 13, 46, 3, p[2]); c.HL(0, 13, 46, p[3]); c.HL(0, 15, 46, p[0]);
        // dough, rolling pin
        c.Ellipse(8, 5, 4.2, 2.8, K.Cream); c.Ellipse(7.5, 4.5, 2.6, 1.4, Hx("FFFAEC")); c.HL(5, 7, 6, K.Parch);
        c.Rect(13, 8, 11, 2, K.Wood[4]); c.HL(13, 8, 11, K.Wood[5]); c.HL(13, 9, 11, K.Wood[2]); c.Set(12, 8, K.Wood[2]); c.Set(24, 9, K.Wood[2]);
        foreach (var (x, y) in new[] { (3, 9), (4, 10), (17, 3), (22, 5), (26, 10), (11, 2) }) c.Set(x, y, K.Cream);
        // bowl with eggs
        U.Bowl(c, 28, 8, 8, K.Blue, K.BlueR[0]);
        c.Rect(29, 4, 3, 3, K.Cream); c.Set(29, 4, Rgba.Clear); c.Set(31, 6, K.Parch); c.Rect(32, 5, 3, 3, Hx("EED2AE")); c.Set(34, 7, Hx("D2A982"));
        // loaf on a board
        c.Rect(37, 3, 8, 7, K.Wood[3]); c.HL(37, 9, 8, K.Wood[1]); c.VL(37, 3, 7, K.Wood[4]);
        c.Ellipse(41, 6, 3.3, 2.2, K.Orange); c.Ellipse(40.5, 5.6, 2.2, 1.3, Hx("F4B87A")); c.Set(42, 6, Hx("B8662F")); c.Set(40, 5, K.Parch);
        // open lower storage with legs
        foreach (var lx in new[] { 0, 42 }) { c.Rect(lx, 16, 4, 18, p[2]); c.VL(lx, 16, 18, p[3]); c.VL(lx + 3, 16, 18, p[0]); }
        c.Rect(4, 16, 38, 12, K.Dark[2]);
        for (int x = 4; x < 42; x += 2) c.Set(x, 16, K.Dark[1]);
        c.Rect(4, 28, 38, 2, p[2]); c.HL(4, 28, 38, p[3]); c.HL(4, 29, 38, p[0]);
        c.Rect(8, 19, 9, 9, K.Roof[2]); c.VL(8, 19, 9, K.Roof[3]); c.VL(16, 19, 9, K.Roof[0]); c.HL(8, 19, 9, K.Roof[4]); c.HL(8, 22, 9, K.Roof[1]);
        c.Rect(22, 21, 14, 7, K.Pine[3]); c.HL(22, 21, 14, K.Pine[4]); c.HL(22, 27, 14, K.Pine[0]);
        for (int x = 23; x < 35; x += 3) { c.Rect(x, 19, 3, 3, Hx("C4A06A")); c.Set(x, 19, Hx("E0C48C")); c.Set(x + 2, 21, Hx("8F7552")); }
        for (int x = 23; x < 35; x += 2) c.VL(x, 22, 5, ((x / 2) % 2 == 0) ? K.Pine[2] : K.Pine[1]);
        c.HL(0, 33, 4, p[0]); c.HL(42, 33, 4, p[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas Desk()
    {
        var c = new Canvas(42, 36);
        var w = K.Wood;
        const int T = 3;   // headroom for the candle flame
        // top surface
        c.Rect(0, T, 42, 12, w[4]); c.HL(0, T, 42, w[5]); c.VL(0, T, 12, w[5]); c.VL(41, T, 12, w[2]);
        c.HL(1, T + 5, 40, w[3]); c.HL(1, T + 9, 40, w[3]);
        // open book
        c.Rect(3, T + 3, 12, 7, K.Cream); c.VL(9, T + 3, 7, K.Parch); c.HL(3, T + 9, 12, K.Sand);
        for (int y = T + 4; y < T + 9; y += 2) { c.HL(4, y, 4, K.Tan); c.HL(10, y, 4, K.Tan); }
        c.HL(3, T + 3, 12, K.Linen[4]);
        // inkwell and quill
        c.Rect(19, T + 5, 4, 4, K.Iron[1]); c.HL(19, T + 5, 4, K.Iron[3]); c.Set(20, T + 6, K.BlueR[4]);
        c.Line(21, T + 5, 25, T, K.Cream); c.Line(22, T + 5, 26, T, K.Parch); c.Set(26, T, K.Terra);
        // letters
        c.Rect(26, T + 5, 7, 5, K.Linen[3]); c.HL(26, T + 5, 7, K.Linen[4]); c.HL(26, T + 9, 7, K.Linen[1]); c.Rect(28, T + 6, 3, 2, K.Terra); c.Set(29, T + 6, K.RedR[4]);
        // candle in a brass holder
        c.Rect(36, T + 4, 4, 2, K.Mustard[3]); c.Rect(37, T + 1, 2, 3, K.Cream); c.VL(37, T + 1, 3, K.Linen[4]); c.Set(38, T, K.Gold); c.Set(38, T - 1, Hx("FBE9A8"));
        c.Rect(35, T + 6, 6, 1, K.Mustard[1]);
        // drawers
        c.Rect(0, T + 12, 42, 2, w[3]); c.HL(0, T + 12, 42, w[4]); c.HL(0, T + 13, 42, w[1]);
        c.Rect(4, T + 14, 34, 13, w[2]);
        c.Rect(5, T + 15, 15, 11, w[3]); c.HL(5, T + 15, 15, w[4]); c.HL(5, T + 25, 15, w[1]); c.VL(19, T + 15, 11, w[1]);
        c.Rect(22, T + 15, 15, 11, w[3]); c.HL(22, T + 15, 15, w[4]); c.HL(22, T + 25, 15, w[1]); c.VL(36, T + 15, 11, w[1]);
        c.Rect(11, T + 19, 3, 2, K.Mustard[3]); c.Rect(28, T + 19, 3, 2, K.Mustard[3]); c.Set(11, T + 19, K.Mustard[4]); c.Set(28, T + 19, K.Mustard[4]);
        foreach (var lx in new[] { 0, 38 }) { c.Rect(lx, T + 14, 4, 19, w[2]); c.VL(lx, T + 14, 19, w[4]); c.VL(lx + 3, T + 14, 19, w[0]); }
        c.HL(4, T + 27, 34, w[0]);
        return U.Finish(c, 0.4);
    }

    static void Pumpkin(Canvas c, double cx, double cy, double rx, double ry)
    {
        var o = new[] { Hx("A8531F"), Hx("C8692B"), Hx("DD8A3F"), Hx("EDA95B"), Hx("F8C98A") };
        for (int y = (int)(cy - ry - 1); y <= cy + ry + 1; y++)
            for (int x = (int)(cx - rx - 1); x <= cx + rx + 1; x++)
            {
                double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry;
                if (nx * nx + ny * ny > 1) continue;
                double light = -(nx * 0.6 + ny * 0.7);
                int idx = Math.Clamp((int)((0.5 + light * 0.45) * 5), 0, 4);
                double rib = Math.Abs(Math.Sin(nx * 2.9));
                if (rib < 0.16) idx = Math.Max(0, idx - 2);
                if (ny > 0.7) idx = Math.Min(idx, 1);
                c.Set(x, y, o[idx]);
            }
        c.Rect((int)cx - 1, (int)(cy - ry) - 2, 2, 3, K.Brown); c.Set((int)cx - 1, (int)(cy - ry) - 3, K.DGreen); c.Set((int)cx + 1, (int)(cy - ry) - 2, K.Green);
    }

    static Canvas Pumpkins()
    {
        var c = new Canvas(40, 28);
        var l = K.Leaf;
        for (int i = 0; i < 7; i++)
        {
            double lx = 4 + i * 5.2, ly = 14 + (i % 3) * 2;
            Draw.Blob(c, lx, ly, 5, 3.6, l, 600 + i, -0.05 + (i % 2) * 0.1, 2);
        }
        c.Line(2, 18, 36, 16, l[1]); c.Line(5, 20, 30, 20, l[2]);
        Pumpkin(c, 10, 20, 7, 5.5);
        Pumpkin(c, 27, 18, 6.5, 5);
        Pumpkin(c, 19, 23, 5, 3.8);
        for (int i = 0; i < 5; i++) Draw.Blob(c, 5 + i * 8, 12, 3.5, 2.4, l, 620 + i, 0.05, 2);
        c.Set(33, 12, K.Gold); c.Set(34, 12, K.Gold); c.Set(33, 13, K.Orange); c.Set(2, 14, K.Gold);
        return U.Finish(c, 0.38);
    }

    static Canvas Sunflowers()
    {
        var c = new Canvas(32, 50);
        var l = K.Leaf;
        var heads = new (int x, int y, int r, int bx)[] { (8, 10, 6, 10), (24, 7, 6, 22), (16, 20, 6, 15) };
        foreach (var h in heads)
            for (int y = h.y + 4; y < 50; y++)
            {
                double t = (y - (h.y + 4)) / (double)(49 - h.y - 4);
                int x = (int)Math.Round(h.x + (h.bx - h.x) * t);
                c.Set(x, y, K.Grass[2]); c.Set(x + 1, y, K.Grass[0]);
            }
        foreach (var (lx, ly, d) in new[] { (11, 38, -1), (14, 32, 1), (19, 42, 1), (9, 28, -1), (22, 30, 1), (16, 46, -1) })
            for (int j = -2; j <= 2; j++)
                for (int i = 0; i < 7 - Math.Abs(j) * 2; i++)
                {
                    int x = lx + d * i, y = ly + j - (i / 3);
                    c.Set(x, y, i < 2 ? l[3] : j < 0 ? l[4] : l[2]);
                }
        foreach (var h in heads)
        {
            c.Disc(h.x, h.y, h.r + 0.5, K.Mustard[1]);
            for (int a = 0; a < 360; a += 24)
            {
                double ar = a * Math.PI / 180;
                int px = (int)Math.Round(h.x + Math.Cos(ar) * (h.r + 0.2)), py = (int)Math.Round(h.y + Math.Sin(ar) * (h.r + 0.2));
                var col = (a / 24) % 2 == 0 ? K.Gold : Hx("FBE9A8");
                if (Math.Cos(ar) + Math.Sin(ar) > 0.4) col = K.Mustard[2];
                c.Set(px, py, col); c.Set(px + (Math.Cos(ar) > 0 ? -1 : 1), py, col);
            }
            c.Disc(h.x, h.y, h.r - 2.2, K.Brown2);
            c.Disc(h.x - 0.5, h.y - 0.5, h.r - 3.2, K.Brown);
            for (int a = 0; a < 360; a += 70) c.Set((int)(h.x + Math.Cos(a * 0.0175) * 1.7), (int)(h.y + Math.Sin(a * 0.0175) * 1.7), K.Tan);
            c.Set(h.x - 1, h.y - 1, K.Brown3);
        }
        return U.Finish(c, 0.4);
    }
}
