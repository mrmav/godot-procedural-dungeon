using Godot;
using System.Collections.Generic;

[Tool]
public class DamageComponent : Area2D
{
    [Signal]
    public delegate void DamageTaken(Damage damage);

    [Signal]
    public delegate void DamageDealt(Damage damage);

    [Export]
    public int AmountOfDamage = 1;
    [Export]
    public string Group = "default";
    [Export]
    public string[] GroupsToDamage;
    [Export]
    public bool OneShot = false;
    [Export]
    public bool DoDamage = true;
    [Export]
    public int Knockback = 100;

    private List<DamageComponent> _areasToDamage;
    
    private bool _firstRun = true;

    public override void _Ready()
    {
        AddToGroup(Group);

        _areasToDamage = new List<DamageComponent>();

    }

    public override void _PhysicsProcess(float delta)
    {

        if(_firstRun)
        {

            // setup

            for(int i = 0; i < GroupsToDamage.Length; i++)
            {
                Godot.Collections.Array nodes = GetTree().GetNodesInGroup(GroupsToDamage[i]);

                for(int j = 0; j < nodes.Count; j++)
                {
                    _areasToDamage.Add((DamageComponent)nodes[j]);
                }

            }            

            _firstRun = false;

        }

        if(DoDamage)
        {
            for(int i = 0; i < _areasToDamage.Count; i++)
            {
                DamageComponent other = _areasToDamage[i];

                if(!IsInstanceValid(other))
                {
                    // this area is no longer available
                    // (a foe died, a trap was destroyed, etc)
                    _areasToDamage.Remove(other);
                    continue;

                }

                if(OverlapsArea(other))
                {
                    Vector2 damageNormal = other.GetParent<Node2D>().GlobalPosition - GetParent<Node2D>().GlobalPosition;

                    Damage damageInfo = new Damage
                    (
                        AmountOfDamage,
                        Knockback,
                        GetParent<Node>(),
                        other.GetParent<Node>(),
                        damageNormal.x,
                        damageNormal.y
                        
                    );

                    EmitSignal(nameof(DamageDealt), damageInfo);
                    other.EmitSignal(nameof(DamageTaken), damageInfo);
                }

            }

            DoDamage = !OneShot;

        }

    }

}
