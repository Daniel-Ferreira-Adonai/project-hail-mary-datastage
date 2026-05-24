using Godot;
using System.Collections.Generic;

public partial class EffectBar : HBoxContainer
{
    public override void _Ready()
    {
        InitTooltip();
    }

    private void InitTooltip()
    {
        if (EffectTooltip.Instance is not null) return;

        var scene = GD.Load<PackedScene>("res://src/Core/Effects/EffectTooltip.tscn");
        if (scene is null) return;

        var tooltip = scene.Instantiate<EffectTooltip>();
        UI.Instance?.AddUI(tooltip);
    }

    public void UpdateEffects(List<(EffectData data, int value)> effects)
    {
        ClearEffects();

        foreach (var (data, value) in effects)
        {
            AddEffect(data, value);
        }
    }

    private void ClearEffects()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddEffect(EffectData effect, int value)
{
    var container = new PanelContainer();
    container.CustomMinimumSize = new Vector2(40, 40);

    // remove fundo cinza padrão
    var bgStyle = new StyleBoxFlat();
    bgStyle.BgColor = new Color(0, 0, 0, 0);
    bgStyle.SetBorderWidthAll(0);
    container.AddThemeStyleboxOverride("panel", bgStyle);

    var icon = new TextureRect
    {
        Texture = effect.Icon,
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        CustomMinimumSize = new Vector2(40, 40),
        SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        SizeFlagsVertical = SizeFlags.ShrinkCenter
    };

    var label = new Label
    {
        Text = value > 1 ? value.ToString() : "",
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
        SizeFlagsHorizontal = SizeFlags.ExpandFill,
        SizeFlagsVertical = SizeFlags.ExpandFill
    };
    label.AddThemeFontSizeOverride("font_size", 12);

    container.AddChild(icon);
    container.AddChild(label);

    var styleHover = new StyleBoxFlat();
    styleHover.BgColor = new Color(1, 1, 1, 0.08f);
    styleHover.SetBorderWidthAll(2);
    styleHover.BorderColor = new Color(1f, 1f, 1f, 0.9f);
    styleHover.SetCornerRadiusAll(6);

    container.MouseEntered += () =>
    {
        container.AddThemeStyleboxOverride("panel", styleHover);
        EffectTooltip.Instance?.ShowTooltip(effect, value);
    };

    container.MouseExited += () =>
    {
        container.AddThemeStyleboxOverride("panel", bgStyle);
        EffectTooltip.Instance?.HideTooltip();
    };

    AddChild(container);
}
}