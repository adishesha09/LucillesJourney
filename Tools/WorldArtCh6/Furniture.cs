namespace WorldArtCh6;

public static class Furniture
{
    static Rgba H(string h) => Rgba.Hex(h);

    public static void Build()
    {
        TableDecision();
        BedBare();
        Bags();
        BagsStowed();
        DoorLock();
        KeyTable();
        HangLamp();
        DryRack();
        Banister();
        Sprites.In("prop_bed_double", "bed_made");
    }

    // ---- the small table with the three things and the coins ---------------------------------------------------
    static void TableDecision()
    {
        var c = new Canvas(48, 44);
        var W = K.Wood;
        // top surface (scrubbed pale wood so the table stands out on the dark floor)
        var P = K.Pine;
        for (int j = 4; j < 31; j++)
            for (int i = 1; i < 47; i++)
            {
                if ((i == 1 || i == 46) && (j == 4 || j == 30)) continue;
                int row = (j - 4) / 6, ly = (j - 4) % 6;
                int idx = 3 + ((Fx.R(row, i / 14, 301) > 0.75) ? 1 : 0);
                if (ly == 5) idx = 2;
                if (j == 4) idx = 4;
                if (i == 1) idx = 4;
                if (i == 46) idx = 2;
                if (j == 30) idx = 2;
                c.Set(i, j, P[Math.Min(4, idx)]);
            }
        // wood grain flecks
        for (int k = 0; k < 18; k++) { int x = 4 + (int)(Fx.R(k, 0, 302) * 40), y = 6 + (int)(Fx.R(k, 1, 302) * 22); if (c.Get(x, y).A != 0) c.Set(x, y, P[2]); }
        // apron
        for (int j = 31; j < 37; j++) for (int i = 2; i < 46; i++) c.Set(i, j, j == 31 ? W[3] : j == 36 ? W[0] : i > 41 ? W[1] : W[2]);
        for (int j = 33; j < 36; j++) { c.Set(22, j, W[0]); c.Set(23, j, W[3]); }   // drawer line
        c.Set(21, 34, K.Gold); c.Set(22, 34, K.Orange);
        // legs
        foreach (var lx in new[] { 3, 42 }) for (int j = 37; j < 44; j++) for (int i = 0; i < 3; i++) c.Set(lx + i, j, W[i == 0 ? 3 : i == 2 ? 0 : 1]);

        // --- school notice envelope (left) ---
        Rgba paper = H("F4EAD2"), paperLo = H("DCCBA4"), blue = H("4F7189"), blueHi = H("6B8FA8");
        for (int j = 0; j < 9; j++) for (int i = 0; i < 12; i++) c.Set(5 + i, 13 + j, j == 0 || i == 0 ? H("FFFAEC") : (j == 8 || i == 11) ? paperLo : paper);
        // the letter sticking out, with a school-blue header and a small crest
        for (int j = 0; j < 4; j++) for (int i = 0; i < 9; i++) c.Set(7 + i, 9 + j, j == 0 ? blueHi : j == 1 ? blue : paper);
        for (int i = 0; i < 6; i += 2) c.Set(8 + i, 12, K.Brown);
        c.Set(15, 10, K.Gold);
        // envelope flap lines + red seal + address squiggle
        for (int i = 0; i < 6; i++) { c.Set(5 + i, 14 + i / 2, paperLo); c.Set(16 - i, 14 + i / 2, paperLo); }
        c.Set(10, 17, K.Terra); c.Set(11, 17, K.Terra); c.Set(10, 18, K.RedR[0]); c.Set(11, 18, K.RedR[0]);
        c.Set(7, 20, K.Brown); c.Set(8, 20, K.Brown); c.Set(10, 20, K.Brown); c.Set(13, 20, K.Brown);

        // --- the bill: electricity + water (middle) ---
        for (int j = 0; j < 15; j++) for (int i = 0; i < 11; i++) c.Set(19 + i, 7 + j, j == 0 || i == 0 ? H("FFFAEC") : (j == 14 || i == 10) ? paperLo : H("F0E6CE"));
        // second sheet peeking out underneath
        for (int j = 0; j < 13; j++) c.Set(30, 9 + j, H("D2C09A"));
        for (int i = 0; i < 10; i++) c.Set(20 + i, 22, H("D2C09A"));
        // lightning bolt (electricity)
        foreach (var (x, y) in new[] { (24, 9), (23, 10), (22, 11), (23, 11), (24, 11), (23, 12), (22, 13) }) c.Set(x, y, K.Gold);
        foreach (var (x, y) in new[] { (24, 10), (24, 12) }) c.Set(x, y, K.Orange);
        // water drop
        foreach (var (x, y) in new[] { (26, 15), (25, 16), (26, 16), (27, 16), (24, 17), (25, 17), (26, 17), (27, 17), (28, 17), (25, 18), (26, 18), (27, 18) }) c.Set(x, y, H("6FB0C8"));
        c.Set(25, 17, H("D6F1F2")); c.Set(25, 16, H("A9DDE6"));
        // amount lines
        for (int i = 0; i < 4; i++) c.Set(21 + i, 20, K.Brown);
        c.Set(27, 20, K.Terra); c.Set(28, 20, K.Terra);

        // --- groceries basket (right) ---
        Rgba wicker = H("C79E68"), wickerHi = H("DDB67B"), wickerLo = H("8E6A44");
        for (int j = 0; j < 9; j++) for (int i = 0; i < 12; i++)
            {
                if ((j == 0 || j == 8) && (i == 0 || i == 11)) continue;
                var col = (i + j) % 3 == 0 ? wickerLo : wicker;
                if (j == 0) col = wickerHi; if (j == 8) col = wickerLo;
                c.Set(33 + i, 17 + j, col);
            }
        // contents: loaf, apple, green leaf
        for (int i = 0; i < 7; i++) c.Set(34 + i, 15, H("D9A05A"));
        for (int i = 0; i < 6; i++) c.Set(35 + i, 14, H("E8B873"));
        c.Set(36, 14, H("FFE3A8")); c.Set(37, 14, H("FFE3A8"));
        for (int j = 0; j < 3; j++) for (int i = 0; i < 3; i++) c.Set(41 + i, 14 + j, (i + j) == 0 ? H("EE9A86") : H("C05A4A"));
        c.Set(42, 13, K.DGreen); c.Set(43, 13, K.Green);
        // handle arc
        foreach (var (x, y) in new[] { (35, 16), (35, 15), (36, 13), (37, 12), (38, 12), (39, 12), (40, 12), (41, 12), (42, 13), (43, 14), (43, 15), (43, 16) }) c.Set(x - 2 + 2, y - 2 > 0 ? y - 1 : y, wickerHi);

        // --- a few coins ---
        var coins = new[] { (20, 26), (23, 27), (26, 26), (24, 24) };
        foreach (var (x, y) in coins)
        {
            c.Set(x, y, K.Gold); c.Set(x + 1, y, K.Gold); c.Set(x, y + 1, K.Orange); c.Set(x + 1, y + 1, H("B8893A"));
            c.Set(x, y, H("FBE9A8"));
        }
        c.Set(29, 26, H("D3CAB8")); c.Set(30, 26, H("B8AE9C")); c.Set(29, 27, H("9A9082")); c.Set(30, 27, H("7A7066"));
        S.Add("prop_c6_table_decision", Kit.Outlined(c));
    }

