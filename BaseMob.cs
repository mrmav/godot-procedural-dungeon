using Godot;
using System;

public class BaseMob : KinematicBody2D
{
    
    protected bool _freeze = false;

    public void SetFreeze(bool val)
    {
        _freeze = val;
    }

}
