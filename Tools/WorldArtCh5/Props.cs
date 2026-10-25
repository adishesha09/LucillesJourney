namespace WorldArtCh5;

/// <summary>New props for the camp (everything that cannot be reused from Chapter 1).</summary>
public static class Props
{
    static Rgba Tn(Rgba[] r, int i) => r[Math.Clamp(i, 0, r.Length - 1)];

    public static void Build()
    {
        S.Cur = "tents";
        Rgba[][] P(params Rgba[][] p) => p;
        S.Add("prop_c5_tent_a", Tents.GableTent(44, 26, 14, 1, P(C.Ochre, C.Blue, C.Cream, C.Drab), 2, 3));
        S.Add("prop_c5_tent_b", Tents.GableTent(36, 22, 12, 2, P(C.Blue, C.Cream, C.Ochre, C.Grey), 1, -2));
        S.Add("prop_c5_tent_c", Tents.GableTent(56, 32, 16, 3, P(C.Drab, C.Clay, C.Ochre, C.Cream), 2, -3));
        S.Add("prop_c5_tent_d", Tents.GableTent(40, 24, 14, 5, P(C.Clay, C.Cream, C.Rust, C.Mustard), 2, 2));
        S.Add("prop_c5_tent_e", Tents.GableTent(48, 28, 14, 6, P(C.Teal, C.Grey, C.Ochre, C.Cream), 1, 0));
        S.Add("prop_c5_tent_f", Tents.GableTent(34, 20, 12, 7, P(C.Mustard, C.Plum, C.Cream, C.Rust), 2, -2));
        S.Add("prop_c5_tent_g", Tents.GableTent(50, 28, 16, 8, P(C.Grey, C.Blue, C.Ochre, C.Clay), 2, 3));
        S.Add("prop_c5_tent_h", Tents.GableTent(42, 24, 12, 9, P(C.Rust, C.Ochre, C.Drab, C.Cream), 1, 0));
        S.Add("prop_c5_tarp_a", Tents.Tarp(52, 22, 11, P(C.Blue, C.Cream, C.Ochre), true));
        S.Add("prop_c5_tarp_b", Tents.Tarp(40, 20, 12, P(C.Drab, C.Clay, C.Grey), true));
        S.Add("prop_c5_tarp_c", Tents.Tarp(44, 21, 13, P(C.Mustard, C.Teal, C.Rust), true));
        S.Add("prop_c5_tarp_d", Tents.Tarp(46, 22, 14, P(C.Plum, C.Cream, C.Teal), true));
        S.Add("prop_c5_shelter_bare", Tents.ShelterBare());
        S.Add("prop_c5_shelter_done", Tents.ShelterDone());

        S.Cur = "belongings";
        S.Add("prop_c5_handcart", Handcart());
        S.Add("prop_c5_bundles", Bundles());
        S.Add("prop_c5_suitcases", Suitcases());
        S.Add("prop_c5_jerrycans", Jerrycans());
        S.Add("prop_c5_jug", Jug());
        S.Add("prop_c5_jar", Jar());
        S.Add("prop_c5_bucket_empty", Bucket(false));
        S.Add("prop_c5_bucket_full", Bucket(true));
        S.Add("prop_c5_kindling", Kindling());
        S.Add("prop_c5_blankets", Blankets());
        S.Add("prop_c5_foodcloth", FoodCloth());
        S.Add("prop_c5_sign", Sign());
        S.Add("prop_c5_mat_a", Mat(30, 16, new[] { C.Clay, C.Cream, C.Mustard }, 1));
        S.Add("prop_c5_mat_b", Mat(24, 14, new[] { C.Teal, C.Ochre, C.Cream }, 2));
        S.Add("prop_c5_mat_c", Mat(22, 12, new[] { C.Grey, C.Plum, C.Cream }, 3));
        S.Add("prop_c5_mat_d", Mat(34, 18, new[] { C.Drab, C.Rust, C.Cream }, 4));

        S.Cur = "fire";
        S.Add("prop_c5_firering_s", FireRing(24, 12, 5, false));
        S.Add("prop_c5_firering_l", FireRing(40, 20, 9, true));
        S.Add("prop_c5_tripod_pot", Tripod());
        S.Add("prop_c5_log_seat", Log(26, 9, 1));
        S.Add("prop_c5_log_seat_s", Log(18, 8, 2));

        S.Cur = "fences";
        S.Add("prop_c5_fence_h", PoleFence(32));
        S.Add("prop_c5_fence_h2", PoleFence(24));
        S.Add("prop_c5_fence_post", PolePost());
        S.Add("prop_c5_windbreak", Windbreak(38, 30, new[] { C.Cream, C.Ochre, C.Blue }, 3));
        S.Add("prop_c5_windbreak_b", Windbreak(30, 26, new[] { C.Clay, C.Grey, C.Mustard }, 1));

        S.Cur = "nature";
        S.Add("prop_c5_scrub_a", Scrub(24, 15, 1));
        S.Add("prop_c5_scrub_b", Scrub(16, 11, 2));
        S.Add("prop_c5_scrub_c", Scrub(32, 18, 3));
        S.Add("prop_c5_drygrass_a", DryTuft(12, 10, 1));
        S.Add("prop_c5_drygrass_b", DryTuft(10, 8, 2));
        S.Add("prop_c5_thistle", Thistle());
        S.Add("prop_c5_boulder_a", Boulder(34, 22, 1));
        S.Add("prop_c5_boulder_b", Boulder(24, 16, 2));
        S.Add("prop_c5_boulder_c", Boulder(48, 30, 3));
        S.Add("prop_c5_boulder_d", Boulder(18, 12, 4));
        S.Add("prop_c5_pebbles", Pebbles());
        S.Add("prop_c5_olive_a", Tints.Dry(Tints.RemoveRed(S.Disk("prop_tree_broad")!), 0.45));
        S.Add("prop_c5_olive_b", Tints.Dry(Tints.RemoveRed(S.Disk("prop_tree_fruit")!), 0.55, 0.8));
        S.Add("prop_c5_cypress", Tints.Dry(S.Disk("prop_tree_cypress")!, 0.25, 0.4));
        S.Add("prop_c5_rock_a", Tints.Warm(Tints.Dry(S.Disk("prop_rock_a")!, 0.5)));
        S.Add("prop_c5_rock_b", Tints.Warm(Tints.Dry(S.Disk("prop_rock_b")!, 0.5)));
        S.Add("prop_c5_rock_c", Tints.Warm(Tints.Dry(S.Disk("prop_rock_c")!, 0.5)));
    }

