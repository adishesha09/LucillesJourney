namespace WorldArtCh4;

public static class Program
{
    public const string Root = @"c:\Users\Adishesha\LucillesJourney";
    public static readonly string AssetDir = Path.Combine(Root, "Assets", "Resources", "World");
    public static readonly string PreviewDir = Path.Combine(Root, "Tools", "WorldArtCh4", "out_preview");

    public static Canvas Disk(string name) => Png.Decode(File.ReadAllBytes(Path.Combine(AssetDir, name + ".png")))!;

    public static int Main(string[] args)
    {
        Directory.CreateDirectory(PreviewDir);
        if (args.Length > 0 && args[0] == "dump")
        {
            var c = Disk(args[1]);
            int x0 = int.Parse(args[2]), y0 = int.Parse(args[3]), x1 = int.Parse(args[4]), y1 = int.Parse(args[5]);
            var legend = new Dictionary<Rgba, char>(); const string ch = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%&*+=";
            for (int y = y0; y < y1; y++)
            {
                var sb = new System.Text.StringBuilder($"{y,3} ");
                for (int x = x0; x < x1; x++)
                {
                    var p = c.Get(x, y);
                    if (p.A == 0) { sb.Append('.'); continue; }
                    if (!legend.TryGetValue(p, out var l)) { l = ch[legend.Count % ch.Length]; legend[p] = l; }
                    sb.Append(l);
                }
                Console.WriteLine(sb);
            }
            Console.WriteLine(string.Join(" ", legend.Select(kv => $"{kv.Value}={kv.Key}")));
            return 0;
        }
        if (args.Length > 0 && args[0] == "inspect")
        {
            int k = int.Parse(args[1]);
            var names = args.Skip(2).ToArray();
            int ix = 4, iy = 4, rowH = 0, maxW = 1700;
            var sheet = new Canvas(maxW, 1400); sheet.Fill(Rgba.Hex("6D7A66"));
            foreach (var n in names)
            {
                var c = Disk(n).Scale(k);
                if (ix + c.W + 8 > maxW) { ix = 4; iy += rowH + 8; rowH = 0; }
                sheet.Blit(c, ix, iy); ix += c.W + 8; rowH = Math.Max(rowH, c.H);
            }
            File.WriteAllBytes(Path.Combine(PreviewDir, "inspect.png"), Png.Encode(sheet.Crop(0, 0, maxW, iy + rowH + 8)));
            return 0;
        }
        Layout.Reset();
        Art.Build();
        Plan.Build();          // registers props, colliders and cast shadows (shadows are baked into the ground)
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
        int count = 0;
        foreach (var kv in S.All)
        {
            if (!(kv.Key.StartsWith("prop_c4_") || kv.Key.StartsWith("npc_c4_") || kv.Key.StartsWith("fx_c4_"))) throw new Exception("refusing to write non-chapter sprite " + kv.Key);
            File.WriteAllBytes(Path.Combine(AssetDir, kv.Key + ".png"), Png.Encode(kv.Value)); count++;
        }
        File.WriteAllBytes(Path.Combine(AssetDir, "ch4_ground.png"), Png.Encode(Ground.Cv)); count++;
        File.WriteAllText(Path.Combine(AssetDir, "ch4_layout.json"), Layout.ToJson());
        var groups = S.Group.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => $"{g.Key}={g.Count()}");
        Console.WriteLine($"wrote {count} PNGs + ch4_layout.json to {AssetDir}");
        Console.WriteLine("sprites per group: " + string.Join(", ", groups));
        Console.WriteLine($"ground {G.W}x{G.H}; layout: props={Layout.Props.Count} animated={Layout.Animated.Count} colliders={Layout.Colliders.Count} interactables={Layout.Spots.Count}");
        return 0;
    }
}
