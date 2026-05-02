using Godot;
using System;
using System.Collections.Generic;

public partial class RelicBar : MarginContainer
{
	    [Export] private HBoxContainer _relicContainer;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	  public void AddRelic(RelicData relic)
{
    var icon = new TextureRect();
    icon.Texture = relic.Icon;
    icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
    icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
    icon.CustomMinimumSize = new Vector2(60, 60); 
    _relicContainer.AddChild(icon);
}
	 public void LoadRelics(List<RelicData> relics)
    {
        foreach (var child in _relicContainer.GetChildren())
            child.QueueFree();

        foreach (var relic in relics)
            AddRelic(relic);
    }
}
