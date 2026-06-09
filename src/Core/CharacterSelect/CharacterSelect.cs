using Godot;
using System.Collections.Generic;

public partial class CharacterSelect : Control
{
    [Export] private PackedScene _gameManagerScene;

    private Control _charactersContainer;
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
    private readonly Dictionary<CharacterData, CharWidget> _charWidgets = new();

    private sealed class CharWidget
    {
        public TextureRect Sprite;
        public Label       NameLabel;
        public Vector2     BaseScale;      // scale from .tscn
        public Vector2     VisualTopLeft;  // top-left on screen, captured while pivot is (0,0)
    }

    public override void _Ready()
    {
        _charactersContainer = GetNode<Control>("Background/CharactersContainer");
        _nameLabel           = GetNode<Label>("Background/InfoPanel/InfoHBox/TextContent/Name");
        _descriptionLabel    = GetNode<Label>("Background/InfoPanel/InfoHBox/TextContent/Description");
        _hpLabel             = GetNode<Label>("Background/InfoPanel/InfoHBox/TextContent/Stats/HP");
        _energyLabel         = GetNode<Label>("Background/InfoPanel/InfoHBox/TextContent/Stats/Energy");
        _goldLabel           = GetNode<Label>("Background/InfoPanel/InfoHBox/TextContent/Stats/Gold");
        _portrait            = GetNodeOrNull<TextureRect>("Background/InfoPanel/InfoHBox/Portrait");
        _confirmButton       = GetNode<Godot.Button>("Background/ConfirmButton");
        _backButton          = GetNodeOrNull<Godot.Button>("Background/BackButton");

        _confirmButton.Disabled = true;

        LoadCharacters();
        PopulateCharacters();

        Callable.From(UpdateSpriteLayout).CallDeferred();
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
        var corvo = new CharacterData
        {
            CharacterName  = "O Corvo",
            Description    = "Um médico da morte que usa conhecimento proibido para derrotar seus inimigos no Datastage. Especialista em venenos e maldições.",
            StartingMaxHp  = 75,
            StartingGold   = 100,
            StartingEnergy = 3,
            IsUnlocked     = true,
            Portrait       = GD.Load<Texture2D>("res://Test/TestImagesSprites/Untitled design (9).png"),
        };
        _characters.Add(corvo);

        var zeIdle   = GD.Load<Texture2D>("res://Test/TestMcFrames/Idle/Seu_zé.png");
        var zeAtaque = GD.Load<Texture2D>("res://Test/TestMcFrames/Idle/Seu_zé_Ataque.png");

        var seZe = new CharacterData
        {
            CharacterName  = "Seu Zé",
            Description    = "Um barraqueiro de frutas que empunha um Coletor de Fruta como arma e, na outra mão, uma Manga Mágica que lhe concede o poder das cartas.",
            StartingMaxHp  = 80,
            StartingGold   = 80,
            StartingEnergy = 3,
            IsUnlocked     = true,
            Portrait       = zeIdle,
            IdleSprite     = zeIdle,
            AttackSprite   = zeAtaque,
            SpriteScale    = 0.8f,
        };
        _characters.Add(seZe);
    }

    private void PopulateCharacters()
    {
        for (int i = 0; i < _characters.Count; i++)
            BuildCharacterSprite(_characters[i], i);
    }

