namespace WorldArtCh5;

/// <summary>Composition of the Chapter 5 map: camp, stream, props, colliders, interactables.</summary>
public static partial class Plan
{
    const string Built = "ch5_shelter_built", NotBuilt = "!ch5_shelter_built";
    const string Found = "ch5_water_found", NotFound = "!ch5_water_found";
    const string Shared = "ch5_food_shared";

    static int idc;
    static string Nm(string s) => $"{s.Replace("prop_", "").Replace("npc_c5_", "").Replace("fx_c5_", "")}_{idc++}";
    static readonly Random Rng = new(5);

    /// <summary>Place a sprite by its bottom-centre pivot. cw/ch = collider footprint (bottom-centred); shadow: 0 none, 1 rect, 2 tree, 3 small ellipse, 4 long ellipse.</summary>
    static void Put(string sprite, int x, int y, string sort = "y", string show = "", int cw = 0, int ch = 0, int cdx = 0, int shadow = 0, int sdx = 0)
    {
        Layout.Prop(Nm(sprite), sprite, x, y, sort, show);
        if (cw > 0) Layout.Col(x + cdx - cw / 2, y - ch, cw, ch);
        var s = S.Get(sprite);
        if (shadow == 1 && cw > 0) Layout.ShadowRect(x + cdx - cw / 2 + 2, y - ch + 1, cw, ch + 1, 0.8);
        else if (shadow == 2) Layout.ShadowEll(x + s.W * 0.32 + sdx, y - 2, s.W * 0.52, 7, 0.9);
        else if (shadow == 3) Layout.ShadowEll(x + 2 + sdx, y - 1, s.W * 0.42, 3.5, 0.8);
        else if (shadow == 4) Layout.ShadowEll(x + s.W * 0.22 + sdx, y - 1, s.W * 0.55, 6, 0.85);
    }

    static void Tree(string sprite, int x, int y, int trunkW = 10) => Put(sprite, x, y, "y", "", trunkW, 6, 0, 2);

    static readonly Dictionary<string, (int w, int ds)> TentDims = new()
    {
        ["a"] = (44, 14), ["b"] = (36, 12), ["c"] = (56, 16), ["d"] = (40, 14), ["e"] = (48, 14), ["f"] = (34, 12), ["g"] = (50, 16), ["h"] = (42, 12),
    };

    static void Tent(string k, int x, int y)
    {
        var (w, ds) = TentDims[k];
        string sp = "prop_c5_tent_" + k;
        Layout.Prop(Nm(sp), sp, x, y, "y", "");
        Layout.Col(x - (w - 4) / 2, y - ds - 2, w - 4, ds + 2);
        Layout.ShadowEll(x + w * 0.3, y - 2, w * 0.62, 7.5, 0.9);
    }

    static void Person(string name, int x, int y, int fps = 1, bool solid = true, string show = "")
    {
        Layout.Anim(Nm(name), new[] { $"npc_c5_{name}_0", $"npc_c5_{name}_1" }, fps, x, y, "y", show);
        if (solid) Layout.Col(x - 5, y - 4, 10, 4);
        Layout.ShadowEll(x + 2, y - 1, 8, 3, 0.8);
    }

    static void Fire(int x, int yc, bool big, bool pot = true)
    {
        int h = big ? 20 : 12;
        string ring = big ? "prop_c5_firering_l" : "prop_c5_firering_s";
        Layout.Prop(Nm(ring), ring, x, yc + h / 2, "floor");
        Layout.Col(x - (big ? 16 : 8), yc - (big ? 6 : 3), big ? 32 : 16, big ? 12 : 7);
        Ground.Scorch.Add((x, yc, big ? 20 : 12, big ? 9 : 5));
        Ground.Glows.Add((x, yc, big ? 44 : 28, big ? 14 : 10));
        if (!big && pot) Put("prop_c5_tripod_pot", x, yc + 3, "y");
        string[] fr = big ? new[] { "fx_c5_flame_l_0", "fx_c5_flame_l_1", "fx_c5_flame_l_2" } : new[] { "fx_c5_flame_s_0", "fx_c5_flame_s_1", "fx_c5_flame_s_2" };
        int rot = idc % 3;
        Layout.Anim(Nm("flame"), new[] { fr[rot], fr[(rot + 1) % 3], fr[(rot + 2) % 3] }, 6, x, yc + (big ? 5 : 4), "y");
        string[] sm = big ? new[] { "fx_c5_smoke_l_0", "fx_c5_smoke_l_1", "fx_c5_smoke_l_2", "fx_c5_smoke_l_3" } : new[] { "fx_c5_smoke_0", "fx_c5_smoke_1", "fx_c5_smoke_2", "fx_c5_smoke_3" };
        Layout.Anim(Nm("smoke"), sm, 3, x + 1, yc - (big ? 10 : 4), "above");
    }

