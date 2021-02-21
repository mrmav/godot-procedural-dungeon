
namespace DungeonGenerator
{
    class Opening
    {

        Room[] _rooms;

        public int X1;
        public int Y1;
        public int X2;
        public int Y2;

        public Opening(int x1, int y1, int x2, int y2, Room room1, Room room2)
        {

            _rooms = new Room[2];
            _rooms[0] = room1;
            _rooms[1] = room2;

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

        }

    }
}
