namespace WorldArtCh2;

/// <summary>Townsfolk and animals: small parametric pixel figures (16x25 adults, 12x20 children), 2-frame idles.</summary>
public static class Folk
{
    static Rgba H(string h) => Rgba.Hex(h);
    public static Rgba[] T(string l, string m, string d) => new[] { H(l), H(m), H(d) };

    // skin tones (light, mid, shade) - varied
    public static readonly Rgba[] SkinA = T("EDC7A2", "D9A47A", "B27B5A");
    public static readonly Rgba[] SkinB = T("E8BC93", "D19A6D", "B0795A");
    public static readonly Rgba[] SkinC = T("C98E66", "A8704B", "865236");
    public static readonly Rgba[] SkinD = T("F2D2B0", "E3B58C", "C28D69");
    public static readonly Rgba[] SkinE = T("A8704B", "8A5538", "6C3E2B");
    // hair
    public static readonly Rgba[] HBlack = T("4A3B38", "33292A", "211A1B");
    public static readonly Rgba[] HDark = T("6A5446", "4A3B32", "33292A");
    public static readonly Rgba[] HBrown = T("94705A", "73594A", "523E34");
    public static readonly Rgba[] HAuburn = T("C0794F", "A0583A", "763E2B");
    public static readonly Rgba[] HGrey = T("E4DED2", "BDB6AA", "8F887E");
    public static readonly Rgba[] HBlond = T("EBCF8A", "D2AE62", "A88442");
    // cloth
    public static readonly Rgba[] Terra = T("D8705C", "C05A4A", "9E4638");
    public static readonly Rgba[] Teal = T("6FB0A6", "4B8F8A", "2F6360");
    public static readonly Rgba[] Blue = T("8FB0C4", "6B8FA8", "4C6E86");
    public static readonly Rgba[] Mustard = T("F8DE8C", "F2D26B", "C9983F");
    public static readonly Rgba[] Cream = T("FFFAEC", "F4EAD2", "CDBFA0");
    public static readonly Rgba[] Plum = T("A88BC4", "7C6AA3", "5E4E80");
    public static readonly Rgba[] Green = T("9CC873", "6FA86B", "447A4B");
    public static readonly Rgba[] Brown = T("A8895E", "7A5F48", "5A4638");
    public static readonly Rgba[] Tan = T("DCC794", "C4AC7C", "9A8258");
    public static readonly Rgba[] Rose = T("EAA5B5", "D4849A", "B5667A");
    public static readonly Rgba[] Grey = T("B7B1A8", "8F8A86", "6A6568");
    public static readonly Rgba[] Rust = T("D98F4B", "B8683A", "8A4B2B");
    public static readonly Rgba[] Navy = T("6B83A0", "4A6080", "34465F");
    static readonly Rgba Shoe = H("5E4638"), ShoeHi = H("86664E");

    public sealed record Look(Rgba[] Skin, Rgba[] Hair, Rgba[] Top, Rgba[] Bottom, bool Skirt = false, Rgba[]? Apron = null,
        Rgba[]? Head = null, string Hat = "", bool Bun = false, bool Long = false, bool Beard = false, bool Stache = false,
        bool Bald = false, int Stout = 0, bool Sleeves = true, Rgba[]? Scarf = null, bool Cane = false, int Tall = 0);

    static Canvas Finish(Canvas c)
    {
        c.SelOutline(0.40, true);
        return c;
    }

    // ---- shared pieces -------------------------------------------------------------------------------------
    static void Head(Canvas c, Look k, int cx, int cy, int dy, string view)
    {
        // base head: ellipse 8 wide, 8 tall
        double ccy = cy + dy;
        c.Paint((x, y) => { double dx = (x + .5 - cx) / 4.3, dd = (y + .5 - ccy) / 4.4; return dx * dx + dd * dd <= 1.0; }, k.Skin[1], cx - 6, (int)ccy - 6, cx + 7, (int)ccy + 7);
        // soft shade on the right and chin
        for (int y = (int)ccy - 5; y <= (int)ccy + 5; y++)
            for (int x = cx - 5; x <= cx + 5; x++)
            {
                if (c.Get(x, y) != k.Skin[1]) continue;
                bool right = view == "front" ? x >= cx + 3 : view == "back" ? x >= cx + 3 : x <= cx - 3;
                bool chin = y >= ccy + 3.2;
                bool lit = (view != "side" && x <= cx - 3 && y < ccy + 1) || (view == "side" && x >= cx + 2 && y < ccy);
                if (right || chin) c.Set(x, y, k.Skin[2]);
                else if (lit) c.Set(x, y, k.Skin[0]);
            }
    }

