namespace WorldArt;

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
        foreach (var p in Layout.Props)
        {
            if (!names.Add(p.Name)) { Fail($"duplicate instance name {p.Name}"); bad++; }
            if (!S.All.TryGetValue(p.Sprite, out var c)) { Fail($"prop {p.Name}: missing sprite {p.Sprite}"); bad++; continue; }
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
            foreach (var f in a.Frames)
            {
                var c = S.All.TryGetValue(f, out var sc) ? sc : LoadChar(f);
                if (c is null) { Fail($"animated {a.Name}: missing frame {f}"); bad++; continue; }
                CheckInside(a.Name, f, c, a.X, a.Y, ref bad);
            }
        }
        foreach (var kv in S.All) if (kv.Value.Bounds() is null) Fail($"sprite {kv.Key} is empty");
        Console.WriteLine($"  sprites: {S.All.Count} registered; placement checked ({bad} problems)");
    }

    static void CheckInside(string name, string sprite, Canvas c, int x, int y, ref int bad)
    {
        int x0 = x - c.W / 2, y0 = y - c.H;
        if (x0 < 0 || y0 < 0 || x0 + c.W > G.W || y > G.H) { Fail($"{name} ({sprite}) lies outside the map: x0={x0} y0={y0} w={c.W} h={c.H}"); bad++; }
    }

    static Canvas? LoadChar(string name)
    {
        string p = Path.Combine(Program.AssetDir, name + ".png");
        return File.Exists(p) ? Png.Decode(File.ReadAllBytes(p)) : null;
    }

    static void CheckAnchors()
    {
        Console.WriteLine("  interactables:");
        foreach (var s in Layout.Spots)
        {
            double min = Layout.Colliders.Min(r => DistToRect(s.X, s.Y, r));
            Console.WriteLine($"    {s.Id}: anchor=({s.X},{s.Y}) clearance={min:0.0}px marker=({s.MX},{s.MY})");
            if (min < 12) Fail($"{s.Id}: anchor only {min:0.0}px from a collider");
            if (Math.Abs(s.MX - s.X) > 28) Fail($"{s.Id}: marker too far (dx={Math.Abs(s.MX - s.X)})");
            if (s.MX < 0 || s.MY < 0 || s.MX >= G.W || s.MY >= G.H) Fail($"{s.Id}: marker outside the map");
        }
        foreach (var id in new[] { "ch1_garden", "ch1_breakfast", "ch1_photo", "ch1_shelf", "ch1_ks_wateringcan" })
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
        Gap("front doorway", 160, 192, 300, blocked);
        Gap("garden gate", 552, 584, 299, blocked);
        Gap("bedroom doorway", 320, 352, 208, blocked);
        int free = 0;
        for (int y = G.DivGY0; y < G.DivGY1; y++) if (!blocked[G.DivX + 4, y]) free++;
        Console.WriteLine($"    gap partition doorway: {free}px free after inflation (raw {G.DivGY1 - G.DivGY0}px)");
        if (free < 8) Fail($"partition doorway is nearly blocked ({free}px free after inflation)");
    }

    static void Gap(string name, int a, int b, int y, bool[,] blocked)
    {
        int free = 0;
        for (int x = a; x < b; x++) if (!blocked[x, y]) free++;
        Console.WriteLine($"    gap {name}: {free}px free after inflation (raw {b - a}px)");
        if (free < 8) Fail($"{name} is nearly blocked ({free}px free after inflation)");
    }

    // ================================================ rendering ==================================================
    static readonly Dictionary<string, Canvas?> CharCache = new();

    static Canvas Sprite(string name)
    {
        if (S.All.TryGetValue(name, out var s)) return s;
        if (!CharCache.TryGetValue(name, out var c))
        {
            c = LoadChar(name);
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

    public static Canvas RenderMap(HashSet<string> state, bool withPlayer = true)
    {
        var cv = Ground.Cv.Clone();
        var items = new List<(int order, int y, Canvas c, int x)>();
        foreach (var p in Layout.Props)
        {
            if (!Visible(p.ShowWhen, state)) continue;
            items.Add((p.Sort == "floor" ? 0 : p.Sort == "above" ? 2 : 1, p.Y, Sprite(p.Sprite), p.X));
        }
        foreach (var a in Layout.Animated)
            items.Add((a.Sort == "floor" ? 0 : a.Sort == "above" ? 2 : 1, a.Y, Sprite(a.Frames[0]), a.X));
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
        var wet = new HashSet<string> { "ch1_garden_watered" };
        var shared = new HashSet<string> { "ch1_breakfast_shared" };
        var map = RenderMap(none);
        File.WriteAllBytes(Path.Combine(dir, "map_full.png"), Png.Encode(map.Scale(3)));
        File.WriteAllBytes(Path.Combine(dir, "map_debug.png"), Png.Encode(Debug(map, 3)));
        void Crop(string file, Canvas src, int x, int y, int w, int h, int k = 4) =>
            File.WriteAllBytes(Path.Combine(dir, file), Png.Encode(src.Crop(x, y, w, h).Scale(k)));
        Crop("crop_house_west.png", map, 44, 36, 184, 300);
        Crop("crop_house_east.png", map, 212, 36, 232, 300);
        Crop("crop_front_yard.png", map, 36, 296, 372, 170);
        Crop("crop_garden_dry.png", map, 452, 36, 240, 290);
        Crop("crop_garden_wet.png", RenderMap(wet), 452, 36, 240, 290);
        var mapShared = RenderMap(shared);
        var a = map.Crop(60, 168, 112, 100).Scale(4); var b = mapShared.Crop(60, 168, 112, 100).Scale(4);
        var both = new Canvas(a.W * 2 + 16, a.H); both.Fill(Rgba.Hex("222222")); both.Blit(a, 0, 0); both.Blit(b, a.W + 16, 0);
        File.WriteAllBytes(Path.Combine(dir, "crop_table_full_vs_empty.png"), Png.Encode(both));
    }
}
