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
    private Position2D _weaponHandle;
    private HealthComponent _health;
    private InvulnerabilityComponent _invulnerability;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _weaponHandle = GetNode<Position2D>("WeaponHandle");
        _animation = GetNode<AnimatedSprite>("AnimatedSprite");
        _health = GetNode<HealthComponent>("HealthComponent");
        _invulnerability = GetNode<InvulnerabilityComponent>("Invulnerability");

        _health.Connect("Died", this, nameof(OnPlayerDead));
        _invulnerability.Connect("InvulnerabilityLifted", this, nameof(OnInvulnerabilityLifted));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(_velocity.Length() != 0)
        {
            _animation.Play("walk");            
            
        } else
        {
            _animation.Play("idle");            
        }

        Vector2 mouse = GetGlobalMousePosition();
        _weaponHandle.LookAt(mouse);
        
        if(mouse.x < Position.x)
        {
            _weaponHandle.Scale = new Vector2(1, -1);
            _animation.FlipH = true;
        } else
        {
            _weaponHandle.Scale = new Vector2(1, 1);
            _animation.FlipH = false;
        }
        
        if(Input.IsActionJustPressed("player_attack"))
        {
            AnimationPlayer anim = _weaponHandle.GetNode<AnimationPlayer>("WeaponRoot/AnimationPlayer");
            anim.Stop();
            anim.Play("swing");
            
            // test components
            _health.Damage(5);
            _invulnerability.SetInvulnerable();
            _animation.Modulate = Colors.Red;


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

    public void OnPlayerDead(int health)
    {
        GD.Print($"player died with {health} health.");
    }

    public void OnInvulnerabilityLifted()
    {
        _animation.Modulate = Colors.White;
        GD.Print($"player is now vulnerable.");
    }


}
