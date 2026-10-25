namespace WorldArtCh2;

/// <summary>Layout verification (reachability, clearances) and preview rendering.</summary>
public static class Verify
{
    const int Inflate = 5;     // half the player's foot width
    static readonly List<string> Failures = new();
    static readonly List<string> Warnings = new();
    public static readonly string[] RequiredIds = { "ch2_stall", "ch2_greet", "ch2_rumor", "ch2_bread", "ch2_ks_basket" };

    static void Fail(string m) { Failures.Add(m); Console.WriteLine("  FAIL: " + m); }
    static void Warn(string m) { Warnings.Add(m); Console.WriteLine("  warn: " + m); }

    public static bool Run()
    {
        Console.WriteLine("== verification ==");
        CheckSprites();
        CheckAnchors();
        CheckReach();
        CheckStatePairs();
        Console.WriteLine(Failures.Count == 0 ? $"VERIFICATION PASSED ({Warnings.Count} warnings)" : $"VERIFICATION FAILED ({Failures.Count} failures)");
        return Failures.Count == 0;
    }

    static double DistToRect(double px, double py, Rc r)
    {
        double dx = Math.Max(Math.Max(r.X - px, 0), px - (r.X + r.W));
        double dy = Math.Max(Math.Max(r.Y - py, 0), py - (r.Y + r.H));
        return Math.Sqrt(dx * dx + dy * dy);
    }

    static void CheckSprites()
    {
        var names = new HashSet<string>();
        int bad = 0;
        foreach (var p in Layout.Props)
        {
            if (!names.Add(p.Name)) { Fail($"duplicate instance name {p.Name}"); bad++; }
            var c = S.Find(p.Sprite);
            if (c is null) { Fail($"prop {p.Name}: missing sprite {p.Sprite}"); bad++; continue; }
            CheckInside(p.Name, p.Sprite, c, p.X, p.Y, ref bad);
            if (p.Sort == "y")
            {
                var b = c.Bounds();
                if (b is null) { Fail($"{p.Sprite} is empty"); bad++; }
                else if (b.Value.y + b.Value.h < c.H) Warn($"{p.Name}/{p.Sprite}: bottom row is transparent ({c.H - b.Value.y - b.Value.h}px above the pivot)");
            }
        }
        foreach (var a in Layout.Animated)
        {
            if (!names.Add(a.Name)) { Fail($"duplicate instance name {a.Name}"); bad++; }
            foreach (var f in a.Frames)
            {
                var c = S.Find(f);
                if (c is null) { Fail($"animated {a.Name}: missing frame {f}"); bad++; continue; }
                CheckInside(a.Name, f, c, a.X, a.Y, ref bad);
            }
        }
        foreach (var kv in S.All)
        {
            if (kv.Value.Bounds() is null) Fail($"sprite {kv.Key} is empty");
            if (kv.Value.W % 2 != 0) Fail($"sprite {kv.Key} has odd width");
            if (!(kv.Key.StartsWith("prop_c2_") || kv.Key.StartsWith("npc_c2_") || kv.Key.StartsWith("fx_c2_"))) Fail($"bad sprite name {kv.Key}");
        }
        Console.WriteLine($"  sprites: {S.All.Count} registered; placement checked ({bad} problems)");
    }

    static void CheckInside(string name, string sprite, Canvas c, int x, int y, ref int bad)
    {
        int x0 = x - c.W / 2, y0 = y - c.H;
        if (x0 < 0 || y0 < 0 || x0 + c.W > G.W || y > G.H) { Fail($"{name} ({sprite}) lies outside the map: x0={x0} y0={y0} w={c.W} h={c.H}"); bad++; }
    }

    static void CheckAnchors()
    {
        Console.WriteLine("  interactables:");
        foreach (var s in Layout.Spots)
        {
            double min = Layout.Colliders.Min(r => DistToRect(s.X, s.Y, r));
            Console.WriteLine($"    {s.Id}: anchor=({s.X},{s.Y}) clearance={min:0.0}px marker=({s.MX},{s.MY}) stateKey={s.StateKey}");
            if (min < 12) Fail($"{s.Id}: anchor only {min:0.0}px from a collider");
            if (Math.Abs(s.MX - s.X) > 28) Fail($"{s.Id}: marker too far (dx={Math.Abs(s.MX - s.X)})");
            if (s.MX < 0 || s.MY < 0 || s.MX >= G.W || s.MY >= G.H) Fail($"{s.Id}: marker outside the map");
        }
        foreach (var id in RequiredIds)
            if (Layout.Spots.All(s => s.Id != id)) Fail("missing interactable " + id);
    }

