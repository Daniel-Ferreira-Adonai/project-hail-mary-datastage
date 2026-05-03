using Godot;
using System;

public partial class PlayerManager : Node
{
    public static PlayerManager Instance { get; private set; }
    public Player Player { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }
}