    static void HairFront(Canvas c, Look k, int cx, int top, int dy)
    {
        var h = k.Hair; int t = top + dy;
        if (k.Head != null)
        {
            var s = k.Head;
            // headscarf: wraps the crown and drapes at the sides
            for (int x = cx - 5; x <= cx + 4; x++)
            {
                int ex = x - (cx - 5);
                int hgt = (x <= cx - 4 || x >= cx + 3) ? 3 : 2;
                if (x == cx - 5 || x == cx + 4) hgt = 3;
                for (int y = t; y < t + hgt + 1; y++)
                    c.Set(x, y, y == t ? s[0] : x >= cx + 2 ? s[2] : s[1]);
                _ = ex;
            }
            for (int y = t + 2; y < t + 7; y++) { c.Set(cx - 5, y, s[1]); c.Set(cx + 4, y, s[2]); }
            c.Set(cx - 4, t + 3, s[2]); c.Set(cx + 3, t + 3, s[2]);
            c.Set(cx - 2, t, s[0]); c.Set(cx - 3, t, s[0]);
            c.Set(cx + 4, t + 6, s[2]);
            c.Set(cx - 3, t + 1, s[2]); c.Set(cx + 1, t + 1, s[2]); c.Set(cx - 1, t + 2, s[0]);   // printed dots on the cloth
            return;
        }
        if (k.Bald)
        {
            for (int y = t + 1; y < t + 5; y++) { c.Set(cx - 5, y + 1, h[1]); c.Set(cx + 4, y + 1, h[2]); }
            c.Set(cx - 3, t + 1, k.Skin[0]); c.Set(cx - 2, t + 1, k.Skin[0]);
            return;
        }
        // short crop
        for (int x = cx - 4; x <= cx + 3; x++)
        {
            c.Set(x, t, h[1]); c.Set(x, t + 1, x <= cx - 2 ? h[0] : h[1]);
        }
        c.Set(cx - 4, t, Rgba.Clear); c.Set(cx + 3, t, Rgba.Clear);
        for (int x = cx - 5; x <= cx + 4; x++)
        {
            c.Set(x, t + 2, x <= cx - 4 || x >= cx + 3 ? h[1] : (x == cx - 3 || x == cx + 2 ? h[1] : h[2] ));
        }
        // fringe: side-swept
        c.Set(cx - 2, t + 2, k.Skin[1]); c.Set(cx - 1, t + 2, k.Skin[1]); c.Set(cx, t + 2, k.Skin[1]); c.Set(cx + 1, t + 2, k.Skin[1]);
        c.Set(cx - 3, t + 2, h[1]); c.Set(cx + 2, t + 2, h[1]);
        for (int y = t + 3; y < t + (k.Long ? 8 : 5); y++) { c.Set(cx - 5, y, h[y == t + 3 ? 0 : 1]); c.Set(cx + 4, y, h[2]); }
        if (k.Long) { c.Set(cx - 5, t + 8, h[2]); c.Set(cx + 4, t + 8, h[2]); }
        if (k.Bun) { c.Rect(cx - 1, t - 2, 3, 2, h[1]); c.Set(cx - 1, t - 2, h[0]); c.Set(cx - 2, t - 1, Rgba.Clear); }
    }

    static void Hat(Canvas c, Look k, int cx, int top, int dy)
    {
        int t = top + dy;
        if (k.Hat == "cap")
        {
            var s = k.Head ?? Brown;
            for (int x = cx - 4; x <= cx + 3; x++) { c.Set(x, t, s[x <= cx - 2 ? 0 : 1]); c.Set(x, t + 1, s[1]); }
            for (int x = cx - 5; x <= cx + 4; x++) c.Set(x, t + 2, s[2]);
            c.Set(cx - 4, t, Rgba.Clear); c.Set(cx + 3, t, Rgba.Clear);
            c.Set(cx - 5, t + 1, s[1]); c.Set(cx + 4, t + 1, s[2]);
        }
        else if (k.Hat == "straw")
        {
            var s = Mustard;
            for (int x = cx - 7; x <= cx + 6; x++) { c.Set(x, t + 2, x <= cx - 3 ? s[0] : s[1]); c.Set(x, t + 3, s[2]); }
            for (int x = cx - 4; x <= cx + 3; x++) { c.Set(x, t, s[0]); c.Set(x, t + 1, s[1]); }
            c.Set(cx - 4, t, Rgba.Clear); c.Set(cx + 3, t, Rgba.Clear);
            for (int x = cx - 4; x <= cx + 3; x++) c.Set(x, t + 1, x >= cx + 2 ? s[2] : s[1]);
            c.HL(cx - 4, t + 2, 8, Terra[1]);
            c.HL(cx - 7, t + 2, 3, s[0]); c.HL(cx + 4, t + 2, 3, s[1]);
        }
        else if (k.Hat == "whitecap")
        {
            var s = Cream;
            for (int x = cx - 4; x <= cx + 3; x++) { c.Set(x, t - 1, s[0]); c.Set(x, t, s[0]); c.Set(x, t + 1, s[1]); }
            c.Set(cx - 4, t - 1, Rgba.Clear); c.Set(cx + 3, t - 1, Rgba.Clear);
            for (int x = cx - 5; x <= cx + 4; x++) c.Set(x, t + 2, s[x >= cx + 2 ? 2 : 1]);
            c.Set(cx + 3, t, s[1]); c.Set(cx + 3, t + 1, s[2]);
        }
    }

