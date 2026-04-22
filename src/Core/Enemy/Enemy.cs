using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Enemy : Node
{
	[Export]public int health;

	[Export]public Label healthLabel;

	[Export] public int Strength;
        
	[Export] public int BuffedStrength;
	
	[Export] public int Vulnerable { get; set; } = 0;

	[Export] public int Weak { get; set; } = 0;


	[Export] public Array<EnemyTurn> enemyTurns;
	public override void _Ready()
	{	
		AddToGroup("enemies");
		healthLabel.Text = health.ToString();
	}

	public Array<IntentData> getTurnIntents()
	{
		return enemyTurns[0].Actions;
	}
	public override void _Process(double delta)
	{
	}
	public void TakeDamage(int damage)
	{
		health -= damage;
		atualizarHp();
	}
	public void die()
	{
		this.QueueFree();
	}
	public void atualizarHp()
	{
		healthLabel.Text = health <= 0 ? "0" :health.ToString() ;
		if(health <= 0 ) 
		die();
	}
}
