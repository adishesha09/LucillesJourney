namespace WorldArtCh5;

/// <summary>Layout verification (reachability, clearances) and preview rendering.</summary>
public static class Verify
{
    const int Inflate = 5;     // half the player's foot width
    static readonly List<string> Failures = new();
    static readonly List<string> Warnings = new();

    static void Fail(string m) { Failures.Add(m); Console.WriteLine("  FAIL: " + m); }
    static void Warn(string m) { Warnings.Add(m); Console.WriteLine("  warn: " + m); }

    public static bool Run()
    {
        Console.WriteLine("== verification ==");
        CheckSprites();
        CheckAnchors();
        CheckReach();
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
        if (G.W % 2 != 0 || G.H % 2 != 0 || G.W > 1024 || G.H > 768 || G.W < 576 || G.H < 336) { Fail("ground size out of range"); bad++; }
        if (Ground.Cv.W != G.W || Ground.Cv.H != G.H) { Fail("ground canvas has the wrong size"); bad++; }
        foreach (var p in Layout.Props)
        {
            if (!names.Add(p.Name)) { Fail($"duplicate instance name {p.Name}"); bad++; }
            var c = S.All.TryGetValue(p.Sprite, out var sc) ? sc : S.Disk(p.Sprite);
            if (c is null) { Fail($"prop {p.Name}: missing sprite {p.Sprite}"); bad++; continue; }
            CheckInside(p.Name, p.Sprite, c, p.X, p.Y, ref bad);
            if (p.Sort == "y")
            {
                var b = c.Bounds();
                if (b is null) { Fail($"{p.Sprite} is empty"); bad++; }
                else if (b.Value.y + b.Value.h < c.H) Warn($"{p.Sprite}: bottom row is transparent ({c.H - b.Value.y - b.Value.h}px above the pivot)");
            }
        }
        foreach (var a in Layout.Animated)
        {
            if (!names.Add(a.Name)) { Fail($"duplicate instance name {a.Name}"); bad++; }
            foreach (var f in a.Frames)
            {
                var c = S.All.TryGetValue(f, out var sc) ? sc : S.Disk(f);
                if (c is null) { Fail($"animated {a.Name}: missing frame {f}"); bad++; continue; }
                CheckInside(a.Name, f, c, a.X, a.Y, ref bad);
            }
        }
        foreach (var kv in S.All) if (kv.Value.Bounds() is null) Fail($"sprite {kv.Key} is empty");
        // state-gated pairs must be the same size
        foreach (var (a, b) in new[] { ("prop_c5_shelter_bare", "prop_c5_shelter_done"), ("prop_c5_bucket_empty", "prop_c5_bucket_full") })
            if (S.All[a].W != S.All[b].W || S.All[a].H != S.All[b].H) { Fail($"{a} and {b} differ in size"); bad++; }
        Console.WriteLine($"  sprites: {S.All.Count} new, {S.Ext.Count} reused; placement checked ({bad} problems)");
    }

    static void CheckInside(string name, string sprite, Canvas c, int x, int y, ref int bad)
    {
        int x0 = x - c.W / 2, y0 = y - c.H;
        if (x0 < 0 || y0 < 0 || x0 + c.W > G.W || y > G.H) { Fail($"{name} ({sprite}) lies outside the map: x0={x0} y0={y0} w={c.W} h={c.H}"); bad++; }
    }

    static readonly string[] Required = { "ch5_water", "ch5_shelter", "ch5_share", "ch5_neighbours", "ch5_ks_jug" };

    static void CheckAnchors()
    {
        Console.WriteLine("  interactables:");
        foreach (var s in Layout.Spots)
        {
            double min = Layout.Colliders.Min(r => DistToRect(s.X, s.Y, r));
            Console.WriteLine($"    {s.Id}: anchor=({s.X},{s.Y}) clearance={min:0.0}px marker=({s.MX},{s.MY}) state={s.StateKey}");
            if (min < 12) Fail($"{s.Id}: anchor only {min:0.0}px from a collider");
            if (Math.Abs(s.MX - s.X) > 28) Fail($"{s.Id}: marker too far (dx={Math.Abs(s.MX - s.X)})");
            if (s.MX < 0 || s.MY < 0 || s.MX >= G.W || s.MY >= G.H) Fail($"{s.Id}: marker outside the map");
        }
        foreach (var id in Required)
            if (Layout.Spots.All(s => s.Id != id)) Fail("missing interactable " + id);
        var keys = Layout.Spots.Select(s => s.StateKey).Where(k => k != "").ToHashSet();
        foreach (var p in Layout.Props) CheckKey(p.Name, p.ShowWhen, keys);
        foreach (var a in Layout.Animated) CheckKey(a.Name, a.ShowWhen, keys);
        // props must not hide the floor of a prop pair ordering mistake: nothing to check further
    }

