using Godot;
using System;
[GlobalClass]
public partial class ArmaduraDourada : RelicData
{
	private bool _UsedThisCombat = false;

    public override void OnCombatStart(Player player)
    {
        _UsedThisCombat = false;
    }
    public override void BeforeCardIsPlayed(Player player, CardData card)
        {
            if (_UsedThisCombat) return;
            if (card.Block == 0) return;

            card.Block = card.Block * 2;
        }
    public override void OnCardPlayed(Player player, CardData card)
     {
         if (_UsedThisCombat) return;
         if (card.Block == 0) return;

        card.Block = card.Block / 2;
         _UsedThisCombat = true;
     }
}