    static void FaceFront(Canvas c, Look k, int cx, int cy, int dy, bool smile = true)
    {
        int y = cy + dy;
        var ink = H("2B2321");
        c.Set(cx - 2, y, ink); c.Set(cx + 1, y, ink);
        if (k.Hair == HGrey) { c.Set(cx - 3, y - 1, k.Hair[1]); c.Set(cx - 2, y - 1, k.Hair[1]); c.Set(cx + 1, y - 1, k.Hair[1]); c.Set(cx + 2, y - 1, k.Hair[1]); }
        c.Set(cx - 3, y + 1, Rgba.Mul(k.Skin[1], 1.0, 0.78, 0.74)); c.Set(cx + 2, y + 1, Rgba.Mul(k.Skin[1], 1.0, 0.78, 0.74));
        var mouth = Rgba.Mul(k.Skin[2], 1.05, 0.72, 0.72);
        if (smile) { c.Set(cx - 1, y + 2, mouth); c.Set(cx, y + 2, mouth); c.Set(cx - 2, y + 1, mouth); c.Set(cx + 1, y + 1, mouth); c.Set(cx - 2, y + 1, Rgba.Mul(k.Skin[1], 1.0, 0.78, 0.74)); c.Set(cx + 1, y + 1, Rgba.Mul(k.Skin[1], 1.0, 0.78, 0.74)); }
        else { c.Set(cx - 1, y + 2, mouth); c.Set(cx, y + 2, mouth); }
        c.Set(cx - 1, y + 1, k.Skin[2]);   // nose shade
        if (k.Stache) { var hh = k.Hair[1]; c.Set(cx - 2, y + 2, hh); c.Set(cx - 1, y + 2, hh); c.Set(cx, y + 2, hh); c.Set(cx + 1, y + 2, hh); c.Set(cx - 1, y + 3, mouth); c.Set(cx, y + 3, mouth); }
        if (k.Beard) { var hh = k.Hair[1]; for (int x = cx - 3; x <= cx + 2; x++) { c.Set(x, y + 3, hh); } c.Set(cx - 3, y + 2, hh); c.Set(cx + 2, y + 2, hh); c.Set(cx - 2, y + 4, k.Hair[2]); c.Set(cx - 1, y + 4, k.Hair[2]); c.Set(cx, y + 4, k.Hair[2]); c.Set(cx + 1, y + 4, k.Hair[2]); }
    }

    static void Torso(Canvas c, Look k, int top, int bottom, int dy, int s)
    {
        int x0 = 4 - s, x1 = 11 + s;
        for (int y = top + dy; y <= bottom; y++)
            for (int x = x0; x <= x1; x++)
            {
                int tone = x == x0 ? 0 : x >= x1 - 1 ? 2 : 1;
                if (x == x0 + 1 && y < top + dy + 4) tone = Math.Min(tone, 1);
                if (y == bottom) tone = Math.Min(2, tone + 1);
                c.Set(x, y, k.Top[tone]);
            }
        // collar notch
        c.Set(7, top + dy, k.Skin[1]); c.Set(8, top + dy, k.Skin[1]);
        c.Set(7, top + dy + 1, k.Top[2]); c.Set(8, top + dy + 1, k.Top[1]);
        // fabric folds
        c.Set(6, bottom - 1, k.Top[2]); c.Set(9, bottom - 2, k.Top[2]);
    }

    static void LegsAndShoes(Canvas c, Look k, int s)
    {
        if (k.Skirt)
        {
            for (int r = 0; r < 7; r++)
            {
                int y = 15 + r, hw = 4 + s + (r + 1) / 3;
                for (int x = 8 - hw; x < 8 + hw; x++)
                {
                    int tone = x < 8 - hw + 2 ? 0 : x >= 8 + hw - 2 ? 2 : 1;
                    if (r == 6) tone = Math.Min(2, tone + 1);
                    c.Set(x, y, k.Bottom[tone]);
                }
                if (r >= 2) { c.Set(6, y, k.Bottom[Math.Min(2, 1 + (r > 4 ? 1 : 0))]); c.Set(10, y, k.Bottom[2]); }
            }
        }
        else
        {
            for (int y = 18; y <= 21; y++)
            {
                for (int x = 5; x <= 7; x++) c.Set(x, y, k.Bottom[x == 5 ? 0 : 1]);
                for (int x = 8; x <= 10; x++) c.Set(x, y, k.Bottom[x == 10 ? 2 : 1]);
                c.Set(8, y, k.Bottom[2]);
            }
        }
        for (int x = 4; x <= 7; x++) { c.Set(x, 22, x == 4 ? ShoeHi : Shoe); c.Set(x, 23, Shoe); }
        for (int x = 8; x <= 11; x++) { c.Set(x, 22, Shoe); c.Set(x, 23, x == 11 ? Rgba.Mul(Shoe, .8, .8, .85) : Shoe); }
        c.Set(4, 23, ShoeHi); c.Set(8, 23, ShoeHi);
    }

    static void Apron(Canvas c, Look k, int dy, int s, bool toFeet)
    {
        if (k.Apron == null) return;
        var a = k.Apron;
        int yb = toFeet ? 21 : 17;
        for (int y = 12 + dy; y <= yb; y++)
        {
            int x0 = y < 14 ? 6 : 5 - (y > 17 ? 1 : 0), x1 = y < 14 ? 9 : 10 + (y > 17 ? 1 : 0);
            for (int x = x0; x <= x1; x++) c.Set(x, y, x == x0 ? a[0] : x >= x1 - 1 ? a[2] : a[1]);
        }
        c.HL(5, yb, 6, a[2]);
        c.Set(5, 14 + dy, a[2]); c.Set(10, 14 + dy, a[2]);   // ties
        c.Set(7, 15, a[0]);
    }

