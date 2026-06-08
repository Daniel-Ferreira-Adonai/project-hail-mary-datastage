using Godot;

[GlobalClass]
public partial class SaladaDeFrutaCard : CardData
{
    public override void Execute(object target = null, object aux = null)
    {
        if (target is Player player)
        {
            player.BlockValue += CombatManager.CalculateBlock(this.Block, player, this);
            player.TryToHeal(this.EffectValue);
            player.UpdateLabelValues();
        }
    }

    public override void UpgradeCard()
    {
        this.Block += 2;
        this.EffectValue += 2;
    }
}
