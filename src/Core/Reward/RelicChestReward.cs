using Godot;
using System;

public partial class RelicChestReward : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var relic = GetNode<RelicForShopOrChest>("RelicForShopOrChest");
		relic.RelicCollected += async (relicData) =>
		{
    		await UI.Instance.FadeOut();

			QueueFree();
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
