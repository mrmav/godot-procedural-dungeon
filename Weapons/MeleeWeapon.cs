using Godot;
using System;

public class MeleeWeapon : Node2D
{    

    // [Signal]
    // public delegate void DamageDealt(int amount, string victim);

    [Export]
    public int Damage = 1;

    [Export]
    public int KnockbackPower = 200;

    [Export]
    public int DamageFrame = 1;

    private DamageComponent _damage;
        
    private AnimationPlayer _animation;

    public override void _Ready()
    {
        _damage = GetNode<DamageComponent>("DamageComponent");
        _damage.AmountOfDamage = Damage;
        _damage.Knockback = KnockbackPower;

        _animation = GetNode<AnimationPlayer>("AnimationPlayer");

    }

    public void Attack()
    {
        AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Stop();
        anim.Play("swing", -1);
    }

    // public void OnDamageDealt(int amount, string victim)
    // {
        
    //     EmitSignal(nameof(DamageDealt), amount, victim);
        
    // }

}

