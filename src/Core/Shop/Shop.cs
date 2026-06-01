using Godot;
using System;
using System.Collections.Generic;

public partial class Shop : Control
{
    private const int CardCount = 4;
    private const int RelicCount = 2;

    [Export] private PackedScene _cardDisplayScene;
    [Export] private PackedScene _relicScene;
    [Export] private Godot.Collections.Array<CardData> _cardPool = [];
    [Export] private int _minCardPrice = 25;
    [Export] private int _maxCardPrice = 55;
    [Export] private int _minRelicPrice = 80;
    [Export] private int _maxRelicPrice = 150;
    [Export] private int _debugStartingGold = 200;

    [Signal] public delegate void CardPurchasedEventHandler(CardData cardData, int price);
    [Signal] public delegate void PurchaseFailedEventHandler(string reason);
    [Signal] public delegate void ExitRequestedEventHandler();

    private HBoxContainer _cardsContainer;
    private HBoxContainer _relicsContainer;
    private Label _goldLabel;
    private Label _messageLabel;
    private readonly List<ShopOffer> _offers = [];
    private readonly List<RelicShopOffer> _relicOffers = [];
    private IShopPlayerState _playerState;
    private int _removeCardCost = 75;
    private bool _cardRemoved = false;

    // ── Paths ─────────────────────────────────────────────────────────────────
    private const string BasePath      = "Panel/MarginContainer/VBoxContainer";
    private const string GoldLabelPath = BasePath + "/Header/GoldGroup/GoldLabel";
    private const string CardsPath     = BasePath + "/CardsContainer";
    private const string RelicsPath    = BasePath + "/BottomBar/RelicsContainer";
    private const string MsgPath       = BasePath + "/MessageLabel";
    private const string RemoveBtnPath = BasePath + "/BottomBar/RemoveButton";

    public override void _Ready()
    {
        MusicManager.Instance?.PlayShop();

        _cardDisplayScene ??= GD.Load<PackedScene>("res://src/Core/card_display.tscn");

        _cardsContainer  = GetNode<HBoxContainer>(CardsPath);
        _relicsContainer = GetNode<HBoxContainer>(RelicsPath);
        _goldLabel       = GetNode<Label>(GoldLabelPath);
        _messageLabel    = GetNode<Label>(MsgPath);

        if (UI.Instance?.TopHud is not null)
            UI.Instance.TopHud.Visible = false;

        EnsureTooltip();

        LoadCardPool();
        _playerState = ResolvePlayerState();

        RefreshGoldLabel();
        PopulateShop();
        PopulateRelics();
        UpdateRemoveButton();

        if (_playerState.IsDebug)
            SetMessage("Modo debug: usando ouro e deck temporarios.");
    }

    private static void EnsureTooltip()
    {
        if (RelicTooltip.Instance is not null) return;
        var scene = GD.Load<PackedScene>("res://src/Core/UI/RelicTooltip.tscn");
        if (scene is null) return;
        var tooltip = scene.Instantiate<RelicTooltip>();
        UI.Instance?.AddUI(tooltip);
    }

    // ── Card pool ─────────────────────────────────────────────────────────────

