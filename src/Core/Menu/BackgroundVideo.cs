using Godot;
using System;

public partial class BackgroundVideo : VideoStreamPlayer
{
    [Export] public Godot.Collections.Array<Resource> Videos = new();

    public override void _Ready()
    {
        if (Videos.Count == 0) return;

        Stream = Videos[new Random().Next(Videos.Count)] as VideoStream;
        Play();
    }
}