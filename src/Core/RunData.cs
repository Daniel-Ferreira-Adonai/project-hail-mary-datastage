using Godot;

public static class RunData
{
    public static CharacterData SelectedCharacter { get; set; }
    public static RunSaveData   PendingLoad       { get; set; }
    public static int           Tower             { get; set; } = 1;
    public static bool          Endless           { get; set; } = false;
}
