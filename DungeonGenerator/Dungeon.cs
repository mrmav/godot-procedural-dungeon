using System;
using System.Collections.Generic;
using System.Linq;

/*
 * About dungeon arquitecture.
 * TBD: Maybe a better approach for the dungeon
 * layout would be to generate sets of special rooms 
 * spawing within their rules and bases on chance.
 * For example, spawn a random number of nests 
 * (within some boundaries) and then roll a chance for 
 * the key room to be adjacent to one of these nests.
 * This way, there would be times that you would need 
 * to fight for the key and times that you could just get it.
 * Keep in mind that the key would then have rules 
 * for the case of being adjacent to a nest room (maybe, tbd).
 * Then, for the connectivity, random cycles could be picked.
 * 
 * Just keep the scope in mind and don't overcomplicate.
 * You need a playable level first of all.
 * Iteration will follow.
 * 
 */ 


namespace DungeonGenerator
{
    enum DungeonRoomType
    {
        Boss,
        PlayerSpawn,
        Key,
        Nest,
        Heal,
        Base
    }

    public enum DungeonHeuristic
    {
        Manhattan,
        Euclidean,
        Weight
    }

    public enum DungeonMergeRooms
    {
        NoMerge,
        WidthHeight,
        Width,
        Height
    }

    enum DungeonConnectivity
    {
        SinglePath,
        FightForKey,
        SuperConnected
    }

    class Dungeon : Godot.Object
    {
        /// <summary>
        /// The tree representing the partitioned world
        /// </summary>
        public BspTree PartitionTree { get; private set; }
        
        /// <summary>
        /// Manages all the dungeon rooms
        /// </summary>
        public RoomManager Rooms { get; private set; }

        public DungeonParameters Parameters;

        int _maxRoomWeight = 20;

        /// <summary>
        /// The maximum weight a node in the navigation graph can have.
        /// </summary>
        public int MaxRoomWeight
        {
            get
            {
                return _maxRoomWeight;
            }
            set
            {
                if (value < 0) _maxRoomWeight = 0;
                else _maxRoomWeight = value;

            }
        }

        public bool GenerateRegions = true;

        public Random Rng;
        public DungeonConnectivity PathConnectiviy = DungeonConnectivity.SinglePath;

        public Dungeon(DungeonParameters parameters)
        {
            Parameters = parameters;
            SetSeed(parameters.Seed);
        }

        public void SetSeed(string seed = "")
        {
            if (seed == "")
            {
                string s = Guid.NewGuid().ToString("N");
                Parameters.Seed = s.Substring(0, 6);

            }
            else
            {
                Parameters.Seed = seed;
            }

            Rng = new Random(Parameters.Seed.GetHashCode());
        }

        public void GeneratePartitioning()
        {

            PartitionTree = new BspTree(Parameters.DungeonWidth, Parameters.DungeonHeight, this);


            int splits = Parameters.Splits;

            if(GenerateRegions)
            {
                PartitionTree.SplitIntoRegions();
                splits -= 2;

            }

            for(int i = 0; i < splits; i++)
            {
                PartitionTree.SplitRandom();
            }
                       
        }

