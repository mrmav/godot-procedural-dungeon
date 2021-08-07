using Godot;
using System;

public class CameraFocusPoint : Position2D
{
    
    [Export]
    public NodePath NodeToFollow = null;

    private Player _node;

    private Camera2D _camera;

    public override void _Ready()
    {
        _node = GetNode<Player>(NodeToFollow);
        _camera = GetNode<Camera2D>("Camera2D");
        
        bool smooth = _camera.SmoothingEnabled;
        _camera.SmoothingEnabled = false;
        GlobalPosition = _node.CameraFocusPoint;
        _camera.SmoothingEnabled = smooth;
    }

    public override void _Process(float delta)
    {
        GlobalPosition = _node.CameraFocusPoint;
    }
    
}
