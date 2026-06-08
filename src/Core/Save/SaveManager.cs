using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class CardSave
{
    public string Path     { get; set; } = "";
    public bool   Upgraded { get; set; }
}

public class RoomSave
{
    public int    Row          { get; set; }
    public int    Column       { get; set; }
    public float  PosX         { get; set; }
    public float  PosY         { get; set; }
    public int    RoomType     { get; set; }
    public string EncounterPath { get; set; } = "";
    public string EventPath    { get; set; } = "";
    public bool   Select       { get; set; }
    public List<int[]> NextRoomIds { get; set; } = new();
}

public class MapSave
{
    public int           Floor         { get; set; }
    public int           CurrentNodeId { get; set; } = -1;
    public List<RoomSave> Rooms        { get; set; } = new();
}

public class RunSaveData
{
    public int           SaveVersion   { get; set; } = 1;
    public string        CharacterName { get; set; } = "";
    public int           CurrentHp     { get; set; }
    public int           MaxHp         { get; set; }
    public int           Gold          { get; set; }
    public int           Tower         { get; set; } = 1;
    public bool          Endless       { get; set; } = false;
    public int           EnemiesKilled { get; set; }
    public List<CardSave> Deck         { get; set; } = new();
    public List<string>  Relics        { get; set; } = new();
    public List<string>  Powers        { get; set; } = new();
    public MapSave       Map           { get; set; } = new();
}

// ── Manager ───────────────────────────────────────────────────────────────────

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    private const string SavePath       = "user://savegame.json";
    private const int    CurrentVersion = 1;

    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented             = false,
        DefaultIgnoreCondition    = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public override void _Ready()
    {
        Instance = this;
        GetTree().AutoAcceptQuit = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
            GetTree().Quit();
    }

    public bool HasSave() => FileAccess.FileExists(SavePath);

    public void SaveRun(RunSaveData data)
    {
        data.SaveVersion = CurrentVersion;
        try
        {
            string json = JsonSerializer.Serialize(data, _json);
            using var f = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            f?.StoreString(json);
        }
        catch (System.Exception e)
        {
            GD.PushError($"SaveManager.SaveRun: {e.Message}");
        }
    }

    public RunSaveData LoadRun()
    {
        if (!HasSave()) return null;
        try
        {
            using var f = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (f is null) return null;
            var data = JsonSerializer.Deserialize<RunSaveData>(f.GetAsText(), _json);
            if (data is null || data.SaveVersion != CurrentVersion) return null;
            return data;
        }
        catch (System.Exception e)
        {
            GD.PushError($"SaveManager.LoadRun: {e.Message}");
            return null;
        }
    }

    public void DeleteSave()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        var dir = DirAccess.Open("user://");
        dir?.Remove("savegame.json");
    }
}
