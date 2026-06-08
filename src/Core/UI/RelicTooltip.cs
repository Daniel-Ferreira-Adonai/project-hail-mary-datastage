using Godot;

public partial class RelicTooltip : Control
{
    public static RelicTooltip Instance { get; private set; }

    private Label _nameLabel;
    private Label _descLabel;
    private TextureRect _icon;

    private const float Padding = 16f;

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

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
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

    public void ShowTooltip(RelicData relic)
    {
        _icon.Texture   = relic.Icon;
        _icon.Visible   = relic.Icon is not null;
        _nameLabel.Text = relic.RelicName ?? "";
        _descLabel.Text = relic.Description ?? "";
        Visible         = true;
    }

    public void ShowTooltip(string title, string desc)
    {
        _icon.Visible   = false;
        _nameLabel.Text = title;
        _descLabel.Text = desc;
        Visible         = true;
    }

    public void HideTooltip()
    {
        Visible = false;
    }
}
