using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public EncounterData CurrentEncounter { get; private set; }

    [Export] private CombatManager _combatManager;
    [Export] private Map _map;

    public override void _Ready()
    {
        Instance = this;
        ShowMap();
    }
    public void ShowMap()
    {
        _combatManager.Visible = false;
        _combatManager.ProcessMode = ProcessModeEnum.Disabled;
        PlayerManager.Instance.Player.Visible = false; 

        _map.ShowMap();
        
    }
    public void OnMapRoomSelected(Room room)
    {
        switch (room.EnumRoomType)
        {
            case Room.RoomType.Combat:
            case Room.RoomType.Boss:
                StartCombat(room.Encounter);
                break;
            case Room.RoomType.Shop:
                break;
            case Room.RoomType.CampFire:
                break;
            case Room.RoomType.Chest:
                break;
        }

        _map.UnlockNextRooms();
    }

    public void StartCombat(EncounterData encounter)
    {
        CurrentEncounter = encounter;
        _map.HideMap();
        _combatManager.Visible = true;
        _combatManager.ProcessMode = ProcessModeEnum.Inherit;
        PlayerManager.Instance.Player.Visible = true; 

        _combatManager.CallDeferred(nameof(CombatManager.InitializeCombat), encounter);
    }

    public void OnCombatVictory()
    {
        _combatManager.Visible = false;
        _combatManager.ProcessMode = ProcessModeEnum.Disabled;
        _map.ShowMap();
        _map.UnlockNextRooms();
    }
}

public enum GameState
{
    Map,
    Combat,
    Reward,
    GameOver
}