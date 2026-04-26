using Godot;
using System;


public partial class UI : CanvasLayer
{
    public static UI Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddUI(Control control)
    {
        AddChild(control);
    }
}
