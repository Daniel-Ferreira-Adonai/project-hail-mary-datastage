using Godot;

public static class MetaProgress
{
    private const string Path = "user://meta.cfg";
    private const string Sect = "cutscenes";

    public static bool HasSeenCutscene(string id)
    {
        var c = new ConfigFile();
        c.Load(Path);
        return (bool)c.GetValue(Sect, id, false);
    }

    public static void MarkCutsceneSeen(string id)
    {
        var c = new ConfigFile();
        c.Load(Path);
        c.SetValue(Sect, id, true);
        c.Save(Path);
    }
}
