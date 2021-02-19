using Godot;
using System;

namespace DungeonGenerator
{
    class Generator : Node
    {

        [Signal]
        public delegate void OnGeneratedDungeon(Dungeon dungeon);

        public Dungeon dungeon;

        private BaseButton generateButton;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            generateButton = GetNode<BaseButton>("../GeneratorUI/Control/Panel/Generate");
            generateButton.Connect("GenerateNewDungeon", this, nameof(GenerateNew));

            GenerateNew();
        }

        public void GenerateNew()
        {
            GenerateNew(new DungeonParameters());
        }

        public void GenerateNew(DungeonParameters parameters)
        {
            dungeon = new Dungeon(parameters);
            

            try
            {
                dungeon.GeneratePartitioning();
                dungeon.GenerateRooms();
                dungeon.BuildGraph();
                dungeon.ConnectDungeonRooms();
                dungeon.MergeRooms();
                dungeon.GenerateDoors();                

            } catch(Exception e)
            {
                Console.WriteLine("There was a problem generating the dungeon...\n");
                Console.WriteLine(dungeon.Parameters);
                Console.WriteLine(e.Message);
            }

            EmitSignal(nameof(OnGeneratedDungeon), dungeon);

        }

    }

}