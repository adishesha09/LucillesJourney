namespace WorldArtCh6;

/// <summary>Building A (our room + stair lobby), the shop next door, neighbours and the rear roofs.</summary>
public static class Buildings
{
    static Canvas Cv => Ground.Cv;
    static void Wall(int x, int y, Rgba c, bool ext = false) { if (x < 0 || y < 0 || x >= G.W || y >= G.H) return; Cv.Set(x, y, c); Ground.M[x, y] = Mat.Wall; SetExt(x, y, ext); }
    static void SetExt(int x, int y, bool e) => ExtFlag(x, y, e);
    static void ExtFlag(int x, int y, bool e) => Ground.SetExtPublic(x, y, e);

    public static readonly (int x0, int y0, int w, int h)[] RoomWindows = { (84, 306, 28, 18), (210, 306, 28, 18) };

    public static void Paint()
    {
        RearRoofs();
        EaveAndFaces();
        Caps();
        Floors();
        Partition();
        SouthWall();
        RoomDecor();
        LobbyDecor();
        Shop();
        Neighbours();
        Alley();
    }

    // ---- rear roofs -----------------------------------------------------------------------------------
    static readonly int[] RearEdges = { 0, 150, 262, 410, 530, 650, 768 };
    static Rgba[] RearRamp(int i) => (i % 3) switch { 0 => E.DuskRoofWarm, 1 => E.DuskRoof, _ => E.DuskRoofGreen };

    static void RearRoofs()
    {
        for (int b = 0; b + 1 < RearEdges.Length; b++)
            for (int y = 0; y < 36; y++)
                for (int x = RearEdges[b]; x < RearEdges[b + 1]; x++)
                    Ground.PxPublic(x, y, Ground.RoofBlockPx(x, y, RearEdges[b], -6, RearEdges[b + 1], 36, -20, RearRamp(b), 71 + b), Mat.Roof, true);
        for (int y = 36; y < 48; y++)
            for (int x = 0; x < G.W; x++)
            {
                int v = y - 36;
                var pl = K.Plaster;
                var c = v == 0 ? pl[4] : v < 3 ? pl[2] : v == 11 ? pl[0] : pl[1];
                double n = Fx.VN(x / 7.0, y / 5.0, 59);
                if (v >= 3 && v < 11 && n > 0.7) c = pl[0];
                if (v >= 3 && v < 11 && ((x + 3) % 38 == 0)) c = pl[0];
                Ground.PxPublic(x, y, c, Mat.Wall, true);
            }
    }

    // ---- roof eave and north faces ---------------------------------------------------------------------------
    static void EaveAndFaces()
    {
        for (int y = Geo.EaveY; y < Geo.FaceY; y++)
            for (int x = Geo.AX0; x < Geo.AX1; x++)
            {
                var c = Ground.RoofPx(x, y, Geo.AX0, Geo.EaveY, K.Roof, 7);
                if (y == Geo.FaceY - 1) c = K.Wood[0];
                if (x < Geo.AX0 + 2 || x >= Geo.AX1 - 2) c = K.Roof[0];
                Wall(x, y, c, true);
            }
        for (int y = Geo.FaceY; y < Geo.FloorY; y++)
            for (int x = Geo.AX0 + 16; x < Geo.AX1 - 16; x++)
            {
                bool lobby = x >= Geo.LobbyX0;
                Wall(x, y, lobby ? WallFace(x, y, K.BlueWall, K.TealR, 2) : WallFace(x, y, K.Peach, K.Wood, 3), false);
            }
    }

    /// <summary>Interior wall face: plaster over a wainscot with rail and baseboard. 32 px tall, starting at FaceY.</summary>
    static Rgba WallFace(int x, int y, Rgba[] pl, Rgba[] wr, int lo)
    {
        int ly = y - Geo.FaceY;
        if (ly >= 23)
        {
            if (ly == 23) return wr[Math.Min(wr.Length - 1, lo + 2)];
            if (ly == 24) return wr[Math.Max(0, lo - 2)];
            if (ly >= 30) return wr[Math.Max(0, lo - 2)];
            int px = (x - 64) % 16;
            if (px < 0) px += 16;
            if (px == 0) return wr[Math.Max(0, lo - 2)];
            if (px == 1) return wr[Math.Min(wr.Length - 1, lo + 1)];
            if (ly == 25) return wr[Math.Min(wr.Length - 1, lo + 1)];
            if (px == 15) return wr[Math.Max(0, lo - 1)];
            return wr[lo];
        }
        int b = 3;
        double v = Fx.VN(x / 9.0, y / 6.0, 47) + Fx.Bay(x, y) * 0.07;
        int idx = v > 0.70 ? b - 1 : v < 0.20 ? b + 1 : b;
        if (ly == 0) return pl[0].Shade(0.5);
        if (ly == 1) return pl[1].Shade(0.4);
        if (ly == 2) return ((x + y) & 1) == 0 ? pl[1] : pl[2];
        if (ly == 3 && ((x + y) & 1) == 0) return pl[2];
        if (idx == b && Fx.R(x / 3, y / 2, 53) > 0.93) idx = b - 1;
        return pl[Math.Clamp(idx, 0, 4)];
    }

    // ---- side caps and partition -----------------------------------------------------------------------
    static void Caps()
    {
        for (int y = Geo.FaceY; y < 304; y++)
        {
            for (int x = Geo.AX0; x < Geo.AX0 + 16; x++) Wall(x, y, CapPx(x - Geo.AX0, y - Geo.FaceY, 16, 0), y >= 300);
            for (int x = Geo.AX1 - 16; x < Geo.AX1; x++) Wall(x, y, CapPx(x - (Geo.AX1 - 16), y - Geo.FaceY, 16, 1), true);
        }
    }

