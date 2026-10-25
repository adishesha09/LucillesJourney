namespace PixelArt;

public static class Program
{
    const string Root = @"c:\Users\Adishesha\LucillesJourney";
    static readonly string AssetDir = Path.Combine(Root, "Assets", "Resources", "JourneyUI");
    static readonly string PreviewDir = Path.Combine(Root, "Tools", "PixelArt", "out_preview");

    public static int Main(string[] args)
    {
        UiFrames.Build();
        Icons.Build();
        Objects.Build();
        Scenes.Build();
        World.Build();

        if (args.Length >= 3 && args[0] == "zoom")
        {
            Directory.CreateDirectory(PreviewDir);
            var names = args[1].Split(',');
            Preview.Zoom(Registry.All.Where(s => names.Contains(s.Name)).ToList(), int.Parse(args[2]), Path.Combine(PreviewDir, "zoom.png"));
            return 0;
        }

        Directory.CreateDirectory(AssetDir);
        foreach (var s in Registry.All)
            File.WriteAllBytes(Path.Combine(AssetDir, s.Name + ".png"), Png.Encode(s.C));

        Directory.CreateDirectory(PreviewDir);
        Preview.BuildAll(PreviewDir);

        foreach (var g in Registry.All.GroupBy(s => s.Group))
            Console.WriteLine($"{g.Key}: {g.Count()} sprites");
        Console.WriteLine($"Total: {Registry.All.Count} -> {AssetDir}");

        var known = new HashSet<Rgba>(P.Legend.Values) { new Rgba(0xD8, 0xD8, 0xD8), new Rgba(0xE6, 0xE6, 0xE6) };
        foreach (var h in new[] { "6A645B", "433F3B", "5A554F", "66605A", "7A6048" }) known.Add(Rgba.Hex(h));
        var off = Registry.All.SelectMany(s => s.C.Pixels.Where(p => !p.IsClear && !known.Contains(p)).Select(p => (s.Name, p)))
            .GroupBy(t => t.p).OrderByDescending(g => g.Count());
        foreach (var g in off) Console.WriteLine($"  off-palette {g.Key}: {g.Count()} px in {string.Join(",", g.Select(t => t.Name).Distinct().Take(6))}");
        return 0;
    }
}
