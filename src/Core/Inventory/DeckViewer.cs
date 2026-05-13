using Godot;
using System.Collections.Generic;

public partial class DeckViewer : Control
{
    public enum ViewerMode { Inspect, Remove, Upgrade }

    [Signal] public delegate void CardRemovedEventHandler(CardData cardData);
    [Signal] public delegate void CardUpgradedEventHandler(CardData cardData);
    [Signal] public delegate void ExitRequestedEventHandler();

    [Export] public ViewerMode Mode { get; set; } = ViewerMode.Inspect;

    private GridContainer _grid;
    private Label _titleLabel;
    private Label _infoLabel;

    private List<CardData> _deck;
    private PackedScene _cardDisplayScene;
    private PackedScene _cardInspectScene;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        // Pausa apenas durante combate (impede clicar nas cartas da mão via Area2D)
        if (GetTree().GetNodesInGroup("enemies").Count > 0)
            GetTree().Paused = true;

        _grid       = GetNode<GridContainer>("Panel/OuterVBox/Scroll/Grid");
        _titleLabel = GetNode<Label>("Panel/OuterVBox/Header/Title");
        _infoLabel  = GetNode<Label>("Panel/OuterVBox/Header/InfoLabel");

        _cardDisplayScene = GD.Load<PackedScene>("res://src/Core/card_display.tscn");

        ApplyModeLabels();
    }

    // Garante que o jogo sempre volta a rodar mesmo se o nó for removido de outra forma
    public override void _ExitTree()
    {
        if (GetTree() is not null)
            GetTree().Paused = false;
    }

    public void LoadDeck(List<CardData> deck)
    {
        _deck = deck;
        Populate();
    }

    private void ApplyModeLabels()
    {
        _titleLabel.Text = Mode switch
        {
            ViewerMode.Remove  => "Remover Carta",
            ViewerMode.Upgrade => "Melhorar Carta",
            _                  => "Baralho",
        };

        _infoLabel.Text = Mode switch
        {
            ViewerMode.Remove  => "Clique em uma carta para removê-la permanentemente.",
            ViewerMode.Upgrade => "Clique em uma carta para melhorá-la.",
            _                  => string.Empty,
        };
    }

    private void Populate()
    {
        if (_grid is null || _cardDisplayScene is null) return;

        foreach (var child in _grid.GetChildren())
            child.QueueFree();

        foreach (var cardData in _deck)
        {
            var display = _cardDisplayScene.Instantiate<CardDisplay>();
            display.Context = CardDisplay.CardDisplayContext.DeckViewer;
            display.SetCard(cardData);

            var captured = cardData;
            if (Mode == ViewerMode.Inspect)
                display.CardChosen += _ => ShowCardInspect(captured);
            else
                display.CardChosen += _ => OnCardChosen(captured, display);

            _grid.AddChild(display);

            var player = PlayerManager.Instance?.Player;
            if (player != null)
                display.UpdatePreview(player);
        }
    }

    private void ShowCardInspect(CardData card)
    {
        _cardInspectScene ??= GD.Load<PackedScene>("res://src/Core/Inventory/CardInspectPopup.tscn");
        if (_cardInspectScene is null) return;
        var popup = _cardInspectScene.Instantiate<CardInspectPopup>();
        AddChild(popup);
        popup.ShowCard(card);
    }

    private void OnCardChosen(CardData card, CardDisplay display)
    {
        switch (Mode)
        {
            case ViewerMode.Remove:
                _deck.Remove(card);
                display.QueueFree();
                EmitSignal(SignalName.CardRemoved, card);
                OnClosePressed();
                break;

            case ViewerMode.Upgrade:
                if (card.IsCardUpgraded) return;
                card.IsCardUpgraded = true;
                card.UpgradeCard();
                display.SetCard(card);
                EmitSignal(SignalName.CardUpgraded, card);
                OnClosePressed();
                break;
        }
    }

    public void OnClosePressed()
    {
        EmitSignal(SignalName.ExitRequested);
        QueueFree();
    }
}
