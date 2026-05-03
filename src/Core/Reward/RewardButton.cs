using Godot;
using System;
using System.Linq;

public partial class RewardButton : Button
{

	[Export] public RewardType RewardType;

	public int goldReward = 0;

	public override void _Ready()
	{
    	MouseFilter = MouseFilterEnum.Pass;

		
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
}