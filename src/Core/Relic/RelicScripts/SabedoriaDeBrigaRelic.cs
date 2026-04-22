using Godot;
using System;

public partial class SabedoriaDeBrigaRelic : RelicData
{
	private bool _usedThisTurn = false;

    public override void OnTurnStart(Player player)
    {
        _usedThisTurn = false;
    }

    // public override void OnCardPlayed(Player player, CardData card, ref int damage)
    // {
    //     if (_usedThisTurn) return;
    //     if (card.Type != CardData.CardType.Attack) return;

    //     damage += 3;
    //     _usedThisTurn = true;
    // }
}
