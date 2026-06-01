using System;
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
    
    [Export] private EnemyEnum enemyType = EnemyEnum.enemy;
   


    [Export] private PackedScene _healthBarScene;
    [Export] private float _intentOffsetY = -230f;
    [Export] private float _attackScaleAdjust = 1.0f;
    [Export] private float _attackStopGap = 240f;
    [Export] private float _attackTimeScale = 2.0f;

    private EnemyHealthBar _healthBar;

    
public List<EnemyPowerData> ActivePowers { get; set; } = new List<EnemyPowerData>();

    private int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            _healthBar?.UpdateHp(_currentHealth);

            if (_currentHealth <= 0)
                Die();
        }
    }
    
    public int MaxHealth { get; private set; }
   private int _strength;
public int Strength
{
    get => _strength;
    set
    {
        _strength = value;
        EffectBar?.UpdateEffects(GetAllEffects());
    }
}
  private int _buffedStrength;
public int BuffedStrength
{
    get => _buffedStrength;
    set
    {
        _buffedStrength = value;
        EffectBar?.UpdateEffects(GetAllEffects());
    }
}

    private int _block;
    public int Block
    {
        get => _block;
        set
        {
            _block = Mathf.Max(0, value);
            if (value > 0) _blockSound?.Play();
            _healthBar?.UpdateBlock(_block);
        }
    }

    public void PlayAttackSound()
    {
        PlayLungeAttack();
    }
    public int Vulnerable
    {
        get => Debuffs.ContainsKey("vulnerable") ? Debuffs["vulnerable"] : 0;
        set => Debuffs["vulnerable"] = value;
    }

    public int Weak
    {
        get => Debuffs.ContainsKey("Weak") ? Debuffs["Weak"] : 0;
        set => Debuffs["Weak"] = value;
    }
    [Export] public EffectBar EffectBar;

    private AudioStreamPlayer _attackSound;
    private AudioStreamPlayer _blockSound;

    private Sprite2D _sprite;
    private Vector2  _baseSpriteScale;
    private Vector2  _baseSpritePos;
    private Tween    _idleTween;
    private Tween    _nodeTween;
    private Tween    _flashTween;
    private Vector2  _baseEnemyPos;
    private bool     _nodeResting = true;

    private Texture2D _idleTexture;
    private Godot.Collections.Array<Texture2D> _attackFrames;
    private bool _hasAttackFrames;
    private Vector2 _idleRendered;

    private CollisionShape2D _collision;
    private Array<EnemyTurn> _turnPatterns;
    private int _currentTurnIndex = 0;
    public System.Collections.Generic.Dictionary<string, int> Debuffs { get; set; } = new();
    public bool NextDebuffDoubled { get; set; } = false;

    
    [Signal]
    public delegate void DiedEventHandler(Enemy enemy);
    
    public override void _Ready()
    {
        _sprite          = GetNodeOrNull<Sprite2D>("Sprite");
        _collision       = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        _intentContainer = GetNodeOrNull<HBoxContainer>("HBoxContainer");
        _attackSound     = GetNodeOrNull<AudioStreamPlayer>("AttackSound");
        _blockSound      = GetNodeOrNull<AudioStreamPlayer>("BlockSound");

        if (_healthBarScene is not null)
        {
            _healthBar = _healthBarScene.Instantiate<EnemyHealthBar>();
            AddChild(_healthBar);
        }

if (_intentContainer != null)
{
    _intentContainer.CustomMinimumSize = new Vector2(200, 56);
    _intentContainer.AddThemeConstantOverride("separation", 4);
    _intentContainer.Alignment = BoxContainer.AlignmentMode.Center;
    _intentContainer.Position = new Vector2(-100f, _intentOffsetY);
    _intentContainer.ZIndex = 1;
}
    else
    {
        GD.PrintErr($"❌ Intent container NÃO encontrado em {Name}!");
    }
        AddToGroup("enemies");
        
        if (Data != null)
            Setup(Data);
    }
    
  
    public void Setup(EnemyData data)
    {
        Data = data;
        Name = data.EnemyName;
        
        MaxHealth = data.MaxHealth;
        Strength  = data.Strength;

        if (_sprite != null && data.Sprite != null)
        {
            _sprite.Texture   = data.Sprite;
            _sprite.Scale = EnemyScaler.CalculateScale(data.Sprite, data.Size, data);
            _sprite.Position  = new Vector2(_sprite.Position.X, data.VerticalOffset);
        }

        Modulate = data.Tint;

        if (_healthBar is not null)
        {
            float dispW = EnemyScaler.CalculateDisplayWidth(data.Sprite, data.Size, this.Data);
            float dispH = EnemyScaler.CalculateDisplayHeight(data.Sprite, data.Size, this.Data);
            _healthBar.Setup(MaxHealth, data.Size, dispW);
            _healthBar.Position = new Vector2(_healthBar.Position.X,
                data.VerticalOffset + dispH / 2f + 8f);
        }

        _turnPatterns = data.TurnPatterns;
        UpdateHitbox();

        // Set HP after health bar is configured so the initial UpdateHp fires correctly
        CurrentHealth = MaxHealth;
        foreach (var power in data.StartingPowers)
        {
            if (power != null)
                ActivePowers.Add(power.Duplicate() as EnemyPowerData);
        }

        if (_sprite is not null)
        {
            _baseSpriteScale = _sprite.Scale;
            _baseSpritePos   = _sprite.Position;

            _idleTexture     = _sprite.Texture;
            _attackFrames    = data.AttackFrames;
            _hasAttackFrames = _attackFrames != null && _attackFrames.Count > 0;
            _idleRendered    = (_idleTexture?.GetSize() ?? Vector2.One) * _baseSpriteScale;

            StartIdle();
        }
        Callable.From(PositionOverlaysAboveSprite).CallDeferred();
    }
    public void TriggerPowers(Action<EnemyPowerData> trigger)
{
    foreach (var power in ActivePowers)
        trigger(power);
}
    public float GetDisplayWidth()
    {
        if (Data?.Sprite == null)
            return 100f;
        
        return EnemyScaler.CalculateDisplayWidth(Data.Sprite, Data.Size,Data);
    }
    
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
public List<(EffectData data, int value)> GetAllEffects()
{
    var list = new List<(EffectData, int)>();

    foreach (var kvp in Debuffs)
    {
        if (kvp.Value <= 0) continue;
        var data = EffectManager.Instance.GetEffect(kvp.Key.ToLower());
        if (data != null)
            list.Add((data, kvp.Value));
    }

    if (Strength > 0)
    {
        var data = EffectManager.Instance.GetEffect("strength");
        if (data != null)
            list.Add((data, Strength));
    }

    var grouped = new Godot.Collections.Dictionary<string, int>();
    foreach (var p in ActivePowers)
    {
        if (!grouped.ContainsKey(p.Id))
            grouped[p.Id] = 0;
        grouped[p.Id]++;
    }

   foreach (var kvp in grouped)
{
    var power = ActivePowers.Find(p => p.Id == kvp.Key);
    var data = EffectManager.Instance.GetEffect(kvp.Key);
    if (data != null)
        list.Add((data, power?.EffectValue ?? kvp.Value));
}

    return list;
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

        if (intent.Type == IntentData.IntentType.Attack) // removeu o && intent.Value > 0
{
    int finalDamage = CalculateFinalDamage(intent.Value);
    
    if (finalDamage > 0)
    {
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
        TriggerPowers(p => p.OnDamageTaken(this, damage));

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
    
    public void FlashDamage()
    {
        if (_sprite is null) return;
        _flashTween?.Kill();
        _sprite.Modulate = Colors.White;
        _flashTween = CreateTween();
        _flashTween.TweenProperty(_sprite, "modulate", new Color(1f, 0.35f, 0.35f), 0.05f);
        _flashTween.TweenProperty(_sprite, "modulate", Colors.White, 0.18f);
    }
    
    private void ShowAttackFrame(int index)
    {
        if (!_hasAttackFrames || _sprite is null) return;
        if (index < 0 || index >= _attackFrames.Count) return;
        var tex = _attackFrames[index];
        if (tex is null) return;

        _sprite.Texture = tex;
        Vector2 sz = tex.GetSize();

        float factor = (sz.Y > 0f ? _idleRendered.Y / sz.Y : 1f) * _attackScaleAdjust;
        float signX = _baseSpriteScale.X < 0f ? -1f : 1f;
        _sprite.Scale = new Vector2(factor * signX, factor);
    }

    private void RestoreIdleSprite()
    {
        if (_sprite is null) return;
        if (_idleTexture is not null) _sprite.Texture = _idleTexture;
        _sprite.Scale = _baseSpriteScale;
    }

    private void StartIdle()
    {
        if (_sprite is null) return;
        _idleTween?.Kill();
        _sprite.Scale    = _baseSpriteScale;
        _sprite.Position = _baseSpritePos;

        Vector2 Mul(float fx, float fy) =>
            new Vector2(_baseSpriteScale.X * fx, _baseSpriteScale.Y * fy);

        _idleTween = CreateTween().SetLoops()
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        _idleTween.TweenProperty(_sprite, "scale", Mul(0.99f, 1.02f), 1.1f);
        _idleTween.Parallel().TweenProperty(_sprite, "position",
            _baseSpritePos + new Vector2(0f, -3f), 1.1f);
        _idleTween.TweenProperty(_sprite, "scale", _baseSpriteScale, 1.1f);
        _idleTween.Parallel().TweenProperty(_sprite, "position", _baseSpritePos, 1.1f);
    }

    private void StopIdle()
    {
        _idleTween?.Kill();
        _idleTween = null;
        if (_sprite is not null)
        {
            _sprite.Scale    = _baseSpriteScale;
            _sprite.Position = _baseSpritePos;
        }
    }

    private void CaptureRestIfNeeded()
    {
        if (_nodeResting) _baseEnemyPos = Position;
    }

    private void PlayLungeAttack()
    {
        CaptureRestIfNeeded();
        _nodeTween?.Kill();
        Position = _baseEnemyPos;
        _nodeResting = false;

        StopIdle();
        _attackSound?.Play();

        var player = PlayerManager.Instance?.Player;
        Vector2 dir;
        Vector2 lungePos;
        if (player is not null && IsInstanceValid(player))
        {
            Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
            float gap = toPlayer.Length();
            dir = gap > 0.001f ? toPlayer / gap : Vector2.Left;
            float travel = Mathf.Max(0f, gap - _attackStopGap);
            lungePos = _baseEnemyPos + dir * travel;
        }
        else
        {
            dir = Vector2.Left;
            lungePos = _baseEnemyPos + dir * 200f;
        }

        float _travelLen = (lungePos - _baseEnemyPos).Length();
        float dashTime   = Mathf.Clamp(_travelLen / 2600f, 0.16f, 0.32f) * _attackTimeScale;
        float returnTime = Mathf.Clamp(_travelLen / 1800f, 0.32f, 0.55f) * _attackTimeScale;

        _nodeTween = CreateTween();

        if (_hasAttackFrames)
        {
            ShowAttackFrame(0);
            Vector2 windupPos = _baseEnemyPos - dir * 26f;

            _nodeTween.TweenProperty(this, "position", windupPos, 0.14f * _attackTimeScale)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            _nodeTween.TweenProperty(this, "position", lungePos, dashTime)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            _nodeTween.TweenCallback(Callable.From(() => { ShowAttackFrame(Mathf.Min(1, _attackFrames.Count - 1)); OnLungeImpact(dir); }));
            _nodeTween.TweenInterval(0.12f * _attackTimeScale);
            _nodeTween.TweenProperty(this, "position", _baseEnemyPos, returnTime)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            _nodeTween.TweenCallback(Callable.From(() =>
            {
                RestoreIdleSprite();
                _nodeResting = true;
                StartIdle();
                Callable.From(PositionOverlaysAboveSprite).CallDeferred();
            }));
        }
        else
        {
            var punch = new Vector2(_baseSpriteScale.X * 1.10f, _baseSpriteScale.Y * 0.90f);
            _nodeTween.TweenProperty(this, "position", lungePos, 0.10f * _attackTimeScale)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            _nodeTween.Parallel().TweenProperty(_sprite, "scale", punch, 0.10f * _attackTimeScale)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            _nodeTween.TweenCallback(Callable.From(() => OnLungeImpact(dir)));
            _nodeTween.TweenProperty(this, "position", _baseEnemyPos, 0.30f * _attackTimeScale)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            _nodeTween.Parallel().TweenProperty(_sprite, "scale", _baseSpriteScale, 0.30f * _attackTimeScale)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            _nodeTween.TweenCallback(Callable.From(() => { _nodeResting = true; StartIdle(); }));
        }
    }

    private void OnLungeImpact(Vector2 dir)
    {
        CombatManager.Instance?.ShakeScreen(6f, 0.22f);
        PlayerManager.Instance?.Player?.PlayHitReaction(dir);
    }

    public void PlayHitReaction(Vector2 pushDir)
    {
        CaptureRestIfNeeded();
        _nodeTween?.Kill();
        if (_hasAttackFrames) RestoreIdleSprite();
        Position = _baseEnemyPos;
        _nodeResting = false;

        Vector2 knock = pushDir.Normalized() * 40f;
        _nodeTween = CreateTween();
        _nodeTween.TweenProperty(this, "position", _baseEnemyPos + knock, 0.06f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _nodeTween.TweenProperty(this, "position", _baseEnemyPos, 0.30f)
            .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        _nodeTween.TweenCallback(Callable.From(() => _nodeResting = true));
    }

    private float GetSpriteTopY()
    {
        var idleTex = _idleTexture ?? _sprite?.Texture;
        float idleH = idleTex != null ? _idleRendered.Y : 160f;
        float centerY = _baseSpritePos.Y + (_sprite?.Offset.Y ?? 0f) * _baseSpriteScale.Y;
        bool centered = _sprite == null || _sprite.Centered;
        return centered ? centerY - idleH * 0.5f : centerY;
    }

    private void PositionOverlaysAboveSprite()
    {
        float topY        = GetSpriteTopY();
        const float margin = 24f;

        if (EffectBar is not null)
        {
            float barH = EffectBar.Size.Y > 0f ? EffectBar.Size.Y : 40f;
            EffectBar.Position = new Vector2(EffectBar.Position.X, topY - margin - barH);
        }

        var intentBox = GetNodeOrNull<Control>("HBoxContainer");
        if (intentBox is not null)
        {
            float barH    = EffectBar?.Size.Y > 0f ? EffectBar.Size.Y : 40f;
            float intentH = intentBox.Size.Y > 0f ? intentBox.Size.Y : 56f;
            intentBox.Position = new Vector2(intentBox.Position.X,
                topY - margin - barH - intentH - 8f);
        }
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
        GD.Print("combatManager: " + combatManager);
        GD.Print("ActivePowers: " + combatManager?.Player.ActivePowers.Count);
        combatManager?.Player.TriggerRelics(r => r.OnDebuffApplied(combatManager.Player, debuff, this));
        combatManager?.Player.TriggerPowers(p => p.OnDebuffApplied(combatManager.Player, debuff, finalValue, this));
        EffectBar?.UpdateEffects(GetAllEffects());
        Callable.From(PositionOverlaysAboveSprite).CallDeferred();
    }
     public int GetDebuffValue(string debuff)
    {
        return Debuffs.TryGetValue(debuff, out int value) ? value : 0;

    }

    public void UpdateTemporaryEffects()
    {
        var keys = new List<string>(Debuffs.Keys);
        foreach (var key in keys)
        {
            
            if (key == "Sangria" && Debuffs[key] > 0)
        {
            int sangriaDamage = Debuffs[key]; 
            TakeDamage(sangriaDamage);
            
            var player = PlayerManager.Instance.Player;
            if (player != null)
            {
                player.TryToHeal(sangriaDamage); 
            }
        }

            if (Debuffs[key] > 0)
                Debuffs[key]--;
            if (Debuffs[key] <= 0)
                Debuffs.Remove(key);
        }

        BuffedStrength = 0;
        EffectBar?.UpdateEffects(GetAllEffects());
        Callable.From(PositionOverlaysAboveSprite).CallDeferred();
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