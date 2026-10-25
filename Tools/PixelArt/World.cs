namespace PixelArt;

/// <summary>Group H: 16x16 world sprites for the top-down scene.</summary>
public static class World
{
    static Canvas N() => new(16, 16);
    static void Add(string n, Canvas c) => Registry.Add("H_world", n, c, 16, 16);
    static Canvas Fin(Canvas c) { c.Outline(P.Ink); return c; }

    static Canvas Planter(bool wet)
    {
        var c = N();
        c.Rect(1, 5, 14, 1, P.LightBrown);                  // back rim
        c.Rect(2, 6, 12, 2, wet ? P.DeepBrown : P.LightBrown);
        if (wet)
        {
            foreach (var (x, y) in new[] { (3, 6), (6, 7), (9, 6), (12, 7), (4, 7), (11, 6) }) c.Set(x, y, P.Brown);
            c.Set(5, 6, P.Blue); c.Set(10, 7, P.Blue); c.Set(13, 6, P.Blue);
        }
        else
        {
            foreach (var (x, y) in new[] { (3, 6), (5, 7), (7, 6), (10, 7), (12, 6) }) c.Set(x, y, P.Sand);
            foreach (var (x, y) in new[] { (4, 6), (9, 6), (11, 7), (13, 7), (6, 7) }) c.Set(x, y, P.Brown);
        }
        c.Rect(1, 8, 14, 7, P.MidBrown);                   // front of the box
        c.Rect(1, 8, 14, 1, P.LightBrown);
        c.Rect(1, 11, 14, 1, P.Brown);
        c.Rect(1, 14, 14, 1, P.Brown);
        c.Rect(1, 8, 2, 7, P.Brown); c.Rect(13, 8, 2, 7, P.Brown);
        c.Rect(1, 8, 2, 1, P.MidBrown); c.Rect(13, 8, 2, 1, P.MidBrown);
        if (wet)
        {
            c.Rect(7, 2, 2, 4, P.Green);
            c.Ellipse(5.4, 3.3, 2.4, 1.4, P.Green); c.Ellipse(10.8, 2.4, 2.4, 1.4, P.Green);
            c.Ellipse(8, 1.4, 1.5, 1.3, P.LightGreen);
            c.Set(8, 5, P.DarkGreen); c.Set(8, 4, P.DarkGreen);
            c.Set(4, 3, P.LightGreen); c.Set(10, 2, P.LightGreen);
        }
        else
        {
            // limp sprout: short stalk with two small leaves flopping down in dull ochre
            c.Rect(8, 3, 1, 3, P.DimOchre); c.Set(8, 5, P.Brown);
            c.Rect(5, 4, 3, 2, P.DimOchre); c.Rect(9, 3, 3, 2, P.DimOchre);
            c.Set(5, 6, Rgba.Clear); c.Set(11, 5, P.DimOchre); c.Set(5, 5, P.Grey); c.Set(6, 5, P.Grey); c.Set(10, 4, P.Grey); c.Set(11, 4, P.Grey);
        }
        return Fin(c);
    }

    static Canvas Plate(bool full)
    {
        var c = N();
        c.Disc(8, 8, 6.9, P.Cream);
        c.Paint((x, y) => x + y > 17 && c.Get(x, y) == P.Cream, P.Sand);
        c.Disc(8, 8, 5.0, P.Parchment);
        c.Paint((x, y) => x + y > 17 && c.Get(x, y) == P.Parchment, P.DarkParch);
        if (full)
        {
            // bread loaf
            c.Ellipse(6.4, 7.4, 3.5, 2.5, P.Orange);
            c.Paint((x, y) => y >= 8 && c.Get(x, y) == P.Orange, P.DarkOrange);
            c.Set(5, 6, P.LightOrange); c.Set(6, 6, P.LightOrange); c.Set(7, 6, P.LightOrange);
            c.Set(5, 8, P.Orange); c.Set(7, 8, P.Orange);
            // cup seen from above
            c.Disc(11.2, 10.3, 2.7, P.Teal);
            c.Disc(11.2, 10.3, 1.7, P.Brown);
            c.Set(10, 9, P.MidBrown);
            c.Set(14, 10, P.Teal); c.Set(14, 11, P.Teal);
        }
        else
        {
            foreach (var (x, y) in new[] { (6, 7), (7, 9), (9, 6), (10, 10), (5, 10), (8, 8) }) c.Set(x, y, P.Tan);
            c.Set(9, 8, P.Sand);
        }
        return Fin(c);
    }