    static void ArmsFront(Canvas c, Look k, int dy, int s, int raiseLeft = 0, bool basketRight = false)
    {
        int top = 11 + dy;
        // left arm (viewer's left)
        int lx = 2 - s;
        if (raiseLeft == 0)
        {
            for (int y = top; y <= top + 4; y++) { c.Set(lx, y, k.Sleeves ? k.Top[0] : k.Skin[0]); c.Set(lx + 1, y, k.Sleeves ? k.Top[1] : k.Skin[1]); }
            if (!k.Sleeves) { }
            else { c.Set(lx, top + 5, k.Skin[1]); c.Set(lx + 1, top + 5, k.Skin[1]); c.Set(lx, top + 6, k.Skin[2]); c.Set(lx + 1, top + 6, k.Skin[2]); }
        }
        else
        {
            // raised, waving: forearm up beside the head
            int hx = lx - (raiseLeft == 2 ? 1 : 0);
            for (int y = top + 1; y <= top + 3; y++) { c.Set(lx, y, k.Top[0]); c.Set(lx + 1, y, k.Top[1]); }
            c.Set(lx, top, k.Top[0]); c.Set(lx + 1, top, k.Top[1]);
            for (int y = top - 4; y <= top - 1; y++) { c.Set(hx + 1, y, k.Skin[1]); c.Set(hx, y, k.Skin[0]); }
            c.Set(hx, top - 5, k.Skin[0]); c.Set(hx + 1, top - 5, k.Skin[1]); c.Set(hx - 1, top - 4, k.Skin[0]);
            c.Set(hx + 2, top - 3, k.Skin[2]);
        }
        // right arm
        int rx = 12 + s;
        for (int y = top; y <= top + 4; y++) { c.Set(rx, y, k.Top[1]); c.Set(rx + 1, y, k.Top[2]); }
        if (k.Sleeves) { c.Set(rx, top + 5, k.Skin[1]); c.Set(rx + 1, top + 5, k.Skin[2]); c.Set(rx, top + 6, k.Skin[2]); c.Set(rx + 1, top + 6, k.Skin[2]); }
        if (basketRight)
        {
            var w1 = H("D2A66A"); var w2 = H("A8783F"); var w3 = H("7A5530");
            c.Line(rx, top + 5, rx + 1, top + 5, w3);
            for (int y = top + 7; y <= top + 10; y++) for (int x = rx - 1; x <= rx + 2; x++) c.Set(x, y, ((x + y) & 1) == 0 ? w1 : w2);
            c.HL(rx - 1, top + 10, 4, w3);
            c.Set(rx - 1, top + 6, w3); c.Set(rx + 2, top + 6, w3);
        }
    }

    // ---- standing adult, front --------------------------------------------------------------------------------
    public static Canvas Front(Look k, int frame, int raiseLeft = 0, bool basketRight = false, bool seated = false)
    {
        var c = new Canvas(16, 25);
        int s = k.Stout, dy = frame == 1 ? 1 : 0;
        LegsAndShoes(c, k, s);
        Torso(c, k, 10, 17, dy, s);
        Apron(c, k, dy, s, k.Skirt);
        ArmsFront(c, k, dy, s, raiseLeft, basketRight);
        Head(c, k, 8, 6, dy, "front");
        if (k.Head != null || k.Hat == "") HairFront(c, k, 8, 2, dy);
        else HairFront(c, k, 8, 2, dy);
        FaceFront(c, k, 8, 6, dy);
        if (k.Hat != "") Hat(c, k, 8, 2, dy);
        if (k.Scarf != null) // shawl over shoulders
        {
            var a = k.Scarf;
            for (int x = 3 - s; x <= 12 + s; x++) { c.Set(x, 10 + dy, a[x < 8 ? 0 : 2]); }
            for (int x = 4 - s; x <= 11 + s; x++) c.Set(x, 11 + dy, a[x < 7 ? 1 : 2]);
            c.Set(7, 12 + dy, a[1]); c.Set(8, 12 + dy, a[2]); c.Set(7, 13 + dy, a[2]);
            c.Set(7, 10 + dy, k.Skin[2]); c.Set(8, 10 + dy, k.Skin[2]);
        }
        if (k.Cane) // walking stick at the right hand
        {
            var w = H("8A6444"); var w2 = H("5E4430");
            for (int y = 16; y <= 23; y++) { c.Set(14 + s, y, w); }
            c.Set(14 + s, 24 - 1, w2); c.Set(13 + s, 16, w2);
        }
        return Finish(c);
    }

    // ---- standing adult, back ---------------------------------------------------------------------------------
    public static Canvas Back(Look k, int frame, int headTurn = 0)
    {
        var c = new Canvas(16, 25);
        int s = k.Stout, dy = frame == 1 ? 1 : 0;
        LegsAndShoes(c, k, s);
        // back torso: no collar notch
        Torso(c, k, 10, 17, dy, s);
        c.Set(7, 10 + dy, k.Top[1]); c.Set(8, 10 + dy, k.Top[1]); c.Set(7, 11 + dy, k.Top[1]); c.Set(8, 11 + dy, k.Top[2]);
        if (k.Apron != null) { c.HL(5, 15, 6, k.Apron[2]); c.Set(7, 15, k.Apron[1]); c.Set(8, 15, k.Apron[1]); }
        ArmsFront(c, k, dy, s);
        int cx = 8 + headTurn;
        Head(c, k, cx, 6, dy, "back");
        // whole head covered by hair / scarf
        var hcol = k.Head ?? k.Hair;
        c.Paint((x, y) => c.Get(x, y) == k.Skin[1] || c.Get(x, y) == k.Skin[2] || c.Get(x, y) == k.Skin[0], hcol[1], cx - 5, 1 + dy, cx + 5, 11 + dy);
        for (int y = 2 + dy; y < 10 + dy; y++)
            for (int x = cx - 5; x <= cx + 4; x++)
                if (c.Get(x, y) == hcol[1] && (x <= cx - 3 && y < 6 + dy)) c.Set(x, y, hcol[0]);
                else if (c.Get(x, y) == hcol[1] && (x >= cx + 2 || y >= 8 + dy)) c.Set(x, y, hcol[2]);
        if (k.Head != null) { c.Set(cx, 9 + dy, hcol[2]); c.Set(cx - 1, 8 + dy, hcol[0]); c.Set(cx, 8 + dy, hcol[0]); c.Set(cx - 1, 9 + dy, hcol[1]); }
        if (k.Bun) { c.Rect(cx - 1, 0 + dy, 3, 2, hcol[1]); c.Set(cx - 1, 0 + dy, hcol[0]); }
        if (k.Hat == "cap") Hat(c, k, cx, 2, dy);
        if (k.Hat == "straw") Hat(c, k, cx, 2, dy);
        if (k.Hat == "whitecap") Hat(c, k, cx, 2, dy);
        if (k.Scarf != null) { var a = k.Scarf; for (int x = 3 - s; x <= 12 + s; x++) c.Set(x, 10 + dy, a[x < 8 ? 0 : 2]); for (int x = 4 - s; x <= 11 + s; x++) c.Set(x, 11 + dy, a[x < 7 ? 1 : 2]); for (int x = 5; x <= 10; x++) c.Set(x, 12 + dy, a[x < 8 ? 1 : 2]); }
        return Finish(c);
    }

