using Godot;
using System;

public class MeleeWeapon : Node2D
{    

    // [Signal]
    // public delegate void DamageDealt(int amount, string victim);

    [Export]
    public int Damage = 1;

    private DamageComponent _damage;

    public override void _Ready()
    {
        _damage = GetNode<DamageComponent>("DamageComponent");
        _damage.AmountOfDamage = Damage;
        // _damage.Connect("DamageTaken", this, nameof(OnDamageDealt));

        base._Ready();

    }

    public void Attack()
    {
        AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Stop();
        anim.Play("swing", -1, 1.5f);
        _damage.DoDamage = true;
    }

    // public void OnDamageDealt(int amount, string victim)
    // {
        
    //     EmitSignal(nameof(DamageDealt), amount, victim);
        
    // }

}

