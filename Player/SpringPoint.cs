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
    public List<NodePath> Connections = new List<NodePath>();
    
    private float[] _originalDistances;
    private List<SpringPoint> _connections = new List<SpringPoint>();
    
    private Vector2 _originalPosition = Vector2.Zero;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    
    public override void _Ready()
    {

        _originalPosition = Position;

        _originalDistances = new float[Connections.Count];

        for(int i = 0; i < Connections.Count; i++)
        {   
            _connections.Add(GetNode<SpringPoint>(Connections[i]));
            _originalDistances[i]= Position.DistanceTo(_connections[i].Position);

        }
    }

    public override void _PhysicsProcess(float delta)
    {   

        if(!Engine.EditorHint)
        {

            if(!IsPinned)
            {
            
                Vector2 springForces = Vector2.Zero;
                for(int i = 0; i < _connections.Count; i++)
                {
                    springForces += GetSpringForce(this, _connections[i], _originalDistances[i], Springness);

                }

                AddForce(Gravity);
                AddForce(springForces);

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

    // public override void _Input(InputEvent @event)
    // {

    //     if(@event.IsActionPressed("player_attack"))
    //     {

    //         Vector2 mouse = GetGlobalMousePosition();
    //         Vector2 dir = GlobalPosition - mouse;
            

    //         AddForce(dir.Normalized() * 400f);
    //     }


    //     base._Input(@event);
    // }

    public void AddForce(Vector2 force)
    {
        if(IsPinned)
            return;

        _acceleration += force / Mass;
    }

    public Vector2 GetSpringForce(SpringPoint a, SpringPoint b, float targetLength, float resistance)
    {
        Vector2 result = Vector2.Zero;
        Vector2 dir = (a.Position - b.Position);

        float strength = -1 * resistance * (dir.Length() - targetLength);

        result = dir.Normalized() * strength / Mass;

        return result;
    }

    public override void _Draw()
    {
        Transform2D inverse = Transform.Inverse();
        DrawSetTransformMatrix(inverse);

        bool debug = true;
        if(debug)
        {           

            DrawCircle(Position, .5f, Colors.Green);

            for(int i = 0; i < _connections.Count; i++)
            {            
                DrawLine(Position, _connections[i].Position, Colors.OrangeRed, .5f, false);
            }

            if(UseLimits)
            {
                Rect2 r = new Rect2(GetLimitBegin(), PositionLimits.Size);
                DrawRect(r, Colors.RoyalBlue, false, 1f, true);
            }

            DrawLine(Position, Position+_velocity, Colors.Purple, 1f, false);


        } else
        {
            DrawCircle(Position, 3f, Colors.DarkGray);

        }

        base._Draw();
    }

    // private Vector2 ClampPosition()
    // {

    //     Vector2 b = GetLimitBegin();
    //     Vector2 e = getLimitEnd();

    //     float x = Mathf.Clamp(Position.x, b.x, e.x);
    //     float y = Mathf.Clamp(Position.y, b.y, e.y);

    //     return new Vector2(x, y);
    // }

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

    // public void AddLimitingForces()
    // {
    //     Vector2 b = GetLimitBegin();
    //     Vector2 e = getLimitEnd();

    //     Vector2 force = Vector2.Zero;

    //     if(Position.x < b.x)
    //         force.x += b.x - Position.x;
        
    //     if(Position.x > e.x)
    //         force.x += e.x - Position.x;

    //     if(Position.y < b.y)
    //         force.y += b.y - Position.y;
        
    //     if(Position.y > e.y)
    //         force.y += e.y - Position.y;

    //     AddForce(force * 100);
    // }

    public void LimitVelocity()
    {
        Vector2 b = GetLimitBegin();
        Vector2 e = getLimitEnd();

        if(Position.x < b.x)
            _velocity.x = 0;
        
        if(Position.x > e.x)
            _velocity.x = 0;

        if(Position.y < b.y)
            _velocity.y = 0;
        
        if(Position.y > e.y)
            _velocity.y = 0;
    }

    private Vector2 GetLimitBegin()
    {
        float min_x = _originalPosition.x - PositionLimits.Position.x - PositionLimits.Size.x / 2f;
        float min_y = _originalPosition.y - PositionLimits.Position.y - PositionLimits.Size.y / 2f;

        return new Vector2(min_x, min_y);
    }

    private Vector2 getLimitEnd()
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


}
