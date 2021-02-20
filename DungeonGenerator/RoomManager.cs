using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGenerator
{
    class RoomManager : Graph<Room>
    {

        private Room _initRoom;

        private Room _bossRoom;

        public Room InitRoom {
            get
            {
                if(_initRoom == null)
                {
                    _initRoom = GetSpecialRoom(DungeonRoomType.PlayerSpawn);
                }

                return _initRoom;
            }
        }
        public Room BossRoom
        {
            get
            {
                if (_bossRoom == null)
                {
                    _bossRoom = GetSpecialRoom(DungeonRoomType.Boss);
                }

                return _bossRoom;
            }
        }

        public Dungeon Dungeon;

        public List<Room> Rooms
        {
            get
            {
                return GetNodes();
            }
        }

        public List<Door> Doors;

        public List<Opening> Openings;


        public RoomManager(Dungeon d)
        {
            Dungeon = d;
            Doors = new List<Door>();
            Openings = new List<Opening>();
        }

        public List<Room> GetAllRooms()
        {
            return GetNodes();
        }

        public bool AddRoom(Room room)
        {
            return AddNode(room);
        }

        public bool Connect(Room room1, Room room2)
        {
            return AddEdge(room1, room2);
        }

        public Room GetSpecialRoom(DungeonRoomType type)
        {

            Room room = null;

            foreach(Room r in Rooms)
            {
                if(r.Type == type)
                {
                    room = r;
                    break;
                }
            }

            return room;
        }
        
        public void RandomizeNodesWeights()
        {
            List<Room> rooms = GetAllRooms();

            foreach (Room r in Rooms)
            {
                r.Weight = (int)Utils.RandomBetween(Dungeon.Rng, 0, Dungeon.MaxRoomWeight);
            }

        }

    }
}
