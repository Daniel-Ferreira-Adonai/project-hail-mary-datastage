using Godot;
using System;
using System.Collections.Generic;

public partial class Shop : Control
{
    private const int CardCount = 8;
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

    private GridContainer _cardsContainer;
    private HBoxContainer _relicsContainer;
    private Label _goldLabel;
    private Label _messageLabel;
    private readonly List<ShopOffer> _offers = [];
    private readonly List<RelicShopOffer> _relicOffers = [];
    private IShopPlayerState _playerState;

    public override void _Ready()
    {
        _cardDisplayScene ??= GD.Load<PackedScene>("res://src/Core/card_display.tscn");
        var cardsScroll = GetNode<ScrollContainer>("Panel/MarginContainer/VBoxContainer/CardsScroll");
        _cardsContainer = GetNode<GridContainer>("Panel/MarginContainer/VBoxContainer/CardsScroll/CardsContainer");
        _goldLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Header/GoldLabel");
        _messageLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/MessageLabel");

        var vbox = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer");
        var relicsLabel = new Label { Text = "Reliquias" };
        relicsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        relicsLabel.AddThemeFontSizeOverride("font_size", 18);
        _relicsContainer = new HBoxContainer();
        _relicsContainer.AddThemeConstantOverride("separation", 20);
        vbox.AddChild(relicsLabel);
        vbox.AddChild(_relicsContainer);
        int cardsIdx = cardsScroll.GetIndex();
        vbox.MoveChild(relicsLabel, cardsIdx + 1);
        vbox.MoveChild(_relicsContainer, cardsIdx + 2);

        LoadCardPool();
        _playerState = ResolvePlayerState();

        GD.Print($"Shop: cartas carregadas no pool = {CountValidCards(_cardPool)}");
        GD.Print($"Shop: cartas no deck usado = {_playerState.Deck.Count}");

        RefreshGoldLabel();
        PopulateShop();
        PopulateRelics();

        GD.Print($"Shop: ofertas criadas = {_offers.Count}");

        if (_playerState.IsDebug)
        {
            SetMessage("Modo debug: usando ouro e deck temporarios.");
        }
    }

    private void LoadCardPool()
    {
        if (CountValidCards(_cardPool) > 0)
        {
            return;
        }

        _cardPool.Clear();

        using var dir = DirAccess.Open("res://Data/Cards");
        if (dir is null)
        {
            GD.PushWarning("Shop: pasta res://Data/Cards nao encontrada.");
            LoadKnownCardsFallback();
            return;
        }

        var error = dir.ListDirBegin();
        if (error != Error.Ok)
        {
            GD.PushWarning($"Shop: erro ao listar res://Data/Cards: {error}");
            LoadKnownCardsFallback();
            return;
        }

        var fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
            {
                var card = GD.Load<CardData>($"res://Data/Cards/{fileName}");
                if (card is not null)
                {
                    _cardPool.Add(card);
                }
            }

            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        if (CountValidCards(_cardPool) == 0)
        {
            LoadKnownCardsFallback();
        }
    }

    private void LoadKnownCardsFallback()
    {
        string[] cardPaths =
        [
            "res://Data/Cards/StrikeCard.tres",
            "res://Data/Cards/BlockCard.tres",
            "res://Data/Cards/BlockVunarable.tres",
        ];

        foreach (string cardPath in cardPaths)
        {
            var card = GD.Load<CardData>(cardPath);
            if (card is not null)
            {
                _cardPool.Add(card);
            }
        }
    }

    private void PopulateShop()
    {
        foreach (var child in _cardsContainer.GetChildren())
        {
            child.QueueFree();
        }
        _offers.Clear();

        if (_cardPool.Count == 0)
        {
            SetMessage("Nenhuma carta disponivel para venda.");
            return;
        }

        var availableCards = new List<CardData>();
        foreach (var cardData in _cardPool)
        {
            if (cardData is not null)
            {
                availableCards.Add(cardData);
            }
        }

        for (int i = 0; i < CardCount && availableCards.Count > 0; i++)
        {
            int index = (int)(GD.Randi() % (uint)availableCards.Count);
            CardData cardData = availableCards[index];
            availableCards.RemoveAt(index);

            var offer = CreateOffer(cardData, GenerateCardPrice(cardData));
            _offers.Add(offer);
            _cardsContainer.AddChild(offer.Root);
        }
    }

