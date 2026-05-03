using Godot;
using System;
using System.Collections.Generic;

public partial class RelicForShopOrChest : TextureButton
{
    public enum RelicContext { Chest, Shop }

    RelicData relicData;

    [Export] public RelicContext Context { get; set; } = RelicContext.Chest;
    [Export] public int Price { get; set; } = 100;
    [Export] public bool StartsDisabled { get; set; } = true;
    [Export] public bool StartsInvisible { get; set; } = true;

    [Signal]
    public delegate void RelicCollectedEventHandler(RelicData relic);

    public override void _Ready()
    {
        relicData = GetRandomRelic();
        TextureNormal = relicData.Icon;
        IgnoreTextureSize = true;
        StretchMode = StretchModeEnum.Scale;
        CustomMinimumSize = new Vector2(62, 62);

        if (StartsDisabled)
        {
            Disabled = true;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        if (StartsInvisible)
            Modulate = new Color(1, 1, 1, 0);

    }

    

    public void Enable()
    {
        Disabled = false;
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Process(double delta)
    {
    }

    private RelicData GetRandomRelic()
    {
        var allRelics = new List<RelicData>();

        var files = DirAccess.GetFilesAt("res://Data/Relics/");
        foreach (var file in files)
        {
            if (file.EndsWith(".tres"))
            {
                var relic = GD.Load<RelicData>($"res://Data/Relics/{file}");
                if (relic != null)
                    allRelics.Add(relic);
            }
        }

        if (allRelics.Count == 0) return null;

        var rng = new Random();
        return allRelics[rng.Next(allRelics.Count)];
    }
    public void _on_pressed()
    {
        if (Context == RelicContext.Shop)
        {
            // A loja gerencia ouro e AddRelic via sinal
            EmitSignal(SignalName.RelicCollected, relicData);
            return;
        }

        // Baú: comportamento original
        PlayerManager.Instance.Player.AddRelic(relicData);
        GameManager.Instance.ShowMapFade();
        EmitSignal(SignalName.RelicCollected, relicData);
    }
}