    private void LoadCardPool()
    {
        if (CountValidCards(_cardPool) > 0) return;

        _cardPool.Clear();

        using var dir = DirAccess.Open("res://Data/Cards");
        if (dir is null)
        {
            GD.PushWarning("Shop: pasta res://Data/Cards nao encontrada.");
            LoadKnownCardsFallback();
            return;
        }

        if (dir.ListDirBegin() != Error.Ok)
        {
            LoadKnownCardsFallback();
            return;
        }

        var fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
            {
                var card = GD.Load<CardData>($"res://Data/Cards/{fileName}");
                if (card is not null) _cardPool.Add(card);
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        if (CountValidCards(_cardPool) == 0) LoadKnownCardsFallback();
    }

    private void LoadKnownCardsFallback()
    {
        string[] paths =
        [
            "res://Data/Cards/StrikeCard.tres",
            "res://Data/Cards/BlockCard.tres",
            "res://Data/Cards/BlockVunarable.tres",
        ];
        foreach (var p in paths)
        {
            var card = GD.Load<CardData>(p);
            if (card is not null) _cardPool.Add(card);
        }
    }

    // ── Populate ──────────────────────────────────────────────────────────────

    private void PopulateShop()
    {
        foreach (var child in _cardsContainer.GetChildren()) child.QueueFree();
        _offers.Clear();

        if (_cardPool.Count == 0) { SetMessage("Nenhuma carta disponivel para venda."); return; }

        var pool = new List<CardData>();
        foreach (var c in _cardPool) if (c is not null) pool.Add(c);

        for (int i = 0; i < CardCount && pool.Count > 0; i++)
        {
            int idx = (int)(GD.Randi() % (uint)pool.Count);
            var offer = CreateOffer(pool[idx], GenerateCardPrice(pool[idx]));
            pool.RemoveAt(idx);
            _offers.Add(offer);
            _cardsContainer.AddChild(offer.Root);
        }
    }

    private void PopulateRelics()
    {
        _relicScene ??= GD.Load<PackedScene>("res://src/Core/Relic/RelicForShopOrChest.tscn");
        if (_relicScene is null) { GD.PushWarning("Shop: cena de relíquia não encontrada."); return; }

        for (int i = 0; i < RelicCount; i++)
        {
            int price = (int)GD.RandRange(_minRelicPrice, _maxRelicPrice);
            var offer = CreateRelicOffer(price);
            _relicOffers.Add(offer);
            _relicsContainer.AddChild(offer.Root);
        }
    }

    // ── Create offer ──────────────────────────────────────────────────────────

    private ShopOffer CreateOffer(CardData cardData, int price)
    {
        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(240, 440),
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        root.AddThemeConstantOverride("separation", 10);

        var display = _cardDisplayScene.Instantiate<CardDisplay>();
        display.Context = CardDisplay.CardDisplayContext.Shop;
        display.SetCard(cardData);
        display.CustomMinimumSize = new Vector2(220, 360);
        display.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

        root.AddChild(display);
        root.AddChild(BuildPriceRow(price, out var priceLabel));

        var offer = new ShopOffer(root, display, priceLabel, cardData, price);
        display.CardChosen += _ => TryBuyCard(offer);
        return offer;
    }

    private RelicShopOffer CreateRelicOffer(int price)
    {
        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(90, 110),
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        root.AddThemeConstantOverride("separation", 6);

        var relicButton = _relicScene.Instantiate<RelicForShopOrChest>();
        relicButton.Context = RelicForShopOrChest.RelicContext.Shop;
        relicButton.Price = price;
        relicButton.StartsDisabled = false;
        relicButton.StartsInvisible = false;

        root.AddChild(relicButton);
        root.AddChild(BuildPriceRow(price, out var priceLabel, iconSize: 18, fontSize: 16));

        var offer = new RelicShopOffer(root, relicButton, priceLabel, price);
        relicButton.RelicCollected += relic => TryBuyRelic(offer, relic);
        return offer;
    }

    private static HBoxContainer BuildPriceRow(int price, out Label label,
        int iconSize = 24, int fontSize = 20)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 6);