    // ---- standing adult, side (facing right; flip for left) ------------------------------------------------------
    public static Canvas Side(Look k, int frame, bool basket = false, bool sack = false, bool left = false, int lean = 0)
    {
        var c = new Canvas(16, 25);
        int dy = frame == 1 ? 1 : 0;
        // legs
        if (k.Skirt)
        {
            for (int r = 0; r < 7; r++)
            {
                int y = 15 + r, hw = 3 + (r + 1) / 3;
                for (int x = 8 - hw; x < 8 + hw; x++) c.Set(x, y, k.Bottom[x < 8 - hw + 2 ? 0 : x >= 8 + hw - 2 ? 2 : 1]);
                c.Set(8 + hw - 1, y, k.Bottom[2]);
            }
        }
        else
        {
            for (int y = 18; y <= 21; y++) { for (int x = 6; x <= 9; x++) c.Set(x, y, k.Bottom[x == 6 ? 0 : x == 9 ? 2 : 1]); c.Set(8, y, k.Bottom[2]); }
        }
        for (int x = 6; x <= 10; x++) { c.Set(x, 22, x == 6 ? ShoeHi : Shoe); c.Set(x, 23, Shoe); }
        c.Set(10, 22, ShoeHi); c.Set(11, 23, Shoe);
        // torso 6 wide
        for (int y = 10 + dy; y <= 17; y++)
            for (int x = 5; x <= 10; x++) c.Set(x, y, k.Top[x <= 5 ? 0 : x >= 10 ? 2 : 1]);
        c.HL(5, 17, 6, k.Top[2]);
        if (k.Apron != null) { for (int y = 13; y <= (k.Skirt ? 21 : 17); y++) { c.Set(10, y, k.Apron[0]); c.Set(11, y, k.Apron[1]); } }
        if (k.Scarf != null) { var a = k.Scarf; for (int x = 4; x <= 10; x++) c.Set(x, 10 + dy, a[x < 7 ? 0 : 2]); for (int x = 4; x <= 10; x++) c.Set(x, 11 + dy, a[x < 7 ? 1 : 2]); }
        // arm (near side)
        for (int y = 11 + dy; y <= 14; y++) { c.Set(7, y, k.Top[0]); c.Set(8, y, k.Top[1]); }
        c.Set(8, 15 + dy, k.Skin[1]); c.Set(9, 15 + dy, k.Skin[1]); c.Set(9, 16 + dy, k.Skin[2]);
        if (basket)
        {
            var w1 = H("D2A66A"); var w2 = H("A8783F"); var w3 = H("7A5530");
            for (int y = 17; y <= 20; y++) for (int x = 9; x <= 13; x++) c.Set(x, y, ((x + y) & 1) == 0 ? w1 : w2);
            c.HL(9, 20, 5, w3); c.VL(9, 17, 4, w3); c.VL(13, 17, 4, w3);
            c.Line(9, 16 + dy, 11, 16, w3); c.Line(11, 16, 13, 17, w3);
            c.Set(10, 16, H("9CC873")); c.Set(11, 15, H("D9786A")); c.Set(12, 16, H("F2D26B"));
        }
        if (sack)
        {
            var s1 = H("D9C29A"); var s2 = H("B89E72"); var s3 = H("8A7550");
            c.Rect(3, 8 + dy, 7, 7, s2); c.Rect(3, 8 + dy, 3, 6, s1); c.VL(9, 9 + dy, 6, s3); c.HL(4, 14 + dy, 6, s3);
            c.Set(3, 8 + dy, Rgba.Clear); c.Set(9, 8 + dy, Rgba.Clear); c.Set(4, 7 + dy, s1); c.Set(5, 7 + dy, s1);
        }
        // head in profile (face right)
        int cx = 9 + lean;
        double ccy = 6 + dy;
        c.Paint((x, y) => { double ddx = (x + .5 - cx) / 4.0, ddy = (y + .5 - ccy) / 4.4; return ddx * ddx + ddy * ddy <= 1.0; }, k.Skin[1], cx - 6, (int)ccy - 6, cx + 7, (int)ccy + 7);
        for (int y = (int)ccy - 5; y <= (int)ccy + 5; y++) for (int x = cx - 5; x <= cx + 5; x++)
                if (c.Get(x, y) == k.Skin[1] && (x <= cx - 3 || y >= ccy + 3.2)) c.Set(x, y, k.Skin[2]);
        c.Set(cx + 4, 7 + dy, k.Skin[0]); c.Set(cx + 4, 6 + dy, k.Skin[1]);            // nose
        c.Set(cx + 2, 6 + dy, H("2B2321"));                                               // eye
        c.Set(cx + 2, 8 + dy, Rgba.Mul(k.Skin[2], 1.05, .72, .72)); c.Set(cx + 3, 8 + dy, Rgba.Mul(k.Skin[2], 1.05, .72, .72));
        c.Set(cx + 1, 7 + dy, Rgba.Mul(k.Skin[1], 1.0, .78, .74));
        c.Set(cx - 1, 7 + dy, k.Skin[2]);                                                // ear
        if (k.Stache) { c.Set(cx + 3, 7 + dy, k.Hair[1]); c.Set(cx + 2, 7 + dy, k.Hair[1]); }
        if (k.Beard) { for (int x = cx; x <= cx + 3; x++) c.Set(x, 9 + dy, k.Hair[1]); c.Set(cx + 3, 8 + dy, k.Hair[1]); }
        // hair / scarf covering the back and crown
        var hc = k.Head ?? k.Hair;
        if (!k.Bald || k.Head != null)
        {
            for (int x = cx - 5; x <= cx + 3; x++)
            {
                c.Set(x, 2 + dy, hc[x <= cx - 2 ? 0 : 1]);
                if (x >= cx - 4) c.Set(x, 3 + dy, hc[1]);
            }
            c.Set(cx - 5, 2 + dy, Rgba.Clear); c.Set(cx + 3, 2 + dy, Rgba.Clear);
            for (int y = 3 + dy; y < (k.Long ? 10 : 8) + dy; y++) { c.Set(cx - 5, y, hc[2]); c.Set(cx - 4, y, hc[1]); c.Set(cx - 3, y, hc[y < 6 + dy ? 1 : 2]); }
            c.Set(cx + 3, 3 + dy, hc[1]); c.Set(cx + 2, 4 + dy, hc[1]);
            c.Set(cx - 2, 4 + dy, hc[1]); c.Set(cx - 2, 5 + dy, hc[2]);
            if (k.Bun) { c.Rect(cx - 6, 5 + dy, 2, 3, hc[1]); c.Set(cx - 6, 5 + dy, hc[0]); }
            if (k.Head != null) { c.Set(cx - 6, 7 + dy, hc[2]); c.Set(cx - 6, 8 + dy, hc[2]); c.Set(cx - 5, 9 + dy, hc[2]); }
        }
        else { c.Set(cx - 4, 4 + dy, k.Hair[1]); c.Set(cx - 4, 5 + dy, k.Hair[2]); c.Set(cx - 3, 3 + dy, k.Skin[0]); }
        if (k.Hat == "cap")
        {
            var s = k.Head ?? Brown;
            for (int x = cx - 4; x <= cx + 3; x++) { c.Set(x, 2 + dy, s[x <= cx - 2 ? 0 : 1]); c.Set(x, 3 + dy, s[1]); }
            for (int x = cx - 4; x <= cx + 6; x++) c.Set(x, 4 + dy, s[2]);   // brim
            c.Set(cx - 4, 2 + dy, Rgba.Clear);
        }
        var img = Finish(c);
        return left ? FlipSideLight(img) : img;
    }

