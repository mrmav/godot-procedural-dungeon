using Godot;
using System;

public class Cursor : Sprite
{
    
    [Export]
    public Texture CursorTexture = null;

    [Export]
    public Vector2 CursorOffset = Vector2.Zero;
    
    [Export]
    public float CursorExtend = 0f;

    [Export]
    public NodePath PlayerPath;

    private Player _player;

    public override void _Ready()
    {
        Texture = CursorTexture;
        ZIndex = 99;

        _player = GetNode<Player>(PlayerPath);

        Input.SetMouseMode(Input.MouseMode.Hidden);
    }

    public override void _Process(float delta)
    {
        
        if(_player.Control == Player.ControlType.Keyboard)
        {
            GlobalPosition = GetGlobalMousePosition() + CursorOffset;

        } else if(_player.Control == Player.ControlType.Controller)
        {
            GlobalPosition =  _player.GlobalPosition + _player.LastRightAxis * CursorExtend + CursorOffset;
        }

        Visible = !_player.IsDashing;

        base._Process(delta);
    }
}
