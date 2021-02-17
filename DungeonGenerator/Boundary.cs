using System;

/// <summary>
/// Represents a geometric rectangle.
/// Units are integers.
/// </summary>
public class Boundary
{

    public int X;
    public int Y;
    public int Width;
    public int Height;

    public int X1
    {
        get
        {
            return X;
        }
    }
    public int Y1
    {
        get
        {
            return Y;
        }
    }

    public int X2
    {
        get
        {
            return X + Width;
        }
    }
    public int Y2
    {
        get
        {
            return Y + Height;
        }
    }
    public int CenterX
    {
        get
        {
            return X + Width / 2;
        }
    }
    public int CenterY
    {
        get
        {
            return Y + Height / 2;
        }
    }


    public Boundary(int x, int y, int w, int h)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
    }

    public Boundary[] SplitBoundaryHorizontally(float deviation)
    {
        Boundary[] result = new Boundary[2];

        int offsetFromCenter = (int)Math.Floor(Width * deviation);
        int halfWidth = (int)Math.Floor(Width / 2.0f);
        int remain = Width % 2;

        // left rectangle
        result[0] = new Boundary(X, Y, halfWidth - offsetFromCenter + remain, Height);

        // right rectangle
        result[1] = new Boundary(X + (halfWidth - offsetFromCenter + remain), Y, halfWidth + offsetFromCenter, Height);

        return result;

    }

    public Boundary[] SplitBoundaryVertically(float deviation)
    {
        Boundary[] result = new Boundary[2];

        int offsetFromCenter = (int)Math.Floor(Height * deviation);
        int halfHeight = (int)Math.Floor(Height / 2.0f);
        int remain = Height % 2;

        // left rectangle
        result[0] = new Boundary(X, Y, Width, halfHeight - offsetFromCenter + remain);

        // right rectangle
        result[1] = new Boundary(X, Y + (halfHeight - offsetFromCenter + remain), Width, halfHeight + offsetFromCenter);

        return result;

    }

    /// <summary>
    /// Returns wether this rectangle can contain another
    /// </summary>
    /// <param name="rect">The rectanlge to contain</param>
    /// <returns></returns>
    public bool CanContain(Boundary rect)
    {
        return Width >= rect.Width && Height >= rect.Height;
    }

    public bool CheckEdgeSharing(Boundary r /*, float sharingPercentage*/)
    {

        // Check if rectangle a touches rectangle b
        // Each object (a and b) should have 2 properties to represent the
        // top-left corner (x1, y1) and 2 for the bottom-right corner (x2, y2).

        // has horizontal gap
        if (this.X1 > r.X2 || r.X1 > this.X2) return false;

        // has vertical gap
        if (this.Y1 > r.Y2 || r.Y1 > this.Y2) return false;

        return true;

    }

    public int CheckHorizontalOverlap(Boundary r /*, float sharingPercentage*/)
    {
        
        // measure the horizontal gap amount

        return Math.Max(0, Math.Min(this.X2, r.X2) - Math.Max(this.X1, r.X1));

    }

    public int CheckVerticalOverlap(Boundary r /*, float sharingPercentage*/)
    {

        // measure the vertical gap amount
        // elegant code from https://math.stackexchange.com/questions/99565/simplest-way-to-calculate-the-intersect-area-of-two-rectangles

        return Math.Max(0, Math.Min(this.Y2, r.Y2) - Math.Max(this.Y1, r.Y1));

    }

    public override string ToString()
    {
        return $"Rect: {{X: {X}, Y:{Y}, W:{Width}, H:{Height}}}";
    }

}