using Godot;
using System;

[GlobalClass]
public partial class EspinhosRelic : RelicData
{
private bool _triggeredThisTurn = false;

public override void OnTurnStart(Player player)
{
    _triggeredThisTurn = false;
}

public override void OnTakeDamage(Player player, ref int damage)
{
    if (_triggeredThisTurn) return;
    _triggeredThisTurn = true;

    var enemies = player.GetTree().GetNodesInGroup("enemies");
    foreach (var node in enemies)
    {
        if (node is Enemy enemy)
        {
            var intents = enemy.GetNextTurnIntents();
            foreach (var intent in intents)
            {
                if (intent.Type == IntentData.IntentType.Attack)
                {
                    enemy.TakeDamage(3);
                    break;
                }
            }
        }
    }
}
}