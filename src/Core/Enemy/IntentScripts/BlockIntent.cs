using Godot;
using System;


[GlobalClass]
public partial class BlockIntent : IntentData
{
    public override void Execute(Enemy enemy, Player player)
    {
        enemy.Block += this.Value;
    }
}