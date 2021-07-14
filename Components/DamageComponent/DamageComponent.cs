using Godot;
using System.Collections.Generic;

[Tool]
public class DamageComponent : Area2D
{
    [Signal]
    public delegate void OnDamageTaken(int amount, string who);

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
                if(OverlapsArea(_areasToDamage[i]))
                {
                    EmitSignal(nameof(OnDamageTaken), _areasToDamage[i].AmountOfDamage, _areasToDamage[i].GetParent().Name);
                }

            }

            DoDamage = !OneShot;

        }

    }

}
