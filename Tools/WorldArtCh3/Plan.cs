namespace WorldArtCh3;

/// <summary>Composition of the Chapter 3 map: street at dusk (north) and the cellar shelter (south).</summary>
public static class Plan
{
    const string Before = "!ch3_took_cover", After = "ch3_took_cover";
    const string Dark = "!ch3_sheltered", Lit = "ch3_sheltered";
    static int idc;
    static string Nm(string s) => $"{s.Replace("prop_c3_", "").Replace("fx_c3_", "").Replace("npc_c3_", "")}_{idc++}";

    /// <summary>Place a sprite by its bottom-centre pivot. cw/ch = collider footprint (bottom-centred); shadow: 0 none, 1 rect, 2 big ellipse, 3 small ellipse.</summary>
    public static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0)
    {
        Layout.Prop(Nm(sprite), sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1 && cw > 0) Layout.ShadowRect(x + cdx - cw / 2 + 2, y - ch + 1, cw, ch + 1, 0.8);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.12, y - 2, s.W * 0.40, 7, 0.9);
        else if (shadow == 3) Layout.ShadowEll(x + 2, y - 1, s.W * 0.42, 4, 0.8);
    }

    static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y", string show = "") => Layout.Anim($"{name}_{idc++}", frames, fps, x, y, sort, show);
    static string[] F(string stem, int n) => Enumerable.Range(0, n).Select(i => $"{stem}_{i}").ToArray();

    public static void Build()
    {
        Structure();
        StreetNorth();
        StreetRoad();
        StreetSouth();
        StairHead();
        Cellar();
        Lighting();
        Spots();
        Layout.PlayerStart = (404, 262);
        Layout.MustReach.Add(("street west", 62, 232));
        Layout.MustReach.Add(("street east", 660, 246));
        Layout.MustReach.Add(("north pavement (door A)", 104, 148));
        Layout.MustReach.Add(("north pavement (door D)", 666, 148));
        Layout.MustReach.Add(("south pavement", 200, 296));
        Layout.MustReach.Add(("stair head", 470, 300));
        Layout.MustReach.Add(("stair middle", 470, 350));
        Layout.MustReach.Add(("cellar landing", 470, 400));
        Layout.MustReach.Add(("cellar west bay", 100, 450));
        Layout.MustReach.Add(("cellar east bay", 660, 520));
        Layout.MustReach.Add(("cellar south", 460, 504));
    }

    // ---- fixed geometry ---------------------------------------------------------------------------------------
    static void Structure()
    {
        Layout.Col(0, 0, G.W, 138);                          // the row of houses
        Layout.Col(0, 0, 14, G.LedgeY);                      // west end of the street
        Layout.Col(G.W - 14, 0, 14, G.LedgeY);               // east end
        Layout.Col(0, G.LedgeY - 2, G.StairX0, G.StairBot - G.LedgeY + 2);                        // ledge + wall, west of the stairs
        Layout.Col(G.StairX1, G.LedgeY - 2, G.W - G.StairX1, G.StairBot - G.LedgeY + 2);          // ... and east
        Layout.Col(0, G.StairBot, G.VaultX0 + 8, G.H - G.StairBot);                               // west vault wall + rock
        Layout.Col(G.VaultX1 - 8, G.StairBot, G.W - G.VaultX1 + 8, G.H - G.StairBot);             // east
        Layout.Col(0, G.VaultFloorEnd, G.W, G.H - G.VaultFloorEnd);                               // south wall
    }

    // ---- north pavement -----------------------------------------------------------------------------------------
    static void StreetNorth()
    {
        foreach (var (lx, ly) in new[] { (96, 156), (330, 156), (560, 156), (704, 156) })
        {
            Put("prop_c3_streetlamp_lit", lx, ly, "y", Before, 6, 4, 0, 3);
            Put("prop_c3_streetlamp_dim", lx, ly, "y", After);
        }
        Put("prop_c3_tree_broad", 172, 154, "y", "", 10, 6, 0, 2);
        Put("prop_c3_tree_broad", 420, 154, "y", "", 10, 6, 0, 2);
        Put("prop_c3_tree_broad", 614, 154, "y", "", 10, 6, 0, 2);
        Put("prop_c3_plant_pot", 80, 150, "y", "", 12, 8, 0, 3);
        Put("prop_c3_plant_pot", 128, 150, "y", "", 12, 8, 0, 3);
        Put("prop_c3_chair_front", 212, 150, "y", "", 10, 6, 0, 3);
        Put("prop_c3_pot_red", 258, 150, "y", "", 10, 6, 0, 3);
        Put("prop_c3_siren", 282, 156, "y", "", 6, 4, 0, 3);
        Put("prop_c3_bench", 536, 152, "y", "", 38, 8, 0, 1);
        Put("prop_c3_pot_red", 644, 150, "y", "", 10, 6, 0, 3);
        Put("prop_c3_crate_b", 700, 152, "y", "", 20, 10, 0, 1);
        Put("prop_c3_crate_a", 684, 150, "y", "", 16, 8, 0, 1);
        Put("prop_c3_bush_round", 36, 152, "y", "", 22, 8, 0, 3);
        Put("prop_c3_grass_tall_a", 300, 156);
        Put("prop_c3_grass_tall_b", 590, 157);
        Put("prop_c3_chimney", 36, 42);
        Put("prop_c3_chimney", 214, 56);
        Put("prop_c3_chimney", 520, 48);
        Put("prop_c3_chimney", 622, 40);
        foreach (var (x, y) in new[] { (214, 28), (622, 12) }) Anim("chimney_smoke", F("fx_c3_chimney_smoke", 4), 4, x, y, "above");
        Anim("sheet_a", F("prop_c3_cloth_sheet", 2), 1, 60, 88, "y");
        Anim("sheet_b", F("prop_c3_cloth_sheet", 2), 1, 152, 88, "y");
        Anim("shirts", F("prop_c3_cloth_shirts", 2), 1, 42, 116, "y");
        Anim("awning", F("prop_c3_awning", 2), 2, 666, 117, "y");
        Anim("cat", new[] { "cat_sleep_0", "cat_sleep_1" }, 1, 310, 148, "y", Before);
    }

    // ---- road --------------------------------------------------------------------------------------------------
    static void StreetRoad()
    {
        foreach (var y in new[] { 186, 224, 262, 300 }) Put("prop_c3_sandbags_ns", 28, y);
        Layout.Col(14, 138, 26, G.LedgeY - 138 - 2);
        Layout.ShadowRect(40, 150, 5, 150, 0.8);
        Put("prop_c3_crate_a", 58, 272, "y", "", 16, 8, 0, 1);
        Put("prop_c3_sacks", 68, 192, "y", "", 30, 10, 0, 1);
        Put("prop_c3_handcart", 130, 246, "y", "", 34, 9, 0, 3);
        Put("prop_c3_apples", 168, 252, "floor");
        Put("prop_c3_bucket", 96, 256, "y", "", 8, 4);
        Put("prop_c3_cover", 302, 218, "y", "", 50, 12, 0, 1);
        Put("prop_c3_wheelbarrow", 214, 268, "y", "", 26, 8, 0, 3);
        Put("prop_c3_sacks", 478, 214, "y", "", 30, 10, 0, 1);
        Put("prop_c3_crate_b", 504, 222, "y", "", 20, 10, 0, 1);
        Put("prop_c3_rock_c", 450, 196, "y", "", 22, 8, 0, 3);
        Put("prop_c3_sandbags_wide", 228, 196, "y", "", 48, 10, 0, 1);
        Put("prop_c3_sandbags_low", 566, 254, "y", "", 30, 8, 0, 1);
        Put("prop_c3_woodstack", 622, 268, "y", "", 40, 10, 0, 1);
        Layout.Col(702, 138, G.W - 702, G.LedgeY - 138 - 2);
        Put("prop_c3_rubble_old_big", 724, 200, "y", "", 52, 20, 0, 2);
        Put("prop_c3_wall_broken", 730, 250, "y", "", 40, 8, 0, 1);
        Put("prop_c3_rubble_old_b", 720, 280, "y", "", 30, 10, 0, 3);
        Put("prop_c3_rubble_old_a", 722, 158, "y", "", 24, 8, 0, 3);
        Put("prop_c3_rubble_old_a", 690, 232, "y", "", 22, 6, 0, 3);
        Put("prop_c3_rock_a", 640, 214, "y", "", 14, 5);
        Put("prop_c3_rock_b", 120, 200, "y", "", 8, 4);
        // after the first blast
        // building materials stand in front of the empty house; after the blast the same spot is rubble (shared collider)
        Put("prop_c3_materials", 356, 158, "y", Before, 62, 16, 0, 1);
        Put("prop_c3_rubble_new_big", 356, 158, "y", After, 0, 0, 0, 2);
        Put("prop_c3_rubble_new_b", 400, 172, "floor", After);
        Put("prop_c3_rubble_new_c", 316, 174, "floor", After);
        Put("prop_c3_rubble_new_a", 266, 166, "floor", After);
        foreach (var (x, y, v) in new[] { (348, 196, 1), (420, 206, 2), (240, 222, 0), (512, 188, 3), (176, 214, 2), (590, 226, 1), (640, 176, 0), (470, 262, 2), (340, 254, 3), (110, 220, 1) })
            Put($"prop_c3_debris_{v}", x, y, "floor", After);
        foreach (var (x, y, v) in new[] { (356, 162, 2), (320, 182, 0), (500, 200, 1), (250, 250, 0) })
            Put($"fx_c3_dust_patch_{v}", x, y, "floor", After);
        Put("prop_c3_dropped_basket", 338, 250, "y", After);
        Put("prop_c3_scarf", 268, 262, "floor", After);
        Put("prop_c3_toy_rabbit", 372, 238, "y", After);
        Put("prop_c3_shutter_fallen", 240, 154, "floor", After);
        Put("prop_c3_shutter_fallen", 600, 152, "floor", After);
        Put("fx_c3_glass_shards", 276, 152, "floor", After);
        Put("fx_c3_glass_shards", 440, 150, "floor", After);
        Put("fx_c3_glass_shards", 706, 150, "floor", After);
    }

    // ---- south pavement --------------------------------------------------------------------------------------------
    static void StreetSouth()
    {
        Put("prop_c3_tree_cypress", 62, 304, "y", "", 8, 6, 0, 3);
        Put("prop_c3_bush_round", 132, 304, "y", "", 22, 8, 0, 3);
        Put("prop_c3_bush_round", 28, 300, "y", "", 22, 8, 0, 3);
        Put("prop_c3_bench", 330, 306, "y", "", 38, 8, 0, 1);
        Put("prop_c3_streetlamp_lit", 384, 306, "y", Before, 6, 4, 0, 3);
        Put("prop_c3_streetlamp_dim", 384, 306, "y", After);
        Put("prop_c3_rain_barrel", 424, 307, "y", "", 18, 8, 0, 3);
        Put("prop_c3_crate_a", 410, 307, "y", "", 16, 8, 0, 1);
        Put("prop_c3_tree_cypress", 538, 304, "y", "", 8, 6, 0, 3);
        Put("prop_c3_stonewall_a", 590, 308, "y", "", 44, 8, 0, 1);
        Put("prop_c3_wall_broken", 636, 308, "y", "", 40, 8, 0, 1);
        Put("prop_c3_rubble_old_a", 668, 306, "y", "", 22, 8, 0, 3);
        Put("prop_c3_stonewall_b", 692, 308, "y", "", 28, 8, 0, 1);
        Put("prop_c3_grass_tall_a", 566, 308);
        Put("prop_c3_grass_tall_b", 466, 309);
    }

    // ---- the stairwell head ---------------------------------------------------------------------------------------
    static void StairHead()
    {
        Put("prop_c3_stair_post", 449, 316, "y");
        Put("prop_c3_stair_post", 491, 316, "y");
        Put("prop_c3_stair_lintel", 470, 276, "above");
        Put("prop_c3_stair_lantern_off", 485, 296, "above", Dark);
        Anim("stair_lantern", F("prop_c3_stair_lantern_on", 2), 3, 485, 296, "above", Lit);
        Put("prop_c3_cellar_door_open", 459, 384, "y");
    }

    // ---- the cellar -------------------------------------------------------------------------------------------------
    static void Cellar()
    {
        Put("prop_c3_shelf_jars", 110, 398, "y", "", 40, 10, 0, 1);
        Put("prop_c3_in_kitchen_shelf", 160, 398, "y", "", 40, 10, 0, 1);
        Put("prop_c3_crates_stack", 232, 400, "y", "", 32, 12, 0, 1);
        Put("prop_c3_jugs", 272, 398, "y", "", 22, 8, 0, 3);
        Put("prop_c3_in_basket", 306, 398, "y", "", 14, 6, 0, 3);
        Put("prop_c3_in_kitchen_shelf", 524, 398, "y", "", 40, 10, 0, 1);
        Put("prop_c3_shelf_jars", 570, 398, "y", "", 40, 10, 0, 1);
        Put("prop_c3_crates_stack", 618, 400, "y", "", 32, 12, 0, 1);
        Put("prop_c3_in_rain_barrel", 654, 404, "y", "", 18, 8, 0, 3);
        Put("prop_c3_in_rain_barrel", 676, 404, "y", "", 18, 8, 0, 3);
        Put("prop_c3_pillar", 190, 470, "y", "", 12, 8, 0, 3);
        Put("prop_c3_pillar", 540, 470, "y", "", 12, 8, 0, 3);
        Put("prop_c3_blanket_crate", 424, 414, "y", "", 18, 8, 0, 1);
        // west bay: bedding, the children, a family with a baby
        Put("prop_c3_mat", 300, 496, "floor");
        Put("prop_c3_bedroll_0", 246, 508, "floor");
        Put("prop_c3_bedroll_1", 352, 510, "floor");
        Put("prop_c3_bedroll_2", 300, 524, "floor");
        Put("prop_c3_blanket_pile", 258, 480, "floor");
        Anim("children_huddle", F("npc_c3_children_huddle", 2), 1, 300, 484, "y");
        Layout.Col(286, 474, 28, 10);
        Put("prop_c3_in_stool", 350, 476, "y", "", 10, 6, 0, 3);
        Put("prop_c3_candle", 372, 492, "y");
        Put("prop_c3_crate_b", 122, 468, "y", "", 20, 10, 0, 1);
        Anim("mother", F("npc_c3_mother", 2), 1, 123, 472, "y");
        Put("prop_c3_suitcase", 96, 494, "y", "", 16, 8, 0, 1);
        Put("prop_c3_bundle", 150, 496, "y", "", 12, 6, 0, 3);
        Put("prop_c3_blanket_pile", 120, 516, "floor");
        Put("prop_c3_jugs", 410, 524, "y", "", 22, 8, 0, 3);
        Put("prop_c3_in_bucket", 436, 520, "y", "", 10, 5);
        // east bay: the bench with an old neighbour, a table, sacks
        Put("prop_c3_in_bench", 604, 450, "y", "", 38, 8, 0, 1);
        Anim("elder", F("npc_c3_elder", 2), 1, 600, 453, "y");
        Put("prop_c3_in_table_empty", 664, 496, "y", "", 40, 12, 0, 1);
        Put("prop_c3_in_chair_left", 636, 500, "y", "", 10, 6, 0, 3);
        Put("prop_c3_in_sacks", 676, 440, "y", "", 30, 10, 0, 1);
        Put("prop_c3_bedroll_1", 560, 516, "floor");
        Put("prop_c3_blanket_pile", 520, 506, "floor");
        Put("prop_c3_in_basket", 494, 520, "y", "", 14, 6, 0, 3);
        Put("prop_c3_in_rug_kitchen", 604, 478, "floor");
        foreach (var (lx, ly) in new[] { (336, 442), (566, 442) })
        {
            Put("prop_c3_lantern_hang_off", lx, ly, "above", Dark);
            Anim("lantern_hang", F("prop_c3_lantern_hang_on", 2), 3, lx, ly, "above", Lit);
        }
    }

    // ---- light, smoke, dust ------------------------------------------------------------------------------------------
    static void Lighting()
    {
        foreach (var (lx, ly) in new[] { (96, 156), (330, 156), (560, 156), (704, 156), (384, 306) })
        {
            Put("fx_c3_lampglow", lx, ly + 26, "floor", Before);
            Put("fx_c3_lamphalo", lx, ly - 24, "above", Before);
        }
        foreach (var (cx, top) in new[] { (108, 66), (240, 66), (438, 100), (666, 66) })
        {
            Anim("window_flicker", F("fx_c3_window_flicker", 2), 3, cx, top + 22, "floor", Before);
            Put("fx_c3_window_dark", cx, top + 22, "floor", After);
        }
        Anim("window_flicker", F("fx_c3_window_flicker", 2), 2, 356, 88, "floor", Before);
        foreach (var (cx, top) in new[] { (148, 66), (276, 66), (438, 66), (706, 66) })
            Put("fx_c3_window_broken", cx, top + 22, "floor", After);
        foreach (var (cx, by, v) in new[] { (50, 96, 0), (141, 98, 1), (225, 106, 2), (469, 98, 3), (543, 130, 1), (632, 92, 4), (722, 96, 0), (280, 112, 4) })
            Put($"fx_c3_crack_{v}", cx, by, "floor", After);
        Anim("smoke_ruin", F("fx_c3_smoke", 4), 3, 356, 66, "above", After);
        Anim("smoke_ruin2", F("fx_c3_smoke", 4), 2, 388, 76, "above", After);
        foreach (var (x, y) in new[] { (100, 34), (250, 46), (490, 36), (700, 28) }) Anim("plume_far", F("fx_c3_plume_far", 4), 2, x, y, "above", After);
        foreach (var (x, y) in new[] { (190, 214), (340, 190), (520, 232), (650, 196), (420, 262) })
            Anim("dust_cloud", F("fx_c3_dust_cloud", 3), 1, x, y + 30, "above", After);
        foreach (var (x, y) in new[] { (96, 150), (330, 150), (560, 150), (704, 150), (384, 300) }) Anim("motes", F("fx_c3_motes", 3), 3, x, y + 24, "above", Before);
        foreach (var (x, y) in new[] { (336, 456), (566, 456), (300, 500), (470, 420) }) Anim("motes", F("fx_c3_motes", 3), 3, x, y + 24, "above");
        foreach (var (x, y) in new[] { (140, 402), (260, 440), (380, 436), (520, 432), (640, 440) }) Anim("ceiling_dust", F("fx_c3_ceiling_dust", 3), 4, x, y + 20, "above", After);
        foreach (var (lx, ly) in new[] { (336, 470), (566, 468) })
        {
            Anim("lantern_glow", F("fx_c3_lantern_glow", 2), 3, lx, ly + 50, "floor", Lit);
            Put("fx_c3_lantern_halo", lx, ly - 44, "above", Lit);
        }
        Put("fx_c3_door_glow", 470, 436, "floor", Lit);
    }

    // ---- interactables ---------------------------------------------------------------------------------------------
    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch3_alarm", 302, 240, 12, 302, 176, "ch3_took_cover"));
        Layout.Spots.Add(new Spot("ch3_shelter", 470, 298, 12, 472, 266, "ch3_sheltered"));
        Layout.Spots.Add(new Spot("ch3_wait", 600, 474, 12, 600, 424, ""));
        Layout.Spots.Add(new Spot("ch3_counting", 300, 504, 12, 300, 452, ""));
        Layout.Spots.Add(new Spot("ch3_ks_blanket", 424, 440, 12, 424, 394, ""));
    }
}
