using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Enemy : Node2D
{
    [Export] public EnemyData Data { get; set; }
    [Export] private PackedScene _damageLabelScene;
    [Export] private HBoxContainer _intentContainer;
    
    [Export] private Texture2D _attackIcon;
    [Export] private Texture2D _defenseIcon;
    [Export] private Texture2D _buffIcon;
    [Export] private Texture2D _debuffIcon;
    [Export] private Texture2D _unknownIcon;
    
    [Export] private float _intentOffsetY = -160f;

    private int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            UpdateHealthLabel();
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
    }
    
    public int MaxHealth { get; private set; }
    public int Strength { get; set; }
    public int BuffedStrength { get; set; }
    public int Vulnerable
    {
        get => Debuffs.ContainsKey("Vulnerable") ? Debuffs["Vulnerable"] : 0;
        set => Debuffs["Vulnerable"] = value;
    }

    public int Weak
    {
        get => Debuffs.ContainsKey("Weak") ? Debuffs["Weak"] : 0;
        set => Debuffs["Weak"] = value;
    }
    
    private Label _healthLabel;
    private Sprite2D _sprite;
    
    private CollisionShape2D _collision;
    private Array<EnemyTurn> _turnPatterns;
    private int _currentTurnIndex = 0;
    public System.Collections.Generic.Dictionary<string, int> Debuffs { get; set; } = new();
    public bool NextDebuffDoubled { get; set; } = false;

    
    [Signal]
    public delegate void DiedEventHandler(Enemy enemy);
    
    public override void _Ready()
    {
       _healthLabel = GetNode<Label>("health");
    _sprite = GetNodeOrNull<Sprite2D>("Sprite");
    _collision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    _intentContainer = GetNodeOrNull<HBoxContainer>("HBoxContainer");

if (_intentContainer != null)
{
    _intentContainer.Position = new Vector2(-60, _intentOffsetY); // ← centraliza mais
    _intentContainer.CustomMinimumSize = new Vector2(120, 56);
    _intentContainer.AddThemeConstantOverride("separation", 4);
    _intentContainer.Alignment = BoxContainer.AlignmentMode.Center;
    _intentContainer.AnchorLeft = 0.5f;
    _intentContainer.AnchorRight = 0.5f;
    _intentContainer.OffsetLeft = -100;
    _intentContainer.OffsetRight = 60;
}
    else
    {
        GD.PrintErr($"❌ Intent container NÃO encontrado em {Name}!");
    }
        AddToGroup("enemies");
        
        if (Data != null)
        {
            Setup(Data);
        }
        else
        {
            MaxHealth = _currentHealth; 
            UpdateHealthLabel();
        }
    }
    
  
    public void Setup(EnemyData data)
    {
        Data = data;
        Name = data.EnemyName;
        
        MaxHealth = data.MaxHealth;
        CurrentHealth = MaxHealth;
        Strength = data.Strength;
        
        if (_sprite != null && data.Sprite != null)
        {
            _sprite.Texture = data.Sprite;
            
            _sprite.Scale = EnemyScaler.CalculateScale(data.Sprite, data.Size);
            
            _sprite.Position = new Vector2(_sprite.Position.X, data.VerticalOffset);
        }
        
        Modulate = data.Tint;
        
        _turnPatterns = data.TurnPatterns;
        UpdateHitbox();
        UpdateHealthLabel();
    }
    
    public float GetDisplayWidth()
    {
        if (Data?.Sprite == null)
            return 100f;
        
        return EnemyScaler.CalculateDisplayWidth(Data.Sprite, Data.Size);
    }
    
    // Pega os intents do turno atual SEM avançar o índice (para preview)
    public Array<IntentData> GetNextTurnIntents()
    {
        if (_turnPatterns == null || _turnPatterns.Count == 0)
        {
            GD.PrintErr($"{Name} não tem padrões de turno definidos!");
            return new Array<IntentData>();
        }
        
        var currentTurn = _turnPatterns[_currentTurnIndex % _turnPatterns.Count];
        return currentTurn.Actions;
    }
    
    // Pega os intents e AVANÇA o índice (para execução)
    public Array<IntentData> getTurnIntents()
    {
        if (_turnPatterns == null || _turnPatterns.Count == 0)
        {
            GD.PrintErr($"{Name} não tem padrões de turno definidos!");
            return new Array<IntentData>();
        }
        
        var currentTurn = _turnPatterns[_currentTurnIndex % _turnPatterns.Count];
        
        _currentTurnIndex++;
        
        return currentTurn.Actions;
    }