    public static void Build()
    {
        Camp();
        Plaza();
        Families();
        WaterSide();
        Scatter();
        Reeds();
        Edges();
        Spots();
        Layout.PlayerStart = (172, 574);
        void Must(string n, int x, int y) => Layout.MustReach.Add((n, x, y));
        Must("camp entrance", 98, 616);
        Must("north tents", 150, 212);
        Must("central plaza", 380, 396);
        Must("laundry lines", 250, 296);
        Must("pen gate", 547, 346); Must("inside the pen", 524, 312);
        Must("firewood", 600, 236);
        Must("water landing", 636, 372);
        Must("jug bank", 716, 240);
        Must("ford west", 684, 470);
        Must("ford east", 780, 470);
        Must("far bank north", 790, 200);
        Must("far bank south", 800, 560);
        Must("south-east corner", 592, 586);
        Must("west edge", 70, 420);
    }
    // ---- ambient dressing, scattered with rules ---------------------------------------------------------------------------
    static readonly List<(int x, int y)> Taken = new();

    static void Scatter()
    {
        foreach (var p in Layout.Props) Taken.Add((p.X, p.Y));
        foreach (var a in Layout.Animated) Taken.Add((a.X, a.Y));
        // trees at the foot of the rise and a shade tree
        Tree("prop_c5_olive_a", 62, 126, 12); Tree("prop_c5_cypress", 188, 120, 8); Tree("prop_c5_olive_b", 262, 118, 10);
        Tree("prop_c5_olive_a", 452, 120, 12); Tree("prop_c5_cypress", 526, 118, 8);
        Tree("prop_c5_olive_a", 64, 470, 12);
        // boulders along the foot of the rise
        foreach (var (x, y, s) in new[] { (30, 104, "c"), (352, 112, "a"), (404, 114, "b"), (580, 116, "a"), (710, 108, "c"), (846, 112, "b") })
            Put($"prop_c5_boulder_{s}", x, y, "y", "", s == "c" ? 40 : s == "a" ? 28 : 20, 8, 0, 4);
        // scrub, tufts and stones
        var sprites = new[] { "prop_c5_scrub_a", "prop_c5_scrub_b", "prop_c5_scrub_c", "prop_c5_boulder_d" };
        for (int i = 0; i < 70; i++)
        {
            int x = 36 + Rng.Next(G.W - 72), y = 110 + Rng.Next(G.H - 150);
            if (!OkSpot(x, y, 34, 22)) continue;
            string sp = sprites[Rng.Next(sprites.Length)];
            int cw = sp.EndsWith("c") ? 26 : sp.EndsWith("a") ? 18 : sp.EndsWith("d") ? 12 : 12;
            Put(sp, x, y, "y", "", cw, 6, 0, 3);
            Taken.Add((x, y));
        }
        string[] flat = { "prop_c5_drygrass_a", "prop_c5_drygrass_b", "prop_c5_thistle", "prop_c5_pebbles" };
        for (int i = 0; i < 160; i++)
        {
            int x = 30 + Rng.Next(G.W - 60), y = 100 + Rng.Next(G.H - 130);
            if (!OkSpot(x, y, 14, 4, false)) continue;
            string sp = flat[Rng.Next(flat.Length)];
            Put(sp, x, y, sp.EndsWith("pebbles") ? "floor" : "y");
        }
    }

