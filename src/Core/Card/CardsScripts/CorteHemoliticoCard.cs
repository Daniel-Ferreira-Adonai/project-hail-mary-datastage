using Godot;
using System;

[GlobalClass]

public partial class CorteHemoliticoCard : CardData
{
	// Called when the node enters the scene tree for the first time.
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Enemy enemy && aux is Player player)
		{
			
        int damage = CombatManager.Calculate(this.Damage, player, enemy, this);
        enemy.TakeDamage(damage);
		}
	}
	public override int GetSpecialModifierValue(object target = null, object aux = null)
{
    if (target is Enemy enemy && aux is Player player)
		{
		return enemy.GetDebuffValue("Sangria") * 2;
		}
	return 0;
}
    public override void UpgradeCard()
    {
        this.Damage += 2;
		this.EffectValue += 1;
    }
}
