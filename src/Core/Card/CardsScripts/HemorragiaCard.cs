using Godot;
using System;

[GlobalClass]
public partial class HemorragiaCard : CardData
{
    public override void Execute(object target = null, object aux = null)
    {
        if (target is Enemy enemy && aux is Player player)
        {
            int currentSangria = enemy.GetDebuffValue("Sangria");
            enemy.ApplyDebuff("Sangria", currentSangria); // dobra
            player.TakeDamage(this.EffectValue);
            this.IsExhausted = true; 
        }
    }

    public override void UpgradeCard()
    {
        this.EffectValue -= 1; 
    }
}