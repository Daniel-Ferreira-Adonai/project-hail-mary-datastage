using Godot;
using System;
[GlobalClass]
public partial class SabedoriaDeBrigaRelic : RelicData
{
	private bool _usedThisTurn = false;

    public override void OnTurnStart(Player player)
    {
        _usedThisTurn = false;
    }
    public override void BeforeOnCombatStart(Player player)
    {
        _usedThisTurn = false;
    }

    public override void BeforeCardIsPlayed(Player player, CardData card)
        {
            if (_usedThisTurn) return;
            if (card.tipoCarta != CardData.CardType.Attack) return;

        }
    public override void OnCardPlayed(Player player, CardData card)
     {
         if (_usedThisTurn) return;
         if (card.tipoCarta != CardData.CardType.Attack || card.Damage <= 0) return;

         _usedThisTurn = true;
     }

     public override int GetDamageBonus(Player player, CardData card)
{
    if (_usedThisTurn) return 0;
    if (card.tipoCarta != CardData.CardType.Attack) return 0;
    return 3;
}
}