    // ---- beds -----------------------------------------------------------------------------------------------------
    static void BedBare()
    {
        var src = S.Ext("prop_bed_double");
        var c = src.Clone();
        // recolour the quilt (teal squares, plus the yellow/red patches) to a dull, crumpled grey wool blanket
        var wool = new[] { H("5A5560"), H("736D74"), H("8B8588"), H("A29B9A"), H("B9B1AB") };
        for (int y = 0; y < c.H; y++)
            for (int x = 0; x < c.W; x++)
            {
                var p = c.Get(x, y);
                if (p.A == 0) continue;
                bool teal = p.G > p.R + 14 && p.B > p.R + 8;
                bool patch = y > 26 && y < 48 && x > 1 && x < c.W - 2 && ((p.R > 0xB0 && p.G > 0x80 && p.B < 0x80) || (p.R > 0xB0 && p.G < 0x70 && p.B < 0x60));
                if (!teal && !patch) continue;
                double fold = Fx.VN(x / 5.0 + y * 0.4, y / 3.0, 311);
                int idx = fold > 0.72 ? 4 : fold > 0.5 ? 3 : fold > 0.28 ? 2 : 1;
                if (((x + y) % 11) == 0) idx = Math.Max(0, idx - 1);
                c.Set(x, y, wool[idx]);
            }
        // rumpled pillow: darker crease and a coat thrown across the foot
        for (int i = 4; i < c.W - 5; i++) { c.Set(i, 32, wool[0]); if (i % 3 != 0) c.Set(i, 33, wool[1]); }
        var coat = new[] { H("5E4A3C"), H("7A6048"), H("937659") };
        for (int j = 0; j < 7; j++)
            for (int i = 0; i < 14; i++)
            {
                if ((i == 0 || i == 13) && (j == 0 || j == 6)) continue;
                c.Set(20 + i, 38 + j, j == 0 ? coat[2] : j > 4 ? coat[0] : coat[1]);
            }
        c.Set(22, 40, K.Gold); c.Set(23, 40, K.Orange);
        c = E.Tinted(c, 0.95, 0.90, 0.90);
        S.Add("prop_c6_bed_bare", c);
    }

