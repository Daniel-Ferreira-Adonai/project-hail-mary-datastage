using Godot;

[GlobalClass]
public partial class MangaMaduraCard : CardData
{
    private static int _combatPlayCount = 0;

    public static void ResetCombat() => _combatPlayCount = 0;

    public override void Execute(object target = null, object aux = null)
    {
        if (target is Player player)
        {
            player.BlockValue += CombatManager.CalculateBlock(this.Block, player, this);
            player.UpdateLabelValues();

            _combatPlayCount++;
            if (_combatPlayCount >= 3)
            {
                _combatPlayCount = 0;
                player.AddPower(new MangaMaduraBurstPower());
            }
        }
    }

    public override void UpgradeCard()
    {
        this.Block += 3;
    }
}
