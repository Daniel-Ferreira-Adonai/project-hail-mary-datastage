using Godot;
using Godot.Collections;

public partial class Enemy : Node2D
{
    [Export] public EnemyData Data { get; set; }
    
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
    public int Vulnerable { get; set; } = 0;
    public int Weak { get; set; } = 0;
    
    private Label _healthLabel;
    private Sprite2D _sprite;
    
    private CollisionShape2D _collision;
    private Array<EnemyTurn> _turnPatterns;
    private int _currentTurnIndex = 0;
    
    [Signal]
    public delegate void DiedEventHandler(Enemy enemy);
    
    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("health");
        _sprite = GetNodeOrNull<Sprite2D>("Sprite");
        _collision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");

        
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
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        
        FlashDamage();
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
        
        EmitSignal(SignalName.Died, this);
        
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
        tween.TweenProperty(this, "scale", Vector2.Zero, 0.5f).SetTrans(Tween.TransitionType.Back);
        tween.TweenCallback(Callable.From(QueueFree));
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
        if (Vulnerable > 0) Vulnerable--;
        if (Weak > 0) Weak--;
        
        BuffedStrength = 0;
    }
}