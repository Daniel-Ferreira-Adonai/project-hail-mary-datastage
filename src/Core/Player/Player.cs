using Godot;
using System;
using System.Data;

public partial class Player : Node2D
{
	[Export]public int MaxHp;

	[Export] public int currentHp;

	[Export] public int BlockValue;

	[Export] Label healthLabel;

	[Export] Label BlockLabel;

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
		UpdateLabelValues();
	}

	public override void _Process(double delta)
	{
		
	}
	public void CalculateDamageTaken(int enemyDamage)
	{	
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
}
