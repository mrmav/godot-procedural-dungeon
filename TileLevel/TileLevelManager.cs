using Godot;
using System;
using DungeonGenerator;

class TileLevelManager : Node
{
    private TileMap tileMap;

    private Node2D debug;

    private Camera2D camera;

    public override void _Ready()
    {

        tileMap = GetNode<TileMap>("TileMap");
        debug = GetNode<Node2D>("DungeonDebug");
        camera = GetNode<Camera2D>("Camera2D");

        camera.Position = new Vector2(
            ((Visualizer)debug).dungeon.Parameters.DungeonWidth / 2,
            ((Visualizer)debug).dungeon.Parameters.DungeonHeight / 2
        ) * ((Visualizer)debug).DebugScale;        
        
        CheckButton debugButton   = GetNode<CheckButton>("GeneratorUI/Control/Panel2/VBoxContainer/DungeonDebug/CheckButton");
        CheckButton tilemapButton = GetNode<CheckButton>("GeneratorUI/Control/Panel2/VBoxContainer/ShowTileMap/CheckButton");

        debugButton.Connect("DebugShowOff", this, nameof(HideDebug));
        tilemapButton.Connect("TileMapShowOff", this, nameof(HideMap));

    

    }

    public void HideDebug()
    {
        debug.Visible = !debug.Visible;
    }

    public void HideMap()
    {
        tileMap.Visible = !tileMap.Visible;
    }
}
