using Godot;

public partial class ComingSoon : Control
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            QueueFree();
    }

    public void OnClosePressed()
    {
        QueueFree();
    }
}