        public void GenerateRooms()
        {
            Rooms = new RoomManager(this);

            Dictionary<DungeonRoomType, Partition> specials = GetPartitionsForSpecials();

            List<Partition> partitions = PartitionTree.GetAllLeafs();
            
            foreach(Partition partition in partitions)
            {
                Room r = new Room(this, partition, DungeonRoomType.Base);

                foreach (DungeonRoomType s in specials.Keys)
                {
                    Partition p = specials[s];
                    if (p == partition)
                        r.Type = s;
                }

                Rooms.AddRoom(r);
            }

        }
        private Dictionary<DungeonRoomType, Partition> GetPartitionsForSpecials()
        {

            if (!PartitionTree.IsRegional)
            {
                Console.WriteLine("DUNGEON::ERROR: Room allocation is only defined for regional trees.");
                return null;

            }

            Dictionary<string, Partition> regions = PartitionTree.GetRegions();

            Partition nw, ne, sw, se;
            regions.TryGetValue("NW", out nw);
            regions.TryGetValue("NE", out ne);
            regions.TryGetValue("SW", out sw);
            regions.TryGetValue("SE", out se);

            if(nw == null ||
               ne == null ||
               sw == null ||
               se == null)
            {
                Console.WriteLine("DUNGEON::ERROR: One of the regions is null.");
                return null;
            }

            // put regions into a List.
            // this will help to pick oposite regions
            // it will also function as a bag where we take 
            // the regions out
            List<Partition> regionsList = new List<Partition>();
            regionsList.Add(nw);
            regionsList.Add(ne);
            regionsList.Add(se);
            regionsList.Add(sw);
            // notice that now, index i + 2 are cardinally opposites

            // let's pick a regions for the boss spawn:
            int bossRegionIndex = Rng.Next(0, regionsList.Count);
            // it is to be kept in mind, that a region should be big enough
            // to hold the boss room

            // let's place the player in the opposite region:
            int playerRegionIndex = (bossRegionIndex + 2) % (regionsList.Count);

            Partition bossRegion = regionsList[bossRegionIndex];
            Partition playerRegion = regionsList[playerRegionIndex];

            regionsList.Remove(bossRegion);
            regionsList.Remove(playerRegion);

            // we have removed the player and boss regions from the bag.
            // now we randomize it and take a region for the key
            regionsList.Shuffle(Rng);

            // ok, now let'us place a key for the boss room. 
            // for now, we'll just place it in another region
            // other than the player and boss.
            Partition keyRegion = regionsList.First();

            // we remove it from the list as well
            regionsList.Remove(keyRegion);  // we still have a free region in the bag!!
            
            // ok.
            // now we need to pick a random partition from the player region
            // for the spawn room
            Partition playerPartition = GetRandomPartition(playerRegion);

            // same for boss.
            // but this one has a twist.
            // we will search or a partition big enough for the boss room
            // be able to spawn in.
            // if none is found, we will start searching in parent nodes
            // until one is found
            Partition bossPartition = PartitionTree.GetConfineNode(
                PartitionTree.GetLeftMostLeaf(bossRegion),
                new Boundary(0, 0, Parameters.MinRoomWidth * 2, Parameters.MinRoomHeight * 2));
            // we assume this size for the boss room
            // later we will have a list of bosses to pick from!
            if(bossPartition == null)
            {
                Console.WriteLine("ERROR::DUNGEON: No partition found for the boss room.");
            }
            // WE ARE NOT DONE YET:
            // We found a partition for the boss yes, but now we need to
            // trim the tree by that node, so it becomes a leaf.
            // easy as pie:
            PartitionTree.Trim(bossPartition);

            // same as player, get a random room for the key spawn
            Partition keyPartition = GetRandomPartition(keyRegion);

            Partition nestPartition = GetRandomPartition(regionsList.ElementAt(0));

            Dictionary<DungeonRoomType, Partition> specials = new Dictionary<DungeonRoomType, Partition>();
            specials.Add(DungeonRoomType.Boss, bossPartition);
            specials.Add(DungeonRoomType.PlayerSpawn, playerPartition);
            specials.Add(DungeonRoomType.Key, keyPartition);
            specials.Add(DungeonRoomType.Nest, nestPartition);

            return specials;

        }

        public Partition GetRandomPartition(Partition rootNode)
        {
            List<Partition> partitions = PartitionTree.GetAllLeafs(rootNode);

            if (partitions.Count <= 0)
                return null;

            partitions.Shuffle(Rng);

            return partitions.First();
        }

        /// <summary>
        /// Checks if a possible connection can be made between the boundaries, based on a percentage of edge sharing.
        /// </summary>
        public bool CheckAmountOfEdgeSharingBetweenPartitions(Boundary r1, Boundary r2, float minEdgeSharingPercentage)
        {

            // get the rooms overlaping dimensions
            int hoverlap = r1.CheckHorizontalOverlap(r2);
            int voverlap = r1.CheckVerticalOverlap(r2);

            // get the smallest side dimensions
            int addedWidth = Math.Min(r1.Width, r2.Width);
            int addedHeight = Math.Min(r1.Height, r2.Height);

            float minHorizontalEdgeSharing = addedWidth * minEdgeSharingPercentage;
            float minVerticalEdgeSharing = addedHeight * minEdgeSharingPercentage;

            return hoverlap >= minHorizontalEdgeSharing || voverlap >= minVerticalEdgeSharing;

        }

