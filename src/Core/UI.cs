using Godot;
using System;


public partial class UI : CanvasLayer
{
    public static UI Instance { get; private set; }
    public TopHud TopHud { get; private set; }

    public override void _Ready()
    {
        Instance = this;
         Layer = 10;
        FollowViewportEnabled = false; 
        TopHud = GetNodeOrNull<TopHud>("TopHud"); 

    }

    public void AddUI(Control control)
    {
        AddChild(control);
    }
}
