using Godot;
using System;
using System.Data;

public partial class Player : Node2D
{
	[Export]public int MaxHp;

	[Export] public int currentHp;

	[Export] public int BlockValue;

	[Export] Label healthLabel;

	private bool isAlive;
	

	public override void _Ready()
	{
		UpdateCurrentHpLabel(currentHp);
	}

	public override void _Process(double delta)
	{
		
	}
	public void CalculateDamageTaken(int enemyDamage)
	{
		int hpValueAux = currentHp -= enemyDamage;
		if(hpValueAux < 0)
		{
			isAlive = false;
			// HandleDeath(); provavelmente vai chamar gameManager para zerar tudo e Menu e zaz
			return;
		}
		UpdateCurrentHpLabel(hpValueAux);
		currentHp = hpValueAux;
	}
	public void UpdateCurrentHpLabel(int currentHealth)
	{
		healthLabel.Text = currentHealth.ToString();
	}
}
