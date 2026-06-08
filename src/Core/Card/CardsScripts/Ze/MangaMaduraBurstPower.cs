using Godot;

// Poder temporário criado pela MangaMaduraCard na 3ª jogada de combate.
// Na virada do próximo turno adiciona +10 de TemporaryStrength e se remove.
[GlobalClass]
public partial class MangaMaduraBurstPower : PowerData
{
    public MangaMaduraBurstPower() { Id = "manga_madura_burst"; }

    public override void OnTurnStart(Player player)
    {
        player.TemporaryStrength += 10;
        player.UpdateLabelValues();
        player.ActivePowers.Remove(this);
    }
}
