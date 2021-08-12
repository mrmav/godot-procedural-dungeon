using Godot;
using System.Collections.Generic;

public class SpringPoint : Node2D
{
    [Export]
    /// <summary>
    /// The general dampening applied to the velocity
    /// </summary>
    public float Dampening = 0.8f;

    [Export]
    public Vector2 Gravity = Vector2.Zero;

    [Export]
    public float Stiffness = 12f;

    [Export]
    public float Mass = 4f;

    [Export]
    public float SpringStrength = 0.5f;

    [Export]
    public float LinearRestitutionDampening = 0.001f;

    [Export]
    public float JointForceMult = 0.001f;

    [Export]
    public float MaxVelocity = 400f;

    [Export]
    public bool IsPinned = false;

    [Export]
    public bool UseLimits = false;

    [Export]
    public Rect2 PositionLimits = new Rect2();

    [Export]
    public List<NodePath> Connections;

    public List<SpringPoint> SpringConnections = new List<SpringPoint>();
    public Vector2 DebugDrawOffset = Vector2.Zero;


    private float[] _originalDistances;
    private Vector2 _originalPosition = Vector2.Zero;
    private Vector2 _basePosition = Vector2.Zero;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    private float _originalMaxVelocity;
    private float _originalLinearRestitution;

    private SpringSystem _parent;
        
    public override void _Ready()
    {

        _parent = GetParent<SpringSystem>();

        _originalPosition = Position;
        _basePosition = Position;
        _originalMaxVelocity = MaxVelocity;
        _originalLinearRestitution = LinearRestitutionDampening;

        _originalDistances = new float[Connections.Count];

        for(int i = 0; i < Connections.Count; i++)
        {   
            SpringConnections.Add(GetNode<SpringPoint>(Connections[i]));
            _originalDistances[i] = Position.DistanceTo(SpringConnections[i].Position);

        }

    }

    public override void _Process(float delta)
    {

        if(GetParent<SpringSystem>().ShowDebugGeometry)
        {           
            Update();
        }

        base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {   

        if(!Engine.EditorHint)
        {

            if(!IsPinned)
            {

                AddForce(Gravity, false);
                AddForce(GetSpringForce(), false);
                AddForce(GetLinearRestitution(LinearRestitutionDampening) * (1f / delta), false);                
                AddForce(GetJointForces(JointForceMult), false);

                _velocity += _acceleration;
                _velocity *= Dampening;

                _acceleration = Vector2.Zero;
                
                // there is a behaviour not intended with max velocity:
                // when receiveing a force (eg.:_knockback), the max velocity
                // limits the spring return to it's original position
                // I'll try to fix this by not limiting the velocity
                // if we are to far away from the goal position

                float originalDistancesCumulative = 0f;
                float currentDistancesCumulative = 0f;
                for(int i = 0; i < SpringConnections.Count; i++)
                {
                    originalDistancesCumulative += _originalDistances[i];
                    currentDistancesCumulative += Mathf.Abs((GlobalPosition - SpringConnections[i].GlobalPosition).Length());
                }

                // now we have the cumulative distances
                // let's check how far apart are them
                float diffPercent = Mathf.Abs(currentDistancesCumulative / originalDistancesCumulative - 1f);
                float maxVelocityThreshold = 0.2f;

                // only cap the velocity if we are within the defined distance threshold
                if(diffPercent < maxVelocityThreshold)
                {
                    if(_velocity.Length() > MaxVelocity)
                    {
                        _velocity = _velocity.Normalized() * MaxVelocity;
                    }
                }

                if(UseLimits)
                {
                    _velocity = CheckVelocityLimits(_velocity);
                    Position += _velocity * delta;
                    Position = ClampPosition();

                } else
                {
                    Position += _velocity * delta;
                }

            }

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

    /// <summary>
    /// Calculates the force to be applied with a spring behaviour.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetSpringForce()
    {

        Vector2 force = Vector2.Zero;
        for(int i = 0; i < SpringConnections.Count; i++)
        {            
            Vector2 dir = (Position - SpringConnections[i].Position);

            float strength = -1 * Stiffness * (dir.Length() - _originalDistances[i]);

            force += dir.Normalized() * strength;
        }
        
        return force * SpringStrength;
    }

    /// <summary>
    /// Calculates the linear force to be applied so the point returns to the original position.
    /// This method is usefull to limit some behaviour.
    /// </summary>
    /// <param name="damp"></param>
    /// <returns></returns>
    public Vector2 GetLinearRestitution(float damp)
    {
        return (_basePosition - Position) * damp;
    }

    /// <summary>
    /// Calculates point force as if it is a joint.
    /// </summary>
    /// <param name="damp"></param>
    /// <returns>The force to be applied to maintain the joint.</returns>
    public Vector2 GetJointForces(float damp)
    {

        Vector2 result = Vector2.Zero;

        for(int i = 0; i < SpringConnections.Count; i++)
        {
            Vector2 dir = SpringConnections[i].Position - Position;
            float length = dir.Length();
            float target = _originalDistances[i];

            result += dir * (length - target) * damp;

        }

        return result;
    }

    /// <summary>
    /// Clamps the position to the bound limits.
    /// </summary>
    /// <returns>The clamped position</returns>
    private Vector2 ClampPosition()
    {

        Vector2 b = GetLimitBegin();
        Vector2 e = GetLimitEnd();

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

    /// <summary>
    /// Limits the velocity to 0f, on the direction that left the bounds.
    /// </summary>
    /// <param name="velocity">The resulting velocity</param>
    /// <returns></returns>
    private Vector2 CheckVelocityLimits(Vector2 velocity)
    {

        Vector2 b = GetLimitBegin();
        Vector2 e = GetLimitEnd();

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

    public Vector2 GetLimitEnd()
    {
        float max_x = _originalPosition.x + PositionLimits.Position.x + PositionLimits.Size.x / 2f;
        float max_y = _originalPosition.y + PositionLimits.Position.y + PositionLimits.Size.y / 2f;

        return new Vector2(max_x, max_y);

    }

    public Vector2 GetOriginalPosition()
    {
        return _originalPosition;
    }

    public void SetBasePosition(Vector2 pos)
    {
        _basePosition = pos;
    }

    public Vector2 GetVelocity()
    {
        return _velocity;
    }


    public float GetOriginalMaxVelocity()
    {
        return _originalMaxVelocity;
    }

    public float GetOriginalLinearRestitution()
    {
        return _originalLinearRestitution;
    }


    public override void _Draw()
    {
        
        if(_parent.ShowDebugGeometry)
        {

            DrawCircle(_parent.DebugDrawOffset, .5f, Colors.Green);

            for(int i = 0; i < SpringConnections.Count; i++)
            {            
                DrawLine(_parent.DebugDrawOffset, SpringConnections[i].Position - Position + _parent.DebugDrawOffset, Colors.OrangeRed, .5f, false);
            }

            if(UseLimits)
            {
                Rect2 r = new Rect2(GetLimitBegin(), PositionLimits.Size);
                DrawRect(r, Colors.RoyalBlue, false, 1f, true);
            }

            DrawLine(_parent.DebugDrawOffset, _velocity * 0.1f + _parent.DebugDrawOffset, Colors.Purple, 1f, false);

        }

        base._Draw();
    }

}
