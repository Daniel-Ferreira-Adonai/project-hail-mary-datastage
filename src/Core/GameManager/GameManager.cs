using System.Threading.Tasks;
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public EncounterData CurrentEncounter { get; private set; }

    [Export] PackedScene chestRewardScene;
    [Export] private PackedScene _shopScene;
    [Export] private PackedScene _campFireScene;
    [Export] private PackedScene _deckViewerScene;

    [Export] private CombatManager _combatManager;
    [Export] private Map _map;
    [Export] private PackedScene _eventScene;
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
    public async void ShowMapFade()
    {
        PlayerManager.Instance.Player.Visible = false; 
        _map.ShowMap();

        await UI.Instance.FadeOut();

        _combatManager.Visible = false;
        _combatManager.ProcessMode = ProcessModeEnum.Disabled;
        PlayerManager.Instance.Player.Visible = false; 

        _map.ShowMap();
        
        UI.Instance.FadeIn();

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
                ShowShop();
                break;
            case Room.RoomType.CampFire:
                ShowCampFire();
                break;
            case Room.RoomType.Chest:
                ChestRoom();
                break;
            case Room.RoomType.Event:
                ShowEvent(room.Event);
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
    public async void ShowShop()
    {
        _shopScene ??= GD.Load<PackedScene>("res://src/Core/Shop/Shop.tscn");

        await UI.Instance.FadeOut();

        var shop = _shopScene.Instantiate<Shop>();
        UI.Instance.AddUI(shop);
        _map.HideMap();

        UI.Instance.FadeIn();

        shop.ExitRequested += () =>
        {
            shop.QueueFree();
            ShowMapFade();
        };
    }
    public async void ShowEvent(EventData eventData)
    {
        await UI.Instance.FadeOut();

        var eventUI = _eventScene.Instantiate<EventUi>();
        UI.Instance.AddUI(eventUI);
        _map.HideMap();

        UI.Instance.FadeIn();
        eventUI.LoadEvent(eventData);

        eventUI.ExitRequested += async () =>
    {
        await UI.Instance.FadeOut();
        eventUI.QueueFree();
        ShowMapFade();
    };
    }
    public async void ShowCampFire()
    {
        _campFireScene ??= GD.Load<PackedScene>("res://src/Core/CampFire/CampFire.tscn");

        await UI.Instance.FadeOut();

        var campFire = _campFireScene.Instantiate<CampFire>();
        UI.Instance.AddUI(campFire);
        _map.HideMap();
        PlayerManager.Instance.Player.Visible = false;

        UI.Instance.FadeIn();

        campFire.ExitRequested += () =>
        {
            campFire.QueueFree();
            ShowMapFade();
        };
    }

     public async void ChestRoom()
    {
        await UI.Instance.FadeOut();
        var chest = chestRewardScene.Instantiate<RelicChestReward>();
        UI.Instance.AddUI(chest); 
        _map.HideMap();
        PlayerManager.Instance.Player.Visible = true; 
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
     public async void GoToMainMenu()
    {
        await UI.Instance.FadeOut();
        GetTree().ChangeSceneToFile("res://src/Core/Menu/MainMenu.tscn");
    }

    public void UpdateTopBar()
    {
        var player = PlayerManager.Instance.Player;
        if (player == null || _topHud == null) return;

        _topHud.UpdateTopBar(_map);

    }

    public void ShowDeckViewer(DeckViewer.ViewerMode mode = DeckViewer.ViewerMode.Inspect)
    {
        _deckViewerScene ??= GD.Load<PackedScene>("res://src/Core/Inventory/DeckViewer.tscn");

        var viewer = _deckViewerScene.Instantiate<DeckViewer>();
        viewer.Mode = mode;
        if (mode == DeckViewer.ViewerMode.Remove)
        {
            viewer.CardRemoved += card => PlayerManager.Instance.Player.RemoveCardFromDeck(card);
        }
        viewer.ExitRequested += () => viewer.QueueFree();
        UI.Instance.AddUI(viewer);

        viewer.LoadDeck(PlayerManager.Instance.Player.GetDeck());
    }
}

public enum GameState
{
    Map,
    Combat,
    Reward,
    GameOver
}
