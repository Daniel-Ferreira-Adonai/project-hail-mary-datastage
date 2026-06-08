using Godot;

[GlobalClass]
public partial class MangaRosaCard : CardData
{
    public override void Execute(object target = null, object aux = null)
    {
        if (target is Player player)
        {
            player.BlockValue += CombatManager.CalculateBlock(this.Block, player, this);
            player.PerCombatTemporaryStrength += this.EffectValue;
            player.UpdateLabelValues();
        }
    }

    public override void UpgradeCard()
    {
        this.Block += 2;
        this.EffectValue += 1;
    }
}