    static Rgba CapPx(int u, int v, int thick, int side)
    {
        int bl = 11, row = v / bl, lv = v % bl;
        int jx = 4 + (int)(Fx.R(row, side, 13) * (thick - 8));
        uint h = Fx.H(row, (u > jx) ? 1 : 0, 17);
        int idx = 3 + ((h & 3) == 0 ? 1 : (h & 3) == 1 ? -1 : 0);
        int outer = side == 0 ? u : thick - 1 - u, inner = thick - 1 - outer;
        if (lv == bl - 1) return K.Plaster[1];
        if (u == jx) return K.Plaster[1];
        if (outer == 0) return K.Plaster[0].Shade(0.3);
        if (inner == 0) return K.Plaster[0];
        if (inner == 1) return K.Plaster[1];
        if (lv == 0) idx++;
        if (outer == 1) idx = Math.Min(4, idx + 1);
        if (Fx.R(u, v, 19) > 0.965) idx--;
        return K.Plaster[Math.Clamp(idx, 0, 4)];
    }

    static void Floors()
    {
        for (int y = Geo.FloorY; y < Geo.FloorBot; y++)
        {
            for (int x = Geo.RoomX0; x < Geo.RoomX1; x++) Ground.PxPublic(x, y, Ground.PlankPx(x, y, K.Wood, 3), Mat.Plank, false);
            for (int x = Geo.LobbyX0; x < Geo.LobbyX1; x++) Ground.PxPublic(x, y, Ground.TilePx(x, y, Geo.LobbyX0, Geo.FloorY), Mat.Tile, false);
        }
    }

