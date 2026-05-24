using Godot;
using System;
using System.Linq;

public partial class RewardButton : Button
{

	[Export] public RewardType RewardType;
	public Texture2D CurrentIcon;

	[Export] public Texture2D GoldIcon;
	[Export] public Texture2D RelicIcon;
	[Export] public Texture2D CardIcon;
	[Export] public TextureRect RewardIcon;
	public int goldReward = 0;

	public override void _Ready()
	{
    	MouseFilter = MouseFilterEnum.Pass;
		DecideIcon(this.RewardType);
		
	}

public void DecideIcon(RewardType rewardType)
{
    switch (rewardType)
    {
        case RewardType.Gold:
            RewardIcon.Texture = GoldIcon;
            break;

        case RewardType.Relic:
             RewardIcon.Texture  = RelicIcon;
            break;

        case RewardType.Card:
             RewardIcon.Texture  = CardIcon;
            break;

        default:
            RewardIcon.Texture  = default;
            GD.Print("Tipo de recompensa não definido");
            break;
    }
}
	public void _on_reward_button_pressed()
	{
		if(RewardType == RewardType.Card)
		{
			 var card = this.FindParent("RewardCard") as RewardCard;
        	card?.OpenCardPicker(this);
		}
		if(RewardType == RewardType.Gold)
		{
			 PlayerManager.Instance.Player.ReceiveGold(goldReward);
			var card = this.FindParent("RewardCard") as RewardCard;
			QueueFree();
			card?.RemoveReward(this);
		}
	}
	public void setGoldReward()
	{
		 goldReward +=  10;
	}
}

public enum RewardType
{
    Card,
	Gold,     
	Relic,
}