namespace WorldArtCh3;

/// <summary>Props and overlays that only exist once the first blast has fallen.</summary>
public static partial class Sprites
{
    static void Damage()
    {
        // fresh rubble: brighter, powdered with plaster dust
        Reg("prop_c3_rubble_new_a", Rubble(30, 16, 11, true));
        Reg("prop_c3_rubble_new_b", Rubble(40, 22, 12, true, true));
        Reg("prop_c3_rubble_new_c", Rubble(22, 12, 13, true));
        Reg("prop_c3_rubble_new_big", Rubble(76, 42, 14, true, true));
        // scattered debris decals (flat on the ground)
        for (int v = 0; v < 4; v++) Reg($"prop_c3_debris_{v}", Debris(28 + v * 6, 12 + v * 2, 20 + v), outline: false);
        FallenShutter();
        DroppedThings();
        WindowOverlays();
        for (int v = 0; v < 5; v++) CrackOverlay(v);
        Shards();
        DustPatches();
    }

    static Canvas Debris(int w, int h, int seed)
    {
        var c = C(w, h);
        var rnd = new Random(seed);
        // pale dust smear first, then a handful of loose chunks
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j + .5 - h / 2.0) / (h / 2.0);
                double d = dx * dx + dy * dy;
                if (d < 1 && Fx.R(i, j, seed) > 0.35 + d * 0.5 && ((i + j) & 1) == 0) c.Set(i, j, Dust[1 + (int)(Fx.R(i, j, seed + 1) * 3)]);
            }
        for (int n = 0; n < w / 6; n++)
        {
            int cw = 3 + rnd.Next(4), ch = 2 + rnd.Next(3);
            int x = 2 + rnd.Next(Math.Max(1, w - cw - 4)), y = 2 + rnd.Next(Math.Max(1, h - ch - 3));
            var ramp = rnd.Next(3) switch { 0 => Brick, 1 => Concrete, _ => Plast };
            Chunk(c, x, y, cw, ch, ramp, seed + n, true);
            // soft shadow under each chunk
            for (int i = 0; i < cw; i++) if (!c.Opaque(x + i, y + ch)) c.Set(x + i, y + ch, K.ShadowP);
        }
        return c;
    }

    static void FallenShutter()
    {
        var c = C(24, 14);
        // a wooden shutter leaf lying on the pavement, slightly askew
        for (int j = 0; j < 10; j++)
            for (int i = 0; i < 20; i++)
            {
                int xx = 2 + i + j / 4, yy = 1 + j;
                int idx = i % 3 == 0 ? 1 : 2;
                if (j == 0 || j == 9) idx = 3; if (i == 0 || i == 19) idx = 0;
                if ((j == 3 || j == 6) && i > 0 && i < 19) idx = 0;
                c.Set(xx, yy, K.TealR[Math.Clamp(idx, 0, 4)]);
            }
        c.Set(21, 3, K.Iron[3]); c.Set(5, 8, K.Iron[3]);
        for (int i = 4; i < 22; i++) if (!c.Opaque(i, 11)) c.Set(i, 11, K.ShadowP);
        Reg("prop_c3_shutter_fallen", c);
    }

    static void DroppedThings()
    {
        // overturned basket with a loaf and some fruit rolling out
        var b = C(30, 16);
        b.Ellipse(11, 9, 9, 5, K.Wood[1]); b.Ellipse(11, 8, 8, 4, K.Wood[3]);
        for (int i = 3; i < 19; i += 3) for (int j = 5; j < 12; j++) if (b.Opaque(i, j)) b.Set(i, j, K.Wood[2]);
        for (int j = 4; j < 13; j += 3) for (int i = 3; i < 19; i++) if (b.Opaque(i, j)) b.Set(i, j, K.Wood[1]);
        b.Ellipse(24, 10, 5, 2.6, K.Sand); b.Ellipse(23, 9, 4, 1.8, K.Orange); b.Rect(21, 8, 6, 1, K.Gold);
        b.Disc(20, 13, 2, K.RedR[2]); b.Disc(28, 5, 2, K.RedR[2]); b.Set(27, 4, K.RedR[4]);
        Reg("prop_c3_dropped_basket", b);
        // a small knitted scarf lying where someone was running
        var s = C(20, 8);
        for (int i = 0; i < 18; i++) { int y = 3 + (int)Math.Round(Math.Sin(i * 0.5) * 1.4); for (int k = 0; k < 3; k++) s.Set(1 + i, y + k, (i / 2 % 2 == 0 ? K.TealR : K.Mustard)[k == 0 ? 3 : k == 1 ? 2 : 0]); }
        for (int k = 0; k < 3; k++) { s.Set(18, 6 + (k % 2), K.Mustard[3]); }
        Reg("prop_c3_scarf", s);
        // a child's toy rabbit, face down on the cobbles
        var r = C(12, 12);
        r.Ellipse(6, 8, 4.5, 3, K.Cream); r.Ellipse(6, 8, 3.5, 2, Rgba.Hex("F4E4D0"));
        r.Rect(3, 2, 2, 5, K.Cream); r.Rect(7, 3, 2, 5, K.Parch); r.Set(3, 2, K.Pink[2]); r.Set(7, 3, K.Pink[2]);
        r.Set(9, 8, K.Brown2); r.Set(2, 9, K.Pink[1]);
        Reg("prop_c3_toy_rabbit", r);
    }

    // ---- windows that go dark / break after the first blast -------------------------------------------------------
    static void WindowOverlays()
    {
        // lights out: dark glass and a half-drawn curtain
        var d = C(16, 22);
        for (int j = 0; j < 22; j++) for (int i = 0; i < 16; i++) d.Set(i, j, Dusk.Mix(H("4A5688"), H("232648"), j / 22.0));
        for (int j = 0; j < 22; j++) d.Set(8, j, K.Wood[1]);
        for (int i = 0; i < 16; i++) { d.Set(i, 10, K.Wood[1]); d.Set(i, 0, K.Wood[2]); }
        for (int j = 1; j < 20; j++) { int cw = 5 - j / 7; for (int i = 0; i < cw; i++) { d.Set(i, j, K.Linen[(i + j / 3) % 3]); d.Set(15 - i, j, K.Linen[(i + j / 3 + 1) % 3]); } }
        RegRaw("fx_c3_window_dark", Dusk.Tint(d));
        // blown-out pane: black gaps, hanging shards, curtain pulled out
        var b = C(16, 22);
        for (int j = 0; j < 22; j++) for (int i = 0; i < 16; i++) b.Set(i, j, H("14121E"));
        var shard = new (double, double)[][] {
            new[] { (0.0, 0.0), (7.0, 0.0), (3.0, 9.0), (0.0, 6.0) },
            new[] { (9.0, 0.0), (16.0, 0.0), (16.0, 5.0), (12.0, 12.0) },
            new[] { (0.0, 17.0), (4.0, 12.0), (7.0, 22.0), (0.0, 22.0) },
            new[] { (12.0, 22.0), (16.0, 15.0), (16.0, 22.0) } };
        foreach (var poly in shard) b.Poly(poly, H("46528A"));
        foreach (var poly in shard) for (int i = 0; i < poly.Length; i++) { var p = poly[i]; var q = poly[(i + 1) % poly.Length]; b.Line((int)p.Item1, (int)p.Item2, (int)q.Item1, (int)q.Item2, H("B8C4E8")); }
        for (int j = 4; j < 20; j++) { int cw = 3 + (j % 5 == 0 ? 1 : 0); for (int i = 0; i < cw; i++) b.Set(6 + i + (j / 5), j, K.Linen[(i + j) % 3]); }
        for (int i = 0; i < 16; i++) { b.Set(i, 0, K.Wood[2]); }
        RegRaw("fx_c3_window_broken", Dusk.Tint(b));
    }

    static void CrackOverlay(int v)
    {
        int w = 40 + v * 6, h = 44;
        var c = C(w, h);
        var rnd = new Random(100 + v);
        var dark = new Rgba(44, 38, 60, 190); var edge = new Rgba(200, 190, 194, 150);
        int x = 4 + rnd.Next(w - 8), y = 0;
        var branches = new List<(int x, int y)>();
        while (y < h - 3)
        {
            c.Set(x, y, dark);
            if (rnd.Next(6) == 0) c.Set(x + 1, y, edge);
            if (rnd.Next(12) == 0) branches.Add((x, y));
            int d = rnd.Next(8);
            if (d == 0) x--; else if (d == 1) x++;
            x = Math.Clamp(x, 2, w - 3);
            y++;
        }
        foreach (var (bx, by) in branches)
        {
            int dir = rnd.Next(2) * 2 - 1, cx = bx, cy = by;
            for (int i = 0; i < 3 + rnd.Next(5); i++) { cx += dir; if (rnd.Next(2) == 0) cy++; c.Set(Math.Clamp(cx, 0, w - 1), Math.Min(cy, h - 1), dark); }
        }
        RegRaw($"fx_c3_crack_{v}", c);
    }

    static void Shards()
    {
        var c = C(40, 12);
        var rnd = new Random(7);
        for (int n = 0; n < 16; n++)
        {
            int x = rnd.Next(38), y = rnd.Next(10);
            var col = new Rgba(200, 214, 244, 230);
            c.Set(x, y, col); if (rnd.Next(2) == 0) c.Set(x + 1, y, new Rgba(120, 140, 200, 210)); if (rnd.Next(3) == 0) c.Set(x, y + 1, new Rgba(255, 255, 255, 240));
        }
        RegRaw("fx_c3_glass_shards", c);
    }

    static void DustPatches()
    {
        for (int v = 0; v < 3; v++)
        {
            int w = 40 + v * 16, h = 14 + v * 2;
            var c = C(w, h);
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                {
                    double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j + .5 - h / 2.0) / (h / 2.0);
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    double f = (1 - d) * 1.2 + (Fx.Fbm(i / 5.0, j / 4.0, 50 + v) - 0.5) * 0.7 + Fx.Bay(i, j) * 0.25;
                    if (f < 0.25) continue;
                    int a = f > 0.75 ? 150 : f > 0.5 ? 110 : 70;
                    c.Set(i, j, new Rgba(214, 204, 192, (byte)a));
                }
            RegRaw($"fx_c3_dust_patch_{v}", c);
        }
    }
}