    /// <summary>State-gated pairs must occupy the same position with the same size.</summary>
    static void CheckStatePairs()
    {
        var groups = Layout.Props.Where(p => p.ShowWhen != "").GroupBy(p => (p.X, p.Y));
        foreach (var g in groups)
        {
            var sizes = g.Select(p => S.Find(p.Sprite)).Where(c => c != null).Select(c => (c!.W, c.H)).Distinct().ToList();
            if (sizes.Count > 1) Fail($"state pair at {g.Key} has different sprite sizes");
        }
    }

    public static bool[,] BlockedGrid()
    {
        var blocked = new bool[G.W, G.H];
        foreach (var r in Layout.Colliders)
            for (int y = Math.Max(0, r.Y - Inflate); y < Math.Min(G.H, r.Y + r.H + Inflate); y++)
                for (int x = Math.Max(0, r.X - Inflate); x < Math.Min(G.W, r.X + r.W + Inflate); x++) blocked[x, y] = true;
        return blocked;
    }

    public static bool[,]? Reach;

    static void CheckReach()
    {
        var blocked = BlockedGrid();
        var (sx, sy) = Layout.PlayerStart;
        if (blocked[sx, sy]) { Fail($"player start ({sx},{sy}) is inside a (inflated) collider"); return; }
        var seen = new bool[G.W, G.H];
        var q = new Queue<(int, int)>();
        q.Enqueue((sx, sy)); seen[sx, sy] = true;
        int n = 0;
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue(); n++;
            foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= G.W || ny >= G.H || seen[nx, ny] || blocked[nx, ny]) continue;
                seen[nx, ny] = true; q.Enqueue((nx, ny));
            }
        }
        Reach = seen;
        Console.WriteLine($"  BFS: {n} walkable cells reachable from start ({sx},{sy})");
        foreach (var s in Layout.Spots)
            if (!seen[s.X, s.Y]) Fail($"interactable {s.Id} anchor ({s.X},{s.Y}) is not reachable"); else Console.WriteLine($"    reach {s.Id}: ok");
        foreach (var (name, x, y) in Layout.MustReach)
            if (blocked[x, y] || !seen[x, y]) Fail($"{name} ({x},{y}) is not reachable"); else Console.WriteLine($"    reach {name}: ok");
        foreach (var (name, x0, x1, y) in Layout.Gaps)
        {
            int free = 0;
            for (int x = x0; x < x1; x++) if (!blocked[x, y] && seen[x, y]) free++;
            Console.WriteLine($"    gap {name}: {free}px walkable after inflation (raw {x1 - x0}px)");
            if (free < 8) Fail($"{name} is nearly blocked ({free}px free after inflation)");
        }
    }

    // ================================================ rendering ==================================================
    static bool Visible(string show, HashSet<string> state)
    {
        if (show == "") return true;
        if (show.StartsWith("!")) return !state.Contains(show[1..]);
        return state.Contains(show);
    }

    static Canvas Sprite(string name)
    {
        var c = S.Find(name);
        if (c != null) return c;
        var f = new Canvas(16, 24); f.Rect(0, 0, 16, 24, K.Terra); f.Outline(K.Line);
        return f;
    }

    public static Canvas RenderMap(HashSet<string> state, int frame = 0, bool withPlayer = true, bool ground = true)
    {
        var cv = ground ? Ground.Cv.Clone() : new Canvas(G.W, G.H);
        var items = new List<(int order, int y, Canvas c, int x)>();
        foreach (var p in Layout.Props)
        {
            if (!Visible(p.ShowWhen, state)) continue;
            items.Add((p.Sort == "floor" ? 0 : p.Sort == "above" ? 2 : 1, p.Y, Sprite(p.Sprite), p.X));
        }
        foreach (var a in Layout.Animated)
        {
            if (!Visible(a.ShowWhen, state)) continue;
            items.Add((a.Sort == "floor" ? 0 : a.Sort == "above" ? 2 : 1, a.Y, Sprite(a.Frames[frame % a.Frames.Length]), a.X));
        }
        if (withPlayer) items.Add((1, Layout.PlayerStart.y, Sprite("lucille_down_idle"), Layout.PlayerStart.x));
        foreach (var it in items.Select((v, i) => (v, i)).OrderBy(t => t.v.order).ThenBy(t => t.v.order == 1 ? t.v.y : 0).ThenBy(t => t.i).Select(t => t.v))
            cv.Blit(it.c, it.x - it.c.W / 2, it.y - it.c.H);
        return cv;
    }

    static Canvas Debug(Canvas map, int k)
    {
        var big = map.Scale(k);
        var red = Rgba.Hex("FF3B30"); var yellow = Rgba.Hex("FFE14D"); var mag = Rgba.Hex("FF4DE1"); var green = Rgba.Hex("39FF6A");
        foreach (var r in Layout.Colliders)
        {
            for (int y = r.Y * k; y < (r.Y + r.H) * k; y++)
                for (int x = r.X * k; x < (r.X + r.W) * k; x++)
                    if (((x / 2 + y / 2) & 1) == 0 && big.In(x, y)) big.Set(x, y, red);
            big.Frame(r.X * k, r.Y * k, r.W * k, r.H * k, red);
        }
        void Cross(int x, int y, Rgba c, int s) { big.Rect(x - s, y, s * 2 + 1, 1, c); big.Rect(x, y - s, 1, s * 2 + 1, c); }
        foreach (var s in Layout.Spots)
        {
            for (int a = 0; a < 360; a += 3)
                big.Set((int)(s.X * k + Math.Cos(a * Math.PI / 180) * s.Radius * k), (int)(s.Y * k + Math.Sin(a * Math.PI / 180) * s.Radius * k), yellow);
            Cross(s.X * k, s.Y * k, yellow, 5);
            big.Disc(s.MX * k, s.MY * k, 4, mag); big.Disc(s.MX * k, s.MY * k, 2, Rgba.Hex("FFFFFF"));
        }
        Cross(Layout.PlayerStart.x * k, Layout.PlayerStart.y * k, green, 8);
        foreach (var (_, x, y) in Layout.MustReach) Cross(x * k, y * k, Rgba.Hex("00E5FF"), 4);
        return big;
    }

    static Canvas Walk(Canvas map, int k)
    {
        var big = map.Scale(k);
        var blocked = BlockedGrid();
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                bool ok = Reach != null && Reach[x, y];
                var c = ok ? Rgba.Hex("39FF6A") : blocked[x, y] ? Rgba.Hex("FF3B30") : Rgba.Hex("3B6BFF");
                for (int j = 0; j < k; j++) for (int i = 0; i < k; i++) if (((x * k + i) + (y * k + j)) % 3 == 0) big.Set(x * k + i, y * k + j, c);
            }
        return big;
    }

    public static void Previews(string dir)
    {
        var none = new HashSet<string>();
        var after = new HashSet<string> { "ch2_vegetables_bought", "ch2_neighbour_greeted" };
        var map = RenderMap(none);
        void Save(string file, Canvas c) => File.WriteAllBytes(Path.Combine(dir, file), Png.Encode(c));
        Save("map_full.png", map.Scale(2));
        Save("map_debug.png", Debug(map, 2));
        Save("map_walk.png", Walk(map, 2));
        Save("map_after.png", RenderMap(after, 1).Scale(2));
        Save("ground_only.png", Ground.Cv.Scale(2));
        void Crop(string file, Canvas src, int x, int y, int w, int h, int k = 4) => Save(file, src.Crop(x, y, w, h).Scale(k));
        var map1 = RenderMap(none, 1);
        Crop("crop_nw.png", map, 0, 0, 400, 280, 3);
        Crop("crop_ne.png", map, 400, 0, 400, 280, 3);
        Crop("crop_sw.png", map, 0, 280, 400, 280, 3);
        Crop("crop_se.png", map, 400, 280, 400, 280, 3);
        Crop("crop_veg_stall.png", map, 220, 380, 130, 100, 6);
        Crop("crop_veg_stall_thin.png", RenderMap(after, 0), 220, 380, 130, 100, 6);
        Crop("crop_centre.png", map1, 240, 230, 320, 200, 4);
        Crop("crop_north_road.png", map, 300, 0, 200, 200, 4);
        Crop("crop_entrance.png", map, 280, 440, 240, 120, 5);
    }
}
