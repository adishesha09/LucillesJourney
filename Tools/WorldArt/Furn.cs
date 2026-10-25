namespace WorldArt;

/// <summary>Shared small drawing helpers used by all prop files.</summary>
public static class U
{
    public static Rgba Lt(Rgba c, double k = 1.4) => c.Light(k);
    public static Rgba Dk(Rgba c, double k = 0.8) => c.Shade(k);

    public static void Jar(Canvas c, int x, int yb, int w, int h, Rgba body, Rgba lid, bool glassy = true)
    {
        int top = yb - h + 1;
        c.Rect(x, top + 2, w, h - 2, body);
        c.VL(x, top + 3, h - 3, Lt(body, 1.6));
        c.VL(x + w - 1, top + 3, h - 3, Dk(body, 0.8));
        c.HL(x, yb, w, Dk(body, 1.0));
        c.Rect(x + 1, top, w - 2, 2, lid);
        c.HL(x + 1, top, w - 2, Lt(lid, 1.2));
        if (glassy && h > 5) c.Set(x + 1, top + 3, Rgba.Hex("FFF6DA"));
    }

    public static void Cup(Canvas c, int x, int yb, Rgba col, bool handleRight = true)
    {
        c.Rect(x, yb - 3, 4, 4, col);
        c.HL(x, yb - 3, 4, Lt(col, 1.6));
        c.HL(x, yb, 4, Dk(col, 0.9));
        c.Set(x + 3, yb - 1, Dk(col, 0.6));
        if (handleRight) { c.Set(x + 4, yb - 2, col); c.Set(x + 4, yb - 1, Dk(col, 0.7)); }
        else { c.Set(x - 1, yb - 2, col); c.Set(x - 1, yb - 1, Dk(col, 0.7)); }
    }

    public static void Bowl(Canvas c, int x, int yb, int w, Rgba col, Rgba inner)
    {
        c.HL(x, yb - 2, w, inner);
        c.Rect(x, yb - 1, w, 1, col);
        c.Rect(x + 1, yb, w - 2, 1, Dk(col, 0.9));
        c.Set(x, yb - 2, Lt(col, 1.5)); c.Set(x + w - 1, yb - 2, Dk(col, 0.7));
        c.HL(x + 1, yb - 1, 1, Lt(col, 1.6));
    }

    public static void Plate(Canvas c, int x, int y, int w, int h, Rgba rim, Rgba inner)
    {
        c.Ellipse(x + w / 2.0, y + h / 2.0, w / 2.0, h / 2.0, rim);
        c.Ellipse(x + w / 2.0, y + h / 2.0 - 0.1, w / 2.0 - 1.3, h / 2.0 - 1.0, inner);
        c.Set(x + 1, y + h / 2 - 1, Lt(rim, 1.8));
    }

    public static void Apple(Canvas c, int x, int y, Rgba col)
    {
        c.Rect(x, y, 3, 3, col); c.Set(x, y, Rgba.Clear); c.Set(x + 2, y + 2, Dk(col, 0.9));
        c.Set(x + 1, y, Lt(col, 1.8)); c.Set(x + 1, y - 1, K.DGreen);
    }

    /// <summary>Spiky leaf blob made from a few strokes.</summary>
    public static void Leaf(Canvas c, int x, int y, int dx, int dy, Rgba mid, Rgba lite, Rgba dark)
    {
        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        for (int i = 0; i <= steps; i++)
        {
            int px = x + dx * i / Math.Max(1, steps), py = y + dy * i / Math.Max(1, steps);
            c.Set(px, py, mid);
            if (i > 0 && i < steps) { c.Set(px + (dx >= 0 ? 0 : 1), py - 1, lite); c.Set(px + (dx >= 0 ? 1 : 0), py + 1, dark); }
        }
    }

    public static Canvas Finish(Canvas c, double k = 0.36)
    {
        c.SelOutline(k);
        return c;
    }

    public static Rgba R(params string[] _) => default;
}

/// <summary>Interior furniture (kitchen, dining, living, bedroom).</summary>
public static class Furn
{
    // ---- 3/4 boxes ----------------------------------------------------------------------------------------
    static void Box(Canvas c, int x, int y, int w, int topH, int frontH, Rgba[] r, int topIdx, int frontIdx)
    {
        c.Rect(x, y, w, topH, r[topIdx]);
        c.HL(x, y, w, r[Math.Min(r.Length - 1, topIdx + 1)]); c.VL(x, y, topH, r[Math.Min(r.Length - 1, topIdx + 1)]);
        c.Rect(x, y + topH, w, frontH, r[frontIdx]);
        c.HL(x, y + topH, w, r[Math.Min(r.Length - 1, frontIdx + 1)]);
        c.VL(x, y + topH, frontH, r[Math.Min(r.Length - 1, frontIdx + 1)]);
        c.VL(x + w - 1, y + topH, frontH, r[Math.Max(0, frontIdx - 1)]);
        c.HL(x, y + topH + frontH - 1, w, r[Math.Max(0, frontIdx - 2)]);
    }

    static void DoorPanel(Canvas c, int x, int y, int w, int h, Rgba[] r, int idx, bool knobRight, Rgba knob)
    {
        c.Rect(x, y, w, h, r[idx]);
        c.HL(x, y, w, r[Math.Min(r.Length - 1, idx + 1)]); c.VL(x, y, h, r[Math.Min(r.Length - 1, idx + 1)]);
        c.HL(x, y + h - 1, w, r[Math.Max(0, idx - 2)]); c.VL(x + w - 1, y, h, r[Math.Max(0, idx - 2)]);
        // inset panel
        c.Rect(x + 3, y + 3, w - 6, h - 6, r[Math.Max(0, idx - 1)]);
        c.HL(x + 3, y + 3, w - 6, r[Math.Max(0, idx - 2)]); c.VL(x + 3, y + 3, h - 6, r[Math.Max(0, idx - 2)]);
        c.HL(x + 3, y + h - 4, w - 6, r[Math.Min(r.Length - 1, idx + 1)]); c.VL(x + w - 4, y + 3, h - 6, r[Math.Min(r.Length - 1, idx + 1)]);
        int kx = knobRight ? x + w - 3 : x + 2;
        c.Rect(kx, y + h / 2 - 1, 2, 2, knob); c.Set(kx, y + h / 2 - 1, Rgba.Hex("FBE9A8"));
    }

    // ================================================ KITCHEN =================================================
    public static void Build()
    {
        S.Cur = "house_kitchen";
        S.Add("prop_counter_sink", CounterSink());
        S.Add("prop_stove", Stove());
        S.Add("prop_kitchen_shelf", Hutch());
        S.Add("prop_herbs", Herbs());
        S.Add("prop_table_full", Table(true));
        S.Add("prop_table_empty", Table(false));
        var cl = ChairSide();
        S.Add("prop_chair_left", cl); S.Add("prop_chair_right", cl.FlipH());
        S.Add("prop_chair_back", ChairBack());
        S.Add("prop_chair_front", ChairFront());
        S.Add("prop_stool", Stool());
        S.Add("prop_basket_logs", LogBasket());
        S.Add("prop_broom", Broom());
        S.Add("prop_veg_crate", VegCrate());
        S.Cur = "house_living";
        S.Add("prop_sofa", Sofa());
        S.Add("prop_armchair", Armchair());
        S.Add("prop_sideboard_photo", Sideboard());
        S.Add("prop_bookcase", Bookcase());
        S.Add("prop_floorlamp", FloorLamp());
        S.Add("prop_plant_pot", PlantPot());
        S.Add("prop_plant_small", PlantSmall());
        S.Add("prop_coffee_table", CoffeeTable());
        S.Add("prop_rug_living", Rug(72, 40, K.Terra, K.Teal));
        S.Add("prop_rug_bedroom", RugOval(44, 26));
        S.Add("prop_rug_kitchen", Runner(44, 14));
        S.Add("prop_rug_dining", Rug(76, 46, K.Mustard[1], K.Terra));
        S.Add("prop_doormat", Doormat());
        S.Cur = "house_bedroom";
        S.Add("prop_bed_double", BedDouble());
        S.Add("prop_bed_child", BedChild());
        S.Add("prop_wardrobe", Wardrobe());
        S.Add("prop_nightstand", Nightstand());
    }

