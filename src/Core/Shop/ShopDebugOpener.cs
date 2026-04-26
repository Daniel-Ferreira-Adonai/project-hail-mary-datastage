using Godot;

public partial class ShopDebugOpener : Node
{
    [Export] private PackedScene _shopScene;

    private Shop _currentShop;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent ||
            !keyEvent.Pressed ||
            keyEvent.Echo ||
            keyEvent.Keycode != Key.S)
        {
            return;
        }

        OpenShop();
    }

    private void OpenShop()
    {
        if (GodotObject.IsInstanceValid(_currentShop))
        {
            return;
        }

        _shopScene ??= GD.Load<PackedScene>("res://src/Core/Shop/Shop.tscn");
        var shop = _shopScene.Instantiate<Shop>();
        _currentShop = shop;

        shop.ExitRequested += () =>
        {
            shop.QueueFree();
            ClearCurrentShop();
        };
        shop.TreeExited += ClearCurrentShop;

        if (UI.Instance is not null)
        {
            UI.Instance.AddUI(shop);
        }
        else
        {
            AddChild(shop);
        }
    }

    private void ClearCurrentShop()
    {
        _currentShop = null;
    }
}
