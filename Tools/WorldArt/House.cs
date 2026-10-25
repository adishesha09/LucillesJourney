namespace WorldArt;

/// <summary>Walls, roof eave, windows, partitions and their cast light/shadow, painted straight into the ground layer.</summary>
public static class HouseArt
{
    static Canvas Cv => Ground.Cv;
    static void Wall(int x, int y, Rgba c) { if (x < 0 || y < 0 || x >= G.W || y >= G.H) return; Cv.Set(x, y, c); Ground.M[x, y] = Mat.Wall; }

    public static readonly (int x0, int w, int room)[] Windows = { (72, 22, 0), (146, 16, 0), (334, 20, 2) };

    public static void Paint()
    {
        Roof();
        NorthWall();
        SideWalls();
        FrontWall();
        Partitions();
        foreach (var w in Windows) Window(w.x0, w.w, w.room);
    }

    // ---- roof eave ------------------------------------------------------------------------------------------
    static void Roof()
    {
        for (int y = G.EaveY; y < G.FaceY; y++)
            for (int x = G.HX0; x < G.HX1; x++)
            {
                int row = (y - G.EaveY) / 8, ly = (y - G.EaveY) % 8;
                int off = row == 0 ? 0 : 4;
                int gx = x - G.HX0 + off, lx = gx % 8, id = gx / 8;
                uint h = Fx.H(id, row, 7);
                int b = 2 + ((h & 7) == 0 ? 1 : (h & 7) == 1 ? -1 : 0);
                Rgba c;
                if (row == 0 && ly == 0) c = K.Roof[0];                       // ridge shadow line
                else if (row == 0 && ly == 1) c = K.Roof[4];                  // ridge highlight
                else if (ly == 0) c = K.Roof[Math.Min(4, b + 1)];
                else if (ly <= 4) c = K.Roof[b];
                else if (ly <= 6) c = K.Roof[b - 1];
                else
                {   // scalloped lower edge
                    bool corner = lx <= 1 || lx >= 6;
                    c = corner ? (row == 0 ? K.Roof[1] : K.Roof[0]) : K.Roof[0];
                }
                if (ly >= 1 && ly <= 6 && lx == 0) c = K.Roof[Math.Max(0, b - 2)];                // vertical joint
                if (ly >= 1 && ly <= 4 && lx == 1) c = K.Roof[Math.Min(4, b + (b < 3 ? 1 : 0))];   // lit left edge of tile
                if (x < G.HX0 + 2 || x >= G.HX1 - 2) c = K.Roof[0];
                Wall(x, y, c);
            }
        // fascia board
        for (int x = G.HX0; x < G.HX1; x++) { Wall(x, G.FaceY - 1, K.Wood[0]); }
    }

    // ---- palettes -------------------------------------------------------------------------------------------
    static Rgba[] PlasterOf(int room) => room == 0 ? K.Plaster : room == 1 ? K.Peach : K.BlueWall;
    static int RoomAt(int x) => x < 220 ? 0 : x < 316 ? 1 : 2;

    // ---- north wall -----------------------------------------------------------------------------------------
    static void NorthWall()
    {
        for (int y = G.FaceY; y < G.FloorY; y++)
            for (int x = G.HX0 + 16; x < G.HX1 - 16; x++)
                Wall(x, y, WallFacePx(x, y, RoomAt(x), G.FaceY));
    }