    static Canvas CounterSink()
    {
        var c = new Canvas(48, 30);
        var pine = K.Pine;
        // worktop (seen from above)
        c.Rect(0, 3, 48, 10, pine[3]);
        c.HL(0, 3, 48, pine[4]); c.VL(0, 3, 10, pine[4]);
        c.HL(0, 7, 48, pine[2]); c.HL(0, 10, 48, pine[2]);
        c.HL(0, 12, 48, pine[1]); c.VL(47, 3, 10, pine[2]);
        for (int x = 3; x < 48; x += 9) { c.Set(x, 5, pine[2]); c.Set(x + 2, 8, pine[4]); }
        // sink basin
        c.Rect(7, 4, 17, 8, K.Stone[4]);
        c.Rect(9, 5, 13, 6, K.Stone[2]);
        c.Rect(9, 5, 13, 1, K.Stone[1]); c.Rect(9, 5, 1, 6, K.Stone[1]);
        c.Rect(10, 9, 11, 2, K.Stone[3]);
        c.HL(11, 8, 5, K.WaterHi); c.HL(17, 9, 3, K.Water);
        c.HL(7, 3, 17, K.Stone[5]);
        // tap
        c.Rect(15, 0, 2, 4, K.Iron[3]); c.HL(12, 0, 4, K.Iron[3]); c.Set(12, 1, K.Iron[2]); c.Set(15, 0, K.Iron[4]);
        c.Set(18, 1, K.Terra); c.Set(18, 2, K.Teal);
        // cabinets
        var t = K.TealR;
        c.Rect(0, 13, 48, 15, t[2]);
        c.HL(0, 13, 48, t[4]);
        DoorPanel(c, 1, 14, 22, 14, t, 2, true, K.Mustard[3]);
        DoorPanel(c, 25, 14, 22, 14, t, 2, false, K.Mustard[3]);
        c.Rect(0, 28, 48, 2, K.Dark[1]);
        c.HL(0, 28, 48, K.Dark[2]);
        // hanging towel on left door
        c.Rect(8, 14, 6, 9, K.Linen[3]);
        c.HL(8, 17, 6, K.Terra); c.HL(8, 20, 6, K.Terra);
        c.VL(13, 14, 9, K.Linen[1]); c.HL(8, 22, 6, K.Linen[1]);
        c.HL(7, 14, 8, K.Dark[3]);
        // fruit bowl on the counter
        U.Bowl(c, 32, 12, 10, K.Blue, K.BlueR[0]);
        U.Apple(c, 33, 7, K.Terra); U.Apple(c, 36, 8, K.Gold); U.Apple(c, 38, 7, K.Terra);
        U.Apple(c, 36, 5, K.Green);
        // cutting board
        c.Rect(43, 5, 4, 7, K.Wood[4]); c.VL(43, 5, 7, K.Wood[5]); c.HL(43, 11, 4, K.Wood[2]);
        return U.Finish(c);
    }

    static Canvas Stove()
    {
        var c = new Canvas(24, 38);
        var ir = K.Iron;
        // pipe
        c.Rect(15, 0, 5, 10, ir[2]); c.VL(15, 0, 10, ir[3]); c.VL(19, 0, 10, ir[1]);
        c.Rect(14, 4, 7, 2, ir[3]); c.HL(14, 5, 7, ir[1]);
        // top plate
        c.Rect(1, 10, 22, 8, ir[2]);
        c.HL(1, 10, 22, ir[4]); c.VL(1, 10, 8, ir[3]); c.HL(1, 17, 22, ir[1]);
        c.Ellipse(8, 14, 3.5, 2, ir[1]); c.Ellipse(8, 13.6, 3.0, 1.6, ir[2]);
        // pot on the stove
        c.Rect(3, 4, 11, 8, K.TealR[2]);
        c.Rect(3, 4, 11, 1, K.TealR[4]); c.VL(3, 5, 7, K.TealR[3]); c.VL(13, 5, 7, K.TealR[0]);
        c.HL(4, 3, 9, K.TealR[3]); c.HL(4, 2, 9, K.TealR[1]);
        c.Rect(7, 0, 3, 2, K.Iron[3]); c.Set(7, 0, K.Iron[4]);
        c.HL(4, 11, 9, K.TealR[0]);
        c.Set(2, 6, K.Iron[3]); c.Set(1, 6, K.Iron[3]); c.Set(14, 6, K.Iron[3]); c.Set(15, 6, K.Iron[3]);
        // body
        c.Rect(2, 18, 20, 17, ir[1]);
        c.VL(2, 18, 17, ir[3]); c.VL(21, 18, 17, ir[0]); c.HL(2, 18, 20, ir[3]);
        // fire door with glowing window
        c.Rect(5, 21, 14, 11, ir[2]);
        c.HL(5, 21, 14, ir[4]); c.VL(5, 21, 11, ir[3]); c.HL(5, 31, 14, ir[0]); c.VL(18, 21, 11, ir[0]);
        c.Rect(8, 23, 8, 5, Rgba.Hex("5A2A22"));
        c.Rect(9, 24, 6, 3, K.Orange); c.Rect(10, 25, 4, 2, K.Gold); c.Set(12, 25, Rgba.Hex("FBE9A8")); c.Set(9, 26, K.Terra); c.Set(14, 24, K.Terra);
        c.Rect(7, 29, 10, 1, ir[0]);
        c.Rect(17, 25, 1, 3, K.Mustard[3]);           // handle
        // feet and ash drawer
        c.Rect(3, 35, 3, 3, ir[0]); c.Rect(18, 35, 3, 3, ir[0]); c.HL(2, 34, 20, ir[0]);
        return U.Finish(c, 0.45);
    }

