using Godot;
using System;
using System.Collections.Generic;


namespace DungeonGenerator
{

    class Visualizer : Node2D
    {
        
        [Export]
        public NodePath camera2DPath;
        [Export]
        public NodePath GeneratorNode;
        [Export]
        public float DebugScale = 1f;

        private Camera2D camera;
        private Generator generator;
        public Dungeon dungeon;
        private BaseButton generateButton;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            camera = GetNode<Camera2D>(camera2DPath);
            generator = (Generator)GetNode(GeneratorNode);
            dungeon = (Dungeon)(generator.Get("dungeon"));

            generator.Connect("OnGeneratedDungeon", this, nameof(OnGeneratedDungeon));

            Position += new Vector2(DebugScale, DebugScale) / 2f;

        }

        public override void _Draw()
        {
            
            if(dungeon != null)
            {

                // draw a rectangle for all the partitions
                List<Partition> partitions = dungeon.PartitionTree.GetAllLeafs();
                foreach (Partition p in partitions)
                {
                    float th = 1f * camera.Zoom.x;
                    DrawRect(new Rect2(p.X * DebugScale, p.Y * DebugScale, p.Width * DebugScale, p.Height * DebugScale), new Color(0f, 1f, 0f), false, th, true);
                }



                // draw graph
                List<Room> rooms = dungeon.Rooms.GetAllRooms();

                foreach (Room room in rooms)
                {

                    // get edges:
                    List<Room> edges = dungeon.Rooms.GetEdges(room);

                    float nodeCenterX = room.Partition.X * DebugScale + room.Partition.Width * DebugScale / 2f;
                    float nodeCenterY = room.Partition.Y * DebugScale + room.Partition.Height * DebugScale / 2f;

                    foreach (Room item in edges)
                    {

                        Color c = new Color(1,0,0);

                        float centerX = item.Partition.X * DebugScale + item.Partition.Width * DebugScale / 2f;
                        float centerY = item.Partition.Y * DebugScale + item.Partition.Height * DebugScale / 2f;

                        float th = 1f * camera.Zoom.x;

                        DrawLine(new Vector2(nodeCenterX, nodeCenterY), new Vector2(centerX, centerY), c, th, true);

                    }
                }

            }

            base._Draw();
        }

        public void SetDungeon(Dungeon d) 
        {
            dungeon = d;
        }

        public void OnGeneratedDungeon(Dungeon d)
        {
            dungeon = d;
            Update();
        }

    }

}