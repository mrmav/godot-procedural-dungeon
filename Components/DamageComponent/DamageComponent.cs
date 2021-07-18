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
    public List<string> GroupsToDamage;
    [Export]
    public bool OneShot = false;
    [Export]
    public bool DoDamage = true;
    [Export]
    public int Knockback = 100;
    [Export]
    public bool IgnoreAllDamage = false;
    [Export]
    public bool DamageOnce = false;

    private List<DamageComponent> _areasToDamage;
    private List<DamageComponent> _damaged;
    
    private bool _firstRun = true;

    public override void _Ready()
    {
        AddToGroup(Group);

        _areasToDamage = new List<DamageComponent>();
        _damaged = new List<DamageComponent>();

    }

    public override void _PhysicsProcess(float delta)
    {

        if(_firstRun)
        {

            // setup
            _areasToDamage.Clear();

            for(int i = 0; i < GroupsToDamage.Count; i++)
            {
                Godot.Collections.Array nodes = GetTree().GetNodesInGroup(GroupsToDamage[i]);

                for(int j = 0; j < nodes.Count; j++)
                {
                    _areasToDamage.Add((DamageComponent)nodes[j]);
                }

            }            

            //_firstRun = false;  // commenting this line makes getting the nodes at all times
                                  // this way, is possible to add and remove nodes from groups
                                  // at runtime, without the need to refresh the lists, or test
                                  // if an area is valid (wasn't "queue_free'd")

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

                    // if the other area is not set to ignore damage
                    if(!other.IgnoreAllDamage)
                    {                        
                        // then check if this area is set to only damage once
                        if(DamageOnce)
                        {
                            if(!(_damaged.Contains(other)))
                            {
                                SendSignals(damageInfo, other);
                                _damaged.Add(other);
                            }

                        } else
                        {
                            SendSignals(damageInfo, other);
                        }
                    }
                }
            }

            DoDamage = !OneShot;

        }

    }

    private void SendSignals(Damage damageInfo, DamageComponent other)
    {
        EmitSignal(nameof(DamageDealt), damageInfo);
        other.EmitSignal(nameof(DamageTaken), damageInfo);
    }

    public void ResetDamaged()
    {
        _damaged.Clear();
    }

}
