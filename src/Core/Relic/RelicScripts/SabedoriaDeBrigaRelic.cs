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
    public override void BeforeCardIsPlayed(Player player, CardData card)
        {
            if (_usedThisTurn) return;
            if (card.tipoCarta != CardData.CardType.Attack) return;

            card.Damage += 3;
        }
    public override void OnCardPlayed(Player player, CardData card)
     {
         if (_usedThisTurn) return;
         if (card.tipoCarta != CardData.CardType.Attack) return;

         card.Damage -= 3;
         _usedThisTurn = true;
     }
}
