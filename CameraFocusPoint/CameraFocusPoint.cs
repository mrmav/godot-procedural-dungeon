using Godot;
using System;

public class CameraFocusPoint : Position2D
{
    
    [Export]
    public NodePath NodeToFollow = null;

    private Player _node;

    public override void _Ready()
    {
        _node = GetNode<Player>(NodeToFollow);
        GlobalPosition = _node.CameraFocusPoint;
    }

    public override void _Process(float delta)
    {
        GlobalPosition = _node.CameraFocusPoint;
    }
    
}
