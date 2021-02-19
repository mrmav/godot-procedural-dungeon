using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGenerator
{
    class Graph<T>
    {

        List<string> Guids;
        List<T> Nodes;
        Dictionary<string, List<T>> Edges;

        public Graph()
        {
            Guids = new List<string>();
            Nodes = new List<T>();
            Edges = new Dictionary<string, List<T>>();

        }

        public string GetNodeGuid(T node)
        {
            int index = Nodes.IndexOf(node);

            if(index < 0)
            {
                return "";
            }

            return Guids.ElementAt(index);
        }

        public bool AddNode(T node)
        {
            bool success = false;

            if(!Nodes.Contains(node))
            {
                Nodes.Add(node);
                Guids.Add(Guid.NewGuid().ToString());

                Edges.Add(GetNodeGuid(node), new List<T>());
                
                success = true;
            }

            return success;
        }

        public bool AddEdge(T node1, T node2)
        {
            bool success = false;

            if(node1.Equals(node2))
            {
                return success;
            }

            if(Nodes.Contains(node1) && Nodes.Contains(node2))
            {

                if (_IsConnected(node1, node2))
                {

                    return success;
                }
                else
                {

                    Edges[GetNodeGuid(node1)].Add(node2);
                    Edges[GetNodeGuid(node2)].Add(node1);

                    success = true;

                }
            }            

            return success;
        }

        public void RemoveNode(T node)
        {
            // first, disconnect
            List<T> edges = GetEdges(node).ToList();

            for (int i = 0; i < edges.Count; i++)
            {
                RemoveEdge(node, edges[i]);
            }

            string guid = GetNodeGuid(node);

            Nodes.Remove(node);
            Guids.Remove(guid);

        }

        public bool RemoveEdge(T node1, T node2)
        {
            return Edges[GetNodeGuid(node1)].Remove(node2) && Edges[GetNodeGuid(node2)].Remove(node1);
        }

        public List<T> GetNodes()
        {
            return Nodes;
        }

        public List<T> GetEdges(T node)    
        {
            return Edges[GetNodeGuid(node)];
        }

        public int CountNodes()
        {
            return Nodes.Count;
        }
        
        private bool _IsConnected(T n1, T n2)
        {
            return Edges[GetNodeGuid(n1)].Contains(n2);            
        }
        
        public void PrintGraph()
        {

            foreach(T node in Nodes)
            {
                List<T> connections = Edges[GetNodeGuid(node)];

                Console.Write($"{GetNodeGuid(node)}: ");

                foreach(T n in connections)
                {
                    Console.Write($" {GetNodeGuid(node)} ");
                }

                Console.Write("\n");

            }

        }
    }
}
