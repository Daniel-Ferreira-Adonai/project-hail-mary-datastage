using Godot;
using System;


[GlobalClass]
public partial class HealIntent : IntentData
{
    public override void Execute(Enemy enemy, Player player)
    {
        enemy.CurrentHealth = Mathf.Min(enemy.CurrentHealth + this.Value, enemy.MaxHealth);
    }
}