    static Canvas Bag(bool open)
    {
        var c = N();
        c.Ellipse(8, 10.6, 6.4, 4.4, P.Sand);
        c.Paint((x, y) => x + y > 20 && c.Get(x, y) == P.Sand, P.Tan);
        c.Set(4, 10, P.Cream); c.Set(4, 11, P.Cream); c.Set(5, 9, P.Cream);
        if (open)
        {
            c.Ellipse(8, 7.2, 4.8, 1.9, P.DeepBrown);
            c.Poly(new[] { (5.0, 7.0), (6.4, 2.8), (8.2, 7.0) }, P.Terracotta);
            c.Rect(9, 1, 2, 5, P.Teal); c.Set(9, 1, P.LightTeal);
            c.Ellipse(11.6, 5.2, 2.0, 1.5, P.Orange); c.Set(11, 4, P.LightOrange);
            c.Rect(3, 8, 10, 1, P.Sand); c.Set(3, 8, Rgba.Clear); c.Set(12, 8, Rgba.Clear);
            c.Set(4, 7, P.Sand); c.Set(11, 7, P.Sand);
        }
        else
        {
            c.Poly(new[] { (5.2, 8.0), (10.8, 8.0), (10.0, 4.6), (6.0, 4.6) }, P.Sand);
            c.Poly(new[] { (5.4, 4.8), (6.4, 2.0), (8.0, 4.2), (9.6, 2.0), (10.6, 4.8) }, P.Sand);
            c.Rect(8, 4, 3, 4, P.Tan); c.Rect(10, 2, 1, 3, P.Tan);
            c.Rect(5, 6, 6, 1, P.Terracotta);
            c.Rect(7, 6, 2, 2, P.DarkTerra);
            c.Set(6, 8, P.Terracotta); c.Set(5, 9, P.Terracotta); c.Set(10, 8, P.Terracotta); c.Set(11, 9, P.Terracotta);
        }
        return Fin(c);
    }

    public static void Build()
    {
        Add("world_planter_dry", Planter(false));
        Add("world_planter_watered", Planter(true));
        Add("world_plate_full", Plate(true));
        Add("world_plate_empty", Plate(false));
        Add("world_bag_open", Bag(true));
        Add("world_bag_packed", Bag(false));

        // pending: soft gold sparkle ring (no outline, meant for dark floors)
        var pe = N();
        pe.Ring(8, 8, 6.9, 5.2, P.Gold);
        pe.Paint((x, y) => ((x + y) & 1) == 0 && (x + .5 - 8) * (x + .5 - 8) + (y + .5 - 8) * (y + .5 - 8) > 6.9 * 6.9
            && (x + .5 - 8) * (x + .5 - 8) + (y + .5 - 8) * (y + .5 - 8) <= 8.0 * 8.0, P.DarkGold);
        pe.Star(8, 8, 4.6, 1.2, 4, P.LightGold);
        pe.Rect(7, 7, 2, 2, P.White);
        foreach (var (x, y) in new[] { (3, 3), (12, 3), (3, 12), (12, 12) }) pe.Set(x, y, P.Cream);
        Add("world_pending", pe);

        // done: same marker, dim and desaturated with a tiny tick
        var dn = N();
        dn.Ring(8, 8, 6.9, 5.6, P.Grey);
        foreach (var (x, y) in new[] { (5, 8), (6, 9), (7, 10), (8, 9), (9, 8), (10, 7), (11, 6) })
        {
            dn.Set(x, y, P.LightGrey); dn.Set(x, y + 1, P.Grey);
        }
        Add("world_done", dn);

        // document: folded paper with a seal
        var doc = N();
        doc.Rect(3, 1, 10, 14, P.Parchment);
        doc.Rect(12, 2, 1, 13, P.DarkParch); doc.Rect(3, 14, 10, 1, P.DarkParch);
        doc.Tri(10, 1, 13, 4, 10, 4, P.Sand);
        doc.Tri(10, 1, 13, 1, 13, 4, Rgba.Clear);
        foreach (var y in new[] { 6, 8, 10 }) doc.Rect(5, y, y == 10 ? 3 : 6, 1, P.Tan);
        doc.Rect(9, 11, 3, 3, P.Terracotta); doc.Rect(11, 13, 1, 1, P.DarkTerra);
        Add("world_doc", Fin(doc));

        // food: bread loaf + small tin
        var food = N();
        food.Rect(9, 2, 6, 8, P.LightGrey);
        food.Rect(9, 2, 6, 2, P.Cream);
        food.Rect(9, 5, 6, 3, P.Terracotta);
        food.Rect(14, 4, 1, 6, P.Grey); food.Rect(9, 9, 6, 1, P.Grey);
        food.Set(9, 2, Rgba.Clear); food.Set(14, 2, Rgba.Clear);
        food.Rect(11, 6, 2, 1, P.Cream);
        food.Ellipse(6.6, 10.4, 6.0, 3.9, P.Orange);
        food.Paint((x, y) => y >= 12 && food.Get(x, y) == P.Orange, P.DarkOrange);
        foreach (var (x, y) in new[] { (3, 9), (4, 8), (4, 9), (6, 9), (7, 8), (7, 9), (9, 10), (10, 9), (9, 11) }) if (food.Get(x, y) == P.Orange) food.Set(x, y, P.LightOrange);
        foreach (var (x, y) in new[] { (4, 10), (5, 11), (7, 10), (8, 11), (10, 11) }) if (food.Opaque(x, y)) food.Set(x, y, P.DarkOrange);
        Add("world_food", Fin(food));

        // frame: small framed photo
        var fr = N();
        fr.RectN(2, 2, 12, 12, P.Gold);
        fr.Rect(2, 2, 12, 1, P.LightGold); fr.Rect(2, 2, 1, 12, P.LightGold);
        fr.Rect(13, 3, 1, 11, P.DarkGold); fr.Rect(3, 13, 11, 1, P.DarkGold);
        fr.Rect(4, 4, 8, 8, P.Brown);
        fr.Rect(5, 5, 6, 6, P.Blue);
        fr.Rect(9, 6, 2, 1, P.LightGold);
        fr.Tri(5, 11, 7.5, 7, 10, 11, P.DarkGreen);
        fr.Rect(5, 10, 6, 1, P.Green);
        Add("world_frame", Fin(fr));
    }
}
