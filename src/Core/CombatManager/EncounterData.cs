using Godot;
using Godot.Collections;

[GlobalClass]
public partial class EncounterData : Resource
{
    [ExportCategory("Encounter Info")]
    [Export] public string EncounterName { get; set; } = "Random Encounter";
    
    [ExportCategory("Enemies")]
    [Export] public Array<EnemyData> Enemies { get; set; } = new();
    [Export] public Vector2[] EnemyPositions { get; set; }
    
    [ExportCategory("Rewards")]
    [Export] public int RewardGold { get; set; } = 50;
    [Export] public int CardRewardCount { get; set; } = 3; 
    [Export] public bool IsElite { get; set; } = false; 
    [Export] public bool IsBoss { get; set; } = false;
    
    [ExportCategory("Difficulty")]
    [Export] public int MinFloor { get; set; } = 1; 
    [Export] public int MaxFloor { get; set; } = 99;
    [Export] public float SpawnWeight { get; set; } = 1.0f; 
}