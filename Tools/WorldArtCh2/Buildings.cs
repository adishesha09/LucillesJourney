namespace WorldArtCh2;

/// <summary>Shopfronts and houses baked into the ground image: roof, plaster facade, windows/shutters, doors, shop awnings.</summary>
public static class Buildings
{
    static Rgba H(string h) => Rgba.Hex(h);
    public static readonly Rgba[] Slate = { H("3E4858"), H("4F5C72"), H("62728C"), H("7F90A8"), H("A3B4C6") };
    public static readonly Rgba[] Ochre = { H("7E5226"), H("A06C30"), H("BC873C"), H("D4A24F"), H("E8C173") };
    public static readonly Rgba[] Ginger = { H("5A3E36"), H("7B5648"), H("9A6F5A"), H("B88A70"), H("D3A98A") };

    public sealed record Bld(int X, int W, int Base, int Top, string Kind, Rgba[] Wall, Rgba[] Roof, Rgba[] Shutter, Rgba[] Awn, int Door, bool Closed = false, int RoofH = 36);

    public static readonly List<Bld> All = new();

    public static void Paint(Canvas cv, Bld b)
    {
        All.Add(b);
        int eave = b.Top + b.RoofH;
        PaintRoof(cv, b, eave);
        PaintWall(cv, b, eave);
        PaintUpper(cv, b, eave);
        PaintGround(cv, b);
        // building edge outline (dark warm), left/right sides, so adjacent houses read separately
        for (int y = b.Top; y < b.Base; y++)
        {
            var l = cv.Get(b.X, y); var r = cv.Get(b.X + b.W - 1, y);
            cv.Set(b.X, y, l.Shade(0.55)); cv.Set(b.X + b.W - 1, y, r.Shade(0.8));
        }
        for (int x = b.X; x < b.X + b.W; x++) cv.Set(x, b.Top, cv.Get(x, b.Top).Shade(0.5));
    }

    static void PaintRoof(Canvas cv, Bld b, int eave)
    {
        var R = b.Roof;
        for (int y = b.Top; y < eave; y++)
            for (int x = b.X; x < b.X + b.W; x++)
            {
                int ry = y - b.Top;
                int tone;
                if (ry == 0) tone = 4;
                else if (ry == 1) tone = 3;
                else if (ry == 2) tone = 1;
                else
                {
                    int r = (ry - 3) / 5, ly = (ry - 3) % 5;
                    int off = (r & 1) * 3;
                    int lx = (x - b.X + off) % 7;
                    uint hsh = Fx.H((x - b.X + off) / 7, r, 5 + b.X);
                    tone = 2 + ((hsh & 3) == 0 ? 1 : (hsh & 3) == 1 ? -1 : 0);
                    if (ly == 0) tone++;
                    if (ly == 4) tone -= 2;
                    else if (ly == 3 && ((x + y) & 1) == 0) tone--;
                    if (lx == 0) tone--;
                    if (x - b.X < 10) tone++;
                    if (b.X + b.W - x < 8) tone--;
                    if (ry > b.RoofH - 8) tone -= 0;
                }
                if (y == eave - 1) tone = 0;
                cv.Set(x, y, R[Math.Clamp(tone, 0, R.Length - 1)]);
            }
        // moss/age speckle
        for (int y = b.Top + 4; y < eave - 2; y++) for (int x = b.X; x < b.X + b.W; x++) if (Fx.R(x, y, 71) > 0.985) cv.Set(x, y, R[3]);
    }