        /// <summary>
        /// Checks if a possible connection can be made between the boundaries, based on a dimension
        /// </summary>
        public bool CheckAmountOfEdgeSharingBetweenPartitions(Boundary r1, Boundary r2, int minEdgeSharingDimension)
        {

            // get the rooms overlaping dimensions
            int hoverlap = r1.CheckHorizontalOverlap(r2);
            int voverlap = r1.CheckVerticalOverlap(r2);

            return hoverlap >= minEdgeSharingDimension || voverlap >= minEdgeSharingDimension;

        }

        public void BuildGraph()
        {

            List<Room> rooms = Rooms.GetAllRooms();
            foreach(Room currentRoom in rooms)
            {
                foreach (Room room in rooms)
                {
                    if(currentRoom != room)
                    {
                        if(currentRoom.Partition.CheckEdgeSharing(room.Partition))
                        {
                            if(CheckAmountOfEdgeSharingBetweenPartitions(
                                currentRoom.Partition,
                                room.Partition,
                                Parameters.EdgeSharing))
                            {
                                Rooms.Connect(currentRoom, room);
                            }
                        }

                    }
                }

            }

            // now we need to obey the special rooms laws:
            Room playerSpawn = Rooms.GetSpecialRoom(DungeonRoomType.PlayerSpawn);
            Room key = Rooms.GetSpecialRoom(DungeonRoomType.Key);
            Room boss = Rooms.GetSpecialRoom(DungeonRoomType.Boss);
            Room nest = Rooms.GetSpecialRoom(DungeonRoomType.Nest);

            // a boss room can only have one single connection
            Rooms.RemoveEdge(boss, key);
            Rooms.RemoveEdge(boss, playerSpawn);
            List<Room> connections = Rooms.GetEdges(boss).ToList<Room>();
            connections.Shuffle(Rng);
            connections.RemoveAt(0);

            for(int i = 0; i < connections.Count; i++)
            {
                Rooms.RemoveEdge(boss, connections[i]);
            }

            // key room can only have one single connection
            Rooms.RemoveEdge(key, boss);
            Rooms.RemoveEdge(key, playerSpawn);
            connections = Rooms.GetEdges(key).ToList<Room>();
            connections.Shuffle(Rng);
            connections.RemoveAt(0);

            for (int i = 0; i < connections.Count; i++)
            {
                Rooms.RemoveEdge(key, connections[i]);
            }

        }

        public List<Room> GetPath(Room start, Room finish)
        {

            foreach (Room room in Rooms.GetAllRooms())
            {
                room.Reset();
            }

            List<Room> open   = new List<Room>();
            List<Room> closed = new List<Room>();

            start.Reset();

            start.costG = 0;
            start.fValue = start.costG + CalculateHeuristic(start, finish);

            Room current = start;

            while(current != finish)
            {
                List<Room> adjacents = Rooms.GetEdges(current);

                foreach (Room room in adjacents)
                {

                    if(!open.Contains(room) && !closed.Contains(room))
                    {
                        open.Add(room);
                    }

                    // the cost from current node (room is and adjacent) + the weight of this room
                    room.costG = current.costG + room.Weight;
                    float f = room.costG + CalculateHeuristic(room, finish);

                    if(room.fValue == 0 || f < room.fValue)
                    {
                        room.fValue = f;
                        room.Parent = current;
                    }

                }


                // fetch next room with smallest f value
                float smallest_f = float.MaxValue;
                int smallest_i = -1;
                for(int i = 0; i < open.Count; i++)
                {
                    if(open[i].fValue < smallest_f)
                    {
                        smallest_f = open[i].fValue;
                        smallest_i = i;
                    }
                }

                if(smallest_i < 0)
                {
                    Console.WriteLine("smallest_i is "  + smallest_i);
                    Console.WriteLine("dungeon parameters are:\n"  + Parameters.ToString());
                    Console.WriteLine(open.Count);
                }

                closed.Add(current);
                current = open[smallest_i];
                open.RemoveAt(smallest_i);

            }

            // build path
            List<Room> path = new List<Room>();
            GraphNode r = finish;
            while(r.Parent != null)
            {
                path.Add((Room)r);
                r = r.Parent;
            }

            path.Reverse();

            return path;

        }