    /// <summary>One pixel of an interior wall face. y0 = top of the face (32 px tall: plaster 21, rail, wainscot 8).</summary>
    static Rgba WallFacePx(int x, int y, int room, int y0)
    {
        var pl = PlasterOf(room);
        int ly = y - y0;
        if (ly >= 23)
        {   // wainscot
            Rgba[] wr = room == 0 ? K.TealR : room == 1 ? K.Wood : K.Linen;
            int lo = room == 0 ? 2 : room == 1 ? 3 : 2;
            if (ly == 23) return wr[Math.Min(wr.Length - 1, lo + 2)];       // rail highlight
            if (ly == 24) return wr[Math.Max(0, lo - 2)];                   // rail shade
            if (ly >= 30) return room == 2 ? K.Wood[3] : wr[Math.Max(0, lo - 2)]; // baseboard
            int px = (x - 64) % 16;
            if (px == 0) return wr[Math.Max(0, lo - 2)];
            if (px == 1) return wr[Math.Min(wr.Length - 1, lo + 1)];
            if (ly == 25) return wr[Math.Min(wr.Length - 1, lo + 1)];
            if (px == 15) return wr[Math.Max(0, lo - 1)];
            if (room == 1 && Fx.VN(x / 3.0, y / 9.0, 51) > 0.7) return wr[lo - 1];
            return wr[lo];
        }
        int b = 3;
        double v = Fx.VN(x / 9.0, y / 6.0, 47) + Fx.Bay(x, y) * 0.07;
        int idx = v > 0.70 ? b - 1 : v < 0.20 ? b + 1 : b;
        // eave shade under the roof
        if (ly == 0) return pl[0].Shade(0.5);
        if (ly == 1) return pl[1].Shade(0.4);
        if (ly == 2) return ((x + y) & 1) == 0 ? pl[1] : pl[2];
        if (ly == 3 && ((x + y) & 1) == 0) return pl[2];
        // subtle plaster stipple in clusters (looks like trowel marks)
        if (idx == b && Fx.R(x / 3, y / 2, 53) > 0.93) idx = b - 1;
        return pl[Math.Clamp(idx, 0, 4)];
    }

    // ---- exterior side walls (stone coping seen from above) -------------------------------------------------
    static void SideWalls()
    {
        for (int y = G.FaceY; y < 304; y++)
        {
            for (int x = G.HX0; x < G.HX0 + 16; x++) Wall(x, y, CapPx(x - G.HX0, y - G.FaceY, 16, 0));
            for (int x = G.HX1 - 16; x < G.HX1; x++) Wall(x, y, CapPx(x - (G.HX1 - 16), y - G.FaceY, 16, 1));
        }
    }

    /// <summary>Whitewashed stone coping. u = across wall, v = along wall. side 0 = outer edge on the left.</summary>
    static Rgba CapPx(int u, int v, int thick, int side)
    {
        int bl = 11, row = v / bl, lv = v % bl;
        int jx = 4 + (int)(Fx.R(row, side, 13) * (thick - 8));
        uint h = Fx.H(row, (u > jx) ? 1 : 0, 17);
        int idx = 3 + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        int outer = side == 0 ? u : thick - 1 - u;     // 0 at the outside edge
        int inner = thick - 1 - outer;
        if (lv == bl - 1) return K.Plaster[1];                     // horizontal mortar
        if (u == jx) return K.Plaster[1];                          // vertical joint
        if (outer == 0) return K.Plaster[0].Shade(0.3);            // outer edge
        if (inner == 0) return K.Plaster[0];                       // inner edge, towards the floor
        if (inner == 1) return K.Plaster[1];
        if (lv == 0) idx++;
        if (outer == 1) idx = Math.Min(4, idx + 1);
        if (Fx.R(u, v, 19) > 0.965) idx--;
        return K.Plaster[Math.Clamp(idx, 0, 4)];
    }

    static Rgba FrontCapPx(int x, int v)
    {
        // horizontal coping: blocks 8 px tall
        int bl = 18 + (int)(Fx.R(x / 20, 0, 23) * 6);
        int jx = (x / 20) * 20 + 4 + (int)(Fx.R(x / 20, 1, 24) * 12);
        uint h = Fx.H(x / 20, x > jx ? 1 : 0, 25);
        int idx = 3 + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        if (v == 0) return K.Plaster[4];
        if (v == 7) return K.Plaster[0];
        if (x == jx) return K.Plaster[1];
        if (v == 1) idx++;
        if (v == 6) idx--;
        return K.Plaster[Math.Clamp(idx, 0, 4)];
    }

