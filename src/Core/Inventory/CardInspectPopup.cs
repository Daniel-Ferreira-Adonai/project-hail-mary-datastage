using Godot;

public partial class CardInspectPopup : Control
{
    private CenterContainer _center;
    private PackedScene _cardDisplayScene;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        ZIndex = 10;
        _center = GetNode<CenterContainer>("Center");
        _cardDisplayScene = GD.Load<PackedScene>("res://src/Core/card_display.tscn");
        GetNode<ColorRect>("DimOverlay").GuiInput += OnDimClicked;
    }

    private void OnDimClicked(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            QueueFree();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            QueueFree();
    }

    public void ShowCard(CardData card)
    {
        if (_cardDisplayScene is null) return;

        foreach (var child in _center.GetChildren())
            child.QueueFree();

        var display = _cardDisplayScene.Instantiate<CardDisplay>();
        display.CustomMinimumSize = new Vector2(360, 600);
        _center.AddChild(display);
        display.SetCard(card);
        display.UpdatePreview(PlayerManager.Instance?.Player);
    }
}
