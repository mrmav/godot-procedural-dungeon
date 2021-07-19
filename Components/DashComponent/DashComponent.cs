using Godot;
using System.Collections.Generic;

public class DashComponent : Node
{
    [Export]
    public int Speed = 450;

    [Export]
    public float Dampening = 0.8f;

    [Export]
    public float Cooldown = 1.5f;

    [Export]
    public float DashFinishThreshold = 40f;

    [Export]
    public bool MakeDamage = false;

    [Export]
    public bool ReceiveDamage = false;

    [Export]
    public NodePath DamageComponent = null;

    [Export]
    public List<string> GroupsToDamageWhileDashing;

    private DamageComponent _damage = null;
    private Timer _timer = null;

    private Vector2 _currentDashVelocity = Vector2.Zero;
    public Vector2 CurrentDashVelocity
    {
        get
        {
            return _currentDashVelocity;
        }
    }

    private bool _isDashing = false;
    public bool IsDashing
    {
        get
        {
            return _isDashing;
        }
    }

    private bool _canDash = true;

    public override void _Ready()
    {
        if(DamageComponent != null)
        {
            _damage = GetNode<DamageComponent>(DamageComponent);
            // _damage.Connect("DamageDealt", this, nameof(StopDamage));
        } 
        _timer = GetNode<Timer>("NextDash");
        _timer.Connect("timeout", this, nameof(AllowDash));
    }
    
    public override void _PhysicsProcess(float delta)
    {
        _currentDashVelocity *= Dampening;
        
        if(_isDashing && _currentDashVelocity.Length() < DashFinishThreshold)
        {
            _isDashing = false;    
            _damage.IgnoreAllDamage = false;  
            StopDamage();
            
            // init cooldown
            _timer.Start(Cooldown);
        }

    }

    public bool Dash(Vector2 direction)
    {

        if(!_canDash)
            return false;

        _currentDashVelocity = direction * Speed;
        _isDashing = true;
                
        _damage.IgnoreAllDamage = !ReceiveDamage;

        if(MakeDamage)
        {
            _damage.DamageOnce = true;
            _damage.ApplyKnockback = false;
            AddDamageGroups();            
        }

        _canDash = false;

        return true;
        
    }

    private void StopDamage()
    {        
        _damage.IgnoreAllDamage = false;
        _damage.DamageOnce = false;
        _damage.ApplyKnockback = true;
        _damage.ResetDamaged();
        RemoveDamageGroups();
    }

    private void AddDamageGroups()
    {
        for(int i = 0; i < GroupsToDamageWhileDashing.Count; i++)
        {
            _damage.GroupsToDamage.Add(GroupsToDamageWhileDashing[i]);            
        }
    }

    private void RemoveDamageGroups()
    {
        for(int i = 0; i < GroupsToDamageWhileDashing.Count; i++)
        {            
            _damage.GroupsToDamage.Remove(GroupsToDamageWhileDashing[i]);
        }
    }

    private void AllowDash()
    {
        _canDash = true;
    }

}
