using Godot;
using System;

[GlobalClass]
public partial class CoagulacaoCard : CardData
{
    public override void Execute(object target = null, object aux = null)
{
    if (target is Enemy enemy && aux is Player player)
    {
        var enemies = player.GetTree().GetNodesInGroup("enemies");
        foreach (var node in enemies)
        {
            if (node is Enemy e)
            {
                int totalDamage = CombatManager.Calculate(this.Damage, player, e, this);
                e.TakeDamage(totalDamage);
            }
        }

        player.BlockValue += CombatManager.CalculateBlock(this.Block, player, this);
        player.UpdateLabelValues();
    }
}

    public override int GetSpecialModifierValue(object target = null, object aux = null)
    {
        if (target is Enemy enemy)
            return enemy.GetDebuffValue("Sangria");
        return 0;
    }

    public override void UpgradeCard()
    {
        this.Block += 3;
        this.Damage += 2;
    }
}