    static Canvas Hutch()
    {
        var c = new Canvas(44, 44);
        var w = K.Wood;
        // carcass
        c.Rect(0, 0, 44, 44, w[2]);
        c.Rect(0, 0, 44, 3, w[3]); c.HL(0, 0, 44, w[5]); c.HL(0, 3, 44, w[1]);
        c.VL(0, 0, 44, w[4]); c.VL(43, 0, 44, w[1]); c.VL(1, 3, 28, w[3]); c.VL(42, 3, 28, w[1]);
        // back wall of the open shelves
        c.Rect(2, 4, 40, 27, w[0]);
        c.Rect(2, 4, 40, 27, K.Dark[1]);
        for (int x = 2; x < 42; x += 5) c.VL(x, 4, 27, K.Dark[2]);   // vertical back boards
        // shelves at y=12, 21, 30
        foreach (int sy in new[] { 12, 21, 30 })
        {
            c.Rect(1, sy, 42, 2, w[4]); c.HL(1, sy + 2, 42, w[0]); c.HL(1, sy, 42, w[5]);
        }
        // row A (y 4..11): bowls stack, jars, cups
        U.Bowl(c, 3, 11, 8, K.Cream, K.Parch); U.Bowl(c, 3, 10, 8, K.Blue, K.BlueR[3]); U.Bowl(c, 4, 9, 6, K.Cream, K.Parch);
        U.Jar(c, 14, 11, 5, 8, Rgba.Hex("D9A441"), K.Brown);
        U.Jar(c, 20, 11, 5, 6, Rgba.Hex("B85A5A"), K.Cream);
        U.Cup(c, 27, 11, K.Terra); U.Cup(c, 33, 11, K.Teal); U.Cup(c, 38, 11, K.Gold, false);
        // row B (y 13..20): tall jars, plates standing
        U.Jar(c, 3, 20, 5, 7, Rgba.Hex("8FB070"), K.Brown2);
        U.Jar(c, 9, 20, 5, 8, Rgba.Hex("C9852F"), K.Parch);
        U.Jar(c, 15, 20, 6, 6, Rgba.Hex("D2B48C"), K.Terra);
        for (int i = 0; i < 3; i++)
        {
            int px = 25 + i * 5;
            c.Rect(px, 14, 4, 7, K.Cream); c.VL(px, 14, 7, K.Parch); c.VL(px + 3, 14, 7, K.Sand);
            c.Rect(px + 1, 15, 2, 5, K.BlueR[3]); c.Set(px + 1, 15, K.BlueR[4]);
        }
        // row C (y 22..29): teapot, mugs on hooks, bread
        c.Ellipse(8, 26, 4.2, 3.2, K.TealR[2]); c.HL(5, 23, 7, K.TealR[3]); c.Rect(7, 21, 3, 2, K.TealR[1]);
        c.Set(13, 25, K.TealR[2]); c.Set(14, 24, K.TealR[2]); c.Set(3, 25, K.TealR[2]); c.Set(3, 26, K.TealR[1]);
        c.Rect(5, 24, 2, 1, K.TealR[4]);
        U.Cup(c, 17, 29, K.Gold); U.Cup(c, 23, 29, K.Terra);
        c.Ellipse(33, 26.5, 6, 3, K.Orange); c.HL(29, 25, 8, K.Gold); c.Set(31, 24, K.Gold); c.Set(34, 24, K.Gold);
        c.HL(28, 28, 10, Rgba.Hex("A96A33")); c.Set(32, 26, Rgba.Hex("A96A33")); c.Set(35, 27, Rgba.Hex("A96A33"));
        c.Rect(28, 29, 11, 1, K.Linen[2]);                          // cloth under the loaf
        // lower cabinet
        c.Rect(1, 32, 42, 10, w[2]);
        DoorPanel(c, 2, 32, 19, 10, w, 2, true, K.Mustard[3]);
        DoorPanel(c, 23, 32, 19, 10, w, 2, false, K.Mustard[3]);
        c.Rect(1, 42, 42, 2, w[0]); c.HL(1, 42, 42, w[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas Herbs()
    {
        var c = new Canvas(10, 22);
        c.Rect(4, 0, 2, 2, K.Iron[2]);                   // nail
        c.VL(5, 1, 4, K.Tan);
        c.Rect(3, 5, 4, 2, K.Terra);                     // twine tie
        // bunch
        var g = new[] { K.Leaf[2], K.Leaf[3], K.Leaf[4], K.Leaf[5] };
        for (int i = 0; i < 4; i++)
        {
            int x0 = 1 + i * 2;
            for (int y = 7; y < 20 - (i % 2) * 3; y++)
            {
                int wob = (y / 3 + i) % 3 == 0 ? 1 : 0;
                c.Set(x0 + wob - (y > 12 ? 1 : 0) * (i < 2 ? 1 : 0), y, g[(i + y / 2) % 4]);
                if ((y + i) % 4 == 0) c.Set(x0 + 1 + wob, y, g[(i + 1) % 4]);
            }
        }
        c.Set(2, 19, K.Leaf[4]); c.Set(7, 18, K.Leaf[3]);
        c.Set(4, 21, K.Purple[2]); c.Set(5, 20, K.Purple[1]); c.Set(3, 19, K.Purple[2]);   // sprigs of lavender
        return U.Finish(c, 0.42);
    }

    // ---- dining -------------------------------------------------------------------------------------------
    static Canvas Table(bool full)
    {
        var c = new Canvas(48, 44);
        var w = K.Wood;
        int top = 10;          // top surface y 10..34 (24 deep), apron 34..40, legs to 44
        // legs (behind the apron)
        foreach (int lx in new[] { 1, 44 })
        {
            c.Rect(lx, 36, 3, 8, w[2]); c.VL(lx, 36, 8, w[4]); c.VL(lx + 2, 36, 8, w[0]);
            c.Rect(lx, 43, 3, 1, w[0]);
        }
        // apron/front
        c.Rect(0, 34, 48, 6, w[2]);
        c.HL(0, 34, 48, w[1]); c.HL(0, 35, 48, w[3]); c.HL(0, 39, 48, w[0]);
        c.VL(0, 34, 6, w[4]); c.VL(47, 34, 6, w[1]);
        // top surface with plank seams (long grain runs left-right)
        c.Rect(0, top, 48, 24, w[4]);
        for (int j = 0; j < 24; j++)
        {
            int row = j / 6;
            int idx = 4;
            if (j % 6 == 0) idx = 3; // seam
            else if (j % 6 == 1) idx = 5;
            if (idx != 4) c.HL(0, top + j, 48, w[idx]);
            // grain dashes
            if (j % 6 == 3)
                for (int x = 0; x < 48; x++) if (Fx.R(x / 6, row, 311) > 0.55 && x % 6 < 4) c.Set(x, top + j, w[3]);
        }
        c.VL(0, top, 24, w[5]); c.HL(0, top, 48, w[5]);
        c.VL(47, top, 24, w[2]); c.HL(0, top + 23, 48, w[2]);
        // linen runner down the middle
        c.Rect(4, top + 9, 40, 6, K.Linen[3]);
        c.HL(4, top + 9, 40, K.Linen[4]); c.HL(4, top + 14, 40, K.Linen[1]);
        for (int x = 5; x < 43; x += 4) c.Set(x, top + 12, K.Terra);
        c.VL(4, top + 9, 6, K.Linen[2]); c.VL(43, top + 9, 6, K.Linen[2]);

        // place settings: 4 plates (left, right, back, front), cups beside them
        U.Plate(c, 6, top + 8, 10, 8, K.Cream, K.Parch);          // left
        U.Plate(c, 32, top + 8, 10, 8, K.Cream, K.Parch);         // right
        U.Plate(c, 19, top + 2, 10, 7, K.Cream, K.Parch);         // back
        U.Plate(c, 19, top + 15, 10, 7, K.Cream, K.Parch);        // front
        // blue rim on plates
        foreach (var (px, py, pw, ph) in new[] { (6, top + 8, 10, 8), (32, top + 8, 10, 8), (19, top + 2, 10, 7), (19, top + 15, 10, 7) })
            for (int i = 2; i < pw - 2; i++) { if (i % 2 == 0) { c.Set(px + i, py, K.BlueR[3]); c.Set(px + i, py + ph - 1, K.BlueR[2]); } }
        // cups
        U.Cup(c, 3, top + 7, K.Teal); U.Cup(c, 41, top + 7, K.Terra, false);
        U.Cup(c, 31, top + 6, K.Gold); U.Cup(c, 14, top + 20, K.Blue);
        if (full)
        {
            // bread on plates
            foreach (var (bx, by) in new[] { (8, top + 10), (34, top + 10), (21, top + 4), (21, top + 17) })
            {
                c.Rect(bx, by, 6, 3, K.Orange); c.HL(bx + 1, by, 4, K.Gold); c.HL(bx, by + 2, 6, Rgba.Hex("A96A33")); c.Set(bx, by, Rgba.Clear); c.Set(bx + 5, by, Rgba.Clear);
                c.Set(bx + 2, by + 1, Rgba.Hex("A96A33"));
            }
            // cup contents
            c.Set(4, top + 4, Rgba.Hex("7A4A30")); c.Set(42, top + 4, Rgba.Hex("7A4A30"));
            // bread board and loaf in the middle-right
            c.Rect(30, top + 17, 11, 5, w[3]); c.HL(30, top + 17, 11, w[5]); c.HL(30, top + 21, 11, w[1]);
            c.Ellipse(35.5, top + 18.3, 4.6, 2.2, K.Orange); c.HL(33, top + 17, 5, K.Gold);
            c.Set(34, top + 18, Rgba.Hex("A96A33")); c.Set(37, top + 18, Rgba.Hex("A96A33"));
            // jam pot
            U.Jar(c, 7, top + 22, 5, 5, Rgba.Hex("B03A48"), K.Cream, false);
            // teapot (centre)
            c.Ellipse(12.5, top + 12, 3.5, 3.0, K.TealR[2]); c.HL(10, top + 9, 5, K.TealR[3]); c.Set(9, top + 12, K.TealR[3]);
            c.Rect(11, top + 8, 3, 1, K.TealR[1]);
            // spout and handle
            c.Set(16, top + 11, K.TealR[2]); c.Set(17, top + 10, K.TealR[3]);
            c.Set(8, top + 11, K.TealR[1]); c.Set(8, top + 12, K.TealR[0]);
            // steam wisps
            var st = new[] { K.Linen[4], K.Linen[3] };
            foreach (var (sx, sy) in new[] { (12, top + 6), (13, top + 4), (12, top + 2), (13, top), (12, top - 2) }) { c.Set(sx, sy, st[0]); c.Set(sx + 1, sy, st[1]); }
            foreach (var (sx, sy) in new[] { (3, top + 5), (4, top + 3), (3, top + 1) }) c.Set(sx, sy, st[1]);
            foreach (var (sx, sy) in new[] { (42, top + 5), (41, top + 3), (42, top + 1) }) c.Set(sx, sy, st[1]);
        }
        else
        {
            // crumbs
            foreach (var (bx, by) in new[] { (9, top + 11), (12, top + 13), (35, top + 11), (37, top + 13), (22, top + 5), (25, top + 6), (23, top + 18), (26, top + 19) }) c.Set(bx, by, K.Tan);
            c.Set(10, top + 10, K.Sand); c.Set(34, top + 12, K.Sand); c.Set(24, top + 17, K.Sand);
            // used cup rings
            c.Set(4, top + 4, K.Parch); c.Set(42, top + 4, K.Parch);
            // empty jam jar / teapot with lid ajar
            U.Jar(c, 7, top + 22, 5, 5, Rgba.Hex("C9B58F"), K.Cream, false);
            c.Ellipse(12.5, top + 12, 3.5, 3.0, K.TealR[2]); c.HL(10, top + 9, 5, K.TealR[3]); c.Set(9, top + 12, K.TealR[3]);
            c.Rect(11, top + 8, 3, 1, K.TealR[1]);
            c.Set(16, top + 11, K.TealR[2]); c.Set(17, top + 10, K.TealR[3]);
            c.Set(8, top + 11, K.TealR[1]); c.Set(8, top + 12, K.TealR[0]);
            // folded napkin
            c.Rect(31, top + 17, 8, 4, K.Linen[3]); c.HL(31, top + 17, 8, K.Linen[4]); c.HL(31, top + 20, 8, K.Linen[1]); c.Set(35, top + 18, K.Terra);
        }
        // outline only where table edge touches transparency; keep the steam clean
        var steamMask = c.Clone();
        U.Finish(c, 0.36);
        return c;
    }

    static Canvas ChairSide()
    {
        var c = new Canvas(16, 20);
        var w = K.Wood;
        // tall back (left) with slats
        c.Rect(1, 0, 3, 18, w[2]); c.VL(1, 0, 18, w[4]); c.VL(3, 0, 18, w[1]);
        c.Rect(1, 0, 3, 1, w[5]);
        c.Rect(4, 2, 1, 2, w[3]); c.Rect(4, 6, 1, 2, w[3]);
        // seat with cushion
        c.Rect(3, 10, 12, 3, w[3]); c.HL(3, 10, 12, w[5]); c.HL(3, 12, 12, w[1]);
        c.Rect(4, 8, 10, 3, K.BlueR[2]); c.HL(4, 8, 10, K.BlueR[3]); c.HL(5, 10, 9, K.BlueR[1]); c.Set(13, 9, K.BlueR[1]);
        c.Set(8, 9, K.Cream); c.Set(10, 9, K.Cream);
        // legs
        c.Rect(3, 13, 2, 7, w[2]); c.VL(3, 13, 7, w[4]);
        c.Rect(12, 13, 2, 7, w[2]); c.VL(12, 13, 7, w[4]); c.VL(13, 13, 7, w[0]);
        c.HL(4, 14, 9, w[1]);
        return U.Finish(c);
    }

    static Canvas ChairBack()
    {
        var c = new Canvas(16, 22);
        var w = K.Wood;
        c.Rect(2, 0, 12, 11, w[2]);
        c.Rect(2, 0, 12, 1, w[5]); c.VL(2, 0, 11, w[4]); c.VL(13, 0, 11, w[1]);
        c.Rect(4, 2, 8, 2, w[3]); c.Rect(4, 6, 8, 2, w[3]);
        c.VL(4, 2, 6, w[4]); c.VL(11, 2, 6, w[1]);
        c.Rect(2, 11, 12, 2, w[4]);
        c.Rect(1, 13, 14, 3, K.BlueR[2]); c.HL(1, 13, 14, K.BlueR[3]); c.HL(2, 15, 12, K.BlueR[1]);
        c.Set(5, 14, K.Cream); c.Set(9, 14, K.Cream);
        c.Rect(2, 16, 2, 6, w[2]); c.VL(2, 16, 6, w[4]); c.Rect(12, 16, 2, 6, w[2]); c.VL(13, 16, 6, w[1]);
        c.HL(3, 17, 10, w[1]);
        return U.Finish(c);
    }

    static Canvas ChairFront()
    {
        var c = new Canvas(16, 20);
        var w = K.Wood;
        c.Rect(2, 0, 12, 12, w[2]);
        c.Rect(2, 0, 12, 1, w[5]); c.VL(2, 0, 12, w[4]); c.VL(13, 0, 12, w[1]);
        c.Rect(4, 2, 8, 2, w[3]); c.Rect(4, 6, 8, 2, w[3]);
        c.VL(4, 2, 6, w[4]); c.VL(11, 2, 6, w[1]);
        c.HL(2, 11, 12, w[0]);
        c.Rect(2, 12, 12, 2, w[3]); c.HL(2, 12, 12, w[4]);
        c.Rect(2, 14, 2, 6, w[2]); c.VL(2, 14, 6, w[4]); c.Rect(12, 14, 2, 6, w[2]); c.VL(13, 14, 6, w[1]);
        return U.Finish(c);
    }

    static Canvas Stool()
    {
        var c = new Canvas(12, 12);
        var w = K.Wood;
        c.Ellipse(6, 3.5, 5.5, 3, w[4]); c.Ellipse(6, 3.2, 4.6, 2.2, w[5]);
        c.HL(2, 4, 8, w[3]);
        c.Rect(2, 6, 2, 6, w[2]); c.VL(2, 6, 6, w[4]); c.Rect(8, 6, 2, 6, w[2]); c.VL(9, 6, 6, w[0]); c.Rect(5, 6, 2, 5, w[1]);
        c.HL(3, 9, 6, w[1]);
        return U.Finish(c);
    }

    static Canvas LogBasket()
    {
        var c = new Canvas(16, 14);
        var w = K.Wood;
        // logs sticking out
        foreach (var (lx, ly) in new[] { (2, 2), (6, 0), (10, 2), (4, 4), (8, 4) })
        {
            c.Disc(lx + 2.5, ly + 2.5, 2.6, w[3]); c.Disc(lx + 2.5, ly + 2.5, 1.5, w[5]); c.Set(lx + 2, ly + 2, w[4]);
            c.Set(lx + 4, ly + 4, w[0]);
        }
        // woven basket
        c.Rect(1, 6, 14, 8, K.Tan);
        for (int j = 0; j < 8; j++) for (int i = 0; i < 14; i++) c.Set(1 + i, 6 + j, ((i + j) & 1) == 0 ? K.Tan : K.Sand);
        c.HL(1, 6, 14, K.Parch); c.HL(1, 13, 14, K.Brown); c.VL(14, 6, 8, K.Brown);
        c.Set(1, 6, Rgba.Clear); c.Set(14, 13, Rgba.Clear); c.Set(1, 13, Rgba.Clear);
        return U.Finish(c);
    }

    static Canvas Broom()
    {
        var c = new Canvas(8, 30);
        c.VL(5, 0, 20, K.Wood[3]); c.VL(6, 0, 20, K.Wood[1]); c.Set(5, 0, K.Wood[5]);
        c.Rect(3, 18, 5, 3, K.Terra); c.HL(3, 18, 5, K.RedR[3]);
        for (int i = 0; i < 6; i++)
        {
            int x = 2 + i; int len = 7 + (i % 2) * 2;
            c.VL(x, 21, len, i % 2 == 0 ? K.Mustard[2] : K.Mustard[1]);
        }
        c.Set(2, 28, K.Mustard[0]); c.Set(7, 28, K.Mustard[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas VegCrate()
    {
        var c = new Canvas(18, 16);
        var w = K.Wood;
        // produce piled on top
        c.Disc(4.5, 5, 3.0, K.Orange); c.Set(3, 3, K.Gold); c.Set(4, 1, K.Green); c.Set(5, 1, K.Leaf[4]); c.Set(4, 2, K.Leaf[3]);
        c.Disc(9.5, 4, 2.8, K.Terra); c.Set(8, 2, K.RedR[4]); c.Set(9, 0, K.Leaf[3]); c.Set(10, 0, K.Leaf[4]);
        c.Disc(14, 5.5, 3.2, K.Leaf[4]); c.Set(12, 3, K.Leaf[5]); c.Set(15, 7, K.Leaf[2]); c.HL(13, 4, 3, K.Leaf[5]);
        c.Rect(7, 6, 5, 2, Rgba.Hex("D8B060"));
        // crate
        c.Rect(0, 6, 18, 10, w[3]);
        c.HL(0, 6, 18, w[5]); c.VL(0, 6, 10, w[4]); c.VL(17, 6, 10, w[1]);
        c.HL(0, 10, 18, w[1]); c.HL(0, 15, 18, w[0]);
        c.VL(5, 7, 8, w[2]); c.VL(12, 7, 8, w[2]);
        return U.Finish(c);
    }

    // ================================================ LIVING =================================================
    static Canvas Sofa()
    {
        var c = new Canvas(52, 28);
        var f = K.TealR;
        // backrest with three cushions
        c.Rect(4, 0, 44, 12, f[1]);
        c.HL(4, 0, 44, f[3]); c.HL(5, 1, 42, f[2]);
        for (int i = 0; i < 3; i++)
        {
            int x = 6 + i * 14;
            c.Rect(x, 2, 13, 10, f[2]); c.HL(x, 2, 13, f[3]); c.VL(x, 2, 10, f[3]); c.VL(x + 12, 3, 9, f[1]); c.HL(x + 1, 11, 12, f[1]);
            c.Set(x + 6, 6, f[3]); c.Set(x + 7, 6, f[1]);   // tuft button
        }
        // arms
        foreach (int ax in new[] { 0, 44 })
        {
            c.Rect(ax, 6, 8, 18, f[2]);
            c.Rect(ax, 6, 8, 3, f[3]); c.HL(ax, 6, 8, f[4]);
            c.VL(ax, 9, 15, f[3]); c.VL(ax + 7, 9, 15, f[1]);
            c.HL(ax, 23, 8, f[0]);
        }
        // seat cushions
        c.Rect(8, 13, 36, 10, f[3]);
        for (int i = 0; i < 2; i++)
        {
            int x = 8 + i * 18;
            c.Rect(x, 13, 18, 10, f[2]); c.HL(x, 13, 18, f[4]); c.VL(x, 14, 9, f[3]); c.VL(x + 17, 14, 9, f[1]); c.HL(x, 22, 18, f[0]);
        }
        c.HL(8, 24, 36, f[0]);
        // throw pillow + folded blanket
        c.Rect(9, 7, 8, 8, K.Gold); c.HL(9, 7, 8, K.Mustard[4]); c.VL(9, 7, 8, K.Mustard[4]); c.VL(16, 8, 7, K.Mustard[1]); c.HL(9, 14, 8, K.Mustard[1]);
        c.Set(10, 7, Rgba.Clear); c.Set(16, 14, Rgba.Clear);
        c.Set(12, 10, K.Terra); c.Set(13, 11, K.Terra); c.Set(14, 10, K.Terra); c.Set(13, 9, K.Terra);
        c.Rect(36, 14, 8, 8, K.Terra); c.HL(36, 14, 8, K.RedR[3]);
        for (int j = 15; j < 22; j += 2) c.HL(36, j, 8, K.RedR[1]);
        // feet
        foreach (int fx in new[] { 2, 46 }) c.Rect(fx, 24, 3, 4, K.Wood[1]);
        c.Rect(8, 24, 36, 1, f[0]);
        return U.Finish(c);
    }

    static Canvas Armchair()
    {
        var c = new Canvas(26, 28);
        var f = K.RedR;
        c.Rect(3, 0, 20, 14, f[1]);
        c.HL(3, 0, 20, f[3]); c.Rect(5, 2, 16, 11, f[2]); c.HL(5, 2, 16, f[3]); c.VL(5, 2, 11, f[3]); c.VL(20, 3, 10, f[1]);
        c.Set(12, 6, f[1]); c.Set(13, 6, f[3]); c.Set(12, 8, f[3]); c.Set(13, 8, f[1]);
        foreach (int ax in new[] { 0, 20 })
        {
            c.Rect(ax, 8, 6, 17, f[2]); c.Rect(ax, 8, 6, 3, f[3]); c.HL(ax, 8, 6, f[4]);
            c.VL(ax, 11, 14, f[3]); c.VL(ax + 5, 11, 14, f[1]); c.HL(ax, 24, 6, f[0]);
        }
        c.Rect(6, 15, 14, 9, f[2]); c.HL(6, 15, 14, f[4]); c.VL(6, 16, 8, f[3]); c.VL(19, 16, 8, f[1]); c.HL(6, 23, 14, f[0]);
        c.Rect(7, 18, 12, 1, f[1]);
        foreach (int fx in new[] { 1, 22 }) c.Rect(fx, 25, 3, 3, K.Wood[1]);
        c.Rect(6, 24, 14, 1, f[0]);
        // crocheted blanket over one arm
        c.Rect(0, 8, 6, 5, K.Cream); c.HL(0, 9, 6, K.Parch); c.HL(0, 11, 6, K.Sand);
        return U.Finish(c);
    }

    static Canvas Sideboard()
    {
        var c = new Canvas(44, 42);
        var w = K.Wood;
        // framed family photograph (centre-left)
        int fx = 11, fy = 0;
        c.Rect(fx, fy, 18, 17, K.Gold);
        c.HL(fx, fy, 18, K.Mustard[4]); c.VL(fx, fy, 17, K.Mustard[4]);
        c.HL(fx, fy + 16, 18, K.Mustard[0]); c.VL(fx + 17, fy, 17, K.Mustard[0]);
        c.Rect(fx + 2, fy + 2, 14, 13, K.Brown2);
        // photograph: warm sepia with four little figures
        c.Rect(fx + 3, fy + 3, 12, 11, Rgba.Hex("E3CFA2"));
        c.Rect(fx + 3, fy + 3, 12, 4, Rgba.Hex("EEDDB8"));
        c.Rect(fx + 3, fy + 11, 12, 3, Rgba.Hex("C9AD7C"));
        var skin = Rgba.Hex("D9A57A");
        // father (blue-grey), mother (terracotta), two children
        c.Rect(fx + 5, fy + 5, 2, 2, skin); c.Rect(fx + 4, fy + 7, 4, 6, K.BlueR[1]); c.HL(fx + 4, fy + 7, 4, K.BlueR[2]);
        c.Rect(fx + 10, fy + 5, 2, 2, skin); c.Rect(fx + 9, fy + 7, 4, 6, K.Terra); c.HL(fx + 9, fy + 7, 4, K.RedR[3]);
        c.HL(fx + 10, fy + 4, 2, K.Brown2);
        c.Rect(fx + 8, fy + 9, 1, 1, skin); c.Rect(fx + 8, fy + 10, 1, 3, K.Mustard[2]);
        c.Rect(fx + 13, fy + 9, 1, 1, skin); c.Rect(fx + 13, fy + 10, 1, 3, K.TealR[2]);
        c.Set(fx + 8, fy + 8, K.Brown2); c.Set(fx + 13, fy + 8, K.Brown2);
        c.Set(fx + 3, fy + 3, K.Cream); c.Set(fx + 4, fy + 3, Rgba.Hex("F7EBCB"));
        // sideboard: top surface, body, doors
        c.Rect(0, 18, 44, 6, w[4]);
        c.HL(0, 18, 44, w[5]); c.VL(0, 18, 6, w[5]); c.HL(0, 23, 44, w[2]); c.VL(43, 18, 6, w[2]);
        for (int x = 4; x < 44; x += 7) c.Set(x, 20, w[3]);
        // top decor: vase with flowers and candle
        c.Rect(33, 11, 4, 7, K.BlueR[2]); c.VL(33, 12, 6, K.BlueR[3]); c.VL(36, 12, 6, K.BlueR[0]); c.HL(33, 17, 4, K.BlueR[0]);
        c.Set(34, 10, K.BlueR[3]); c.Set(36, 10, K.BlueR[1]);
        c.Set(33, 7, Rgba.Hex("EAA5B5")); c.Set(34, 8, K.Leaf[3]); c.Set(35, 6, K.Gold); c.Set(36, 8, K.Leaf[4]); c.Set(37, 7, K.Cream);
        c.Set(35, 9, K.Leaf[2]); c.Set(35, 7, K.Leaf[3]); c.Set(34, 6, Rgba.Hex("EAA5B5")); c.Set(36, 6, K.Cream);
        c.Rect(4, 13, 3, 5, K.Cream); c.VL(4, 13, 5, K.Parch); c.Set(5, 12, K.Orange); c.Set(5, 11, K.Gold);   // candle
        c.HL(2, 17, 7, K.Linen[3]); c.HL(2, 17, 1, K.Linen[4]);
        // doors
        c.Rect(0, 24, 44, 15, w[2]);
        c.HL(0, 24, 44, w[1]); c.VL(0, 24, 15, w[4]); c.VL(43, 24, 15, w[0]);
        DoorPanel(c, 2, 25, 18, 13, w, 2, true, K.Mustard[3]);
        DoorPanel(c, 24, 25, 18, 13, w, 2, false, K.Mustard[3]);
        c.Rect(20, 25, 4, 13, w[1]);
        c.Rect(1, 39, 42, 1, w[0]);
        c.Rect(2, 40, 4, 2, w[0]); c.Rect(38, 40, 4, 2, w[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas Bookcase()
    {
        var c = new Canvas(32, 42);
        var w = K.Wood;
        c.Rect(0, 0, 32, 42, w[2]);
        c.HL(0, 0, 32, w[5]); c.VL(0, 0, 42, w[4]); c.VL(31, 0, 42, w[0]);
        c.Rect(2, 2, 28, 29, K.Dark[1]);
        var bk = new[] { K.Terra, K.Teal, K.Blue, K.Gold, K.Cream, K.Brown, K.Orange, K.Green, K.RedR[1], K.BlueR[1], K.Parch };
        int[] rows = { 2, 12, 22 };
        for (int r = 0; r < 3; r++)
        {
            int top = rows[r], bot = top + 8;           // bot = shelf board row
            c.Rect(1, bot + 1, 30, 2, w[4]); c.HL(1, bot + 3, 30, w[0]); c.HL(1, bot + 1, 30, w[5]);
            int x = 3;
            int seed = 400 + r;
            while (x < 28)
            {
                uint h = Fx.H(x, r, seed);
                int bw = 2 + (int)(h % 2), bh = 5 + (int)((h >> 3) % 4);
                var col = bk[(int)((h >> 6) % (uint)bk.Length)];
                if (r == 2 && x > 14)
                {   // lower-right: a basket and a box instead of books
                    c.Rect(16, bot - 5, 7, 6, K.Tan); c.HL(16, bot - 5, 7, K.Parch); for (int j = 0; j < 5; j++) for (int i = 0; i < 7; i++) if (((i + j) & 1) == 1) c.Set(16 + i, bot - 4 + j, K.Sand);
                    c.Rect(24, bot - 4, 5, 5, K.TealR[2]); c.HL(24, bot - 4, 5, K.TealR[4]); c.VL(28, bot - 4, 5, K.TealR[0]);
                    break;
                }
                if (r == 1 && x == 15)
                {   // small framed portrait / figurine in the middle row
                    c.Rect(16, bot - 6, 6, 7, K.Gold); c.Rect(17, bot - 5, 4, 5, K.Cream); c.Rect(18, bot - 3, 2, 3, K.Terra); c.Set(18, bot - 4, Rgba.Hex("D9A57A")); c.Set(19, bot - 4, Rgba.Hex("D9A57A"));
                    x = 23; continue;
                }
                c.Rect(x, bot - bh + 1, bw, bh, col);
                c.VL(x, bot - bh + 1, bh, U.Lt(col, 1.5)); c.VL(x + bw - 1, bot - bh + 1, bh, U.Dk(col, 0.8));
                c.HL(x, bot - bh + 1, bw, U.Lt(col, 1.2));
                if (bh > 5) c.HL(x, bot - bh + 3, bw, U.Dk(col, 0.4));
                x += bw;
                if (h % 7 == 0) x += 1;
            }
        }
        // one leaning book + plant on top shelf end handled by row logic; base cabinet
        c.Rect(1, 31, 30, 2, w[1]);
        c.Rect(1, 33, 30, 8, w[2]);
        DoorPanel(c, 2, 33, 13, 8, w, 2, true, K.Mustard[3]);
        DoorPanel(c, 17, 33, 13, 8, w, 2, false, K.Mustard[3]);
        c.Rect(1, 41, 30, 1, w[0]);
        return U.Finish(c, 0.4);
    }

    static Canvas FloorLamp()
    {
        var c = new Canvas(12, 36);
        // shade (trapezoid) with warm light
        c.Poly(new[] { (2.0, 0.0), (10.0, 0.0), (11.5, 11.0), (0.5, 11.0) }, K.Mustard[3]);
        c.Poly(new[] { (2.5, 1.0), (9.5, 1.0), (10.5, 10.0), (1.5, 10.0) }, Rgba.Hex("FBE9A8"));
        c.Rect(3, 2, 3, 7, Rgba.Hex("FFF6DA"));
        c.HL(1, 10, 10, K.Mustard[1]); c.HL(2, 0, 8, K.Mustard[4]);
        // pole and base
        c.Rect(5, 11, 2, 21, K.Wood[2]); c.VL(5, 11, 21, K.Wood[4]);
        c.Ellipse(6, 33, 5, 2.4, K.Wood[2]); c.Ellipse(5.5, 32.6, 3.6, 1.5, K.Wood[4]);
        c.Rect(5, 31, 2, 1, K.Wood[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas PlantPot()
    {
        var c = new Canvas(18, 30);
        // leaves: arching fronds
        var l = K.Leaf;
        void Frond(int x, int y, int dx, int dy, int len)
        {
            for (int i = 0; i < len; i++)
            {
                int px = x + dx * i / 2, py = y + dy * i / 3 + (i * i) / (len * 2);
                c.Set(px, py, l[3]); c.Set(px, py - 1, l[4]); c.Set(px + 1, py, l[2]);
            }
        }
        c.Ellipse(9, 9, 4.2, 6, l[3]);
        c.Ellipse(5.5, 12, 4.4, 3.2, l[2]); c.Ellipse(12.5, 12, 4.4, 3.2, l[2]);
        c.Ellipse(6, 7, 3.6, 5, l[3]); c.Ellipse(12, 6.5, 3.4, 5, l[3]);
        c.Ellipse(9, 5, 3.2, 4.6, l[4]); c.Ellipse(8, 4, 1.8, 3, l[5]);
        c.Ellipse(4, 13, 3, 1.8, l[1]); c.Ellipse(14, 13, 3, 1.8, l[1]);
        // leaf veins
        c.VL(9, 3, 9, l[2]); c.Line(6, 5, 7, 9, l[2]); c.Line(12, 5, 11, 9, l[2]);
        c.Set(5, 12, l[1]); c.Set(13, 12, l[1]);
        c.Set(7, 3, l[5]); c.Set(11, 4, l[5]); c.Set(4, 10, l[4]);
        // terracotta pot
        c.Rect(3, 16, 12, 3, K.Roof[3]); c.HL(3, 16, 12, K.Roof[4]); c.HL(3, 18, 12, K.Roof[1]);
        c.Rect(4, 19, 10, 9, K.Roof[2]); c.VL(4, 19, 9, K.Roof[3]); c.VL(13, 19, 9, K.Roof[1]); c.VL(12, 19, 9, K.Roof[1]);
        c.HL(4, 27, 10, K.Roof[0]);
        c.Rect(7, 21, 4, 1, K.Roof[1]); c.Set(9, 23, K.Roof[1]);
        c.HL(5, 28, 8, K.Roof[0]);
        return U.Finish(c, 0.42);
    }

    static Canvas PlantSmall()
    {
        var c = new Canvas(12, 18);
        var l = K.Leaf;
        c.Ellipse(6, 6, 4, 5, l[3]); c.Ellipse(4, 7, 3, 3.5, l[2]); c.Ellipse(8.5, 6, 3, 4, l[4]);
        c.Set(5, 3, l[5]); c.Set(8, 4, l[5]); c.VL(6, 4, 7, l[2]);
        c.Set(3, 5, Rgba.Hex("EAA5B5")); c.Set(9, 3, Rgba.Hex("EAA5B5"));
        c.Rect(2, 11, 8, 2, K.Blue); c.HL(2, 11, 8, K.BlueR[4]);
        c.Rect(3, 13, 6, 4, K.BlueR[2]); c.VL(3, 13, 4, K.BlueR[3]); c.VL(8, 13, 4, K.BlueR[0]); c.HL(3, 17, 6, K.BlueR[0]);
        return U.Finish(c, 0.42);
    }

    static Canvas CoffeeTable()
    {
        var c = new Canvas(28, 20);
        var w = K.Wood;
        c.Rect(1, 0, 26, 12, w[4]);
        c.HL(1, 0, 26, w[5]); c.VL(1, 0, 12, w[5]); c.HL(1, 11, 26, w[2]); c.VL(26, 0, 12, w[2]);
        c.HL(2, 4, 24, w[3]); c.HL(2, 8, 24, w[3]);
        c.Rect(0, 12, 28, 3, w[2]); c.HL(0, 12, 28, w[3]); c.HL(0, 14, 28, w[0]);
        c.Rect(1, 15, 3, 5, w[2]); c.VL(1, 15, 5, w[4]); c.Rect(24, 15, 3, 5, w[2]); c.VL(26, 15, 5, w[0]);
        // tea tray: cups and a small book
        c.Rect(5, 3, 8, 5, K.Linen[3]); c.HL(5, 3, 8, K.Linen[4]); c.Rect(6, 4, 6, 3, K.BlueR[2]); c.Set(7, 4, K.BlueR[3]);
        U.Cup(c, 15, 7, K.Terra); U.Cup(c, 20, 6, K.Teal);
        c.Rect(7, 3, 4, 1, K.Terra);
        return U.Finish(c);
    }

    // ---- rugs / mats (flat, drawn without outline, with a darker edge) ------------------------------------
    static Canvas Rug(int w, int h, Rgba border, Rgba field)
    {
        var c = new Canvas(w, h);
        c.Rect(2, 0, w - 4, h, K.RedR[2]);
        // fringe on the short sides
        for (int j = 1; j < h - 1; j += 2) { c.Set(0, j, K.Cream); c.Set(1, j, K.Parch); c.Set(w - 1, j, K.Cream); c.Set(w - 2, j, K.Parch); }
        // border bands
        c.Rect(2, 0, w - 4, h, border);
        c.HL(2, 0, w - 4, K.RedR[3]); c.HL(2, h - 1, w - 4, K.RedR[0]);
        c.Rect(4, 2, w - 8, h - 4, K.Cream);
        c.Rect(5, 3, w - 10, h - 6, K.TealR[2]);
        c.HL(5, 3, w - 10, K.TealR[3]); c.HL(5, h - 4, w - 10, K.TealR[1]);
        // diamond motif row
        int cy = h / 2;
        for (int x = 9; x < w - 9; x += 8)
        {
            c.Set(x, cy, K.Gold); c.Set(x - 1, cy, K.Cream); c.Set(x + 1, cy, K.Cream);
            c.Set(x, cy - 1, K.Cream); c.Set(x, cy + 1, K.Cream);
            c.Set(x, cy - 2, K.Terra); c.Set(x, cy + 2, K.Terra); c.Set(x - 2, cy, K.Terra); c.Set(x + 2, cy, K.Terra);
        }
        for (int x = 7; x < w - 7; x += 2) { c.Set(x, 3, K.Cream); c.Set(x + 1, h - 4, K.Cream); }
        // mid shade right/bottom edge (soft)
        c.VL(w - 3, 1, h - 2, K.RedR[1]);
        return c;
    }

    static Canvas RugOval(int w, int h)
    {
        var c = new Canvas(w, h);
        c.Ellipse(w / 2.0, h / 2.0, w / 2.0, h / 2.0, K.BlueR[1]);
        c.Ellipse(w / 2.0, h / 2.0, w / 2.0 - 1.5, h / 2.0 - 1.2, K.BlueR[3]);
        c.Ellipse(w / 2.0, h / 2.0, w / 2.0 - 4, h / 2.0 - 3, K.Cream);
        c.Ellipse(w / 2.0, h / 2.0, w / 2.0 - 6, h / 2.0 - 4.4, K.Gold);
        c.Ellipse(w / 2.0, h / 2.0, w / 2.0 - 9, h / 2.0 - 6.6, K.Terra);
        c.Ellipse(w / 2.0, h / 2.0 + 0.2, 3, 2, K.Cream);
        for (int x = 8; x < w - 8; x += 6) c.Set(x, h / 2, K.Cream);
        var ring = c.Clone();
        c.Paint((x, y) => ring.Get(x, y) == K.BlueR[3] && x + y > (w + h) * 0.62, K.BlueR[2]);
        c.Paint((x, y) => ring.Get(x, y) == K.BlueR[1] && x + y > (w + h) * 0.62, K.BlueR[0]);
        return c;
    }

    static Canvas Runner(int w, int h)
    {
        var c = new Canvas(w, h);
        c.Rect(2, 0, w - 4, h, K.Mustard[3]);
        for (int j = 1; j < h - 1; j += 2) { c.Set(0, j, K.Cream); c.Set(1, j, K.Parch); c.Set(w - 1, j, K.Cream); c.Set(w - 2, j, K.Parch); }
        for (int x = 2; x < w - 2; x++)
        {
            int k = ((x - 2) / 4) % 2;
            for (int j = 0; j < h; j++)
            {
                Rgba col = j == 0 || j == h - 1 ? K.Mustard[1] : (j == 2 || j == h - 3) ? K.Cream : (k == 0 ? K.Mustard[3] : K.Mustard[2]);
                if (j > 2 && j < h - 3 && (x - 2) % 4 == 1 && j % 3 == 1) col = K.Terra;
                c.Set(x, j, col);
            }
        }
        c.HL(2, 0, w - 4, K.Mustard[1]); c.HL(2, h - 1, w - 4, K.Mustard[0]);
        return c;
    }

    static Canvas Doormat()
    {
        var c = new Canvas(24, 10);
        c.Rect(0, 0, 24, 10, K.Brown);
        for (int j = 0; j < 10; j++) for (int i = 0; i < 24; i++)
            {
                bool edge = i < 2 || i > 21 || j < 2 || j > 7;
                var a = ((i / 2 + j) & 1) == 0 ? K.Tan : Rgba.Hex("8F7552");
                if (edge) a = ((i + j / 2) & 1) == 0 ? K.Brown : K.Brown2;
                c.Set(i, j, a);
            }
        c.HL(0, 0, 24, K.Brown); c.HL(0, 9, 24, K.Brown2);
        // woven stripe
        for (int i = 5; i < 19; i += 2) c.Set(i, 4, K.Terra);
        for (int i = 6; i < 18; i += 2) c.Set(i, 5, K.Terra);
        c.Set(0, 0, Rgba.Clear); c.Set(23, 0, Rgba.Clear); c.Set(0, 9, Rgba.Clear); c.Set(23, 9, Rgba.Clear);
        return c;
    }

    // ================================================ BEDROOM ================================================
    static Canvas BedDouble()
    {
        var c = new Canvas(38, 52);
        var w = K.Wood;
        // headboard (we see its front face rising above the mattress)
        c.Rect(0, 0, 38, 14, w[2]);
        c.HL(0, 0, 38, w[5]); c.VL(0, 0, 14, w[4]); c.VL(37, 0, 14, w[0]);
        c.Rect(3, 3, 32, 9, w[3]); c.HL(3, 3, 32, w[1]); c.VL(3, 3, 9, w[1]); c.HL(3, 11, 32, w[4]); c.VL(34, 3, 9, w[4]);
        for (int x = 7; x < 33; x += 6) c.VL(x, 4, 7, w[2]);
        c.Rect(0, 0, 4, 14, w[3]); c.HL(0, 0, 4, w[5]); c.Rect(34, 0, 4, 14, w[3]); c.HL(34, 0, 4, w[5]);
        c.VL(0, 0, 14, w[5]); c.VL(37, 0, 14, w[0]);
        // mattress + sheets
        c.Rect(1, 14, 36, 30, K.Linen[3]);
        c.HL(1, 14, 36, K.Linen[2]);
        // pillows
        foreach (int px in new[] { 4, 20 })
        {
            c.Rect(px, 15, 14, 8, K.Cream); c.HL(px, 15, 14, K.Linen[4]); c.VL(px, 15, 8, K.Linen[4]); c.HL(px, 22, 14, K.Linen[1]); c.VL(px + 13, 16, 7, K.Linen[1]);
            c.Set(px, 15, Rgba.Clear); c.Set(px + 13, 15, Rgba.Clear);
            c.HL(px + 3, 18, 8, K.Linen[2]);
        }
        // folded sheet edge then quilt with patchwork pattern
        c.Rect(1, 24, 36, 3, K.Linen[4]); c.HL(1, 26, 36, K.Linen[1]);
        for (int j = 27; j < 43; j++)
            for (int i = 1; i < 37; i++)
            {
                int bx = (i - 1) / 6, by = (j - 27) / 5;
                bool alt = ((bx + by) & 1) == 0;
                Rgba col = alt ? K.Teal : K.TealR[3];
                if ((i - 1) % 6 == 0 || (j - 27) % 5 == 0) col = K.TealR[1];
                else if ((i - 1) % 6 == 1 && !alt) col = K.TealR[4];
                if (bx == 2 && by == 1) col = K.Gold; 
                if (bx == 4 && by == 2) col = K.Terra;
                if (i >= 35) col = K.TealR[0];
                c.Set(i, j, col);
            }
        c.HL(1, 43, 36, K.TealR[0]);
        // quilt drape and footboard
        c.Rect(0, 44, 38, 8, w[2]);
        c.HL(0, 44, 38, w[5]); c.VL(0, 44, 8, w[4]); c.VL(37, 44, 8, w[0]); c.HL(0, 51, 38, w[0]);
        c.Rect(3, 46, 32, 4, w[3]); c.HL(3, 46, 32, w[1]); c.HL(3, 49, 32, w[4]);
        c.Rect(0, 42, 4, 3, w[3]); c.Rect(34, 42, 4, 3, w[3]);
        return U.Finish(c, 0.4);
    }

    static Canvas BedChild()
    {
        var c = new Canvas(26, 40);
        var w = K.Pine;
        c.Rect(0, 0, 26, 9, w[2]);
        c.HL(0, 0, 26, w[4]); c.VL(0, 0, 9, w[3]); c.VL(25, 0, 9, w[0]);
        c.Rect(2, 2, 22, 6, w[3]); c.HL(2, 2, 22, w[1]); c.VL(2, 2, 6, w[1]); c.HL(2, 7, 22, w[4]);
        c.Rect(1, 9, 24, 24, K.Linen[3]);
        c.Rect(4, 10, 18, 7, K.Cream); c.HL(4, 10, 18, K.Linen[4]); c.HL(4, 16, 18, K.Linen[1]); c.VL(21, 11, 6, K.Linen[1]);
        c.Set(4, 10, Rgba.Clear); c.Set(21, 10, Rgba.Clear);
        // teddy bear on the pillow
        c.Disc(18.5, 13, 2.2, K.Orange); c.Set(16, 10, K.Orange); c.Set(20, 10, K.Orange); c.Set(18, 13, K.Brown2); c.Set(19, 12, K.Brown2); c.Set(17, 12, Rgba.Hex("A96A33"));
        c.Rect(17, 15, 3, 2, K.Orange); c.Set(19, 15, Rgba.Hex("A96A33"));
        // blanket with stripes
        for (int j = 18; j < 34; j++)
            for (int i = 1; i < 25; i++)
            {
                Rgba col = ((j - 18) / 3) % 2 == 0 ? K.Gold : K.Orange;
                if ((j - 18) % 3 == 0) col = ((j - 18) / 3) % 2 == 0 ? K.Mustard[4] : Rgba.Hex("F4B07A");
                if (i >= 23) col = K.Mustard[0];
                c.Set(i, j, col);
            }
        c.HL(1, 17, 24, K.Linen[4]); c.HL(1, 18, 24, K.Cream); c.HL(1, 33, 24, K.Mustard[0]);
        c.Rect(0, 33, 26, 7, w[2]);
        c.HL(0, 33, 26, w[4]); c.VL(0, 33, 7, w[3]); c.VL(25, 33, 7, w[0]); c.HL(0, 39, 26, w[0]);
        c.Rect(3, 35, 20, 3, w[3]); c.HL(3, 35, 20, w[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas Wardrobe()
    {
        var c = new Canvas(34, 44);
        var w = K.Wood;
        c.Rect(0, 0, 34, 44, w[2]);
        // crown moulding
        c.Rect(0, 0, 34, 4, w[3]); c.HL(0, 0, 34, w[5]); c.HL(0, 3, 34, w[1]); c.HL(1, 4, 32, w[0]);
        c.VL(0, 0, 44, w[4]); c.VL(33, 0, 44, w[0]);
        DoorPanel(c, 2, 5, 14, 35, w, 2, true, K.Mustard[3]);
        DoorPanel(c, 18, 5, 14, 35, w, 2, false, K.Mustard[3]);
        // carved arch hint on each door
        c.HL(6, 9, 6, w[4]); c.HL(22, 9, 6, w[4]);
        c.HL(6, 31, 6, w[4]); c.HL(22, 31, 6, w[4]);
        c.Rect(16, 5, 2, 35, w[1]);
        c.Rect(0, 40, 34, 2, w[1]); c.HL(0, 40, 34, w[3]);
        c.Rect(1, 42, 4, 2, w[0]); c.Rect(29, 42, 4, 2, w[0]);
        // a straw hat on top peeking up is not drawn (keeps the silhouette clean); small ribbon hanging from the door
        c.Rect(14, 14, 2, 8, K.Terra); c.Set(14, 21, K.RedR[1]);
        return U.Finish(c, 0.4);
    }

    static Canvas Nightstand()
    {
        var c = new Canvas(14, 24);
        var w = K.Wood;
        // candle and a little book on top
        c.Rect(8, 3, 3, 6, K.Cream); c.VL(8, 3, 6, K.Linen[4]); c.VL(10, 3, 6, K.Linen[1]);
        c.Set(9, 2, K.Orange); c.Set(9, 1, K.Gold); c.Set(9, 0, Rgba.Hex("FBE9A8"));
        c.Rect(2, 6, 5, 3, K.BlueR[2]); c.HL(2, 6, 5, K.BlueR[4]); c.HL(2, 8, 5, K.BlueR[0]); c.HL(2, 7, 5, K.Cream);
        c.Rect(0, 9, 14, 3, w[4]); c.HL(0, 9, 14, w[5]); c.HL(0, 11, 14, w[2]);
        c.Rect(0, 12, 14, 8, w[2]); c.VL(0, 12, 8, w[4]); c.VL(13, 12, 8, w[0]); c.HL(0, 19, 14, w[0]);
        c.Rect(2, 13, 10, 5, w[3]); c.HL(2, 13, 10, w[1]); c.HL(2, 17, 10, w[4]);
        c.Rect(6, 15, 2, 1, K.Mustard[3]);
        c.Rect(1, 20, 2, 4, w[1]); c.Rect(11, 20, 2, 4, w[1]);
        return U.Finish(c, 0.4);
    }
}

static class RgbaExt { }
