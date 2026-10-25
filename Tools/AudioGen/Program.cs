using AudioGen;

// Locate the project directory (contains AudioGen.csproj) and the Unity project root (two levels up).
string FindProjectDir()
{
    var d = new DirectoryInfo(AppContext.BaseDirectory);
    while (d != null && !File.Exists(Path.Combine(d.FullName, "AudioGen.csproj"))) d = d.Parent;
    return d?.FullName ?? throw new Exception("AudioGen.csproj not found above " + AppContext.BaseDirectory);
}

string projDir = FindProjectDir();
string root = Path.GetFullPath(Path.Combine(projDir, "..", ".."));
string audioDir = Path.Combine(root, "Assets", "Resources", "Audio");
string previewDir = Path.Combine(projDir, "out_preview");
Directory.CreateDirectory(audioDir);

var sw = System.Diagnostics.Stopwatch.StartNew();
Console.WriteLine("Output: " + audioDir);

Console.WriteLine("Music...");
foreach (var (name, data) in Music.RenderAll())
{
    Wav.Write(Path.Combine(audioDir, name + ".wav"), data, Dsp.MusicSr);
    Console.WriteLine($"  wrote {name} ({sw.Elapsed.TotalSeconds:0.0}s)");
}

Console.WriteLine("Ambience...");
foreach (var (name, data) in Ambience.RenderAll())
{
    Wav.Write(Path.Combine(audioDir, name + ".wav"), data, Dsp.MusicSr);
    Console.WriteLine($"  wrote {name} ({sw.Elapsed.TotalSeconds:0.0}s)");
}

Console.WriteLine("SFX / UI...");
foreach (var it in Sfx.RenderAll())
{
    var x = it.Data;
    Dsp.Fade(x, Dsp.SfxSr, 3, it.FadeOutMs);
    Dsp.Normalize(x, it.Db);
    Wav.Write(Path.Combine(audioDir, it.Name + ".wav"), x, Dsp.SfxSr);
}
Console.WriteLine($"  done ({sw.Elapsed.TotalSeconds:0.0}s)");

var (rows, failures) = Analysis.Run(audioDir);
Console.WriteLine("Previews -> " + previewDir);
Preview.RenderGroups(rows, previewDir);
return failures == 0 ? 0 : 1;
