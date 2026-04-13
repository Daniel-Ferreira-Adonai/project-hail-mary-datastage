using Godot;
using System;

[GlobalClass]
public partial class StrikeCard : CardData
{
	public override void ExecuteEnemy(Enemy target = null)
	{
		if (target is Enemy enemy)
		 	enemy.TakeDamage(Damage);
	}
  }
