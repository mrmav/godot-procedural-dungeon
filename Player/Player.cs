using Godot;
using System;

public class Player : BaseMob
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
    private KnockbackComponent _knockback;
    private DashComponent _dash;
    private FlashComponent _flash;
    private HitstopComponent _hitstop;
    private SpringSystem _hoodie;
    private SpringSystem _cape;

    private AudioStreamPlayer _hitsfx;

    public ControlType Control = ControlType.Keyboard;
    public Vector2 LastRightAxis = Vector2.Zero;

    public Vector2 CameraFocusPoint;

    private Vector2 _lastDirection;

    public bool IsDashing
    {
        get
        {
            return _dash.IsDashing;
        }
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _weaponHandle = GetNode<Position2D>("WeaponHandle");
        _animation = GetNode<AnimatedSprite>("AnimatedSprite");
        _health = GetNode<HealthComponent>("HealthComponent");
        _invulnerability = GetNode<InvulnerabilityComponent>("Invulnerability");
        _damage = GetNode<DamageComponent>("DamageComponent");
        _melee = GetNode("WeaponHandle").GetChild<MeleeWeapon>(0);
        _knockback = GetNode<KnockbackComponent>("KnockbackComponent");
        _dash = GetNode<DashComponent>("DashComponent");
        _flash = GetNode<FlashComponent>("FlashComponent");
        _hitstop = GetNode<HitstopComponent>("HitstopComponent");
        _hoodie = GetNode<SpringSystem>("Hoodie");
        _cape   = GetNode<SpringSystem>("Cape");

        _hitsfx = GetNode<AudioStreamPlayer>("HitSfx");

        _health.Connect("Died", this, nameof(OnPlayerDead));
        _invulnerability.Connect("InvulnerabilityLifted", this, nameof(OnInvulnerabilityLifted));
        _damage.Connect("DamageTaken", this, nameof(OnDamageTaken));

        CameraFocusPoint = Vector2.Right * CameraExtendZone;

        GD.Print("player ready: pause mode is: " + GetTree().Paused);
        GetTree().Paused = false;

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

        float movementThreshold = 0.1f;
        if(Mathf.Abs(_velocity.Length()) >= movementThreshold)
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
        
        if(_dash.IsDashing)
        {
            focusPoint = _dash.CurrentDashVelocity + Position;
            _weaponHandle.Visible = false;
        } else
        {
            _weaponHandle.Visible = true;
        }
        
        if(focusPoint.x < Position.x)
        {
            _weaponHandle.Scale = new Vector2(1, -1);
            _animation.FlipH = true;
            _hoodie.MirrorHorizontal(1);
        } else
        {
            _weaponHandle.Scale = new Vector2(1, 1);
            _animation.FlipH = false;
            _hoodie.MirrorHorizontal(-1);


        }

        if(Input.IsActionJustPressed("player_attack") && !_dash.IsDashing)
        {
            _melee.Attack();
        }

        if(Input.IsActionJustPressed("player_dash"))
        {
            // should the hero dash in the input direction
            // or in the poiting direction
            //_dash.Dash(LastRightAxis);
            bool success = _dash.Dash(_lastDirection);            
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
        if(_freeze)
            return;

        Vector2 direction = Vector2.Zero;

        direction.y -= Input.GetActionStrength("player_move_up");
        direction.y += Input.GetActionStrength("player_move_down");

        direction.x -= Input.GetActionStrength("player_move_left");
        direction.x += Input.GetActionStrength("player_move_right");

        direction = direction.Normalized();
        _lastDirection = direction;

        // adds the player movement input
        _velocity = direction * Speed;

        // adds the knockback set by knockback component
        _velocity += _knockback.CurrentValue;
        
        // adds the dash contribution to the velocity
        // dashing resets all velocity
        if(_dash.IsDashing)
            _velocity = _dash.CurrentDashVelocity;


        _velocity = MoveAndSlide(_velocity);
        float clothingInfluence = 0.4f;
        Vector2 clothingForce = new Vector2(_velocity.x * clothingInfluence * -1 * _hoodie.Scale.x, _velocity.y * clothingInfluence * -1);
        AddClothForce(clothingForce);

        base._PhysicsProcess(delta);

    }

    public void AddClothForce(Vector2 force)
    {
        _hoodie.AddForce(force, true, 0.4f);        
        _cape.AddForce(force, true, 0.8f);
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
        GetTree().ChangeScene(GetTree().CurrentScene.Filename);        
    }

    public void OnInvulnerabilityLifted()
    {
        _animation.Modulate = Colors.White;
        //GD.Print($"player is now vulnerable.");
    }
    
    public void OnDamageTaken(Damage damageInfo)
    {
        if(_invulnerability.IsVulnerable)
        {
            _health.Damage(damageInfo.Amount);
            _knockback.SetKnockback(damageInfo.Normal, damageInfo.Knockback);
            _invulnerability.SetInvulnerable();
            _flash.SetFlash(true);
            _hitsfx.Play();
            _hitstop.StartFreeze();
            _hoodie.AddForce(damageInfo.Normal * -60);
            _cape.AddForce(damageInfo.Normal * -10);
        }
        
    }

}
