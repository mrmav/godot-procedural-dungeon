using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class SpringSystem : Node
{

    [Export]
    public bool ShowDebugGeometry = false;

    public Vector2 DebugDrawOffset = Vector2.Zero;

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
        // if(ShowDebugGeometry)
        //     Update();
        
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

            _springs[i].SetBasePosition(pos);
        }
    }

}
