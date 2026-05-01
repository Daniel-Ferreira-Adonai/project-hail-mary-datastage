using System.Threading.Tasks;
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public EncounterData CurrentEncounter { get; private set; }

    [Export] private CombatManager _combatManager;
    [Export] private Map _map;

    public TopHud _topHud;
    public override void _Ready()
    {
        Instance = this;
        _topHud = UI.Instance.TopHud;
        ShowMap();
        CallDeferred(nameof(UpdateTopBar)); 

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

    public async void StartCombat(EncounterData encounter)
    {
        await UI.Instance.FadeOut();

        CurrentEncounter = encounter;
        _map.HideMap();
        _combatManager.Visible = true;
        _combatManager.ProcessMode = ProcessModeEnum.Inherit;
        PlayerManager.Instance.Player.Visible = true; 

        _combatManager.CallDeferred(nameof(CombatManager.InitializeCombat), encounter);
        
        UI.Instance.FadeIn();

    }

    public async void OnCombatVictory()
    {
        await UI.Instance.FadeOut();

        _combatManager.Visible = false;
        _combatManager.ProcessMode = ProcessModeEnum.Disabled;
        _map.ShowMap();

        PlayerManager.Instance.Player.Visible = false; 
        UI.Instance.FadeIn();

    }
     public void UpdateTopBar()
    {
        var player = PlayerManager.Instance.Player;
        if (player == null || _topHud == null) return;

        _topHud.UpdateTopBar(_map);
        
    }
}

public enum GameState
{
    Map,
    Combat,
    Reward,
    GameOver
}