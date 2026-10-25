namespace PixelArt;

/// <summary>Group B: 16x16 icons (full colour, 1px ink outline).</summary>
public static class Icons
{
    static Canvas N() => new(16, 16);

    static void Add(string name, Canvas c) => Registry.Add("B_icons", name, c, 16, 16);

    static Canvas Fin(Canvas c) { c.Outline(P.Ink); return c; }

    public static Canvas Magnifier()
    {
        var c = N();
        // handle first so the lens overlaps its start
        for (int i = 0; i < 5; i++) { c.Set(9 + i, 9 + i, P.MidBrown); c.Set(10 + i, 9 + i, P.MidBrown); c.Set(9 + i, 10 + i, P.Brown); }
        c.Set(10, 9, P.Tan);
        c.Disc(6.5, 6.5, 5.5, P.Gold);
        c.Paint((x, y) => x + y > 11 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Disc(6.5, 6.5, 3.7, P.LightBlue);
        c.Paint((x, y) => x + y > 11 && c.Get(x, y) == P.LightBlue, P.Blue);
        c.Set(4, 4, P.White); c.Set(5, 4, P.White); c.Set(4, 5, P.White);
        return Fin(c);
    }

    public static Canvas Locket()
    {
        var c = N();
        // chain
        for (int i = 0; i < 5; i++)
        {
            c.Set(2 + i, 0 + i, i % 2 == 0 ? P.Sand : P.Tan);
            c.Set(13 - i, 0 + i, i % 2 == 0 ? P.Sand : P.Tan);
        }
        c.Rect(7, 4, 2, 2, P.Gold);
        c.Disc(8, 10.3, 5, P.Gold);
        c.Paint((x, y) => x + y > 17 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Disc(8, 10.3, 3.2, P.Cream);
        // tiny heart in the picture window
        c.Set(7, 9, P.Terracotta); c.Set(9, 9, P.Terracotta);
        c.Rect(7, 10, 3, 1, P.Terracotta); c.Set(8, 11, P.Terracotta);
        c.Set(5, 7, P.LightGold); c.Set(6, 6, P.LightGold);
        return Fin(c);
    }

    public static void Build()
    {
        Add("icon_scroll", Fin(Canvas.FromMap(16, 16, new[]
        {
            "................",
            "..kkkkkkkkkkkk..",
            ".ktssccccccsstk.",
            ".kttssssssssttk.",
            "..kkkkkkkkkkkk..",
            "...kppppppppk...",
            "...kpbbbbbppk...",
            "...kppppppppk...",
            "...kpbbbbbbpk...",
            "...kppprrpppk...",
            "...kpbbbrrppk...",
            "...kppppppppk...",
            "..kkkkkkkkkkkk..",
            ".ktssccccccsstk.",
            ".kttssssssssttk.",
            "..kkkkkkkkkkkk..",
        }, "icon_scroll")));

        Add("icon_journal", Fin(Canvas.FromMap(16, 16, new[]
        {
            "................",
            "...kkkkkkkkkk...",
            "..kRrrrrrrrrrk..",
            "..kRrrrrrrrrrk..",
            "..kRrrrryrrrrk..",
            "..kRrrryLyrrrk..",
            "..kRrrrryrrrrk..",
            "..kRrrrrrrrrrk..",
            "..kRrrrrrrrrrk..",
            "..kRrrrrrrrrrk..",
            "..kRrrrrrrrrrk..",
            "..kRRRRRRRRRRk..",
            "..kccccccceeck..",
            "..kkkkkkkkeekk..",
            ".........keek...",
            ".........k..k...",
        }, "icon_journal")));

        // album: photo frame with sun + mountain picture
        var album = N();
        album.Rect(1, 2, 14, 12, P.Gold);
        album.Rect(1, 2, 14, 1, P.LightGold); album.Rect(1, 2, 1, 12, P.LightGold);
        album.Rect(14, 3, 1, 11, P.DarkGold); album.Rect(2, 13, 13, 1, P.DarkGold);
        album.Rect(3, 4, 10, 8, P.Brown);
        album.Rect(4, 5, 8, 6, P.Blue);
        album.Rect(9, 6, 2, 2, P.LightGold);
        album.Tri(3.5, 11, 7.5, 5.5, 11.5, 11, P.DarkGreen);
        album.Set(7, 6, P.Cream); album.Set(8, 6, P.Cream);
        album.Rect(4, 10, 8, 1, P.Green);
        Add("icon_album", Fin(album));

        Add("icon_keepsake", Locket());

        // trophy
        var tr = N();
        tr.Rect(3, 1, 10, 2, P.Gold);
        tr.Rect(3, 3, 10, 2, P.Gold);
        tr.Rect(4, 5, 8, 1, P.Gold);
        tr.Rect(5, 6, 6, 1, P.Gold);
        tr.Rect(6, 7, 4, 1, P.Gold);
        tr.Frame(1, 2, 3, 4, P.DarkGold); tr.Frame(12, 2, 3, 4, P.DarkGold);
        tr.Rect(7, 8, 2, 3, P.DarkGold);
        tr.Rect(4, 11, 8, 3, P.Gold);
        tr.Rect(4, 13, 8, 1, P.DarkGold);
        tr.Paint((x, y) => x >= 11 && y < 8 && tr.Get(x, y) == P.Gold, P.DarkGold);
        tr.Rect(4, 2, 1, 3, P.LightGold); tr.Set(5, 5, P.LightGold);
        tr.Set(7, 3, P.Cream); tr.Set(8, 3, P.Cream); tr.Set(7, 4, P.Cream);
        Add("icon_trophy", Fin(tr));

        // journey: signpost on a green mound
        var sp = N();
        sp.Ellipse(8, 14.6, 6.5, 2.2, P.Green);
        sp.Rect(7, 1, 2, 13, P.MidBrown);
        sp.Rect(3, 1, 9, 1, P.Cream); sp.Rect(3, 2, 11, 2, P.Cream); sp.Rect(3, 4, 9, 1, P.Cream);
        sp.Rect(3, 4, 9, 1, P.Sand); sp.Set(12, 3, P.Sand); sp.Set(13, 3, P.Sand);
        sp.Rect(4, 6, 9, 1, P.Gold); sp.Rect(2, 7, 11, 2, P.Gold); sp.Rect(4, 9, 9, 1, P.Gold);
        sp.Rect(4, 9, 9, 1, P.DarkGold); sp.Set(2, 8, P.DarkGold); sp.Set(3, 8, P.DarkGold);
        sp.Rect(7, 12, 2, 2, P.MidBrown);
        Add("icon_journey", Fin(sp));

        // stats: three bars on a baseline
        var st = N();
        foreach (var (x, h) in new[] { (1, 5), (6, 8), (11, 11) })
        {
            st.Rect(x, 13 - h, 4, h, P.Gold);
            st.Rect(x, 13 - h, 1, h, P.LightGold);
            st.Rect(x + 3, 13 - h, 1, h, P.DarkGold);
        }
        st.Rect(1, 14, 14, 1, P.Tan);
        Add("icon_stats", Fin(st));

        // settings: gear
        var gear = N();
        gear.Disc(8, 8, 5.4, P.Sand);
        gear.Rect(6, 1, 4, 3, P.Sand); gear.Rect(6, 12, 4, 3, P.Sand);
        gear.Rect(1, 6, 3, 4, P.Sand); gear.Rect(12, 6, 3, 4, P.Sand);
        gear.Rect(2, 2, 2, 2, P.Sand); gear.Rect(3, 3, 2, 2, P.Sand);
        gear.Rect(12, 2, 2, 2, P.Sand); gear.Rect(11, 3, 2, 2, P.Sand);
        gear.Rect(2, 12, 2, 2, P.Sand); gear.Rect(3, 11, 2, 2, P.Sand);
        gear.Rect(12, 12, 2, 2, P.Sand); gear.Rect(11, 11, 2, 2, P.Sand);
        gear.Paint((x, y) => x + y >= 21 && gear.Get(x, y) == P.Sand, P.Tan);
        gear.Paint((x, y) => x + y <= 10 && gear.Get(x, y) == P.Sand, P.Cream);
        gear.Disc(8, 8, 2.6, Rgba.Clear);
        Add("icon_settings", Fin(gear));

        // lock
        var lk = N();
        lk.Rect(5, 1, 6, 2, P.Sand);
        lk.Rect(4, 3, 2, 5, P.Sand); lk.Rect(10, 3, 2, 5, P.Sand);
        lk.Set(10, 3, P.Tan); lk.Rect(11, 3, 1, 5, P.Tan);
        lk.Rect(2, 7, 12, 8, P.Gold);
        lk.Rect(2, 7, 12, 1, P.LightGold); lk.Rect(2, 7, 1, 8, P.LightGold);
        lk.Rect(13, 8, 1, 7, P.DarkGold); lk.Rect(3, 14, 11, 1, P.DarkGold);
        lk.Rect(6, 10, 4, 2, P.Ink); lk.Rect(7, 12, 2, 2, P.Ink);
        Add("icon_lock", Fin(lk));

        // checkbox, checked / empty
        Add("icon_check", Fin(CheckBox(true)));
        Add("icon_box", Fin(CheckBox(false)));

        // star
        var star = N();
        star.Star(8, 8.7, 6.7, 3.0, 5, P.Gold);
        star.Paint((x, y) => x >= 8 && star.Get(x, y) == P.Gold, P.DarkGold);
        star.Paint((x, y) => x < 8 && y < 8 && star.Get(x, y) == P.Gold && x > 3, P.LightGold);
        star.Set(7, 7, P.Cream);
        Add("icon_star", Fin(star));

        // pause
        var pa = N();
        foreach (var x in new[] { 3, 9 })
        {
            pa.Rect(x, 2, 4, 12, P.Cream);
            pa.Rect(x + 3, 2, 1, 12, P.Sand); pa.Rect(x, 13, 4, 1, P.Sand);
        }
        Add("icon_pause", Fin(pa));

        // play
        var pl = N();
        pl.Tri(3.5, 1.5, 3.5, 14.5, 13.5, 8, P.Cream);
        pl.Paint((x, y) => y >= 8 && pl.Get(x, y) == P.Cream, P.Sand);
        Add("icon_play", Fin(pl));

        // close (X)
        var cl = N();
        for (int i = 0; i < 10; i++)
        {
            foreach (var (bx, by) in new[] { (3 + i, 3 + i), (12 - i + 1, 3 + i) })
            {
                cl.Set(bx, by, P.Cream); cl.Set(bx + 1, by, P.Cream); cl.Set(bx, by + 1, P.Sand); cl.Set(bx + 1, by + 1, P.Sand);
            }
        }
        Add("icon_close", Fin(cl));

        // arrows
        var ar = N();
        ar.Rect(2, 6, 7, 4, P.Cream);
        ar.Tri(7.5, 2, 14, 8, 7.5, 14, P.Cream);
        ar.Paint((x, y) => y >= 8 && ar.Get(x, y) == P.Cream, P.Sand);
        ar = Fin(ar);
        Add("icon_arrow_left", ar.FlipH());
        Add("icon_arrow_right", ar);

        // help: round blue badge with a pixel question mark
        var he = N();
        he.Disc(8, 8, 6.6, P.Blue);
        he.Paint((x, y) => x + y > 17 && he.Get(x, y) == P.Blue, P.DarkBlue);
        he.Ring(8, 8, 6.6, 5.6, P.LightBlue);
        he.Paint((x, y) => x + y > 17 && he.Get(x, y) == P.LightBlue, P.Blue);
        he.Map(5, 3, new[]
        {
            ".cccc.",
            "cc..cc",
            "....cc",
            "...cc.",
            "..cc..",
            "..cc..",
            "......",
            "..cc..",
            "..cc..",
        }, "help_q");
        Add("icon_help", Fin(he));

        // door
        var dr = N();
        dr.Rect(3, 1, 10, 14, P.MidBrown); dr.Set(3, 1, Rgba.Clear); dr.Set(12, 1, Rgba.Clear);
        dr.Rect(4, 2, 8, 13, P.Orange);
        dr.Rect(5, 3, 6, 5, P.DarkOrange); dr.Rect(6, 4, 4, 3, P.Orange);
        dr.Rect(5, 10, 6, 4, P.DarkOrange); dr.Rect(6, 11, 4, 2, P.Orange);
        dr.Rect(9, 8, 2, 2, P.Gold); dr.Set(9, 8, P.LightGold);
        Add("icon_door", Fin(dr));

        // memory: sparkle
        var mem = N();
        mem.Star(7, 8.6, 6.4, 1.7, 4, P.Gold);
        mem.Paint((x, y) => x >= 7 && y >= 9 && mem.Get(x, y) == P.Gold, P.DarkGold);
        mem.Set(12, 2, P.LightGold); mem.Set(11, 3, P.LightGold); mem.Set(12, 3, P.Cream); mem.Set(13, 3, P.LightGold); mem.Set(12, 4, P.LightGold);
        mem.Set(7, 8, P.Cream); mem.Set(6, 8, P.Cream); mem.Set(7, 7, P.Cream);
        Add("icon_memory", Fin(mem));

        // objective: solid gold diamond marker
        var ob = N();
        ob.Poly(new[] { (8.0, 1.0), (14.5, 8.0), (8.0, 15.0), (1.5, 8.0) }, P.Gold);
        ob.Poly(new[] { (8.0, 1.0), (1.5, 8.0), (8.0, 8.0) }, P.LightGold);
        ob.Poly(new[] { (8.0, 8.0), (14.5, 8.0), (8.0, 15.0) }, P.DarkGold);
        ob.Poly(new[] { (8.0, 5.0), (11.0, 8.0), (8.0, 11.0), (5.0, 8.0) }, P.Orange);
        Add("icon_objective", Fin(ob));

        // optional: teal compass-style ring with cardinal ticks
        var op = N();
        op.Ring(8, 8, 5.8, 3.4, P.Teal);
        op.Paint((x, y) => x + y < 14 && op.Get(x, y) == P.Teal, P.LightBlue);
        op.Paint((x, y) => x + y > 17 && op.Get(x, y) == P.Teal, P.DarkTeal);
        op.Rect(7, 1, 2, 2, P.Cream); op.Rect(7, 13, 2, 2, P.Cream);
        op.Rect(1, 7, 2, 2, P.Cream); op.Rect(13, 7, 2, 2, P.Cream);
        op.Rect(7, 7, 2, 2, P.Cream);
        Add("icon_optional", Fin(op));

        // sound: speaker with two waves
        var so = N();
        so.Rect(1, 6, 3, 4, P.Sand);
        so.Poly(new[] { (4.0, 6.0), (8.0, 2.0), (8.0, 14.0), (4.0, 10.0) }, P.Cream);
        so.Rect(1, 9, 3, 1, P.Tan);
        so.Paint((x, y) => y >= 8 && x >= 4 && so.Get(x, y) == P.Cream, P.Sand);
        foreach (var (x, y) in new[] { (10, 6), (11, 7), (11, 8), (10, 9) }) so.Set(x, y, P.Gold);
        foreach (var (x, y) in new[] { (13, 4), (14, 5), (14, 6), (14, 7), (14, 8), (14, 9), (14, 10), (13, 11) }) so.Set(x, y, P.Gold);
        Add("icon_sound", Fin(so));

        Add("icon_text", Magnifier());

        // motion: three wavy lines
        var mo = N();
        int[] wave = { 0, -1, -1, 0, 0, 1, 1, 0 };
        for (int line = 0; line < 3; line++)
            for (int x = 1; x < 15; x++)
            {
                int y = 2 + line * 5 + wave[(x + line * 3) % 8];
                mo.Set(x, y, line == 1 ? P.Gold : P.Cream);
                mo.Set(x, y + 1, line == 1 ? P.DarkGold : P.Sand);
                mo.Set(x, y + 2, P.Ink);
            }
        Add("icon_motion", mo);

        // save: floppy disk
        var sv = N();
        sv.Rect(1, 1, 14, 14, P.DarkBlue);
        sv.Set(13, 1, Rgba.Clear); sv.Set(14, 1, Rgba.Clear); sv.Set(14, 2, Rgba.Clear);
        sv.Rect(1, 1, 12, 1, P.Blue); sv.Rect(1, 1, 1, 14, P.Blue);
        sv.Rect(4, 1, 7, 5, P.Sand); sv.Rect(4, 5, 7, 1, P.Tan); sv.Rect(8, 2, 2, 3, P.DarkBlue);
        sv.Rect(3, 8, 10, 7, P.Cream);
        sv.Rect(5, 10, 6, 1, P.Tan); sv.Rect(5, 12, 5, 1, P.Tan);
        Add("icon_save", Fin(sv));

        Add("icon_hand", Fin(Canvas.FromMap(16, 16, new[]
        {
            "................",
            ".....kk.........",
            "....kppk........",
            "....kppk........",
            "....kppk........",
            "....kppk........",
            "....kppkkk.kk.k.",
            "....kppkppkppkpk",
            ".kkkkppkppkppkpk",
            "kppppppsppsppspk",
            "kppppppppppppppk",
            ".kppppppppppppk.",
            "..kppppppppppk..",
            "..keeeeeeeeeek..",
            "..kEEEEEEEEEEk..",
            "..kkkkkkkkkkkk..",
        }, "icon_hand")));
    }

    static Canvas CheckBox(bool checkedBox)
    {
        var c = N();
        c.RectN(1, 1, 14, 14, P.Tan);
        c.Rect(2, 2, 12, 12, P.Cream);
        c.Rect(2, 2, 12, 1, P.Sand); c.Rect(2, 2, 1, 12, P.Sand);
        if (checkedBox)
            foreach (var (col, ox) in new[] { (P.DarkGreen, 1), (P.Green, 0) })
            {
                c.Line(4 + ox, 8, 6 + ox, 10, col); c.Line(4 + ox, 7, 6 + ox, 9, col);
                c.Line(6 + ox, 10, 11 + ox, 4, col); c.Line(6 + ox, 9, 11 + ox, 3, col);
            }
        return c;
    }
}