    private ShopOffer CreateOffer(CardData cardData, int price)
    {
        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(220, 370),
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        root.AddThemeConstantOverride("separation", 10);

        var display = _cardDisplayScene.Instantiate<CardDisplay>();
        display.Context = CardDisplay.CardDisplayContext.Shop;
        display.SetCard(cardData);

        var priceLabel = new Label
        {
            Text = $"{price} ouro",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        priceLabel.AddThemeFontSizeOverride("font_size", 22);

        root.AddChild(display);
        root.AddChild(priceLabel);

        var offer = new ShopOffer(root, display, priceLabel, cardData, price);
        display.CardChosen += chosenCard => TryBuyCard(offer);

        return offer;
    }

    private int GenerateCardPrice(CardData cardData)
    {
        int basePrice = (int)GD.RandRange(_minCardPrice, _maxCardPrice);
        return Math.Max(0, basePrice + cardData.EnergyCost * 5);
    }

    private void TryBuyCard(ShopOffer offer)
    {
        if (offer.IsSold)
        {
            return;
        }

        if (!_playerState.SpendGold(offer.Price))
        {
            Fail("Ouro insuficiente.");
            return;
        }

        _playerState.AddCardToDeck(offer.CardData);
        offer.MarkSold();

        RefreshGoldLabel();
        SetMessage($"{offer.CardData.CardName} comprada.");
        EmitSignal(SignalName.CardPurchased, offer.CardData, offer.Price);
    }

    private void PopulateRelics()
    {
        if (_relicScene is null)
        {
            _relicScene = GD.Load<PackedScene>("res://src/Core/Relic/RelicForShopOrChest.tscn");
        }

        if (_relicScene is null)
        {
            GD.PushWarning("Shop: cena de relíquia não encontrada.");
            return;
        }

        for (int i = 0; i < RelicCount; i++)
        {
            int price = (int)GD.RandRange(_minRelicPrice, _maxRelicPrice);
            var offer = CreateRelicOffer(price);
            _relicOffers.Add(offer);
            _relicsContainer.AddChild(offer.Root);
        }
    }

    private RelicShopOffer CreateRelicOffer(int price)
    {
        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(100, 120),
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        root.AddThemeConstantOverride("separation", 8);

        var relicButton = _relicScene.Instantiate<RelicForShopOrChest>();
        relicButton.Context = RelicForShopOrChest.RelicContext.Shop;
        relicButton.Price = price;
        relicButton.StartsDisabled = false;
        relicButton.StartsInvisible = false;

        var priceLabel = new Label
        {
            Text = $"{price} ouro",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        priceLabel.AddThemeFontSizeOverride("font_size", 18);

        root.AddChild(relicButton);
        root.AddChild(priceLabel);

        var offer = new RelicShopOffer(root, relicButton, priceLabel, price);
        relicButton.RelicCollected += relic => TryBuyRelic(offer, relic);

        return offer;
    }

    private void TryBuyRelic(RelicShopOffer offer, RelicData relic)
    {
        if (offer.IsBought) return;

        if (!_playerState.SpendGold(offer.Price))
        {
            Fail("Ouro insuficiente.");
            return;
        }

        _playerState.AddRelic(relic);
        offer.MarkBought();

        RefreshGoldLabel();
        SetMessage($"{relic.RelicName} comprada.");
    }

    public void OnViewDeckPressed()
    {
        var deckViewerScene = GD.Load<PackedScene>("res://src/Core/Inventory/DeckViewer.tscn");
        if (deckViewerScene is null) return;
        var viewer = deckViewerScene.Instantiate<DeckViewer>();
        viewer.Mode = DeckViewer.ViewerMode.Inspect;
        AddChild(viewer);
        viewer.LoadDeck(new List<CardData>(_playerState.Deck));
    }

    public void OnExitPressed()
    {
        EmitSignal(SignalName.ExitRequested);
    }

    private void RefreshGoldLabel()
    {
        _goldLabel.Text = $"Ouro: {_playerState.Gold}";
    }

    private IShopPlayerState ResolvePlayerState()
    {
        var player = PlayerManager.Instance?.Player;
        if (ShouldUseRealPlayer(player))
        {
            GD.Print("Shop: Player real encontrado. Usando estado real.");
            return new RealPlayerShopState(player);
        }

        GD.Print("Shop: Player real nao encontrado. Entrando em modo debug/local.");
        return new DebugShopState(_debugStartingGold, _cardPool);
    }

    private bool ShouldUseRealPlayer(Player player)
    {
        if (!GodotObject.IsInstanceValid(player) || player.IsQueuedForDeletion())
        {
            return false;
        }

        var playerManager = PlayerManager.Instance;
        bool isAutoloadFallbackPlayer =
            GodotObject.IsInstanceValid(playerManager) &&
            playerManager.IsAncestorOf(player) &&
            GetTree().CurrentScene == this;

        return !isAutoloadFallbackPlayer;
    }

    private int CountValidCards(IReadOnlyList<CardData> cards)
    {
        int count = 0;
        foreach (var card in cards)
        {
            if (card is not null)
            {
                count++;
            }
        }

        return count;
    }

    private void Fail(string reason)
    {
        SetMessage(reason);
        EmitSignal(SignalName.PurchaseFailed, reason);
    }

    private void SetMessage(string message)
    {
        _messageLabel.Text = message;
    }

    private sealed class ShopOffer
    {
        public VBoxContainer Root { get; }
        public CardDisplay Display { get; }
        public Label PriceLabel { get; }
        public CardData CardData { get; }
        public int Price { get; }
        public bool IsSold { get; private set; }

        public ShopOffer(VBoxContainer root, CardDisplay display, Label priceLabel, CardData cardData, int price)
        {
            Root = root;
            Display = display;
            PriceLabel = priceLabel;
            CardData = cardData;
            Price = price;
        }

        public void MarkSold()
        {
            IsSold = true;
            Display.MouseFilter = MouseFilterEnum.Ignore;
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

        public RealPlayerShopState(Player player)
        {
            _player = player;
        }

        public bool SpendGold(int amount)
        {
            return _player.SpendGold(amount);
        }

        public void AddCardToDeck(CardData cardData)
        {
            _player.AddCardToDeck(cardData);
        }

        public void AddRelic(RelicData relicData)
        {
            _player.AddRelic(relicData);
        }
    }

    private sealed class DebugShopState : IShopPlayerState
    {
        private readonly List<CardData> _deck = [];

        public int Gold { get; private set; }
        public IReadOnlyList<CardData> Deck => _deck;
        public bool IsDebug => true;

        public DebugShopState(int startingGold, IReadOnlyList<CardData> cardPool)
        {
            Gold = startingGold;

            for (int i = 0; i < cardPool.Count && _deck.Count < 6; i++)
            {
                if (cardPool[i] is not null)
                {
                    _deck.Add(cardPool[i]);
                }
            }
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            return true;
        }

        public void AddCardToDeck(CardData cardData)
        {
            _deck.Add(cardData);
        }

        public void AddRelic(RelicData relicData)
        {
            GD.Print($"Debug Shop: relíquia adicionada — {relicData?.RelicName}");
        }
    }

    private sealed class RelicShopOffer
    {
        public VBoxContainer Root { get; }
        public RelicForShopOrChest RelicButton { get; }
        public Label PriceLabel { get; }
        public int Price { get; }
        public bool IsBought { get; private set; }

        public RelicShopOffer(VBoxContainer root, RelicForShopOrChest relicButton, Label priceLabel, int price)
        {
            Root = root;
            RelicButton = relicButton;
            PriceLabel = priceLabel;
            Price = price;
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
}
