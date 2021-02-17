using Godot;

class DungeonDebugShow : CheckButton
{    

    [Signal]
    public delegate void DebugShowOff();

    public override void _Ready()
    {
        
    }

    public override void _Toggled(bool buttonPressed)
    {

        EmitSignal(nameof(DebugShowOff));

        base._Toggled(buttonPressed);
    }

}
