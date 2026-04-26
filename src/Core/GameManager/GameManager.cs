using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public EncounterData CurrentEncounter { get; private set; }

    [Export] private Node2D _combatScene;
    [Export] private Node2D _mapScene;
    
    private CombatManager _combatManager;

    public override void _Ready()
    {
        Instance = this;
        _combatManager = _combatScene.GetNode<CombatManager>("CombatManager");
        
        // começa no mapa
        _combatScene.Visible = false;
        _mapScene.Visible = true;
    }

    public void StartCombat(EncounterData encounter)
    {
        CurrentEncounter = encounter;
        _mapScene.Visible = false;
        _combatScene.Visible = true;
        // _combatManager.ResetCombat(encounter);
    }

    public void OnCombatVictory()
    {
        _combatScene.Visible = false;
        _mapScene.Visible = true;
    }
}

public enum GameState
{
    Map,
    Combat,
    Reward,
    GameOver
}