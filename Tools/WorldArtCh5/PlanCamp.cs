namespace WorldArtCh5;

public static partial class Plan
{
    static readonly Dictionary<string, (int cw, int ch, int sh)> ItemDims = new()
    {
        ["prop_c5_bundles"] = (30, 10, 1), ["prop_c5_suitcases"] = (20, 8, 3), ["prop_c5_jerrycans"] = (22, 8, 3), ["prop_c5_blankets"] = (18, 6, 3),
        ["prop_sacks"] = (30, 10, 1), ["prop_veg_crate"] = (16, 8, 3), ["prop_c5_jar"] = (12, 6, 3), ["prop_c5_kindling"] = (14, 5, 3),
        ["prop_bucket"] = (8, 4, 0), ["prop_basket"] = (14, 6, 3), ["prop_basket_logs"] = (14, 6, 3), ["prop_rain_barrel"] = (20, 8, 3),
        ["prop_c5_jug"] = (8, 4, 3), ["prop_stool"] = (10, 6, 3),
    };

    static void Item(string sprite, int x, int y)
    {
        var (cw, ch, sh) = ItemDims[sprite];
        Put(sprite, x, y, "y", "", cw, ch, 0, sh);
    }

    static void Line(string v, int x, int y, bool swap = false)
    {
        var f = swap ? new[] { $"fx_c5_line_{v}_1", $"fx_c5_line_{v}_0" } : new[] { $"fx_c5_line_{v}_0", $"fx_c5_line_{v}_1" };
        Layout.Anim(Nm("line"), f, 1, x, y, "y");
        Layout.Col(x - 42, y - 5, 6, 5); Layout.Col(x + 38, y - 5, 6, 5);
        Layout.ShadowEll(x + 6, y - 1, 44, 3, 0.7);
    }

    static void Mat(string k, int x, int y) => Put("prop_c5_mat_" + k, x, y, "floor");

