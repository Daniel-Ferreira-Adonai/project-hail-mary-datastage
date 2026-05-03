using Godot;
using System;

public partial class TopHud : VBoxContainer
{
	[Export] TopBar _topBar;
	[Export] RelicBar _relicBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void UpdateTopBar(Map _map)
    {
        var player = PlayerManager.Instance.Player;
        if (player == null || _topBar == null) return;

        _topBar.UpdateHP(player.currentHp, player.MaxHp);
        _topBar.UpdateGold(player.Gold);
        _topBar.UpdateFloor(_map._floorsClimbed);
        _relicBar.LoadRelics(player.Relics);
    }
}
