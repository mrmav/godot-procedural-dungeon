using Godot;
using System;

public class FlashComponent : Node
{
    
    [Export]
    public bool IsFlashing = false;

    [Export]
    public Color FlashColor = Colors.White;
    
    [Export]
    public float FlashTime = 1f;

    [Export]
    public NodePath FlashableItem = null;

    private CanvasItem _flashable = null;
    private ShaderMaterial _material = null;
    private Timer _timer = null;

    public override void _Ready()
    {
        if(FlashableItem != null)
        {
            _flashable = GetNode<CanvasItem>(FlashableItem);
        } else
        {
            GD.Print($"Path to Flashable Item is null in {GetParent().Name}");
        }

        _material = new ShaderMaterial();
        _material.Shader = GD.Load<Shader>("res://Components/FlashComponent/FlashShader.shader");

        _timer = new Timer();
        _timer.OneShot = true;
        _timer.Connect("timeout", this, nameof(Unflash));
        AddChild(_timer);

    }

    public void SetFlash(bool flash)
    {
        // first, check if the correct material is assigned to the canvas item
        if((_flashable.Material) != _material)
        {
            _flashable.Material = _material;
        }

        _material.SetShaderParam("IsFlashing", flash);

        if(flash)
        {
            _timer.WaitTime = FlashTime;
            _timer.Start();
        }

    }

    private void Unflash()
    {
        SetFlash(false);
        GD.Print($"{GetParent().Name} unflash");
    }

}
