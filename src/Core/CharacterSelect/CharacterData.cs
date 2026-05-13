using Godot;

public partial class CharacterData : Resource
{
    [Export] public string CharacterName { get; set; } = "Guerreiro";
    [Export] public string Description { get; set; } = "Um aventureiro pronto para escalar o Datastage.";
    [Export] public Texture2D Portrait { get; set; }
    [Export] public int StartingMaxHp { get; set; } = 75;
    [Export] public int StartingGold { get; set; } = 100;
    [Export] public int StartingEnergy { get; set; } = 3;
    [Export] public bool IsUnlocked { get; set; } = true;
}