        var coinTexture = GD.Load<Texture2D>("res://Test/TestImagesSprites/art/gold.png");
        if (coinTexture is not null)
        {
            row.AddChild(new TextureRect
            {
                Texture = coinTexture,
                CustomMinimumSize = new Vector2(iconSize, iconSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            });
        }

        label = new Label { Text = price.ToString(), HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        row.AddChild(label);
        return row;
    }

    // ── Buy logic ─────────────────────────────────────────────────────────────

    private int GenerateCardPrice(CardData cardData) =>
        Math.Max(0, (int)GD.RandRange(_minCardPrice, _maxCardPrice) + cardData.EnergyCost * 5);

    private void TryBuyCard(ShopOffer offer)
    {
        if (offer.IsSold) return;
        if (!_playerState.SpendGold(offer.Price)) { Fail("Ouro insuficiente."); return; }

        _playerState.AddCardToDeck(offer.CardData);
        offer.MarkSold();
        RefreshGoldLabel();
        SetMessage($"{offer.CardData.CardName} comprada.");
        EmitSignal(SignalName.CardPurchased, offer.CardData, offer.Price);
    }

    private void TryBuyRelic(RelicShopOffer offer, RelicData relic)
    {
        if (offer.IsBought) return;
        if (!_playerState.SpendGold(offer.Price)) { Fail("Ouro insuficiente."); return; }

        _playerState.AddRelic(relic);
        offer.MarkBought();
        RefreshGoldLabel();
        SetMessage($"{relic.RelicName} comprada.");
    }

    // ── Remove card service ───────────────────────────────────────────────────

    public void OnRemoveCardPressed()
    {
        if (_cardRemoved) return;

        if (!_playerState.SpendGold(_removeCardCost))
        {
            SetMessage("Ouro insuficiente para remover uma carta!");
            return;
        }

        _cardRemoved = true;
        RefreshGoldLabel();
        UpdateRemoveButton();

        var deckScene = GD.Load<PackedScene>("res://src/Core/Inventory/DeckViewer.tscn");
        if (deckScene is null) { SetMessage("Erro ao abrir baralho."); return; }

        var viewer = deckScene.Instantiate<DeckViewer>();
        viewer.Mode = DeckViewer.ViewerMode.Remove;
        AddChild(viewer);
        viewer.LoadDeck(new List<CardData>(_playerState.Deck));
        viewer.CardRemoved += OnCardRemovedFromViewer;
        SetMessage("Escolha uma carta para remover.");
    }

    private void OnCardRemovedFromViewer(CardData card)
    {
        PlayerManager.Instance?.Player?.RemoveCardFromDeck(card);
    }

    private void UpdateRemoveButton()
    {
        var btn = GetNodeOrNull<Button>(RemoveBtnPath);
        if (btn is null) return;
        btn.Disabled = _cardRemoved;
        btn.Text = _cardRemoved
            ? "Carta removida"
            : $"Remover carta do baralho — {_removeCardCost} ouro";
    }

    // ── Deck viewer ───────────────────────────────────────────────────────────

    public void OnViewDeckPressed()
    {
        var scene = GD.Load<PackedScene>("res://src/Core/Inventory/DeckViewer.tscn");
        if (scene is null) return;
        var viewer = scene.Instantiate<DeckViewer>();
        viewer.Mode = DeckViewer.ViewerMode.Inspect;
        AddChild(viewer);
        viewer.LoadDeck(new List<CardData>(_playerState.Deck));
    }

    public void OnExitPressed()
    {
        if (UI.Instance?.TopHud is not null)
            UI.Instance.TopHud.Visible = true;
        EmitSignal(SignalName.ExitRequested);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshGoldLabel() => _goldLabel.Text = _playerState.Gold.ToString();

    private void SetMessage(string msg) => _messageLabel.Text = msg;

    private void Fail(string reason)
    {
        SetMessage(reason);
        EmitSignal(SignalName.PurchaseFailed, reason);
    }

    private IShopPlayerState ResolvePlayerState()
    {
        var player = PlayerManager.Instance?.Player;
        if (ShouldUseRealPlayer(player))
        {
            GD.Print("Shop: Player real encontrado.");
            return new RealPlayerShopState(player);
        }
        GD.Print("Shop: Modo debug.");
        return new DebugShopState(_debugStartingGold, _cardPool);
    }

    private bool ShouldUseRealPlayer(Player player)
    {
        if (!GodotObject.IsInstanceValid(player) || player.IsQueuedForDeletion()) return false;

        var pm = PlayerManager.Instance;
        bool isFallback = GodotObject.IsInstanceValid(pm)
            && pm.IsAncestorOf(player)
            && GetTree().CurrentScene == this;
        return !isFallback;
    }

    private int CountValidCards(IReadOnlyList<CardData> cards)
    {
        int n = 0;
        foreach (var c in cards) if (c is not null) n++;
        return n;
    }

    // ── Inner types ───────────────────────────────────────────────────────────

    private sealed class ShopOffer
    {
        public VBoxContainer Root { get; }
        public CardDisplay Display { get; }
        public Label PriceLabel { get; }
        public CardData CardData { get; }
        public int Price { get; }
        public bool IsSold { get; private set; }

        public ShopOffer(VBoxContainer root, CardDisplay display, Label priceLabel,
            CardData cardData, int price)
        {
            Root = root; Display = display; PriceLabel = priceLabel;
            CardData = cardData; Price = price;
        }

        public void MarkSold()
        {
            IsSold = true;
            Display.MouseFilter = MouseFilterEnum.Ignore;
            Root.Modulate = new Color(1f, 1f, 1f, 0.35f);
            PriceLabel.Text = "Comprada";
        }
    }

    private sealed class RelicShopOffer
    {
        public VBoxContainer Root { get; }
        public RelicForShopOrChest RelicButton { get; }
        public Label PriceLabel { get; }
        public int Price { get; }
        public bool IsBought { get; private set; }

        public RelicShopOffer(VBoxContainer root, RelicForShopOrChest relicButton,
            Label priceLabel, int price)
        {
            Root = root; RelicButton = relicButton; PriceLabel = priceLabel; Price = price;
        }

        public void MarkBought()
        {
            IsBought = true;
            RelicButton.Disabled = true;
            RelicButton.MouseFilter = MouseFilterEnum.Ignore;
            Root.Modulate = new Color(1f, 1f, 1f, 0.35f);
            PriceLabel.Text = "Comprada";
        }
    }

    private interface IShopPlayerState
    {
        int Gold { get; }
        IReadOnlyList<CardData> Deck { get; }
        bool IsDebug { get; }
        bool SpendGold(int amount);
        void AddCardToDeck(CardData cardData);
        void AddRelic(RelicData relicData);
    }

    private sealed class RealPlayerShopState : IShopPlayerState
    {
        private readonly Player _player;
        public int Gold => _player.Gold;
        public IReadOnlyList<CardData> Deck => _player.GetDeck();
        public bool IsDebug => false;

        public RealPlayerShopState(Player player) { _player = player; }
        public bool SpendGold(int amount) => _player.SpendGold(amount);
        public void AddCardToDeck(CardData c) => _player.AddCardToDeck(c);
        public void AddRelic(RelicData r) => _player.AddRelic(r);
    }

    private sealed class DebugShopState : IShopPlayerState
    {
        private readonly List<CardData> _deck = [];
        public int Gold { get; private set; }
        public IReadOnlyList<CardData> Deck => _deck;
        public bool IsDebug => true;

        public DebugShopState(int startingGold, IReadOnlyList<CardData> pool)
        {
            Gold = startingGold;
            for (int i = 0; i < pool.Count && _deck.Count < 6; i++)
                if (pool[i] is not null) _deck.Add(pool[i]);
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public void AddCardToDeck(CardData c) => _deck.Add(c);
        public void AddRelic(RelicData r) =>
            GD.Print($"Debug Shop: relíquia — {r?.RelicName}");
    }
}
