using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class SpringSystem : Node2D
{

    [Export]
    public bool ShowDebugGeometry = false;

    protected List<SpringPoint> _springs = new List<SpringPoint>();

    public override void _Ready()
    {
        Array children = GetChildren();
        for(int i = 0; i < children.Count; i++)
        {
            if(children[i] is SpringPoint)
            {
                _springs.Add((SpringPoint)children[i]);
            }

        }

    }

    public override void _Process(float delta)
    {
        if(ShowDebugGeometry)
            Update();
        
    }

    public void AddForce(Vector2 force, bool random = false, float scale = 10f)
    {
        for(int i = 0; i < _springs.Count; i++)
        {
            _springs[i].AddForce(force, random, scale);
        }
    }

    public void MirrorHorizontal(int side)
    {
        for(int i = 0; i < _springs.Count; i++)
        {
            Vector2 pos = _springs[i].GetOriginalPosition();
            
            pos.x = Mathf.Abs(pos.x) * side;

            _springs[i].RefreshBasePosition(pos);
        }
    }

    public override void _Draw()
    {
        if(ShowDebugGeometry)
        {
            for(int i = 0; i < _springs.Count; i++)
            {
                Debug(_springs[i]);
            }
        }

        base._Draw();
    }

    
    public void Debug(SpringPoint spring)
    {
        // Transform2D inverse = Transform.Inverse();
        // DrawSetTransformMatrix(inverse);

        DrawCircle(spring.Position, .5f, Colors.Green);

        for(int i = 0; i < spring.SpringConnections.Count; i++)
        {            
            DrawLine(spring.Position, spring.SpringConnections[i].Position, Colors.OrangeRed, .5f, false);
        }

        if(spring.UseLimits)
        {
            Rect2 r = new Rect2(spring.GetLimitBegin(), spring.PositionLimits.Size);
            DrawRect(r, Colors.RoyalBlue, false, 1f, true);
        }

        DrawLine(spring.Position, spring.Position + spring.GetVelocity(), Colors.Purple, 1f, false);

    }

}