    static void CheckKey(string name, string show, HashSet<string> keys)
    {
        if (show == "") return;
        var k = show.TrimStart('!');
        if (!keys.Contains(k)) Fail($"{name}: showWhen '{show}' references an unknown state key");
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
            if (!seen[x, y]) Fail($"{name} ({x},{y}) is not reachable"); else Console.WriteLine($"    reach {name}: ok");
        // walkable cells that are unreachable pockets bigger than a few tiles indicate wasted/odd space
        var visited = (bool[,])seen.Clone();
        int pockets = 0;
        for (int y = 0; y < G.H; y += 2)
            for (int x = 0; x < G.W; x += 2)
            {
                if (blocked[x, y] || visited[x, y]) continue;
                int cnt = 0; var st = new Stack<(int, int)>(); st.Push((x, y)); visited[x, y] = true;
                while (st.Count > 0)
                {
                    var (cx, cy) = st.Pop(); cnt++;
                    foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        int nx = cx + dx, ny = cy + dy;
                        if (nx < 0 || ny < 0 || nx >= G.W || ny >= G.H || visited[nx, ny] || blocked[nx, ny]) continue;
                        visited[nx, ny] = true; st.Push((nx, ny));
                    }
                }
                if (cnt > 400) { pockets++; Warn($"unreachable walkable pocket of {cnt} cells near ({x},{y})"); }
            }
    }

    // ================================================ rendering ==================================================
    static readonly Dictionary<string, Canvas?> CharCache = new();

    static Canvas Sprite(string name)
    {
        if (S.All.TryGetValue(name, out var s)) return s;
        if (!CharCache.TryGetValue(name, out var c))
        {
            c = S.Disk(name);
            if (c is null)
            {
                bool l = name.StartsWith("lucille"), cat = name.StartsWith("cat");
                int w = l ? 16 : cat ? 14 : 12, h = l ? 24 : cat ? 10 : 16;
                c = new Canvas(w, h);
                c.Rect(0, 0, w, h, l ? K.Terra : cat ? K.Orange : K.Teal);
                c.Outline(K.Line);
            }
            CharCache[name] = c;
        }
        return c!;
    }

    static bool Visible(string show, HashSet<string> state)
    {
        if (show == "") return true;
        if (show.StartsWith("!")) return !state.Contains(show[1..]);
        return state.Contains(show);
    }

    public static Canvas RenderMap(HashSet<string> state, bool withPlayer = true, int frame = 0)
    {
        var cv = Ground.Cv.Clone();
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
        foreach (var (it, _) in items.Select((v, i) => (v, i)).OrderBy(t => t.v.order).ThenBy(t => t.v.order == 1 ? t.v.y : 0).ThenBy(t => t.i))
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

    public static void Previews(string dir)
    {
        var none = new HashSet<string>();
        var done = new HashSet<string> { "ch5_shelter_built", "ch5_water_found", "ch5_food_shared" };
        var map = RenderMap(none);
        File.WriteAllBytes(Path.Combine(dir, "map_full.png"), Png.Encode(map.Scale(2)));
        File.WriteAllBytes(Path.Combine(dir, "map_debug.png"), Png.Encode(Debug(map, 2)));
        File.WriteAllBytes(Path.Combine(dir, "ground_only.png"), Png.Encode(Ground.Cv.Scale(2)));
        void Crop(string file, Canvas src, int x, int y, int w, int h, int k = 4) =>
            File.WriteAllBytes(Path.Combine(dir, file), Png.Encode(src.Crop(x, y, w, h).Scale(k)));
        var mapDone = RenderMap(done, true, 1);
        Crop("crop_nw.png", map, 40, 80, 300, 220);
        Crop("crop_centre.png", map, 260, 230, 300, 200);
        Crop("crop_family.png", map, 60, 400, 300, 200);
        Crop("crop_stream.png", map, 560, 280, 200, 260);
        Crop("crop_stream_north.png", map, 560, 60, 200, 230);
        Crop("crop_family_done.png", mapDone, 60, 400, 300, 200);
        Crop("crop_rise_west.png", map, 0, 0, 300, 170);
        Crop("crop_sw_corner.png", map, 0, 470, 300, 170);
        Crop("crop_east_bank.png", map, 700, 60, 180, 260);
        Crop("crop_se.png", map, 400, 440, 300, 200);
        Crop("crop_stream_done.png", mapDone, 560, 280, 200, 260);
        File.WriteAllBytes(Path.Combine(dir, "map_done_frame1.png"), Png.Encode(mapDone.Scale(2)));
    }
}
