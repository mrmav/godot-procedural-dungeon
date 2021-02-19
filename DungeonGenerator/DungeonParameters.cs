using System;

namespace DungeonGenerator
{
    public class DungeonParameters : Godot.Object
    {
        public int DungeonWidth;
        public int DungeonHeight;
        private int _roomWidth;
        private int _roomHeight;
        private int _splits;
        private float _splitDeviation;        
        private float _edgeSharing;
        public DungeonHeuristic Algorithm;
        public DungeonMergeRooms Merge;
        public string Seed;

        /// <summary>
        /// The number of splits to make to the tree.
        /// The number of partitions generate is 2^n
        /// </summary>
        public int Splits
        {
            get
            {
                return _splits;
            }
            set
            {
                if(value < 2)
                {
                    Console.WriteLine("value less than two given to split number on dungeon");
                    _splits = 2;
                } else
                {
                    _splits = value;
                }
            }
        }
        
        /// <summary>
        /// This value defines the deviation of the split by the center
        /// </summary>
        public float SplitDeviation
        {
            get
            {
                return _splitDeviation;
            }
            set
            {
                if (value > 1.0f) _splitDeviation = 1.0f;
                else if (value < 0.0f) _splitDeviation = 0.0f;
                else _splitDeviation = value;

            }
        }

        /// <summary>
        /// This value defines the minimum amount of edge sharing to be considered a possible connection.
        /// For example, a value of 0f will indicate that partitions that only share the corner will be possible.
        /// A value of 1.0f will make that the the sharing must be greater or equal to at least on of the 
        /// partitions dimensions (width or height).
        /// connected.
        /// </summary>
        public float EdgeSharing
        {
            get
            {
                return _edgeSharing;
            }
            set
            {
                if (value > 1.0f) _edgeSharing = 1.0f;
                else if (value < 0.0f) _edgeSharing = 0.0f;
                else _edgeSharing = value;

            }
        }

        /// <summary>
        /// The minimum width that a partition may have.
        /// </summary>
        public int MinRoomWidth
        {
            get
            {
                return _roomWidth;
            }
            set
            {
                if (value < 0) _roomWidth = 0;
                else _roomWidth = value;

            }
        }

        /// <summary>
        /// The minimum Height that a partition may have.
        /// </summary>
        public int MinRoomHeight
        {
            get
            {
                return _roomHeight;
            }
            set
            {
                if (value < 0) _roomHeight = 0;
                else _roomHeight = value;

            }
        }     

        public DungeonParameters(
            int w = 106, 
            int h = 106, 
            int roomW = 8, 
            int roomH = 8, 
            int splits = 14, 
            float dev = 0.15f, 
            float edgeShare = 0.2f, 
            DungeonHeuristic algo = DungeonHeuristic.Weight, 
            DungeonMergeRooms merge = DungeonMergeRooms.NoMerge, 
            string seed = "")
        {

            DungeonWidth = w;
            DungeonHeight = h;
            MinRoomWidth = roomW;
            MinRoomHeight = roomH;
            Splits = splits;
            SplitDeviation = dev;
            EdgeSharing = edgeShare;
            Algorithm = algo;
            Merge = merge;
            Seed = seed;

        }

        public override string ToString()
        {
            return $"DungeonWidth: {DungeonWidth}\nDungeonHeight: {DungeonHeight}\nMinRoomWidth: {MinRoomWidth}\nMinRoomHeight: {MinRoomHeight}\nSplits: {Splits}\nSplitDeviation: {SplitDeviation}\nEdgeSharing: {EdgeSharing}\nAlgorithm: {Algorithm}\nMerge: {Merge}\nSeed: {Seed}";            
        }

    }
}