    static Canvas FlipSideLight(Canvas c) => c.FlipH();

    // ---- children -------------------------------------------------------------------------------------------------
    public static Canvas Child(Look k, int frame, bool pigtails = false, bool wave = false)
    {
        var c = new Canvas(12, 21);
        int dy = frame == 1 ? 1 : 0;
        // legs
        for (int y = 15; y <= 16; y++) { c.Set(4, y, k.Skin[0]); c.Set(5, y, k.Skin[1]); c.Set(7, y, k.Skin[1]); c.Set(8, y, k.Skin[2]); }
        for (int x = 3; x <= 5; x++) { c.Set(x, 17, Shoe); c.Set(x, 18, Shoe); }
        for (int x = 7; x <= 9; x++) { c.Set(x, 17, Shoe); c.Set(x, 18, Shoe); }
        c.Set(3, 17, ShoeHi); c.Set(7, 17, ShoeHi);
        // body
        for (int y = 10 + dy; y <= 15; y++)
            for (int x = 3; x <= 8; x++)
            {
                var col = k.Skirt && y >= 13 ? k.Bottom : k.Top;
                c.Set(x, y, col[x == 3 ? 0 : x >= 7 ? 2 : 1]);
            }
        if (k.Skirt) { c.Set(2, 15, k.Bottom[1]); c.Set(9, 15, k.Bottom[2]); c.Set(2, 14, k.Bottom[0]); c.Set(9, 14, k.Bottom[2]); }
        else { c.HL(3, 14, 6, k.Bottom[1]); c.HL(3, 15, 6, k.Bottom[2]); }
        // arms
        c.Set(2, 11 + dy, k.Top[0]); c.Set(2, 12 + dy, k.Top[1]); c.Set(2, 13 + dy, k.Skin[1]);
        if (wave) { c.Set(9, 10 + dy, k.Top[2]); c.Set(10, 9 + dy, k.Skin[1]); c.Set(10, 8 + dy, k.Skin[0]); c.Set(9, 7 + dy, k.Skin[1]); }
        else { c.Set(9, 11 + dy, k.Top[2]); c.Set(9, 12 + dy, k.Top[2]); c.Set(9, 13 + dy, k.Skin[2]); }
        // head 8x7
        double cx = 6, cy = 5.2 + dy;
        c.Paint((x, y) => { double ddx = (x + .5 - cx) / 4.1, ddy = (y + .5 - cy) / 3.8; return ddx * ddx + ddy * ddy <= 1.0; }, k.Skin[1], 0, 0, 12, 12);
        for (int y = 0; y < 12; y++) for (int x = 0; x < 12; x++) if (c.Get(x, y) == k.Skin[1] && (x >= 9 || y >= 7 + dy)) c.Set(x, y, k.Skin[2]);
        var h = k.Hair;
        for (int x = 3; x <= 8; x++) { c.Set(x, 1 + dy, h[x <= 5 ? 0 : 1]); c.Set(x, 2 + dy, h[1]); }
        c.Set(2, 3 + dy, h[1]); c.Set(9, 3 + dy, h[2]); c.Set(2, 4 + dy, h[1]); c.Set(9, 4 + dy, h[2]);
        c.Set(5, 3 + dy, k.Skin[0]); c.Set(6, 3 + dy, k.Skin[0]); c.Set(4, 3 + dy, h[1]); c.Set(7, 3 + dy, h[1]);
        c.Set(4, 5 + dy, H("2B2321")); c.Set(7, 5 + dy, H("2B2321"));
        c.Set(3, 6 + dy, Rgba.Mul(k.Skin[1], 1.0, .78, .74)); c.Set(8, 6 + dy, Rgba.Mul(k.Skin[1], 1.0, .78, .74));
        var mo = Rgba.Mul(k.Skin[2], 1.05, .72, .72); c.Set(5, 7 + dy, mo); c.Set(6, 7 + dy, mo);
        if (pigtails) { c.Rect(1, 5 + dy, 1, 3, h[1]); c.Rect(10, 5 + dy, 1, 3, h[2]); c.Set(1, 8 + dy, h[2]); c.Set(10, 8 + dy, h[2]); c.Set(1, 4 + dy, Mustard[1]); c.Set(10, 4 + dy, Mustard[1]); }
        return Finish(c);
    }

