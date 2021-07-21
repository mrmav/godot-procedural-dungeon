using Godot;
using System;

public class HitstopComponent : Node
{   

    [Export]
    public int FreezeFrames = 0;

    [Export]
    public float SlowMotionProbability = 0.1f;

    private bool _isSlowMo = false;
    private Timer _timer;

    public override void _Ready()
    {
        _timer = new Timer();
        _timer.OneShot = true;
        _timer.Connect("timeout", this, nameof(Reset));
        AddChild(_timer);

        PauseMode = PauseModeEnum.Process;
    }

    private void Reset()
    {
        // Physics2DServer.SetActive(true);  // this doesnt work
        GetTree().Paused = false;   // pauses everything. sound, vfx, ...
        Engine.TimeScale = 1.0f;
        
    }

    public void StartFreeze()
    {        
        StartFreeze(FreezeFrames);
    }

    public void StartFreeze(int frames = 16)
    {    
        //Physics2DServer.SetActive(false);
        
        GD.Randomize();
        if(SlowMotionProbability >= GD.Randf())
        {
            Engine.TimeScale = 0.1f;

        } else
        {
            GetTree().Paused = true;
        }        

        _timer.WaitTime = 1f / 60f * frames;
        _timer.Start();
    }

}


// on hitting an enemy or the player hit by something
// freeze everything for some number of frames

