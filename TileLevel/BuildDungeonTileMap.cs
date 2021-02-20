using Godot;
using System;
using System.Collections.Generic;
using DungeonGenerator;

class BuildDungeonTileMap : TileMap
{
    [Export]
    public NodePath GeneratorNode;
    private Dungeon dungeon;
    private Generator generator;
    private BaseButton generateButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        generator = (Generator)GetNode(GeneratorNode);
        dungeon = (Dungeon)generator.Get("dungeon");
        
        generator.Connect("OnGeneratedDungeon", this, nameof(OnGeneratedDungeon));

        BuildTileMap();

    }

    public void BuildTileMap()
    {

        this.Clear();

        // well, build your thing
        List<Room> rooms = dungeon.Rooms.GetAllRooms();
        foreach (Room r in rooms)
        {
            int x = r.Partition.X;
            int y = r.Partition.Y;

            int w = r.Partition.Width;
            int h = r.Partition.Height;
            
            // fill in the ground
            for(int yy = y; yy <= y + h; yy++)
            {
                for(int xx = x; xx <= x + w; xx++)
                {
                    int cellType = 1;
                    if(r.Type == DungeonRoomType.Boss)
                        cellType = 2;
                    if(r.Type == DungeonRoomType.Key)
                        cellType = 3;
                    if(r.Type == DungeonRoomType.PlayerSpawn)
                        cellType = 4;

                    SetCell(xx, yy, cellType);  
                }
            }

            // fill top and bottom rows
            for(int xx = x; xx <= x + w; xx++)
            {
                // top row
                SetCell(xx, y, 0);

                // bottom row
                SetCell(xx, y + h, 0);
            }

            // fill sides rows
            for(int yy = y; yy <= y + h; yy++)
            {
                // top row
                SetCell(x, yy, 0);

                // bottom row
                SetCell(x + w, yy, 0);
            }

        }

        List<Door> doors = dungeon.Rooms.Doors;
        foreach (Door d in doors)
        {
            int x = d.X;
            int y = d.Y;
            
            SetCell(x, y, 1);
            
        }

        List<Opening> opening = dungeon.Rooms.Openings;
        foreach (Opening o in opening)
        {

            for(int x = o.X1; x <= o.X2; x++)
            {
                for(int y = o.Y1; y <= o.Y2; y++)
                {
                    SetCell(x, y, 1);
                }
            }
                        
        }

    }

    public void OnGeneratedDungeon(Dungeon d)
    {
        dungeon = d;
        BuildTileMap();
    }
}