    static Rgba PartCapPx(int u, int w, int v)
    {
        int idx = 3;
        if (u == 0) idx = 4; else if (u == w - 1) idx = 0; else if (u == w - 2) idx = 1;
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

    static void Partition()
    {
        VertCap(Geo.PartX, Geo.FloorY, Geo.DoorY0, Geo.PartW, false, true);
        VertCap(Geo.PartX, Geo.DoorY1, Geo.FloorBot, Geo.PartW, true, false);
        // wooden threshold in the door gap
        for (int y = Geo.DoorY0; y < Geo.DoorY1; y++)
            for (int x = Geo.PartX; x < Geo.PartX + Geo.PartW; x++)
            {
                int u = x - Geo.PartX;
                var t = K.Wood[u == 0 ? 4 : u == Geo.PartW - 1 ? 2 : 3];
                if (u % 5 == 4) t = K.Wood[1];
                if (y == Geo.DoorY0 || y == Geo.DoorY1 - 1) t = K.Wood[1];
                Ground.PxPublic(x, y, t, Mat.Thresh, false);
            }
    }

    // ---- south wall (the facade seen from the street) -------------------------------------------------------
    static Rgba FrontCapPx(int x, int v)
    {
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

    static Rgba FacePlasterPx(int x, int y)
    {
        var pl = K.Plaster;
        if (y == 307) return pl[1];
        if (y == 308) return pl[2];
        int idx = 3;
        double v = Fx.VN(x / 8.0, y / 5.0, 59) + Fx.Bay(x, y) * 0.07;
        if (v > 0.72) idx = 2; else if (v < 0.22) idx = 4;
        if (y == 327) idx = 2;
        // a little damp staining near the foundation
        if (y > 318 && Fx.VN(x / 11.0, y / 3.0, 83) > 0.7) idx = 2;
        return pl[idx];
    }

    public static Rgba FoundationPx(int x, int y, int y0)
    {
        int row = (y - y0) / 4, ly = (y - y0) % 4;
        int off = row == 0 ? 0 : 6;
        int gx = x + off, id = 0, sx = -(int)(Fx.R(row, 0, 61) * 8);
        while (true)
        {
            int w = 9 + (int)(Fx.R(row, id + 100 * (x / 400), 62) * 6);
            if (gx < sx + w) break;
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
        return Rgba.Mul(K.Stone[Math.Clamp(idx, 0, 5)], 1.02, 0.97, 0.88);
    }

    static void Post(int x, int y, int w, int h)
    {
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                int idx = i == 0 ? 4 : i == w - 1 ? 1 : i == 1 ? 3 : 2;
                if (j == 0) idx = Math.Min(5, idx + 1);
                Wall(x + i, y + j, K.Wood[idx], true);
            }
    }

    static void SouthWall()
    {
        for (int x = Geo.AX0; x < Geo.AX1; x++)
        {
            bool gap = x >= Geo.FrontX0 && x < Geo.FrontX1;
            if (gap) continue;
            for (int y = 296; y < 304; y++) Wall(x, y, FrontCapPx(x, y - 296), true);
            for (int y = 304; y < 328; y++) Wall(x, y, FacePlasterPx(x, y), true);
            for (int y = 328; y < 336; y++) Wall(x, y, FoundationPx(x, y, 328), true);
            Wall(x, 304, K.Wood[4], true); Wall(x, 305, K.Wood[2], true); Wall(x, 306, K.Wood[1], true);
        }
        // doorway floor: worn stone threshold running through the wall
        for (int y = 296; y < 336; y++)
            for (int x = Geo.FrontX0; x < Geo.FrontX1; x++)
                Ground.PxPublic(x, y, Rgba.Mul(Ground.CobblePxPublic(x, y, true), 1.0, 1.0, 1.0), Mat.Thresh, y >= 316);
        Post(Geo.AX0, 296, 8, 40); Post(Geo.AX1 - 8, 296, 8, 40);
        Post(Geo.FrontX0 - 6, 296, 6, 40); Post(Geo.FrontX1, 296, 6, 40);
        foreach (var px in new[] { 160, 300 }) Post(px, 304, 5, 24);
        // open shutters beside the lit windows, belt course under the window sills
        foreach (var w in new[] { (84, 28), (210, 28), (416, 28) })
            for (int side = 0; side < 2; side++)
                for (int j = 0; j < 18; j++)
                    for (int i = 0; i < 5; i++)
                    {
                        int x = side == 0 ? w.Item1 - 5 + i : w.Item1 + w.Item2 + i, y = 306 + j;
                        int idx = i == (side == 0 ? 0 : 4) ? 3 : i == (side == 0 ? 4 : 0) ? 1 : 2;
                        if (j % 3 == 2) idx = Math.Max(0, idx - 1);
                        Wall(x, y, K.TealR[idx], true);
                    }
    }

    // ---- interior decoration painted on the room's north wall --------------------------------------------------
    static void RoomDecor()
    {
        // pot rail over the stove with a ladle and a pan
        int ry = 70;
        for (int x = 84; x < 132; x++) { Wall(x, ry, K.Iron[3]); Wall(x, ry + 1, K.Iron[1]); }
        foreach (var (hx, kind) in new[] { (90, 0), (104, 1), (118, 2) })
        {
            for (int j = 2; j < 5; j++) Wall(hx, ry + j, K.Iron[2]);
            if (kind == 0) { for (int j = 5; j < 10; j++) { Wall(hx - 1, ry + j, K.Iron[3]); Wall(hx, ry + j, K.Iron[2]); Wall(hx + 1, ry + j, K.Iron[1]); } Wall(hx, ry + 10, K.Iron[0]); }
            if (kind == 1) { for (int j = 5; j < 12; j++) Wall(hx, ry + j, K.Wood[3]); for (int i = -1; i < 2; i++) Wall(hx + i, ry + 12, K.Wood[2]); }
            if (kind == 2) { for (int j = 5; j < 8; j++) Wall(hx, ry + j, K.Iron[2]); Ground.DiscWall(hx, ry + 10, 4, K.Iron[2], K.Iron[3], K.Iron[0]); }
        }
        // small framed photo and a calendar east of the window
        Frame(268, 70, 14, 12, K.Wood[3], Rgba.Hex("C9D8D2"));
        for (int j = 0; j < 4; j++) for (int i = 0; i < 2; i++) Wall(272 + i * 4, 73 + j, i == 0 ? K.Terra : K.Blue);
        Wall(274, 74, K.Cream); Wall(278, 74, K.Cream);
        // towel on a hook beside the sink
        Wall(60, 72, K.Iron[2]); for (int j = 0; j < 8; j++) for (int i = 0; i < 4; i++) Wall(58 + i, 73 + j, i == 3 ? K.Linen[1] : i == 0 ? K.Linen[3] : K.Linen[2]);
        for (int i = 0; i < 4; i++) Wall(58 + i, 80, K.Teal);
    }

    static void Frame(int x, int y, int w, int h, Rgba[] wood, Rgba inner) => Frame(x, y, w, h, wood[3], inner);
    static void Frame(int x, int y, int w, int h, Rgba frame, Rgba inner)
    {
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++) Wall(x + i, y + j, frame);
        for (int j = 1; j < h - 1; j++) for (int i = 1; i < w - 1; i++) Wall(x + i, y + j, inner);
        for (int i = 0; i < w; i++) Wall(x + i, y + h - 1, K.Wood[1]);
    }

    // ---- lobby: neighbour's door, notice board, mail slots, staircase --------------------------------------------
    static void LobbyDecor()
    {
        // neighbour door (closed), painted sage
        Door(322, 66, 24, 30, BlueRamp: true);
        // second neighbour door at the far west end of the lobby wall is hidden behind our door leaf; mailboxes instead
        for (int i = 0; i < 4; i++) MailBox(352 + i * 8, 76);
        NoticeBoard(358, 62 + 14, 0);
        // stair alcove: dark opening in the wall with a rail, steps painted on the floor
        for (int y = 66; y < 96; y++)
            for (int x = 404; x < 452; x++)
            {
                double t = (y - 66) / 30.0;
                var c = Rgba.Mul(K.BlueWall[0], 0.32 + 0.28 * t, 0.34 + 0.28 * t, 0.46 + 0.28 * t);
                if (((x + y) & 1) == 0 && y % 6 == 0) c = Rgba.Mul(c, 0.9, 0.9, 0.95);
                Wall(x, y, c);
            }
        for (int y = 64; y < 96; y++) { Wall(402, y, K.Wood[4]); Wall(403, y, K.Wood[2]); Wall(452, y, K.Wood[2]); Wall(453, y, K.Wood[1]); }
        for (int x = 402; x < 454; x++) { Wall(x, 64, K.Wood[4]); Wall(x, 65, K.Wood[2]); }
        // landing light above (a pale lit slit suggesting the floor above)
        for (int x = 408; x < 448; x++) Wall(x, 67, E.Lobby[3].Shade(0.2));
        Steps(404, 96, 48, 5);
    }

    static void Steps(int x0, int y0, int w, int n)
    {
        int stepH = 7;
        for (int s = 0; s < n; s++)
        {
            int yTop = y0 + (n - 1 - s) * stepH;           // top step is the one nearest the wall
            for (int j = 0; j < stepH; j++)
                for (int x = x0; x < x0 + w; x++)
                {
                    int lx = x - x0;
                    bool riser = j >= 4;
                    Rgba c;
                    if (riser)
                    {
                        c = K.Wood[j == 4 ? 0 : 1];
                        if (j == stepH - 1) c = K.Wood[0];
                    }
                    else
                    {
                        c = K.Wood[j == 0 ? 5 : j == 1 ? 4 : 3];
                        if (Fx.R(lx / 6, s, 351) > 0.6 && j == 2) c = K.Wood[2];
                        if (lx % 17 == 5 && j > 1) c = K.Wood[2];
                    }
                    // higher steps sit deeper in the stairwell's shade
                    if (s >= 2) c = c.Shade(0.12 * (s - 1));
                    if (lx < 2 || lx >= w - 2) c = K.Wood[lx == 0 || lx == w - 1 ? 0 : 2];       // stringers
                    Ground.PxPublic(x, yTop + j, c, Mat.Plank, false);
                }
        }
    }

    static void Door(int x0, int y0, int w, int h, bool BlueRamp)
    {
        var ramp = BlueRamp ? K.BlueR : K.Wood;
        // frame
        for (int j = 0; j < h; j++) for (int i = -2; i < w + 2; i++) Wall(x0 + i, y0 + j, K.Wood[(i < 0 || i >= w || j < 2) ? 1 : 2]);
        for (int j = 2; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = 2;
                if (i == 0) idx = 3; else if (i == w - 1) idx = 1;
                // two raised panels
                bool panel = (i >= 3 && i < w - 3) && ((j >= 5 && j < 14) || (j >= 17 && j < h - 3));
                if (panel) { idx = 3; if (i == 3 || j == 5 || j == 17) idx = 4; if (i == w - 4 || j == 13 || j == h - 4) idx = 1; if (i > 3 && i < w - 4 && j > 5 && j < 13) idx = 2; }
                Wall(x0 + i, y0 + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
            }
        // number plate, peephole and handle
        for (int i = 0; i < 5; i++) { Wall(x0 + w / 2 - 2 + i, y0 + 3, K.Gold); Wall(x0 + w / 2 - 2 + i, y0 + 4, K.Orange); }
        Wall(x0 + w / 2, y0 + 8, K.Iron[0]);
        Wall(x0 + w - 5, y0 + 17, K.Gold); Wall(x0 + w - 5, y0 + 18, K.Orange);
        // threshold
        for (int i = -2; i < w + 2; i++) Wall(x0 + i, y0 + h - 1, K.Wood[0]);
    }

    static void MailBox(int x0, int y0)
    {
        for (int j = 0; j < 8; j++)
            for (int i = 0; i < 7; i++)
            {
                int idx = j == 0 ? 4 : j == 7 ? 0 : i == 0 ? 3 : i == 6 ? 1 : 2;
                Wall(x0 + i, y0 + j, K.Iron[Math.Clamp(idx, 0, 4)]);
            }
        for (int i = 1; i < 6; i++) Wall(x0 + i, y0 + 2, K.Iron[0]);
        Wall(x0 + 2, y0 + 5, K.Cream); Wall(x0 + 3, y0 + 5, K.Cream); Wall(x0 + 4, y0 + 5, K.Cream);
    }

    static void NoticeBoard(int x0, int y0, int variant)
    {
        int w = 26, h = 18;
        y0 -= 4;
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++) Wall(x0 + i, y0 + j, K.Wood[(i == 0 || j == 0) ? 4 : (i == w - 1 || j == h - 1) ? 1 : 3]);
        for (int j = 2; j < h - 2; j++)
            for (int i = 2; i < w - 2; i++)
            {
                var c = K.Sand;
                double n = Fx.VN(i / 2.0, j / 2.0, 91);
                if (n > 0.7) c = K.Tan; else if (n < 0.25) c = Rgba.Hex("D8C69A");
                Wall(x0 + i, y0 + j, c);
            }
        var papers = new[] { (4, 3, 7, 8, K.Cream), (13, 4, 6, 6, Rgba.Hex("F6E7A8")), (20, 3, 4, 9, Rgba.Hex("CFE3EA")), (6, 10, 8, 5, Rgba.Hex("F4D9CF")) };
        foreach (var (px, py, pw, ph, col) in papers)
        {
            for (int j = 0; j < ph; j++) for (int i = 0; i < pw; i++) Wall(x0 + px + i, y0 + py + j, col);
            for (int j = 2; j < ph - 1; j += 2) for (int i = 1; i < pw - 1; i++) if (Fx.R(i, j, px + py) > 0.3) Wall(x0 + px + i, y0 + py + j, K.Brown);
            Wall(x0 + px + pw / 2, y0 + py, K.Terra);
        }
    }

    // ---- the shop next door ------------------------------------------------------------------------------------
    static void Shop()
    {
        for (int y = 48; y < Geo.ShopFaceY; y++)
            for (int x = Geo.BX0; x < Geo.BX1; x++)
                Ground.PxPublic(x, y, Ground.RoofBlockPx(x, y, Geo.BX0, 48, Geo.BX1, Geo.ShopFaceY, 142, E.DuskRoofGreen, 91, 12, 8), Mat.Roof, true);
        // facade
        for (int y = Geo.ShopFaceY; y < Geo.FrontBot; y++)
            for (int x = Geo.BX0; x < Geo.BX1; x++)
            {
                Rgba c;
                if (y < 262) c = K.Plaster[y == Geo.ShopFaceY ? 4 : y == 261 ? 0 : 3];                      // cornice
                else if (y < 282) c = Rgba.Mul(K.TealR[y < 264 ? 3 : 1], 0.9, 0.95, 1.0);                    // sign band
                else if (y < 303) c = Awning(x, y);
                else if (y < 328) c = ShopWallPx(x, y);
                else c = FoundationPx(x, y, 328);
                Wall(x, y, c, true);
            }
        // pilasters at both ends
        for (int y = 262; y < Geo.FrontBot; y++)
        {
            for (int i = 0; i < 6; i++) { Wall(Geo.BX0 + i, y, K.Wood[i == 0 ? 4 : i == 5 ? 1 : 3], true); Wall(Geo.BX1 - 6 + i, y, K.Wood[i == 0 ? 4 : i == 5 ? 1 : 3], true); }
        }
    }

    static Rgba Awning(int x, int y)
    {
        int ly = y - 282;
        int stripe = ((x - Geo.BX0 - 6) / 9) & 1;
        Rgba[] ramp = stripe == 0 ? K.RedR : K.Linen;
        int idx = ly < 3 ? 3 : ly < 9 ? 2 : ly < 15 ? 2 : 1;
        if (ly == 0) idx = 4;
        if (ly >= 17) { int sc = (x - Geo.BX0 - 6) % 9; bool edge = sc < 1 || sc > 7; return ramp[edge ? 0 : 1]; }  // scalloped lower edge
        if ((x + y) % 7 == 0 && ly < 14) idx -= 1;
        return ramp[Math.Clamp(idx, 0, 4)];
    }

    static Rgba ShopWallPx(int x, int y)
    {
        var pl = K.Plaster;
        // wood-panelled lower facade and plaster above
        if (y < 304) return pl[0].Shade(0.6);               // shadow under the awning
        if (y < 306) return ((x + y) & 1) == 0 ? pl[0].Shade(0.4) : pl[1];
        int idx = 2;
        double v = Fx.VN(x / 8.0, y / 5.0, 59) + Fx.Bay(x, y) * 0.07;
        if (v > 0.74) idx = 1; else if (v < 0.22) idx = 3;
        return pl[idx];
    }

    // ---- neighbours left and right ---------------------------------------------------------------------------
    static void Neighbours()
    {
        foreach (var side in new[] { 0, 1 })
        {
            int x0 = side == 0 ? 0 : 736, x1 = side == 0 ? 32 : 768;
            for (int y = 48; y < 296; y++)
                for (int x = x0; x < x1; x++)
                    Ground.PxPublic(x, y, Ground.RoofBlockPx(x, y, x0, 48, x1, 296, side == 0 ? 160 : 120, side == 0 ? E.DuskRoofWarm : E.DuskRoof, 140 + side, 8, 8), Mat.Roof, true);
            for (int y = 296; y < 336; y++)
                for (int x = x0; x < x1; x++)
                {
                    Rgba c = y < 304 ? FrontCapPx(x, y - 296) : y < 328 ? FacePlasterPx(x, y) : FoundationPx(x, y, 328);
                    Wall(x, y, side == 0 ? Rgba.Mul(c, 1.0, 0.94, 0.9) : Rgba.Mul(c, 0.94, 0.98, 1.0), true);
                }
        }
    }

    static void Alley()
    {
        for (int y = 48; y < Geo.FrontBot; y++)
            for (int x = Geo.AlleyX0; x < Geo.AlleyX1; x++)
            {
                var c = Ground.CobbleDark(x, y);
                // drain channel along the middle
                int cx = (Geo.AlleyX0 + Geo.AlleyX1) / 2;
                if (Math.Abs(x - cx) <= 1 && y > 60) c = x == cx ? E.Cobble[0] : E.Cobble[1];
                // side-wall shadows
                if (x - Geo.AlleyX0 < 3 && y < 330) c = c.Shade(0.45 - (x - Geo.AlleyX0) * 0.1);
                Ground.PxPublic(x, y, c, Mat.Alley, true);
            }
        // back wall closing the alley
        for (int y = 48; y < 64; y++)
            for (int x = Geo.AlleyX0; x < Geo.AlleyX1; x++)
                Wall(x, y, y == 48 ? K.Plaster[1] : y < 60 ? K.Plaster[2].Shade(0.15) : K.Plaster[0], true);
    }

    // =====================================================================================================
    // emissive pass (windows, doors, lanterns) - painted after the dusk grade
    // =====================================================================================================
    public static void Emit()
    {
        SkyWindow(182, 66, 36, 23);
        foreach (var w in RoomWindows) LitWindow(w.x0, w.y0, w.w, w.h, 0);
        LitWindow(416, 306, 28, 18, 1);
        FrontDoorLamp();
        ShopFront();
        NeighbourWindows();
        ShadowedAlleyLamp();
    }

    static void SkyWindow(int x0, int y0, int w, int h)
    {
        // light spills onto the wall around the frame
        for (int j = -3; j < h + 5; j++)
            for (int i = -9; i < w + 9; i++)
            {
                int x = x0 + i, y = y0 + j;
                bool inFrame = i >= 0 && i < w && j >= 0 && j < h;
                if (!inFrame && ((x + y) & 1) == 0 && y > Geo.FaceY + 2 && y < 92 && Ground.M[x, y] == Mat.Wall) Over(x, y, Rgba.Mul(Cv.Get(x, y), 0.95, 1.0, 1.12));
            }
        // frame
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = 3;
                if (i == 0 || j == 0) idx = 4; else if (i == w - 1 || j == h - 1) idx = 1;
                Ground.LitPublic(x0 + i, y0 + j, K.Wood[idx]);
            }
        int px0 = x0 + 2, py0 = y0 + 2, pw = w - 4, ph = h - 5;
        for (int j = 0; j < ph; j++)
            for (int i = 0; i < pw; i++)
            {
                double t = (double)j / (ph - 1);
                double v = Math.Min(1.0, t * 1.25) + Fx.Bay(i, j) * 0.18;
                int gi = Math.Clamp((int)(v * (E.Sky.Length - 1) + 0.5), 0, E.Sky.Length - 1);
                Ground.LitPublic(px0 + i, py0 + j, E.Sky[E.Sky.Length - 1 - gi]);
            }
        // stars
        foreach (var (sx, sy) in new[] { (4, 1), (11, 2), (22, 1), (27, 3), (16, 0) }) if (sx < pw) Ground.LitPublic(px0 + sx, py0 + sy, Rgba.Hex("F4EAD2"));
        // crescent moon
        Ground.LitPublic(px0 + 24, py0 + 2, Rgba.Hex("FFF6D6")); Ground.LitPublic(px0 + 25, py0 + 2, Rgba.Hex("FFF6D6")); Ground.LitPublic(px0 + 24, py0 + 3, Rgba.Hex("FFF6D6"));
        // skyline silhouettes with a few lit windows
        var sil = new[] { (0, 7, 5), (5, 9, 4), (9, 5, 6), (15, 11, 3), (18, 7, 6), (24, 9, 5), (29, 6, 3) };
        var dark = Rgba.Hex("2C3566"); var darker = Rgba.Hex("232A55");
        foreach (var (bx, bh, bw) in sil)
            for (int j = 0; j < bh; j++)
                for (int i = 0; i < bw; i++)
                {
                    int x = px0 + bx + i, y = py0 + ph - 3 - bh + j;
                    if (bx + i >= pw) continue;
                    Ground.LitPublic(x, y, (i + j) % 7 == 0 ? darker : dark);
                }
        foreach (var (wx, wy) in new[] { (1, 4), (6, 3), (10, 1), (19, 2), (25, 4), (20, 5), (3, 6), (12, 8) })
            if (wx < pw) Ground.LitPublic(px0 + wx, py0 + ph - 12 + wy, Rgba.Hex("F2C46B"));
        // mullions
        int mx = px0 + pw / 2 - 1, my = py0 + ph / 2 - 1;
        for (int j = 0; j < ph; j++) { Ground.LitPublic(mx, py0 + j, K.Wood[4]); Ground.LitPublic(mx + 1, py0 + j, K.Wood[1]); }
        for (int i = 0; i < pw; i++) { Ground.LitPublic(px0 + i, my, K.Wood[4]); Ground.LitPublic(px0 + i, my + 1, K.Wood[1]); }
        // curtains, tied back (muted sage)
        var curt = K.TealR;
        for (int side = 0; side < 2; side++)
            for (int j = 0; j < ph - 1; j++)
            {
                int tie = ph - 7;
                int width = j < tie ? 4 : j < tie + 2 ? 3 : 4;
                if (j >= tie + 2) width = 3;
                for (int i = 0; i < width; i++)
                {
                    int ix = side == 0 ? px0 + i : px0 + pw - 1 - i;
                    int idx = 3 - (i == width - 1 ? 1 : 0) - (i == 1 && j % 4 < 2 ? 1 : 0);
                    if (j == tie || j == tie + 1) idx = 1;
                    Ground.LitPublic(ix, py0 + j, curt[Math.Clamp(idx, 0, curt.Length - 1)]);
                }
            }
        // sill
        for (int i = -2; i < w + 2; i++)
        {
            Ground.LitPublic(x0 + i, y0 + h, K.Plaster[4]);
            Ground.LitPublic(x0 + i, y0 + h + 1, K.Plaster[1]);
            Ground.LitPublic(x0 + i, y0 + h + 2, K.Plaster[0].Shade(0.5));
        }
    }

