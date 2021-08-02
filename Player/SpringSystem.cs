using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class SpringSystem : Node2D
{
    protected List<SpringPoint> _springs = new List<SpringPoint>();

    // Called when the node enters the scene tree for the first time.
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

    public void AddForce(Vector2 force)
    {
        for(int i = 0; i < _springs.Count; i++)
        {
            _springs[i].AddForce(force, true);
        }
    }

    public void MirrorHorizontal(int side)
    {
        for(int i = 0; i < _springs.Count; i++)
        {
            Vector2 pos = _springs[i].GetBasePosition();
            
            pos.x = Mathf.Abs(pos.x) * side;

            _springs[i].RefreshBasePosition(pos);
        }
    }

}
