namespace WorldArtCh3;

public static class Program
{
    public const string Root = @"c:\Users\Adishesha\LucillesJourney";
    public static readonly string AssetDir = Path.Combine(Root, "Assets", "Resources", "World");
    public static readonly string PreviewDir = Path.Combine(Root, "Tools", "WorldArtCh3", "out_preview");

    static readonly string[] Allowed = { "prop_c3_", "npc_c3_", "fx_c3_" };

    public static int Main(string[] args)
    {
        Directory.CreateDirectory(PreviewDir);
        Layout.Reset();
        Sprites.Build();       // all new + re-tinted sprites
        Plan.Build();          // props, colliders, cast shadows, interactables
        Ground.Build();
        Overlays.Build();      // cut from the finished ground
        Sheet.Write(PreviewDir);
        Verify.Previews(PreviewDir);
        bool ok = Verify.Run();
        if (!ok)
        {
            Console.Error.WriteLine("Verification failed - nothing written to " + AssetDir);
            return 1;
        }
        Directory.CreateDirectory(AssetDir);
        // only sprites that the layout really references are shipped; stale chapter-3 files from earlier runs are removed
        var used = new HashSet<string>(Layout.Props.Select(p => p.Sprite).Concat(Layout.Animated.SelectMany(a => a.Frames)));
        int n = 0, skipped = 0;
        foreach (var kv in S.All)
        {
            if (!Allowed.Any(r => kv.Key.StartsWith(r))) throw new Exception("sprite name outside the chapter-3 prefixes: " + kv.Key);
            if (!used.Contains(kv.Key)) { skipped++; continue; }
            File.WriteAllBytes(Path.Combine(AssetDir, kv.Key + ".png"), Png.Encode(kv.Value)); n++;
        }
        foreach (var f in Directory.GetFiles(AssetDir, "*.png").Concat(Directory.GetFiles(AssetDir, "*.png.meta")))
        {
            string name = Path.GetFileName(f).Replace(".png.meta", "").Replace(".png", "");
            if (Allowed.Any(r => name.StartsWith(r)) && !used.Contains(name)) { File.Delete(f); Console.WriteLine("removed stale " + Path.GetFileName(f)); }
        }
        if (skipped > 0) Console.WriteLine($"{skipped} registered-but-unused sprites not written");
        File.WriteAllBytes(Path.Combine(AssetDir, "ch3_ground.png"), Png.Encode(Ground.Cv)); n++;
        File.WriteAllText(Path.Combine(AssetDir, "ch3_layout.json"), Layout.ToJson());
        var groups = S.Group.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => $"{g.Key}={g.Count()}");
        Console.WriteLine($"wrote {n} PNGs + ch3_layout.json to {AssetDir}  (ground {G.W}x{G.H})");
        Console.WriteLine("sprites per group: " + string.Join(", ", groups));
        Console.WriteLine($"layout: props={Layout.Props.Count} animated={Layout.Animated.Count} colliders={Layout.Colliders.Count} interactables={Layout.Spots.Count}");
        return 0;
    }
}