    // ---- tents, lines, belongings ---------------------------------------------------------------------------------
    static void Camp()
    {
        // northern row at the foot of the rise, each with its own little yard
        Tent("c", 150, 188); Tent("g", 240, 172); Tent("b", 322, 192); Tent("e", 406, 176); Tent("h", 494, 190); Tent("f", 568, 178);
        // --- yard of the big olive tent
        Mat("a", 150, 218);
        Item("prop_sacks", 100, 208); Item("prop_c5_jerrycans", 236, 232); Item("prop_c5_kindling", 112, 236);
        Fire(196, 240, false);
        Person("cook_sit", 168, 246, 2);
        Person("kid_play_a", 128, 226, 2);
        // --- grey/blue tent
        Item("prop_c5_jar", 214, 198); Item("prop_veg_crate", 276, 198); Item("prop_c5_bundles", 284, 218);
        Person("woman_stand", 258, 216);
        // --- small blue tent
        Mat("b", 326, 218);
        Item("prop_c5_suitcases", 296, 208);
        Person("man_sit", 334, 224);
        // --- teal/ochre tent
        Item("prop_c5_blankets", 372, 208); Mat("c", 408, 212);
        Person("woman_sit_b", 404, 222);
        Person("kid_play_c", 440, 216, 2);
        // --- tents near the pen
        Item("prop_sacks", 524, 210); Item("prop_c5_jerrycans", 462, 208);
        Item("prop_veg_crate", 544, 200); Item("prop_c5_bundles", 604, 200);
        Person("woman_sit_a", 580, 218);
        // washing lines (posts collide, clothes hang above head height)
        Line("a", 304, 276);
        Line("b", 488, 272, true);
        Person("woman_stand", 352, 284, 1);
        Put("prop_c5_blankets", 270, 290, "y", "", 18, 6, 0, 3);

        // --- west edge: windbreak, tents, tarps
        Put("prop_c5_windbreak", 64, 248, "y", "", 34, 6, 0, 3);
        Tent("d", 104, 302);
        Mat("c", 104, 324);
        Item("prop_c5_bundles", 62, 326); Item("prop_c5_blankets", 142, 318);
        Person("man_sit", 130, 336, 1);
        Tent("a", 182, 358);
        Item("prop_c5_suitcases", 142, 354); Item("prop_c5_jerrycans", 222, 352); Item("prop_basket", 160, 376);
        Put("prop_c5_windbreak_b", 62, 380, "y", "", 26, 6, 0, 3);
        Item("prop_sacks", 74, 408);
        Put("prop_c5_tarp_a", 118, 440, "y", "", 48, 12, 0, 4);
        Person("woman_stand", 158, 412, 1);
        Item("prop_c5_kindling", 190, 426);
        Item("prop_stool", 200, 392);

        // --- inner yards between the tents and the plaza
        Item("prop_c5_jar", 236, 390);
        Put("prop_c5_tarp_c", 262, 440, "y", "", 40, 12, 0, 4);
        Mat("b", 266, 462);
        Person("kid_sit", 246, 472, 1);
        Item("prop_c5_jerrycans", 304, 446);

        // --- pen, cart, sign
        Put("prop_c5_sign", 612, 312, "y", "", 6, 5, 0, 3);
        for (int px = 514; px <= 578; px += 32) Put("prop_c5_fence_h", px, 292, "y", "", 30, 5, 0, 3);
        Put("prop_c5_fence_h", 514, 338, "y", "", 30, 5, 0, 3);
        Put("prop_c5_fence_h2", 588, 338, "y", "", 22, 5, 0, 3);
        for (int py = 304; py <= 338; py += 14) { Put("prop_c5_fence_post", 496, py, "y", "", 4, 4); Put("prop_c5_fence_post", 600, py, "y", "", 4, 4); }
        Put("prop_c5_fence_post", 496, 292, "y", "", 4, 4); Put("prop_c5_fence_post", 600, 292, "y", "", 4, 4);
        Person("donkey", 550, 324, 1, true);
        Item("prop_bucket", 518, 328);
        Put("prop_c5_kindling", 576, 328, "y");
        Put("prop_c5_handcart", 456, 336, "y", "", 34, 12, 4, 3);
        Item("prop_c5_bundles", 424, 296);

        // --- firewood corner
        Put("prop_woodstack", 612, 218, "y", "", 40, 10, 0, 1);
        Put("prop_chopping_block", 652, 224, "y", "", 12, 6, 0, 3);
        Put("prop_logs_split", 666, 236, "floor");
        Item("prop_c5_kindling", 636, 236);
        Person("man_stand", 640, 252, 1);
    }

    // ---- central fire with its gathering ------------------------------------------------------------------------------
    static void Plaza()
    {
        Fire(380, 346, true);
        Mat("d", 380, 394);
        Mat("a", 322, 356);
        Mat("b", 440, 352);
        Person("elder_sit", 330, 354);
        Person("man_sit", 360, 322);
        Person("woman_sit_b", 402, 322);
        Person("woman_sit_a", 424, 356);
        Person("cook_sit", 352, 382);
        Put("prop_c5_log_seat", 408, 384, "y", "", 22, 5, 0, 3);
        Put("prop_bench", 330, 318, "y", "", 34, 8, 0, 3);
        Person("man_stand", 304, 380, 1);
        Person("kid_play_a", 440, 410, 2);
        Person("kid_play_b", 458, 398, 2);
        Put("prop_c5_pebbles", 290, 424, "floor");
        Item("prop_basket_logs", 312, 334);
    }

    // ---- Lucille's family, the neighbours, and the south camp -----------------------------------------------------------
    const int ShDsCol = 18;