    static void PaintWall(Canvas cv, Bld b, int eave)
    {
        var W = b.Wall;
        for (int y = eave; y < b.Base; y++)
            for (int x = b.X; x < b.X + b.W; x++)
            {
                double n = Fx.VN(x / 9.0, y / 5.0, 21) * 0.6 + Fx.R(x, y, 22) * 0.2;
                int tone = n < 0.28 ? 1 : n < 0.62 ? 2 : 3;
                if (Fx.Bay(x, y) > 0.2 && tone == 2 && n > 0.5) tone = 3;
                int ey = y - eave;
                if (ey < 2) tone = 1;                               // eave shadow
                else if (ey < 4 && ((x + y) & 1) == 0) tone = Math.Max(0, tone - 1);
                if (x < b.X + 3) tone = Math.Min(W.Length - 1, tone + 1);
                if (x >= b.X + b.W - 4) tone = Math.Max(0, tone - 1);
                if (y > b.Base - 12 && Fx.R(x / 3, y, 23) > 0.55 && ((x + y) & 1) == 0) tone = Math.Max(0, tone - 1);
                cv.Set(x, y, W[Math.Clamp(tone, 0, W.Length - 1)]);
            }
        // stone plinth
        for (int y = b.Base - 6; y < b.Base; y++)
            for (int x = b.X; x < b.X + b.W; x++)
            {
                int row = (y - (b.Base - 6)) / 3, ly = (y - (b.Base - 6)) % 3;
                int sx = (x - b.X + row * 6) % 12;
                int tone = 3 - (int)(Fx.R((x - b.X + row * 6) / 12, row, 25) * 2);
                if (ly == 0) tone++;
                if (ly == 2 || sx == 0) tone = 1;
                cv.Set(x, y, K.Stone[Math.Clamp(tone, 0, 5)]);
            }
        // quoins at both corners
        for (int y = eave + 4; y < b.Base - 6; y += 6)
        {
            int w = ((y - eave) / 6 & 1) == 0 ? 5 : 3;
            cv.Rect(b.X + 1, y, w, 5, W[Math.Min(4, W.Length - 1)]); cv.HL(b.X + 1, y + 4, w, W[1]);
            int w2 = ((y - eave) / 6 & 1) == 0 ? 3 : 5;
            cv.Rect(b.X + b.W - 1 - w2, y, w2, 5, W[Math.Min(3, W.Length - 1)]); cv.HL(b.X + b.W - 1 - w2, y + 4, w2, W[1]);
        }
    }

    static void Window(Canvas cv, int cx, int y, bool closed, Rgba[] shutter, bool flowerBox, int seed)
    {
        int x = cx - 7;
        // frame
        cv.Rect(x - 1, y - 1, 16, 18, K.Cream); cv.Frame(x - 1, y - 1, 16, 18, K.Plaster[1]);
        if (!closed)
        {
            // glass: sky reflection, mullion, curtain
            for (int j = 0; j < 16; j++) for (int i = 0; i < 14; i++)
                {
                    int tone = (i + j) % 9 == 0 || (i + j) % 9 == 1 ? 3 : 2;
                    if (j > 10) tone = 1;
                    cv.Set(x + i, y + j, K.BlueR[Math.Clamp(tone + (i < 6 ? 0 : -1) + 0, 0, 4)]);
                }
            cv.VL(x + 6, y, 16, K.Cream); cv.VL(x + 7, y, 16, K.Plaster[1]); cv.HL(x, y + 7, 14, K.Cream);
            // curtain edge
            for (int j = 0; j < 8; j++) { cv.Set(x + 1, y + j, K.Terra); cv.Set(x + 2, y + j / 2 + 0, K.Terra); }
            if (Fx.R(seed, 1, 4) > 0.5) { cv.Rect(x + 8, y + 1, 5, 6, K.Linen[3]); cv.VL(x + 10, y + 1, 6, K.Linen[1]); }
            // open shutters flat on the wall
            Shutter(cv, x - 6, y - 1, 5, 18, shutter, false);
            Shutter(cv, x + 15, y - 1, 5, 18, shutter, false);
        }
        else
        {
            // closed: shutters meet over the glass, slats, rusty latch bar
            Shutter(cv, x, y, 7, 16, shutter, true);
            Shutter(cv, x + 7, y, 7, 16, shutter, true);
            cv.HL(x - 1, y + 7, 16, K.Iron[2]); cv.Set(x + 6, y + 7, K.Iron[4]); cv.Set(x + 7, y + 7, K.Iron[4]);
        }
        // sill
        cv.Rect(x - 2, y + 17, 18, 2, K.Stone[4]); cv.HL(x - 2, y + 18, 18, K.Stone[2]);
        if (flowerBox && !closed)
        {
            cv.Rect(x - 1, y + 14, 16, 5, K.TealR[2]); cv.HL(x - 1, y + 14, 16, K.TealR[3]); cv.HL(x - 1, y + 18, 16, K.TealR[0]);
            var fc = new[] { K.Terra, K.Gold, H("EAA5B5"), K.Cream };
            for (int i = 0; i < 8; i++) { cv.Set(x + i * 2, y + 12 + (i & 1), K.Leaf[3]); cv.Set(x + i * 2, y + 11 + (i & 1), fc[(i + seed) % 4]); cv.Set(x + i * 2 + 1, y + 13, K.Leaf[2]); }
        }
    }

