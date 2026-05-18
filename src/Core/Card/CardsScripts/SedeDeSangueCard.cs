using Godot;
using System;

[GlobalClass]
public partial class SedeDeSangueCard : PowerData
{
    public override void Execute(object target = null, object aux = null)
{
    if (target is Player player)
    {
        GD.Print("SedeDeSangue executada, adicionando poder");
        GD.Print(player.ActivePowers.Count);
        GD.Print(player.ActivePowers.Count);
    }
}

    public override void OnDebuffApplied(Player player, string debuff, int value, Enemy enemy)
{
    if (debuff == "Sangria")
    {
        player.TemporaryStrength += value * this.EffectValue;
        player.UpdateLabelValues();
    }
}

    public override void UpgradeCard()
    {
        // upgraded: ganha 2 de força em vez de 1
    }
}