    // ---- front (south) wall --------------------------------------------------------------------------------
    static void FrontWall()
    {
        for (int x = G.HX0; x < G.HX1; x++)
        {
            if (x >= G.DoorX0 && x < G.DoorX1) continue;
            for (int y = 296; y < 304; y++) Wall(x, y, FrontCapPx(x, y - 296));
            for (int y = 304; y < 312; y++) Wall(x, y, FacePlasterPx(x, y));
            for (int y = 312; y < 320; y++) Wall(x, y, FoundationPx(x, y));
        }
        // timber beam under the cap
        for (int x = G.HX0; x < G.HX1; x++)
        {
            if (x >= G.DoorX0 && x < G.DoorX1) continue;
            Wall(x, 304, K.Wood[4]); Wall(x, 305, K.Wood[2]); Wall(x, 306, K.Wood[1]);
        }
        // vertical posts (corners and jambs reach up through the coping)
        Post(G.HX0, 296, 8, 24); Post(G.HX1 - 8, 296, 8, 24);
        Post(G.DoorX0 - 6, 296, 6, 24); Post(G.DoorX1, 296, 6, 24);
        foreach (var px in new[] { 104, 264, 340 }) Post(px, 304, 5, 8);
    }

    static void Post(int x, int y, int w, int h)
    {
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                int idx = i == 0 ? 4 : i == w - 1 ? 1 : i == 1 ? 3 : 2;
                if (j == 0) idx = Math.Min(5, idx + 1);
                if (i == 0 || i == w - 1 || j == h - 1) { }
                Wall(x + i, y + j, K.Wood[idx]);
            }
    }

    static Rgba FacePlasterPx(int x, int y)
    {
        var pl = K.Plaster;
        if (y == 307) return pl[1];
        if (y == 308) return pl[2];
        int idx = 3;
        double v = Fx.VN(x / 8.0, y / 5.0, 59) + Fx.Bay(x, y) * 0.07;
        if (v > 0.72) idx = 2; else if (v < 0.22) idx = 4;
        if (y == 311) idx = 2;
        return pl[idx];
    }

    static Rgba FoundationPx(int x, int y)
    {
        int row = (y - 312) / 4, ly = (y - 312) % 4;
        int off = row == 0 ? 0 : 6;
        int gx = x + off, id = 0, sx = -(int)(Fx.R(row, 0, 61) * 8);
        while (true)
        {
            int w = 9 + (int)(Fx.R(row, id + 100 * (x / 400), 62) * 6);
            if (gx < sx + w + 0) break;
            sx += w; id++;
        }
        int lx = gx - sx;
        uint h = Fx.H(id, row, 63 + x / 120);
        if (ly == 3 && row == 1) return K.Stone[0];
        if (ly == 3) return K.Mortar;
        if (lx == 0) return K.Mortar;
        int idx = 2 + (int)(h % 3);
        if (ly == 0) idx++;
        if (ly == 2) idx--;
        if (row == 0 && ly == 0 && y == 312) idx = 5;
        return Rgba.Mul(K.Stone[Math.Clamp(idx, 0, 5)], 1.02, 0.97, 0.88);
    }

    // ---- partitions -----------------------------------------------------------------------------------------
    static Rgba PartCapPx(int u, int w, int v)
    {
        int idx = 3;
        if (u == 0) idx = 4;
        else if (u == w - 1) idx = 0;
        else if (u == w - 2) idx = 1;
        else if (Fx.VN(u * 2.0, v / 7.0, 71) > 0.78) idx = 2;
        return K.Plaster[idx];
    }

    static void VertCap(int x, int y0, int y1, int w, bool trimN, bool trimS)
    {
        for (int y = y0; y < y1; y++)
            for (int u = 0; u < w; u++)
            {
                Rgba c = PartCapPx(u, w, y);
                bool trim = (trimN && y < y0 + 4) || (trimS && y >= y1 - 4);
                if (trim)
                {
                    int ty = trimN && y < y0 + 4 ? y - y0 : y1 - 1 - y;
                    c = K.Wood[ty == 3 ? 1 : u == 0 ? 4 : u == w - 1 ? 1 : 3];
                    if (ty == 0) c = K.Wood[5];
                }
                Wall(x + u, y, c);
            }
    }

    static void Partitions()
    {
        // kitchen | living partition with a doorway
        VertCap(G.DivX, G.FloorY, G.DivGY0, 8, false, true);
        VertCap(G.DivX, G.DivGY1, G.FloorBot, 8, true, false);
        // bedroom west wall
        VertCap(G.BedWX, G.FloorY, G.BedWallY + 32, 8, false, false);
        // bedroom south wall: cap, face (living side) and doorway
        for (int x = G.BedWX + 8; x < G.HX1 - 16; x++)
        {
            bool gap = x >= G.BedGX0 && x < G.BedGX1;
            for (int y = G.BedWallY; y < G.BedWallY + 8; y++)
            {
                int v = y - G.BedWallY;
                if (gap)
                {   // lintel beam
                    int idx = v == 0 ? 5 : v == 7 ? 0 : v < 3 ? 4 : v < 6 ? 3 : 2;
                    Wall(x, y, K.Wood[idx]);
                }
                else Wall(x, y, K.Plaster[v == 0 ? 4 : v == 7 ? 0 : v == 6 ? 1 : 3]);
            }
            if (gap)
            {
                // shaded doorway: floor continues, with the lintel's shadow on it
                for (int y = G.BedWallY + 8; y < G.BedWallY + 32; y++)
                {
                    var f = Cv.Get(x, y);
                    int d = y - (G.BedWallY + 8);
                    if (d < 3 || (d == 3 && ((x + y) & 1) == 0)) Cv.Set(x, y, f.Shade(d < 2 ? 0.9 : 0.6));
                }
                continue;
            }
            for (int y = G.BedWallY + 8; y < G.BedWallY + 32; y++)
                Wall(x, y, WallFacePx(x, y, 1, G.BedWallY + 8 - 8));
        }
        // trims either side of the doorway
        for (int y = G.BedWallY + 8; y < G.BedWallY + 32; y++)
        {
            for (int i = 0; i < 3; i++)
            {
                Wall(G.BedGX0 + i, y, K.Wood[i == 0 ? 4 : i == 1 ? 3 : 1]);
                Wall(G.BedGX1 - 3 + i, y, K.Wood[i == 0 ? 4 : i == 1 ? 2 : 1]);
            }
        }
    }

    // ---- windows -------------------------------------------------------------------------------------------
    static void Window(int x0, int w, int room)
    {
        Rgba[] shut = room == 0 ? K.TealR : room == 1 ? K.RedR : K.BlueR;
        Rgba[] curt = room == 0 ? K.RedR : room == 1 ? K.Linen : K.Linen;
        int y0 = 68, h = 19;           // outer frame
        int sw = w >= 20 ? 6 : 4;       // shutter width

        // warm glow on the plaster around the window
        for (int y = y0 - 3; y < y0 + h + 4; y++)
            for (int x = x0 - sw - 3; x < x0 + w + sw + 3; x++)
            {
                if (y < G.FaceY + 3 || y > 86) continue;
                bool inFrame = x >= x0 - sw && x < x0 + w + sw && y >= y0 && y < y0 + h;
                if (!inFrame && ((x + y) & 1) == 0 && Ground.M[x, y] == Mat.Wall) Cv.Set(x, y, Cv.Get(x, y).Light(0.7));
            }

        // shutters (open, flat against the wall)
        for (int side = 0; side < 2; side++)
        {
            int sx = side == 0 ? x0 - sw : x0 + w;
            for (int j = 0; j < h; j++)
                for (int i = 0; i < sw; i++)
                {
                    int idx = 2;
                    if (i == 0) idx = side == 0 ? 3 : 2; else if (i == sw - 1) idx = 1;
                    if (j % 3 == 2) idx = Math.Max(0, idx - 1);           // louvre slats
                    if (j == 0) idx = 3;
                    if (j == h - 1) idx = 0;
                    Wall(sx + i, y0 + j, shut[idx]);
                }
        }
        // frame
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = 3;
                if (i == 0 || j == 0) idx = 4; else if (i == w - 1 || j == h - 1) idx = 1;
                Wall(x0 + i, y0 + j, K.Wood[idx]);
            }
        // pane with warm daylight gradient
        int px0 = x0 + 2, py0 = y0 + 2, pw = w - 4, ph = h - 5;
        var glow = new[] { Rgba.Hex("FFF8D6"), Rgba.Hex("FDEFB4"), Rgba.Hex("F9E093"), Rgba.Hex("F2D26B") };
        for (int j = 0; j < ph; j++)
            for (int i = 0; i < pw; i++)
            {
                double t = (double)j / ph + Fx.Bay(i, j) * 0.12;
                int gi = Math.Clamp((int)(t * 4), 0, 3);
                // sunlit diagonal streak
                if ((i + j) % 9 == 0 && i > 1 && j > 0 && j < ph - 1) gi = 0;
                Wall(px0 + i, py0 + j, glow[gi]);
            }
        // mullions
        int mx = px0 + pw / 2 - 1, my = py0 + ph / 2 - 1;
        for (int j = 0; j < ph; j++) { Wall(mx, py0 + j, K.Wood[4]); Wall(mx + 1, py0 + j, K.Wood[1]); }
        for (int i = 0; i < pw; i++) { Wall(px0 + i, my, K.Wood[4]); Wall(px0 + i, my + 1, K.Wood[1]); }
        // curtains tied back at both sides
        int cw = w >= 20 ? 4 : 3;
        for (int side = 0; side < 2; side++)
            for (int j = 0; j < ph - 1; j++)
            {
                int tie = ph - 6;
                int width = j < tie ? cw : (j < tie + 2 ? cw - 1 : cw);
                if (j >= tie + 2) width = cw - 1;
                for (int i = 0; i < width; i++)
                {
                    int ix = side == 0 ? px0 + i : px0 + pw - 1 - i;
                    int idx = 3 - (i == width - 1 ? 1 : 0) - (i == 1 && j % 4 < 2 ? 1 : 0);
                    if (j == tie || j == tie + 1) idx = 1;
                    Wall(ix, py0 + j, curt[Math.Clamp(idx, 0, curt.Length - 1)]);
                }
            }
        // sill
        for (int i = -2; i < w + 2; i++)
        {
            Wall(x0 + i, y0 + h, K.Plaster[4]);
            Wall(x0 + i, y0 + h + 1, K.Plaster[1]);
            Wall(x0 + i, y0 + h + 2, K.Plaster[0].Shade(0.5));
        }
        // flower pot on the sill of the sink window
        if (x0 == 72)
        {
            for (int i = 0; i < 5; i++) for (int j = 0; j < 4; j++) Wall(x0 + 12 + i, y0 + h - 4 + j, K.Roof[j == 0 ? 3 : i == 4 ? 1 : 2]);
            Wall(x0 + 14, y0 + h - 7, K.Green); Wall(x0 + 13, y0 + h - 6, K.DGreen); Wall(x0 + 15, y0 + h - 6, K.Green); Wall(x0 + 14, y0 + h - 5, K.DGreen);
            Wall(x0 + 14, y0 + h - 8, K.Gold); Wall(x0 + 12, y0 + h - 7, Rgba.Hex("EAA5B5"));
        }
    }

    // ---- interior shadows and sun patches -------------------------------------------------------------------
    public static void InteriorShadows()
    {
        Ground.ShadeRect(64, G.FloorY, 352, 3, 0.9);                        // under the north wall
        Ground.ShadeRect(64, G.FloorY, 3, 200, 0.8);                        // east of the west wall
        Ground.ShadeRect(G.DivX + 8, G.FloorY, 3, G.DivGY0 - G.FloorY, 0.8);
        Ground.ShadeRect(G.DivX + 8, G.DivGY1, 3, G.FloorBot - G.DivGY1, 0.8);
        Ground.ShadeRect(G.BedWX + 8, G.FloorY, 3, G.BedWallY - G.FloorY, 0.8);   // in the bedroom
        Ground.ShadeRect(G.BedGX1, G.BedWallY + 32, G.HX1 - 16 - G.BedGX1, 4, 0.9);   // south of the bedroom wall (living side)
        // sun shafts from the windows
        foreach (var w in Windows)
        {
            double a = w.x0 + 1, b = w.x0 + w.w - 1;
            Ground.LightPoly(new[] { (a + 2, 103.0), (b + 2, 103.0), (b + 28, 146.0), (a + 28, 146.0) }, 0.9);
        }
    }
}
