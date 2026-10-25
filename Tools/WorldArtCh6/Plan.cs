namespace WorldArtCh6;

/// <summary>Composition of the Chapter 6 map: where every prop sits, collision rectangles, lights, interactables.</summary>
public static class Plan
{
    const string Unsettled = "!ch6_settled", Settled = "ch6_settled";
    const string NoWork = "!ch6_work_found", Worked = "ch6_work_found";

    static int idc;
    static string Nm(string s) => $"{s.Replace("prop_c6_", "").Replace("prop_", "")}_{idc++}";

    static readonly Rc Room = new(48, 96, 248, 200), Lobby = new(312, 96, 144, 200);

    /// <summary>Place a sprite by its bottom-centre pivot. cw/ch = collider footprint (bottom-centred), shadow: 0 none, 1 rect under footprint, 2 big ellipse, 3 small ellipse.</summary>
    static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0, int sdx = 0)
    {
        Layout.Prop(Nm(sprite), sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1 && cw > 0) Layout.ShadowRect(x + cdx - cw / 2 + 2, y - ch + 1, cw, ch + 1, 0.8);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.12 + sdx, y - 2, s.W * 0.40, 7, 0.9);
        else if (shadow == 3) Layout.ShadowEll(x + 2 + sdx, y - 1, s.W * 0.42, 4, 0.8);
    }

    static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y", string show = "") => Layout.Anim(name, frames, fps, x, y, sort, show);
    static string[] F(string baseName, int n) => Enumerable.Range(0, n).Select(i => $"{baseName}_{i}").ToArray();

    public static void Build()
    {
        S.Cur = "reused";
        Walls();
        RoomFurniture();
        Lobby_();
        Facade();
        Roofs();
        Street_();
        Characters();
        Lighting();
        Spots();
        Layout.PlayerStart = (270, 262);
        Layout.MustReach.Add(("room door", 304, 240));
        Layout.MustReach.Add(("lobby", 340, 200));
        Layout.MustReach.Add(("lobby south", 350, 272));
        Layout.MustReach.Add(("front door", 384, 316));
        Layout.MustReach.Add(("sidewalk west", 70, 346));
        Layout.MustReach.Add(("street west", 60, 400));
        Layout.MustReach.Add(("street east", 700, 400));
        Layout.MustReach.Add(("far sidewalk", 400, 448));
        Layout.MustReach.Add(("alley", 484, 140));
        Layout.MustReach.Add(("shop doorway", 600, 350));
        Layout.MustReach.Add(("bed corner", 120, 250));
        Layout.MustReach.Add(("kitchen", 140, 140));
        Layout.MustReach.Add(("window desk", 200, 152));
        Layout.MustReach.Add(("table south", 196, 272));
        Layout.MustReach.Add(("room east", 275, 150));
    }

    // ---- walls, caps and map edges ------------------------------------------------------------------------------
    static void Walls()
    {
        Layout.Col(0, 0, 768, 48);                        // rear roofs and back wall
        Layout.Col(32, 48, 440, 48);                      // eave + north wall of building A
        Layout.Col(32, 96, 16, 200);                      // west wall
        Layout.Col(456, 96, 16, 200);                     // east wall
        Layout.Col(Geo.PartX, 96, Geo.PartW, Geo.DoorY0 - 96);          // partition above the door
        Layout.Col(Geo.PartX, Geo.DoorY1, Geo.PartW, 296 - Geo.DoorY1); // partition below the door
        Layout.Col(32, 296, Geo.FrontX0 - 32, 40);        // south wall west of the entrance
        Layout.Col(Geo.FrontX1, 296, 472 - Geo.FrontX1, 40);
        Layout.Col(0, 48, 32, 288);                       // west neighbour
        Layout.Col(736, 48, 32, 288);                     // east neighbour
        Layout.Col(Geo.BX0, 48, Geo.BX1 - Geo.BX0, 288);  // shop
        Layout.Col(Geo.AlleyX0, 48, 24, 24);              // dead end of the alley
        Layout.Col(0, 456, 768, 56);                      // buildings across the street
        Layout.Col(0, 336, 6, 120); Layout.Col(762, 336, 6, 120);   // the street runs on beyond the map
        Layout.Col(402, 96, 52, 38);                      // staircase
    }

    // ---- the room ------------------------------------------------------------------------------------------------
    static void RoomFurniture()
    {
        // rugs first (flat)
        Put(Sprites.In("prop_rug_dining"), 196, 264, "floor");
        Put(Sprites.In("prop_rug_bedroom"), 100, 226, "floor");

        // kitchenette along the north wall
        Put(Sprites.In("prop_counter_sink"), 76, 108, "y", "", 48, 12, 0, 1);
        Put(Sprites.In("prop_stove"), 112, 108, "y", "", 24, 12, 0, 1);
        Put(Sprites.In("prop_kitchen_shelf"), 148, 112, "y", "", 44, 16, 0, 1);
        Put(Sprites.In("prop_veg_crate"), 66, 130, "y", "", 16, 7, 0, 3);
        Put(Sprites.In("prop_bucket"), 134, 130, "y", "", 10, 5, 0, 3);

        // window desk: the children's homework place
        Put(Sprites.In("prop_desk"), 200, 120, "y", "", 40, 14, 0, 1);
        Put(Sprites.In("prop_stool"), 200, 136, "y", "", 10, 6, 0, 3);
        Put("prop_c6_schoolbag", 176, 140, "y", "", 12, 5, 0, 3);

        // wardrobe, lamp and drying rack in the north-east
        Put(Sprites.In("prop_wardrobe"), 250, 112, "y", "", 34, 16, 0, 1);
        Put(Sprites.In("prop_floorlamp"), 284, 112, "y", "", 8, 6, 0, 3);
        Anim("dryrack", F("prop_c6_dryrack", 2), 1, 258, 184, "y");
        Layout.Col(258 - 15, 184 - 8, 30, 8);
        Layout.ShadowRect(258 - 13, 184 - 7, 30, 9, 0.7);

        // sleeping corner: two beds, with the packed bags at their feet until the room is settled
        Put("prop_c6_bed_bare", 76, 206, "y", Unsettled, 38, 36, 0, 1);
        Put("prop_c6_bed_made", 76, 206, "y", Settled);
        Put(Sprites.In("prop_bed_child"), 124, 206, "y", "", 26, 24, 0, 1);
        Put(Sprites.In("prop_nightstand"), 100, 170, "y", "", 14, 10, 0, 3);
        Put("prop_c6_bags", 80, 240, "y", Unsettled, 38, 10, 0, 1);
        Put("prop_c6_bags_stowed", 80, 240, "y", Settled);
        Put(Sprites.In("prop_plant_small"), 62, 292, "y", "", 10, 6, 0, 3);

        // the small table: three things and a few coins, with the children's chairs
        Put("prop_c6_table_decision", 196, 246, "y", "", 46, 20, 0, 1);
        Put(Sprites.In("prop_chair_left"), 158, 244, "y", "", 12, 8);
        Put(Sprites.In("prop_chair_right"), 235, 244, "y", "", 12, 8);
        Put("prop_c6_hanglamp", 196, 198, "above");

        // by the door: key table and a coat rack
        Put("prop_c6_keytable", 282, 222, "y", "", 14, 6, 0, 3);
        Put(Sprites.In("prop_coatrack"), 285, 290, "y", "", 12, 6, 0, 3);
        Put(Sprites.In("prop_basket"), 242, 288, "y", "", 14, 6, 0, 3);
    }

    // ---- stair lobby -------------------------------------------------------------------------------------------------
    static void Lobby_()
    {
        Put("prop_c6_runner", 384, 292, "floor");
        Put(Sprites.In("prop_doormat", "doormat_in"), 334, 106, "floor");
        Put("prop_c6_shoerack", 334, 120, "y", "", 20, 6, 0, 3);
        Put("prop_c6_bicycle", 372, 142, "y", "", 30, 6, 0, 3);
        Put("prop_c6_banister", 401, 136, "y");
        Put("prop_c6_door_lock", 327, 226, "y", "", 28, 5, 0, 0);
        Layout.ShadowRect(314, 224, 30, 7, 0.7);
        Put(Sprites.Out("prop_plant_pot", "plant_lobby"), 446, 292, "y", "", 12, 8, 0, 3);
        Put(Sprites.In("prop_bench", "bench_lobby"), 428, 236, "y", "", 38, 8, 0, 1);
        Put(Sprites.In("prop_plant_small", "plant_lobby_small"), 326, 292, "y", "", 10, 6, 0, 3);
        Put(Sprites.In("prop_doormat", "doormat_front"), 384, 320, "floor");
        Put("prop_c6_hanglamp", 384, 204, "above");
    }

    // ---- facade, step and entrance --------------------------------------------------------------------------------------
    static void Facade()
    {
        // window boxes under the lit windows
        Put(Sprites.Out("prop_windowbox_a"), 98, 338, "y", "", 28, 5);
        Put(Sprites.Out("prop_windowbox_b"), 224, 338, "y", "", 22, 5);
        Put(Sprites.Out("prop_windowbox_a", "windowbox_a2"), 430, 338, "y", "", 28, 5);
        // plants flanking the entrance steps
        Put(Sprites.Out("prop_plant_pot"), 344, 350, "y", "", 12, 6, 0, 3);
        Put(Sprites.Out("prop_pot_blue"), 410, 350, "y", "", 10, 5, 0, 3);
        // shop frontage
        Put("prop_c6_clothrail", 520, 352, "y", "", 34, 6, 0, 3);
        Put(Sprites.Out("prop_doormat", "doormat_shop"), 600, 346, "floor");
        Put("prop_c6_notice", 638, 352, "y", NoWork, 22, 6, 0, 3);
        Put("prop_c6_notice_taken", 638, 352, "y", Worked);
        Put(Sprites.Out("prop_plant_pot", "plant_shop"), 672, 350, "y", "", 12, 6, 0, 3);
        Put(Sprites.Out("prop_pot_red"), 716, 350, "y", "", 10, 5, 0, 3);
        Put("prop_c6_sign_spool", 512, 296, "y");
        // alley
        Put("prop_c6_bin", 484, 84, "y", "", 14, 6, 0, 3);
        Put("prop_c6_alley_line_0", 484, 196, "above");
    }

    static void Roofs()
    {
        // shop roof
        Put(Sprites.Out("prop_chimney"), 706, 134);
        Put("prop_c6_tank", 560, 122);
        Put("prop_c6_antenna", 528, 100);
        Put("prop_c6_skylight", 596, 182);
        Put("prop_c6_skylight", 668, 172);
        Put("prop_c6_vent", 706, 214);
        Put("prop_c6_vent", 522, 238);
        // roofs behind the building
        Put("prop_c6_antenna", 190, 36);
        Put("prop_c6_antenna", 640, 36);
        Put(Sprites.Out("prop_chimney"), 340, 34);
        Put(Sprites.Out("prop_chimney"), 110, 34);
        Put(Sprites.Out("prop_chimney"), 560, 34);
        Put("prop_c6_vent", 450, 34);
        // west and east neighbours
        Put(Sprites.Out("prop_chimney"), 16, 140);
        Put("prop_c6_antenna", 752, 120);
        Put("prop_c6_vent", 752, 240);
        // roofs across the street
        Put(Sprites.Out("prop_chimney"), 96, 500);
        Put(Sprites.Out("prop_chimney"), 330, 498);
        Put(Sprites.Out("prop_chimney"), 612, 500);
        Put("prop_c6_antenna", 480, 506);
        Put("prop_c6_vent", 210, 500);
        Put("prop_c6_vent", 700, 500);
    }

    // ---- street ------------------------------------------------------------------------------------------------------------
    static void Street_()
    {
        foreach (var x in new[] { 136, 332, 438, 560, 700 })
        {
            Anim($"lamp_{x}", F("prop_c6_lamppost", 2), 2, x, 356, "y");
            Anim($"halo_{x}", F("fx_c6_halo", 2), 2, x, 322, "above");
            Layout.Col(x - 4, 351, 8, 5);
            Layout.ShadowEll(x + 4, 355, 9, 3, 0.9);
            Ground.LightSource(x, 348, 56, 26, 1.0);
            Ground.LightSource(x, 396, 44, 30, 0.4);
        }
        // laundry strung on the shop roof (re-tinted from Chapter 1)
        Anim("roof_laundry", new[] { Sprites.Out("prop_laundry_0", "laundry_0"), Sprites.Out("prop_laundry_1", "laundry_1") }, 1, 620, 244, "y");
        Put(Sprites.Out("prop_bench"), 252, 354, "y", "", 38, 8, 0, 1);
        Put(Sprites.Out("prop_veg_crate"), 454, 350, "y", "", 16, 7, 0, 3);
        Put(Sprites.Out("prop_sacks"), 20, 352, "y", "", 30, 8, 0, 1);
        Put("prop_c6_handcart", 214, 392, "y", "", 32, 7, 0, 1);
    }

    // ---- people, animals, moving things --------------------------------------------------------------------------------------
    static void NpcAt(string nm, int x, int y, string show = "")
    {
        Anim(nm, F("npc_c6_" + nm, 2), 1, x, y, "y", show);
        Layout.Col(x - 4, y - 4, 8, 4);
    }

    static void Characters()
    {
        // the children at the table
        Anim("child_a_table", new[] { "child_a_sit_right_0", "child_a_sit_right_1" }, 2, 159, 245, "y");
        Anim("child_b_table", new[] { "child_b_sit_left_0", "child_b_sit_left_1" }, 2, 233, 245, "y");
        // the cat that has adopted the front step
        Anim("cat_step", new[] { "cat_sleep_0", "cat_sleep_1" }, 1, 366, 346, "y");
        // neighbours and passers-by
        NpcAt("neighbour", 352, 176);
        NpcAt("woman_basket", 292, 350);
        NpcAt("man_coat", 548, 394);
        NpcAt("youth", 80, 350);
        NpcAt("shopkeeper", 600, 334, Worked);
        // life: steam, smoke, birds, moths
        Anim("steam", F("fx_c6_steam", 4), 5, 108, 76, "above");
        var smoke = F("prop_smoke", 4);
        Anim("smoke_shop", smoke, 4, 706, 104, "above");
        Anim("smoke_rear", smoke, 4, 340, 28, "above");
        Anim("smoke_front", smoke, 4, 330, 470, "above");
        Anim("smoke_front2", smoke, 4, 612, 472, "above");
        Anim("pigeon_a", F("fx_c6_pigeon", 2), 2, 306, 376, "y");
        Anim("pigeon_b", F("fx_c6_pigeon", 2), 3, 318, 382, "y");
        Anim("pigeon_c", F("fx_c6_pigeon", 2), 2, 500, 350, "y");
        Anim("bird_a", F("fx_c6_bird", 2), 3, 250, 440, "above");
        Anim("bird_b", F("fx_c6_bird", 2), 4, 262, 432, "above");
        Anim("bird_c", F("fx_c6_bird", 2), 3, 580, 440, "above");
        foreach (var x in new[] { 136, 332, 560 })
            Anim($"moth_{x}", F("fx_c6_moth", 2), 6, x, 306, "above");
        Anim("moth_hall", F("fx_c6_moth", 2), 7, 386, 186, "above");
        Anim("alley_line", F("prop_c6_alley_line", 2), 1, 484, 196, "above");
    }

    // ---- lights and cast shadows (baked into the ground) -----------------------------------------------------------------------
    static void Lighting()
    {
        // room: the hanging lamp over the table is the warmest, brightest place in the whole map
        Ground.LightSource(196, 228, 70, 54, 1.05, Room);
        Ground.LightSource(284, 126, 34, 30, 0.6, Room);
        Ground.LightSource(100, 176, 32, 26, 0.5, Room);
        Ground.LightSource(112, 118, 22, 14, 0.4, Room);
        Ground.LightCool(200, 124, 28, 44, 0.9, Room);
        Ground.LightSource(304, 240, 26, 22, 0.55);
        // lobby
        Ground.LightSource(384, 196, 62, 58, 0.85, Lobby);
        Ground.LightSource(428, 114, 30, 24, 0.35, Lobby);
        // interior wall shadows (light comes from the upper left)
        Layout.ShadowRect(48, 96, 248, 4, 0.55);
        Layout.ShadowRect(312, 96, 144, 4, 0.55);
        Layout.ShadowRect(48, 100, 4, 196, 0.4);
        Layout.ShadowRect(312, 100, 7, 124, 0.55);
        Layout.ShadowRect(312, 256, 7, 40, 0.55);
        Layout.ShadowRect(48, 292, 248, 4, 0.4);
        Layout.ShadowRect(312, 292, 144, 4, 0.4);
        // buildings cast onto the pavement
        Layout.ShadowRect(0, 336, 368, 5, 0.9);
        Layout.ShadowRect(400, 336, 72, 5, 0.9);
        Layout.ShadowRect(496, 336, 272, 5, 0.9);
        Layout.ShadowRect(472, 300, 4, 36, 0.7);
        Layout.ShadowRect(496, 72, 4, 264, 0.6);
    }

    // ---- interactables -----------------------------------------------------------------------------------------------------------
    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch6_settle", 100, 262, 12, 84, 206, "ch6_settled"));
        Layout.Spots.Add(new Spot("ch6_work", 638, 376, 12, 638, 316, "ch6_work_found"));
        Layout.Spots.Add(new Spot("ch6_decision", 196, 272, 12, 196, 196, ""));
        Layout.Spots.Add(new Spot("ch6_lock", 328, 248, 12, 328, 188, ""));
        Layout.Spots.Add(new Spot("ch6_window", 200, 156, 12, 200, 80, ""));
        Layout.Spots.Add(new Spot("ch6_ks_key", 270, 246, 12, 282, 204, ""));
    }
}
