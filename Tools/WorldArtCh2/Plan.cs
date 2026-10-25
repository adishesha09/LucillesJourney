namespace WorldArtCh2;

/// <summary>Composition of the Chapter 2 map: buildings, stalls, props, folk, colliders, shadows and interactables.</summary>
public static class Plan
{
    const string VegKey = "ch2_vegetables_bought", GreetKey = "ch2_neighbour_greeted";
    static Rgba H(string h) => Rgba.Hex(h);
    static readonly Rgba[] Sandy = { H("A8895E"), H("C8B58A"), H("DCCBA4"), H("E8D9B5"), H("F4EAD2") };

    public static readonly List<(int x, int y)> StallBases = new()
    {
        (164, 176), (262, 176), (540, 176), (640, 176), (176, 330), (624, 330), (270, 456), (536, 456),
    };

    // Base - Top = 100 (roof 32..36 + facade >= 60)
    public static readonly List<Buildings.Bld> Blds = new()
    {
        new(96, 96, 112, 20, "bakery", K.Peach, K.Roof, K.TealR, K.RedR, 2, false, 32),
        new(192, 76, 112, 16, "closed", Sandy, Buildings.Slate, Buildings.Ginger, K.Linen, 1, true, 36),
        new(268, 84, 112, 10, "store", K.BlueWall, Buildings.Ochre, K.LeafWarm, K.Mustard, 0, false, 36),
        new(448, 88, 112, 14, "tailor", K.Plaster, Buildings.Slate, K.RedR, K.BlueR, 2, false, 36),
        new(536, 80, 112, 18, "house", K.Peach, K.Roof, K.BlueR, K.RedR, 1, false, 34),
        new(616, 88, 112, 8, "store", Sandy, Buildings.Ochre, K.TealR, K.TealR, 2, false, 36),
        new(0, 96, 190, 90, "house", K.Plaster, K.Roof, K.TealR, K.RedR, 2, false, 36),
        new(0, 96, 360, 260, "house", K.Peach, Buildings.Slate, K.RedR, K.RedR, 1, false, 36),
        new(704, 96, 190, 90, "house", Sandy, Buildings.Ochre, K.BlueR, K.RedR, 0, false, 36),
        new(704, 96, 360, 260, "closed", K.Plaster, Buildings.Slate, Buildings.Ginger, K.Linen, 1, true, 36),
    };

    static int idc;
    static string Nm(string s) => $"{s}_{idc++}";

