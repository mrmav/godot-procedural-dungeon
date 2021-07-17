using Godot;
using System;

public class TextPopup : Node2D
{

    [Export]
    public Color TextColor = Colors.Cornflower;

    [Export]
    public string Text;

    public override void _Ready()
    {
        GetNode<Label>("Node2D/Label").Text = Text;
        GetNode<Node2D>("Node2D").Modulate = TextColor;
        
        AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
        animation.Connect("animation_finished", this, nameof(_OnAnimationFinished));
        animation.Play("fade-up");
    }

    private void _OnAnimationFinished(string name)
    {
        if(name == "fade-up")
            QueueFree();
    }

}
