namespace WorldArtCh6;

public static class Program
{
    public const string Root = @"c:\Users\Adishesha\LucillesJourney";
    public static readonly string AssetDir = Path.Combine(Root, "Assets", "Resources", "World");
    public static readonly string PreviewDir = Path.Combine(Root, "Tools", "WorldArtCh6", "out_preview");

    static readonly string[] Reserved = { "lucille_", "child_", "cat_" };

    public static int Main(string[] args)
    {
        Directory.CreateDirectory(PreviewDir);
        if (args.Length > 0 && args[0] == "ref")
        {
            Ref.Run(args.Skip(2).ToArray(), int.Parse(args[1]), Path.Combine(PreviewDir, "ref.png"));
            return 0;
        }
        Layout.Reset();
        Sprites.Build();
        Plan.Build();
        Ground.Build();
        Sheet.Write(PreviewDir);
        Verify.Previews(PreviewDir);
        bool ok = Verify.Run();
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
            File.WriteAllBytes(Path.Combine(AssetDir, kv.Key + ".png"), Png.Encode(kv.Value)); n++;
        }
        File.WriteAllBytes(Path.Combine(AssetDir, "ch6_ground.png"), Png.Encode(Ground.Cv)); n++;
        File.WriteAllText(Path.Combine(AssetDir, "ch6_layout.json"), Layout.ToJson());
        var groups = S.Group.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => $"{g.Key}={g.Count()}");
        Console.WriteLine($"wrote {n} PNGs + ch6_layout.json to {AssetDir}");
        Console.WriteLine("new sprites per group: " + string.Join(", ", groups));
        Console.WriteLine($"layout: props={Layout.Props.Count} animated={Layout.Animated.Count} colliders={Layout.Colliders.Count} interactables={Layout.Spots.Count}");
        return 0;
    }
}
