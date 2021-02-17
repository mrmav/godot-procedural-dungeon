using Godot;
using System;

public class SeedInput : LineEdit
{
    [Export]
    public NodePath SeedLabelPath;

    private Label _seedLabel;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _seedLabel = GetNode<Label>(SeedLabelPath);
        _seedLabel.Connect("SeedClicked", this, nameof(Fill));
    }

    private void Fill(string text)
    {
        Text = text;
    }

}
