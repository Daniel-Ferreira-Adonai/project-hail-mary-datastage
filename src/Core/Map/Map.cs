
using Godot;
using System;
using System.Linq;
public partial class Map : Node2D
{
    public const int SCROLL_SPEED = 15;

    private static readonly PackedScene MAP_ROOM = GD.Load<PackedScene>("res://src/Core/Map/MapRoom.tscn");
    private static readonly PackedScene MAP_LINE = GD.Load<PackedScene>("res://src/Core/Map/MapLine.tscn");
	
    private MapGenerator _mapGenerator;
    private Node2D _lines;
    private Node2D _rooms;
    private Node2D _visuals;
    private Camera2D _camera2D;

    private Room[][] _mapData;
    private int _floorsClimbed;
    private Room _lastRoom;
    private float _cameraEdgeY;
	public override void _Ready()
{
    _mapGenerator = GetNode<MapGenerator>("MapGenerator");
    _lines        = GetNode<Node2D>("%Lines");
    _rooms        = GetNode<Node2D>("%Rooms");
    _visuals      = GetNode<Node2D>("Visuals");
    _camera2D     = GetNode<Camera2D>("Camera2D");

    _cameraEdgeY = MapGenerator.Y_DIST * (MapGenerator.FLOORS - 1);

    GenerateNewMap();
    UnlockFloor(0);
}

public override void _Input(InputEvent @event)
{
    if (@event.IsActionPressed("scroll_up"))
        _camera2D.Position = new Vector2(_camera2D.Position.X, _camera2D.Position.Y - SCROLL_SPEED);
    else if (@event.IsActionPressed("scroll_down"))
        _camera2D.Position = new Vector2(_camera2D.Position.X, _camera2D.Position.Y + SCROLL_SPEED);

    _camera2D.Position = new Vector2(
        _camera2D.Position.X,
        Mathf.Clamp(_camera2D.Position.Y, -_cameraEdgeY, 0)
    );
}

public void GenerateNewMap()
{
    _floorsClimbed = 0;
    _mapData       = _mapGenerator.GenerateMap();
    CreateMap();
}

private void CreateMap()
{
    foreach (Room[] currentFloor in _mapData)
        foreach (Room room in currentFloor)
            if (room.NextRooms.Length > 0)
                _SpawnRoom(room);

    int middle = Mathf.FloorToInt(MapGenerator.MAP_WIDTH * 0.5f);
    _SpawnRoom(_mapData[MapGenerator.FLOORS - 1][middle]);

    int mapWidthPixels = MapGenerator.X_DIST * (MapGenerator.MAP_WIDTH - 1);
    _visuals.Position = new Vector2(
        (GetViewportRect().Size.X - mapWidthPixels) / 2,
        GetViewportRect().Size.Y / 2
    );
}

public void UnlockFloor(int whichFloor = -1)
{
    if (whichFloor == -1) whichFloor = _floorsClimbed;

    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (mapRoom.Room.Row == whichFloor)
            mapRoom.Available = true;
}

public void UnlockNextRooms()
{
    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (Array.Exists(_lastRoom.NextRooms, r => r == mapRoom.Room))
            mapRoom.Available = true;
}

public void ShowMap()
{
    Show();
    _camera2D.Enabled = true;
    ZIndex = 10; // acima do combate
}

public void HideMap()
{
    Hide();
    _camera2D.Enabled = false;
    ZIndex = 0;
}

private void _SpawnRoom(Room room)
{
    var newMapRoom = MAP_ROOM.Instantiate<MapRoom>();
    _rooms.AddChild(newMapRoom);
    newMapRoom.Room = room;
    newMapRoom.Selected += _OnMapRoomSelected;
    _ConnectLines(room);

    if (room.Select && room.Row < _floorsClimbed)
        newMapRoom.ShowSelected();
}

private void _ConnectLines(Room room)
{
    if (room.NextRooms.Length == 0)
        return;

    foreach (Room next in room.NextRooms)
    {
        var newMapLine = MAP_LINE.Instantiate<Line2D>();
        newMapLine.AddPoint(room.Position);
        newMapLine.AddPoint(next.Position);
        _lines.AddChild(newMapLine);
    }
}

private void _OnMapRoomSelected(Room room)
{
    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (mapRoom.Room.Row == room.Row)
            mapRoom.Available = false;

    _lastRoom = room;
    _floorsClimbed += 1;
    GameManager.Instance.OnMapRoomSelected(room);
}
}
