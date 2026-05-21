using Godot;
using System.Collections.Generic;

public partial class TopBar : PanelContainer
{
    [Export] private Label _hpLabel;
    [Export] private Label _hpMaxLabel;
    [Export] private Label _goldLabel;
    [Export] private Label _floorLabel;
    [Export] private Label _deckCountLabel;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.TopWide);
        MouseFilter = MouseFilterEnum.Pass;
        ApplyTheme();

        var deckButton = GetNodeOrNull<TextureButton>("MarginTopBar/HBoxContainer/DeckPanel/panelContainer/deckButton");
        if (deckButton is not null)
            deckButton.Pressed += OnDeckPressed;

        ConnectStatTooltips();
    }

    private void ConnectStatTooltips()
    {
        var hpContainer = GetNodeOrNull<Control>("MarginTopBar/HBoxContainer/HpContainer");
        if (hpContainer is not null)
        {
            hpContainer.MouseFilter = MouseFilterEnum.Stop;
            hpContainer.MouseEntered += () => RelicTooltip.Instance?.ShowTooltip(
                "Pontos de Vida",
                $"{_hpLabel.Text} / {_hpMaxLabel.Text} HP\nAo chegar em 0, você morre.");
            hpContainer.MouseExited += () => RelicTooltip.Instance?.HideTooltip();
        }

        var goldContainer = GetNodeOrNull<Control>("MarginTopBar/HBoxContainer/GoldContainer");
        if (goldContainer is not null)
        {
            goldContainer.MouseFilter = MouseFilterEnum.Stop;
            goldContainer.MouseEntered += () => RelicTooltip.Instance?.ShowTooltip(
                "Ouro",
                $"{_goldLabel.Text} moedas\nUsado para comprar cartas e relíquias.");
            goldContainer.MouseExited += () => RelicTooltip.Instance?.HideTooltip();
        }
    }

    private static void OnDeckPressed()
    {
        GameManager.Instance?.ShowDeckViewer(DeckViewer.ViewerMode.Inspect);
    }

    private void ApplyTheme()
    {
        var textColor  = new Color("e8d5b0");
        var mutedColor = new Color("7a6a55");

        _hpLabel.AddThemeColorOverride("font_color", textColor);
        _hpLabel.AddThemeFontSizeOverride("font_size", 16);

        _hpMaxLabel.AddThemeColorOverride("font_color", mutedColor);
        _hpMaxLabel.AddThemeFontSizeOverride("font_size", 16);

        _goldLabel.AddThemeColorOverride("font_color", textColor);
        _goldLabel.AddThemeFontSizeOverride("font_size", 16);

        _floorLabel.AddThemeColorOverride("font_color", mutedColor);
        _floorLabel.AddThemeFontSizeOverride("font_size", 16);
    }

    public void UpdateHP(int current, int max)
    {
        _hpLabel.Text    = current.ToString();
        _hpMaxLabel.Text = max.ToString();
    }

    public void UpdateGold(int gold)
    {
        _goldLabel.Text = gold.ToString();
    }

    public void UpdateFloor(int floor)
    {
        _floorLabel.Text = $"Andar {floor}";
    }

    public void UpdateDeckCount(int count)
    {
        _deckCountLabel.Text = count.ToString();
    }
}