    // ---- belongings ------------------------------------------------------------------------------------------------
    static Canvas Wheel(int r)
    {
        var c = new Canvas(r * 2 + 1, r * 2 + 1);
        c.Disc(r, r, r, K.Wood[1]);
        c.Disc(r, r, r - 1.5, K.Wood[3]);
        c.Disc(r, r, r - 2.6, new Rgba(0, 0, 0, 0));
        for (int a = 0; a < 6; a++)
        {
            double an = a * Math.PI / 3 + 0.3;
            c.Line(r, r, (int)Math.Round(r + Math.Cos(an) * (r - 1)), (int)Math.Round(r + Math.Sin(an) * (r - 1)), K.Wood[2]);
        }
        c.Disc(r, r, 1.5, K.Wood[0]);
        c.Set(r, r, K.Gold);
        return c;
    }

    static Canvas Handcart()
    {
        var c = new Canvas(44, 30);
        int yb = 29;
        // handles (left), resting on the ground
        c.Line(1, yb - 1, 12, yb - 13, K.Wood[2]); c.Line(1, yb, 12, yb - 12, K.Wood[1]);
        c.Line(4, yb - 1, 14, yb - 9, K.Wood[2]);
        // far wheel (hidden mostly) and bed
        c.Blit(Wheel(8), 26, yb - 17);
        // cart bed: planks
        Draw.Planks(c, 10, yb - 20, 30, 10, K.Wood, 41, 3);
        c.HL(10, yb - 20, 30, K.Wood[4]); c.HL(10, yb - 11, 30, K.Wood[0]);
        c.VL(10, yb - 20, 10, K.Wood[4]); c.VL(39, yb - 20, 10, K.Wood[0]);
        // load: sacks, bundle and rolled mat
        c.Disc(18, yb - 22, 5, C.Cream[2]); c.Disc(17, yb - 23, 3, C.Cream[3]); c.Set(18, yb - 27, C.RopeD);
        c.Rect(24, yb - 28, 10, 8, C.Clay[2]); c.HL(24, yb - 28, 10, C.Clay[3]); c.HL(24, yb - 21, 10, C.Clay[0]); c.VL(33, yb - 28, 8, C.Clay[1]);
        c.HL(24, yb - 25, 10, C.Rope);
        c.Rect(34, yb - 25, 6, 6, C.Rust[2]); c.HL(34, yb - 25, 6, C.Rust[3]); c.HL(34, yb - 20, 6, C.Rust[0]);
        // near wheel in front
        c.Blit(Wheel(8), 14, yb - 16);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Bundles()
    {
        var c = new Canvas(34, 26);
        int yb = 25;
        // rolled mat leaning at the back
        for (int y = yb - 22; y <= yb - 3; y++) { c.Set(5, y, C.Ochre[3]); c.Set(6, y, C.Ochre[2]); c.Set(7, y, C.Ochre[1]); c.Set(8, y, C.Ochre[0]); }
        c.HL(5, yb - 22, 4, C.Ochre[3]); c.Set(5, yb - 12, C.Rope); c.Set(8, yb - 12, C.RopeD); c.HL(5, yb - 8, 4, C.Rope);
        // suitcase
        c.Rect(10, yb - 11, 16, 11, K.Brown); c.HL(10, yb - 11, 16, K.Tan); c.VL(10, yb - 11, 11, K.Tan); c.HL(10, yb - 1, 16, K.Brown3); c.VL(25, yb - 11, 11, K.Brown2);
        c.HL(10, yb - 6, 16, K.Brown2); c.Rect(16, yb - 7, 4, 3, K.Gold); c.Set(17, yb - 6, K.Brown3);
        c.Set(10, yb - 11, K.Gold); c.Set(25, yb - 11, K.Gold); c.Set(10, yb - 1, K.Gold); c.Set(25, yb - 1, K.Gold);
        c.HL(15, yb - 13, 6, K.Brown3); c.Set(15, yb - 12, K.Brown3); c.Set(20, yb - 12, K.Brown3);
        // big cloth bundle with knot
        c.Disc(28, yb - 6, 6, C.Clay[2]);
        c.Paint((x, y) => x >= 22 && x < 34 && y >= yb - 12 && y < yb - 5 && (x + y) % 5 == 0, C.Clay[3]);
        c.Paint((x, y) => x >= 22 && x < 34 && y >= yb - 4 && y <= yb && ((x - 28) * (x - 28) + (y - (yb - 6)) * (y - (yb - 6))) <= 36, C.Clay[1]);
        c.Rect(26, yb - 13, 4, 3, C.Clay[3]); c.Set(27, yb - 12, C.Clay[0]); c.Set(28, yb - 14, C.Clay[2]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Suitcases()
    {
        var c = new Canvas(24, 18);
        int yb = 17;
        c.Rect(1, yb - 7, 20, 8, K.Brown2); c.HL(1, yb - 7, 20, K.Brown); c.VL(1, yb - 7, 8, K.Brown); c.HL(1, yb, 20, K.Brown3);
        c.Rect(9, yb - 6, 4, 3, K.Gold);
        c.Rect(4, yb - 13, 15, 6, C.Grey[1]); c.HL(4, yb - 13, 15, C.Grey[3]); c.VL(4, yb - 13, 6, C.Grey[2]); c.HL(4, yb - 8, 15, C.Grey[0]);
        c.VL(8, yb - 13, 6, C.RopeD); c.VL(15, yb - 13, 6, C.RopeD);
        c.Rect(19, yb - 4, 4, 5, C.Blue[1]); c.HL(19, yb - 4, 4, C.Blue[3]); c.HL(19, yb, 4, C.Blue[0]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Jerrycans()
    {
        var c = new Canvas(26, 18);
        int yb = 17;
        void Can(int x, int h, Rgba[] pal)
        {
            c.Rect(x, yb - h, 9, h + 1, pal[1]); c.VL(x, yb - h, h + 1, pal[3]); c.VL(x + 8, yb - h, h + 1, pal[0]); c.VL(x + 7, yb - h, h + 1, pal[0]);
            c.HL(x, yb - h, 9, pal[3]); c.HL(x, yb, 9, pal[0]);
            c.Rect(x + 1, yb - h - 3, 4, 3, pal[2]); c.Set(x + 2, yb - h - 2, C.Out);   // handle
            c.Rect(x + 6, yb - h - 2, 2, 2, pal[0]);                                       // cap
            c.HL(x + 1, yb - h / 2, 7, pal[0]);
        }
        Can(1, 11, C.Blue); Can(12, 9, C.Mustard);
        // small clay jug in front
        c.Disc(21, yb - 4, 4, C.Clay[2]); c.Rect(19, yb - 10, 5, 3, C.Clay[2]); c.VL(19, yb - 10, 3, C.Clay[3]); c.Set(18, yb - 8, C.Clay[1]);
        c.HL(19, yb - 10, 5, C.Clay[3]); c.Paint((x, y) => x >= 22 && y >= yb - 7 && ((x - 21) * (x - 21) + (y - (yb - 4)) * (y - (yb - 4))) <= 16, C.Clay[1]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Jug()
    {
        var c = new Canvas(12, 16);
        int yb = 15;
        c.Ellipse(6, yb - 5, 5, 5.5, C.Clay[2]);
        c.Paint((x, y) => x >= 7 && ((x - 6) * (x - 6) / 25.0 + (y - (yb - 5.0)) * (y - (yb - 5.0)) / 30.0) <= 1, C.Clay[1]);
        c.Paint((x, y) => x <= 4 && x >= 2 && y >= yb - 9 && y <= yb - 6 && Fx.R(x, y, 3) > 0.2, C.Clay[3]);
        c.Rect(4, 1, 5, 4, C.Clay[2]); c.VL(4, 1, 4, C.Clay[3]); c.VL(8, 1, 4, C.Clay[1]); c.HL(3, 1, 7, C.Clay[3]); c.HL(4, 4, 5, C.Clay[0]);
        c.Rect(5, 0, 3, 1, C.Clay[0]);
        c.Line(9, 3, 11, 6, C.Clay[1]); c.Line(11, 6, 10, 9, C.Clay[0]);   // handle
        c.HL(3, yb - 6, 7, C.Clay[0]); c.HL(4, yb, 5, C.Clay[0]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Jar()
    {
        var c = new Canvas(16, 18);
        int yb = 17;
        c.Ellipse(8, yb - 7, 6.5, 7.5, C.Rust[2]);
        c.Paint((x, y) => x >= 9 && ((x - 8) * (x - 8) / 40.0 + (y - (yb - 7.0)) * (y - (yb - 7.0)) / 56.0) <= 1, C.Rust[1]);
        c.Paint((x, y) => x <= 6 && x >= 3 && y >= yb - 12 && y <= yb - 7, C.Rust[3]);
        c.Rect(4, 1, 8, 3, C.Rust[2]); c.HL(4, 1, 8, C.Rust[3]); c.HL(4, 3, 8, C.Rust[0]);
        c.Rect(5, 0, 6, 1, C.Cream[2]);                           // cloth lid
        c.HL(3, yb - 9, 10, C.Mustard[1]); c.HL(3, yb - 8, 10, C.Mustard[2]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Bucket(bool full)
    {
        var c = new Canvas(14, 14);
        int yb = 13;
        c.Poly(new[] { (2.0, 5.0), (12.0, 5.0), (11.0, yb + 1.0), (3.0, yb + 1.0) }, C.Grey[1]);
        c.VL(3, 6, 8, C.Grey[3]); c.VL(4, 6, 7, C.Grey[2]); c.VL(11, 6, 8, C.Grey[0]);
        c.HL(2, 5, 10, C.Grey[3]); c.HL(3, yb, 8, C.Grey[0]); c.HL(3, 9, 9, C.Grey[0]);
        // opening
        if (full) { c.Rect(3, 6, 8, 2, C.Water[3]); c.HL(4, 6, 4, C.Water[5]); c.HL(3, 8, 8, C.Water[1]); c.Set(9, 7, C.Water[4]); }
        else { c.Rect(3, 6, 8, 2, C.Inside); c.HL(3, 8, 8, C.Grey[1]); }
        // handle
        c.Line(2, 5, 3, 1, C.Grey[0]); c.Line(3, 1, 10, 1, C.Grey[0]); c.Line(10, 1, 12, 5, C.Grey[0]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Kindling()
    {
        var c = new Canvas(20, 14);
        int yb = 13;
        var rng = new Random(7);
        for (int i = 0; i < 9; i++)
        {
            int x0 = 2 + i, len = 9 + rng.Next(3);
            c.Line(x0, yb, x0 + 5 + rng.Next(3) - 1, yb - len, i % 3 == 0 ? K.Wood[1] : i % 3 == 1 ? K.Wood[2] : K.Wood[3]);
        }
        c.HL(5, yb - 5, 8, C.Rope); c.HL(5, yb - 4, 8, C.RopeD);
        c.Rect(13, yb - 4, 6, 4, K.Wood[3]); c.HL(13, yb - 4, 6, K.Wood[4]); c.HL(13, yb, 6, K.Wood[0]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Blankets()
    {
        var c = new Canvas(22, 14);
        int yb = 13;
        void Layer(int y, int x, int w, Rgba[] p, bool stripe)
        {
            c.Rect(x, y, w, 4, p[2]); c.HL(x, y, w, p[3]); c.HL(x, y + 3, w, p[0]); c.VL(x, y, 4, p[3]);
            if (stripe) c.HL(x + 2, y + 1, w - 4, p[0]);
        }
        Layer(yb - 3, 1, 20, C.Clay, true); Layer(yb - 7, 2, 17, C.Cream, false); Layer(yb - 10, 3, 14, C.Blue, true);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas FoodCloth()
    {
        var c = new Canvas(28, 14);
        int yb = 13;
        c.Poly(new[] { (1.0, 3.0), (27.0, 3.0), (26.0, yb + 1.0), (2.0, yb + 1.0) }, C.Cream[2]);
        c.HL(1, 3, 26, C.Cream[3]); c.HL(2, yb, 24, C.Cream[0]);
        for (int x = 3; x < 26; x += 4) { c.Set(x, 5, C.Clay[1]); c.Set(x, yb - 2, C.Clay[1]); }
        // flat bread
        c.Ellipse(8, 8, 4.5, 2.5, C.Ochre[2]); c.Ellipse(8, 7.6, 3.4, 1.6, C.Ochre[3]); c.Set(6, 8, C.Ochre[1]); c.Set(10, 7, C.Ochre[1]);
        // bowls
        c.Ellipse(18, 7, 3.5, 2.2, C.Rust[1]); c.Ellipse(18, 6.8, 2.5, 1.2, C.Mustard[2]);
        c.Ellipse(22, 10, 3, 2, C.Teal[1]); c.Ellipse(22, 9.8, 2, 1, C.Cream[3]);
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Sign()
    {
        var c = new Canvas(18, 32);
        int yb = 31;
        for (int y = 8; y <= yb; y++) { c.Set(8, y, K.Wood[3]); c.Set(9, y, K.Wood[1]); }
        c.Rect(2, 2, 14, 9, C.Cream[2]); c.HL(2, 2, 14, C.Cream[3]); c.HL(2, 10, 14, C.Cream[0]); c.VL(15, 2, 9, C.Cream[1]);
        // non-readable squiggles
        for (int i = 0; i < 3; i++)
            for (int x = 4; x < 14; x++) if ((x + i) % 3 != 0) c.Set(x, 4 + i * 2 + ((x / 2) & 1), C.Rust[0]);
        c.Set(2, 2, C.RopeD); c.Set(15, 10, C.RopeD);
        c.Line(9, 11, 14, 14, C.Rope);
        c.Rect(13, 14, 3, 5, C.Clay[2]); c.Set(14, 19, C.Clay[1]);                // strip of cloth
        c.SelOutline(0.36);
        return c;
    }

    static Canvas Mat(int w, int h, Rgba[][] pal, int seed)
    {
        var c = new Canvas(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool edge = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                int m = (x + seed * 2) % 11;
                var ramp = m < 2 ? pal[1] : m == 5 ? pal[2] : pal[0];
                if (x < 3 || x >= w - 3) ramp = pal[1];
                int idx = 2;
                if (edge) idx = 0;
                else if (y == 1) idx = 3;
                else if ((x + y * 2) % 7 == 0) idx = 1;
                c.Set(x, y, Tn(ramp, idx));
            }
        for (int y = 1; y < h - 1; y += 2) { c.Set(0, y, C.Rope); c.Set(w - 1, y, C.Rope); }   // fringe
        c.Set(0, 0, Rgba.Clear); c.Set(w - 1, 0, Rgba.Clear); c.Set(0, h - 1, Rgba.Clear); c.Set(w - 1, h - 1, Rgba.Clear);
        c.SelOutline(0.4);
        return c;
    }

    // ---- fire --------------------------------------------------------------------------------------------------------
    static Canvas FireRing(int w, int h, int stones, bool big)
    {
        var c = new Canvas(w, h);
        double cx = (w - 1) / 2.0, cy = (h - 1) / 2.0, rx = w / 2.0 - 1.5, ry = h / 2.0 - 1.2;
        c.Ellipse(cx, cy, rx, ry, Rgba.Hex("2E2523"));
        c.Ellipse(cx, cy + 0.5, rx - 2, ry - 1.4, Rgba.Hex("4C3A33"));
        c.Ellipse(cx, cy + 0.5, rx - 4, ry - 2.5, Rgba.Hex("6A3E2C"));
        // embers
        for (int i = 0; i < (big ? 9 : 4); i++)
        {
            int ex = (int)(cx + (Fx.R(i, 1, 5) - 0.5) * (rx * 1.1)), ey = (int)(cy + (Fx.R(i, 2, 5) - 0.5) * (ry * 1.0));
            c.Set(ex, ey, i % 2 == 0 ? K.Orange : K.Terra);
        }
        // stones around the rim
        for (int i = 0; i < stones * 2; i++)
        {
            double a = i * Math.PI * 2 / (stones * 2);
            int sx = (int)Math.Round(cx + Math.Cos(a) * rx), sy = (int)Math.Round(cy + Math.Sin(a) * ry);
            int sw = big ? 4 : 3;
            var tone = 2 + (int)(Fx.R(i, 7, 9) * 2);
            c.Rect(sx - sw / 2, sy - 1, sw, 3, C.Rock[tone]);
            c.HL(sx - sw / 2, sy - 1, sw, C.Rock[Math.Min(5, tone + 1)]);
            c.HL(sx - sw / 2, sy + 1, sw, C.Rock[0]);
        }
        if (big)
        {
            // charred logs lying across
            c.Line(8, (int)cy + 1, w - 9, (int)cy - 1, K.Wood[0]); c.Line(8, (int)cy + 2, w - 9, (int)cy, K.Wood[1]);
            c.Line(12, (int)cy - 3, w - 13, (int)cy + 3, K.Wood[0]);
        }
        else c.Line(8, (int)cy + 1, w - 9, (int)cy, K.Wood[0]);
        // soot-stained outline: no outline pass, ground feel
        return c;
    }

    static Canvas Tripod()
    {
        var c = new Canvas(24, 30);
        int yb = 29;
        var w1 = K.Wood[2]; var w2 = K.Wood[1];
        c.Line(12, 2, 3, yb - 1, w1); c.Line(13, 2, 4, yb - 1, w2);
        c.Line(12, 2, 21, yb - 1, w2); c.Line(11, 2, 20, yb - 1, K.Wood[0]);
        c.Line(12, 2, 13, yb - 6, K.Wood[1]);
        c.Rect(10, 1, 4, 3, C.Rope); c.Set(11, 3, C.RopeD);
        // chain and pot
        for (int y = 4; y < 10; y++) c.Set(12, y, (y & 1) == 0 ? C.Grey[2] : C.Grey[0]);
        c.Ellipse(12, 15, 6.2, 5, Rgba.Hex("3A3438"));
        c.Paint((x, y) => x <= 10 && x >= 8 && y >= 12 && y <= 15, Rgba.Hex("6B6068"));
        c.Paint((x, y) => x >= 14 && ((x - 12) * (x - 12) / 38.0 + (y - 15.0) * (y - 15.0) / 25.0) <= 1, Rgba.Hex("262226"));
        c.Rect(6, 10, 13, 2, Rgba.Hex("4A4249")); c.HL(7, 10, 11, Rgba.Hex("857A83"));
        c.Rect(8, 9, 9, 1, Rgba.Hex("D9C9A0"));  // stew showing at the rim
        c.SelOutline(0.4);
        return c;
    }

    static Canvas Log(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 2; x < w; x++)
            {
                double t = y / (double)(h - 1);
                int idx = t < 0.25 ? 4 : t < 0.55 ? 3 : t < 0.8 ? 2 : 1;
                if (Fx.R(x / 2, y, seed) > 0.9 && idx > 1) idx--;
                c.Set(x, y, K.Wood[idx]);
            }
        c.Ellipse(2, h / 2.0, 2.4, h / 2.0, K.Wood[5]); c.Ellipse(2, h / 2.0, 1.2, h / 2.0 - 2, K.Wood[3]); c.Set(2, h / 2, K.Wood[1]);
        c.HL(2, h - 1, w - 2, K.Wood[0]);
        c.SelOutline(0.38);
        return c;
    }

    // ---- fences -----------------------------------------------------------------------------------------------------
    static void PoleAt(Canvas c, int x, int top, int bottom, int seed)
    {
        for (int y = top; y <= bottom; y++)
        {
            int dx = (y - top) * ((seed & 1) == 0 ? 1 : -1) / 9;
            c.Set(x + dx, y, K.Wood[3]); c.Set(x + dx + 1, y, K.Wood[1]);
        }
        c.Set(x + (bottom - top) / 9 * ((seed & 1) == 0 ? 1 : -1), bottom, K.Wood[0]);
    }

    static Canvas PoleFence(int w)
    {
        var c = new Canvas(w, 20);
        int yb = 19;
        var posts = w > 28 ? new[] { 2, 15, 28 } : new[] { 2, 12, 21 };
        int sd = 0;
        // rails first (behind posts)
        foreach (int ry in new[] { 6, 12 })
            for (int x = 0; x < w; x++)
            {
                int y = ry + (int)Math.Round(Math.Sin(x * 0.25 + ry) * 0.7);
                c.Set(x, y, K.Wood[4]); c.Set(x, y + 1, K.Wood[2]);
            }
        foreach (int px in posts) { PoleAt(c, px, 1 + (sd * 2 % 3), yb, sd); sd++; }
        foreach (int px in posts) foreach (int ry in new[] { 6, 12 }) { c.Set(px, ry, C.Rope); c.Set(px + 1, ry, C.RopeD); c.Set(px, ry + 1, C.RopeD); }
        c.SelOutline(0.38);
        return c;
    }

    static Canvas PolePost()
    {
        var c = new Canvas(8, 20);
        PoleAt(c, 2, 1, 19, 1);
        c.Rect(1, 6, 4, 2, C.Rope); c.HL(2, 7, 3, C.RopeD);
        c.SelOutline(0.38);
        return c;
    }

    static Canvas Windbreak(int w, int h, Rgba[][] pal, int seed)
    {
        var c = new Canvas(w, h);
        int yb = h - 1;
        // tarp between two poles
        for (int x = 3; x < w - 3; x++)
        {
            double t = (x - 3) / (double)(w - 7);
            int sag = (int)Math.Round(Math.Sin(t * Math.PI) * 2);
            for (int y = 3; y < 19 + sag - (x % 5 == 0 ? 1 : 0); y++)
            {
                int pi = ((x / 9) + (y / 7) + seed) % pal.Length;
                int idx = y < 5 ? 3 : y > 15 ? 1 : 2;
                if ((x + 1) % 9 == 0 || (y + 2) % 7 == 0) idx = 0;
                c.Set(x, y, Tn(pal[pi], idx));
            }
        }
        PoleAt(c, 1, 0, yb, 2); PoleAt(c, w - 4, 0, yb, 3);
        for (int y = 3; y < 18; y += 5) { c.Set(2, y, C.Rope); c.Set(w - 4, y, C.Rope); }
        c.SelOutline(0.38);
        return c;
    }

    // ---- nature -----------------------------------------------------------------------------------------------------
    static Canvas Scrub(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        Draw.Blob(c, w / 2.0, h * 0.58, w / 2.0 - 1, h * 0.42, C.Scrub, seed, -0.02, 3);
        // dry twigs and a few ochre leaves
        for (int i = 0; i < w / 3; i++)
        {
            int x = 2 + (int)(Fx.R(i, 1, seed) * (w - 4)), y = 2 + (int)(Fx.R(i, 2, seed) * (h - 5));
            if (c.Opaque(x, y)) c.Set(x, y, i % 3 == 0 ? C.Dry[4] : C.Scrub[5]);
        }
        c.SelOutline(0.4);
        // flat base
        for (int x = 0; x < c.W; x++) if (c.Opaque(x, h - 1)) c.Set(x, h - 1, C.ScrubDark[0]);
        return c;
    }

    static Canvas DryTuft(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        int n = 5 + w / 3;
        for (int i = 0; i < n; i++)
        {
            int x0 = 1 + (int)(i * (w - 3) / (double)n) + (int)(Fx.R(i, 1, seed) * 2);
            int len = h - 3 - (int)(Fx.R(i, 2, seed) * (h / 2.0));
            int lean = (int)Math.Round((Fx.R(i, 3, seed) - 0.5) * 6);
            var col = C.Dry[2 + (i % 4)];
            c.Line(x0, h - 1, x0 + lean, h - 1 - len, col);
            if (i % 3 == 0) c.Set(x0 + lean, h - 1 - len, C.Dry[5]);
        }
        for (int x = 1; x < w - 1; x++) c.Set(x, h - 1, C.Dry[0]);
        return c;
    }

    static Canvas Thistle()
    {
        var c = new Canvas(8, 12);
        var head = Rgba.Hex("9C8394"); var head2 = Rgba.Hex("B9A3AE");
        c.Line(4, 11, 4, 4, C.Dry[2]); c.Line(4, 8, 2, 5, C.Dry[1]); c.Line(4, 7, 6, 5, C.Dry[1]); c.Line(4, 11, 3, 9, C.Dry[1]);
        c.Disc(4, 3, 1.8, head); c.Set(4, 1, head2); c.Set(3, 2, head2);
        c.SelOutline(0.45);
        return c;
    }

    static Canvas Pebbles()
    {
        var c = new Canvas(16, 8);
        foreach (var (x, y, r) in new[] { (3, 5, 2.4), (9, 4, 1.8), (13, 6, 1.4), (7, 6, 1.2) })
        {
            c.Ellipse(x, y, r + 0.8, r, C.Rock[3]); c.Ellipse(x - 0.3, y - 0.4, r * 0.7, r * 0.6, C.Rock[4]);
        }
        c.SelOutline(0.4);
        return c;
    }

    static Canvas Boulder(int w, int h, int seed)
    {
        var c = new Canvas(w, h);
        double cx = w / 2.0, cy = h * 0.56, rx = w / 2.0 - 1, ry = h * 0.46;
        // jagged silhouette: radius modulated by angle
        c.Paint((x, y) =>
        {
            double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry;
            double a = Math.Atan2(ny, nx);
            double m = 1.0 - 0.16 * Math.Abs(Math.Sin(a * 2.5 + seed)) - 0.06 * Fx.R((int)(a * 9), 0, seed);
            double r = Math.Pow(Math.Abs(nx), 2.6) + Math.Pow(Math.Abs(ny), 2.6);
            return Math.Pow(r, 1 / 2.6) <= m && (ny < 0.95);
        }, C.Rock[2]);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!c.Opaque(x, y)) continue;
                double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry;
                double light = -(nx * 0.55 + ny * 0.85);
                double t = 0.5 + 0.42 * light + (Fx.R(x / 3, y / 2, seed + 4) - 0.5) * 0.28 + Fx.Bay(x, y) * 0.08;
                if (y > h - 3) t -= 0.25;
                c.Set(x, y, Tn(C.Rock, (int)(t * 6)));
            }
        // crack lines
        int k = w / 14;
        for (int i = 0; i < k; i++)
        {
            int sx = (int)(w * (0.25 + 0.5 * Fx.R(i, 3, seed))), sy = (int)(h * 0.25);
            for (int j = 0; j < h / 3; j++)
            {
                int px = sx + (int)Math.Round(Math.Sin(j * 0.9 + i) * 1.5), py = sy + j;
                if (c.Opaque(px, py)) c.Set(px, py, C.Rock[0]);
            }
        }
        c.SelOutline(0.4);
        return c;
    }
}