public void ShowIntent(Array<IntentData> intents)
{
    if (_intentContainer == null || intents == null) return;

    ClearIntent();

    bool first = true;
    foreach (var intent in intents)
    {
        if (!first)
        {
            var separator = new Label();
            separator.Text = "|";
            separator.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.8f));
            separator.AddThemeFontSizeOverride("font_size", 28);
            separator.VerticalAlignment = VerticalAlignment.Center;
            _intentContainer.AddChild(separator);
        }
        first = false;

        var iconTexture = new TextureRect();
        iconTexture.Texture = GetIconForIntent(intent.Type);
        iconTexture.CustomMinimumSize = new Vector2(56, 56);
        iconTexture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        iconTexture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _intentContainer.AddChild(iconTexture);

        if (intent.Type == IntentData.IntentType.Attack && intent.Value > 0)
        {
            int finalDamage = CalculateFinalDamage(intent.Value);
            
            var label = new Label();
            label.Text = finalDamage.ToString();
            label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
            label.AddThemeConstantOverride("outline_size", 8);
            label.AddThemeFontSizeOverride("font_size", 32);
            label.VerticalAlignment = VerticalAlignment.Center;
            _intentContainer.AddChild(label);
        }
    }
}

    public void ClearIntent()
    {
        if (_intentContainer == null) return;

        foreach (Node child in _intentContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

   private Texture2D GetIconForIntent(IntentData.IntentType intent)
{
    return intent switch
    {
        IntentData.IntentType.Attack => _attackIcon,
        IntentData.IntentType.Defend => _defenseIcon,
        IntentData.IntentType.Buff => _buffIcon,
        IntentData.IntentType.Debuff => _debuffIcon,
        _ => _unknownIcon
    };
}

    private void UpdateHitbox()
    {
        if (_sprite?.Texture == null || _collision == null)
            return;

        Vector2 size = _sprite.Texture.GetSize() * _sprite.Scale;

        var shape = new RectangleShape2D();
        shape.Size = size;

        _collision.Shape = shape;

        _collision.Position = Vector2.Zero;
    }

    private void SpawnDamageLabel(int damage)
    {
        if (_damageLabelScene == null) return;
        
        var label = _damageLabelScene.Instantiate<DamageLabel>();
        AddChild(label);
        label.Scale = new Vector2(3f, 3f) / Scale;
        label.Visible = true;
        label.ZIndex = 100 + GetChildCount();
        label.Position = new Vector2(0, -50);
            
        label.Setup(damage);
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        SpawnDamageLabel(damage);

        var player = PlayerManager.Instance.Player;
        
        if (player._isAttacking)
        {
            player.AttackImpact += OnImpact;

            void OnImpact()
            {
                player.AttackImpact -= OnImpact;
                
                if (IsInstanceValid(this) && !IsQueuedForDeletion())
                    FlashDamage();
            }
        }
        else
        {
            if (IsInstanceValid(this) && !IsQueuedForDeletion())
                FlashDamage();
        }
    }
    
    private void FlashDamage()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", Colors.Red, 0.1f);
        tween.TweenProperty(this, "modulate", 
            Data?.Tint ?? Colors.White, 0.1f);
    }
    
    private void Die()
    {
        GD.Print($"{Name} morreu!");
        RemoveFromGroup("enemies"); 

        EmitSignal(SignalName.Died, this);
        
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
        tween.TweenProperty(this, "scale", Vector2.Zero, 0.5f).SetTrans(Tween.TransitionType.Back);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    public void ApplyDebuff(string debuff, int value)
    {
        if (!Debuffs.ContainsKey(debuff))
            Debuffs[debuff] = 0;

        int finalValue = NextDebuffDoubled ? value * 2 : value;
        NextDebuffDoubled = false;
        Debuffs[debuff] += finalValue;

        var combatManager = GetTree().GetFirstNodeInGroup("combat_manager") as CombatManager;
        combatManager?.Player.TriggerRelics(r => r.OnDebuffApplied(combatManager.Player, debuff));
    }

    private void UpdateHealthLabel()
    {
        if (_healthLabel != null)
        {
            _healthLabel.Text = _currentHealth <= 0 ? "0" : _currentHealth.ToString();
        }
    }
    
    public void UpdateTemporaryEffects()
    {
        var keys = new List<string>(Debuffs.Keys);
        foreach (var key in keys)
        {
            if (Debuffs[key] > 0)
                Debuffs[key]--;
            if (Debuffs[key] <= 0)
                Debuffs.Remove(key);
        }

        BuffedStrength = 0;
    }
    private int CalculateFinalDamage(int baseDamage)
{
    float damage = baseDamage;
    
    // Adiciona a força do inimigo
    damage += Strength;
    damage += BuffedStrength;
    
    // Aplica Weak (enfraquecido) - reduz 25%
    if (Weak > 0)
        damage *= 0.75f;
    
    // Vulnerável é aplicado no PLAYER, não no inimigo
    // Então não calculamos aqui, pois o inimigo não sabe se o player está vulnerável
    // Isso seria calculado quando o ataque realmente acontece
    
    return Mathf.Max(0, Mathf.FloorToInt(damage));
}
}