    static void Shutter(Canvas cv, int x, int y, int w, int h, Rgba[] s, bool closedFace)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int tone = j % 3 == 0 ? 1 : (j % 3 == 1 ? 3 : 2);
                if (i == 0) tone++; if (i == w - 1) tone--;
                if (closedFace && Fx.R(i, j, 33) > 0.9) tone--;
                cv.Set(x + i, y + j, s[Math.Clamp(tone, 0, s.Length - 1)]);
            }
        cv.VL(x + w / 2, y + 1, h - 2, s[1]);
        if (closedFace) { cv.Line(x, y + h - 2, x + w - 1, y + 1, s[0]); }
    }

    static void Door(Canvas cv, int cx, int baseY, bool closed, int seed)
    {
        int x = cx - 8, y = baseY - 29;
        // step
        cv.Rect(x - 2, baseY - 2, 20, 2, K.Stone[4]); cv.HL(x - 2, baseY - 1, 20, K.Stone[2]);
        // frame + lintel
        cv.Rect(x - 1, y - 2, 18, 29, K.Wood[0]);
        cv.Rect(x - 2, y - 3, 20, 3, K.Stone[4]); cv.HL(x - 2, y - 3, 20, K.Stone[5]);
        for (int j = 0; j < 27; j++) for (int i = 0; i < 16; i++)
            {
                int tone = (i / 4) % 2 == 0 ? 3 : 2;
                if (i % 4 == 0) tone = 4; if (i % 4 == 3) tone = 1;
                if (j == 0) tone = 1;
                cv.Set(x + i, y + j, K.Wood[Math.Clamp(tone, 0, 5)]);
            }
        // panels
        cv.Frame(x + 2, y + 3, 12, 9, K.Wood[1]); cv.Frame(x + 2, y + 14, 12, 10, K.Wood[1]);
        cv.Rect(x + 3, y + 4, 10, 7, K.Wood[3]); cv.Rect(x + 3, y + 15, 10, 8, K.Wood[3]);
        cv.HL(x + 3, y + 4, 10, K.Wood[4]); cv.HL(x + 3, y + 15, 10, K.Wood[4]);
        cv.Set(x + 12, y + 14, K.Gold); cv.Set(x + 12, y + 15, K.Mustard[1]);
        if (closed)
        {
            // boarded across, chain
            cv.Rect(x - 3, y + 8, 22, 4, K.Pine[1]); cv.HL(x - 3, y + 8, 22, K.Pine[3]); cv.HL(x - 3, y + 11, 22, K.Pine[0]);
            cv.Rect(x - 3, y + 17, 22, 4, K.Pine[2]); cv.HL(x - 3, y + 17, 22, K.Pine[4]); cv.HL(x - 3, y + 20, 22, K.Pine[0]);
            for (int i = 0; i < 4; i++) { cv.Set(x - 1 + i * 6, y + 9, K.Iron[3]); cv.Set(x - 1 + i * 6, y + 18, K.Iron[3]); }
        }
    }

    static void ShopWindow(Canvas cv, int x0, int baseY, int w, bool closed, Rgba[] awn, string kind, int seed)
    {
        int y = baseY - 28;     // window top
        int h = 17;
        cv.Rect(x0 - 1, y - 1, w + 2, h + 2, K.Cream); cv.Frame(x0 - 1, y - 1, w + 2, h + 2, K.Plaster[1]);
        if (!closed)
        {
            for (int j = 0; j < h; j++) for (int i = 0; i < w; i++)
                {
                    int tone = ((i + j) % 12 < 2) ? 3 : 2;
                    if (j > h - 6) tone = 1;
                    cv.Set(x0 + i, y + j, kind == "bakery" ? K.Peach[Math.Clamp(tone + 1, 0, 4)] : K.BlueR[Math.Clamp(tone, 0, 4)]);
                }
            // wares inside
            for (int i = 0; i < 4; i++)
            {
                int gx = x0 + 3 + i * (w - 6) / 4;
                if (kind == "bakery") { cv.Ellipse(gx + 2, y + h - 4, 3.2, 2.2, K.Orange); cv.Set(gx + 1, y + h - 5, K.Gold); cv.HL(gx - 1, y + h - 2, 7, K.Wood[2]); }
                else { cv.Rect(gx, y + h - 6, 4, 4, new[] { K.Terra, K.Teal, K.Gold, K.Purple[2] }[i % 4]); cv.HL(gx - 1, y + h - 2, 7, K.Wood[2]); }
            }
            cv.VL(x0 + w / 2, y, h, K.Cream);
        }
        else
        {
            // boarded with planks, one hanging loose
            cv.Rect(x0, y, w, h, K.Wood[0]);
            for (int j = 0; j < 4; j++)
            {
                int py = y + 1 + j * 4;
                cv.Rect(x0, py, w, 3, K.Pine[1 + (j & 1)]); cv.HL(x0, py, w, K.Pine[3]); cv.HL(x0, py + 2, w, K.Pine[0]);
                cv.Set(x0 + 1, py + 1, K.Iron[3]); cv.Set(x0 + w - 2, py + 1, K.Iron[3]);
            }
            cv.Line(x0 + w - 8, y + 1, x0 + w - 3, y + h - 2, K.Wood[1]);
        }
        cv.Rect(x0 - 2, y + h + 1, w + 4, 2, K.Stone[4]); cv.HL(x0 - 2, y + h + 2, w + 4, K.Stone[2]);
        // cloth awning over the window (stripes) - rolled up and faded when closed
        int ay = y - 9, aw = w + 8, ax = x0 - 4;
        if (!closed)
        {
            for (int j = 0; j < 8; j++)
                for (int i = 0; i < aw; i++)
                {
                    int inset = (7 - j) / 4;
                    if (i < inset || i >= aw - inset) continue;
                    bool stripe = ((i + 2) / 5) % 2 == 0;
                    var r = stripe ? awn : K.Linen;
                    int tone = j < 2 ? 3 : j > 5 ? 1 : 2;
                    if (i < 4) tone++; if (i > aw - 5) tone--;
                    cv.Set(ax + i, ay + j, r[Math.Clamp(tone, 0, r.Length - 1)]);
                }
            for (int i = 0; i < aw; i++) { int sc = (i % 5 == 1 || i % 5 == 2 || i % 5 == 3) ? 1 : 0; for (int j = 0; j <= sc; j++) cv.Set(ax + i, ay + 8 + j, ((i + 2) / 5) % 2 == 0 ? awn[2] : K.Linen[2]); }
            // shadow of the awning on the window
            for (int i = 0; i < w; i++) if (((i + 0) & 1) == 0) cv.Set(x0 + i, y, cv.Get(x0 + i, y).Shade(1.0));
        }
        else
        {
            cv.Rect(ax, ay + 2, aw, 3, K.Linen[1]); cv.HL(ax, ay + 2, aw, K.Linen[3]); cv.HL(ax, ay + 4, aw, K.Linen[0]);
            for (int i = 2; i < aw; i += 6) cv.VL(ax + i, ay + 2, 3, K.Brown);
        }
    }

    static void PaintUpper(Canvas cv, Bld b, int eave)
    {
        int n = Math.Max(1, (b.W - 14) / 30);
        for (int i = 0; i < n; i++)
        {
            int cx = b.X + (int)(b.W * (i + 0.5) / n);
            Window(cv, cx, eave + 8, b.Closed, b.Shutter, !b.Closed && (i + b.X) % 2 == 0, i + b.X);
        }
        // string course
        int sy = eave + 30;
        cv.HL(b.X + 1, sy, b.W - 2, b.Wall[Math.Min(4, b.Wall.Length - 1)]); cv.HL(b.X + 1, sy + 1, b.W - 2, b.Wall[1]);
    }

    static void PaintGround(Canvas cv, Bld b)
    {
        bool shop = b.Kind is "bakery" or "store" or "tailor" or "closed";
        int cxDoor = b.X + (b.Door == 0 ? 20 : b.Door == 1 ? b.W / 2 : b.W - 20);
        if (shop)
        {
            int ww = 28;
            int wx = b.Door == 0 ? b.X + b.W - ww - 10 : b.X + 10;
            ShopWindow(cv, wx, b.Base, ww, b.Closed, b.Awn, b.Kind, b.X);
            Door(cv, cxDoor, b.Base, b.Closed, b.X);
            // hanging sign on an iron bracket next to the door
            int sx = b.Door == 0 ? b.X + 36 : b.X + b.W - 38;
            int sy = b.Base - 40;
            if (!b.Closed)
            {
                cv.HL(sx - 4, sy, 9, K.Iron[1]); cv.VL(sx - 4, sy, 2, K.Iron[1]);
                cv.VL(sx - 2, sy + 1, 3, K.Iron[2]); cv.VL(sx + 3, sy + 1, 3, K.Iron[2]);
                cv.Rect(sx - 4, sy + 4, 9, 7, K.Wood[3]); cv.Frame(sx - 4, sy + 4, 9, 7, K.Wood[1]);
                for (int i = 0; i < 4; i++) cv.Set(sx - 2 + i * 2 - (i & 1), sy + 7 + (i & 1), K.Brown2);
                cv.Set(sx - 2, sy + 5, b.Awn[2]); cv.Set(sx + 3, sy + 5, b.Awn[2]);
            }
            else
            {
                // sign hanging by one chain, faded and tilted
                cv.VL(sx - 3, sy + 1, 3, K.Iron[2]);
                cv.Line(sx - 4, sy + 4, sx + 4, sy + 9, K.Wood[2]); cv.Line(sx - 4, sy + 5, sx + 4, sy + 10, K.Wood[1]); cv.Line(sx - 4, sy + 6, sx + 4, sy + 11, K.Wood[2]);
                cv.Line(sx - 2, sy + 6, sx + 2, sy + 8, K.Brown2);
            }
        }
        else
        {
            // house: door centred with a window each side
            Door(cv, cxDoor, b.Base, false, b.X);
            int w0 = b.Door == 0 ? cxDoor + 26 : cxDoor - 26;
            for (int s = -1; s <= 1; s += 2)
            {
                int wx = (b.Door == 1 ? cxDoor + s * 28 : (s == 1 ? w0 : cxDoor - 28));
                if (wx < b.X + 12 || wx > b.X + b.W - 12) continue;
                Window(cv, wx, b.Base - 29, false, b.Shutter, true, wx);
            }
        }
    }
}
