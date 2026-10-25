using System.Text;

namespace WorldArtCh6;

public readonly record struct Shadow(char Kind, double A, double B, double C, double D, double K);
public record PropDef(string Name, string Sprite, int X, int Y, string Sort, string ShowWhen);
public record AnimDef(string Name, string[] Frames, int Fps, int X, int Y, string Sort, string ShowWhen);
public record Rc(int X, int Y, int W, int H);   // image coords, top-left origin
public record Spot(string Id, int X, int Y, int Radius, int MX, int MY, string StateKey);   // image coords (marker too)

/// <summary>Fixed world geometry (image coordinates: origin top-left, y down; 16 px tiles).</summary>
public static class G
{
    public const int W = 768, H = 512;
}

/// <summary>Sprite registry. All = new sprites written to Resources/World; Ext = existing sprites read from disk.</summary>
public static class S
{
    public static readonly Dictionary<string, Canvas> All = new();
    public static readonly Dictionary<string, string> Group = new();
    static readonly Dictionary<string, Canvas?> ExtCache = new();
    public static string Cur = "misc";

    static readonly string[] Allowed = { "prop_c6_", "npc_c6_", "fx_c6_" };

    public static void Add(string name, Canvas c)
    {
        if (!Allowed.Any(name.StartsWith)) throw new Exception("sprite name outside the chapter prefixes: " + name);
        if (All.ContainsKey(name)) throw new Exception("duplicate sprite " + name);
        c = c.EvenW();
        All[name] = c; Group[name] = Cur;
    }

    /// <summary>Existing sprite from Assets/Resources/World (never rewritten).</summary>
    public static Canvas Ext(string name)
    {
        if (!ExtCache.TryGetValue(name, out var c))
        {
            string p = Path.Combine(Program.AssetDir, name + ".png");
            c = File.Exists(p) ? Png.Decode(File.ReadAllBytes(p)) : null;
            ExtCache[name] = c;
        }
        return c ?? throw new Exception("existing sprite not found on disk: " + name);
    }

    public static bool Has(string name) => All.ContainsKey(name) || (ExtCache.TryGetValue(name, out var c) ? c != null : File.Exists(Path.Combine(Program.AssetDir, name + ".png")));
    public static Canvas Get(string name) => All.TryGetValue(name, out var c) ? c : Ext(name);
}

public static class Layout
{
    public static readonly List<PropDef> Props = new();
    public static readonly List<AnimDef> Animated = new();
    public static readonly List<Rc> Colliders = new();
    public static readonly List<Spot> Spots = new();
    public static readonly List<Shadow> Shadows = new();
    public static (int x, int y) PlayerStart;
    public static readonly List<(string name, int x, int y)> MustReach = new();

    public static void Reset() { Props.Clear(); Animated.Clear(); Colliders.Clear(); Spots.Clear(); Shadows.Clear(); MustReach.Clear(); }

    public static void Prop(string name, string sprite, int x, int y, string sort = "y", string show = "") => Props.Add(new PropDef(name, sprite, x, y, sort, show));
    public static void Anim(string name, string[] frames, int fps, int x, int y, string sort = "y", string show = "") => Animated.Add(new AnimDef(name, frames, fps, x, y, sort, show));
    public static void Col(int x, int y, int w, int h) => Colliders.Add(new Rc(x, y, w, h));
    public static void ShadowEll(double cx, double cy, double rx, double ry, double k = 1.0) => Shadows.Add(new Shadow('e', cx, cy, rx, ry, k));
    public static void ShadowRect(int x, int y, int w, int h, double k = 1.0) => Shadows.Add(new Shadow('r', x, y, w, h, k));

    static string Q(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    static int UY(int imgY) => G.H - imgY;

    public static string ToJson()
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append($" \"ppu\": 16, \"widthPx\": {G.W}, \"heightPx\": {G.H},\n");
        sb.Append(" \"ground\": \"ch6_ground\",\n");
        sb.Append($" \"playerStart\": {{\"x\": {PlayerStart.x}, \"y\": {UY(PlayerStart.y)}}},\n");
        sb.Append(" \"props\": [\n");
        for (int i = 0; i < Props.Count; i++)
        {
            var p = Props[i];
            sb.Append($"  {{\"name\": {Q(p.Name)}, \"sprite\": {Q(p.Sprite)}, \"x\": {p.X}, \"y\": {UY(p.Y)}, \"sort\": {Q(p.Sort)}, \"showWhen\": {Q(p.ShowWhen)}}}");
            sb.Append(i + 1 < Props.Count ? ",\n" : "\n");
        }
        sb.Append(" ],\n \"animated\": [\n");
        for (int i = 0; i < Animated.Count; i++)
        {
            var a = Animated[i];
            sb.Append($"  {{\"name\": {Q(a.Name)}, \"frames\": [{string.Join(", ", a.Frames.Select(Q))}], \"fps\": {a.Fps}, \"x\": {a.X}, \"y\": {UY(a.Y)}, \"sort\": {Q(a.Sort)}, \"showWhen\": {Q(a.ShowWhen)}}}");
            sb.Append(i + 1 < Animated.Count ? ",\n" : "\n");
        }
        sb.Append(" ],\n \"colliders\": [\n");
        for (int i = 0; i < Colliders.Count; i++)
        {
            var c = Colliders[i];
            sb.Append($"  {{\"x\": {c.X}, \"y\": {UY(c.Y + c.H)}, \"w\": {c.W}, \"h\": {c.H}}}");
            sb.Append(i + 1 < Colliders.Count ? ",\n" : "\n");
        }
        sb.Append(" ],\n \"interactables\": [\n");
        for (int i = 0; i < Spots.Count; i++)
        {
            var s = Spots[i];
            string sk = string.IsNullOrEmpty(s.StateKey) ? "" : $", \"stateKey\": {Q(s.StateKey)}";
            sb.Append($"  {{\"id\": {Q(s.Id)}, \"x\": {s.X}, \"y\": {UY(s.Y)}, \"radius\": {s.Radius}, \"markerX\": {s.MX}, \"markerY\": {UY(s.MY)}{sk}}}");
            sb.Append(i + 1 < Spots.Count ? ",\n" : "\n");
        }
        sb.Append(" ]\n}\n");
        return sb.ToString();
    }
}
