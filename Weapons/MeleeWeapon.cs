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

    [Export]
    public NodePath SwingAudioPlayer;

    private DamageComponent _damage;        
    private AnimationPlayer _animation;
    private AudioStreamPlayer _swingSfx;
    private bool _isAttacking = false;

    public override void _Ready()
    {
        _damage = GetNode<DamageComponent>("DamageComponent");
        _damage.AmountOfDamage = Damage;
        _damage.Knockback = KnockbackPower;

        _animation = GetNode<AnimationPlayer>("AnimationPlayer");
        _animation.Connect("animation_finished", this, nameof(OnAnimationFinish));    

        _swingSfx = GetNode<AudioStreamPlayer>(SwingAudioPlayer);

    }

    public void Attack()
    {
        if(!_isAttacking)
        {
            _animation.Stop();
            _animation.Play("swing", -1);
                        
            _swingSfx.Play();
            
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