    static void Over(int x, int y, Rgba c) { if (x >= 0 && y >= 0 && x < G.W && y < G.H) Cv.Set(x, y, c); }

    /// <summary>Warm lit window seen from the street.</summary>
    static void LitWindow(int x0, int y0, int w, int h, int kind)
    {
        // soft glow on the plaster around it
        for (int j = -3; j < h + 4; j++)
            for (int i = -4; i < w + 4; i++)
            {
                int x = x0 + i, y = y0 + j;
                bool inFrame = i >= 0 && i < w && j >= 0 && j < h;
                if (!inFrame && ((x + y) & 1) == 0 && y > 304 && y < 328) Over(x, y, E.Glow(Cv.Get(x, y), E.Warm, 0.7));
            }
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = 3;
                if (i == 0 || j == 0) idx = 4; else if (i == w - 1 || j == h - 1) idx = 1;
                Ground.LitPublic(x0 + i, y0 + j, K.Wood[idx]);
            }
        int px0 = x0 + 2, py0 = y0 + 2, pw = w - 4, ph = h - 5;
        var glow = new[] { E.WarmHi, Rgba.Hex("FFE9A8"), Rgba.Hex("FBD684"), Rgba.Hex("F2B960") };
        for (int j = 0; j < ph; j++)
            for (int i = 0; i < pw; i++)
            {
                double t = (double)j / ph + Fx.Bay(i, j) * 0.12;
                int gi = Math.Clamp((int)(t * 4), 0, 3);
                if ((i + j) % 9 == 0 && i > 1 && j > 0 && j < ph - 1) gi = 0;
                Ground.LitPublic(px0 + i, py0 + j, glow[gi]);
            }
        // silhouette inside: a lamp shade and a head, so the room feels lived in
        if (kind == 0)
        {
            Ground.LitPublic(px0 + 3, py0 + 3, K.Terra); Ground.LitPublic(px0 + 4, py0 + 3, K.Terra); Ground.LitPublic(px0 + 2, py0 + 4, K.Terra);
            Ground.LitPublic(px0 + 3, py0 + 4, K.Terra); Ground.LitPublic(px0 + 4, py0 + 4, K.Terra); Ground.LitPublic(px0 + 5, py0 + 4, K.Terra);
        }
        int mx = px0 + pw / 2 - 1, my = py0 + ph / 2;
        for (int j = 0; j < ph; j++) { Ground.LitPublic(mx, py0 + j, K.Wood[4]); Ground.LitPublic(mx + 1, py0 + j, K.Wood[1]); }
        for (int i = 0; i < pw; i++) Ground.LitPublic(px0 + i, my, K.Wood[1]);
        // curtains
        for (int side = 0; side < 2; side++)
            for (int j = 0; j < ph; j++)
                for (int i = 0; i < 3; i++)
                {
                    int ix = side == 0 ? px0 + i : px0 + pw - 1 - i;
                    int idx = i == 2 ? 1 : (i == 0 && j % 4 < 2 ? 3 : 2);
                    Ground.LitPublic(ix, py0 + j, K.TealR[Math.Clamp(idx, 0, 4)]);
                }
        // sill
        for (int i = -2; i < w + 2; i++)
        {
            Ground.LitPublic(x0 + i, y0 + h, K.Plaster[4]);
            Ground.LitPublic(x0 + i, y0 + h + 1, K.Plaster[1]);
        }
        Ground.LightSource(x0 + w / 2, y0 + h + 14, 26, 11, 0.55);
    }

    static void FrontDoorLamp()
    {
        // wall lanterns on both jambs, glowing
        foreach (int lx in new[] { Geo.FrontX0 - 14, Geo.FrontX1 + 10 })
        {
            for (int j = 0; j < 8; j++)
                for (int i = 0; i < 5; i++)
                {
                    int x = lx + i, y = 308 + j;
                    bool frame = i == 0 || i == 4 || j == 0 || j == 7;
                    Ground.LitPublic(x, y, frame ? K.Iron[1] : (j < 4 ? E.WarmHi : E.Warm));
                }
            Ground.LightSource(lx + 2, 340, 30, 16, 0.75);
        }
        // house number plate above the door (non-readable squiggle)
        for (int i = 0; i < 14; i++) for (int j = 0; j < 7; j++) Ground.LitPublic(Geo.FrontX0 + 9 + i, 302 + j, j == 0 || j == 6 || i == 0 || i == 13 ? K.Wood[1] : K.Teal);
        foreach (var x in new[] { 3, 5, 6, 8, 10 }) Ground.LitPublic(Geo.FrontX0 + 9 + x, 305, K.Cream);
    }

    static void ShopFront()
    {
        // fascia sign: a swatch of cloth + needle, plus squiggle "lettering"
        for (int i = 0; i < 112; i++)
            for (int j = 0; j < 14; j++)
            {
                int x = 552 + i, y = 266 + j;
                bool edge = i == 0 || i == 111 || j == 0 || j == 13;
                Ground.LitPublic(x, y, edge ? K.Wood[0] : Rgba.Mul(K.Cream, 0.78, 0.82, 0.92));
            }
        var rnd = new Random(5);
        for (int g = 0; g < 14; g++)
        {
            int gx = 574 + g * 6; if (g >= 7) gx += 6;
            int h = 3 + rnd.Next(4);
            for (int j = 0; j < h; j++) Ground.LitPublic(gx, 280 - 4 - j, K.Brown2);
            Ground.LitPublic(gx + 1, 280 - 4 - h + 1, K.Brown2);
        }
        // small button/needle icons on both ends of the sign
        foreach (int ix in new[] { 560, 650 })
        {
            Ground.DiscLit(ix, 273, 4, K.Terra, K.Terra.Light(), K.Brown2);
            Ground.LitPublic(ix - 1, 272, K.Cream); Ground.LitPublic(ix + 1, 272, K.Cream); Ground.LitPublic(ix - 1, 274, K.Cream); Ground.LitPublic(ix + 1, 274, K.Cream);
        }
        // lit windows
        ShopWindow(510, 306, 64, 22, 0);
        ShopWindow(654, 306, 64, 22, 1);
        // the door: open, warm inside
        for (int j = 0; j < 30; j++)
            for (int i = -3; i < 35; i++)
            {
                int x = 584 + i, y = 306 + j;
                bool jamb = i < 0 || i >= 32;
                Ground.LitPublic(x, y, jamb ? K.Wood[i < 0 ? 4 : 1] : Interior(i, j));
            }
        for (int i = -3; i < 35; i++) { Ground.LitPublic(584 + i, 304, K.Wood[4]); Ground.LitPublic(584 + i, 305, K.Wood[2]); }
        // threshold step
        for (int i = -3; i < 35; i++) { Ground.LitPublic(584 + i, 336 - 2, K.Stone[4]); Ground.LitPublic(584 + i, 335, K.Stone[2]); }
        // shop bell
        Ground.LitPublic(622, 316, K.Gold); Ground.LitPublic(622, 317, K.Orange);
        Ground.LightSource(600, 349, 46, 17, 1.0);
        Ground.LightSource(542, 346, 40, 14, 0.7);
        Ground.LightSource(686, 346, 40, 14, 0.7);
    }

    static Rgba Interior(int i, int j)
    {
        // back wall, counter and a hanging cloth, warmly lit
        var wall = j < 20 ? Rgba.Hex("E9C890") : Rgba.Hex("B98B5A");
        if (j < 4) wall = Rgba.Hex("D8AE72");
        if (j >= 18 && j < 22) wall = Rgba.Hex("D9A468");   // counter edge
        if (j >= 22) wall = Rgba.Hex("8C6440");
        if (i >= 4 && i <= 11 && j >= 3 && j <= 17) wall = (i + j) % 5 == 0 ? Rgba.Hex("C76B5A") : Rgba.Hex("D9786A");     // hanging cloth
        if (i >= 18 && i <= 27 && j >= 4 && j <= 17) wall = (i % 3 == 0) ? Rgba.Hex("5E86A0") : Rgba.Hex("6B8FA8");
        double n = Fx.R(i, j, 8);
        if (n > 0.9) wall = Rgba.Mul(wall, 0.92, 0.92, 0.92);
        return wall;
    }

    static void ShopWindow(int x0, int y0, int w, int h, int variant)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                bool frame = i < 2 || j < 2 || i >= w - 2 || j >= h - 2;
                if (frame) { Ground.LitPublic(x0 + i, y0 + j, K.Wood[i < 2 || j < 2 ? 4 : 1]); continue; }
                double t = (double)j / h;
                var c = t < 0.4 ? Rgba.Hex("F8D790") : t < 0.75 ? Rgba.Hex("EFBE72") : Rgba.Hex("D9A05A");
                if (((i + j) & 1) == 0 && Fx.R(i, j, 4) > 0.8) c = Rgba.Hex("FFE9B0");
                Ground.LitPublic(x0 + i, y0 + j, c);
            }
        // glazing bars
        for (int i = 20; i < w - 2; i += 20) for (int j = 2; j < h - 2; j++) Ground.LitPublic(x0 + i, y0 + j, K.Wood[1]);
        if (variant == 0)
        {
            // mannequin in a blue coat + folded cloth stacks
            int mx = x0 + 12;
            Ground.DiscLit(mx, y0 + 6, 3, Rgba.Hex("E7C9A0"), Rgba.Hex("F2DDBC"), Rgba.Hex("B98F6C"));
            for (int j = 0; j < 10; j++) for (int i = -4; i <= 4; i++) Ground.LitPublic(mx + i, y0 + 9 + j, i < -1 ? Rgba.Hex("6B8FA8") : i > 2 ? Rgba.Hex("3E5A6E") : Rgba.Hex("4F7189"));
            Ground.LitPublic(mx, y0 + 19, K.Brown2);
            int[] cols = { 0xC0, 0x5A, 0x4A, 0xF2, 0xD2, 0x6B, 0x6F, 0xA8, 0x6B };
            for (int s = 0; s < 3; s++)
                for (int k = 0; k < 4; k++)
                {
                    var col = new[] { Rgba.Hex("C05A4A"), Rgba.Hex("F2D26B"), Rgba.Hex("6FA86B") }[s];
                    for (int i = 0; i < 12; i++) Ground.LitPublic(x0 + 28 + s * 11 + (i % 12 < 10 ? i : 9), y0 + 17 - k * 3, k % 2 == 0 ? col : Rgba.Mul(col, 0.85, 0.85, 0.85));
                }
        }
        else
        {
            // spools shelf + hanging jackets
            for (int s = 0; s < 3; s++) for (int i = 0; i < 6; i++) Ground.LitPublic(x0 + 6 + s * 8 + (i % 3), y0 + 5 + i / 3 * 3, new[] { K.Terra, K.Teal, K.Gold }[s]);
            for (int i = 0; i < 52; i++) Ground.LitPublic(x0 + 6 + i, y0 + 11, K.Wood[2]);
            foreach (var jx in new[] { 36, 46, 56 })
                for (int j = 0; j < 8; j++) for (int i = 0; i < 6; i++) Ground.LitPublic(x0 + jx + i, y0 + 3 + j, new[] { Rgba.Hex("C05A4A"), Rgba.Hex("6B8FA8"), Rgba.Hex("D2BE8E") }[(jx / 10) % 3].Shade(i > 3 ? 0.4 : 0));
        }
        // sill
        for (int i = -2; i < w + 2; i++) { Ground.LitPublic(x0 + i, y0 + h, K.Plaster[4]); Ground.LitPublic(x0 + i, y0 + h + 1, K.Plaster[1]); }
    }

    static void NeighbourWindows()
    {
        // west neighbour: a door and a dim window; east neighbour: lit window
        Door2(8, 300, 16, 28);
        LitWindowSmall(744, 308, 16, 14, true);
    }

    static void Door2(int x0, int y0, int w, int h)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = (i == 0 || j == 0) ? 3 : (i == w - 1) ? 0 : 2;
                if (i > 2 && i < w - 3 && j > 3 && j < h - 4 && ((i + j) & 1) == 0 && Fx.R(i, j, 5) > 0.7) idx = 1;
                Ground.LitPublic(x0 + i, y0 + j, Rgba.Mul(K.TealR[Math.Clamp(idx + 1, 0, 4)], 1, 1, 1));
            }
        Ground.LitPublic(x0 + w - 4, y0 + h / 2, K.Gold);
    }

    static void LitWindowSmall(int x0, int y0, int w, int h, bool lit)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                bool frame = i == 0 || j == 0 || i == w - 1 || j == h - 1;
                Ground.LitPublic(x0 + i, y0 + j, frame ? K.Wood[3] : lit ? (j < h / 2 ? E.WarmHi : E.Warm) : Rgba.Hex("3B4A72"));
            }
        for (int j = 1; j < h - 1; j++) Ground.LitPublic(x0 + w / 2, y0 + j, K.Wood[1]);
        Ground.LightSource(x0 + w / 2, 340, 16, 8, 0.4);
    }

    static void ShadowedAlleyLamp() { }
}