    static void Bags()
    {
        var c = new Canvas(44, 28);
        Rgba cb = H("C8A06A"), cbHi = H("DEB97F"), cbLo = H("9A7448"), cbDk = H("7A5A38"), tape = H("E8D9B5");
        // bottom box (left)
        Kit.Prism(c, 1, 12, 17, 5, 11, new[] { cbDk, cbLo, cb, cbHi, H("EACB92") }, 2);
        for (int j = 0; j < 5; j++) c.Set(8, 12 + j, tape); for (int j = 0; j < 5; j++) c.Set(9, 12 + j, H("CDBB94"));
        for (int j = 0; j < 11; j++) c.Set(8, 17 + j, H("DDCBA2"));
        c.Set(4, 21, K.Brown); c.Set(5, 21, K.Brown); c.Set(6, 21, K.Brown); c.Set(12, 23, K.Brown2);
        // box on top (smaller, askew)
        Kit.Prism(c, 3, 3, 12, 4, 8, new[] { cbDk, cbLo, cb, cbHi, H("EACB92") }, 3);
        for (int j = 0; j < 4; j++) c.Set(9, 3 + j, tape);
        c.Set(5, 12, K.Brown); c.Set(6, 12, K.Brown);
        // duffel bag (right): blue-grey canvas with strap
        Rgba bg = H("6B8FA8"), bgHi = H("8FB0C2"), bgLo = H("4F7189"), bgDk = H("3E5A6E");
        Kit.Ellipse(c, 30, 20, 13, 6.5, bg, bgHi, bgLo);
        for (int i = 0; i < 22; i++) c.Set(19 + i, 25, bgDk);
        // zipper and strap
        for (int i = 0; i < 22; i++) c.Set(19 + i, 17 + (i % 6 == 0 ? 0 : 0), i % 2 == 0 ? H("C9D2D6") : bgLo);
        for (int j = 0; j < 12; j++) { c.Set(27, 14 + j, K.Terra); c.Set(28, 14 + j, K.RedR[1]); }
        c.Set(27, 14, K.RedR[3]);
        // handles
        foreach (var (x, y) in new[] { (24, 13), (25, 12), (26, 12), (27, 12), (28, 12), (29, 12), (30, 13) }) c.Set(x, y, K.Brown2);
        // cloth bundle tied in a scarf, in front
        Kit.Ellipse(c, 12, 24, 5.5, 3.5, H("D4849A"), H("EAA5B5"), H("B5667A"));
        c.Set(12, 20, H("B5667A")); c.Set(11, 20, H("B5667A")); c.Set(13, 21, H("EAA5B5"));
        c.Set(9, 24, K.Cream); c.Set(15, 25, K.Cream);
        // plastic carrier bag, slumped against the box
        for (int j = 0; j < 7; j++) for (int i = 0; i < 6; i++)
            {
                if (j == 0 && i != 1 && i != 4) continue;
                c.Set(37 + i, 21 + j, (i + j) % 4 == 0 ? H("DCE6EA") : (i > 3 ? H("A9BEC6") : H("EEF4F4")));
            }
        S.Add("prop_c6_bags", Kit.Outlined(c).Trim().PadTo(46, 30));
    }