    /// <summary>Place by bottom-centre pivot. cw/ch = bottom-centred collider; shadow: 0 none, 1 box-cast, 2 tree, 3 small blob.</summary>
    static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0, double sh = 0)
    {
        Layout.Prop(Nm(sprite.Replace("prop_c2_", "").Replace("prop_", "")), sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1) Ground.BoxShadow(x + cdx - (cw > 0 ? cw / 2.0 : s.W * 0.4), y - Math.Max(ch, 3), x + cdx + (cw > 0 ? cw / 2.0 : s.W * 0.4), y, sh > 0 ? sh : s.H * 0.8, 0.8);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.16, y - 2, s.W * 0.42, 7, 0.9);
        else if (shadow == 3) Layout.ShadowEll(x + 3 + s.W * 0.06, y - 1, s.W * 0.42, 3.5, 0.8);
    }

    static void Tree(string sprite, int x, int y, int trunkW = 10) => Put(sprite, x, y, "y", "", trunkW, 6, 0, 2);

    static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y", string show = "") => Layout.Anim(Nm(name), frames, fps, x, y, sort, show);

    static string[] F(string n, int count = 2) => Enumerable.Range(0, count).Select(i => $"{n}_{i}").ToArray();

    /// <summary>Person with idle frames, a small foot collider and a blob shadow.</summary>
    static void Person(string sprite, int x, int y, int fps = 1)
    {
        Anim(sprite, F($"npc_c2_{sprite}"), fps, x, y, "y");
        Layout.Col(x - 4, y - 4, 8, 4);
        Layout.ShadowEll(x + 2.5, y - 1, 6.5, 2.6, 0.8);
    }

    static void Stall(string kind, int x, int Y, string vendor, int vdx, int fps = 2)
    {
        Put($"prop_c2_stallback_{kind}", x, Y - 14);
        if (vendor != "") Anim($"vendor_{vendor}", F($"npc_c2_vendor_{vendor}"), 1, x + vdx, Y - 12);
        Anim($"stall_{kind}", F($"prop_c2_stall_{kind}"), fps, x, Y);
        Layout.Col(x - 29, Y - 14, 58, 14);
        Ground.BoxShadow(x - 28, Y - 18, x + 28, Y - 2, 30, 0.85);
        Layout.ShadowRect(x - 27, Y - 1, 56, 5, 0.6);
    }

    public static void Build()
    {
        Colliders();
        Stalls();
        Square();
        Edges();
        Folks();
        Atmosphere();
        Spots();
        Layout.PlayerStart = (400, 538);
        var mr = Layout.MustReach;
        mr.Add(("south road", 400, 543));
        mr.Add(("entrance gate", 400, 515));
        mr.Add(("well plaza", 400, 372));
        mr.Add(("north road", 366, 30));
        mr.Add(("north road east", 436, 40));
        mr.Add(("north aisle", 400, 135));
        mr.Add(("west street", 22, 226));
        mr.Add(("east street", 778, 226));
        mr.Add(("sw corner", 122, 470));
        mr.Add(("se corner", 660, 470));
        mr.Add(("ne corner", 676, 140));
        mr.Add(("nw corner", 122, 150));
        mr.Add(("behind veg stall", 270, 428));
        mr.Add(("behind grain stall", 164, 150));
        mr.Add(("east middle", 690, 330));
        mr.Add(("west middle", 112, 330));
        Layout.Gaps.Add(("north road mouth", 352, 448, 118));
        Layout.Gaps.Add(("south entrance", 372, 428, 515));
    }

    // ---- baked geometry colliders ----------------------------------------------------------------------------
    static void Colliders()
    {
        Layout.Col(96, 0, 256, 112);          // north row west of the road (N1-N3)
        Layout.Col(448, 0, 256, 112);         // north row east of the road (N4-N6)
        Layout.Col(0, 0, 96, 190);            // NW house + forest corner
        Layout.Col(704, 0, 96, 190);
        Layout.Col(0, 262, 96, 98);           // west and east houses south of the side streets
        Layout.Col(704, 262, 96, 98);
        Layout.Col(0, 360, 102, 200);         // west garden wall + woods
        Layout.Col(698, 360, 102, 200);
        Layout.Col(0, 510, 372, 50);          // south wall (west), gate pillar included
        Layout.Col(428, 510, 372, 50);        // south wall (east)
        // map-edge stops at the end of each street / road
        Layout.Col(Geo.NX0, 0, Geo.NX1 - Geo.NX0, 8);
        Layout.Col(0, 190, 6, 72);
        Layout.Col(794, 190, 6, 72);
        Layout.Col(Geo.SX0, 552, Geo.SX1 - Geo.SX0, 8);
    }

    // ---- the stalls ------------------------------------------------------------------------------------------
    static void Stalls()
    {
        Stall("grain", 164, 176, "grain", -16);
        Stall("bread", 262, 176, "bread", 0);
        Stall("fruit", 540, 176, "fruit", 0);
        Stall("spice", 640, 176, "spice", 0);
        Stall("pots", 176, 330, "pots", 0);
        Stall("cloth", 624, 330, "cloth", 0);

        // vegetable stall: stocked / thinned once bought
        int vx = 270, vy = 456;
        Put("prop_c2_stallback_veg", vx, vy - 14);
        Anim("vendor_veg", F("npc_c2_vendor_veg"), 1, vx, vy - 12);
        Anim("stall_veg_full", F("prop_c2_stall_veg"), 2, vx, vy, "y", "!" + VegKey);
        Anim("stall_veg_thin", F("prop_c2_stall_veg_thin"), 2, vx, vy, "y", VegKey);
        Layout.Col(vx - 29, vy - 14, 58, 14);
        Ground.BoxShadow(vx - 28, vy - 18, vx + 28, vy - 2, 30, 0.85);
        Layout.ShadowRect(vx - 27, vy - 1, 56, 5, 0.6);
        Put("prop_c2_crate_carrot", 230, 468, "y", "!" + VegKey, 20, 8, 0, 1, 14);
        Put("prop_c2_crate_carrot_thin", 230, 468, "y", VegKey);
        Put("prop_c2_crate_cabbage", 310, 468, "y", "!" + VegKey, 20, 8, 0, 1, 14);
        Put("prop_c2_crate_cabbage_thin", 310, 468, "y", VegKey);
        Put("prop_c2_sack", 212, 456, "y", "", 12, 6, 0, 3);

        // the closed stall in the sparse south-east, awning rolled up
        int cx = 536, cy = 456;
        Put("prop_c2_stallback_closed", cx, cy - 14);
        Anim("stall_closed", F("prop_c2_stall_closed"), 1, cx, cy);
        Layout.Col(cx - 29, cy - 14, 58, 14);
        Ground.BoxShadow(cx - 28, cy - 18, cx + 28, cy - 2, 22, 0.8);
    }

    // ---- square dressing ---------------------------------------------------------------------------------------------
    static void Square()
    {
        // well at the heart, lamp posts around it
        Put("prop_well", 400, 346, "y", "", 28, 12, 0, 1, 26);
        Put("prop_bucket", 430, 352, "y", "", 8, 4, 0, 3);
        foreach (var (lx, ly) in new[] { (344, 242), (456, 242), (344, 424), (456, 424) })
            Put("prop_lantern", lx, ly, "y", "", 6, 4, 0, 1, 30);
        Put("prop_bench", 470, 376, "y", "", 38, 10, 0, 1, 16);
        Put("prop_bench", 330, 376, "y", "", 38, 10, 0, 1, 16);

        // north aisle
        Put("prop_c2_noticeboard", 320, 144, "y", "", 22, 6, 0, 1, 34);
        Put("prop_c2_signpost", 468, 144, "y", "", 8, 5, 0, 1, 34);
        Put("prop_c2_planter_a", 118, 130, "y", "", 30, 8, 0, 3);
        Put("prop_c2_planter_b", 684, 130, "y", "", 30, 8, 0, 3);
        Put("prop_plant_pot", 104, 124, "y", "", 12, 6, 0, 3);
        Put("prop_plant_pot", 592, 124, "y", "", 12, 6, 0, 3);

        // goods & clutter around the stalls
        Put("prop_sacks", 108, 188, "y", "", 30, 8, 0, 3);
        Put("prop_c2_sack_open", 206, 182, "y", "", 16, 6, 0, 3);
        Put("prop_c2_barrel", 296, 186, "y", "", 16, 7, 0, 1, 20);
        Put("prop_c2_barrel_open", 590, 184, "y", "", 16, 7, 0, 1, 20);
        Put("prop_c2_crate_apple", 504, 188, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_crate_orange", 676, 188, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_crate_empty", 212, 340, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_crate_pear", 666, 342, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_barrel", 142, 346, "y", "", 16, 7, 0, 1, 20);
        Put("prop_c2_sack_b", 658, 346, "y", "", 12, 6, 0, 3);
        Put("prop_c2_handcart", 176, 482, "y", "", 34, 10, 4, 1, 20);
        Put("prop_c2_haybale", 498, 474, "y", "", 22, 8, 0, 1, 14);

        // the neglected south-east: tipped crates, a lone barrel, weeds
        Put("prop_c2_crate_empty_b", 612, 446, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_crate_empty", 636, 458, "y", "", 20, 8, 0, 1, 14);
        Put("prop_c2_barrel", 586, 494, "y", "", 16, 7, 0, 1, 20);
        Put("prop_grass_tall_a", 560, 428, "y");
        Put("prop_grass_tall_b", 650, 482, "y");
        Put("prop_grass_tall_a", 600, 404, "y");
        Put("prop_wildflowers_a", 560, 480, "floor");
        Put("prop_wildflowers_b", 690, 420, "floor");
        Put("prop_rock_b", 680, 462, "y", "", 10, 5, 0, 3);

        // trees give the corners shade
        Tree("prop_tree_broad", 128, 496, 12);
        Tree("prop_tree_broad", 678, 500, 12);
        Put("prop_bush_round", 160, 508, "y", "", 24, 6, 0, 3);
        Put("prop_bush_berry", 650, 508, "y", "", 20, 6, 0, 3);
        Put("prop_c2_planter_b", 468, 506, "y", "", 30, 6, 0, 3);
        // the keepsake: a woven market basket left by the entrance
        Put("prop_c2_market_basket", 346, 506, "y", "", 16, 6, 0, 3);
    }

    // ---- beyond the square: the north road, side streets, gardens, woods -------------------------------------------
    static void Edges()
    {
        // north road: a cart heading out, loaded with bundles
        Put("prop_c2_cart_loaded", 404, 108, "y", "", 40, 10, 0, 1, 30);
        Put("prop_c2_bundles", 434, 80, "y", "", 24, 7, 0, 1, 14);
        Person("carter", 404, 56, 1);
        Put("prop_c2_barrel", 364, 60, "y", "", 16, 7, 0, 3);
        // west / east gardens behind the walls
        Tree("prop_tree_fruit", 56, 418, 10);
        Tree("prop_tree_broad", 52, 486, 12);
        Put("prop_bush_round", 74, 450, "y", "", 22, 6, 0, 3);
        Tree("prop_tree_fruit", G.W - 56, 424, 10);
        Tree("prop_tree_broad", G.W - 50, 490, 12);
        Put("prop_bush_berry", G.W - 74, 456, "y", "", 20, 6, 0, 3);
        // hedges outside the south wall
        foreach (var x in new[] { 130, 220, 300, 500, 590, 670 }) Put("prop_hedge", x, 540, "y", "", 30, 6, 0, 3);
        // side-street dressing
        Put("prop_bench", 46, 258, "y", "", 38, 10, 0, 1, 16);
        Put("prop_c2_crate_empty", 758, 258, "y", "", 20, 8, 0, 1, 14);
        Put("prop_lantern", 84, 196, "y", "", 6, 4, 0, 1, 30);
        Put("prop_lantern", 716, 196, "y", "", 6, 4, 0, 1, 30);
        Put("prop_plant_pot", 40, 196, "y", "", 12, 6, 0, 3);
        Put("prop_c2_barrel", 740, 197, "y", "", 16, 7, 0, 1, 20);
        // chimney on the bakery roof
        Put("prop_chimney", 168, 46, "y");
    }

    // ---- people and animals --------------------------------------------------------------------------------------------
    static void Folks()
    {
        // the neighbour: idle until greeted, then waving with a little heart bubble
        int nx = 334, ny = 440;
        Anim("neighbour", F("npc_c2_neighbour"), 1, nx, ny, "y", "!" + GreetKey);
        Anim("neighbour_wave", F("npc_c2_neighbour_wave"), 2, nx, ny, "y", GreetKey);
        Layout.Col(nx - 5, ny - 4, 10, 4);
        Layout.ShadowEll(nx + 2.5, ny - 1, 7, 2.8, 0.8);
        Anim("greet_bubble", F("fx_c2_bubble"), 2, nx + 6, ny - 28, "above", GreetKey);

        // rumour near the grain stall: two neighbours with their heads close together
        Anim("talker_woman", F("npc_c2_talker_woman"), 1, 214, 204, "y");
        Anim("talker_man", F("npc_c2_talker_man"), 1, 227, 205, "y");
        Layout.Col(209, 200, 24, 5);
        Layout.ShadowEll(224, 203, 12, 3, 0.8);

        // shoppers
        Person("shopper_basket_l", 238, 350);
        Person("shopper_basket", 570, 352);
        Person("shopper_sack", 512, 214);
        Person("shopper_front", 480, 276);
        Person("child_girl", 496, 278, 2);
        Person("shopper_front_b", 316, 300);
        Person("child", 372, 366, 2);
        // someone glancing up the north road, back turned
        Person("watcher", 372, 144);
        Person("watcher_man", 434, 160);
        // old man resting on the bench, dog at his feet
        Person("oldman", 470, 378);
        Anim("dog", F("npc_c2_dog"), 1, 448, 382, "y"); Layout.Col(440, 378, 16, 4); Layout.ShadowEll(452, 381, 9, 2.6, 0.8);
        // cat on the south wall, ginger cat by the handcart
        Anim("cat_wall", F("npc_c2_cat"), 1, 318, 515, "y");
        Anim("cat_cart", F("npc_c2_cat_ginger"), 1, 214, 488, "y"); Layout.ShadowEll(217, 487, 6, 2.2, 0.8);
        // pigeons pecking around the plaza and under the bread stall
        foreach (var (px, py, flip) in new[] { (386, 392, false), (412, 396, true), (362, 400, false), (300, 204, true), (318, 212, false), (452, 468, false), (130, 396, true) })
            Anim("pigeon", new[] { flip ? "npc_c2_pigeon_stand_l" : "npc_c2_pigeon_stand", flip ? "npc_c2_pigeon_peck_l" : "npc_c2_pigeon_peck" }, 1 + (px % 2), px, py, "floor");
        // pigeons on the roofs
        foreach (var (px, py) in new[] { (126, 24), (172, 22), (230, 20), (310, 15), (500, 18), (650, 14) })
            Anim("roof_pigeon", new[] { "npc_c2_pigeon_stand", "npc_c2_pigeon_peck" }, 1, px, py, "above");
    }

    // ---- ambience: bunting, steam, smoke -------------------------------------------------------------------------------------
    static void Atmosphere()
    {
        Anim("bunting", F("prop_c2_bunting_short"), 2, 213, 150, "above");
        Anim("bunting", F("prop_c2_bunting_short"), 2, 590, 150, "above");
        Anim("bunting", F("prop_c2_bunting_long"), 2, 400, 236, "above");
        Anim("bunting", F("prop_c2_bunting_long"), 2, 400, 408, "above");
        Anim("bunting", F("prop_c2_bunting_mid"), 2, 400, 28, "above");
        Anim("bread_steam", F("fx_c2_steam", 4), 4, 244, 156, "above");
        Anim("bread_steam", F("fx_c2_steam", 4), 3, 276, 158, "above");
        Anim("chimney_smoke", F("fx_c2_smoke", 4), 3, 172, 18, "above");
    }

    // ---- interactables ----------------------------------------------------------------------------------------------------
    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch2_stall", 270, 474, 12, 270, 398, VegKey));
        Layout.Spots.Add(new Spot("ch2_greet", 334, 462, 12, 334, 410, GreetKey));
        Layout.Spots.Add(new Spot("ch2_rumor", 219, 226, 12, 219, 172));
        Layout.Spots.Add(new Spot("ch2_bread", 262, 194, 12, 262, 116));
        Layout.Spots.Add(new Spot("ch2_ks_basket", 368, 496, 12, 352, 480));
    }
}