        /// <summary>
        /// Makes the connection between all the special rooms.
        /// Calling this function will leave the room manager with only
        /// the needed rooms for the dungeon.
        /// </summary>
        public void ConnectDungeonRooms()
        {

            List<Room> dungeonRooms = new List<Room>();

            // connect all rooms:
            List<Room> specialRooms = new List<Room>();

            for(int i = 0; i < Rooms.Rooms.Count; i++)
            {
                if(Rooms.Rooms[i].Type != DungeonRoomType.Base)
                {
                    specialRooms.Add(Rooms.Rooms[i]);
                }
            }
            
            // now we obey the connectivity parameters:
            if (PathConnectiviy == DungeonConnectivity.SinglePath)
            {
                dungeonRooms = GetSinglePath(specialRooms);

            } else if(PathConnectiviy == DungeonConnectivity.FightForKey)
            {
                dungeonRooms = GetFightForKeyPath(specialRooms);
            }

            // we need to eliminate the duplicates now:
            dungeonRooms = dungeonRooms.Distinct().ToList();

            // remove all extra rooms:            
            int count = Rooms.Rooms.Count;
            for(int i = count; i > 0; i--)
            {
                if (!dungeonRooms.Contains(Rooms.Rooms[i-1]))
                    Rooms.RemoveNode(Rooms.Rooms[i-1]);
            }

        }

        private List<Room> GetSinglePath(List<Room> specialRooms)
        {

            if(!ListHasInitAndFinal(specialRooms))
            {
                Console.WriteLine("DUNGEON::PATH: The list doesn't contain a start and finish rooms.");
                return null;
            }

            List<Room> result = new List<Room>();

            // TODO: implement this single path using krukal algorithm maybe?

            // build a path that connects all special rooms:
            // lets first remove the init and boss
            specialRooms.Remove(Rooms.InitRoom);
            specialRooms.Remove(Rooms.BossRoom);

            // shuffle
            specialRooms.Shuffle(Rng);

            // add finish back to the list
            specialRooms.Insert(0, Rooms.InitRoom);
            specialRooms.Add(Rooms.BossRoom);
            // the list is now the order we will connect them

            // let's add the init already to the rooms in the dungeon
            result.Add(Rooms.InitRoom);

            // cycle through list and connect
            // we will be removing rooms from the list
            for (int i = specialRooms.Count; i >= 2; i--)
            {
                List<Room> path = GetPath(specialRooms.ElementAt(i - 1), specialRooms.ElementAt(i - 2));

                result.Add(specialRooms.ElementAt(i - 1));  // the path doesn't have the start room, so we add it too
                result.AddRange(path);

            }

            return result;
        }

        private List<Room> GetFightForKeyPath(List<Room> specialRooms)
        {
            if (!ListHasInitAndFinal(specialRooms) ||
                !specialRooms.Contains(Rooms.GetSpecialRoom(DungeonRoomType.Key)) ||
                !specialRooms.Contains(Rooms.GetSpecialRoom(DungeonRoomType.Nest)))
            {
                Console.WriteLine("DUNGEON::PATH: The list doesn't contain a start, finish, nest or key rooms.");
                return null;
            }

            List<Room> result = new List<Room>();

            Room key  = Rooms.GetSpecialRoom(DungeonRoomType.Key);
            Room nest = Rooms.GetSpecialRoom(DungeonRoomType.Nest);

            // build a path that first leads to the fight and only then,
            // the key.
            specialRooms.Remove(key);
            specialRooms.Shuffle(Rng);
            
            // cycle through list and connect
            // every room to every other
            for (int i = 0; i < specialRooms.Count; i++)
            {
                for (int j = 0; j < specialRooms.Count; j++)
                {

                    if (specialRooms[i] == specialRooms[j])
                    {
                        continue;
                    }

                    List<Room> p = GetPath(specialRooms[i], specialRooms[j]);

                    result.Add(specialRooms[i]);  // the path doesn't have the start room, so we add it too
                    result.AddRange(p);

                }

            }

            List<Room> path = GetPath(nest, key);

            result.Add(nest);  // the path doesn't have the start room, so we add it too
            result.AddRange(path);

            return result;
        }

