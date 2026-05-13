using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class Player : Node2D
{
	
	[Export] private int _startingMaxHp = 75;
	private int _maxHp;
	[Export] public int MaxHp
	{
		get => _maxHp;
		set
		{
			_maxHp = value;
			GameManager.Instance?.UpdateTopBar();
			UpdateLabelValues();
		}
	}
	private int _currentHp;
	[Export] public int currentHp
	{
		get => _currentHp;
		set
		{
			_currentHp = value;
			GameManager.Instance?.UpdateTopBar();
			UpdateLabelValues();
		}
	}

	[Export] private int _startingGold = 100;
	private int _gold;
	public int Gold
	{
		get => _gold;
		set
		{
			_gold = value;
			GameManager.Instance?.UpdateTopBar();
		}
	}
	[Export] private int _BlockValue;
	[Export] public int BlockValue
	{
		get => _BlockValue;
		set
		{
			_BlockValue = value;
			UpdateLabelValues();
		}
	}


	
	
	private List<CardData> _BaseDeck = new List<CardData>();  

	public List<RelicData> Relics { get; set; } = new();
	public int BonusCardsToDraw { get; set; } = 0;
	public bool NextDebuffDoubled { get; set; } = false;
	[Export] public BarraDeVida _hpBar;

	public bool _isAttacking = false;
	[Export] public int ImpactFrame = 4;


 	[Signal]
    public delegate void StatsChangedEventHandler();
	private int _TemporaryDexterity = 0;

	
	[Export] public int TemporaryDexterity
    {
        get => _TemporaryDexterity;
        set
        {
            _TemporaryDexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _temporaryStrength = 0;

	[Export] public int TemporaryStrength
    {
        get => _temporaryStrength;
        set
        {
            _temporaryStrength = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _perCombatTemporaryStrength = 0;

	[Export] public int PerCombatTemporaryStrength
    {
        get => _perCombatTemporaryStrength;
        set
        {
            _perCombatTemporaryStrength = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _perCombatTemporarydexterity = 0;
    [Export] public int PerCombatTemporaryDexterity
    {
        get => _perCombatTemporarydexterity;
        set
        {
            _dexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _dexterity = 0;
    [Export] public int Dexterity
    {
        get => _dexterity;
        set
        {
            _dexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _strength = 0;
    [Export] public int Strength
    {
        get => _strength;
        set
        {
            _strength = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	public bool IsWeak { get; set; } = false;
    public bool IsFrail { get; set; } = false;
	[Export] public AnimatedSprite2D animation;
	[Export] private PackedScene _damageLabelScene;

	[Signal]
	public delegate void AttackImpactEventHandler();

	private bool isAlive;
	

		public override void _Ready()
	{
		ApplyCharacterData();
		setupBasicDeck();
		PlayerManager.Instance.Player = this;
		SetupHpBar();
		UpdateLabelValues();
	}

	private void ApplyCharacterData()
	{
		var character = RunData.SelectedCharacter;
		if (character is not null)
		{
			_maxHp        = character.StartingMaxHp;
			_currentHp    = character.StartingMaxHp;
			Gold          = character.StartingGold;
		}
		else
		{
			_maxHp     = _startingMaxHp;
			_currentHp = _startingMaxHp;
			Gold       = _startingGold;
		}
	}

private void SetupHpBar()
{
    if (_hpBar == null) return;
    
    _hpBar.Setup(MaxHp, currentHp);
}	
	public override void _Process(double delta)
	{
	}
		public void TriggerRelics(Action<RelicData> hook)
	{
		foreach (var relic in Relics)
			hook(relic);
	}
	public void AddRelic(RelicData relic)
{
    Relics.Add(relic);
    RelicBar.Instance?.LoadRelics(Relics);
}
	public void CalculateDamageTaken(int enemyDamage)
	{	
		TriggerRelics(r => r.OnTakeDamage(this, ref enemyDamage)); 

		int damageToHp = Math.Max(0, enemyDamage - BlockValue);		
		
		BlockValue = Math.Max(0, BlockValue - enemyDamage);		
		currentHp -= damageToHp;
		if(currentHp <= 0)
		{
			isAlive = false;
			return;
		}
		UpdateLabelValues();
		SpawnDamageLabel(damageToHp);

	}
	public void TakeDamage(int damage)
{
    currentHp -= damage;
    
    if (currentHp <= 0)
    {
        currentHp = 0;
        isAlive = false;
    }
    
    SpawnDamageLabel(damage);
    UpdateLabelValues();
}

private void SpawnDamageLabel(int damage)
{
    if (_damageLabelScene == null) return;
    
    var label = _damageLabelScene.Instantiate<DamageLabel>();
    AddChild(label); // ← filho do Player
    label.Visible = true;
    label.ZIndex = 100;
    label.Position = new Vector2(0, -50); 
    label.Setup(damage);
}
	public void UpdateLabelValues()
{
    
    _hpBar?.Setup(MaxHp, currentHp);
    _hpBar?.updateLabels(currentHp, MaxHp, BlockValue);
}
	public void UpdateTemporaryValues()
	{
		_temporaryStrength = 0;
		_TemporaryDexterity = 0;
		BlockValue = 0;

	}
	public void UpdatePerCombatTemporaryValues()
	{
		_perCombatTemporarydexterity = 0;
		_perCombatTemporaryStrength = 0;
	}
	  public void ReceiveGold(int amount)
    {
        Gold += amount;
    }
	  public bool SpendGold(int amount)
    {
        if(Gold < amount) return false;
        Gold -= amount;
        return true;
    }
	public List<CardData> GetDeck() => _BaseDeck;
	public void AddCardToDeck(CardData cardData)
	{
		_BaseDeck.Add(cardData);
		GameManager.Instance?.UpdateTopBar();
	}
	public CardData UpgradeRandomCard()
{
    var nonUpgraded = _BaseDeck.FindAll(cardData => !cardData.IsCardUpgraded);
    
    if (nonUpgraded.Count == 0) return null;
    
    int random = (int)GD.RandRange(0, nonUpgraded.Count - 1);
	CardData cardData = nonUpgraded[random]; 
	cardData.IsCardUpgraded = true;
    return cardData;
}
public CardData RemoveRandomCard()
{
    if (_BaseDeck.Count == 0) return null;
    
    int random = GD.RandRange(0, _BaseDeck.Count - 1);
    CardData cardData = _BaseDeck[random];
    _BaseDeck.RemoveAt(random); 
    return cardData;
}
	public void setupBasicDeck()
	{
		CardData strikeData = GD.Load<CardData>("res://Data/Cards/StrikeCard.tres");
        CardData defendData = GD.Load<CardData>("res://Data/Cards/BlockCard.tres");
        CardData blockVunarable = GD.Load<CardData>("res://Data/Cards/BlockVunarable.tres");
		for(int i = 0; i < 5; i++)
		{
			AddCardToDeck((CardData)strikeData.Duplicate());
			AddCardToDeck((CardData)defendData.Duplicate());
			AddCardToDeck((CardData)blockVunarable.Duplicate());
		}
	}
	public void TryToHeal(int healValue)
	{
		if(this._currentHp == MaxHp) 
		return;

		if(this._currentHp + healValue >= MaxHp)
		{
			currentHp = MaxHp;
			return;
		}
		this.currentHp += healValue;
	}


public async void PlayAttackAnimation()
{
    if (_isAttacking) return;
    _isAttacking = true;
    
    animation.Play("attack");
    
    animation.FrameChanged += OnAttackFrameChanged;
    
    await ToSignal(animation, AnimatedSprite2D.SignalName.AnimationFinished);
    animation.FrameChanged -= OnAttackFrameChanged;
    
    animation.Play("idle");
    _isAttacking = false;
}

private void OnAttackFrameChanged()
{
    if (animation.Animation == "attack" && animation.Frame == ImpactFrame)
    {
        EmitSignal(SignalName.AttackImpact);
    }
}
}