    static bool OkSpot(int x, int y, int minD, int pathD, bool solid = true)
    {
        if (x < 40 || y < Terrain.EdgeY[Math.Clamp(x, 0, G.W - 1)] + 14 || x > G.W - 50 || y > G.H - 34) return false;
        if (Terrain.WD[x, y] < 14) return false;
        if (Terrain.PathDist(x, y, out _) < pathD + 4 && solid) return false;
        if (Terrain.PathDist(x, y, out _) < 2) return false;
        foreach (var (tx, ty) in Taken) { double d = Math.Sqrt((tx - x) * (tx - x) + (ty - y) * (ty - y) * 1.3); if (d < minD) return false; }
        foreach (var r in Layout.Colliders) if (x > r.X - 14 && x < r.X + r.W + 14 && y > r.Y - 8 && y < r.Y + r.H + 14) return false;
        if (solid) foreach (var (cx, cy, rx, ry) in Terrain.Trample) { double dx = (x - cx) / (double)(rx + 12), dy = (y - cy) / (double)(ry + 12); if (dx * dx + dy * dy < 1) return false; }
        return true;
    }

    static void Reeds()
    {
        var rect = new List<(int x0, int y0, int x1, int y1)> { (620, 346, 700, 400), (690, 222, 770, 252), (680, 448, 790, 492) };
        bool InClear(int x, int y) => rect.Any(r => x >= r.x0 && x <= r.x1 && y >= r.y0 && y <= r.y1);
        int k = 0;
        for (int y = 112; y < G.H - 50; y += 17 + Rng.Next(10))
        {
            int xl = -1, xr = -1;
            for (int x = 600; x < G.W - 40; x++) if (Terrain.WD[x, y] < -1) { if (xl < 0) xl = x; xr = x; }
            if (xl < 0) continue;
            foreach (var (bx, side) in new[] { (xl - 2, 0), (xr + 4, 1) })
            {
                if (Rng.NextDouble() < 0.28 || InClear(bx, y)) continue;
                var tag = k++ % 2 == 0 ? "a" : "b";
                Layout.Anim(Nm("reeds"), new[] { $"fx_c5_reeds_{tag}_0", $"fx_c5_reeds_{tag}_1" }, 2, bx, y, "y");
            }
        }
        // glints on the water surface
        for (int y = 100; y < G.H - 30; y += 15 + Rng.Next(10))
        {
            int cx = (int)Terrain.StreamX(y), hw = (int)Terrain.StreamHw(y);
            int x = cx + Rng.Next(-hw + 6, hw - 6);
            if (x < 10 || x >= G.W || Terrain.WD[x, y] > -5) continue;
            if (Terrain.InFord(x, y) || Terrain.InFord(x, y + 8)) continue;
            bool a = Rng.Next(2) == 0;
            string t = a ? "a" : "b";
            int ph = Rng.Next(3);
            Layout.Anim(Nm("glint"), new[] { $"fx_c5_glint_{t}_{ph}", $"fx_c5_glint_{t}_{(ph + 1) % 3}", $"fx_c5_glint_{t}_{(ph + 2) % 3}" }, 3, x, y, "floor");
        }
    }

    static void Edges()
    {
        // map-edge scrub clumps drawn in front of the border to soften it
        Put("prop_c5_scrub_c", 90, 612, "y");
        Put("prop_c5_scrub_c", 140, 616, "y");
    }

    static void Spots()
    {
        Layout.Spots.Add(new Spot("ch5_water", 632, 376, 12, 650, 362, "ch5_water_found"));
        Layout.Spots.Add(new Spot("ch5_shelter", 150, 548, 12, 150, 470, "ch5_shelter_built"));
        Layout.Spots.Add(new Spot("ch5_share", 302, 572, 12, 302, 520, "ch5_food_shared"));
        Layout.Spots.Add(new Spot("ch5_neighbours", 380, 402, 12, 380, 330, ""));
        Layout.Spots.Add(new Spot("ch5_ks_jug", 720, 242, 12, 738, 224, ""));
    }
}
