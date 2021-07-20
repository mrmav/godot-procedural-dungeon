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

    [Export]
    public bool CanCancel = false;

    private DamageComponent _damage;        
    private AnimationPlayer _animation;
    private bool _isAttacking = false;

    public override void _Ready()
    {
        _damage = GetNode<DamageComponent>("DamageComponent");
        _damage.AmountOfDamage = Damage;
        _damage.Knockback = KnockbackPower;

        _animation = GetNode<AnimationPlayer>("AnimationPlayer");
        _animation.Connect("animation_finished", this, nameof(OnAnimationFinish));    

    }

    public void Attack()
    {
        if(!_isAttacking)
        {
            AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
            anim.Stop();
            anim.Play("swing", -1);
            _isAttacking = true;
        }

        if(CanCancel && !_isAttacking)
        {
            _isAttacking = false;
            Attack();
        }
    }

    private void OnAnimationFinish(string anim)
    {
        _isAttacking = false;
        GD.Print("anim finished");
    }

    // public void OnDamageDealt(int amount, string victim)
    // {
        
    //     EmitSignal(nameof(DamageDealt), amount, victim);
        
    // }

}