        public void GeneratePassages()
        {
            // for all edges, generate a passage
            for(int i = 0; i < Rooms.Rooms.Count; i++)
            {
                Room current = Rooms.Rooms[i];

                List<Room> edges = Rooms.GetEdges(current);

                foreach(Room edge in edges)
                {

                    // let us think about if we want to make a door, a just a passage (opening between rooms)
                    // should we have a parameter to define the frequency of big openings? 
                    // maybe like Parameters.Openness (0.0f to 1.0f), where if 1, only generate openings.

                    int chance = (int)Utils.RandomBetween(Rng, 1, 100);
                    bool opening = chance < Parameters.Openness * 100;

                    if(opening)
                    {
                        bool horizontalNeighbors = current.Partition.Y == edge.Partition.Y && current.Partition.Height == edge.Partition.Height;
                        bool verticalNeighbors   = current.Partition.X == edge.Partition.X && current.Partition.Width  == edge.Partition.Width;

                        if(horizontalNeighbors)
                        {
                            // get the right edge of the left most neighbour
                            int openingX;

                            if(current.Partition.X1 < edge.Partition.X1)
                            {
                                // current is on the left side
                                // lets add x + width to get opening x
                                openingX = current.Partition.X2;
                            } else
                            {
                                // edge is on the left side
                                openingX = edge.Partition.X2;
                            }

                            int y1 = current.Partition.Y1 + 1;  // we remove/add 1, so it creates a little 'pillar' next to the walls
                            int y2 = current.Partition.Y2 - 1;

                            // TODO: check duplication of openings creation because there is no check if 
                            // an opening was already created due to all rooms vs edges being checked
                            Rooms.Openings.Add(new Opening(openingX, y1, openingX, y2, current, edge));

                        } else if(verticalNeighbors)
                        {
                            // get the bottom edge of the top most neighbour
                            int openingY;

                            if(current.Partition.Y1 < edge.Partition.Y1)
                            {
                                // current is on the top side
                                // lets add y + height to get opening y
                                openingY = current.Partition.Y2;
                            } else
                            {
                                // edge is on the top side
                                openingY = edge.Partition.Y2;
                            }

                            int x1 = current.Partition.X1 + 1;  // we remove/add 1, so it creates a little 'pillar' next to the walls
                            int x2 = current.Partition.X2 - 1;

                            // TODO: check duplication of openings creation because there is no check if 
                            // an opening was already created due to all rooms vs edges being checked
                            Rooms.Openings.Add(new Opening(x1, openingY, x2, openingY, current, edge));
                        }

                    } else
                    {

                        // get the middle of the overlaping edge
                        int verticalOverlap = current.Partition.CheckVerticalOverlap(edge.Partition);
                        int horizontalOverlap = current.Partition.CheckHorizontalOverlap(edge.Partition);

                        int x, y;
                        Door d;

                        if(verticalOverlap > 0)
                        {
                            // if it is a vertical overlap
                            // we need to know from each side it is

                            if(current.Partition.X1 < edge.Partition.X1)
                            {
                                // the current is on the right side

                                x = current.Partition.X2;

                            } else
                            {
                                x = current.Partition.X1;
                            }

                            y = Math.Max(current.Partition.Y1, edge.Partition.Y1) + verticalOverlap / 2;

                        } else if(horizontalOverlap > 0)
                        {
                            // an horizontal overlap means
                            // that we need to know whos
                            // on top

                            if(current.Partition.Y1 < edge.Partition.Y1)
                            {
                                // the current is on top

                                y = current.Partition.Y2;

                            } else
                            {
                                y = current.Partition.Y1;
                            }

                            x = Math.Max(current.Partition.X1, edge.Partition.X1) + horizontalOverlap / 2;
                        } else
                        {
                            // this should not happen.
                            // this will mean that the rooms are sharing 
                            // only a corner vertice.
                            // check dungeon parameters.
                            Console.WriteLine("DUNGEON::DOORS::ERROR: a vertice case was found!");
                            continue;
                        }

                        // TODO: check duplication of doors creation because there is no check if 
                        // a door was already created due to all rooms vs edges being checked
                        d = new Door(x, y, current, edge);

                        Rooms.Doors.Add(d);

                    }

                }

            }

        }

