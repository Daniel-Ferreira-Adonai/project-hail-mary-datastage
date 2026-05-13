using Godot;
using System.Collections.Generic;

public partial class CharacterSelect : Control
{
    [Export] private PackedScene _gameManagerScene;

    private HBoxContainer _charactersContainer;
    private Label _nameLabel;
    private Label _descriptionLabel;
    private Label _hpLabel;
    private Label _energyLabel;
    private Label _goldLabel;
    private TextureRect _portrait;
    private Godot.Button _confirmButton;
    private Godot.Button _backButton;

    private CharacterData _selectedCharacter;
    private readonly List<CharacterData> _characters = new();

    public override void _Ready()
    {
        _charactersContainer = GetNode<HBoxContainer>("Background/Panel/VBox/CharactersContainer");
        _nameLabel           = GetNode<Label>("Background/Panel/VBox/InfoPanel/InfoHBox/TextContent/Name");
        _descriptionLabel    = GetNode<Label>("Background/Panel/VBox/InfoPanel/InfoHBox/TextContent/Description");
        _hpLabel             = GetNode<Label>("Background/Panel/VBox/InfoPanel/InfoHBox/TextContent/Stats/HP");
        _energyLabel         = GetNode<Label>("Background/Panel/VBox/InfoPanel/InfoHBox/TextContent/Stats/Energy");
        _goldLabel           = GetNode<Label>("Background/Panel/VBox/InfoPanel/InfoHBox/TextContent/Stats/Gold");
        _portrait            = GetNodeOrNull<TextureRect>("Background/Panel/VBox/InfoPanel/InfoHBox/Portrait");
        _confirmButton       = GetNode<Godot.Button>("Background/Panel/VBox/ConfirmButton");
        _backButton          = GetNodeOrNull<Godot.Button>("Background/Panel/VBox/BackButton");

        _confirmButton.Disabled = true;

        LoadCharacters();
        PopulateCharacters();
    }

    private void LoadCharacters()
    {
        using var dir = DirAccess.Open("res://Data/Characters");
        if (dir is not null)
        {
            dir.ListDirBegin();
            var file = dir.GetNext();
            while (!string.IsNullOrEmpty(file))
            {
                if (!dir.CurrentIsDir() && file.EndsWith(".tres"))
                {
                    var c = GD.Load<CharacterData>($"res://Data/Characters/{file}");
                    if (c is not null) _characters.Add(c);
                }
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }

        if (_characters.Count == 0)
            AddFallbackCharacters();
    }

    private void AddFallbackCharacters()
    {
        var doutor = new CharacterData
        {
            CharacterName  = "Doutor Peste",
            Description    = "Um médico da morte que usa conhecimento proibido para derrotar seus inimigos no Datastage. Especialista em venenos e maldições.",
            StartingMaxHp  = 75,
            StartingGold   = 100,
            StartingEnergy = 3,
            IsUnlocked     = true,
        };
        _characters.Add(doutor);

        var seZe = new CharacterData
        {
            CharacterName  = "Seu Zé",
            Description    = "Em breve...",
            StartingMaxHp  = 80,
            StartingGold   = 80,
            StartingEnergy = 3,
            IsUnlocked     = false,
        };
        _characters.Add(seZe);
    }

    private void PopulateCharacters()
    {
        foreach (var character in _characters)
            _charactersContainer.AddChild(BuildCharacterCard(character));

        var unlocked = _characters.FindAll(c => c.IsUnlocked);
        if (unlocked.Count == 1)
            SelectCharacter(unlocked[0]);
    }

    private Godot.Button BuildCharacterCard(CharacterData character)
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.08f, 0.06f, 0.9f);
        style.SetBorderWidthAll(2);
        style.BorderColor = new Color(0.72f, 0.46f, 0.24f, 0.9f);
        style.SetCornerRadiusAll(8);

        var styleHover = (StyleBoxFlat)style.Duplicate();
        styleHover.BgColor = new Color(0.28f, 0.14f, 0.08f, 0.95f);
        styleHover.BorderColor = new Color(1f, 0.72f, 0.36f, 1f);

        var styleLocked = new StyleBoxFlat();
        styleLocked.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        styleLocked.SetBorderWidthAll(2);
        styleLocked.BorderColor = new Color(0.35f, 0.35f, 0.35f, 0.7f);
        styleLocked.SetCornerRadiusAll(8);

        var btn = new Godot.Button
        {
            CustomMinimumSize = new Vector2(180, 240),
            Text              = character.IsUnlocked
                                    ? character.CharacterName
                                    : character.CharacterName + "\n\n[Em Breve]",
        };
        btn.AddThemeFontSizeOverride("font_size", 20);

        if (character.IsUnlocked)
        {
            btn.AddThemeStyleboxOverride("normal",  style);
            btn.AddThemeStyleboxOverride("hover",   styleHover);
            btn.AddThemeStyleboxOverride("pressed", styleHover);
            btn.AddThemeStyleboxOverride("focus",   styleHover);
            btn.Pressed += () => SelectCharacter(character);
        }
        else
        {
            btn.AddThemeStyleboxOverride("normal",   styleLocked);
            btn.AddThemeStyleboxOverride("disabled", styleLocked);
            btn.Modulate = new Color(0.55f, 0.55f, 0.55f, 0.8f);
            btn.Disabled = true;
        }

        return btn;
    }

    private void SelectCharacter(CharacterData character)
    {
        if (!character.IsUnlocked) return;

        _selectedCharacter = character;

        _nameLabel.Text        = character.CharacterName;
        _descriptionLabel.Text = character.Description;
        _hpLabel.Text          = $"Vida: {character.StartingMaxHp}";
        _energyLabel.Text      = $"Energia: {character.StartingEnergy}";
        _goldLabel.Text        = $"Ouro: {character.StartingGold}";

        if (_portrait is not null && character.Portrait is not null)
            _portrait.Texture = character.Portrait;

        _confirmButton.Disabled = false;
    }

    public void OnConfirmPressed()
    {
        if (_selectedCharacter is null) return;

        RunData.SelectedCharacter = _selectedCharacter;

        _gameManagerScene ??= GD.Load<PackedScene>("res://src/Core/GameManager/GameManager.tscn");

        if (_gameManagerScene is null)
        {
            GD.PushError("CharacterSelect: GameManager.tscn não encontrado.");
            return;
        }

        GetTree().ChangeSceneToPacked(_gameManagerScene);
    }

    public void OnBackPressed()
    {
        var menuScene = GD.Load<PackedScene>("res://src/Core/Menu/MainMenu.tscn");
        if (menuScene is null) return;
        GetTree().ChangeSceneToPacked(menuScene);
    }
}
