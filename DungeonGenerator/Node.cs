namespace DungeonGenerator
{
    class GraphNode
    {
        public int Weight;
        public float fValue;
        public float costG;
        public GraphNode Parent;
        
        public GraphNode(int weight = 1)
        {
            Weight = weight;
        }

        public void Reset()
        {
            fValue = 0;
            costG = 0;
            Parent = null;
        }
    }
}