    static void Families()
    {
        // the patch of ground where the lean-to goes
        Mat("c", 150, 556);
        Layout.Prop(Nm("shelter"), "prop_c5_shelter_bare", 150, 524, "y", NotBuilt);
        Layout.Prop(Nm("shelter"), "prop_c5_shelter_done", 150, 524, "y", Built);
        Layout.Col(150 - 26, 524 - ShDsCol, 52, ShDsCol);
        Layout.ShadowEll(150 + 18, 522, 40, 8, 0.9);
        Item("prop_c5_bundles", 92, 520);
        Put("prop_c5_suitcases", 206, 524, "y", "", 20, 8, 0, 3);
        Item("prop_c5_jerrycans", 92, 560);
        Layout.Anim(Nm("kid_a"), new[] { "npc_c5_kid_a_0", "npc_c5_kid_a_1" }, 1, 120, 558, "y");
        Layout.Col(120 - 5, 558 - 4, 10, 4);
        Layout.Anim(Nm("kid_b"), new[] { "npc_c5_kid_b_0", "npc_c5_kid_b_1" }, 1, 186, 560, "y");
        Layout.Col(186 - 5, 560 - 4, 10, 4);
        Layout.ShadowEll(122, 557, 7, 3, 0.8); Layout.ShadowEll(188, 559, 7, 3, 0.8);

        // the neighbours: a mother and a small child by a little fire
        Fire(304, 542, false);
        Mat("b", 298, 568);
        Layout.Anim(Nm("mother_child"), new[] { "npc_c5_mother_child_0", "npc_c5_mother_child_1" }, 1, 278, 548, "y");
        Layout.Col(278 - 8, 548 - 4, 16, 4);
        Layout.ShadowEll(281, 547, 10, 3, 0.8);
        Item("prop_c5_jar", 336, 550);
        Put("prop_c5_foodcloth", 332, 574, "floor", Shared);
        Item("prop_c5_jar", 252, 528);
        Put("prop_c5_tarp_b", 374, 548, "y", "", 36, 12, 0, 4);
        Item("prop_c5_blankets", 238, 560);

        // another gathering in the south-east
        Fire(486, 560, false, false);
        Put("prop_c5_log_seat_s", 462, 558, "y", "", 16, 5, 0, 3);
        Put("prop_c5_log_seat_s", 512, 560, "y", "", 16, 5, 0, 3);
        Person("elder_sit", 462, 560);
        Person("man_sit", 512, 562);
        Item("prop_c5_bundles", 436, 580);
        Item("prop_c5_jerrycans", 540, 580);
        Item("prop_veg_crate", 548, 548);
        Put("prop_c5_windbreak", 478, 520, "y", "", 34, 6, 0, 3);
        Person("woman_sit_a", 566, 560);
        Tree("prop_c5_olive_a", 616, 548, 12);
    }

    // ---- the water --------------------------------------------------------------------------------------------------
    static void WaterSide()
    {
        Put("prop_c5_rock_c", 660, 400, "y", "", 22, 8, 0, 3);
        Put("prop_c5_rock_a", 640, 344, "y", "", 16, 6, 0, 3);
        Put("prop_c5_bucket_empty", 654, 380, "y", NotFound, 10, 5, 0, 3);
        Put("prop_c5_bucket_full", 654, 380, "y", Found);
        Item("prop_c5_jerrycans", 612, 412);
        Item("prop_rain_barrel", 568, 426);
        Item("prop_c5_jar", 592, 438);
        Person("woman_carry", 606, 356, 1);
        Mat("a", 560, 466);
        Put("prop_c5_tarp_d", 536, 484, "y", "", 40, 12, 0, 4);
        Person("kid_play_b", 580, 392, 2);
        // jug set down on the north bank
        Item("prop_c5_jug", 740, 238);
        Put("prop_c5_rock_b", 752, 248, "y", "", 10, 5);
        // far bank and edges
        Tree("prop_c5_olive_b", 614, 164, 10);
        Tree("prop_c5_cypress", 642, 132, 8);
        Tree("prop_c5_cypress", 812, 132, 8);
        Tree("prop_c5_olive_a", 818, 314, 12);
        Tree("prop_c5_olive_b", 776, 566, 10);
        Tree("prop_c5_cypress", 836, 524, 8);
        Put("prop_c5_boulder_a", 830, 404, "y", "", 28, 8, 0, 4);
        Put("prop_c5_boulder_b", 790, 258, "y", "", 20, 7, 0, 4);
        Put("prop_c5_boulder_b", 770, 424, "y", "", 20, 7, 0, 4);
        Put("prop_c5_scrub_c", 808, 484, "y", "", 26, 8, 0, 3);
        Put("prop_c5_scrub_a", 744, 600, "y", "", 20, 8, 0, 3);
    }
}
