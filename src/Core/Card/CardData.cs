using Godot;
using System;

public partial class CardData : Resource
{
	public enum CardType{Attack, Skill, SkillWithEnemyEffect, Power}
	[Export] public string CardName { get; set; }
    [Export] public int EnergyCost { get; set; }
    [Export] public int Damage { get; set; }
    [Export] public int Block { get; set; }
    [Export] public int EffectValue { get; set; }

    [Export] public int SecondaryEffectValue { get; set; }


    [Export] public int VunarableValue { get; set; }

    [Export] public int WeakValue { get; set; }

    [Export] public string Description { get; set; }
    
    [Export] public Texture2D Art { get; set; }

    [Export] public CardType tipoCarta {get; set;}

    [Export] public bool IsCardUpgraded {get; set;}
    [Export] public bool IsExhausted  {get; set;}
    [Export] public bool IsAoe  {get; set;}


    public virtual void Execute(object target = null, object aux = null) { }

public virtual int GetSpecialModifierValue(object target = null, object aux = null)
{
    return 0;
}
    public virtual void UpgradeCard()
    {
        
    }

}