    static void BagsStowed()
    {
        // same footprint as the bags pile: everything now put away - a tidy stack of boxes with a folded blanket, empty bag flat, shoes
        var c = new Canvas(44, 28);
        Rgba cb = H("C8A06A"), cbHi = H("DEB97F"), cbLo = H("9A7448"), cbDk = H("7A5A38"), tape = H("E8D9B5");
        Kit.Prism(c, 1, 12, 17, 5, 11, new[] { cbDk, cbLo, cb, cbHi, H("EACB92") }, 2);
        for (int j = 0; j < 5; j++) c.Set(8, 12 + j, tape);
        for (int j = 0; j < 11; j++) c.Set(8, 17 + j, H("DDCBA2"));
        // folded blanket (teal, same as the bed quilt) on top
        for (int j = 0; j < 6; j++) for (int i = 0; i < 15; i++)
            {
                if ((i == 0 || i == 14) && (j == 0 || j == 5)) continue;
                var cc = j == 0 ? K.TealR[4] : j < 4 ? K.TealR[2] : K.TealR[1];
                if (j < 4 && (i + j) % 4 == 0) cc = K.TealR[3];
                c.Set(2 + i, 6 + j, cc);
            }
        c.Set(10, 7, K.Gold);
        // folded-flat duffel
        Kit.Prism(c, 22, 20, 18, 2, 5, new[] { H("3E5A6E"), H("4F7189"), H("6B8FA8"), H("8FB0C2") }, 2);
        for (int i = 0; i < 16; i++) c.Set(23 + i, 21, i % 2 == 0 ? H("C9D2D6") : H("4F7189"));
        // pair of shoes
        foreach (var sx in new[] { 24, 31 })
        {
            for (int j = 0; j < 3; j++) for (int i = 0; i < 6; i++) c.Set(sx + i, 15 + j, j == 0 ? K.Tan : j == 1 ? H("7A5F48") : K.Brown2);
        }
        // small plant that came out of a bag
        for (int j = 0; j < 4; j++) for (int i = 0; i < 5; i++) c.Set(33 + i, 23 + j - 0, j == 0 ? K.Terra : K.RedR[1]);
        c.Set(35, 22, K.Green); c.Set(34, 21, K.DGreen); c.Set(36, 21, K.Green); c.Set(35, 20, K.Leaf[4]);
        S.Add("prop_c6_bags_stowed", Kit.Outlined(c).Trim().PadTo(46, 30));
    }

    static Canvas PadTo(this Canvas c, int w, int h)
    {
        // anchor bottom-centre inside a canvas of the target size
        var o = new Canvas(w, h);
        o.Blit(c, (w - c.W) / 2, h - c.H);
        return o;
    }

