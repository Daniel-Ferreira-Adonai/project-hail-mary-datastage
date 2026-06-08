using Godot;

// Dano final = 10 + 2×Força.
// CombatManager.Calculate já soma 1×Força; GetSpecialModifierValue soma +1×Força extra.
[GlobalClass]
public partial class FacaoDoFeiranteCard : CardData
{
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
        if (aux is Player player)
            return player.GetTotalStrength();
        return 0;
    }

    public override void UpgradeCard()
    {
        this.Damage += 3;
    }
}
