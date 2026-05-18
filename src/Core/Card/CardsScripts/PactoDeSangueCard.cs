using Godot;
using System;

[GlobalClass]
public partial class PactoDeSangueCard : CardData
{
   public override void Execute(object target = null, object aux = null)
{
    if (target is Player player)
    {
        player.TakeDamage(this.EffectValue);
        
        int energyGain = IsCardUpgraded ? 2 : 1;
        CombatManager.Instance.currentEnergy += energyGain;
        CombatManager.Instance.UpdateEnergy(CombatManager.Instance.currentEnergy);
        CombatManager.Instance._cardManager.DrawCard(1);

        player.UpdateLabelValues();
    }
}

    public override void UpgradeCard()
	{
		SecondaryEffectValue += 1;
	}
}