    // ---- the open door with its lock and chain ------------------------------------------------------------------------
    static void DoorLock()
    {
        var c = new Canvas(28, 38);
        var W = K.Wood;
        // stiles and rails of a framed panel door
        for (int j = 0; j < 38; j++)
            for (int i = 0; i < 28; i++)
            {
                int idx = 3;
                if (i < 3) idx = 4; else if (i >= 25) idx = 1; else if (i == 3) idx = 2;
                if (j < 3) idx = 4; else if (j == 3) idx = 2;
                if (j >= 35) idx = 0;
                if (Fx.R(i / 2, j / 5, 321) > 0.9) idx = Math.Max(0, idx - 1);
                c.Set(i, j, W[idx]);
            }
        // raised panels
        foreach (var (py0, py1) in new[] { (7, 18), (21, 33) })
            for (int j = py0; j < py1; j++)
                for (int i = 6; i < 22; i++)
                {
                    int idx = 2;
                    if (j == py0 || i == 6) idx = 1; else if (j == py1 - 1 || i == 21) idx = 4; else if (j == py0 + 1 || i == 7) idx = 0 + 1;
                    if (j > py0 + 1 && j < py1 - 1 && i > 7 && i < 21) idx = 3 - (Fx.R(i, j, 322) > 0.93 ? 1 : 0);
                    c.Set(i, j, W[Math.Clamp(idx, 0, 5)]);
                }
        // iron hinges
        foreach (var hy in new[] { 5, 19, 32 }) { for (int i = 0; i < 6; i++) { c.Set(i, hy, K.Iron[3]); c.Set(i, hy + 1, K.Iron[1]); } c.Set(5, hy, K.Iron[4]); c.Set(1, hy, K.Iron[4]); }
        // hasp and staple on the free edge
        for (int j = 0; j < 4; j++) for (int i = 0; i < 6; i++) c.Set(22 + i, 12 + j, j == 0 ? K.Iron[4] : j == 3 ? K.Iron[0] : K.Iron[2]);
        c.Set(26, 14, K.Iron[4]);
        // door knob
        c.Set(23, 19, K.Gold); c.Set(24, 19, H("FBE9A8")); c.Set(23, 20, K.Orange); c.Set(24, 20, H("B8893A"));
        // chain looped from the hasp round the knob, with a padlock
        var link = K.Iron[4]; var linkLo = K.Iron[2];
        foreach (var (x, y) in new[] { (24, 16), (25, 17), (26, 18), (26, 19), (26, 20), (25, 21), (24, 22), (23, 22), (22, 21), (21, 20), (21, 19) }) c.Set(x, y, ((x + y) & 1) == 0 ? link : linkLo);
        // padlock
        for (int j = 0; j < 6; j++) for (int i = 0; i < 6; i++) c.Set(17 + i, 23 + j, j == 0 ? H("FBE9A8") : j == 5 ? H("8A5E2A") : i == 0 ? K.Gold : i == 5 ? H("B8893A") : K.Gold);
        c.Set(19, 26, K.Brown3); c.Set(20, 26, K.Brown3);
        c.Set(18, 22, K.Iron[4]); c.Set(22, 22, K.Iron[4]); for (int i = 18; i < 23; i++) c.Set(i, 21, K.Iron[4]);
        c.Set(21, 21, link); c.Set(22, 21, link);
        // small number plate and viewer
        c.Set(14, 4, K.Iron[1]);
        // shadow strip on the floor side
        var o = Kit.Outlined(c);
        S.Add("prop_c6_door_lock", o);
    }

    /// <summary>The children's school bag, leaning by the homework desk.</summary>
    static void SchoolBag()
    {
        var c = new Canvas(14, 14);
        var B = K.BlueR;
        for (int j = 2; j < 14; j++)
            for (int i = 1; i < 13; i++)
            {
                if ((j == 2 || j == 13) && (i == 1 || i == 12)) continue;
                int idx = i < 3 ? 3 : i > 9 ? 1 : 2;
                if (j >= 12) idx = Math.Max(0, idx - 1);
                c.Set(i, j, B[idx]);
            }
        for (int i = 1; i < 13; i++) { c.Set(i, 2, B[4]); c.Set(i, 8, B[0]); }       // flap line
        for (int j = 8; j < 12; j++) for (int i = 4; i < 10; i++) c.Set(i, j + 1, j == 8 ? B[3] : B[1]);
        c.Set(6, 9, K.Gold); c.Set(7, 9, K.Orange);                                // buckle
        for (int j = 0; j < 3; j++) { c.Set(5, j, K.Terra); c.Set(8, j, K.RedR[1]); }
        c.Set(6, 0, K.Terra); c.Set(7, 0, K.Terra);
        S.Add("prop_c6_schoolbag", Kit.Outlined(c));
    }

    static void KeyTable()
    {
        SchoolBag();
        var c = new Canvas(18, 20);
        // round side table
        Kit.Ellipse(c, 9, 7, 8.5, 5, K.Wood[3], K.Wood[5], K.Wood[2]);
        for (int i = 2; i < 16; i++) { c.Set(i, 11, K.Wood[1]); c.Set(i, 12, K.Wood[0]); }
        for (int j = 12; j < 20; j++) { c.Set(4, j, K.Wood[2]); c.Set(5, j, K.Wood[0]); c.Set(12, j, K.Wood[2]); c.Set(13, j, K.Wood[0]); }
        // key ring with two keys (gold), and a little dish
        Kit.Ellipse(c, 7, 7, 3.5, 2, K.Stone[3], K.Stone[5], K.Stone[2]);
        c.Set(6, 6, K.Gold); c.Set(7, 6, K.Gold); c.Set(8, 6, H("FBE9A8")); c.Set(9, 7, K.Gold); c.Set(10, 7, K.Gold); c.Set(11, 8, K.Gold); c.Set(5, 7, K.Orange);
        c.Set(6, 8, K.Gold); c.Set(7, 8, K.Orange);
        // folded cloth
        for (int i = 0; i < 4; i++) { c.Set(11 + i, 6, K.TealR[3]); c.Set(11 + i, 7, K.TealR[2]); }
        S.Add("prop_c6_keytable", Kit.Outlined(c));
    }

