namespace CharacterArt;

/// <summary>Palette: one char per tone. Kid/cat sprites copy Base and override placeholder chars.</summary>
public static class Pal
{
    static Rgba H(string h) => Rgba.Hex(h);

    public static Dictionary<char, Rgba> Base() => new()
    {
        // hair (dark brown)
        ['H'] = H("73594A"), ['h'] = H("4A3B32"), ['j'] = H("33292A"),
        // skin (warm medium-brown)
        ['a'] = H("E8BC93"), ['s'] = H("D19A6D"), ['S'] = H("B0795A"), ['X'] = H("BE8861"), ['x'] = H("8E5C42"),
        ['m'] = H("B5624E"), ['P'] = H("DE8C78"),
        // eyes
        ['e'] = H("2B2321"), ['w'] = H("FFF8E8"), ['i'] = H("5A3E30"),
        // terracotta dress
        ['Q'] = H("D8705C"), ['r'] = H("C05A4A"), ['R'] = H("A04838"), ['D'] = H("7E3A32"),
        // cream apron
        ['c'] = H("F4EAD2"), ['C'] = H("DCCBA4"), ['K'] = H("B5A07A"),
        // sandals / leather
        ['t'] = H("A8895E"), ['T'] = H("7A5F48"),
        // gold
        ['y'] = H("F2D26B"), ['Y'] = H("B8893A"), ['L'] = H("FBE9A8"),
        // wood (chair)
        ['B'] = H("8A7058"), ['M'] = H("6A5444"), ['b'] = H("4A3B32"),
        // ginger cat
        ['o'] = H("D98F4B"), ['q'] = H("E9AE72"), ['O'] = H("A96A33"), ['n'] = H("8A4F2B"), ['p'] = H("E7A39A"),
        // teal / blue
        ['z'] = H("8FC1B8"), ['E'] = H("2F6360"), ['G'] = H("4B8F8A"), ['u'] = H("6B8FA8"), ['f'] = H("8FB0C4"), ['U'] = H("3E5A6E"),
    };

    static readonly Rgba OutlineTarget = new(0x2B, 0x1C, 0x1A);

    /// <summary>Selective outline colour: the material's own colour pulled toward a warm dark brown.</summary>
    public static Rgba OutlineOf(Rgba c)
    {
        double t = 0.62 + 0.10 * (1 - c.Luma / 255.0);
        byte L(byte a, byte b) => (byte)Math.Round(a * (1 - t) + b * t);
        return new Rgba(L(c.R, OutlineTarget.R), L(c.G, OutlineTarget.G), L(c.B, OutlineTarget.B));
    }
}

/// <summary>Character grid sprite: one palette char per pixel, ' ' = empty.</summary>
public sealed class Spr
{
    public readonly int W, H;
    readonly char[] g;

    public Spr(int w, int h) { W = w; H = h; g = new char[w * h]; Array.Fill(g, ' '); }

    public char Get(int x, int y) => x >= 0 && y >= 0 && x < W && y < H ? g[y * W + x] : ' ';
    public void Put(int x, int y, char c) { if (x >= 0 && y >= 0 && x < W && y < H && c != '.') g[y * W + x] = c; }
    public void Erase(int x, int y) { if (x >= 0 && y >= 0 && x < W && y < H) g[y * W + x] = ' '; }

    public void Map(int ox, int oy, string[] rows, string name)
    {
        int w = rows[0].Length;
        for (int j = 0; j < rows.Length; j++)
        {
            if (rows[j].Length != w) throw new Exception($"{name}: row {j} has length {rows[j].Length}, expected {w}: '{rows[j]}'");
            for (int i = 0; i < w; i++) Put(ox + i, oy + j, rows[j][i]);
        }
    }

    public void Ellipse(double cx, double cy, double rx, double ry, char c)
    {
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry;
                if (dx * dx + dy * dy <= 1.0) g[y * W + x] = c;
            }
    }

    public void Rect(int x, int y, int w, int h, char c)
    {
        for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) Put(i, j, c);
    }

    public void Paint(Func<int, int, char?> f)
    {
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { var c = f(x, y); if (c.HasValue) g[y * W + x] = c.Value; }
    }

    static readonly (char, char)[] LightPairs =
        { ('a', 'S'), ('Q', 'R'), ('c', 'C'), ('1', '3'), ('4', '6'), ('q', 'O'), ('f', 'U') };

    /// <summary>Horizontal flip that also swaps light/shade tones so light still comes from the top-left.</summary>
    public Spr Mirror(bool swapLight = true)
    {
        var o = new Spr(W, H);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                char c = g[y * W + (W - 1 - x)];
                if (swapLight) foreach (var (a, b) in LightPairs) { if (c == a) { c = b; break; } if (c == b) { c = a; break; } }
                o.g[y * W + x] = c;
            }
        return o;
    }

    public Spr Clone() { var o = new Spr(W, H); Array.Copy(g, o.g, g.Length); return o; }

    public Canvas Render(Dictionary<char, Rgba> pal, bool outline = true)
    {
        var c = new Canvas(W, H);
        Rgba P(char ch) => pal.TryGetValue(ch, out var v) ? v : throw new Exception($"unknown palette char '{ch}'");
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) if (g[y * W + x] != ' ') c.Set(x, y, P(g[y * W + x]));
        if (!outline) return c;
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                if (g[y * W + x] != ' ') continue;
                Rgba? best = null;
                foreach (var (dx, dy) in new[] { (0, 1), (-1, 0), (1, 0), (0, -1) })
                {
                    char n = Get(x + dx, y + dy);
                    if (n == ' ') continue;
                    var oc = Pal.OutlineOf(P(n));
                    if (best == null || oc.Luma < best.Value.Luma) best = oc;
                }
                if (best != null) c.Set(x, y, best.Value);
            }
        return c;
    }
}