    // ---- animals ------------------------------------------------------------------------------------------------------
    /// <summary>Dog lying down, facing right.</summary>
    public static Canvas Dog(int frame)
    {
        var c = new Canvas(22, 13);
        var f1 = H("D9A66A"); var f2 = H("B8814A"); var f3 = H("7A4C2A"); var fl = H("EBC48C"); var ink = H("2B2321");
        // body, lit along the back
        c.Ellipse(9.5, 7.8, 7.8, 3.8, f2);
        c.Ellipse(9.2, 6.8, 7.0, 2.9, f1);
        c.Ellipse(8, 5.9, 4.6, 1.5, fl);
        c.HL(4, 11, 12, f3);
        // haunch
        c.Ellipse(4.5, 8, 3.2, 2.8, f2); c.Set(3, 7, f1); c.Set(4, 7, f1);
        // front paws stretched out
        c.Rect(14, 9, 6, 2, f1); c.HL(14, 10, 6, f2); c.Set(19, 9, fl); c.Set(19, 10, fl); c.Set(16, 9, fl);
        // head with a pale muzzle and a floppy ear
        c.Ellipse(16.8, 6, 3.3, 3.0, f1);
        c.Rect(18, 6, 4, 3, fl); c.Set(21, 6, ink); c.Set(21, 7, f3);
        c.Set(18, 4, ink); c.Set(19, 4, fl);
        c.Rect(15, 4 + (frame == 1 ? 1 : 0), 2, 4, f3);
        c.HL(13, 7, 3, H("C05A4A"));           // collar
        // tail wags
        if (frame == 0) { c.Set(1, 6, f2); c.Set(0, 5, f2); c.Set(0, 4, f1); }
        else { c.Set(1, 7, f2); c.Set(0, 7, f2); c.Set(-1 + 1, 8, f1); }
        if (frame == 1) { c.Set(9, 3, f1); c.Set(10, 3, f1); }
        return Finish(c);
    }

    /// <summary>Sitting cat seen from the front-left, tail flick.</summary>
    public static Canvas Cat(int frame, bool grey = true)
    {
        var c = new Canvas(14, 16);
        Rgba b1 = grey ? H("A9A4B0") : H("D98F4B"), b2 = grey ? H("7F7A88") : H("B8683A"), b3 = grey ? H("5E5966") : H("8A4B2B"), b0 = grey ? H("CEC9D4") : H("E9AE72");
        c.Ellipse(6.5, 10.5, 4.2, 4.6, b1);
        c.Ellipse(6, 9.6, 3.0, 3.6, b0);
        c.Rect(8, 11, 3, 4, b2);
        c.Rect(3, 13, 3, 2, b0); c.Rect(6, 13, 3, 2, b1);
        c.Ellipse(6.5, 5.2, 3.6, 3.2, b1);
        c.Tri(3.5, 4, 3.5, 0.5, 6, 2.6, b1); c.Tri(9.5, 4, 9.5, 0.5, 7, 2.6, b2);
        c.Set(4, 1, H("E7A39A")); c.Set(9, 1, H("E7A39A"));
        c.Set(5, 5, H("2B2321")); c.Set(8, 5, H("2B2321")); c.Set(6, 6, H("E7A39A")); c.Set(7, 6, H("E7A39A"));
        if (frame == 1) { c.Set(5, 5, b1); c.Set(8, 5, b1); }
        // tail
        if (frame == 0) { c.Set(11, 14, b2); c.Set(12, 13, b2); c.Set(12, 12, b3); }
        else { c.Set(11, 14, b2); c.Set(12, 14, b2); c.Set(13, 13, b3); }
        c.Rect(4, 9, 2, 1, b2); c.Set(7, 8, b2);
        return Finish(c);
    }

