using Godot;
using System;

[GlobalClass]
public partial class MoedaDaSorteRelic : RelicData
{
    private const float Chance = 0.01f;

    public override void OnCardPlayed(Player player, CardData card)
    {
        var rng = new Random();
        if (rng.NextDouble() < Chance)
        {
            CombatManager.Instance.currentEnergy += card.EnergyCost;
            CombatManager.Instance.UpdateEnergy(CombatManager.Instance.currentEnergy);
        }
    }
}