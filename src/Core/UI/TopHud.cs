using Godot;

public partial class TopHud : VBoxContainer
{
    [Export] TopBar _topBar;
    [Export] RelicBar _relicBar;

    public override void _Ready() { }

    public void UpdateTopBar(Map map)
    {
        var player = PlayerManager.Instance?.Player;
        if (!IsInstanceValid(player) || !IsInstanceValid(_topBar) || !IsInstanceValid(_relicBar)) return;

        _topBar.UpdateHP(player.currentHp, player.MaxHp);
        _topBar.UpdateGold(player.Gold);
        _topBar.UpdateFloor(map._floorsClimbed);
        _topBar.UpdateDeckCount(player.GetDeck().Count);
        _relicBar.LoadRelics(player.Relics);
    }
}
