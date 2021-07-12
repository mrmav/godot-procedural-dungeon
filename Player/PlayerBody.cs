using Godot;
using System;

public class PlayerBody : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export]
    float Speed = 1f;

    private Vector2 _velocity = Vector2.Zero;
    private AnimatedSprite _animation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animation = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(_velocity.Length() != 0)
        {
            _animation.Play("walk");
            _animation.FlipH = _velocity.x < 0;
        } else
        {
            _animation.Play("idle");            
        }        
        
    }

    public override void _PhysicsProcess(float delta)
    {

        Vector2 direction = Vector2.Zero;

        direction.y -= Input.GetActionStrength("player_move_up");
        direction.y += Input.GetActionStrength("player_move_down");

        direction.x -= Input.GetActionStrength("player_move_left");
        direction.x += Input.GetActionStrength("player_move_right");

        direction = direction.Normalized();

        _velocity = direction * Speed;

        _velocity = MoveAndSlide(_velocity);

        base._PhysicsProcess(delta);
    }


}
