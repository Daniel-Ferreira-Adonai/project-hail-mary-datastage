using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public EncounterData CurrentEncounter { get; private set; }

    
    [Export] private CombatManager _combatManager;

    public override void _Ready()
    {
        Instance = this;
        
        // começa no mapa
        var encounter = GD.Load<EncounterData>("res://Data/Encounters/TesteEncounter.tres");
        StartCombat(encounter);
    }

    public void StartCombat(EncounterData encounter)
    {
        CurrentEncounter = encounter;
        // _mapScene.Visible = false;
        _combatManager.Visible = true;
        _combatManager.ProcessMode = ProcessModeEnum.Inherit;
        _combatManager.CallDeferred(nameof(CombatManager.InitializeCombat), encounter);
        // _combatManager.ResetCombat(encounter);
    }

    public void OnCombatVictory()
    {
        _combatManager.Visible = false;
        _combatManager.ProcessMode = ProcessModeEnum.Disabled;
        // _mapScene.Visible = true;
    }
}

public enum GameState
{
    Map,
    Combat,
    Reward,
    GameOver
}