        public void MergeRooms()
        {
            List<Room> rooms = Rooms.GetAllRooms();
            List<Room[]> possibleMerges = new List<Room[]>(); 

            foreach(Room r in rooms)
            {
                
                if(r == null)
                    continue;

                // get neighbors
                List<Room> neighbors = Rooms.GetEdges(r);

                foreach(Room n in neighbors)
                {
                    if(n == null)
                        continue;

                    if(_checkPossibleMerge(r, n))
                    {
                        possibleMerges.Add(new Room[] {r, n});
                    }

                }

            }

            // remnove duplicate cases
            foreach(Room room in rooms)
            {
                int count = 0;

                for(int i = possibleMerges.Count - 1; i >= 0; i--)
                {

                    Room[] checkPair = possibleMerges[i];

                    // in how many pairs does a room appear?
                    if(room == checkPair[0] || room == checkPair[1])
                    {
                        count++;
                    }

                    if(count > 1)
                    {
                        possibleMerges.RemoveAt(i);
                        count--;
                    }

                }
            }

            foreach (Room[] pair in possibleMerges)
            {
                Room r = _mergeRooms(pair[0], pair[1]);
            }

        }

        private bool _checkPossibleMerge(Room a, Room b)
        {
            bool sucess = false;

            if(a == null || b == null)
                return sucess;

            // we skip special rooms, they have special needs
            if(a.Type != DungeonRoomType.Base || b.Type != DungeonRoomType.Base)
                return sucess;

            // do to the nature of our dungeon structure (binary space partition tree),
            // we can only perform the merge if the rooms are siblings in the BSP itself
            // hopefully, this will be the majority of the times.
            // the deviation parameter will "ensure" that only siblings share sides.
            if(a.Partition != b.Partition.GetSibling())
                return sucess;

            bool horizontalNeighbors = a.Partition.Y == b.Partition.Y && a.Partition.Height == b.Partition.Height;
            bool verticalNeighbors   = a.Partition.X == b.Partition.X && a.Partition.Width == b.Partition.Width;

            switch(Parameters.Merge)
            {                    
                case DungeonMergeRooms.WidthHeight:
                {
                    sucess = horizontalNeighbors || verticalNeighbors;
                    break;
                }
                case DungeonMergeRooms.Width:
                {
                    sucess = verticalNeighbors;
                    break;
                }
                case DungeonMergeRooms.Height:
                {
                    sucess = horizontalNeighbors;
                    break;
                }
                default:
                    sucess = false;
                    break;

            }

            return sucess;
        }

        private Room _mergeRooms(Room a, Room b)
        {
            // preserve the connections:
            List<Room> connections = Rooms.GetEdges(a).ToList<Room>();
            connections.AddRange(Rooms.GetEdges(b).ToList<Room>());
            connections.Remove(a);
            connections.Remove(b);
            
            // the newly, bigger, merged room
            Room newRoom = new Room(this, a.Partition.Parent, DungeonRoomType.Base);

            Rooms.RemoveNode(a);
            Rooms.RemoveNode(b);
            a.Partition.Parent.KillChildren();

            bool addSucess = Rooms.AddRoom(newRoom);
            
            foreach (Room edge in connections)
            {
                Rooms.Connect(newRoom, edge);
            }

            return newRoom;
        }

        private bool ListHasInitAndFinal(List<Room> specials)
        {
            return specials.Contains(Rooms.InitRoom) && specials.Contains(Rooms.BossRoom);
        }

        private float CalculateHeuristic(Room current, Room target)
        {
            float value = 0f;

            if (Parameters.Algorithm == DungeonHeuristic.Euclidean)
                value = CalculateEuclidianHeuristic(current, target);
            if (Parameters.Algorithm == DungeonHeuristic.Manhattan)
                value = CalculateManhattanHeuristic(current, target);
            if (Parameters.Algorithm == DungeonHeuristic.Weight)
                value = CalculateWeightHeuristic();

            return value;

        }

        private float CalculateEuclidianHeuristic(Room current, Room target)
        {
            float a = (float)Math.Pow((current.Partition.CenterX - target.Partition.CenterX), 2);
            float b = (float)Math.Pow((current.Partition.CenterY - target.Partition.CenterY), 2);

            return (float)Math.Sqrt(a + b);

        }

        private float CalculateManhattanHeuristic(Room current, Room target)
        {
            int x = Math.Max(target.Partition.CenterX, current.Partition.CenterX) - Math.Min(target.Partition.CenterX, current.Partition.CenterX);
            int y = Math.Max(target.Partition.CenterY, current.Partition.CenterY) - Math.Min(target.Partition.CenterY, current.Partition.CenterY);

            return x + y;

        }

        private float CalculateWeightHeuristic()
        {
            return MaxRoomWeight;
        }
    }
}
