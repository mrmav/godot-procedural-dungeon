using Godot;
using System;

public class KnockbackComponent : Node
{
    
    [Export]
    public float Dampening = 0.8f;

    public Vector2 CurrentValue = Vector2.Zero;
    
    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {

        CurrentValue *= Dampening;

    }

    public void SetKnockback(Vector2 direction, float strength = 80f)
    {
        CurrentValue += direction.Normalized() * strength;
    }

}