    /// <summary>Little ground pigeon (8x8): 0 standing, 1 pecking.</summary>
    public static Canvas Pigeon(int pose, bool left = false)
    {
        var c = new Canvas(10, 9);
        var g1 = H("A29CAE"); var g2 = H("7D778A"); var g3 = H("5B566A"); var g0 = H("C9C4D2"); var neck = H("6FA0A0"); var fl = H("D98F8F");
        c.Ellipse(5, 5.2, 3.2, 2.2, g1);
        c.Ellipse(4.4, 4.7, 2.2, 1.4, g0);
        c.Set(7, 6, g2); c.Set(8, 6, g3); c.Set(8, 5, g3);                    // tail
        c.Set(2, 6, g2); c.Set(3, 6, g2);
        if (pose == 0) { c.Ellipse(3.2, 3.0, 1.6, 1.5, g2); c.Set(2, 2, g3); c.Set(1, 3, H("E8B84A")); c.Set(3, 4, neck); c.Set(2, 3, H("2B2321")); }
        else { c.Ellipse(2.4, 5.4, 1.5, 1.4, g2); c.Set(0, 6, H("E8B84A")); c.Set(1, 5, H("2B2321")); c.Set(2, 4, neck); }
        c.Set(4, 7, fl); c.Set(6, 7, fl); c.Set(4, 8, fl); c.Set(6, 8, fl);
        var o = Finish(c);
        return left ? o : o.FlipH();
    }

    // ---- named cast ---------------------------------------------------------------------------------------------------
    public static void Build()
    {
        S.Cur = "folk";
        void Add2(string n, Func<int, Canvas> f) { S.Add($"npc_c2_{n}_0", f(0)); S.Add($"npc_c2_{n}_1", f(1)); }

        // vendors (front, seen from the chest up behind their counters)
        Add2("vendor_veg", f => Front(new Look(SkinB, HGrey, Mustard, Brown, Apron: Green, Bald: true, Stache: true, Stout: 1), f));
        Add2("vendor_grain", f => Front(new Look(SkinC, HDark, Rust, Brown, Hat: "cap", Head: Brown, Beard: true, Stout: 1), f));
        Add2("vendor_bread", f => Front(new Look(SkinD, HBrown, Cream, Tan, Apron: Cream, Hat: "whitecap", Stout: 0), f));
        Add2("vendor_fruit", f => Front(new Look(SkinB, HBlack, Teal, Brown, Apron: Mustard, Hat: "cap", Head: Terra), f));
        Add2("vendor_pots", f => Front(new Look(SkinA, HDark, Blue, Navy, Head: Rose, Skirt: true, Bun: false), f));
        Add2("vendor_spice", f => Front(new Look(SkinE, HGrey, Terra, Rust, Head: Plum, Skirt: true, Scarf: Mustard), f));
        Add2("vendor_cloth", f => Front(new Look(SkinD, HAuburn, Plum, Navy, Stache: true, Stout: 0), f));

        // the friendly neighbour: plum dress, mustard headscarf, big smile, basket on her arm
        var neigh = new Look(SkinB, HBrown, Plum, Plum, Skirt: true, Apron: Cream, Head: Mustard, Stout: 1);
        Add2("neighbour", f => Front(neigh, f, 0, true));
        S.Add("npc_c2_neighbour_wave_0", Front(neigh, 0, 1, true));
        S.Add("npc_c2_neighbour_wave_1", Front(neigh, 1, 2, true));

        // shoppers & others
        Add2("shopper_basket", f => Side(new Look(SkinC, HBlack, Teal, Teal, Skirt: true, Head: Terra), f, basket: true));
        Add2("shopper_basket_l", f => Side(new Look(SkinA, HDark, Rust, Rust, Skirt: true, Bun: true), f, basket: true, left: true));
        Add2("shopper_sack", f => Side(new Look(SkinE, HBlack, Green, Brown, Stache: true), f, sack: true, left: true));
        Add2("shopper_front", f => Front(new Look(SkinD, HAuburn, Blue, Navy, Skirt: true, Stout: 0), f, 0, true));
        Add2("shopper_front_b", f => Front(new Look(SkinC, HBlack, Green, Brown, Hat: "cap", Head: Terra, Stache: true, Stout: 1), f));
        Add2("watcher", f => Back(new Look(SkinB, HDark, Grey, Navy, Skirt: true, Head: Cream, Scarf: Rose), f, 0));
        Add2("watcher_man", f => Back(new Look(SkinA, HBrown, Navy, Brown, Hat: "cap", Head: Grey), f));
        Add2("carter", f => Back(new Look(SkinC, HBlack, Teal, Brown, Hat: "straw"), f));
        Add2("talker_woman", f => Side(new Look(SkinB, HBrown, Terra, Brown, Skirt: true, Head: Rose, Scarf: Cream), f, left: false, lean: 1));
        Add2("talker_man", f => Side(new Look(SkinA, HGrey, Brown, Navy, Beard: true, Hat: "cap", Head: Tan), f, left: true, lean: 0));
        Add2("oldman", f => Front(new Look(SkinC, HGrey, Brown, Navy, Hat: "straw", Beard: true, Cane: true), f));
        Add2("child", f => Child(new Look(SkinB, HDark, Mustard, Green, Skirt: false), f, false));
        Add2("child_girl", f => Child(new Look(SkinA, HBrown, Rose, Plum, Skirt: true), f, true));
        S.Add("npc_c2_child_wave_0", Child(new Look(SkinB, HDark, Mustard, Green), 0, false, true));

        // animals
        Add2("dog", f => Dog(f));
        Add2("cat", f => Cat(f, true));
        Add2("cat_ginger", f => Cat(f, false));
        S.Add("npc_c2_pigeon_stand", Pigeon(0)); S.Add("npc_c2_pigeon_peck", Pigeon(1));
        S.Add("npc_c2_pigeon_stand_l", Pigeon(0, true)); S.Add("npc_c2_pigeon_peck_l", Pigeon(1, true));
    }
}
