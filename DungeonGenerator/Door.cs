using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator
{
    class Door
    {

        Room[] _rooms;

        public int X;
        public int Y;

        public Door(int x, int y, Room room1, Room room2)
        {

            _rooms = new Room[2];
            _rooms[0] = room1;
            _rooms[1] = room2;

            X = x;
            Y = y;

        }

    }
}
