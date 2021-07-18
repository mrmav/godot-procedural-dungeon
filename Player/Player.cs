using Godot;
using System;

public class Player : KinematicBody2D
{
    public enum ControlType
    {
        Keyboard,
        Controller
    }

    [Export]
    float Speed = 1f;
    [Export]
    float ControllerDeadzone = 0.2f;
    [Export]
    float CameraExtendZone = 4f;

    private Vector2 _velocity = Vector2.Zero;
    private AnimatedSprite _animation;
    private Position2D _weaponHandle;
    private HealthComponent _health;
    private InvulnerabilityComponent _invulnerability;
    private DamageComponent _damage;
    private MeleeWeapon _melee;

    public ControlType Control = ControlType.Keyboard;
    public Vector2 LastRightAxis = Vector2.Zero;

    public Vector2 CameraFocusPoint = Vector2.Zero;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _weaponHandle = GetNode<Position2D>("WeaponHandle");
        _animation = GetNode<AnimatedSprite>("AnimatedSprite");
        _health = GetNode<HealthComponent>("HealthComponent");
        _invulnerability = GetNode<InvulnerabilityComponent>("Invulnerability");
        _damage = GetNode<DamageComponent>("DamageComponent");
        _melee = GetNode("WeaponHandle").GetChild<MeleeWeapon>(0);

        _health.Connect("Died", this, nameof(OnPlayerDead));
        _invulnerability.Connect("InvulnerabilityLifted", this, nameof(OnInvulnerabilityLifted));
        _damage.Connect("DamageTaken", this, nameof(OnDamageTaken));
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

        // make weapon and player rotate around a focus point
        Vector2 focusPoint = Vector2.Zero;

        if(Control == ControlType.Keyboard)
        {
            focusPoint = GetGlobalMousePosition();
            CameraFocusPoint = focusPoint - GlobalPosition;
            LastRightAxis = CameraFocusPoint.Normalized();
            CameraFocusPoint = LastRightAxis * CameraExtendZone + GlobalPosition;
            
        } else if(Control == ControlType.Controller)
        {
            Vector2 lookDir = new Vector2(Input.GetJoyAxis(0, (int)JoystickList.AnalogRx), Input.GetJoyAxis(0, (int)JoystickList.AnalogRy));
            
            if(lookDir.Length() >= ControllerDeadzone)
            {                
                lookDir = lookDir.Normalized();
                LastRightAxis = lookDir;
            }
            
            focusPoint = GlobalPosition + LastRightAxis * CameraExtendZone;
            CameraFocusPoint = focusPoint;

        }

        _weaponHandle.LookAt(focusPoint);
            
        if(focusPoint.x < Position.x)
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
            _melee.Attack();
        }

        if(!_invulnerability.IsVulnerable)
        {
            uint time = OS.GetTicksMsec();
            if(time % 100 < 50)
            {
                Modulate = Colors.Red;
            } else
            {
                Modulate = Colors.White;
            }
        } else
        {
            Modulate = Colors.White;
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

    public override void _Input(InputEvent @event)
    {

        if(@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
        {
            Control = ControlType.Controller;
        } else
        {
            Control = ControlType.Keyboard;
        }

        base._Input(@event);
    }

    public void OnPlayerDead(int health)
    {
        GD.Print($"Player should have died with {health} health.");
    }

    public void OnInvulnerabilityLifted()
    {
        _animation.Modulate = Colors.White;
        //GD.Print($"player is now vulnerable.");
    }
    
    public void OnDamageTaken(int amount, string who)
    {
        if(_invulnerability.IsVulnerable)
        {
            _health.Damage(amount);
            _invulnerability.SetInvulnerable();
            GD.Print($"{who} dealt {amount} of damage to {Name}.");
        }
        
    }

}
