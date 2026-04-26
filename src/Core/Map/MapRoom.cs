using Godot;
using System;
using System.Collections.Generic;

public partial class MapRoom : Area2D
{
	[Signal] public delegate void SelectedEventHandler(Room room);

	[Export] private Sprite2D _sprite2D;
	[Export] private Line2D _line2D;
	[Export] private AnimationPlayer _animationPlayer;
	
	private bool _available = false;
public bool Available
{
    get => _available;
    set
    {
        _available = value;
        SetAvailable(value);
    }
}
private void SetAvailable(bool newValue)
{
    _available = newValue;

    if (_available)
        _animationPlayer.Play("highlight");
    else if (!_room.Select)
        _animationPlayer.Play("RESET");
}
private Room _room;
public Room Room
{
    get => _room;
    set
    {
        _room = value;
        SetRoom(value);
    }
}
	private static readonly Dictionary<Room.RoomType, (Texture2D texture, Vector2 scale)> ICONS = new()
{
    { Room.RoomType.NotAssigned, (null,                                                         Vector2.One * 3f) },
    { Room.RoomType.Combat,      (GD.Load<Texture2D>("res://Test/TestImagesSprites/art/tile_0103.png"), Vector2.One * 3f) },
    { Room.RoomType.Chest,       (GD.Load<Texture2D>("res://Test/TestImagesSprites/art/tile_0089.png"), Vector2.One * 3f) },
    { Room.RoomType.CampFire,    (GD.Load<Texture2D>("res://Test/TestImagesSprites/art/player_heart.png"), Vector2.One * 2f) },
    { Room.RoomType.Shop,        (GD.Load<Texture2D>("res://Test/TestImagesSprites/art/gold.png"),         Vector2.One * 2f) },
    { Room.RoomType.Boss,        (GD.Load<Texture2D>("res://Test/TestImagesSprites/art/tile_0106.png"),    Vector2.One * 4f) },
};
	public override async void _Ready()
{
   
}
	private void SetRoom(Room newData)
	{
		_room    = newData;
		Position = _room.Position;

		_line2D.RotationDegrees  = (float)GD.RandRange(0, 360);
		_sprite2D.Texture        = ICONS[_room.EnumRoomType].texture;
		_sprite2D.Scale          = ICONS[_room.EnumRoomType].scale;
	}
	
	public override void _Process(double delta)
	{
	}
	public void ShowSelected()
{
    _line2D.Modulate = Colors.White;
}

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
{
    if (!_available || @event is not InputEventMouseButton mouseEvent)
        return;

    if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
    {
        _room.Select = true;
        _animationPlayer.Play("select");
    }
}
	public void _OnMapRoomSelected()
	{
		EmitSignal(SignalName.Selected, _room);
	}
}

