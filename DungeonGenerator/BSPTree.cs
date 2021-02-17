using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGenerator
{
    
    /// <summary>
    /// 2D Binary Space Partitioning Tree
    /// </summary>
    /// <remarks>
    /// Please take into account that in order to get a tree with regions
    /// like NW, NE, SW and SE, you should always split first vertically and then
    /// horizontally. Or the other way around.
    /// If you want these cardinal regions, I would suggest to first split vertically
    /// and then horizontally. This way, you'll get the following regions:
    ///           root
    ///          /   \
    ///         a     b
    ///        / \   / \
    ///       c   d e   f
    /// c = NW
    /// d = NE
    /// e = SE
    /// f = SW
    /// </remarks>
    class BspTree
    {
        public Dungeon Dungeon { get; }

        Partition Root;

        Random rng;
        
        public bool IsRegional { get; private set; }
        
        /// <summary>
        /// Creates the partitioning tree.
        /// </summary>
        /// <param name="w">Width of the space to partition</param>
        /// <param name="h">Height of the space to partition</param>
        public BspTree(int w, int h, Dungeon dungeon)
        {
            Dungeon = dungeon;
            
            Root = new Partition(this, null, 0, 0, w, h);

            // create a rng based on a seed for debug
            rng = dungeon.Rng;
            
            IsRegional = false;
        }


        /// <summary>
        /// Splits every leaf node randomly.
        /// </summary>
        public void SplitRandom()
        {
            _splitRandom(Root);
        }

        private float GetDeviationForCut()
        {

            float maxDeviation = Dungeon.Parameters.SplitDeviation / 2.0f;
            float minDeviation = -Dungeon.Parameters.SplitDeviation / 2.0f;

            // this is the percentage that can be deviated from the center
            return (float)(rng.NextDouble() * (maxDeviation - minDeviation) + minDeviation);
        }

        private void _splitRandom(Partition node)
        {

            if (node.IsLeaf())
            {
                // this is the percentage that can be deviated from the center
                float deviation = GetDeviationForCut();

                // the ratio
                //float maxRatioAllowed = Dungeon.MaxRatioAllowed;

                //// we will start by making the split of the boundary
                //// this way we can test the ratio
                //// with this information, we will decide if the split
                //// is horizontal or vertical.
                //// if the vertical split yelds bigger ratios than allowed
                //// we cut horizontally and vice versa
                //Boundary[] sampleh = node.SplitBoundaryHorizontally(deviation);
                //Boundary[] samplev = node.SplitBoundaryVertically(deviation);

                //// test partitions first:
                //if (!node.TestSplitResults(sampleh) || !node.TestSplitResults(samplev))
                //    return;

                //// this calculates the new partitions ratios
                //float ratioh0 = sampleh[0].Width / sampleh[0].Height;
                //float ratioh1 = sampleh[1].Width / sampleh[1].Height;
                //float ratiov0 = samplev[0].Width / samplev[0].Height;
                //float ratiov1 = samplev[1].Width / samplev[1].Height;

                //// now we have the new partitions ratios
                //// let's test which is the biggest
                ////bool hsplit = Math.Max(ratioh0, ratioh1) < Math.Max(ratiov0, ratiov1);
                //float maxRatioH = Math.Max(ratioh0, ratioh1);
                //float maxRatioV = Math.Max(ratiov0, ratiov1);

                //bool hsplit = maxRatioV < maxRatioAllowed;

                bool hsplit = node.Width >= node.Height;
                bool splitSucess = false;

                //hsplit = rng.NextDouble() > 0.49f;

                if (hsplit)
                {
                    splitSucess = node.SplitHorizontally(deviation);
                    if(!splitSucess)
                    {
                        node.SplitVertically(deviation);
                    }
                }
                else
                {
                    splitSucess = node.SplitVertically(deviation);
                    if(!splitSucess)
                    {
                        node.SplitHorizontally(deviation);
                    }
                }

            } else
            {
                // go recursive
                _splitRandom(node.Left);
                _splitRandom(node.Right);
            }
            
        }

        /// <summary>
        /// Splits every leaf node with a horizontal line
        /// ####
        /// #  #
        /// ----
        /// #  #
        /// ####
        /// </summary>
        public void SplitVertically()
        {
            _splitVertically(Root);
        }

        private void _splitVertically(Partition node)
        {

            if (node.IsLeaf())
            {
                
                // this is the percentage that can be deviated from the center
                float deviation = GetDeviationForCut();

                node.SplitVertically(deviation);

            }
            else
            {
                // go recursive
                _splitVertically(node.Left);
                _splitVertically(node.Right);
            }

        }

        /// <summary>
        /// Splits every leaf node with a vertical line
        /// ##|##
        /// # | #
        /// ##|##
        /// </summary>
        public void SplitHorizontally()
        {
            _splitHorizontally(Root);
        }

        private void _splitHorizontally(Partition node)
        {

            if (node.IsLeaf())
            {

                // this is the percentage that can be deviated from the center
                float deviation = GetDeviationForCut();

                node.SplitHorizontally(deviation);

            }
            else
            {
                // go recursive
                _splitHorizontally(node.Left);
                _splitHorizontally(node.Right);
            }

        }


        /// <summary>
        /// Based on a leaf node, this function will find a node that can contain 
        /// the given rectangle.
        /// </summary>
        /// <param name="leaf">The leaf where to start the search</param>
        /// <param name="rect">The rectangle to contain</param>
        /// <returns>The node that can contain the rectangle. Null if none found.</returns>
        public Partition GetConfineNode(Partition leaf, Boundary rect)
        {
            Partition result = null;

            // we will figure if the leaf or the sibling can hold the rect
            if(leaf.CanContain(rect))
            {
                result = leaf;

            } else if(leaf.GetSibling().CanContain(rect))
            {
                result = leaf.GetSibling();
            } else
            {
                // ok, now we need to work up the tree until we find one. 
                result = GetConfineNode(leaf.Parent, rect);
            }

            return result;

        }

        /// <summary>
        /// Gets the left most node
        /// </summary>
        /// <returns></returns>
        public Partition GetLeftMostLeaf()
        {
            return _getLeftMostLeaf(Root);
        }

        /// <summary>
        /// Gets the left most leaf from this node dowm
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Partition GetLeftMostLeaf(Partition node)
        {
            return _getLeftMostLeaf(node);
        }

        private Partition _getLeftMostLeaf(Partition node)
        {
            Partition leftmost = null;

            if(node.Left == null)
            {
                leftmost = node;

            } else
            {
                leftmost = _getLeftMostLeaf(node.Left);
            }

            return leftmost;

        }

        public Partition GetRightMostLeaf()
        {
            return _getRightMostLeaf(Root);
        }

        private Partition _getRightMostLeaf(Partition node)
        {
            Partition rightmost = null;

            if (node.Right == null)
            {
                rightmost = node;

            }
            else
            {
                rightmost = _getRightMostLeaf(node.Right);
            }

            return rightmost;

        }

        /// <summary>
        /// Given a node, it will trim the tree from this node down.
        /// This deletes all the childs from this node down.
        /// As a reminder, this will unbalance the tree.
        /// </summary>
        /// <param name="node">The trim start. This node will not be deleted</param>
        public void Trim(Partition node)
        {
            // note: this will trigger the GC.
            //       I think.
            //       This is why I like C.
            //       I feel a bit lost with the memory management here.
            //       *sigh* guess i'll have to read more documentation.

            // kill the relationship with children:
            node.KillChildren();      
        }

        /// <summary>
        /// Gets all nodes that are leafs
        /// </summary>
        /// <returns></returns>
        public List<Partition> GetAllLeafs()
        {
            return _getAllLeafs(Root);
        }

        /// <summary>
        /// Gets all leaf nodes based starting on teh provided one
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Partition> GetAllLeafs(Partition node)
        {
            return _getAllLeafs(node);
        }

        private List<Partition> _getAllLeafs(Partition node)
        {
            List<Partition> list = new List<Partition>();

            if(node.IsLeaf())
            {
                list.Add(node);
            } else
            {
                list.AddRange(_getAllLeafs(node.Left));
                list.AddRange(_getAllLeafs(node.Right));
            }

            return list;

        }

        /// <summary>
        /// Makes the first two splits with deviation 0.
        /// This will make the tree separated into regions.
        /// </summary>
        public void SplitIntoRegions()
        {
            // clear the current tree
            Root.KillChildren();

            //float oldDeviation = MaxDeviation;
            //MaxDeviation = 0.0f;

            SplitVertically();
            SplitHorizontally();

            //MaxDeviation = oldDeviation;

            IsRegional = true;

        }

        /// <summary>
        /// Clears the complete tree
        /// </summary>
        public void ClearTree()
        {
            Trim(Root);
        }

        /// <summary>
        /// Gets the tree regions
        /// </summary>
        /// <returns>A dictionary with keys NW, NE, SW and SE</returns>
        public Dictionary<string, Partition> GetRegions()
        {
            if(IsRegional)
            {
                Dictionary<string, Partition> regionalNodes = new Dictionary<string, Partition>();

                regionalNodes.Add("NW", Root.Left.Left);
                regionalNodes.Add("NE", Root.Left.Right);
                regionalNodes.Add("SW", Root.Right.Left);
                regionalNodes.Add("SE", Root.Right.Right);

                return regionalNodes;
            }

            return null;
        }

    }
}
