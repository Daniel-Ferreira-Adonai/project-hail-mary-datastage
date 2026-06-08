using Godot;
using Godot.Collections;

[GlobalClass]
public partial class EnemyData : Resource
{
    [ExportCategory("Basic Info")]
    [Export] public string EnemyName { get; set; } = "Enemy";
    [Export] public Texture2D Sprite { get; set; }
    [Export] public int MaxHealth { get; set; } = 50;
    [Export] public int Gold { get; set; } = 10;
    [Export] private EnemyEnum enemyType = EnemyEnum.enemy;

    [ExportCategory("Custom Size")]
    [Export] public float CustomTargetHeight { get; set; } = 220f;
    [Export] public float CustomMaxWidth { get; set; } = 300f;
    [ExportCategory("Stats")]
    [Export] public int Strength { get; set; } = 0;
    
    [ExportCategory("Behavior")]
    [Export] public Array<EnemyTurn> TurnPatterns { get; set; }
    
    [ExportCategory("Visual")]
    [Export] public EnemySize Size { get; set; } = EnemySize.Medium;
    [Export] public Godot.Collections.Array<Texture2D> AttackFrames { get; set; } = new();


    [Export] public Color Tint { get; set; } = Colors.White;
    
    [Export] public float VerticalOffset { get; set; } = 0f;

    [ExportCategory("Powers")]
    [Export] public Array<EnemyPowerData> StartingPowers { get; set; } = new Array<EnemyPowerData>();
}


public enum EnemySize
{
    Tiny,      
    Small,     
    Medium,   
    Large,     
    Boss      ,
    Custom 
}