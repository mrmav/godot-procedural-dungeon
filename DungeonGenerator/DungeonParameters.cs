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
        private float _openness;
        private int _edgeSharing;
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
        /// This value defines the deviation of the split by the center
        /// </summary>
        public float Openness
        {
            get
            {
                return _openness;
            }
            set
            {
                if (value > 1.0f) _openness = 1.0f;
                else if (value < 0.0f) _openness = 0.0f;
                else _openness = value;

            }
        }

        /// <summary>
        /// This value defines the minimum amount of edge sharing to be considered a possible connection.
        /// </summary>
        public int EdgeSharing
        {
            get
            {
                return _edgeSharing;
            }
            set
            {
                _edgeSharing = value;

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
            int w = 80, 
            int h = 80, 
            int roomW = 6, 
            int roomH = 3, 
            int splits = 8, 
            float dev = 0.15f, 
            int edgeShare = 3, 
            float openness = 0.5f,
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
            Openness = openness;
            Algorithm = algo;
            Merge = merge;
            Seed = seed;

        }

        public override string ToString()
        {
            return $"DungeonWidth: {DungeonWidth}\nDungeonHeight: {DungeonHeight}\nMinRoomWidth: {MinRoomWidth}\nMinRoomHeight: {MinRoomHeight}\nSplits: {Splits}\nSplitDeviation: {SplitDeviation}\nOpenness: {Openness}\nEdgeSharing: {EdgeSharing}\nAlgorithm: {Algorithm}\nMerge: {Merge}\nSeed: {Seed}";            
        }

    }
}