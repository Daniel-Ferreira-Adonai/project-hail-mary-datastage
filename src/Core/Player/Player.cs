using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class Player : Node2D
{
	
	private int _maxHp;
	[Export] public int MaxHp
	{
		get => _maxHp;
		set
		{
			_maxHp = value;
			GameManager.Instance?.UpdateTopBar();
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
		}
	}

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
	[Export] public int BlockValue;

	[Export] Label healthLabel;

	[Export] Label BlockLabel;
	private List<CardData> _BaseDeck = new List<CardData>();  

	public List<RelicData> Relics { get; set; } = new();
	public int BonusCardsToDraw { get; set; } = 0;
	public bool NextDebuffDoubled { get; set; } = false;


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
	private bool isAlive;
	

		public override void _Ready()
	{
		setupBasicDeck();
		PlayerManager.Instance.Player = this;
		UpdateLabelValues();

		 var relicFiles = DirAccess.GetFilesAt("res://Data/Relics/");
    foreach (var file in relicFiles)
    {
        if (file.EndsWith(".tres"))
        {
            var relic = GD.Load<RelicData>($"res://Data/Relics/{file}");
            if (relic != null)
                Relics.Add(relic);
        }
    }
	}

	public override void _Process(double delta)
	{
	}
		public void TriggerRelics(Action<RelicData> hook)
	{
		foreach (var relic in Relics)
			hook(relic);
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
	}
	public void UpdateLabelValues()
	{
		healthLabel.Text = currentHp.ToString();
		BlockLabel.Text = BlockValue.ToString();
	}
	public void UpdateTemporaryValues()
	{
		_temporaryStrength = 0;
		_TemporaryDexterity = 0;
		BlockValue = 0;

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
	}
	public void setupBasicDeck()
	{
		CardData strikeData = GD.Load<CardData>("res://Data/Cards/StrikeCard.tres");
        CardData defendData = GD.Load<CardData>("res://Data/Cards/BlockCard.tres");
        CardData blockVunarable = GD.Load<CardData>("res://Data/Cards/BlockVunarable.tres");
		for(int i = 0; i < 5; i++)
		{
			AddCardToDeck(strikeData);
			AddCardToDeck(defendData);
			AddCardToDeck(blockVunarable);
		}
	}
}
