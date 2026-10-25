namespace WorldArt;

/// <summary>Composition of the Chapter 1 map: where every prop sits, collision rectangles, interactables.</summary>
public static class Plan
{
    const string Dry = "!ch1_garden_watered", Wet = "ch1_garden_watered";
    const string Full = "!ch1_breakfast_shared", Empty = "ch1_breakfast_shared";

    static int idc;
    static string Nm(string s) => $"{s}_{idc++}";

    /// <summary>Place a sprite by its bottom-centre pivot. cw/ch = collider footprint (bottom-centred), shadow: 0 none, 1 rect under footprint, 2 ellipse.</summary>
    static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0, int sdx = 0)
    {
        string name = Nm(sprite.Replace("prop_", ""));
        Layout.Prop(name, sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1 && cw > 0) Layout.ShadowRect(x + cdx - cw / 2 + 2, y - ch + 1, cw, ch + 1, 0.8);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.12 + sdx, y - 2, s.W * 0.40, 7, 0.9);
        else if (shadow == 3) Layout.ShadowEll(x + 2 + sdx, y - 1, s.W * 0.42, 4, 0.8);
    }

    static void Tree(string sprite, int x, int y, int trunkW = 10) => Put(sprite, x, y, "y", "", trunkW, 6, 0, 2);

    static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y") => Layout.Anim(name, frames, fps, x, y, sort);

    public static void Build()
    {
        Walls();
        Kitchen();
        Living();
        Bedroom();
        Front();
        Yard();
        GardenArea();
        Border();
        Characters();
        Spots();
        Layout.PlayerStart = (176, 372);
        Layout.MustReach.Add(("doorway", 176, 308));
        Layout.MustReach.Add(("garden gate", 568, 300));
        Layout.MustReach.Add(("kitchen", 140, 180));
        Layout.MustReach.Add(("living", 270, 230));
        Layout.MustReach.Add(("bedroom", 340, 176));
        Layout.MustReach.Add(("well", 100, 440));
        Layout.MustReach.Add(("garden north", 520, 70));
        Layout.MustReach.Add(("garden east", 650, 168));
    }

    // ---- walls, partitions and the outer border ---------------------------------------------------------------
    static void Walls()
    {
        Layout.Col(48, 0, 384, 96);                      // north wall + roof eave
        Layout.Col(48, 96, 16, 224);                     // west wall
        Layout.Col(416, 96, 16, 224);                    // east wall
        Layout.Col(48, 296, 112, 24);                    // front wall, west of the doorway
        Layout.Col(192, 296, 240, 24);                   // front wall, east of the doorway
        Layout.Col(G.DivX, 96, 8, G.DivGY0 - 96);        // kitchen | living partition
        Layout.Col(G.DivX, G.DivGY1, 8, 296 - G.DivGY1);
        Layout.Col(G.BedWX, 96, 8, G.BedWallY + 32 - 96);   // bedroom west wall
        Layout.Col(G.BedGX0, G.BedWallY, 3, 32);            // bedroom doorway trims
        Layout.Col(G.BedGX1 - 3, G.BedWallY, 416 - (G.BedGX1 - 3), 32);
        // forest border
        Layout.Col(0, 0, 704, 36);
        Layout.Col(0, 0, 38, 480);
        Layout.Col(684, 0, 20, 480);
        Layout.Col(0, 458, 704, 22);
    }

    // ---- kitchen & dining ---------------------------------------------------------------------------------
    static void Kitchen()
    {
        Put("prop_rug_dining", 112, 258, "floor");
        Put("prop_rug_bedroom", 178, 254, "floor");
        Put("prop_rug_kitchen", 124, 168, "floor");
        Put("prop_basket_logs", 76, 186, "y", "", 14, 6, 0, 3);
        Put("prop_plant_small", 210, 272, "y", "", 10, 6, 0, 3);
        Put("prop_counter_sink", 88, 108, "y", "", 48, 12, 0, 1);
        Put("prop_stove", 124, 108, "y", "", 24, 12, 0, 1);
        Put("prop_herbs", 154, 92);
        Put("prop_basket_logs", 148, 112, "y", "", 14, 6, 0, 3);
        Put("prop_kitchen_shelf", 192, 112, "y", "", 44, 16, 0, 1);
        Put("prop_kitchen_island", 124, 152, "y", "", 46, 16, 0, 1);
        Put("prop_veg_crate", 74, 128, "y", "", 18, 8, 0, 3);
        Put("prop_stool", 152, 166, "y", "", 10, 6, 0, 3);
        Put("prop_sacks", 200, 182, "y", "", 32, 10, 0, 1);
        // dining set
        Put("prop_chair_back", 100, 208, "y", "", 12, 6);
        Put("prop_chair_back", 124, 208, "y", "", 12, 6);
        Put("prop_table_full", 112, 240, "y", Full, 48, 20, 0, 1);
        Put("prop_table_empty", 112, 240, "y", Empty);
        Put("prop_chair_left", 74, 238, "y", "", 12, 8);
        Put("prop_chair_right", 150, 238, "y", "", 12, 8);
        Put("prop_chair_front", 98, 264, "y", "", 12, 6);
        // entrance
        Put("prop_doormat", 176, 313, "floor");
        Put("prop_broom", 146, 296, "y", "", 6, 4);
        Put("prop_coatrack", 207, 294, "y", "", 12, 6, 0, 3);
        Put("prop_plant_pot", 78, 292, "y", "", 12, 8, 0, 3);
    }

    // ---- living room ----------------------------------------------------------------------------------------
    static void Living()
    {
        Put("prop_bookcase", 244, 110, "y", "", 32, 14, 0, 1);
        Put("prop_sideboard_photo", 286, 110, "y", "", 44, 14, 0, 1);
        Put("prop_rug_kitchen", 286, 128, "floor");
        Put("prop_plant_pot", 238, 164, "y", "", 12, 8, 0, 3);
        Put("prop_armchair", 270, 196, "y", "", 22, 12, 0, 1);
        Put("prop_floorlamp", 298, 192, "y", "", 8, 6, 0, 3);
        // desk corner
        Put("prop_desk", 268, 264, "y", "", 40, 14, 0, 1);
        Put("prop_stool", 268, 280, "y", "", 10, 6, 0, 3);
        Put("prop_plant_small", 234, 292, "y", "", 10, 6, 0, 3);
        // seating group
        Put("prop_rug_living", 378, 274, "floor");
        Put("prop_sofa", 380, 254, "y", "", 50, 16, 0, 1);
        Put("prop_floorlamp", 410, 250, "y", "", 8, 6, 0, 3);
        Put("prop_coffee_table", 380, 284, "y", "", 26, 10, 0, 1);
        Put("prop_armchair", 332, 284, "y", "", 22, 12, 0, 1);
        Put("prop_plant_pot", 306, 292, "y", "", 12, 8, 0, 3);
    }

    // ---- bedroom --------------------------------------------------------------------------------------------
    static void Bedroom()
    {
        Put("prop_rug_bedroom", 352, 176, "floor");
        Put("prop_bed_double", 346, 148, "y", "", 38, 26, 0, 1);
        Put("prop_nightstand", 372, 124, "y", "", 14, 10, 0, 3);
        Put("prop_wardrobe", 398, 112, "y", "", 34, 16, 0, 1);
        Put("prop_bed_child", 398, 172, "y", "", 26, 22, 0, 1);
    }

    // ---- house front --------------------------------------------------------------------------------------
    static void Front()
    {
        Put("prop_chimney", 124, 60);
        foreach (var x in new[] { 84, 128 }) Put("prop_windowbox_a", x, 319);
        Put("prop_windowbox_b", 232, 319);
        Put("prop_windowbox_a", 300, 319);
        Put("prop_windowbox_b", 376, 319);
        Put("prop_lantern", 144, 323, "y", "", 6, 4);
        Put("prop_lantern", 208, 323, "y", "", 6, 4);
        Put("prop_pot_red", 152, 334, "y", "", 10, 5);
        Put("prop_pot_blue", 217, 334, "y", "", 10, 5);
        // flower beds along the wall (colliders cover the beds + stone edging)
        Layout.Col(56, 322, 88, 18);
        Layout.Col(212, 322, 212, 18);
        var fl = new[] { "prop_flowers_red", "prop_flowers_yellow", "prop_flowers_pink", "prop_flowers_lavender", "prop_flowers_white" };
        int k = 0;
        for (int x = 68; x < 140; x += 22) Put(fl[k++ % fl.Length], x, 337);
        for (int x = 226; x < 420; x += 24) Put(fl[k++ % fl.Length], x, 337);
    }

    // ---- yard -----------------------------------------------------------------------------------------------
    static void Yard()
    {
        // wood corner
        Put("prop_woodstack", 70, 368, "y", "", 40, 10, 0, 1);
        Put("prop_chopping_block", 108, 368, "y", "", 12, 6, 0, 3);
        Put("prop_logs_split", 124, 372, "floor");
        Put("prop_wheelbarrow", 150, 366, "y", "", 26, 8, 0, 3);
        // well
        Put("prop_well", 100, 428, "y", "", 28, 12, 0, 2);
        Put("prop_bucket", 132, 424, "y", "", 10, 5);
        Put("prop_basket", 66, 428, "y", "", 14, 6);
        // washing line on the south lawn
        Anim("laundry", new[] { "prop_laundry_0", "prop_laundry_1" }, 1, 332, 440);
        Layout.Col(282, 434, 8, 6); Layout.Col(378, 434, 8, 6);
        Layout.ShadowEll(332, 440, 52, 3, 0.7);
        // bench corner in the SE lawn
        Tree("prop_tree_broad", 640, 398, 12);
        Put("prop_bench", 640, 414, "y", "", 38, 10, 0, 1);
        Tree("prop_tree_fruit", 588, 442, 10);
        Tree("prop_tree_fruit", 218, 438, 10);
        Tree("prop_tree_broad", 504, 447, 12);
        Tree("prop_tree_cypress", 666, 342, 6);
        Put("prop_sunflowers", 612, 346, "y", "", 20, 6, 0, 3);
        // yard dressing: bushes, rocks, grass, flowers
        Put("prop_bush_berry", 52, 346, "y", "", 20, 8, 0, 3);
        Put("prop_bush_round", 220, 352, "y", "", 24, 8, 0, 3);
        Put("prop_bush_round", 430, 352, "y", "", 24, 8, 0, 3);
        Put("prop_rock_a", 172, 432, "y", "", 16, 6, 0, 3);
        Put("prop_rock_b", 346, 420, "y", "", 10, 5);
        Put("prop_rock_c", 450, 424, "y", "", 22, 8, 0, 3);
        Put("prop_rock_b", 76, 404, "y", "", 10, 5);
        Put("prop_hedge", 440, 440, "y", "", 30, 8, 0, 3);
        Put("prop_hedge", 408, 442, "y", "", 30, 8, 0, 3);
        Put("prop_hedge_short", 566, 408, "y", "", 16, 8, 0, 3);
        Put("prop_grass_tall_b", 200, 362);
        Put("prop_grass_tall_a", 284, 360);
        Put("prop_grass_tall_b", 346, 358);
        Put("prop_grass_tall_a", 480, 362);
        Put("prop_grass_tall_b", 624, 440);
        Put("prop_grass_tall_a", 154, 404);
        Put("prop_wildflowers_a", 252, 352, "floor");
        Put("prop_wildflowers_b", 392, 356, "floor");
        Put("prop_wildflowers_a", 300, 416, "floor");
        Put("prop_wildflowers_b", 206, 416, "floor");
        Put("prop_wildflowers_a", 560, 428, "floor");
        Put("prop_wildflowers_b", 600, 360, "floor");
        Put("prop_wildflowers_a", 40, 390, "floor");
        Put("prop_wildflowers_b", 140, 448, "floor");
        // low stone wall along the east of the front yard
        Put("prop_stonewall_a", 616, 372, "y", "", 48, 8);
        Put("prop_stonewall_b", 656, 372, "y", "", 32, 8);
    }

    // ---- vegetable garden ----------------------------------------------------------------------------------
    static void GardenArea()
    {
        // beds: NW tomatoes, NE corn, SW greens, SE carrots
        var pos = new[] { (514, 142), (622, 142), (514, 246), (622, 246) };
        for (int i = 0; i < 4; i++)
        {
            var (x, y) = pos[i];
            Put($"prop_bed_{i + 1}_dry", x, y, "y", Dry, 76, 52, 0, 1);
            Put($"prop_bed_{i + 1}_watered", x, y, "y", Wet);
        }
        Put("prop_scarecrow", 568, 98, "y", "", 6, 5);
        Put("prop_rain_barrel", 488, 284, "y", "", 18, 8, 0, 3);
        Put("prop_compost", 646, 286, "y", "", 36, 14, 0, 1);
        Put("prop_pumpkins", 604, 282, "y", "", 34, 8, 0, 3);
        Put("prop_trellis", 600, 74, "y", "", 12, 4);
        Put("prop_trellis", 534, 74, "y", "", 12, 4);
        // fence: north run, west & east runs, south runs with the gate
        for (int x = 472; x < 670; x += 16) Put("prop_fence_h", x, 64);
        Layout.Col(462, 56, 212, 8);
        for (int y = 78; y <= 302; y += 16) { Put("prop_fence_v", 466, y); Put("prop_fence_v", 668, y); }
        Layout.Col(460, 56, 8, 252); Layout.Col(666, 56, 8, 252);
        for (int x = 472; x <= 536; x += 16) Put("prop_fence_h", x, 304);
        for (int x = 598; x <= 662; x += 16) Put("prop_fence_h", x, 304);
        Layout.Col(462, 294, 84, 12);
        Layout.Col(590, 294, 82, 12);
        Layout.Prop(Nm("gate"), "prop_gate", 568, 304);
        Layout.Col(546, 294, 8, 12); Layout.Col(582, 294, 8, 12);
        foreach (var (x, y) in new[] { (466, 62), (668, 62), (466, 304), (668, 304) }) Put("prop_fence_post", x, y);
        // keepsake by the gate
        Put("prop_watering_can", 528, 320, "y", "", 12, 5, 0, 3);
        Put("prop_bucket", 540, 318, "y", "", 8, 4);
        // wild flowers & grass inside and just outside the fence
        Put("prop_flowers_sway_0", 456, 120, "y");
        Put("prop_flowers_sway_0", 474, 166, "y");
        Put("prop_flowers_sway_1", 662, 166, "y");
        Put("prop_rock_b", 596, 164, "y", "", 10, 5);
        Put("prop_rock_a", 636, 166, "y", "", 16, 6, 0, 3);
        Put("prop_wheelbarrow", 506, 166, "y", "", 26, 8, 0, 3);
    }

    static void Border()
    {
        // flanking the house east wall
        Tree("prop_tree_cypress", 448, 112, 6);
        Tree("prop_tree_cypress", 449, 300, 6);
        Put("prop_sunflowers", 446, 190, "y", "", 20, 6, 0, 3);
        Put("prop_hedge", 448, 222, "y", "", 28, 8, 0, 3);
        Put("prop_bush_round", 449, 258, "y", "", 22, 8, 0, 3);
        Put("prop_rock_c", 50, 300, "y", "", 22, 8, 0, 3);
        Put("prop_bush_berry", 54, 262, "y", "", 20, 8, 0, 3);
        Put("prop_bush_round", 52, 130, "y", "", 24, 8, 0, 3);
        Put("prop_grass_tall_b", 60, 200);
        Put("prop_wildflowers_b", 54, 230, "floor");
        Put("prop_rock_b", 440, 154, "y", "", 10, 5);
    }

    // ---- animated actors & small movers -----------------------------------------------------------------------
    static void Characters()
    {
        Anim("child_a_dining", new[] { "child_a_sit_right_0", "child_a_sit_right_1" }, 2, 75, 239, "y");
        Anim("child_b_dining", new[] { "child_b_sit_left_0", "child_b_sit_left_1" }, 2, 149, 239, "y");
        Anim("cat_step", new[] { "cat_sleep_0", "cat_sleep_1" }, 1, 198, 332, "y");
        Anim("smoke", new[] { "prop_smoke_0", "prop_smoke_1", "prop_smoke_2", "prop_smoke_3" }, 4, 128, 32, "above");
        Anim("butterfly_a1", new[] { "prop_butterfly_a_0", "prop_butterfly_a_1" }, 6, 240, 330, "above");
        Anim("butterfly_b1", new[] { "prop_butterfly_b_0", "prop_butterfly_b_1" }, 7, 98, 352, "above");
        Anim("butterfly_a2", new[] { "prop_butterfly_a_0", "prop_butterfly_a_1" }, 6, 540, 170, "above");
        Anim("butterfly_b2", new[] { "prop_butterfly_b_0", "prop_butterfly_b_1" }, 7, 610, 300, "above");
        Anim("butterfly_a3", new[] { "prop_butterfly_a_0", "prop_butterfly_a_1" }, 6, 410, 420, "above");
        var fr = new[] { "prop_flowers_sway_0", "prop_flowers_sway_1" };
        foreach (var (x, y) in new[] { (60, 420), (190, 404), (240, 424), (310, 438), (380, 414), (470, 400), (560, 416), (620, 430), (150, 380), (330, 360), (500, 352), (36, 332), (452, 134), (452, 236) })
            Anim($"flowers_sway_{x}_{y}", fr, 2, x, y, "y");
    }

    // ---- interactables --------------------------------------------------------------------------------------
    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch1_garden", 530, 268, 12, 516, 164));
        Layout.Spots.Add(new Spot("ch1_breakfast", 130, 264, 12, 112, 182));
        Layout.Spots.Add(new Spot("ch1_photo", 286, 134, 12, 286, 63));
        Layout.Spots.Add(new Spot("ch1_shelf", 192, 134, 12, 192, 63));
        Layout.Spots.Add(new Spot("ch1_ks_wateringcan", 538, 338, 12, 528, 298));
    }
}
