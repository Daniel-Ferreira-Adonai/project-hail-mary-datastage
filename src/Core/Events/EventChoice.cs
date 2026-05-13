using Godot;
using System;

[GlobalClass]

public partial class EventChoice : Resource
{
    [Export] public string ChoiceText { get; set; }
    [Export] public string ResultText { get; set; }
    [Export] public EventEffect[] Effects { get; set; }
}