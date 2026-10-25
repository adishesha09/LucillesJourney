namespace WorldArtCh2;

public static class Program
{
    public const string Root = @"c:\Users\Adishesha\LucillesJourney";
    public static readonly string AssetDir = Path.Combine(Root, "Assets", "Resources", "World");
    public static readonly string PreviewDir = Path.Combine(Root, "Tools", "WorldArtCh2", "out_preview");

    static readonly string[] Reserved = { "lucille_", "child_", "cat_", "ch1_" };

    public static int Main(string[] args)
    {
        Directory.CreateDirectory(PreviewDir);
        Folk.Build();
        Stalls.Build();
        Props.Build();
        Plan.Build();          // registers props, colliders and cast shadows (baked into the ground)
        Ground.Build();
        Sheet.Write(PreviewDir, 4);
        bool ok = Verify.Run();
        Verify.Previews(PreviewDir);
        if (!ok)
        {
            Console.Error.WriteLine("Verification failed - nothing written to " + AssetDir);
            return 1;
        }
        Directory.CreateDirectory(AssetDir);
        int n = 0;
        foreach (var kv in S.All)
        {
            if (Reserved.Any(r => kv.Key.StartsWith(r))) throw new Exception("refusing to overwrite character sprite " + kv.Key);
            if (!(kv.Key.StartsWith("prop_c2_") || kv.Key.StartsWith("npc_c2_") || kv.Key.StartsWith("fx_c2_"))) throw new Exception("unexpected sprite name " + kv.Key);
            File.WriteAllBytes(Path.Combine(AssetDir, kv.Key + ".png"), Png.Encode(kv.Value)); n++;
        }
        File.WriteAllBytes(Path.Combine(AssetDir, "ch2_ground.png"), Png.Encode(Ground.Cv)); n++;
        File.WriteAllText(Path.Combine(AssetDir, "ch2_layout.json"), Layout.ToJson());
        var groups = S.Group.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => $"{g.Key}={g.Count()}");
        Console.WriteLine($"wrote {n} PNGs + ch2_layout.json to {AssetDir}");
        Console.WriteLine("sprites per group: " + string.Join(", ", groups));
        Console.WriteLine($"layout: props={Layout.Props.Count} animated={Layout.Animated.Count} colliders={Layout.Colliders.Count} interactables={Layout.Spots.Count}");
        return 0;
    }
}
