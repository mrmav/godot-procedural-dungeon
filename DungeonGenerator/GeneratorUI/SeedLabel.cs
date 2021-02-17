using Godot;
using System;
using DungeonGenerator;

class SeedLabel : Label
{
    
    private Generator generator;

    public override void _Ready()
    {
        generator = (Generator)GetTree().Root.GetNode("TileLevel/Generator");
        generator.Connect("OnGeneratedDungeon", this, nameof(OnGeneratedDungeon));
    }

    public void OnGeneratedDungeon(Dungeon d)
    {
        Text = "Seed: " + d.Parameters.Seed;
    }

}
