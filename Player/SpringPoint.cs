using Godot;
using System.Collections.Generic;

public class SpringPoint : Node2D
{
    [Export]
    public float Dampening = 0.8f;

    [Export]
    public Vector2 Gravity = Vector2.Zero;

    [Export]
    public float Springness = 10;

    [Export]
    public float Mass = 24f;

    [Export]
    public bool IsPinned = false;

    [Export]
    public bool UseLimits = false;

    [Export]
    public Rect2 PositionLimits = new Rect2();

    [Export]
    public List<NodePath> Connections;

    private float[] _originalDistances;
    public List<SpringPoint> SpringConnections = new List<SpringPoint>();
    
    private Vector2 _originalPosition = Vector2.Zero;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    
    public override void _Ready()
    {

        _originalPosition = Position;

        _originalDistances = new float[Connections.Count];

        for(int i = 0; i < Connections.Count; i++)
        {   
            SpringConnections.Add(GetNode<SpringPoint>(Connections[i]));
            _originalDistances[i]= Position.DistanceTo(SpringConnections[i].Position);

        }

        ZIndex = 99;

    }

    public override void _PhysicsProcess(float delta)
    {   

        if(!Engine.EditorHint)
        {

            if(!IsPinned)
            {
            
                Vector2 springForces = Vector2.Zero;
                for(int i = 0; i < SpringConnections.Count; i++)
                {
                    springForces += GetSpringForce(this, SpringConnections[i], _originalDistances[i], Springness);

                }

                AddForce(Gravity, false);
                AddForce(springForces, false);

                _velocity += _acceleration;
            
                if(UseLimits)
                {
                    _velocity = CheckVelocityLimits(_velocity);
                    Position += _velocity * delta;
                    Position = ClampPosition();

                } else
                {
                    Position += _velocity * delta;
                }

                _velocity *= Dampening;
                _acceleration = Vector2.Zero;
            }

            Update();

        }

    }

    public void AddForce(Vector2 force, bool random, float scale = 0.1f)
    {
        if(IsPinned)
            return;

        Vector2 rand = Vector2.Zero;
        if(random)
        {
            float s = force.Length() * scale;
            float r = (float)(GD.RandRange(-1f * s, 1f * s));
            rand = new Vector2(r, r);
        }

        _acceleration += (force + rand) / Mass;
    }

    public Vector2 GetSpringForce(SpringPoint a, SpringPoint b, float targetLength, float resistance)
    {
        Vector2 result = Vector2.Zero;
        Vector2 dir = (a.Position - b.Position);

        float strength = -1 * resistance * (dir.Length() - targetLength);

        result = dir.Normalized() * strength;

        return result;
    }

    private Vector2 ClampPosition()
    {

        Vector2 b = GetLimitBegin();
        Vector2 e = getLimitEnd();

        Vector2 pos = Position;

        if(Position.x < b.x)
        {
            pos.x = b.x;
            _velocity.x = 0;
        }
        
        if(Position.x > e.x)
        {
            pos.x = e.x;
            _velocity.x = 0;
        }

        if(Position.y < b.y)
        {
            pos.y = b.y;
            _velocity.y = 0;
        }
        
        if(Position.y > e.y)
        {
            pos.y = e.y;
            _velocity.y = 0;
        }

        return pos;
    }

    private Vector2 CheckVelocityLimits(Vector2 velocity)
    {

        Vector2 b = GetLimitBegin();
        Vector2 e = getLimitEnd();

        Vector2 force = velocity;


        if(Position.x < b.x && velocity.x < 0)
            force.x = 0;
        
        if(Position.x > e.x && velocity.x > 0)
            force.x = 0;

        if(Position.y < b.y && velocity.y < 0)
            force.y = 0;
        
        if(Position.y > e.y && velocity.y > 0)
            force.y = 0;

        return force;
    }

    public Vector2 GetLimitBegin()
    {
        float min_x = _originalPosition.x - PositionLimits.Position.x - PositionLimits.Size.x / 2f;
        float min_y = _originalPosition.y - PositionLimits.Position.y - PositionLimits.Size.y / 2f;

        return new Vector2(min_x, min_y);
    }

    public Vector2 getLimitEnd()
    {
        float max_x = _originalPosition.x + PositionLimits.Position.x + PositionLimits.Size.x / 2f;
        float max_y = _originalPosition.y + PositionLimits.Position.y + PositionLimits.Size.y / 2f;

        return new Vector2(max_x, max_y);

    }

    public void RefreshBasePosition(Vector2 pos)
    {
        _originalPosition = pos;
    }

    public Vector2 GetBasePosition()
    {
        return _originalPosition;
    }

    public Vector2 GetVelocity()
    {
        return _velocity;
    }


}
