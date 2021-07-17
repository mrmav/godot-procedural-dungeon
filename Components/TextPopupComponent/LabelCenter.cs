using Godot;
using System;

public class LabelCenter : Label
{

    public override void _Ready()
    {
        _center();
    }

    private void _center()
    {
        MarginLeft   = -RectSize.x / 2;
        MarginRight  =  RectSize.x / 2;
        MarginTop    = -RectSize.y / 2;
        MarginBottom =  RectSize.y / 2;
    }

}