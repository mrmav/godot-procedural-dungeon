using Godot;
using System;

public class PanCamera : Camera2D
{

    [Export]
    public float Sensitivity = 1.0f;

    private Vector2 previousMousePosition = Vector2.Zero;
    private float _smoothness = 0.9f;
    public float _targetZoom = 1f;
    private float _currentZoom = 1f;
    private float _zoomIncrement = .5f;
    private uint _timeZoomTransition = 2000;
    private uint _lastTimeZoom = 0;
    private uint _targetTime = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        SceneTree tree = GetTree();
        Vector2 mousePosition = tree.Root.GetMousePosition();        
        Vector2 d = previousMousePosition - mousePosition;     

        if(Input.IsMouseButtonPressed((int)ButtonList.Left)) {


            Position += d * _currentZoom;
        }
        previousMousePosition = mousePosition;


        uint now = OS.GetTicksMsec();
        if(Input.IsActionJustReleased("zoom_in_wheel"))
        {            
            _targetZoom -= _zoomIncrement;
            ResetZoomTimers(now);
        } else if(Input.IsActionJustReleased("zoom_out_wheel"))
        {
            _targetZoom += _zoomIncrement;
            ResetZoomTimers(now);
        }

        //interpolate value with time to zoom
        _targetZoom = Mathf.Clamp(_targetZoom, 0.2f, 4f);
        if(now < _targetTime)
        {
            float _newZoom = Remap(_lastTimeZoom, _targetTime, _currentZoom, _targetZoom, now);     
            _currentZoom = _newZoom;
            Zoom = new Vector2(_currentZoom, _currentZoom);
        }
    }

    void ResetZoomTimers(uint now)
    {
        _lastTimeZoom = now;
        _targetTime = _lastTimeZoom + _timeZoomTransition;
    }

    float Lerp(float a, float b, float t)
    {
        return (1.0f - t) * a + b * t;
    }

    float InvLerp(float a, float b, float v)
    {
        return (v - a) / (b - a);
    }

    float Remap(float iMin, float iMax, float oMin, float oMax, float v)
    {
        float t = InvLerp(iMin, iMax, v);
        return Lerp(oMin, oMax, t);
    }


}
