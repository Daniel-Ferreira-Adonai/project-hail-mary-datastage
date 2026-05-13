using Godot;
using System;

[GlobalClass]
public partial class AttackIntent : IntentData
{
	public override void Execute(Enemy enemy, Player player)
	{
		int EnemyDamage = enemy.Strength + this.Value;
		player.CalculateDamageTaken(EnemyDamage);
	}
}
