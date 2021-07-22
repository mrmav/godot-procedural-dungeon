using Godot;

public class HealthComponent : Node
{

    [Signal]
    public delegate void Died(int health);
    
    [Export]
    public int Health = 10;

    private PackedScene _notification;

    public override void _Ready()
    {
        
        _notification = GD.Load<PackedScene>("res://Components/TextPopupComponent/TextPopup.tscn");
        
        base._Ready();

    }

    public int Damage(int amount)
    {
        Health -= amount;
        
        if(Health <= 0)
        {
            // send death signal
            EmitSignal(nameof(Died), Health);

        }

        TextPopup n = (TextPopup)_notification.Instance();
        n.TextColor = Colors.OrangeRed;
        n.Text = string.Concat("-", amount);
        n.Position = ((Node2D)GetParent()).Position;
                
        GetTree().Root.GetNode("World/NotificationContainer").AddChild(n);

        return Health;

    }

}