    private void BuildCharacterSprite(CharacterData character, int index)
    {
        var spriteCorvo = GetNode<TextureRect>("Background/CharactersContainer/SpriteCorvo");
        var spriteSeuZe = GetNode<TextureRect>("Background/CharactersContainer/SpriteSeuZe");
        var sprite = index == 0 ? spriteCorvo : spriteSeuZe;

        sprite.Texture  = character.Portrait ?? character.IdleSprite;
        sprite.Modulate = character.IsUnlocked
            ? new Color(0.78f, 0.78f, 0.78f, 1f)
            : new Color(0.38f, 0.38f, 0.38f, 0.65f);

        var font = GD.Load<Font>("res://Data/Fonte/citadel_of_blackrose/Enchanted Land.otf");

        var nameLabel = new Label();
        nameLabel.Text                = character.IsUnlocked ? character.CharacterName : character.CharacterName + "\n[Em Breve]";
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.MouseFilter         = Control.MouseFilterEnum.Ignore;
        nameLabel.AddThemeFontSizeOverride("font_size", 30);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.88f, 0.65f, 1f));
        nameLabel.Modulate            = new Color(1f, 1f, 1f, 0f);
        if (font is not null)
            nameLabel.AddThemeFontOverride("font", font);

        _charactersContainer.AddChild(nameLabel);
        nameLabel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);

        // Label position and pivot correction are deferred — Size may not be stable yet.
        _charWidgets[character] = new CharWidget
        {
            Sprite        = sprite,
            NameLabel     = nameLabel,
            BaseScale     = sprite.Scale,    // scale from .tscn, reliable here
            VisualTopLeft = sprite.Position, // pivot is (0,0) so Position == visual top-left
        };

        if (!character.IsUnlocked) return;

        sprite.MouseFilter = Control.MouseFilterEnum.Stop;
        sprite.MouseEntered += () => OnSpriteHovered(character);
        sprite.MouseExited  += () => OnSpriteUnhovered(character);
        sprite.GuiInput += inputEvent =>
        {
            if (inputEvent is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                SelectCharacterVisual(character);
        };
    }

    private void OnSpriteHovered(CharacterData character)
    {
        if (!_charWidgets.TryGetValue(character, out var w)) return;

        PreviewCharacter(character);

        var tween = CreateTween().SetParallel();
        tween.TweenProperty(w.Sprite, "scale", w.BaseScale * 1.06f, 0.18f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(w.Sprite, "modulate", new Color(1f, 0.92f, 0.72f, 1f), 0.18f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(w.NameLabel, "modulate", new Color(1f, 1f, 1f, 1f), 0.18f);
    }

    private void OnSpriteUnhovered(CharacterData character)
    {
        if (!_charWidgets.TryGetValue(character, out var w)) return;

        if (_selectedCharacter != character)
            ClearInfoPanel();

        bool isSelected = _selectedCharacter == character;

        var tween = CreateTween().SetParallel();
        tween.TweenProperty(w.Sprite, "scale", w.BaseScale, 0.22f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        var targetMod = isSelected
            ? new Color(1f, 0.88f, 0.55f, 1f)
            : new Color(0.78f, 0.78f, 0.78f, 1f);
        tween.TweenProperty(w.Sprite, "modulate", targetMod, 0.22f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        if (!isSelected)
            tween.TweenProperty(w.NameLabel, "modulate", new Color(1f, 1f, 1f, 0f), 0.22f);
    }

    private void SelectCharacterVisual(CharacterData character)
    {
        if (!character.IsUnlocked) return;

        var previous = _selectedCharacter;
        _selectedCharacter = character;
        _confirmButton.Disabled = false;

        if (previous is not null && previous != character && _charWidgets.TryGetValue(previous, out var prevW))
        {
            var prevTween = CreateTween().SetParallel();
            prevTween.TweenProperty(prevW.Sprite, "modulate", new Color(0.78f, 0.78f, 0.78f, 1f), 0.22f)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            prevTween.TweenProperty(prevW.NameLabel, "modulate", new Color(1f, 1f, 1f, 0f), 0.22f);
        }

        if (_charWidgets.TryGetValue(character, out var w))
        {
            var selTween = CreateTween().SetParallel();
            selTween.TweenProperty(w.Sprite, "modulate", new Color(1f, 0.88f, 0.55f, 1f), 0.22f)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            selTween.TweenProperty(w.NameLabel, "modulate", new Color(1f, 1f, 1f, 1f), 0.15f);
        }

        UpdateInfoPanel(character);
    }

    private void PreviewCharacter(CharacterData character)
    {
        UpdateInfoPanel(character);
    }

    private void UpdateInfoPanel(CharacterData character)
    {
        _nameLabel.Text        = character.CharacterName;
        _descriptionLabel.Text = character.Description;
        _hpLabel.Text          = $"Vida: {character.StartingMaxHp}";
        _energyLabel.Text      = $"Energia: {character.StartingEnergy}";
        _goldLabel.Text        = $"Ouro: {character.StartingGold}";
        if (_portrait is not null)
            _portrait.Texture = character.Portrait;
    }

    private void ClearInfoPanel()
    {
        if (_selectedCharacter is not null)
        {
            UpdateInfoPanel(_selectedCharacter);
            return;
        }
        _nameLabel.Text        = "—";
        _descriptionLabel.Text = "Selecione um personagem para ver suas informações.";
        _hpLabel.Text          = "Vida: —";
        _energyLabel.Text      = "Energia: —";
        _goldLabel.Text        = "Ouro: —";
        if (_portrait is not null)
            _portrait.Texture = null;
    }

    private void UpdateSpriteLayout()
    {
        foreach (var kvp in _charWidgets)
        {
            var w      = kvp.Value;
            var sprite = w.Sprite;

            var size  = sprite.Size;   // unscaled, e.g. (2660, 1600)
            var scale = w.BaseScale;

            // Pivot at unscaled center so hover scale animates from the visual centre.
            var pivot = size / 2f;
            sprite.PivotOffset = pivot;

            // Re-express the same visual rectangle with the new pivot.
            // Uses VisualTopLeft captured before pivot was changed (idempotent).
            sprite.Position = w.VisualTopLeft - pivot * (Vector2.One - scale);

            // On-screen width for the label.
            var renderedW = size.X * scale.X;

            // Name ABOVE the sprite (over the head), aligned to visual top-left.
            const float labelH = 48f;
            w.NameLabel.Position = new Vector2(w.VisualTopLeft.X, w.VisualTopLeft.Y - labelH - 8f);
            w.NameLabel.Size     = new Vector2(renderedW, labelH);
        }
    }

    public void OnConfirmPressed()
    {
        if (_selectedCharacter is null) return;

        SaveManager.Instance?.DeleteSave();
        RunData.SelectedCharacter = _selectedCharacter;

        if (_selectedCharacter.CharacterName == "Seu Zé")
            CursorManager.Instance?.SetSeuZe();
        else
            CursorManager.Instance?.SetCorvo();

        string cutsceneId = $"intro_{_selectedCharacter.CharacterName}";
        CutsceneData intro = _selectedCharacter.CharacterName switch
        {
            "O Corvo" => BuildCorvoIntro(),
            "Seu Zé"  => BuildSeuZeIntro(),
            _         => null,
        };

        const string GameMgrPath = "res://src/Core/GameManager/GameManager.tscn";

        if (intro is not null && !MetaProgress.HasSeenCutscene(cutsceneId))
        {
            var cutsceneScene = GD.Load<PackedScene>("res://src/Core/Cutscene/CutscenePlayer.tscn");
            if (cutsceneScene is not null)
            {
                SceneLoader.Instance?.StartLoad(GameMgrPath); // carrega durante a cutscene
                var cutscene = cutsceneScene.Instantiate<CutscenePlayer>();
                cutscene.PendingSeenId = cutsceneId;
                AddChild(cutscene);
                cutscene.Play(intro);
                return;
            }
        }

        if (SceneLoader.Instance is not null)
            SceneLoader.Instance.GoTo(GameMgrPath, showLoading: true);
        else
        {
            _gameManagerScene ??= GD.Load<PackedScene>(GameMgrPath);
            GetTree().ChangeSceneToPacked(_gameManagerScene);
        }
    }

    private static CutsceneData BuildCorvoIntro()
    {
        return new CutsceneData
        {
            NextScene = "res://src/Core/GameManager/GameManager.tscn",
            Slides = new[]
            {
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/Corvo/entrada_corvo.jpg"),
                    NarrativeText = "Após anos vagando pelo reino, o Médico da Peste finalmente " +
                        "chegou à lendária Fortaleza de Unifor — a maior escola de magos e " +
                        "inovadores do reino.",
                    Duration = 7.0f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/Corvo/aprendizado_corvo.jpg"),
                    NarrativeText = "Sob a tutela do grande mago Narak, ele aprendeu a arte proibida " +
                        "de digitalizar monstros — aprisionando suas essências dentro de cartas " +
                        "com a antiga magia da modelagem.",
                    Duration = 7.5f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/Corvo/formatura_corvo.png"),
                    NarrativeText = "Após anos de dedicação, o Médico da Peste se formou na Fortaleza " +
                        "de Unifor. O grande mago Narak lhe entregou seu diploma pessoalmente — " +
                        "pronto para enfrentar os perigos do reino.",
                    Duration = 7.0f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/Corvo/partida_corvo.png"),
                    NarrativeText = "Com seu diploma em mãos e suas cartas a tiracolo, o Médico da " +
                        "Peste deixou os muros de Unifor e partiu rumo ao reino de Valdermoor — " +
                        "onde a verdadeira aventura aguardava.",
                    Duration = 7.5f,
                },
            },
        };
    }

    private static CutsceneData BuildSeuZeIntro()
    {
        return new CutsceneData
        {
            NextScene = "res://src/Core/GameManager/GameManager.tscn",
            Slides = new[]
            {
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/SeuZe/dia_de_feira.png"),
                    NarrativeText = "Mais um dia de feira. Seu Zé vendia suas frutas como em toda manhã " +
                        "— sem saber que seria a última vez que sua vida seria simples.",
                    Duration = 7.0f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/SeuZe/a_carta.png"),
                    NarrativeText = "Entre as mangas, achou uma carta estranha, quente ao toque. Quando a " +
                        "segurou, a fruta amadureceu num piscar de olhos — e o velho feirante descobriu, " +
                        "tarde demais, o dom que sempre carregou.",
                    Duration = 7.5f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/SeuZe/aluno_mais_velho.png"),
                    NarrativeText = "Levou seu dom à lendária Fortaleza de Unifor, onde se tornou o aluno " +
                        "mais velho que aqueles muros já viram. Sob a tutela do arquimago Narah, " +
                        "formou-se invocador.",
                    Duration = 7.0f,
                },
                new CutsceneSlideData
                {
                    Image = GD.Load<Texture2D>("res://Cutscenes/SeuZe/partida.png"),
                    NarrativeText = "Cartas a tiracolo e cajado em punho, Seu Zé partiu rumo a Valdermoor. " +
                        "Algo o esperava lá — e sussurrava que ele precisava quebrar o loop.",
                    Duration = 7.5f,
                },
            },
        };
    }

    public void OnBackPressed()
    {
        CursorManager.Instance?.SetCorvo();
        var menuScene = GD.Load<PackedScene>("res://src/Core/Menu/MainMenu.tscn");
        if (menuScene is null) return;
        GetTree().ChangeSceneToPacked(menuScene);
    }
}
