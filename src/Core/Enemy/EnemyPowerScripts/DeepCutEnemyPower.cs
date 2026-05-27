using Godot;

[GlobalClass]
public partial class DeepCutEnemyPower : EnemyPowerData
{
    public override void OnDamageTaken(Enemy enemy, int damage)
    {
		    GD.Print("DeepCut ativado!");

        var cardManager = CombatManager.Instance._cardManager;
        if (cardManager == null) return;

        var hand = cardManager._handList;
        if (hand.Count == 0) return;

        int randomIndex = (int)GD.RandRange(0, hand.Count - 1);
        Card cardToDiscard = hand[randomIndex];
        cardManager.handleCardDeckTurn(cardToDiscard);
    }
}