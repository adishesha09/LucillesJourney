namespace WorldArtCh4;

/// <summary>Composition of the Chapter 4 map: the Chapter 1 house and garden on the morning the family leaves.</summary>
public static class Plan
{
    public static readonly string[] RequiredIds =
    {
        "ch4_pack_documents", "ch4_pack_food", "ch4_pack_photo", "ch4_depart", "ch4_farewell", "ch4_shelf", "ch4_ks_clothbag",
    };
    public const int YardGateX0 = 162, YardGateX1 = 190, YardGateY = 458;

    const string NoDocs = "!ch4_documents_packed", NoFood = "!ch4_food_packed", NoPhoto = "!ch4_photo_packed";
    const string BagOpen = "!ch4_bag_packed", BagShut = "ch4_bag_packed";

    static int idc;
    static string Nm(string s) => $"{s}_{idc++}";

    /// <summary>Sprite name of a (graded) Chapter 1 prop re-used in this chapter.</summary>
    static string P(string name) => Art.Use("prop_" + name);

    /// <summary>Place a sprite by its bottom-centre pivot. cw/ch = collider footprint (bottom-centred), shadow: 0 none, 1 rect under footprint, 2 ellipse, 3 small ellipse.</summary>
    static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0, int sdx = 0)
    {
        string name = Nm(sprite.Replace("prop_c4_", ""));
        Layout.Prop(name, sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1 && cw > 0) Layout.ShadowRect(x + cdx - cw / 2 + 3, y - ch + 1, cw, ch + 2, 0.7);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.20 + sdx, y - 1, s.W * 0.52, 8, 0.75);
        else if (shadow == 3) Layout.ShadowEll(x + 3 + sdx, y - 1, s.W * 0.48, 4, 0.7);
    }

    static void Tree(string sprite, int x, int y, int trunkW = 10) => Put(P(sprite), x, y, "y", "", trunkW, 6, 0, 2);

    static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y", string show = "") => Layout.Anim(name, frames, fps, x, y, sort, show);

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
        Road();
        Characters();
        Spots();
        Layout.PlayerStart = (176, 272);
        Layout.MustReach.Add(("doorway", 176, 308));
        Layout.MustReach.Add(("garden gate", 568, 300));
        Layout.MustReach.Add(("kitchen", 140, 180));
        Layout.MustReach.Add(("living", 270, 230));
        Layout.MustReach.Add(("bedroom", 340, 176));
        Layout.MustReach.Add(("well", 100, 440));
        Layout.MustReach.Add(("garden north", 520, 70));
        Layout.MustReach.Add(("garden east", 650, 168));
        Layout.MustReach.Add(("yard gate", 176, 458));
        Layout.MustReach.Add(("road west", 70, 510));
        Layout.MustReach.Add(("road east", 640, 504));
        Layout.StateSetters.Add(("ch4_pack", "ch4_bag_packed"));
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
        // forest border; the road passes through the west and east edges and ends in mist
        Layout.Col(0, 0, 704, 36);
        Layout.Col(0, 0, 38, 576);
        Layout.Col(684, 0, 20, 576);
        Layout.Col(0, 554, 704, 22);
    }

    // ---- kitchen & dining ---------------------------------------------------------------------------------
    static void Kitchen()
    {
        Look.RugMarks.Add(new Rc(76, 214, 72, 42));    // where the dining rug lay
        Look.RugMarks.Add(new Rc(104, 156, 40, 10));   // kitchen runner
        Put(P("counter_sink"), 88, 108, "y", "", 48, 12, 0, 1);
        Put("prop_c4_stove_cold", 124, 108, "y", "", 24, 12, 0, 1);
        Put("prop_c4_shelf_bare", 192, 112, "y", "", 44, 16, 0, 1);
        Put(P("kitchen_island"), 124, 152, "y", "", 46, 16, 0, 1);
        Put(P("stool"), 152, 166, "y", "", 10, 6, 0, 3);
        Put(P("sacks"), 200, 182, "y", "", 32, 10, 0, 1);
        // a packed crate and bound parcels against the west wall
        Put("prop_c4_crate", 82, 150, "y", "", 28, 10, 0, 1);
        Put("prop_c4_parcel", 76, 198, "y", "", 18, 8, 0, 1);
        Put("prop_c4_bundle", 76, 178, "y", "", 12, 6, 0, 3);
        // dining set: bare table, chairs pushed in
        Put(P("chair_back"), 100, 213, "y", "", 12, 6);
        Put(P("chair_back"), 124, 213, "y", "", 12, 6);
        Put("prop_c4_table_bare", 112, 240, "y", "", 48, 20, 0, 1);
        Put("prop_c4_food", 112, 241, "y", NoFood);
        Put(P("chair_left"), 81, 241, "y", "", 12, 8);
        Put(P("chair_right"), 143, 241, "y", "", 12, 8);
        Put(P("chair_front"), 98, 257, "y", "", 12, 6);
        Put("prop_c4_clothbag", 69, 239, "y", "", 10, 5);
        // entrance
        Put(P("doormat"), 176, 313, "floor");
        Put(P("coatrack"), 207, 294, "y", "", 12, 6, 0, 3);
        Put(P("plant_pot"), 78, 292, "y", "", 12, 8, 0, 3);
        Put("prop_c4_rug_roll", 96, 290, "y", "", 10, 6, 0, 3);
        Put("prop_c4_scraps", 158, 262, "floor");
        Put("prop_c4_scraps", 130, 286, "floor");
        // the open travelling bag by the door, closed and strapped once everything is in
        Put("prop_c4_bag_open", 152, 272, "y", BagOpen, 18, 6, 0, 3);
        Put("prop_c4_bag_closed", 152, 272, "y", BagShut);
        // the door itself stands ajar against the east jamb
        Put("prop_c4_door_ajar", 197, 321, "y", "", 0, 0);
        Layout.Col(193, 300, 8, 20);
    }

    // ---- living room ----------------------------------------------------------------------------------------
    static void Living()
    {
        Look.RugMarks.Add(new Rc(344, 258, 68, 36));   // living-room rug
        Look.RugMarks.Add(new Rc(266, 120, 40, 8));
        Put("prop_c4_bookcase_sheeted", 244, 110, "y", "", 32, 14, 0, 1);
        Put("prop_c4_sideboard_bare", 286, 110, "y", "", 44, 14, 0, 1);
        Put("prop_c4_photo_frame", 286, 110, "y", NoPhoto);
        Put(P("plant_pot"), 238, 164, "y", "", 12, 8, 0, 3);
        Put("prop_c4_armchair_sheeted", 270, 196, "y", "", 22, 12, 0, 1);
        Put(P("floorlamp"), 298, 192, "y", "", 8, 6, 0, 3);
        // desk corner
        Put("prop_c4_desk_bare", 268, 264, "y", "", 40, 14, 0, 1);
        Put("prop_c4_papers", 268, 265, "y", NoDocs);
        Put(P("plant_small"), 234, 292, "y", "", 10, 6, 0, 3);
        // seating group under dust sheets
        Put("prop_c4_sofa_sheeted", 380, 254, "y", "", 50, 16, 0, 1);
        Put(P("floorlamp"), 410, 250, "y", "", 8, 6, 0, 3);
        Put("prop_c4_coffee_table_sheeted", 366, 284, "y", "", 26, 10, 0, 1);
        Put("prop_c4_armchair_sheeted", 322, 284, "y", "", 22, 12, 0, 1);
        Put(P("plant_pot"), 296, 292, "y", "", 12, 8, 0, 3);
        Put("prop_c4_crate", 400, 292, "y", "", 26, 10, 0, 1);
        Put("prop_c4_scraps", 300, 240, "floor");
    }

    // ---- bedroom --------------------------------------------------------------------------------------------
    static void Bedroom()
    {
        Put("prop_c4_bed_sheeted", 346, 148, "y", "", 38, 26, 0, 1);
        Put(P("nightstand"), 372, 124, "y", "", 14, 10, 0, 3);
        Put(P("wardrobe"), 398, 112, "y", "", 34, 16, 0, 1);
        Put(P("bed_child"), 398, 172, "y", "", 26, 22, 0, 1);
    }

    // ---- house front --------------------------------------------------------------------------------------
    static void Front()
    {
        Put(P("chimney"), 124, 60);
        foreach (var x in new[] { 84, 128 }) Put(P("windowbox_a"), x, 319);
        Put(P("windowbox_b"), 232, 319);
        Put(P("windowbox_a"), 300, 319);
        Put(P("windowbox_b"), 376, 319);
        foreach (var x in new[] { 144, 208 })
        {
            Layout.Anim($"lantern_{x}", new[] { "prop_c4_lantern_0", "prop_c4_lantern_1" }, 3, x, 323, "y");
            Layout.Col(x - 3, 319, 6, 4);
            Look.Pools.Add((x, 330, 34, 18, 1.15));
        }
        Look.Pools.Add((154, 112, 32, 16, 0.95));       // the lantern left burning in the kitchen window
        Put(P("pot_red"), 152, 334, "y", "", 10, 5);
        Put(P("pot_blue"), 217, 334, "y", "", 10, 5);
        // flower beds along the wall (colliders cover the beds + stone edging)
        Layout.Col(56, 322, 88, 18);
        Layout.Col(212, 322, 212, 18);
        var fl = new[] { "flowers_red", "flowers_yellow", "flowers_pink", "flowers_lavender", "flowers_white" };
        int k = 0;
        for (int x = 68; x < 140; x += 22) Put(P(fl[k++ % fl.Length]), x, 337);
        for (int x = 226; x < 420; x += 24) Put(P(fl[k++ % fl.Length]), x, 337);
    }

    // ---- yard -----------------------------------------------------------------------------------------------
    static void Yard()
    {
        // wood corner
        Put(P("woodstack"), 70, 368, "y", "", 40, 10, 0, 1);
        Put(P("chopping_block"), 108, 368, "y", "", 12, 6, 0, 3);
        Put(P("logs_split"), 124, 372, "floor");
        // well
        Put(P("well"), 100, 428, "y", "", 28, 12, 0, 2);
        Put(P("bucket"), 132, 424, "y", "", 10, 5);
        Put(P("basket"), 66, 428, "y", "", 14, 6);
        // the line is bare: only the posts remain
        Layout.Col(282, 434, 8, 6); Layout.Col(378, 434, 8, 6);
        // bench corner in the SE lawn
        Tree("tree_broad", 640, 398, 12);
        Put(P("bench"), 640, 414, "y", "", 38, 10, 0, 1);
        Tree("tree_fruit", 588, 442, 10);
        Tree("tree_fruit", 218, 438, 10);
        Tree("tree_broad", 504, 447, 12);
        Tree("tree_cypress", 666, 342, 6);
        Put(P("sunflowers"), 612, 346, "y", "", 20, 6, 0, 3);
        // yard dressing: bushes, rocks, grass
        Put(P("bush_berry"), 52, 346, "y", "", 20, 8, 0, 3);
        Put(P("bush_round"), 220, 352, "y", "", 24, 8, 0, 3);
        Put(P("bush_round"), 430, 352, "y", "", 24, 8, 0, 3);
        Put(P("rock_a"), 246, 454, "y", "", 16, 6, 0, 3);
        Put(P("rock_b"), 346, 420, "y", "", 10, 5);
        Put(P("rock_c"), 450, 424, "y", "", 22, 8, 0, 3);
        Put(P("rock_b"), 76, 404, "y", "", 10, 5);
        Put(P("hedge"), 440, 440, "y", "", 30, 8, 0, 3);
        Put(P("hedge"), 408, 442, "y", "", 30, 8, 0, 3);
        Put(P("hedge_short"), 566, 408, "y", "", 16, 8, 0, 3);
        Put(P("grass_tall_b"), 200, 362);
        Put(P("grass_tall_a"), 284, 360);
        Put(P("grass_tall_b"), 346, 358);
        Put(P("grass_tall_a"), 480, 362);
        Put(P("grass_tall_b"), 624, 440);
        Put(P("grass_tall_a"), 154, 404);
        // low stone wall along the east of the front yard
        Put(P("stonewall_a"), 616, 372, "y", "", 48, 8);
        Put(P("stonewall_b"), 656, 372, "y", "", 32, 8);
        // the picket fence along the road, with the gate standing open in the middle
        for (int x = 52; x <= 148; x += 16) Put(P("fence_h"), x, 462);
        for (int x = 204; x <= 676; x += 16) Put(P("fence_h"), x, 462);
        Layout.Col(44, 454, 114, 8);
        Layout.Col(194, 454, 482, 8);
        Put(P("gate"), 176, 462);
        Layout.Col(154, 452, 8, 12); Layout.Col(190, 452, 8, 12);
        foreach (var x in new[] { 44, 154, 198, 676 }) Put(P("fence_post"), x, 464);
    }

    // ---- vegetable garden (unwatered) ------------------------------------------------------------------------
    static void GardenArea()
    {
        var pos = new[] { (514, 142), (622, 142), (514, 246), (622, 246) };
        for (int i = 0; i < 4; i++)
        {
            var (x, y) = pos[i];
            Put($"prop_c4_bed_{i + 1}_wilted", x, y, "y", "", 76, 52, 0, 1);
        }
        Put(P("scarecrow"), 568, 98, "y", "", 6, 5);
        Put(P("rain_barrel"), 488, 284, "y", "", 18, 8, 0, 3);
        Put(P("compost"), 646, 286, "y", "", 36, 14, 0, 1);
        Put(P("trellis"), 600, 74, "y", "", 12, 4);
        Put(P("trellis"), 534, 74, "y", "", 12, 4);
        // fence: north run, west & east runs, south runs with the gate
        for (int x = 472; x < 670; x += 16) Put(P("fence_h"), x, 64);
        Layout.Col(462, 56, 212, 8);
        for (int y = 78; y <= 302; y += 16) { Put(P("fence_v"), 466, y); Put(P("fence_v"), 668, y); }
        Layout.Col(460, 56, 8, 252); Layout.Col(666, 56, 8, 252);
        for (int x = 472; x <= 536; x += 16) Put(P("fence_h"), x, 304);
        for (int x = 598; x <= 662; x += 16) Put(P("fence_h"), x, 304);
        Layout.Col(462, 294, 84, 12);
        Layout.Col(590, 294, 82, 12);
        Layout.Prop(Nm("gate"), P("gate"), 568, 304);
        Layout.Col(546, 294, 8, 12); Layout.Col(582, 294, 8, 12);
        foreach (var (x, y) in new[] { (466, 62), (668, 62), (466, 304), (668, 304) }) Put(P("fence_post"), x, y);
        // the can nobody will carry
        Put(P("watering_can"), 528, 322, "y", "", 12, 5, 0, 3);
        Put(P("bucket"), 540, 320, "y", "", 8, 4);
        Put(P("flowers_sway_0"), 456, 120, "y");
        Put(P("rock_b"), 596, 164, "y", "", 10, 5);
        Put(P("rock_a"), 636, 166, "y", "", 16, 6, 0, 3);
        Put(P("wheelbarrow"), 506, 166, "y", "", 26, 8, 0, 3);
    }

    static void Border()
    {
        Tree("tree_cypress", 448, 112, 6);
        Tree("tree_cypress", 449, 300, 6);
        Put(P("sunflowers"), 446, 190, "y", "", 20, 6, 0, 3);
        Put(P("hedge"), 448, 222, "y", "", 28, 8, 0, 3);
        Put(P("bush_round"), 449, 258, "y", "", 22, 8, 0, 3);
        Put(P("rock_c"), 50, 300, "y", "", 22, 8, 0, 3);
        Put(P("bush_berry"), 54, 262, "y", "", 20, 8, 0, 3);
        Put(P("bush_round"), 52, 130, "y", "", 24, 8, 0, 3);
        Put(P("grass_tall_b"), 60, 200);
        Put(P("rock_b"), 440, 154, "y", "", 10, 5);
    }

    // ---- the road beyond the gate ---------------------------------------------------------------------------
    static void Road()
    {
        Put("prop_c4_signpost", 214, 492, "y", "", 6, 4, 0, 3);
        Put(P("rock_a"), 150, 486, "y", "", 16, 6, 0, 3);
        Put(P("rock_c"), 322, 490, "y", "", 22, 8, 0, 3);
        Put(P("rock_b"), 470, 494, "y", "", 10, 5);
        Put(P("grass_tall_b"), 120, 478);
        Put(P("grass_tall_a"), 256, 482);
        Put(P("grass_tall_b"), 400, 484);
        Put(P("grass_tall_a"), 540, 488);
        Put(P("grass_tall_b"), 610, 480);
        Put(P("bush_round"), 80, 548, "y", "", 24, 8, 0, 3);
        Put(P("bush_round"), 372, 549, "y", "", 24, 8, 0, 3);
        Put(P("bush_berry"), 288, 548, "y", "", 20, 8, 0, 3);
        Tree("tree_cypress", 20, 540, 6);
        Tree("tree_cypress", 680, 538, 6);
        Tree("tree_cypress", 690, 470, 6);
        Tree("tree_broad", 520, 552, 12);
    }

    // ---- animated actors & small movers -----------------------------------------------------------------------
    static void Characters()
    {
        // the children wait on the porch with their bundles, the cat sits on the step
        Anim("child_a_wait", new[] { "npc_c4_child_a_wait_0", "npc_c4_child_a_wait_1" }, 2, 152, 346, "y");
        Layout.Col(148, 343, 8, 3);
        Anim("child_b_wait", new[] { "npc_c4_child_b_wait_0", "npc_c4_child_b_wait_1" }, 2, 204, 350, "y");
        Layout.Col(200, 347, 8, 3);
        Layout.ShadowEll(154, 346, 6, 2.5, 0.8); Layout.ShadowEll(206, 350, 6, 2.5, 0.8);
        Put("prop_c4_bundle", 134, 352, "y", "", 12, 5, 0, 3);
        Put("prop_c4_bundle", 222, 354, "y", "", 12, 5, 0, 3);
        Anim("cat_step", new[] { "npc_c4_cat_wait_0", "npc_c4_cat_wait_1" }, 1, 190, 332, "y");
        Layout.Col(185, 329, 10, 3);
        // wilted wildflowers stir in the draught
        var fr = new[] { Art.Use("prop_flowers_sway_0"), Art.Use("prop_flowers_sway_1") };
        foreach (var (x, y) in new[] { (60, 420), (240, 424), (380, 414), (470, 400), (150, 380), (330, 360), (36, 332), (452, 134) })
            Anim($"flowers_sway_{x}_{y}", fr, 2, x, y, "y");
        // mist drifting across the road, the yard and the garden
        foreach (var (n, x, y) in new[] { ("a", 110, 500), ("c", 290, 514), ("a", 400, 528), ("c", 520, 522), ("b", 640, 496), ("a", 600, 536), ("b", 40, 520), ("b", 230, 470), ("c", 420, 450), ("a", 520, 340), ("b", 610, 40), ("b", 300, 396) })
            Layout.Anim($"mist_{n}_{x}_{y}", new[] { $"fx_c4_mist_{n}_0", $"fx_c4_mist_{n}_1" }, 1, x, y, "floor");
    }

    // ---- interactables --------------------------------------------------------------------------------------
    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch4_pack_documents", 268, 278, 12, 268, 236, "ch4_documents_packed"));
        Layout.Spots.Add(new Spot("ch4_pack_food", 122, 268, 12, 112, 206, "ch4_food_packed"));
        Layout.Spots.Add(new Spot("ch4_pack_photo", 286, 134, 12, 286, 72, "ch4_photo_packed"));
        Layout.Spots.Add(new Spot("ch4_depart", 176, 440, 12, 176, 448, "ch4_departed"));
        Layout.Spots.Add(new Spot("ch4_farewell", 568, 320, 12, 568, 298));
        Layout.Spots.Add(new Spot("ch4_shelf", 192, 134, 12, 192, 68));
        Layout.Spots.Add(new Spot("ch4_ks_clothbag", 80, 262, 12, 72, 236));
    }
}
