using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class RewardCard : PanelContainer
{
	int goldRewardCount {get; set;} = 2;

	int CardRewardCount {get;set;} = 1;

	List<RewardButton> rewardButtons = [];

	[Export] PackedScene _rewardButtonScene;
	[Export] VBoxContainer ButtonContainers;
	[Export] PackedScene _pickCardRewardsScene;
	public List<CardData> PendingCards { get; set; } = new();

	public override void _Ready()
	{
		setupRewards();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void setupRewards()
	{
			for(int i = 0; i < goldRewardCount; i++)
		{
			var goldReward = _rewardButtonScene.Instantiate<RewardButton>();
			goldReward.RewardType = RewardType.Gold;
			ButtonContainers.AddChild(goldReward);
			rewardButtons.Add(goldReward);

		}
		for(int i = 0; i < CardRewardCount; i++)
		{
			var cardReward = _rewardButtonScene.Instantiate<RewardButton>();
			cardReward.RewardType = RewardType.Card;
			ButtonContainers.AddChild(cardReward);
			rewardButtons.Add(cardReward);

		}
	}
	public void RemoveReward(RewardButton button)
	{
		rewardButtons.Remove(button);
		
		if(rewardButtons.Count == 0)
		{
			GameManager.Instance.OnCombatVictory();
			QueueFree();
		}
	}
	public void OpenCardPicker(RewardButton rewardButton)
{
    var picker = _pickCardRewardsScene.Instantiate<PickCardRewards>();
	picker.CardsToDisplay = PendingCards; 

    UI.Instance.AddUI(picker);
    
    Visible = false;
    
    picker.AnimationFinished += () =>
    {
        rewardButton.QueueFree();
        RemoveReward(rewardButton);
        Visible = true;
    };
	 picker.BackPressed += () =>
    {
        Visible = true; 
    };
}
}
