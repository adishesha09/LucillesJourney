namespace WorldArtCh3;

/// <summary>Layout verification (reachability, clearances) and preview rendering.</summary>
public static class Verify
{
    const int Inflate = 5;     // half the player's foot width
    static readonly List<string> Failures = new();
    static readonly List<string> Warnings = new();
    static readonly string[] RequiredIds = { "ch3_alarm", "ch3_shelter", "ch3_wait", "ch3_counting", "ch3_ks_blanket" };

    static void Fail(string m) { Failures.Add(m); Console.WriteLine("  FAIL: " + m); }
    static void Warn(string m) { Warnings.Add(m); Console.WriteLine("  warn: " + m); }

    public static bool Run()
    {
        Console.WriteLine("== verification ==");
        CheckSprites();
        CheckStatePairs();
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

    static readonly Dictionary<string, Canvas?> DiskCache = new();
    /// <summary>Own sprite, or any existing sprite on disk (characters, chapter-1 props).</summary>
    static Canvas? Find(string name)
    {
        if (S.All.TryGetValue(name, out var s)) return s;
        if (!DiskCache.TryGetValue(name, out var c))
        {
            string p = Path.Combine(Program.AssetDir, name + ".png");
            c = File.Exists(p) ? Png.Decode(File.ReadAllBytes(p)) : null;
            DiskCache[name] = c;
        }
        return c;
    }

    static void CheckSprites()
    {
        var names = new HashSet<string>();
        int bad = 0;
        foreach (var p in Layout.Props)
        {
            if (!names.Add(p.Name)) { Fail($"duplicate instance name {p.Name}"); bad++; }
            var c = Find(p.Sprite);
            if (c is null) { Fail($"prop {p.Name}: missing sprite {p.Sprite}"); bad++; continue; }
            CheckInside(p.Name, p.Sprite, c, p.X, p.Y, p.Sort == "above", ref bad);
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
                var c = Find(f);
                if (c is null) { Fail($"animated {a.Name}: missing frame {f}"); bad++; continue; }
                CheckInside(a.Name, f, c, a.X, a.Y, a.Sort == "above", ref bad);
            }
        }
        foreach (var kv in S.All) if (kv.Value.Bounds() is null) Fail($"sprite {kv.Key} is empty");
        Console.WriteLine($"  sprites: {S.All.Count} registered; placement checked ({bad} problems)");
    }

    static void CheckInside(string name, string sprite, Canvas c, int x, int y, bool lenient, ref int bad)
    {
        int x0 = x - c.W / 2, y0 = y - c.H;
        if (lenient) { if (x < 0 || x >= G.W || y < 0 || y > G.H) { Fail($"{name} ({sprite}) pivot outside the map"); bad++; } return; }
        if (x0 < 0 || y0 < 0 || x0 + c.W > G.W || y > G.H) { Fail($"{name} ({sprite}) lies outside the map: x0={x0} y0={y0} w={c.W} h={c.H}"); bad++; }
    }

    static void CheckStatePairs()
    {
        var keys = new HashSet<string> { "ch3_took_cover", "ch3_sheltered" };
        foreach (var sw in Layout.Props.Select(p => p.ShowWhen).Concat(Layout.Animated.Select(a => a.ShowWhen)))
        {
            if (sw == "") continue;
            var k = sw.TrimStart('!');
            if (!keys.Contains(k)) Fail($"unknown showWhen key '{sw}'");
        }
    }

    static void CheckAnchors()
    {
        Console.WriteLine("  interactables:");
        foreach (var s in Layout.Spots)
        {
            double min = Layout.Colliders.Min(r => DistToRect(s.X, s.Y, r));
            Console.WriteLine($"    {s.Id}: anchor=({s.X},{s.Y}) clearance={min:0.0}px marker=({s.MX},{s.MY}) stateKey={(s.StateKey == "" ? "-" : s.StateKey)}");
            if (min < 12) Fail($"{s.Id}: anchor only {min:0.0}px from a collider");
            if (Math.Abs(s.MX - s.X) > 28) Fail($"{s.Id}: marker too far (dx={Math.Abs(s.MX - s.X)})");
            if (s.MX < 0 || s.MY < 0 || s.MX >= G.W || s.MY >= G.H) Fail($"{s.Id}: marker outside the map");
        }
        foreach (var id in RequiredIds)
            if (Layout.Spots.All(s => s.Id != id)) Fail("missing interactable " + id);
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
        int free = 0;
        for (int x = G.StairX0; x < G.StairX1; x++) if (!blocked[x, 350]) free++;
        Console.WriteLine($"    stairwell: {free}px free after inflation (raw {G.StairX1 - G.StairX0}px)");
        if (free < 20) Fail($"stairwell is too narrow ({free}px free after inflation)");
        int unreachedFree = 0;
        for (int y = 0; y < G.H; y++) for (int x = 0; x < G.W; x++) if (!blocked[x, y] && !seen[x, y]) unreachedFree++;
        Console.WriteLine($"    unreachable free cells (sealed pockets): {unreachedFree}");
    }

    // ================================================ rendering ==================================================
    static Canvas Sprite(string name)
    {
        var c = Find(name);
        if (c is not null) return c;
        bool l = name.StartsWith("lucille");
        var f = new Canvas(l ? 16 : 12, l ? 24 : 16);
        f.Rect(0, 0, f.W, f.H, l ? K.Terra : K.Teal); f.Outline(K.Line);
        return f;
    }

    static bool Visible(string show, HashSet<string> state)
    {
        if (show == "") return true;
        if (show.StartsWith("!")) return !state.Contains(show[1..]);
        return state.Contains(show);
    }

    public static Canvas RenderMap(HashSet<string> state, bool withPlayer = true, bool animFrame1 = false)
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
            items.Add((a.Sort == "floor" ? 0 : a.Sort == "above" ? 2 : 1, a.Y, Sprite(a.Frames[animFrame1 ? Math.Min(1, a.Frames.Length - 1) : 0]), a.X));
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
        var before = new HashSet<string>();
        var after = new HashSet<string> { "ch3_took_cover" };
        var sheltered = new HashSet<string> { "ch3_took_cover", "ch3_sheltered" };
        void Save(string file, Canvas c) => File.WriteAllBytes(Path.Combine(dir, file), Png.Encode(c));
        var mapB = RenderMap(before);
        var mapA = RenderMap(after);
        var mapS = RenderMap(sheltered, false);
        Save("map_before.png", mapB.Scale(2));
        Save("map_after.png", mapA.Scale(2));
        Save("map_sheltered.png", mapS.Scale(2));
        Save("map_debug.png", Debug(mapB, 2));
        void Crop(string file, Canvas src, int x, int y, int w, int h, int k = 4) => Save(file, src.Crop(x, y, w, h).Scale(k));
        Crop("crop_street_west_before.png", mapB, 0, 0, 384, 216, 3);
        Crop("crop_street_west_after.png", mapA, 0, 0, 384, 216, 3);
        Crop("crop_street_east_before.png", mapB, 376, 0, 384, 216, 3);
        Crop("crop_street_east_after.png", mapA, 376, 0, 384, 216, 3);
        Crop("crop_stairwell.png", mapS, 380, 270, 240, 216, 4);
        Crop("crop_cellar_west.png", mapS, 40, 340, 384, 216, 3);
        Crop("crop_cellar_east.png", mapS, 376, 340, 384, 216, 3);
        Crop("crop_cellar_before.png", mapB, 376, 340, 384, 216, 3);
        Crop("crop_ruins_after.png", mapA, 270, 10, 200, 160, 5);
    }
}
