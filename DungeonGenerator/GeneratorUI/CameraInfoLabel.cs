using Godot;
using System;

public class CameraInfoLabel : Label
{    
    private Camera2D _camera;
    private string _instructions = "Drag & Zoom In/Out to explore generated dungeon.";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _camera = GetTree().Root.GetNode("TileLevel").GetNode("Camera2D") as Camera2D;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Text = $"Camera Info:\nPosition: {_camera.Position}\nZoom: {_camera.Zoom.x}\n" + _instructions;
    }
}
