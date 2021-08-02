using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Cape : SpringSystem
{
    private Polygon2D _polygon;
    private MeshInstance2D _meshInstance;
    private ArrayMesh _mesh;
    private MeshDataTool _mdt;    

    private Vector2 _center = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        Array children = GetChildren();
        for(int i = 0; i < children.Count; i++)
        {
            if(children[i] is Polygon2D)
            {
                _polygon = (Polygon2D)children[i];
            }    
        }

        _mdt = new MeshDataTool();

        _meshInstance = GetNode<MeshInstance2D>("CapeMeshInstance");
        _mesh = new ArrayMesh();
        Array arr = new Array();
        arr.Resize((int)Mesh.ArrayType.Max);

        Vector3[] vertices = new Vector3[_springs.Count];        
        Color[] colors = new Color[_springs.Count];        

        for(int i = 0; i < _springs.Count; i++)
        {
            Vector2 s = _springs[i].Position;
            vertices[i] = new Vector3(s.x, s.y, 0f);

            colors[i] = Colors.DarkRed;
        }

        int[] indices = new int[24];

        // create indices
        int currentIndice = 0;
        int currentSubDivison = 0;
        int ZSubs = 2;
        int XSubs = 2;
        int nVerticesWidth = 3;
        for (int z = 0; z < ZSubs; z++)
        {
            for (int x = 0; x < XSubs; x++)
            {
                /* calculate positions in the array
                * 
                *  1---2
                *  | / |
                *  4---3
                *  
                */

                int vert1 = currentSubDivison + z;
                int vert2 = vert1 + 1;
                int vert3 = vert2 + nVerticesWidth;
                int vert4 = vert3 - 1;

                // first tri
                indices[currentIndice++] = (short)vert1;
                indices[currentIndice++] = (short)vert2;
                indices[currentIndice++] = (short)vert4;

                // secon tri
                indices[currentIndice++] = (short)vert4;
                indices[currentIndice++] = (short)vert2;
                indices[currentIndice++] = (short)vert3;

                currentSubDivison++;

            }

        }

        arr[(int)Mesh.ArrayType.Vertex] = vertices;
        arr[(int)Mesh.ArrayType.Color] = colors;
        arr[(int)Mesh.ArrayType.Index]  = indices;
        
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);
        _meshInstance.Mesh = _mesh;

        //UpdatePoints();

    }

    public void UpdatePoints()
    {
        List<Vector2> pts = new List<Vector2>();
        for(int i = 0; i < _springs.Count - 1; i++)  // we do not want the last two spring pointsfor a closed polygon
        {
            pts.Add(_springs[i].Position);
        }

        // sort the points in clockwise order
        _center = CalculateCenterPoint(pts);
        pts.Sort(ComparePoints);

        _polygon.Polygon = pts.ToArray();
    }

    public void UpdateMesh()
    {
        _mdt.CreateFromSurface(_mesh, 0);

        int count = _mdt.GetVertexCount();
        for(int i = 0; i < count; i++)
        {            
            _mdt.SetVertex(i, new Vector3(_springs[i].Position.x, _springs[i].Position.y, 0f));
        }

        _mesh.SurfaceRemove(0);
        _mdt.CommitToSurface(_mesh);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        //UpdatePoints();
        UpdateMesh();
        Update();
    }
    
    // see: https://stackoverflow.com/questions/6989100/sort-points-in-clockwise-order
    private int ComparePoints(Vector2 a, Vector2 b)
    {

        if (a.x - _center.x >= 0 && b.x - _center.x < 0)
            return -1;
        if (a.x - _center.x < 0 && b.x - _center.x >= 0)
            return 1;
        if (a.x - _center.x == 0 && b.x - _center.x == 0) {
            if (a.y - _center.y >= 0 || b.y - _center.y >= 0)
                return Bool2Int(a.y > b.y);
            return Bool2Int(b.y > a.y);
        }

        // compute the cross product of vectors (center -> a) x (center -> b)
        float det = (a.x - _center.x) * (b.y - _center.y) - (b.x - _center.x) * (a.y - _center.y);
        if (det < 0)
            return -1;
        if (det > 0)
            return 1;

        // points a and b are on the same line from the center
        // check which point is closer to the center
        float d1 = (a.x - _center.x) * (a.x - _center.x) + (a.y - _center.y) * (a.y - _center.y);
        float d2 = (b.x - _center.x) * (b.x - _center.x) + (b.y - _center.y) * (b.y - _center.y);
        return Bool2Int(d1 > d2);

    }

    private Vector2 CalculateCenterPoint(List<Vector2> pts)
    {
        Vector2 c = Vector2.Zero;

        for(int i = 0; i < pts.Count; i++)
        {
            c += pts[i];
        }

        return c / pts.Count;

    }

    public override void _Draw()
    {

        DrawCircle(_center, 1f, Colors.White);

        base._Draw();
    }

    private int Bool2Int(bool b)
    {
        if(b)
            return 1;
        
        return -1;
    }
}
