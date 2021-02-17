using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGenerator
{
    class Room : GraphNode
    {   
        public Dungeon Dungeon { get; private set; }
        public Partition Partition { get; private set; }
        public DungeonRoomType Type { get; set; }

        public Room(Dungeon dungeon, Partition partition, DungeonRoomType type = DungeonRoomType.Base) 
            : base ((int)Utils.RandomBetween(dungeon.Rng, 0, dungeon.MaxRoomWeight))
        {
            Dungeon = dungeon;
            Partition = partition;
            
            Type = type;

        }
    }
}
