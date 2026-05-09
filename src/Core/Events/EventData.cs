using Godot;
using System;

[GlobalClass]
public partial class EventData : Resource
{
    [Export] public string EventName { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Texture2D Art { get; set; }
    [Export] public EventChoice[] Choices { get; set; }

}
