using Godot;

public class HealthComponent : Node
{

    [Signal]
    public delegate void Died(int health);
    
    [Export]
    private int Health = 10;

    public int GetHealth()
    {
        return Health;
    }    

    public int Damage(int amount)
    {
        Health -= amount;
        
        if(Health < 0)
        {
            // send death signal
            EmitSignal(nameof(Died), Health);

        }

        return Health;

    }

}
