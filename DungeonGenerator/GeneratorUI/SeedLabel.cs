using Godot;
using System;
using DungeonGenerator;

class SeedLabel : Label
{
    [Signal]
    public delegate void SeedClicked(string seed);

    private Generator generator;
    private bool _mouseInside = false;
    private string seed;

    public override void _Ready()
    {
        generator = (Generator)GetTree().Root.GetNode("TileLevel/Generator");
        generator.Connect("OnGeneratedDungeon", this, nameof(OnGeneratedDungeon));

        Set("custom_colors/font_color", Godot.Colors.WhiteSmoke);  

    }

    public override void _Process(float delta)
    {
        if(_mouseInside && Input.IsMouseButtonPressed((int)ButtonList.Left))
        {
            // send signal
            EmitSignal(nameof(SeedClicked), seed);
        }

        base._Process(delta);
    }

    public void OnGeneratedDungeon(Dungeon d)
    {
        seed = d.Parameters.Seed;
        Text = "Seed: " + seed;
    }

    private void _on_SeedText_mouse_entered()
    {        
        _mouseInside = true;
        Set("custom_colors/font_color", Godot.Colors.RoyalBlue);  
    }

    private void _on_SeedText_mouse_exited()
    {        
        _mouseInside = false;     
        Set("custom_colors/font_color", Godot.Colors.WhiteSmoke);  

    }

}
