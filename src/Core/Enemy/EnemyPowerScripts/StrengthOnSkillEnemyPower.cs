using Godot;
using System;

[GlobalClass]
public partial class StrengthOnSkillEnemyPower : EnemyPowerData
{
    public override void OnPlayerCardPlayed(Enemy enemy, CardData card)
    {
        if (card.tipoCarta == CardData.CardType.Skill || 
            card.tipoCarta == CardData.CardType.SkillWithEnemyEffect ||
            card.tipoCarta == CardData.CardType.Power)
        {
            enemy.BuffedStrength += EffectValue;
        }
    }
}