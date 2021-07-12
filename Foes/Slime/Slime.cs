using Godot;
using Godot.Collections;
using System;


public class Slime : KinematicBody2D
{

    public enum Behaviour
    {
        Wander,
        Follow,
        Attack
    }

    [Export]
    public float Speed = 20f;
    [Export]
    public int Damage = 1;    
    [Export]
    public Behaviour Mode = Behaviour.Wander;
    [Export]
    public float NewTargetTime = 2f;
    [Export]
    public float WalkTime = 4f;  

    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _target = Vector2.Zero;    
    private KinematicBody2D _targetFoe = null;
        
    private AnimatedSprite _animation;
    private Area2D _damageArea;
    private Area2D _detectArea;
    private Timer _newTargetTimer;
    private Timer _walkDurationTimer;
    private bool _refreshTargetTimer = true;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animation = GetNode<AnimatedSprite>("AnimatedSprite");
        _damageArea = GetNode<Area2D>("DamageArea");
        _detectArea = GetNode<Area2D>("DetectArea");
        _newTargetTimer = GetNode<Timer>("NewTargetTimer");
        _walkDurationTimer = GetNode<Timer>("WalkingDurationTimer");
        _targetFoe = GetParent().GetNode<KinematicBody2D>("Player");

        _newTargetTimer.Connect("timeout", this, nameof(SetRandomTarget));
        _walkDurationTimer.Connect("timeout", this, nameof(SetRefreshTimer));

        _rng.Randomize();
        
        _walkDurationTimer.WaitTime = WalkTime;

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

        Mode = Behaviour.Wander;
        // try to find the player        
        if(_detectArea.OverlapsArea(_targetFoe.GetNode<Area2D>("DamageArea")))
        {

            var spaceState = GetWorld2d().DirectSpaceState;
            Dictionary result = spaceState.IntersectRay(GlobalPosition, _targetFoe.GlobalPosition, new Godot.Collections.Array { this, _targetFoe });

            if(!result.Contains("collider"))
            {

                Mode = Behaviour.Follow;
            } else 
            {
                Mode = Behaviour.Wander;
            }


        }

     }

    public override void _PhysicsProcess(float delta)
    {
        
        if(Mode == Behaviour.Wander)
        {    
            _velocity = Vector2.Zero;

            if(_refreshTargetTimer)
            {
                _newTargetTimer.WaitTime = NewTargetTime * _rng.RandfRange(0.8f, 1.2f);  // add some variation
                _newTargetTimer.Start();
                _refreshTargetTimer = false;

            }

            // move until the counter is not 0
            if(!_walkDurationTimer.IsStopped())
            {

                Vector2 direction = _target - Position;
                direction = direction.Normalized();

                _velocity = direction * Speed;
                MoveAndSlide(_velocity);

                float distMargin = 3f;
                if((Position - _target).Length() < distMargin)
                {
                    SetRefreshTimer();
                }
                
            }
           
        }

        if(Mode == Behaviour.Follow)
        {
            
            Vector2 direction = _targetFoe.Position - Position;
            direction = direction.Normalized();

            _velocity = direction * Speed;
            MoveAndSlide(_velocity);
        }

        base._PhysicsProcess(delta);
    }

    // gets a random position inside the detect area
    private Vector2 GetRandomPos()
    {
        Vector2 t = Vector2.Zero;
        CollisionShape2D c = _detectArea.GetNode<CollisionShape2D>("CollisionShape2D");
        
        int theta = _rng.RandiRange(-180, 180);

        // make the point only from outter ring of the _detectArea
        float maxRadius = ((CircleShape2D)c.Shape).Radius;
        float minRadius = maxRadius * 0.5f;

        t.x = c.Position.x + (maxRadius - minRadius) * Mathf.Cos(theta) + minRadius;
        t.y = c.Position.y + (maxRadius - minRadius) * Mathf.Sin(theta) + minRadius;

        return t;
    }

    private void SetRandomTarget()
    {
        _target = GetRandomPos();
        GD.Print(_target);
        _walkDurationTimer.Start();
    }

    private void SetRefreshTimer()
    {
        _refreshTargetTimer = true;
        _walkDurationTimer.Stop();
    }

}
