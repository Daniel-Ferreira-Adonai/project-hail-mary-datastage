using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Enemy : Node
{
	[Export]public int health;

	[Export]public Label healthLabel;

	[Export] public int Strength;

	[Export] public Array<EnemyTurn> enemyTurns;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		AddToGroup("enemies");
		healthLabel.Text = health.ToString();
	}

	public Array<IntentData> getTurnIntents()
	{
		return enemyTurns[0].Actions;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void TakeDamage(int damage)
	{
		health -= damage;
		atualizarHp();
	}
	public void atualizarHp()
	{
		healthLabel.Text = health.ToString();
	}
}
