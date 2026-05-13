using Godot;
using System.Collections.Generic;

public partial class RelicBar : MarginContainer
{
    public static RelicBar Instance { get; private set; }

    [Export] private HBoxContainer _relicContainer;

    public override void _Ready()
    {
        Instance = this;
        EnsureTooltipExists();
    }

    private static void EnsureTooltipExists()
    {
        if (RelicTooltip.Instance is not null) return;

        var scene = GD.Load<PackedScene>("res://src/Core/UI/RelicTooltip.tscn");
        if (scene is null) return;

        var tooltip = scene.Instantiate<RelicTooltip>();
        UI.Instance?.AddUI(tooltip);
    }

    public void AddRelic(RelicData relic)
    {
        var btn = new TextureButton
        {
            TextureNormal    = relic.Icon,
            IgnoreTextureSize = true,
            StretchMode      = TextureButton.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(60, 60),
        };

        // borda sutil para identificar cada relíquia
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0, 0, 0, 0);
        style.SetBorderWidthAll(2);
        style.BorderColor = new Color(0.72f, 0.46f, 0.24f, 0f);
        style.SetCornerRadiusAll(6);

        var styleHover = (StyleBoxFlat)style.Duplicate();
        styleHover.BorderColor = new Color(0.9f, 0.65f, 0.2f, 0.9f);
        styleHover.BgColor = new Color(1, 1, 1, 0.08f);

        btn.AddThemeStyleboxOverride("normal", style);
        btn.AddThemeStyleboxOverride("hover",  styleHover);
        btn.AddThemeStyleboxOverride("focus",  style);

        btn.MouseEntered += () => RelicTooltip.Instance?.ShowTooltip(relic);
        btn.MouseExited  += () => RelicTooltip.Instance?.HideTooltip();

        _relicContainer.AddChild(btn);
    }

    public void LoadRelics(List<RelicData> relics)
    {
        foreach (var child in _relicContainer.GetChildren())
            child.QueueFree();

        foreach (var relic in relics)
            AddRelic(relic);
    }
}
