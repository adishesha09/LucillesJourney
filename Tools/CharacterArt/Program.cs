namespace CharacterArt;

public static class Program
{
    const string Root = @"c:\Users\Adishesha\LucillesJourney";
    static readonly string WorldDir = Path.Combine(Root, "Assets", "Resources", "World");
    static readonly string PreviewDir = Path.Combine(Root, "Tools", "CharacterArt", "out_preview");

    static readonly List<(string name, Canvas c, int w, int h)> All = new();

    static void Add(string name, Canvas c, int w, int h)
    {
        if (name.StartsWith("ch1_ground") || name.StartsWith("prop_")) throw new Exception($"reserved prefix: {name}");
        if (c.W != w || c.H != h) throw new Exception($"{name}: is {c.W}x{c.H}, expected {w}x{h}");
        if (!c.HardAlphaOnly()) throw new Exception($"{name}: soft alpha");
        if (All.Any(a => a.name == name)) throw new Exception($"duplicate {name}");
        All.Add((name, c, w, h));
    }

    static readonly string[] Dirs = { "down", "up", "left", "right" };
    static string Frame(int f) => f < 0 ? "idle" : $"walk{f}";

    public static int Main()
    {
        Directory.CreateDirectory(WorldDir);
        Directory.CreateDirectory(PreviewDir);

        var lucille = new List<List<Canvas>>();
        var strips = new List<List<Canvas>>();
        for (int d = 0; d < 4; d++)
        {
            var row = new List<Canvas>(); var strip = new List<Canvas>();
            for (int f = -1; f < 4; f++)
            {
                var c = Lucille.Build((Dir)d, f);
                Add($"lucille_{Dirs[d]}_{Frame(f)}", c, Lucille.W, Lucille.H);
                row.Add(c); if (f >= 0) strip.Add(c);
            }
            lucille.Add(row); strips.Add(strip);
        }
        Save("lucille_sheet", Preview.Sheet(lucille));
        Save("lucille_walk_strips", Preview.Strip(strips));
        Save("lucille_ingame", Preview.InGame(lucille));
        Save("lucille_zoom", Preview.Sheet(new() { lucille.Select(r => r[0]).ToList() }, 20, 1, true, true));
        Save("lucille_zoom_walk_side", Preview.Sheet(new() { lucille[3].Skip(1).ToList() }, 16, 1, true, true));

        Kids.Register(Add, Save);
        Cat.Register(Add, Save);
        Portrait.Register(Add, Save);

        Validate();
        foreach (var (name, c, _, _) in All)
            File.WriteAllBytes(Path.Combine(WorldDir, name + ".png"), Png.Encode(c));
        Console.WriteLine($"Wrote {All.Count} sprites to {WorldDir}");
        return 0;
    }

    static void Save(string name, Canvas c) => File.WriteAllBytes(Path.Combine(PreviewDir, name + ".png"), Png.Encode(c));

    static void Validate()
    {
        // every Lucille frame: same size, feet on the bottom row, body centred, nothing outside the canvas bbox of the set
        var luc = All.Where(a => a.name.StartsWith("lucille_") && a.name != "lucille_portrait").ToList();
        foreach (var (name, c, _, _) in luc)
        {
            var b = c.Bounds()!.Value;
            if (b.y1 != c.H - 1) throw new Exception($"{name}: feet not on bottom row (y1={b.y1})");
            Console.WriteLine($"{name,-22} bbox x{b.x0}..{b.x1} y{b.y0}..{b.y1}");
        }
        foreach (var (name, c, _, _) in All)
            if (c.Bounds() == null) throw new Exception($"{name}: empty");
    }
}