    static void HangLamp()
    {
        var c = new Canvas(20, 36);
        // cord
        for (int j = 0; j < 16; j++) { c.Set(10, j, K.Brown2); }
        for (int j = 0; j < 3; j++) { c.Set(9, j, K.Iron[3]); c.Set(10, j, K.Iron[4]); c.Set(11, j, K.Iron[3]); }
        // shade (cone) in warm enamel
        for (int j = 0; j < 10; j++)
        {
            int half = 2 + j * 7 / 10 + (j > 8 ? 1 : 0);
            for (int i = -half; i <= half; i++)
            {
                int idx = i < -half / 2 ? 3 : i > half / 2 ? 1 : 2;
                if (j == 0) idx = 4;
                if (j == 9) idx = 0;
                c.Set(10 + i, 16 + j, K.Mustard[Math.Clamp(idx, 0, 4)]);
            }
        }
        // warm glow under the shade
        for (int j = 0; j < 4; j++) for (int i = -6 + j; i <= 6 - j; i++) c.Set(10 + i, 26 + j, j < 2 ? E.WarmHi : E.Warm);
        S.Add("prop_c6_hanglamp", Kit.Outlined(c).Trim());
    }

    static void DryRack()
    {
        for (int f = 0; f < 2; f++)
        {
            var c = new Canvas(32, 32);
            Rgba bar = K.Pine[3], barLo = K.Pine[1];
            // A-frame ends + three cross bars
            foreach (var (x0, x1) in new[] { (3, 6), (26, 23) })
            {
                for (int j = 0; j < 22; j++) { int x = x0 + (x1 - x0) * j / 22; c.Set(x, 8 + j, bar); c.Set(x + 1, 8 + j, barLo); }
            }
            foreach (var by in new[] { 9, 14, 19 }) for (int i = 3; i < 29; i++) { c.Set(i, by, bar); c.Set(i, by + 1, barLo); }
            // cloths
            int sw = f; // sway
            void Cloth(int x, int y, int w, int h, Rgba[] ramp)
            {
                for (int j = 0; j < h; j++)
                    for (int i = 0; i < w; i++)
                    {
                        int xx = x + i + (j > h / 2 ? sw : 0), idx = i == 0 ? 3 : i == w - 1 ? 1 : 2;
                        if (j == 0) idx = 4;
                        c.Set(xx, y + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
                    }
            }
            Cloth(5, 10, 8, 10, K.Linen);
            Cloth(14, 10, 7, 8, K.BlueR);
            Cloth(22, 10, 5, 12, K.RedR);
            Cloth(7, 15, 6, 8, K.Mustard);
            Cloth(15, 15, 5, 5, K.TealR);
            // feet
            c.Set(3, 30, K.Brown2); c.Set(4, 30, K.Brown2); c.Set(26, 30, K.Brown2); c.Set(25, 30, K.Brown2);
            S.Add($"prop_c6_dryrack_{f}", Kit.Outlined(c).Trim().PadTo(34, 34));
        }
    }

    static void Banister()
    {
        var c = new Canvas(8, 44);
        for (int j = 0; j < 44; j++) { c.Set(3, j, j < 4 ? K.Wood[5] : K.Wood[4]); c.Set(4, j, K.Wood[2]); }
        // newel post on the near end, balusters
        for (int j = 22; j < 44; j++) for (int i = 1; i < 7; i++) c.Set(i, j, i == 1 ? K.Wood[4] : i == 6 ? K.Wood[0] : K.Wood[2]);
        for (int i = 0; i < 8; i++) { c.Set(i, 20, K.Wood[5]); c.Set(i, 21, K.Wood[3]); }
        for (int j = 4; j < 20; j += 1) if (j % 5 == 0) { c.Set(2, j, K.Wood[3]); c.Set(5, j, K.Wood[1]); }
        S.Add("prop_c6_banister", Kit.Outlined(c).Trim().PadTo(10, 46));
    }
}
