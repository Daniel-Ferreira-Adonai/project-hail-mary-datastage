using Godot;

public partial class EffectTooltip : Control
{
    public static EffectTooltip Instance { get; private set; }

    private Label _nameLabel;
    private Label _descLabel;
    private TextureRect _icon;

    public override void _Ready()
    {
        Instance    = this;
        Visible     = false;
        ZIndex      = 1000;
        ZAsRelative = false;

        _nameLabel = GetNode<Label>("Panel/VBox/Header/Name");
        _descLabel = GetNode<Label>("Panel/VBox/Desc");
        _icon      = GetNode<TextureRect>("Panel/VBox/Header/Icon");
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;

        var mouse    = GetViewport().GetMousePosition();
        var viewport = GetViewport().GetVisibleRect().Size;
        var size     = GetNode<Control>("Panel").Size;

        float x = mouse.X + 12;
        float y = mouse.Y - size.Y - 8;

        if (x + size.X > viewport.X) x = mouse.X - size.X - 12;
        if (y < 0)                   y = mouse.Y + 20;

        Position = new Vector2(x, y);
    }

    public void ShowTooltip(EffectData effect, int value)
    {
        string desc = effect.GetDescription(value);
        if (string.IsNullOrEmpty(desc)) return;

        _icon.Texture   = effect.Icon;
        _icon.Visible   = effect.Icon is not null;
        _nameLabel.Text = effect.Name ?? "";
        _descLabel.Text = desc;
        Visible = true;
    }

    public void HideTooltip()
    {
        Visible = false;
    }
}