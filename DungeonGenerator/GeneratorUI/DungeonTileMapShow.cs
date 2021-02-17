using Godot;

class DungeonTileMapShow : CheckButton
{    

    [Signal]
    public delegate void TileMapShowOff();

    public override void _Ready()
    {
        
    }

    public override void _Toggled(bool buttonPressed)
    {

        EmitSignal(nameof(TileMapShowOff));

        base._Toggled(buttonPressed);
    }

}
