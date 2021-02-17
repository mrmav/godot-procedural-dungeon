using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGenerator
{
    class Partition : Boundary
    {

        public BspTree Tree { get; private set; }
        public Partition Parent { get; private set; }
        public Partition Left { get; private set; }
        public Partition Right { get; private set; }
        
        public Partition(BspTree tree, Partition parent, int x, int y, int w, int h)
            : base(x, y, w, h)
        {
            Tree = tree;

            Parent = parent;
            Left = null;
            Right = null;
        }

        public bool IsLeaf()
        {
            return Left == null && Right == null;
        }

        public bool IsRoot()
        {
            return Parent == null;
        }

        public bool SplitHorizontally(float deviation)
        {
            Boundary[] r = SplitBoundaryHorizontally(deviation);
            bool success = TestSplitResults(r); // we ned to test for room min sizes
            
            if(success)
            {
                CreateChildren(r);
            }

            return success;

        }

        public bool SplitVertically(float deviation)
        {
            Boundary[] r = SplitBoundaryVertically(deviation);
            bool success = TestSplitResults(r); // we ned to test for room min sizes

            if (success)
            {
                CreateChildren(r);
            }

            return success;
        }

        private void CreateChildren(Boundary[] rects)
        {
            Boundary b1 = rects[0];
            Boundary b2 = rects[1];

            Left = new Partition(Tree, this, b1.X, b1.Y, b1.Width, b1.Height);
            Right = new Partition(Tree, this, b2.X, b2.Y, b2.Width, b2.Height);
        }

        public bool TestSplitResults(Boundary[] rects)
        {
            // test if results are valid
            Boundary minRoomBounds = new Boundary(0, 0, Tree.Dungeon.Parameters.MinRoomWidth, Tree.Dungeon.Parameters.MinRoomHeight);

            for(int i = 0; i < rects.Length; i++)
            {
                if (!rects[i].CanContain(minRoomBounds))
                {
                    return false;
                }

            }

            return true;
        }

        public Partition GetSibling()
        {
            Partition result = null;

            if(Parent != null)
            {

                if (this == Parent.Left)
                {
                    result = Parent.Right;
                } else
                {
                    result = Parent.Left;
                }

            }

            return result;
        }

        public void KillChildren()
        {
            Left = null;
            Right = null;
        }

    }
}
